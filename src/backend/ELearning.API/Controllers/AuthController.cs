using ELearning.API.Extensions;
using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Auth.Commands;
using ELearning.Application.Features.Auth.DTOs.AuthResponse;
using ELearning.Application.Features.Auth.DTOs.User;
using ELearning.Application.Features.Auth.Queries.GetCurrentUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
     ICommandHandler<RegisterUserCommand, Guid> registerHandler,
    ICommandHandler<LoginCommand, LoginResponseDto> loginHandler,
    ICommandHandler<VerifyEmailCommand> verifyEmailHandler,
    ICommandHandler<ForgotPasswordCommand> requestResetHandler,
    ICommandHandler<ResetPasswordCommand> resetPasswordHandler,
    IQueryHandler<GetCurrentUserQuery, UserDto> getCurrentUserHandler
) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand cmd)
    {
        var result = await registerHandler.HandleAsync(cmd, HttpContext.RequestAborted);
        return this.ToCreatedResult(result, nameof(Register), new { });
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand cmd)
    { 
        var result = await loginHandler.HandleAsync(cmd, HttpContext.RequestAborted);
        return this.ToActionResult(result);
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailCommand cmd)
    {
        var result = await verifyEmailHandler.HandleAsync(cmd, HttpContext.RequestAborted);
        return this.ToActionResult(result);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand cmd)
    {
        var result = await requestResetHandler.HandleAsync(cmd, HttpContext.RequestAborted);
        return this.ToActionResult(result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand cmd)
    {
        var result = await resetPasswordHandler.HandleAsync(cmd, HttpContext.RequestAborted);
        return this.ToActionResult(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.GetUserId(); // ClaimsPrincipalExtensions que definimos antes
        var result = await getCurrentUserHandler.HandleAsync(
            new GetCurrentUserQuery(userId),
            HttpContext.RequestAborted);
        return this.ToActionResult(result);
    }
}