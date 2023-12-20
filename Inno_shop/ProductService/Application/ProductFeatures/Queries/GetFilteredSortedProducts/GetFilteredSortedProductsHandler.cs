using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Data;
using Sieve.Services;

namespace ProductService.Application.ProductFeatures.Queries.GetFilteredSortedProducts;

public class GetFilteredSortedProductsHandler : BaseHandler, IRequestHandler<GetFilteredSortedProductsQuery, IEnumerable<Product>>
{
    private readonly SieveProcessor _sieveProcessor;
    public GetFilteredSortedProductsHandler(ProductDbContext context, SieveProcessor sieveProcessor) : base(context) 
    {
        _sieveProcessor = sieveProcessor;
    }

    public async Task<IEnumerable<Product>> Handle(GetFilteredSortedProductsQuery request, CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            var products = _context.Products.AsNoTracking();
            products = _sieveProcessor.Apply(request.SieveModel, products);
            return await products.ToListAsync();
        }
       
        return null;
    }
}