using MediatR;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Interfaces;

namespace ProductService.Application.ProductFeatures.Queries.GetAllProducts;

public class GetAllProductsHandler : BaseHandler, IRequestHandler<GetAllProductsQuery, IEnumerable<Product>>
{
    public GetAllProductsHandler(IProductRepository repository) : base(repository) { }

    public async Task<IEnumerable<Product>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        if(cancellationToken.IsCancellationRequested)
            throw new TaskCanceledException();

        return await _repository.GetAllAsync();
    }
}
