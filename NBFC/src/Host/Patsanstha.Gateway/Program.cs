var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("X-Correlation-Id"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors("FrontendDev");
}

app.MapGet("/health/live", () => Results.Ok(new { status = "live", component = "gateway" }))
    .WithTags("Health")
    .WithName("GatewayHealthLive");

app.MapReverseProxy();

app.Run();
