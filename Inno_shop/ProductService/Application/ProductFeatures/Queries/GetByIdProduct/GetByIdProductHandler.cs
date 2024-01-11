using MediatR;
using ProductService.Domain.Entities;
using ProductService.Domain.Exceptions;
using ProductService.Infrastructure.Interfaces;

namespace ProductService.Application.ProductFeatures.Queries.GetByIdProduct
{
    public class GetByIdProductHandler : BaseHandler, IRequestHandler<GetByIdProductQuery, Product>
    {
        public GetByIdProductHandler(IProductRepository repository) : base(repository) { }

        public async Task<Product> Handle(GetByIdProductQuery request, CancellationToken cancellationToken)
        {
            if(cancellationToken.IsCancellationRequested)
                throw new TaskCanceledException();

            return await _repository.GetByIdAsync(request.Id)
                ?? throw new NotFoundException(nameof(Product));
        }
    }
}
