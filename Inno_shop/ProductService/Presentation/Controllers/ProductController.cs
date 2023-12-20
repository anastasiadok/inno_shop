using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Dtos;
using ProductService.Application.ProductFeatures.Commands.AddProduct;
using ProductService.Application.ProductFeatures.Commands.DeleteProduct;
using ProductService.Application.ProductFeatures.Commands.UpdateProduct;
using ProductService.Application.ProductFeatures.Queries.GetAllProducts;
using ProductService.Application.ProductFeatures.Queries.GetByIdProduct;
using ProductService.Application.ProductFeatures.Queries.GetFilteredSortedProducts;
using ProductService.Domain.Entities;
using Sieve.Models;


namespace ProductService.Presentation.Controllers;

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

        if (products is null)
            return BadRequest();

        return Ok(products);
    }

    [HttpGet("id")]
    public async Task<ActionResult<Product>> GetById([FromRoute] Guid id)
    {
        var product = await _mediator.Send(new GetByIdProductQuery(id));

        if (product is null)
            return BadRequest();

        return Ok(product);
    }

    [HttpGet("filtersort")]
    public async Task<ActionResult<IEnumerable<Product>>> GetProductsByFilter([FromQuery] SieveModel sieveModel)
    {
        var products = await _mediator.Send(new GetFilteredSortedProductsQuery(sieveModel));

        if (products is null)
            return BadRequest();

        return Ok(products);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto product)
    {
        var result = await _mediator.Send(new CreateProductCommand(product));

        if (!result)
            return BadRequest();

        return Ok();
    }

    [HttpPut] 
    public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductDto product)
    {
        var result = await _mediator.Send(new UpdateProductCommand(product));

        if(!result)
            return BadRequest();

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new DeleteProductCommand(id));

        if (!result)
            return BadRequest();

        return Ok();
    }
}
