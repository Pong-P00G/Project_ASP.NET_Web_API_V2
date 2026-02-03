#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmallEcommerceApi.Db;
using SmallEcommerceApi.DTOs.Products;
using SmallEcommerceApi.Models.Products;

namespace SmallEcommerceApi.Controllers.Products
{
    [ApiController]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ProductController(AppDbContext db) => _db = db;

        // Helper method - Can be used in memory
        private string GetStockStatus(int stock, int? minStock)
        {
            var min = minStock ?? 10;
            if (stock == 0) return "out-of-stock";
            if (stock <= min) return "low-stock";
            return "in-stock";
        }

        // CREATE PRODUCT
        [HttpPost]
        [Authorize(Roles = "ADMIN,Admin,admin")]
        public async Task<IActionResult> CreateProductWithVariants(
            [FromBody] CreateProductWithVariantDto dto)
        {
            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                // ─── VALIDATION ──────────────────────────
                if (string.IsNullOrWhiteSpace(dto.ProductName))
                    return BadRequest("ProductName is required");

                if (dto.BasePrice == null || dto.BasePrice <= 0)
                    return BadRequest("BasePrice must be greater than 0");

                if (dto.Variants == null || !dto.Variants.Any())
                    return BadRequest("At least one variant is required");

                // ─── PRODUCT ────────────────────────────
                var product = new Product
                {
                    ProductName = dto.ProductName,
                    Description = dto.Description ?? "",
                    BasePrice = dto.BasePrice.Value,
                    SKU = dto.SKU,
                    Stock = dto.Stock ?? 0,
                    MinStock = dto.MinStock ?? 10,
                    Supplier = dto.Supplier,
                    IsActive = dto.IsActive,
                    Featured = dto.Featured,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _db.Products.Add(product);
                await _db.SaveChangesAsync();

                // ─── CATEGORIES ─────────────────────────
                if (dto.CategoryNames != null)
                {
                    foreach (var name in dto.CategoryNames.Where(x => !string.IsNullOrWhiteSpace(x)))
                    {
                        var category = await _db.Categories
                            .FirstOrDefaultAsync(c => c.CategoryName == name.Trim());

                        if (category == null)
                        {
                            category = new Category
                            {
                                CategoryName = name.Trim(),
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };
                            _db.Categories.Add(category);
                            await _db.SaveChangesAsync();
                        }

                        if (!await _db.ProductCategories.AnyAsync(pc =>
                            pc.ProductId == product.ProductId &&
                            pc.CategoryId == category.CategoryId))
                        {
                            _db.ProductCategories.Add(new ProductCategory
                            {
                                ProductId = product.ProductId,
                                CategoryId = category.CategoryId
                            });
                        }
                    }
                }

                // ─── IMAGES ─────────────────────────────
                if (dto.ImageUrls != null)
                {
                    int order = 0;
                    foreach (var img in dto.ImageUrls.Where(x => !string.IsNullOrWhiteSpace(x)))
                    {
                        _db.ProductImages.Add(new ProductImage
                        {
                            ProductId = product.ProductId,
                            ImageUrl = img,
                            IsPrimary = order == 0,
                            DisplayOrder = order++,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                // ─── VARIANTS ───────────────────────────
                foreach (var v in dto.Variants)
                {
                    if (string.IsNullOrWhiteSpace(v.SKU))
                        return BadRequest("Variant SKU is required");

                    if (await _db.ProductVariants.AnyAsync(x => x.SKU == v.SKU))
                        return BadRequest($"Variant SKU '{v.SKU}' already exists");

                    var pv = new ProductVariant
                    {
                        ProductId = product.ProductId,
                        SKU = v.SKU,
                        Price = v.Price,
                        StockQuantity = v.StockQuantity,
                        IsActive = v.IsActive,
                        DiscountPrice = v.DiscountPrice,
                        DiscountPercentage = v.DiscountPercentage,
                        DiscountStartDate = v.DiscountStartDate,
                        DiscountEndDate = v.DiscountEndDate,
                        CreatedAt = DateTime.UtcNow
                    };

                    _db.ProductVariants.Add(pv);
                    await _db.SaveChangesAsync();

                    // Handle Variant Options
                    foreach (var opt in v.Options)
                    {
                        if (string.IsNullOrWhiteSpace(opt.Variant) || string.IsNullOrWhiteSpace(opt.Value))
                            return BadRequest(new { message = "Variant option name and value are required" });

                        // Variant (Color, Size, DPI)
                        var variant = await _db.Variants
                            .FirstOrDefaultAsync(x => x.Name == opt.Variant.Trim());

                        if (variant == null)
                        {
                            variant = new Variant
                            {
                                Name = opt.Variant.Trim(),
                                CreatedAt = DateTime.UtcNow
                            };
                            _db.Variants.Add(variant);
                            await _db.SaveChangesAsync(); // REQUIRED
                        }

                        // 🔹 VariantOption (Black, XL, 1600)
                        var option = await _db.VariantOptions
                            .FirstOrDefaultAsync(o =>
                                o.VariantId == variant.VariantId &&
                                o.OptionValue == opt.Value.Trim());

                        if (option == null)
                        {
                            option = new VariantOption
                            {
                                VariantId = variant.VariantId,
                                OptionValue = opt.Value.Trim(),
                                CreatedAt = DateTime.UtcNow
                            };
                            _db.VariantOptions.Add(option);
                            await _db.SaveChangesAsync(); // ✅ REQUIRED
                        }

                        // 🔹 NOW OptionId IS VALID
                        _db.ProductVariantOptions.Add(new ProductVariantOption
                        {
                            ProductVariantId = pv.ProductVariantId,
                            OptionId = option.OptionId
                        });
                    }

                }

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                return Ok(new
                {
                    message = "Product with variants created successfully",
                    productId = product.ProductId
                });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return BadRequest(new
                {
                    message = "Failed to create product",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }
   
        // GET ALL (Search + Pagination + Filters)
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] bool? featured = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? stockStatus = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _db.Products.AsNoTracking();

            // Search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.ProductName.Contains(search) ||
                    (p.SKU != null && p.SKU.Contains(search)) ||
                    (p.Supplier != null && p.Supplier.Contains(search)));
            }

            // Featured filter
            if (featured.HasValue)
                query = query.Where(p => p.Featured == featured.Value);

            // Active filter
            if (isActive.HasValue)
                query = query.Where(p => p.IsActive == isActive.Value);

            // Stock status filter
            if (!string.IsNullOrWhiteSpace(stockStatus))
            {
                query = stockStatus.ToLower() switch
                {
                    "out-of-stock" => query.Where(p => p.Stock == 0),
                    "low-stock" => query.Where(p => p.Stock > 0 && p.Stock <= p.MinStock),
                    "in-stock" => query.Where(p => p.Stock > p.MinStock),
                    _ => query
                };
            }

            var totalItems = await query.CountAsync();

            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductResponseDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Description = p.Description,
                    BasePrice = p.BasePrice,
                    SKU = p.SKU,
                    Stock = p.Stock,
                    MinStock = p.MinStock,
                    Supplier = p.Supplier,
                    StockStatus = p.Stock == 0 ? "out-of-stock" :
                                  p.Stock <= p.MinStock ? "low-stock" :
                                  "in-stock",
                    IsActive = p.IsActive,
                    Featured = p.Featured,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    Categories = p.ProductCategories.Select(pc => new CategoryDto
                    {
                        CategoryId = pc.CategoryId,
                        CategoryName = pc.Category != null ? (pc.Category!.CategoryName ?? string.Empty) : string.Empty
                    }).ToList(),
                    Images = p.ProductImages.Select(img => new ImageDto
                    {
                        ImageId = img.ImageId,
                        ImageUrl = img.ImageUrl,
                        IsPrimary = img.IsPrimary,
                        DisplayOrder = img.DisplayOrder
                    }).ToList(),
                    Variants = p.ProductVariants!.Select(pv => new VariantResponseDto
                    {
                        ProductVariantId = pv.ProductVariantId,
                        SKU = pv.SKU,
                        Price = pv.Price,
                        StockQuantity = pv.StockQuantity,
                        DiscountPrice = pv.DiscountPrice,
                        DiscountPercentage = pv.DiscountPercentage,
                        DiscountStartDate = pv.DiscountStartDate,
                        DiscountEndDate = pv.DiscountEndDate,
                        Options = pv.ProductVariantOptions.Select(pvo => new VariantOptionDto
                        {
                            Variant = pvo.VariantOption.Variant.Name ?? string.Empty,
                            Value = pvo.VariantOption.OptionValue
                        }).ToList()
                    }).ToList()
                })
                .ToListAsync();

            return Ok(new
            {
                page,
                pageSize,
                totalItems,
                totalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                items = products
            });
        }

        // GET BY ID
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await GetProductDtoById(id);
            if (product == null)
                return NotFound(new { message = "Product not found" });
            return Ok(product);
        }

