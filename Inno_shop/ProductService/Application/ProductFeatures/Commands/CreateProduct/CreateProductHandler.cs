using Mapster;
using MediatR;
using ProductService.Domain.Entities;
using ProductService.Domain.Exceptions;
using ProductService.Infrastructure.Interfaces;

namespace ProductService.Application.ProductFeatures.Commands.CreateProduct;

public class CreateProductHandler : BaseHandler, IRequestHandler<CreateProductCommand>
{
    public CreateProductHandler(IProductRepository repository) : base(repository) { }

    public async Task Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new TaskCanceledException();

        if (request.UserId != request.ProductDto.CreatorId)
            throw new UserAccessException();

        var product = request.ProductDto.Adapt<Product>();
        product.Id = Guid.NewGuid();
        product.CreationDate = DateTime.UtcNow;

        await _repository.AddAsync(product);
    }
}
