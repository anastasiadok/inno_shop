using MediatR;

namespace ProductService.Application.ProductFeatures.Commands.DeleteUserProducts;

public record DeleteUserProductsCommand(Guid RequestUserId, Guid AuthUserId) : IRequest;