using Microsoft.EntityFrameworkCore;
using SmallEcommerceApi.Db;
using SmallEcommerceApi.DTOs.Orders;
using SmallEcommerceApi.Models.Orders;
using SmallEcommerceApi.Services.Interfaces;

namespace SmallEcommerceApi.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<OrderResponseDto> CreateOrderAsync(int userId, string? sessionId, CreateOrderDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Try to find cart by userId first
                var cart = await _context.Carts
                    .Include(c => c.Items)
                        .ThenInclude(ci => ci.ProductVariant)
                            .ThenInclude(pv => pv.Product)
                                .ThenInclude(p => p!.ProductImages)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                // If no cart found by userId or cart is empty, try finding by sessionId
                if ((cart == null || !cart.Items.Any()) && !string.IsNullOrEmpty(sessionId))
                {
                    cart = await _context.Carts
                        .Include(c => c.Items)
                            .ThenInclude(ci => ci.ProductVariant)
                                .ThenInclude(pv => pv.Product)
                                    .ThenInclude(p => p!.ProductImages)
                        .FirstOrDefaultAsync(c => c.SessionId == sessionId);
                    
                    // If found by session, associate it with the user
                    if (cart != null && cart.Items.Any())
                    {
                        cart.UserId = userId;
                        cart.SessionId = null; // Clear session since it's now linked to user
                        await _context.SaveChangesAsync();
                    }
                }

                if (cart == null || !cart.Items.Any())
                {
                    throw new InvalidOperationException("Cart is empty or not found");
                }

                // ─── STOCK VALIDATION & DEDUCTION ─────────────────────────
                foreach (var item in cart.Items)
                {
                    var variant = item.ProductVariant;
                    if (variant == null) continue;

                    if (variant.StockQuantity < item.Quantity)
                    {
                        var prodName = variant.Product?.ProductName ?? "Unknown Product";
                        throw new InvalidOperationException($"Insufficient stock for {prodName} ({variant.SKU}). Available: {variant.StockQuantity}");
                    }

                    // Deduct from Variant Stock
                    variant.StockQuantity -= item.Quantity;

                    // Deduct from Parent Product Stock
                    if (variant.Product != null)
                    {
                        variant.Product.Stock -= item.Quantity;
                        
                        // Prevent negative stock just in case
                        if (variant.Product.Stock < 0) variant.Product.Stock = 0;
                    }
                }

                // Calculate totals
                decimal subtotal = cart.Items.Sum(item => item.Price * item.Quantity);
                decimal shippingCost = subtotal > 100 ? 0 : 15;
                decimal tax = subtotal * 0.08m;
                decimal total = subtotal + shippingCost + tax;

                // Generate unique order number
                string orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

                // Create order
                var order = new Order
                {
                    UserId = userId,
                    OrderNumber = orderNumber,
                    OrderStatus = "PENDING",
                    PaymentMethod = dto.PaymentMethod ?? "cash",
                    Phone = dto.Phone ?? "",
                    ShippingAddress = dto.Location ?? "",
                    Subtotal = subtotal,
                    ShippingCost = shippingCost,
                    Tax = tax,
                    TotalAmount = total,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Create order items from cart items
                foreach (var cartItem in cart.Items)
                {
                    // Safely get product info with null checks
                    string productName = "Unknown Product";
                    string? productImage = null;
                    
                    if (cartItem.ProductVariant?.Product != null)
                    {
                        productName = cartItem.ProductVariant.Product.ProductName ?? productName;
                        productImage = cartItem.ProductVariant.Product.ProductImages?.FirstOrDefault()?.ImageUrl;
                    }

                    var orderItem = new OrderItem
                    {
                        OrderId = order.OrderId,
                        ProductVariantId = cartItem.ProductVariantId,
                        ProductName = productName,
                        ProductImage = productImage,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.Price,
                        TotalPrice = cartItem.Price * cartItem.Quantity
                    };

                    _context.OrderItems.Add(orderItem);
                }

                // Clear the cart items
                _context.CartItems.RemoveRange(cart.Items);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // Return the created order
                return await GetOrderByIdAsync(userId, order.OrderId) 
                    ?? throw new InvalidOperationException("Failed to retrieve created order");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<OrderResponseDto>> GetOrdersAsync(int userId)
        {
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return orders.Select(MapToDto).ToList();
        }

        public async Task<OrderResponseDto?> GetOrderByIdAsync(int userId, int orderId)
        {
            var order = await _context.Orders
                .Where(o => o.UserId == userId && o.OrderId == orderId)
                .Include(o => o.Items)
                .FirstOrDefaultAsync();

            return order == null ? null : MapToDto(order);
        }

        public async Task<List<OrderResponseDto>> GetAllOrdersAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return orders.Select(MapToDto).ToList();
        }

        public async Task<OrderResponseDto?> GetOrderByIdForAdminAsync(int orderId)
        {
            var order = await _context.Orders
                .Where(o => o.OrderId == orderId)
                .Include(o => o.Items)
                .FirstOrDefaultAsync();

            return order == null ? null : MapToDto(order);
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            order.OrderStatus = status;
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        private static OrderResponseDto MapToDto(Order order)
        {
            return new OrderResponseDto
            {
                OrderId = order.OrderId,
                OrderNumber = order.OrderNumber,
                OrderStatus = order.OrderStatus,
                PaymentMethod = order.PaymentMethod,
                Phone = order.Phone,
                ShippingAddress = order.ShippingAddress,
                Subtotal = order.Subtotal,
                ShippingCost = order.ShippingCost,
                Tax = order.Tax,
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt,
                Items = order.Items.Select(item => new OrderItemResponseDto
                {
                    OrderItemId = item.OrderItemId,
                    ProductVariantId = item.ProductVariantId,
                    ProductName = item.ProductName,
                    ProductImage = item.ProductImage,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                }).ToList()
            };
        }
    }
}
