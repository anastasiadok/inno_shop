using Mapster;
using MediatR;
using ProductService.Domain.Entities;
using ProductService.Domain.Exceptions;
using ProductService.Infrastructure.Data;

namespace ProductService.Application.ProductFeatures.Commands.CreateProduct;

public class CreateProductHandler : BaseHandler, IRequestHandler<CreateProductCommand>
{
    public CreateProductHandler(ProductDbContext context) : base(context) { }

    public async Task Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;

        if (request.UserId != request.ProductDto.CreatorId)
            throw new UserAccessException();

        var product = request.ProductDto.Adapt<Product>();
        product.Id = Guid.NewGuid();
        product.CreationDate = DateTime.UtcNow;

        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
    }
}
