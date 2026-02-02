using System.ComponentModel.DataAnnotations;

namespace SmallEcommerceApi.DTOs.Products
{
    public class CreateProductDto
    {
        public string? ProductName { get; set; }
        public string? Description { get; set; }
        public decimal? BasePrice { get; set; }
        public string? SKU { get; set; }

        public int? Stock { get; set; } = 0;
        public int? MinStock { get; set; } = 10;
        public string? Supplier { get; set; }

        public List<string>? CategoryNames { get; set; }
        public List<int>? CategoryIds { get; set; }
        public List<string>? ImageUrls { get; set; }

        public bool IsActive { get; set; }
        public bool Featured { get; internal set; }
    }

    public class UpdateProductDto : CreateProductDto
    {
        [MaxLength(255)]
        public new string? ProductName { get; set; }

        public new string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Base price must be greater than 0")]
        public new decimal? BasePrice { get; set; }

        [MaxLength(100)]
        public new string? SKU { get; set; }

        public new int? Stock { get; set; }
        public new int? MinStock { get; set; }
        [MaxLength(200)]
        public new string? Supplier { get; set; }

        public new bool? IsActive { get; set; }
        public new bool? Featured { get; set; }

        public List<ProductVariantDto>? Variants { get; set; }
    }
}
