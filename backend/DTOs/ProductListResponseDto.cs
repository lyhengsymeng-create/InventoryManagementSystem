namespace backend.DTOs;
public class ProductListResponseDto
{
    public List<ProductResponseDto> Items { get; set; } = [];

    public int Page { get; set; }
    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }

}