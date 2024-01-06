namespace ProductService.Application.Dtos;

public record UpdateProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    bool IsAvailible);