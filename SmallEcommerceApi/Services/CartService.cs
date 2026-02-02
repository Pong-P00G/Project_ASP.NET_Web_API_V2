using Microsoft.EntityFrameworkCore;
using SmallEcommerceApi.Db;
using SmallEcommerceApi.DTOs.Carts;
using SmallEcommerceApi.Models.Carts;
using SmallEcommerceApi.Models.Products;
using SmallEcommerceApi.Services.Interfaces;

namespace SmallEcommerceApi.Services
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _context;

        public CartService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Cart> GetCartAsync(int? userId, string? sessionId)
        {
            var cart = await GetOrCreateCartAsync(userId, sessionId);
            return cart;
        }

        public async Task<Cart> AddToCartAsync(int? userId, string? sessionId, AddToCartDto addToCartDto)
        {
            var cart = await GetOrCreateCartAsync(userId, sessionId);

            // Determine ProductVariantId
            int variantId = 0;
            if (addToCartDto.ProductVariantId.HasValue && addToCartDto.ProductVariantId.Value > 0)
            {
                variantId = addToCartDto.ProductVariantId.Value;
            }
            else if (addToCartDto.ProductId.HasValue)
            {
                // Find default variant or first variant for the product
                var variant = await _context.ProductVariants
                    .Where(v => v.ProductId == addToCartDto.ProductId.Value && v.IsActive)
                    .OrderBy(v => v.Price) // Default to lowest price? Or display order?
                    .FirstOrDefaultAsync();

                if (variant == null)
                {
                    // Check if product exists
                    var product = await _context.Products.FindAsync(addToCartDto.ProductId.Value);
                    if (product == null || !product.IsActive)
                    {
                         throw new Exception("Product not found or unavailable.");
                    }

                    // Create a default variant for this product so it can be added to cart
                    variant = new ProductVariant
                    {
                        ProductId = product.ProductId,
                        SKU = (product.SKU ?? $"PROD-{product.ProductId}") + "-DEF",
                        Price = product.BasePrice,
                        StockQuantity = product.Stock,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.ProductVariants.Add(variant);
                    await _context.SaveChangesAsync();
                }
                variantId = variant.ProductVariantId;
            }
            else
            {
                throw new ArgumentException("Either ProductId or ProductVariantId must be provided.");
            }

            // Check if item exists in cart
            var cartItem = cart.Items.FirstOrDefault(i => i.ProductVariantId == variantId);

            if (cartItem != null)
            {
                cartItem.Quantity += addToCartDto.Quantity;
            }
            else
            {
                // Get price from variant
                var variant = await _context.ProductVariants.FindAsync(variantId);
                if (variant == null) throw new Exception("Variant not found");

                cartItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductVariantId = variantId,
                    Quantity = addToCartDto.Quantity,
                    Price = variant.Price // Snapshot price
                };
                cart.Items.Add(cartItem);
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            return await GetCartAsync(userId, sessionId);
        }

        public async Task<Cart> RemoveFromCartAsync(int? userId, string? sessionId, int cartItemId)
        {
            var cart = await GetOrCreateCartAsync(userId, sessionId);
            var item = cart.Items.FirstOrDefault(i => i.CartItemId == cartItemId);

            if (item != null)
            {
                cart.Items.Remove(item);
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task<Cart> UpdateCartItemQuantityAsync(int? userId, string? sessionId, int cartItemId, int quantity)
        {
            var cart = await GetOrCreateCartAsync(userId, sessionId);
            var item = cart.Items.FirstOrDefault(i => i.CartItemId == cartItemId);

            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Items.Remove(item);
                    _context.CartItems.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task ClearCartAsync(int? userId, string? sessionId)
        {
            var cart = await GetOrCreateCartAsync(userId, sessionId);
            _context.CartItems.RemoveRange(cart.Items);
            await _context.SaveChangesAsync();
        }

        public async Task MergeCartsAsync(int userId, string sessionId)
        {
            var sessionCart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId && c.UserId == null);

            if (sessionCart == null || !sessionCart.Items.Any()) return;

            var userCart = await GetOrCreateCartAsync(userId, null);

            foreach (var item in sessionCart.Items)
            {
                var existingItem = userCart.Items.FirstOrDefault(i => i.ProductVariantId == item.ProductVariantId);
                if (existingItem != null)
                {
                    existingItem.Quantity += item.Quantity;
                }
                else
                {
                    var newItem = new CartItem
                    {
                        CartId = userCart.CartId,
                        ProductVariantId = item.ProductVariantId,
                        Quantity = item.Quantity,
                        Price = item.Price
                    };
                    userCart.Items.Add(newItem);
                    _context.CartItems.Add(newItem);
                }
            }

            _context.Carts.Remove(sessionCart); // Remove session cart
            await _context.SaveChangesAsync();
        }

        private async Task<Cart> GetOrCreateCartAsync(int? userId, string? sessionId)
        {
            IQueryable<Cart> query = _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .ThenInclude(p => p!.ProductImages); // Include images for display

            Cart? cart = null;

            if (userId.HasValue)
            {
                cart = await query.FirstOrDefaultAsync(c => c.UserId == userId);
            }
            else if (!string.IsNullOrEmpty(sessionId))
            {
                cart = await query.FirstOrDefaultAsync(c => c.SessionId == sessionId && c.UserId == null);
            }

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    SessionId = userId.HasValue ? null : sessionId,
                    Items = new List<CartItem>()
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }
    }
}
