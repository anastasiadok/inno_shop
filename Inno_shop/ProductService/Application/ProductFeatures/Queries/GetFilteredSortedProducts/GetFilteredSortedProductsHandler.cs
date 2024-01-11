using MediatR;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Interfaces;
using Sieve.Services;

namespace ProductService.Application.ProductFeatures.Queries.GetFilteredSortedProducts;

public class GetFilteredSortedProductsHandler : BaseHandler, IRequestHandler<GetFilteredSortedProductsQuery, IEnumerable<Product>>
{
    private readonly SieveProcessor _sieveProcessor;
    public GetFilteredSortedProductsHandler(IProductRepository repository, SieveProcessor sieveProcessor) : base(repository)
    {
        _sieveProcessor = sieveProcessor;
    }

    public Task<IEnumerable<Product>> Handle(GetFilteredSortedProductsQuery request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new TaskCanceledException();

        var products = _repository.GetAllIQueryable();
        var result = _sieveProcessor.Apply(request.SieveModel, products).ToList();
        return Task.FromResult<IEnumerable<Product>>(result);
    }
}