using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Api.Dtos;
using StockFlow.Api.Security;
using StockFlow.Api.Services;

namespace StockFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/categories")]
public sealed class CategoriesController(ICatalogService catalogService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> GetAll(
        CancellationToken cancellationToken) =>
        Ok(await catalogService.GetCategoriesAsync(cancellationToken));

    [HttpPost]
    [Authorize(Roles = UserRoles.Manager)]
    public async Task<ActionResult<CategoryResponse>> Create(
        CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = await catalogService.CreateCategoryAsync(request, cancellationToken);
        return Created($"/api/categories/{category.Id}", category);
    }
}
