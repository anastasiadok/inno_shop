using MediatR;
using ProductService.Domain.Entities;
using Sieve.Models;

namespace ProductService.Application.ProductFeatures.Queries.GetFilteredSortedProducts;

public record GetFilteredSortedProductsQuery(SieveModel SieveModel) : IRequest<IEnumerable<Product>>;