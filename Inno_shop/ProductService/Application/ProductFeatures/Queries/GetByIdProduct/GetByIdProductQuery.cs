using MediatR;
using ProductService.Domain.Entities;

namespace ProductService.Application.ProductFeatures.Queries.GetByIdProduct
{
    public record GetByIdProductQuery(Guid Id) : IRequest<Product>;
}