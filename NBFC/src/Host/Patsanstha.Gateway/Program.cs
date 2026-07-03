var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:4200"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("X-Correlation-Id"));
});

var app = builder.Build();

app.UseCors("Frontend");

app.MapGet("/health/live", () => Results.Ok(new { status = "live", component = "gateway" }))
    .WithTags("Health")
    .WithName("GatewayHealthLive");

app.MapReverseProxy();

app.Run();
