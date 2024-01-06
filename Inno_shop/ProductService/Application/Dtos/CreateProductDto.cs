namespace ProductService.Application.Dtos;

public record CreateProductDto(
    string Name,
    string Description,
    decimal Price, 
    bool IsAvailible, 
    Guid CreatorId);
