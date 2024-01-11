using MediatR;
using ProductService.Domain.Exceptions;
using ProductService.Infrastructure.Interfaces;

namespace ProductService.Application.ProductFeatures.Commands.DeleteUserProducts;

public class DeleteUserProductsHandler : BaseHandler, IRequestHandler<DeleteUserProductsCommand>
{
    public DeleteUserProductsHandler(IProductRepository repository) : base(repository) { }

    public async Task Handle(DeleteUserProductsCommand request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new TaskCanceledException();

        if (request.RequestUserId != request.AuthUserId)
            throw new UserAccessException();

        await _repository.DeleteUserProductsAsync(request.RequestUserId);
    }
}
