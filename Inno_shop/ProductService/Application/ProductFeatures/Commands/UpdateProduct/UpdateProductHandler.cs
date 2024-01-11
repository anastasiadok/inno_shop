using Mapster;
using MediatR;
using ProductService.Domain.Entities;
using ProductService.Domain.Exceptions;
using ProductService.Infrastructure.Interfaces;

namespace ProductService.Application.ProductFeatures.Commands.UpdateProduct;

public class UpdateProductHandler : BaseHandler, IRequestHandler<UpdateProductCommand>
{
    public UpdateProductHandler(IProductRepository repository) : base(repository) { }

    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new TaskCanceledException();

        var product = await _repository.GetByIdAsync(request.ProductDto.Id) 
            ?? throw new NotFoundException(nameof(Product));

        if (request.UserId != product.CreatorId)
            throw new UserAccessException();

        request.ProductDto.Adapt(product);
        await _repository.UpdateAsync(product);
    }
}