        // UPDATE
        [HttpPut("{id:int}")]
        [Authorize(Roles = "ADMIN,Admin,admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            if (!string.IsNullOrEmpty(dto.SKU) && dto.SKU != product.SKU)
            {
                if (await _db.Products.AnyAsync(p => p.SKU == dto.SKU))
                    return BadRequest(new { message = "SKU already exists" });

                product.SKU = dto.SKU;
            }

            if (dto.BasePrice.HasValue && dto.BasePrice < 0)
                return BadRequest(new { message = "Base price must be >= 0" });

            if (!string.IsNullOrEmpty(dto.ProductName))
                product.ProductName = dto.ProductName;

            if (dto.Description != null)
                product.Description = dto.Description;

            if (dto.BasePrice.HasValue)
                product.BasePrice = dto.BasePrice.Value;

            if (dto.Stock.HasValue)
                product.Stock = dto.Stock.Value;

            if (dto.MinStock.HasValue)
                product.MinStock = dto.MinStock.Value;

            if (!string.IsNullOrEmpty(dto.Supplier))
                product.Supplier = dto.Supplier;

            if (dto.IsActive.HasValue)
                product.IsActive = dto.IsActive.Value;

            if (dto.Featured.HasValue)
                product.Featured = dto.Featured.Value;


            if (dto.ImageUrls != null)
            {
                var existingImages = _db.ProductImages.Where(i => i.ProductId == id);
                _db.ProductImages.RemoveRange(existingImages);

                int order = 0;
                foreach (var imgUrl in dto.ImageUrls.Where(url => !string.IsNullOrWhiteSpace(url)))
                {
                    _db.ProductImages.Add(new ProductImage
                    {
                        ProductId = id,
                        ImageUrl = imgUrl,
                        IsPrimary = order == 0,
                        DisplayOrder = order++,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            // Categories
            if (dto.CategoryNames != null)
            {
                var existingCategories = _db.ProductCategories.Where(pc => pc.ProductId == id);
                _db.ProductCategories.RemoveRange(existingCategories);

                foreach (var name in dto.CategoryNames.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    var category = await _db.Categories
                        .FirstOrDefaultAsync(c => c.CategoryName == name.Trim());

                    if (category == null)
                    {
                        category = new Category
                        {
                            CategoryName = name.Trim(),
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        _db.Categories.Add(category);
                        await _db.SaveChangesAsync(); // Need ID for new category
                    }

                    _db.ProductCategories.Add(new ProductCategory
                    {
                        ProductId = product.ProductId,
                        CategoryId = category.CategoryId
                    });
                }
            }

            // Variants
            if (dto.Variants != null)
            {
                var existingVariants = _db.ProductVariants
                    .Include(v => v.ProductVariantOptions)
                    .Where(v => v.ProductId == id);
                
                foreach (var v in existingVariants)
                {
                    _db.ProductVariantOptions.RemoveRange(v.ProductVariantOptions);
                }
                _db.ProductVariants.RemoveRange(existingVariants);
                await _db.SaveChangesAsync();

                foreach (var v in dto.Variants)
                {
                    if (string.IsNullOrWhiteSpace(v.SKU)) continue;

                    var pv = new ProductVariant
                    {
                        ProductId = product.ProductId,
                        SKU = v.SKU,
                        Price = v.Price,
                        StockQuantity = v.StockQuantity,
                        IsActive = v.IsActive,
                        DiscountPrice = v.DiscountPrice,
                        DiscountPercentage = v.DiscountPercentage,
                        DiscountStartDate = v.DiscountStartDate,
                        DiscountEndDate = v.DiscountEndDate,
                        CreatedAt = DateTime.UtcNow
                    };

                    _db.ProductVariants.Add(pv);
                    await _db.SaveChangesAsync();

                    if (v.Options != null)
                    {
                        foreach (var opt in v.Options)
                        {
                            if (string.IsNullOrWhiteSpace(opt.Variant) || string.IsNullOrWhiteSpace(opt.Value)) continue;

                            var variant = await _db.Variants.FirstOrDefaultAsync(x => x.Name == opt.Variant.Trim());
                            if (variant == null)
                            {
                                variant = new Variant { Name = opt.Variant.Trim(), CreatedAt = DateTime.UtcNow };
                                _db.Variants.Add(variant);
                                await _db.SaveChangesAsync();
                            }

                            var option = await _db.VariantOptions.FirstOrDefaultAsync(x => x.VariantId == variant.VariantId && x.OptionValue == opt.Value.Trim());
                            if (option == null)
                            {
                                option = new VariantOption { VariantId = variant.VariantId, OptionValue = opt.Value.Trim() };
                                _db.VariantOptions.Add(option);
                                await _db.SaveChangesAsync();
                            }

                            _db.ProductVariantOptions.Add(new ProductVariantOption
                            {
                                ProductVariantId = pv.ProductVariantId,
                                OptionId = option.OptionId
                            });
                        }
                    }
                }
            }

            product.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            var result = await GetProductDtoById(id);
            return Ok(result);
        }

        // SOFT DELETE
        [HttpDelete("{id:int}/Soft")] //Soft it does not remove the row from the database
        [Authorize(Roles = "ADMIN,Admin,admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(new { message = $"Product {id} has been deactivated" });
        }

        [HttpDelete("{id:int}/hard")]
        [Authorize(Roles = "ADMIN,Admin,admin")]
        public async Task<IActionResult> HardDelete(int id)
        {
            var product = await _db.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductCategories)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
                return NotFound(new { message = "Product not found" });

            // Remove related entities first (FK safety)
            _db.ProductImages.RemoveRange(product.ProductImages);
            _db.ProductCategories.RemoveRange(product.ProductCategories);

            _db.Products.Remove(product);

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = $"Product {id} permanently deleted"
            });
        }


        // INVENTORY MANAGEMENT - Update single product stock
        [HttpPatch("{id:int}/stock")]
        [Authorize(Roles = "ADMIN,Admin,admin")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] UpdateStockDto dto)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            if (dto.Quantity < 0)
                return BadRequest(new { message = "Quantity cannot be negative" });

            product.Stock = dto.Quantity;
            product.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                productId = product.ProductId,
                stock = product.Stock,
                stockStatus = GetStockStatus(product.Stock, product.MinStock),
                message = "Stock updated successfully"
            });
        }

