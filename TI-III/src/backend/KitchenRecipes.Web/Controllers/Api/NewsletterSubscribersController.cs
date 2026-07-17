using KitchenRecipes.Application.Abstractions;
using KitchenRecipes.Application.DTOs;
using KitchenRecipes.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KitchenRecipes.Web.Controllers.Api;

[ApiController]
[Route("api/newsletter-subscribers")]
[Authorize]
public sealed class NewsletterSubscribersController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromServices] INewsletterSubscriberService newsletterSubscriberService,
        CancellationToken cancellationToken)
    {
        var subscribers = await newsletterSubscriberService.GetAllAsync(cancellationToken);
        return Ok(subscribers);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id,
        [FromServices] INewsletterSubscriberService newsletterSubscriberService,
        CancellationToken cancellationToken)
    {
        var subscriber = await newsletterSubscriberService.GetByIdAsync(id, cancellationToken);
        return subscriber is null ? NotFound() : Ok(subscriber);
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Create(
        [FromBody] UpsertNewsletterSubscriberRequest request,
        [FromServices] INewsletterSubscriberService newsletterSubscriberService,
        CancellationToken cancellationToken)
    {
        try
        {
            var createdSubscriber = await newsletterSubscriberService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = createdSubscriber.Id }, createdSubscriber);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpsertNewsletterSubscriberRequest request,
        [FromServices] INewsletterSubscriberService newsletterSubscriberService,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await newsletterSubscriberService.UpdateAsync(id, request, cancellationToken);
            return updated ? NoContent() : NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Delete(
        int id,
        [FromServices] INewsletterSubscriberService newsletterSubscriberService,
        CancellationToken cancellationToken)
    {
        var deleted = await newsletterSubscriberService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
