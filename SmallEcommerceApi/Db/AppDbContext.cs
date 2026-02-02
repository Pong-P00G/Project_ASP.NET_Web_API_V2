using Microsoft.EntityFrameworkCore;
using SmallEcommerceApi.Models;
using SmallEcommerceApi.Models.Auth;
using SmallEcommerceApi.Models.Carts;
using SmallEcommerceApi.Models.Orders;
using SmallEcommerceApi.Models.Payments;
using SmallEcommerceApi.Models.Products;
using SmallEcommerceApi.Models.Users;
using SmallEcommerceApi.Models.Wishlists;

namespace SmallEcommerceApi.Db
{
      public class AppDbContext : DbContext
      {
            public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

            // USERS
            public DbSet<User> Users { get; set; }
            public DbSet<UserRole> UserRoles { get; set; }
            public DbSet<UserProfile> UserProfiles { get; set; }
            public DbSet<RefreshToken> RefreshTokens { get; set; }

            // PRODUCTS
            public DbSet<Product> Products => Set<Product>();
            public DbSet<ProductImage> ProductImages => Set<ProductImage>();
            public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
            public DbSet<Category> Categories => Set<Category>();

            public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
            public DbSet<ProductVariantOption> ProductVariantOptions => Set<ProductVariantOption>();

            public DbSet<Variant> Variants => Set<Variant>();
            public DbSet<VariantOption> VariantOptions => Set<VariantOption>();

            // ORDERS / CART
            public DbSet<Order> Orders => Set<Order>();
            public DbSet<OrderItem> OrderItems => Set<OrderItem>();
            public DbSet<Cart> Carts => Set<Cart>();
            public DbSet<CartItem> CartItems => Set<CartItem>();

            // PAYMENTS
            public DbSet<Payment> Payments => Set<Payment>();
            public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();

            public DbSet<Coupon> Coupons => Set<Coupon>();
            public DbSet<Address> Addresses => Set<Address>();
            public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

            // WISHLIST
            public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                // User configuration
                modelBuilder.Entity<User>(entity =>
                {
                    entity.HasIndex(e => e.Username).IsUnique();
                    entity.HasIndex(e => e.Email).IsUnique();

                    entity.Property(e => e.FullName)
                        .HasComputedColumnSql("CASE WHEN [first_name] IS NOT NULL AND [last_name] IS NOT NULL THEN ([first_name]+' ')+[last_name] WHEN [first_name] IS NOT NULL THEN [first_name] WHEN [last_name] IS NOT NULL THEN [last_name] END", stored: false);

                    entity.HasOne(e => e.Role)
                        .WithMany(r => r.Users)
                        .HasForeignKey(e => e.RoleId)
                        .OnDelete(DeleteBehavior.Restrict);

                    entity.HasOne(e => e.Profile)
                        .WithOne(p => p.User)
                        .HasForeignKey<UserProfile>(p => p.UserId)
                        .OnDelete(DeleteBehavior.Cascade);
                });

                // UserRole configuration
                modelBuilder.Entity<UserRole>(entity =>
                {
                    entity.HasIndex(e => e.RoleName).IsUnique();
                });

                // UserProfile configuration
                modelBuilder.Entity<UserProfile>(entity =>
                {
                    entity.HasIndex(e => e.UserId).IsUnique();
                });

                // RefreshToken configuration
                modelBuilder.Entity<RefreshToken>(entity =>
                {
                    entity.HasIndex(e => e.Token).IsUnique();

                    entity.HasOne(e => e.User)
                        .WithMany(u => u.RefreshTokens)
                        .HasForeignKey(e => e.UserId)
                        .OnDelete(DeleteBehavior.Cascade);
                });

            // PRODUCT CATEGORY (MANY-TO-MANY)
            modelBuilder.Entity<ProductCategory>(entity =>
                {
                    entity.ToTable("product_category");

                    entity.HasKey(x => new { x.ProductId, x.CategoryId });

                    entity.Property(x => x.ProductId).HasColumnName("product_id");
                    entity.Property(x => x.CategoryId).HasColumnName("category_id");

                    entity.HasOne(x => x.Product)
                            .WithMany(p => p.ProductCategories)
                            .HasForeignKey(x => x.ProductId)
                            .OnDelete(DeleteBehavior.Cascade);

                    entity.HasOne(x => x.Category)
                            .WithMany(c => c.ProductCategories)
                            .HasForeignKey(x => x.CategoryId)
                            .OnDelete(DeleteBehavior.Cascade);
                });

                // CATEGORY
                modelBuilder.Entity<Category>(entity =>
                  {
                        entity.ToTable("category");

                        entity.HasKey(x => x.CategoryId);

                        entity.Property(x => x.CategoryId)
                        .HasColumnName("category_id")
                        .ValueGeneratedOnAdd();

                        entity.Property(x => x.CategoryName)
                        .HasColumnName("category_name")
                        .HasMaxLength(100)
                        .IsRequired();

                        entity.Property(x => x.Description).HasColumnName("description");
                        entity.Property(x => x.ParentCategoryId).HasColumnName("parent_category_id");
                        entity.Property(x => x.IsActive).HasColumnName("is_active");
                        entity.Property(x => x.CreatedAt).HasColumnName("created_at");
                        entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

                        entity.HasIndex(x => x.CategoryName).IsUnique();
                  });