        // BULK STOCK UPDATE
        [HttpPatch("bulk-stock")]
        [Authorize(Roles = "ADMIN,Admin,admin")]
        public async Task<IActionResult> BulkUpdateStock([FromBody] List<BulkStockUpdateDto> updates)
        {
            if (updates == null || !updates.Any())
                return BadRequest(new { message = "No updates provided" });

            var productIds = updates.Select(u => u.ProductId).ToList();
            var products = await _db.Products.Where(p => productIds.Contains(p.ProductId)).ToListAsync();

            foreach (var update in updates)
            {
                var product = products.FirstOrDefault(p => p.ProductId == update.ProductId);
                if (product != null && update.Quantity >= 0)
                {
                    product.Stock = update.Quantity;
                    product.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _db.SaveChangesAsync();

            return Ok(new { message = "Stock updated successfully", updated = products.Count });
        }

        // Helper method to get product DTO
        private async Task<ProductResponseDto?> GetProductDtoById(int id)
        {
            // ⭐ FIX: Calculate stock status in SQL
            var product = await _db.Products
                .Where(p => p.ProductId == id)
                .Select(p => new ProductResponseDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Description = p.Description,
                    BasePrice = p.BasePrice,
                    SKU = p.SKU,
                    Stock = p.Stock,
                    MinStock = p.MinStock,
                    Supplier = p.Supplier,
                    StockStatus = p.Stock == 0 ? "out-of-stock" :
                                  p.Stock <= p.MinStock ? "low-stock" :
                                  "in-stock",
                    IsActive = p.IsActive,
                    Featured = p.Featured,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    Categories = p.ProductCategories.Select(pc => new CategoryDto
                    {
                        CategoryId = pc.CategoryId,
                        CategoryName = pc.Category != null ? (pc.Category!.CategoryName ?? string.Empty) : string.Empty
                    }).ToList(),
                    Images = p.ProductImages.Select(img => new ImageDto
                    {
                        ImageId = img.ImageId,
                        ImageUrl = img.ImageUrl,
                        IsPrimary = img.IsPrimary,
                        DisplayOrder = img.DisplayOrder
                    }).OrderBy(i => i.DisplayOrder).ToList(),
                    Variants = p.ProductVariants!.Select(pv => new VariantResponseDto
                    {
                        ProductVariantId = pv.ProductVariantId,
                        SKU = pv.SKU,
                        Price = pv.Price,
                        StockQuantity = pv.StockQuantity,
                        DiscountPrice = pv.DiscountPrice,
                        DiscountPercentage = pv.DiscountPercentage,
                        DiscountStartDate = pv.DiscountStartDate,
                        DiscountEndDate = pv.DiscountEndDate,
                        Options = pv.ProductVariantOptions.Select(pvo => new VariantOptionDto
                        {
                            Variant = pvo.VariantOption.Variant.Name ?? string.Empty,
                            Value = pvo.VariantOption.OptionValue
                        }).ToList()
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return product;
        }
    }

    // Additional DTOs for stock management
    public class UpdateStockDto
    {
        public int Quantity { get; set; }
    }

    public class BulkStockUpdateDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}