using System.Security.Claims;
using KitchenRecipes.Application.Abstractions;
using KitchenRecipes.Application.DTOs;
using KitchenRecipes.Domain.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KitchenRecipes.Web.Controllers.Api;

[ApiController]
[Route("api/account")]
public sealed class AccountController : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserRequest request,
        [FromServices] IAccountService accountService,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await accountService.RegisterAsync(request, cancellationToken);
            await SignInAsync(user);
            return Ok(user);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        [FromServices] IAccountService accountService,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await accountService.ValidateCredentialsAsync(request, cancellationToken);
            if (user is null)
            {
                return Unauthorized(new { message = "Invalid credentials." });
            }

            await SignInAsync(user);
            return Ok(user);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(
        [FromServices] IAccountService accountService,
        CancellationToken cancellationToken)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var validId = int.TryParse(idClaim, out var userId);
        if (!validId)
        {
            return Unauthorized();
        }

        var user = await accountService.GetByIdAsync(userId, cancellationToken);
        return user is null ? Unauthorized() : Ok(user);
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromServices] IAccountService accountService,
        CancellationToken cancellationToken)
    {
        var users = await accountService.GetUsersAsync(cancellationToken);
        return Ok(users);
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpPut("users/{id:int}/role")]
    public async Task<IActionResult> UpdateRole(
        int id,
        [FromBody] UpdateUserRoleRequest request,
        [FromServices] IAccountService accountService,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await accountService.UpdateRoleAsync(id, request, cancellationToken);
            return updated ? NoContent() : NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    private async Task SignInAsync(AuthUserDto user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }
}
