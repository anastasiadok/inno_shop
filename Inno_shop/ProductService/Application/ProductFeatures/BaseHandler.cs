using ProductService.Infrastructure.Interfaces;

namespace ProductService.Application.ProductFeatures;

public class BaseHandler
{
    protected readonly IProductRepository _repository;

    public BaseHandler(IProductRepository repository)
    {
        _repository = repository;
    }
}
