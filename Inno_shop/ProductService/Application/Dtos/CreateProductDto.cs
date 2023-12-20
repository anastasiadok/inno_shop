﻿namespace ProductService.Application.Dtos;

public class CreateProductDto
{
    public string Name { get; set; }

    public string Description { get; set; }

    public decimal Price { get; set; }

    public bool IsAvailible { get; set; }

    public Guid CreatorId { get; set; }
}
