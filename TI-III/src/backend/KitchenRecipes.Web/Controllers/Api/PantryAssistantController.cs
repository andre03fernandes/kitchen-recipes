using KitchenRecipes.Application.Abstractions;
using KitchenRecipes.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace KitchenRecipes.Web.Controllers.Api;

[ApiController]
[Route("api/assistant")]
[Route("api/pantry-assistant")]
[Authorize]
public sealed class PantryAssistantController : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [HttpGet("profile")]
    public IActionResult GetAiProfile([FromServices] IAiProfilePreferenceService profilePreferenceService)
    {
        var settings = profilePreferenceService.GetCurrent();
        return Ok(settings);
    }

    [HttpPut("profile")]
    public IActionResult UpdateAiProfile(
        [FromBody] UpdateAiProfileRequestDto request,
        [FromServices] IAiProfilePreferenceService profilePreferenceService)
    {
        if (string.IsNullOrWhiteSpace(request.Profile))
        {
            return BadRequest(new { message = "Profile is required." });
        }

        var settings = profilePreferenceService.UpdateProfile(request.Profile);
        return Ok(settings);
    }

    [HttpGet("threads")]
    public async Task<IActionResult> GetThreads(
        [FromServices] IAssistantConversationService conversationService,
        CancellationToken cancellationToken)
    {
        var userId = TryGetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await conversationService.GetThreadsAsync(userId.Value, cancellationToken);
        return Ok(result);
    }

    [HttpGet("threads/{threadId:int}")]
    public async Task<IActionResult> GetThread(
        int threadId,
        [FromServices] IAssistantConversationService conversationService,
        CancellationToken cancellationToken)
    {
        var userId = TryGetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await conversationService.GetThreadAsync(userId.Value, threadId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("threads/message")]
    public async Task<IActionResult> SendMessage(
        [FromBody] SendAssistantChatMessageRequestDto request,
        [FromServices] IAssistantConversationService conversationService,
        CancellationToken cancellationToken)
    {
        var userId = TryGetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var result = await conversationService.SendMessageAsync(userId.Value, request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("threads/message/stream")]
    public async Task StreamMessage(
        [FromBody] SendAssistantChatMessageRequestDto request,
        [FromServices] IAssistantConversationService conversationService,
        CancellationToken cancellationToken)
    {
        var userId = TryGetUserId();
        if (userId is null)
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        Response.StatusCode = StatusCodes.Status200OK;
        Response.ContentType = "application/x-ndjson";

        try
        {
            await foreach (var evt in conversationService.StreamMessageAsync(userId.Value, request, cancellationToken))
            {
                var line = JsonSerializer.Serialize(evt, JsonOptions);
                await Response.WriteAsync(line + "\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }
        catch (ArgumentException exception)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            var payload = JsonSerializer.Serialize(new { message = exception.Message }, JsonOptions);
            await Response.WriteAsync(payload, cancellationToken);
        }
    }

    [HttpPatch("threads/{threadId:int}")]
    public async Task<IActionResult> RenameThread(
        int threadId,
        [FromBody] RenameAssistantChatThreadRequestDto request,
        [FromServices] IAssistantConversationService conversationService,
        CancellationToken cancellationToken)
    {
        var userId = TryGetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var updated = await conversationService.RenameThreadAsync(userId.Value, threadId, request.Title, cancellationToken);
            return updated ? NoContent() : NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpDelete("threads/{threadId:int}")]
    public async Task<IActionResult> DeleteThread(
        int threadId,
        [FromServices] IAssistantConversationService conversationService,
        CancellationToken cancellationToken)
    {
        var userId = TryGetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var deleted = await conversationService.DeleteThreadAsync(userId.Value, threadId, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("suggestions")]
    public async Task<IActionResult> GetSuggestions(
        [FromServices] IPantryAssistantService pantryAssistantService,
        [FromQuery] int limit,
        CancellationToken cancellationToken)
    {
        var result = await pantryAssistantService.GetSuggestionsAsync(limit, cancellationToken);
        return Ok(result);
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat(
        [FromBody] PantryAssistantChatRequestDto request,
        [FromServices] IPantryAssistantService pantryAssistantService,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await pantryAssistantService.GetChatReplyAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    private int? TryGetUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idClaim, out var userId) ? userId : null;
    }
}
