using MediatR;
using ProductService.Application.Dtos;

namespace ProductService.Application.ProductFeatures.Commands.UpdateProduct;

public record UpdateProductCommand(UpdateProductDto ProductDto, Guid UserId) : IRequest;
