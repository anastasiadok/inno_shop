using MediatR;
using ProductService.Domain.Entities;

namespace ProductService.Application.ProductFeatures.Queries.GetAllProducts;

public record GetAllProductsQuery : IRequest<IEnumerable<Product>>;