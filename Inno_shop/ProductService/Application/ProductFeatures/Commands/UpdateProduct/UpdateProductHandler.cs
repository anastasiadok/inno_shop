using MediatR;
using ProductService.Domain.Exceptions;
using ProductService.Infrastructure.Data;

namespace ProductService.Application.ProductFeatures.Commands.UpdateProduct;

public class UpdateProductHandler : BaseHandler, IRequestHandler<UpdateProductCommand, bool>
{
    public UpdateProductHandler(ProductDbContext context) : base(context) { }

    public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            var product = await _context.Products.FindAsync(request.ProductDto.Id);

            if (product is null)
                throw new NotFoundException("Product");

            product.Description = request.ProductDto.Description;
            product.Price = request.ProductDto.Price;
            product.Name = request.ProductDto.Name;
            product.IsAvailible = request.ProductDto.IsAvailible;

            await _context.SaveChangesAsync();

            return true;
        }

        return false;
    }
}
