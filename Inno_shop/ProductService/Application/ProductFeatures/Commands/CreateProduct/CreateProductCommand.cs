using MediatR;
using ProductService.Application.Dtos;

namespace ProductService.Application.ProductFeatures.Commands.AddProduct;

public record CreateProductCommand(CreateProductDto ProductDto) : IRequest<bool>;