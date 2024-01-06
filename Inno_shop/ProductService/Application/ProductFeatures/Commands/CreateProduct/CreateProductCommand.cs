using MediatR;
using ProductService.Application.Dtos;

namespace ProductService.Application.ProductFeatures.Commands.CreateProduct;

public record CreateProductCommand(CreateProductDto ProductDto, Guid UserId) : IRequest;