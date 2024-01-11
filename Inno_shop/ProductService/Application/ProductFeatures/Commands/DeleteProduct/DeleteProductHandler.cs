using MediatR;
using ProductService.Domain.Entities;
using ProductService.Domain.Exceptions;
using ProductService.Infrastructure.Interfaces;

namespace ProductService.Application.ProductFeatures.Commands.DeleteProduct;

public class DeleteProductHandler : BaseHandler, IRequestHandler<DeleteProductCommand>
{
    public DeleteProductHandler(IProductRepository repository) : base(repository) { }

    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new TaskCanceledException();

        var product = await _repository.GetByIdAsync(request.ProductId)
            ?? throw new NotFoundException(nameof(Product));

        if (request.UserId != product.CreatorId)
            throw new UserAccessException();

        await _repository.DeleteByIdAsync(request.ProductId);
    }
}
