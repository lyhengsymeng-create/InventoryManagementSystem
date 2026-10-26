using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;
public class ProductUpdateDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string SKU { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "999999999999.99")]
    public decimal UnitPrice { get; set; }

    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(1, int.MaxValue)]
    public int? CategoryId {get; set;}

}