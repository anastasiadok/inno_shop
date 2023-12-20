using ProductService.Infrastructure.Data;

namespace ProductService.Application.ProductFeatures;

public class BaseHandler
{
    protected readonly ProductDbContext _context;

    public BaseHandler(ProductDbContext context)
    {
        _context = context;
    }
}
