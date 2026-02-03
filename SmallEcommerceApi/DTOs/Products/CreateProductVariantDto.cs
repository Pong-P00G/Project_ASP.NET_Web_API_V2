namespace SmallEcommerceApi.DTOs.Products
{
    public class CreateProductVariantDto
    {
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

        // Discount fields
        public decimal? DiscountPrice { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public DateTime? DiscountStartDate { get; set; }
        public DateTime? DiscountEndDate { get; set; }

        // IDs from variant_option table
        public List<int> OptionIds { get; set; } = new();
    }
}
