

namespace SmallEcommerceApi.DTOs.Products
{
    public class ProductVariantDto
    {
        private static readonly List<int> list = new List<int>();

        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        // Discount fields
        public decimal? DiscountPrice { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public DateTime? DiscountStartDate { get; set; }
        public DateTime? DiscountEndDate { get; set; }
        
        // Computed fields for convenience
        public bool IsOnSale => DiscountPrice.HasValue && DiscountPrice < Price &&
            (!DiscountStartDate.HasValue || DiscountStartDate <= DateTime.UtcNow) &&
            (!DiscountEndDate.HasValue || DiscountEndDate >= DateTime.UtcNow);
        public decimal FinalPrice => IsOnSale ? DiscountPrice!.Value : Price;

        // Variant options: { "Size": "Large", "Color": "Red" }
        public List<VariantOptionDto> Options { get; set; } = new List<VariantOptionDto>();
    }
}