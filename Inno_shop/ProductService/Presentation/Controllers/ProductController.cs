using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Dtos;
using ProductService.Application.ProductFeatures.Commands.CreateProduct;
using ProductService.Application.ProductFeatures.Commands.DeleteProduct;
using ProductService.Application.ProductFeatures.Commands.DeleteUserProducts;
using ProductService.Application.ProductFeatures.Commands.UpdateProduct;
using ProductService.Application.ProductFeatures.Queries.GetAllProducts;
using ProductService.Application.ProductFeatures.Queries.GetByIdProduct;
using ProductService.Application.ProductFeatures.Queries.GetFilteredSortedProducts;
using ProductService.Domain.Entities;
using Sieve.Models;
using System.Security.Claims;


namespace ProductService.Presentation.Controllers;

[Authorize]
[Route("api/products")]
[ApiController]
public class ProductController : Controller
{
    private readonly IMediator _mediator;
    
    public ProductController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll()
    {
        var products = await _mediator.Send(new GetAllProductsQuery());
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetById([FromRoute] Guid id)
    {
        var product = await _mediator.Send(new GetByIdProductQuery(id));
        return Ok(product);
    }

    [HttpGet("filtersort")]
    public async Task<ActionResult<IEnumerable<Product>>> GetProductsByFilter([FromQuery] SieveModel sieveModel)
    {
        var products = await _mediator.Send(new GetFilteredSortedProductsQuery(sieveModel));
        return Ok(products);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto product)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        await _mediator.Send(new CreateProductCommand(product, userId));
        return Ok();
    }

    [HttpPut] 
    public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductDto product)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        await _mediator.Send(new UpdateProductCommand(product, userId));
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct([FromRoute] Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        await _mediator.Send(new DeleteProductCommand(id, userId));
         return Ok();
    }

    [HttpDelete("user")]
    public async Task<IActionResult> DeleteUserProducts([FromQuery] Guid userid)
    {
        if (userid != Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            return Unauthorized();

        await _mediator.Send(new DeleteUserProductsCommand(userid));
        return Ok();
    }
}
