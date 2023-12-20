using MediatR;
using ProductService.Domain.Entities;
using ProductService.Domain.Exceptions;
using ProductService.Infrastructure.Data;

namespace ProductService.Application.ProductFeatures.Queries.GetByIdProduct
{
    public class GetByIdProductHandler : BaseHandler, IRequestHandler<GetByIdProductQuery, Product>
    {
        public GetByIdProductHandler(ProductDbContext context) : base(context) { }

        public async Task<Product> Handle(GetByIdProductQuery request, CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                var product = await _context.Products.FindAsync(request.Id);
                if (product is null)
                    throw new NotFoundException("Product");
            }

            return null;
        }
    }
}
