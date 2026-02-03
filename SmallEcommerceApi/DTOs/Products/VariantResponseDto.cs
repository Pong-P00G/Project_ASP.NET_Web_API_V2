namespace SmallEcommerceApi.DTOs.Products
{
    public class VariantResponseDto
    {
        public int ProductVariantId { get; set; }
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        
        // Discount fields
        public decimal? DiscountPrice { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public DateTime? DiscountStartDate { get; set; }
        public DateTime? DiscountEndDate { get; set; }
        
        // Computed fields
        public bool IsOnSale => DiscountPrice.HasValue && DiscountPrice < Price &&
            (!DiscountStartDate.HasValue || DiscountStartDate <= DateTime.UtcNow) &&
            (!DiscountEndDate.HasValue || DiscountEndDate >= DateTime.UtcNow);
        public decimal FinalPrice => IsOnSale ? DiscountPrice!.Value : Price;
        
        public List<VariantOptionDto> Options { get; set; } = new List<VariantOptionDto>();
    }
}

