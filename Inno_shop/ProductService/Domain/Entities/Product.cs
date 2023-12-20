using Sieve.Attributes;

namespace ProductService.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public decimal Price { get; set; }

    public bool IsAvailible { get; set; }

    public Guid CreatorId { get; set; }

    public DateTime CreationDate { get; set; }
}
