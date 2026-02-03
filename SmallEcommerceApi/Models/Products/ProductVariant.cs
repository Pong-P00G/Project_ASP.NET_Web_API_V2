using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmallEcommerceApi.Models.Products
{
    public class ProductVariant
    {
        [Key]
        [Column("product_variant_id")]
        public int ProductVariantId { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("sku")]
        public string SKU { get; set; } = null!;

        [Column("price")]
        public decimal Price { get; set; }

        [Column("stock_quantity")]
        public int StockQuantity { get; set; } = 0;

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("discount_price")]
        public decimal? DiscountPrice { get; set; }

        [Column("discount_percentage")]
        public decimal? DiscountPercentage { get; set; }

        [Column("discount_start")]
        public DateTime? DiscountStartDate { get; set; }

        [Column("discount_end")]
        public DateTime? DiscountEndDate { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        public Product? Product { get; set; }
        public ICollection<ProductVariantOption> ProductVariantOptions { get; set; } = new List<ProductVariantOption>();
    }
}
