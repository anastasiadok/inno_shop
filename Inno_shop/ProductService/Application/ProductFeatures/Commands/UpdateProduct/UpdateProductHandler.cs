using Mapster;
using MediatR;
using ProductService.Domain.Entities;
using ProductService.Domain.Exceptions;
using ProductService.Infrastructure.Data;

namespace ProductService.Application.ProductFeatures.Commands.UpdateProduct;

public class UpdateProductHandler : BaseHandler, IRequestHandler<UpdateProductCommand>
{
    public UpdateProductHandler(ProductDbContext context) : base(context) { }

    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;

        var product = await _context.Products.FindAsync(request.ProductDto.Id) 
            ?? throw new NotFoundException(nameof(Product));

        if (request.UserId != product.CreatorId)
            throw new UserAccessException();

        request.ProductDto.Adapt(product);
        await _context.SaveChangesAsync();
    }
}
