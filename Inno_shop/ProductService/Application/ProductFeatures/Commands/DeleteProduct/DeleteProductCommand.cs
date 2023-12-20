using MediatR;

namespace ProductService.Application.ProductFeatures.Commands.DeleteProduct;

public record DeleteProductCommand(Guid ProductId) : IRequest<bool>;