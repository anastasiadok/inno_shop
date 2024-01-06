using MediatR;
using ProductService.Domain.Entities;
using ProductService.Domain.Exceptions;
using ProductService.Infrastructure.Data;

namespace ProductService.Application.ProductFeatures.Commands.DeleteProduct;

public class DeleteProductHandler : BaseHandler, IRequestHandler<DeleteProductCommand>
{
    public DeleteProductHandler(ProductDbContext context) : base(context) { }

    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;

        var product = await _context.Products.FindAsync(request.ProductId) 
            ?? throw new NotFoundException(nameof(Product));

        if (request.UserId != product.CreatorId)
            throw new UserAccessException();

        _context.Remove(product);
        await _context.SaveChangesAsync();
    }
}
