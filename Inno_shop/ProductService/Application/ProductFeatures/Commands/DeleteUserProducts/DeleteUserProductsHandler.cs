using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Domain.Exceptions;
using ProductService.Infrastructure.Data;

namespace ProductService.Application.ProductFeatures.Commands.DeleteUserProducts;

public class DeleteUserProductsHandler : BaseHandler, IRequestHandler<DeleteUserProductsCommand>
{
    public DeleteUserProductsHandler(ProductDbContext context) : base(context) { }

    public async Task Handle(DeleteUserProductsCommand request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;

        var products = await _context.Products.Where(p => p.CreatorId == request.UserId).ToListAsync();
        _context.RemoveRange(products);
        await _context.SaveChangesAsync();
    }
}
