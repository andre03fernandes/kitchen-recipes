using KitchenRecipes.Application.Abstractions;
using KitchenRecipes.Application.DTOs;
using KitchenRecipes.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KitchenRecipes.Web.Controllers.Api;

[ApiController]
[Route("api/recipes")]
[Authorize]
public sealed class RecipesController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromServices] IRecipeService recipeService,
        CancellationToken cancellationToken)
    {
        var recipes = await recipeService.GetAllAsync(cancellationToken);
        return Ok(recipes);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id,
        [FromServices] IRecipeService recipeService,
        CancellationToken cancellationToken)
    {
        var recipe = await recipeService.GetByIdAsync(id, cancellationToken);
        return recipe is null ? NotFound() : Ok(recipe);
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Create(
        [FromBody] UpsertRecipeRequest request,
        [FromServices] IRecipeService recipeService,
        CancellationToken cancellationToken)
    {
        try
        {
            var createdRecipe = await recipeService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = createdRecipe.Id }, createdRecipe);
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
        [FromBody] UpsertRecipeRequest request,
        [FromServices] IRecipeService recipeService,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await recipeService.UpdateAsync(id, request, cancellationToken);
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
        [FromServices] IRecipeService recipeService,
        CancellationToken cancellationToken)
    {
        var deleted = await recipeService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