                  // PRODUCT
                  modelBuilder.Entity<Product>(entity =>
                  {
                        entity.ToTable("product");

                        entity.HasKey(x => x.ProductId);

                        entity.Property(x => x.ProductId)
                        .HasColumnName("product_id")
                        .ValueGeneratedOnAdd();

                        entity.Property(x => x.ProductName)
                        .HasColumnName("product_name")
                        .HasMaxLength(255)
                        .IsRequired();

                        entity.Property(x => x.Description).HasColumnName("description");
                        entity.Property(x => x.BasePrice).HasColumnName("base_price").HasColumnType("decimal(10,2)");
                        entity.Property(x => x.SKU).HasColumnName("sku").HasMaxLength(100);
                        entity.Property(x => x.IsActive).HasColumnName("is_active");
                        entity.Property(x => x.Featured).HasColumnName("featured");
                        entity.Property(x => x.CreatedAt).HasColumnName("created_at");
                        entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

                        entity.HasIndex(x => x.SKU)
                        .IsUnique()
                        .HasFilter("[sku] IS NOT NULL");
                  });

                  // PRODUCT IMAGE
                  modelBuilder.Entity<ProductImage>(entity =>
                  {
                        entity.ToTable("product_image");

                        entity.HasKey(x => x.ImageId);

                        entity.Property(x => x.ImageId)
                        .HasColumnName("image_id")
                        .ValueGeneratedOnAdd();

                        entity.Property(x => x.ProductId).HasColumnName("product_id");
                        entity.Property(x => x.ImageUrl).HasColumnName("image_url").HasMaxLength(500);
                        entity.Property(x => x.IsPrimary).HasColumnName("is_primary");
                        entity.Property(x => x.DisplayOrder).HasColumnName("display_order");
                        entity.Property(x => x.CreatedAt).HasColumnName("created_at");

                        entity.HasOne(x => x.Product)
                        .WithMany(p => p.ProductImages)
                        .HasForeignKey(x => x.ProductId)
                        .OnDelete(DeleteBehavior.Cascade);
                  });

                  // VARIANT
                  modelBuilder.Entity<Variant>(entity =>
                  {
                        entity.ToTable("variant");

                        entity.HasKey(x => x.VariantId);

                        entity.Property(x => x.VariantId)
                        .HasColumnName("variant_id")
                        .ValueGeneratedOnAdd();

                        entity.Property(x => x.Name)
                        .HasColumnName("name")
                        .HasMaxLength(50)
                        .IsRequired();

                        entity.Property(x => x.CreatedAt)
                        .HasColumnName("created_at");
                  });

                  // VARIANT OPTION
                  modelBuilder.Entity<VariantOption>(entity =>
                  {
                        entity.ToTable("variant_option");

                        entity.HasKey(x => x.OptionId);

                        entity.Property(x => x.OptionId)
                        .HasColumnName("option_id")
                        .ValueGeneratedOnAdd();

                        entity.Property(x => x.VariantId)
                        .HasColumnName("variant_id");

                        entity.Property(x => x.OptionValue)
                        .HasColumnName("option_value")
                        .HasMaxLength(50)
                        .IsRequired();

                        entity.Property(x => x.CreatedAt)
                        .HasColumnName("created_at");

                        entity.HasOne(x => x.Variant)
                        .WithMany(v => v.Options)
                        .HasForeignKey(x => x.VariantId)
                        .OnDelete(DeleteBehavior.Cascade);
                  });

                  // PRODUCT VARIANT
                  modelBuilder.Entity<ProductVariant>(entity =>
                  {
                        entity.ToTable("product_variant");

                        entity.HasKey(x => x.ProductVariantId);

                        entity.Property(x => x.ProductVariantId)
                        .HasColumnName("product_variant_id")
                        .ValueGeneratedOnAdd();

                        entity.Property(x => x.ProductId).HasColumnName("product_id");
                        entity.Property(x => x.SKU).HasColumnName("sku").HasMaxLength(100);
                        entity.Property(x => x.Price).HasColumnName("price").HasColumnType("decimal(10,2)");
                        entity.Property(x => x.StockQuantity).HasColumnName("stock_quantity");
                        entity.Property(x => x.IsActive).HasColumnName("is_active");
                        entity.Property(x => x.CreatedAt).HasColumnName("created_at");

                        entity.HasOne(x => x.Product)
                        .WithMany(p => p.ProductVariants)
                        .HasForeignKey(x => x.ProductId)
                        .OnDelete(DeleteBehavior.Cascade);
                  });

