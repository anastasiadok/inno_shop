namespace ProductService.Application.Dtos;

public class UpdateProductDto
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public decimal Price { get; set; }

    public bool IsAvailible { get; set; }
}