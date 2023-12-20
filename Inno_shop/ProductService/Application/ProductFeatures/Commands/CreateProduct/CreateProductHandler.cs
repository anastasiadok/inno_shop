using Mapster;
using MediatR;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Data;

namespace ProductService.Application.ProductFeatures.Commands.AddProduct;

public class CreateProductHandler : BaseHandler, IRequestHandler<CreateProductCommand, bool>
{
    public CreateProductHandler(ProductDbContext context) : base(context) { }

    public async Task<bool> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            var product = request.ProductDto.Adapt<Product>();
            product.Id = Guid.NewGuid();
            product.CreationDate = DateTime.UtcNow;

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return true;
        }

        return false;
    }
}
