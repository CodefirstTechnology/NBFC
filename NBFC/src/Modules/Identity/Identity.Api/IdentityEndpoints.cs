using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Patsanstha.BuildingBlocks.Infrastructure.Api;
using Patsanstha.Modules.Identity.Application.Auth.Login;
using Patsanstha.Modules.Identity.Application.Auth.Logout;
using Patsanstha.Modules.Identity.Application.Auth.Refresh;
using Patsanstha.Modules.Identity.Application.Auth.TwoFactor;
using Patsanstha.Modules.Identity.Application.Roles.GetRoles;
using Patsanstha.Modules.Identity.Application.Users.AssignRoles;
using Patsanstha.Modules.Identity.Application.Users.CreateUser;
using Patsanstha.Modules.Identity.Application.Users.ListUsers;
using Patsanstha.Modules.Identity.Application.Users.UpdateUser;
using Patsanstha.Modules.Identity.Application.Users.GetCurrentUser;
using Patsanstha.Modules.Identity.Domain.Authorization;

namespace Patsanstha.Modules.Identity.Api;

public static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/v1/auth").WithTags("Identity");

        auth.MapPost("/login", async (
            LoginRequest request,
            HttpContext httpContext,
            ISender sender) =>
        {
            var command = new LoginCommand(
                request.Email,
                request.Password,
                httpContext.Connection.RemoteIpAddress?.ToString());

            var result = await sender.Send(command);
            return result.ToHttpResult();
        })
        .AllowAnonymous()
        .WithName("AuthLogin");

        auth.MapPost("/refresh", async (
            RefreshRequest request,
            HttpContext httpContext,
            ISender sender) =>
        {
            var command = new RefreshTokenCommand(
                request.RefreshToken,
                httpContext.Connection.RemoteIpAddress?.ToString());

            var result = await sender.Send(command);
            return result.ToHttpResult();
        })
        .AllowAnonymous()
        .WithName("AuthRefresh");

        auth.MapPost("/logout", async (
            LogoutRequest request,
            ISender sender) =>
        {
            var result = await sender.Send(new LogoutCommand(request.RefreshToken));
            return result.ToHttpResult();
        })
        .RequireAuthorization()
        .WithName("AuthLogout");

        auth.MapPost("/2fa/verify", async (
            VerifyTwoFactorLoginRequest request,
            HttpContext httpContext,
            ISender sender) =>
        {
            var command = new VerifyTwoFactorLoginCommand(
                request.ChallengeToken,
                request.VerificationCode,
                httpContext.Connection.RemoteIpAddress?.ToString());

            var result = await sender.Send(command);
            return result.ToHttpResult();
        })
        .AllowAnonymous()
        .WithName("AuthVerifyTwoFactorLogin");

        auth.MapPost("/2fa/setup", async (ClaimsPrincipal user, ISender sender) =>
        {
            if (!TryGetUserId(user, out var userId))
            {
                return Results.Unauthorized();
            }

            var result = await sender.Send(new SetupTwoFactorCommand(userId));
            return result.ToHttpResult();
        })
        .RequireAuthorization()
        .WithName("AuthSetupTwoFactor");

        auth.MapPost("/2fa/confirm", async (
            ConfirmTwoFactorRequest request,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            if (!TryGetUserId(user, out var userId))
            {
                return Results.Unauthorized();
            }

            var result = await sender.Send(new ConfirmTwoFactorCommand(userId, request.VerificationCode));
            return result.ToHttpResult();
        })
        .RequireAuthorization()
        .WithName("AuthConfirmTwoFactor");

        auth.MapPost("/2fa/disable", async (
            DisableTwoFactorRequest request,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            if (!TryGetUserId(user, out var userId))
            {
                return Results.Unauthorized();
            }

            var result = await sender.Send(new DisableTwoFactorCommand(userId, request.VerificationCode));
            return result.ToHttpResult();
        })
        .RequireAuthorization()
        .WithName("AuthDisableTwoFactor");

        auth.MapGet("/me", async (ClaimsPrincipal user, ISender sender) =>
        {
            if (!TryGetUserId(user, out var userId))
            {
                return Results.Unauthorized();
            }

            var result = await sender.Send(new GetCurrentUserQuery(userId));
            return result.ToHttpResult();
        })
        .RequireAuthorization()
        .WithName("AuthMe");

        var admin = app.MapGroup("/api/v1/admin").WithTags("Identity Admin");

        admin.MapGet("/roles", async (ISender sender) =>
        {
            var result = await sender.Send(new GetRolesQuery());
            return result.ToHttpResult();
        })
        .RequireAuthorization(Permissions.AdminRolesManage)
        .WithName("AdminGetRoles");

        admin.MapGet("/permissions", async (ISender sender) =>
        {
            var result = await sender.Send(new GetPermissionsQuery());
            return result.ToHttpResult();
        })
        .RequireAuthorization(Permissions.AdminRolesManage)
        .WithName("AdminGetPermissions");

        admin.MapGet("/users", async (
            ISender sender,
            int page = 1,
            int pageSize = 20,
            string? search = null,
            bool? isActive = null) =>
        {
            var result = await sender.Send(new ListUsersQuery(page, pageSize, search, isActive));
            return result.ToHttpResult();
        })
        .RequireAuthorization(Permissions.AdminUsersManage)
        .WithName("AdminListUsers");

        admin.MapPost("/users", async (
            CreateUserRequestBody request,
            ISender sender) =>
        {
            var command = new CreateUserCommand(
                request.Email,
                request.Password,
                request.FullName,
                request.BranchId,
                request.Roles);

            var result = await sender.Send(command);
            return result.IsSuccess
                ? result.ToCreatedResult($"/api/v1/admin/users/{result.Value.Id}")
                : result.Error.ToProblemResult();
        })
        .RequireAuthorization(Permissions.AdminUsersManage)
        .WithName("AdminCreateUser");

        admin.MapPut("/users/{userId:guid}", async (
            Guid userId,
            UpdateUserRequestBody request,
            ISender sender) =>
        {
            var result = await sender.Send(new UpdateUserCommand(
                userId,
                request.FullName,
                request.BranchId,
                request.IsActive));

            return result.ToHttpResult();
        })
        .RequireAuthorization(Permissions.AdminUsersManage)
        .WithName("AdminUpdateUser");

        admin.MapPut("/users/{userId:guid}/roles", async (
            Guid userId,
            AssignRolesRequestBody request,
            ISender sender) =>
        {
            var result = await sender.Send(new AssignUserRolesCommand(userId, request.Roles));
            return result.ToHttpResult();
        })
        .RequireAuthorization(Permissions.AdminUsersManage)
        .WithName("AdminAssignUserRoles");

        return app;
    }

    private static bool TryGetUserId(ClaimsPrincipal user, out Guid userId)
    {
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub");

        return Guid.TryParse(userIdClaim, out userId);
    }

    private sealed record LoginRequest(string Email, string Password);

    private sealed record RefreshRequest(string RefreshToken);

    private sealed record LogoutRequest(string RefreshToken);

    private sealed record VerifyTwoFactorLoginRequest(string ChallengeToken, string VerificationCode);

    private sealed record ConfirmTwoFactorRequest(string VerificationCode);

    private sealed record DisableTwoFactorRequest(string VerificationCode);

    private sealed record CreateUserRequestBody(
        string Email,
        string Password,
        string FullName,
        Guid? BranchId,
        IReadOnlyList<string> Roles);

    private sealed record UpdateUserRequestBody(
        string? FullName,
        Guid? BranchId,
        bool? IsActive);

    private sealed record AssignRolesRequestBody(IReadOnlyList<string> Roles);
}
