using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Data;

namespace ProductService.Application.ProductFeatures.Queries.GetAllProducts;

public class GetAllProductsHandler : BaseHandler, IRequestHandler<GetAllProductsQuery, IEnumerable<Product>>
{
    public GetAllProductsHandler(ProductDbContext context) : base(context) { }

    public async Task<IEnumerable<Product>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            return await _context.Products.AsNoTracking().ToListAsync();
        }

        return null;
    }
}
