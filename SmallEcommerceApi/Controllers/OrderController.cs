using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmallEcommerceApi.DTOs.Orders;
using SmallEcommerceApi.Services.Interfaces;
using System.Security.Claims;

namespace SmallEcommerceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value
                ?? User.FindFirst("userId")?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }
            return userId;
        }

        private string? GetSessionId()
        {
            // Get session ID from header (same as CartController)
            if (Request.Headers.TryGetValue("X-Cart-Session", out var headerSessionId) 
                && !string.IsNullOrEmpty(headerSessionId.ToString()))
            {
                return headerSessionId.ToString();
            }

            // Then try cookie
            if (Request.Cookies.TryGetValue("CartSessionId", out string? cookieSessionId) 
                && !string.IsNullOrEmpty(cookieSessionId))
            {
                return cookieSessionId;
            }

            return null;
        }

        /// <summary>
        /// Create a new order from the user's cart
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<OrderResponseDto>> CreateOrder([FromBody] CreateOrderDto dto)
        {
            try
            {
                var userId = GetUserId();
                var sessionId = GetSessionId();
                var order = await _orderService.CreateOrderAsync(userId, sessionId, dto);
                return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId }, order);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }
            catch (Exception ex)
            {
                // Log the full exception for debugging
                Console.WriteLine($"Order creation error: {ex}");
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get all orders for the authenticated user
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<OrderResponseDto>>> GetOrders()
        {
            try
            {
                var userId = GetUserId();
                var orders = await _orderService.GetOrdersAsync(userId);
                return Ok(orders);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }
        }

        /// <summary>
        /// Get a specific order by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponseDto>> GetOrder(int id)
        {
            try
            {
                var userId = GetUserId();
                var order = await _orderService.GetOrderByIdAsync(userId, id);
                
                if (order == null)
                {
                    return NotFound(new { message = "Order not found" });
                }
                
                return Ok(order);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }
        }

        /// <summary>
        /// Get all orders (Admin only)
        /// </summary>
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<OrderResponseDto>>> GetAllOrdersForAdmin()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        /// <summary>
        /// Get order details (Admin only)
        /// </summary>
        [HttpGet("admin/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<OrderResponseDto>> GetOrderForAdmin(int id)
        {
            var order = await _orderService.GetOrderByIdForAdminAsync(id);
            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }
            return Ok(order);
        }

        /// <summary>
        /// Update order status (Admin only)
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto dto)
        {
            var success = await _orderService.UpdateOrderStatusAsync(id, dto.Status);
            
            if (!success)
            {
                return NotFound(new { message = "Order not found" });
            }
            
            return Ok(new { message = "Order status updated successfully" });
        }
    }
}
