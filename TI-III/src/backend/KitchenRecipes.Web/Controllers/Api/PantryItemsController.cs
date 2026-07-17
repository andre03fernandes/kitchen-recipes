using KitchenRecipes.Application.Abstractions;
using KitchenRecipes.Application.DTOs;
using KitchenRecipes.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KitchenRecipes.Web.Controllers.Api;

[ApiController]
[Route("api/pantry-items")]
[Authorize]
public sealed class PantryItemsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromServices] IPantryItemService pantryItemService,
        CancellationToken cancellationToken)
    {
        var pantryItems = await pantryItemService.GetAllAsync(cancellationToken);
        return Ok(pantryItems);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id,
        [FromServices] IPantryItemService pantryItemService,
        CancellationToken cancellationToken)
    {
        var pantryItem = await pantryItemService.GetByIdAsync(id, cancellationToken);
        return pantryItem is null ? NotFound() : Ok(pantryItem);
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Create(
        [FromBody] UpsertPantryItemRequest request,
        [FromServices] IPantryItemService pantryItemService,
        CancellationToken cancellationToken)
    {
        try
        {
            var createdPantryItem = await pantryItemService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = createdPantryItem.Id }, createdPantryItem);
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
        [FromBody] UpsertPantryItemRequest request,
        [FromServices] IPantryItemService pantryItemService,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await pantryItemService.UpdateAsync(id, request, cancellationToken);
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
        [FromServices] IPantryItemService pantryItemService,
        CancellationToken cancellationToken)
    {
        var deleted = await pantryItemService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
