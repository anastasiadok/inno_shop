using MediatR;
using ProductService.Domain.Exceptions;
using ProductService.Infrastructure.Data;

namespace ProductService.Application.ProductFeatures.Commands.DeleteProduct;

public class DeleteProductHandler : BaseHandler, IRequestHandler<DeleteProductCommand, bool>
{
    public DeleteProductHandler(ProductDbContext context) : base(context) { }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            var product = await _context.Products.FindAsync(request.ProductId);
            if (product is null)
                throw new NotFoundException("Product");

            _context.Remove(product);
            _context.SaveChanges();

            return true;
        }

        return false;
    }
}
