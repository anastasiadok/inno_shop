using Microsoft.Extensions.Options;
using ProductService.Domain.Entities;
using Sieve.Models;
using Sieve.Services;

namespace ProductService.Application.ProductFeatures.Queries.GetFilteredSortedProducts;

public class ApplicationSieveProcessor : SieveProcessor
{
    public ApplicationSieveProcessor(IOptions<SieveOptions> options) : base(options) { }

    protected override SievePropertyMapper MapProperties(SievePropertyMapper mapper)
    {
        mapper.Property<Product>(p => p.Price).CanFilter().CanSort();

        mapper.Property<Product>(p => p.CreationDate).CanFilter().CanSort();

        mapper.Property<Product>(p => p.IsAvailible).CanFilter();

        mapper.Property<Product>(p => p.CreatorId).CanFilter();

        mapper.Property<Product>(p => p.Name).CanSort();

        return mapper;
    }
}