                  // PRODUCT VARIANT OPTION (JOIN TABLE)
                  modelBuilder.Entity<ProductVariantOption>(entity =>
                  {
                        entity.ToTable("product_variant_option");

                        entity.HasKey(x => new { x.ProductVariantId, x.OptionId });

                        entity.Property(x => x.ProductVariantId)
                        .HasColumnName("product_variant_id");

                        entity.Property(x => x.OptionId)
                        .HasColumnName("option_id");

                        entity.HasOne(x => x.ProductVariant)
                        .WithMany(pv => pv.ProductVariantOptions)
                        .HasForeignKey(x => x.ProductVariantId)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.HasOne(x => x.VariantOption)
                        .WithMany(vo => vo.ProductVariantOptions)
                        .HasForeignKey(x => x.OptionId)
                        .OnDelete(DeleteBehavior.Cascade);
                  });

                  // WISHLIST ITEM
                  modelBuilder.Entity<WishlistItem>(entity =>
                  {
                        entity.ToTable("wishlist_item");
                        entity.HasKey(x => x.WishlistItemId);

                        entity.Property(x => x.WishlistItemId)
                            .HasColumnName("wishlist_item_id")
                            .ValueGeneratedOnAdd();

                        entity.Property(x => x.UserId).HasColumnName("user_id");
                        entity.Property(x => x.ProductId).HasColumnName("product_id");
                        entity.Property(x => x.AddedAt).HasColumnName("added_at");

                        entity.HasOne(x => x.Product)
                            .WithMany()
                            .HasForeignKey(x => x.ProductId)
                            .OnDelete(DeleteBehavior.Cascade);

                        entity.HasIndex(x => new { x.UserId, x.ProductId }).IsUnique();
                  });

                  // ORDERS
                  modelBuilder.Entity<Order>(entity =>
                  {
                        entity.ToTable("orders");

                        entity.HasKey(x => x.OrderId);

                        entity.Property(x => x.OrderId)
                        .HasColumnName("order_id")
                        .ValueGeneratedOnAdd();

                        entity.Property(x => x.UserId).HasColumnName("user_id");
                        entity.Property(x => x.PaymentId).HasColumnName("payment_id");
                        entity.Property(x => x.OrderNumber).HasColumnName("order_number").HasMaxLength(100).IsRequired();
                        entity.Property(x => x.OrderStatus).HasColumnName("order_status").HasMaxLength(50);
                        entity.Property(x => x.PaymentMethod).HasColumnName("payment_method").HasMaxLength(50);
                        entity.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(20);
                        entity.Property(x => x.ShippingAddress).HasColumnName("shipping_address").HasMaxLength(500);
                        entity.Property(x => x.Subtotal).HasColumnName("subtotal").HasColumnType("decimal(10,2)");
                        entity.Property(x => x.ShippingCost).HasColumnName("shipping_cost").HasColumnType("decimal(10,2)");
                        entity.Property(x => x.Tax).HasColumnName("tax").HasColumnType("decimal(10,2)");
                        entity.Property(x => x.TotalAmount).HasColumnName("total_amount").HasColumnType("decimal(10,2)");
                        entity.Property(x => x.CreatedAt).HasColumnName("created_at");
                        entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

                        entity.HasOne(x => x.User)
                        .WithMany()
                        .HasForeignKey(x => x.UserId)
                        .OnDelete(DeleteBehavior.Cascade);
                  });

                  // ORDER ITEMS
                  modelBuilder.Entity<OrderItem>(entity =>
                  {
                        entity.ToTable("order_item");

                        entity.HasKey(x => x.OrderItemId);

                        entity.Property(x => x.OrderItemId)
                        .HasColumnName("order_item_id")
                        .ValueGeneratedOnAdd();

                        entity.Property(x => x.OrderId).HasColumnName("order_id");
                        entity.Property(x => x.ProductVariantId).HasColumnName("product_variant_id");
                        entity.Property(x => x.ProductName).HasColumnName("product_name").HasMaxLength(255);
                        entity.Property(x => x.ProductImage).HasColumnName("product_image").HasMaxLength(500);
                        entity.Property(x => x.Quantity).HasColumnName("quantity");
                        entity.Property(x => x.UnitPrice).HasColumnName("unit_price").HasColumnType("decimal(10,2)");
                        entity.Property(x => x.TotalPrice).HasColumnName("total_price").HasColumnType("decimal(10,2)");

                        entity.HasOne(x => x.Order)
                        .WithMany(o => o.Items)
                        .HasForeignKey(x => x.OrderId)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.HasOne(x => x.ProductVariant)
                        .WithMany()
                        .HasForeignKey(x => x.ProductVariantId)
                        .OnDelete(DeleteBehavior.Restrict);
                  });

                  // SEED ROLES
                  modelBuilder.Entity<UserRole>().HasData(
                      new UserRole { RoleId = 1, RoleName = "Admin" },
                      new UserRole { RoleId = 2, RoleName = "Customer" }
                  );

                  base.OnModelCreating(modelBuilder);
            }
      }
}
