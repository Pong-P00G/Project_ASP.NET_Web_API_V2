using Microsoft.AspNetCore.Mvc;
using SmallEcommerceApi.DTOs.Carts;
using SmallEcommerceApi.Models.Carts;
using SmallEcommerceApi.Security.Api.Security;
using SmallEcommerceApi.Services.Interfaces;
using System.Security.Claims;

namespace SmallEcommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        private int? GetUserId()
        {
            // TokenService uses ClaimTypes.NameIdentifier for user ID
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                        ?? User.FindFirst(ClaimTypesCustom.UserId); // fallback to custom "uid"
            if (claim != null && int.TryParse(claim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }

        private string GetSessionId()
        {
            // First, try to get session ID from header (for cross-origin requests)
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

            // For guests without a session, generate one
            var sessionId = Guid.NewGuid().ToString();
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(30),
                SameSite = SameSiteMode.Lax,
                Secure = false,
                Path = "/"
            };
            Response.Cookies.Append("CartSessionId", sessionId, cookieOptions);
            
            return sessionId;
        }

        [HttpGet]
        public async Task<ActionResult<Cart>> GetCart()
        {
            try
            {
                var userId = GetUserId();
                var sessionId = GetSessionId(); // Always ensure session ID exists for continuity
                var cart = await _cartService.GetCartAsync(userId, sessionId);
                
                // We need to map to a DTO to avoid cycles and expose only needed data
                // For simplicity, we return Cart directly but configured JSON options handle cycles?
                // Ideally, return CartDto.
                // Let's rely on global JSON settings or implement a mapper.
                // The frontend expects { data: ... } or just ...
                // Looking at other controllers might help. 
                // But let's return Ok(cart).
                
                return Ok(cart);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<Cart>> AddToCart([FromBody] AddToCartDto addToCartDto)
        {
            try
            {
                var userId = GetUserId();
                var sessionId = GetSessionId();
                
                var cart = await _cartService.AddToCartAsync(userId, sessionId, addToCartDto);
                return Ok(cart);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("{cartItemId}")]
        public async Task<ActionResult<Cart>> UpdateCartItem(int cartItemId, [FromBody] AddToCartDto dto)
        {
            try
            {
                var userId = GetUserId();
                var sessionId = GetSessionId();
                var cart = await _cartService.UpdateCartItemQuantityAsync(userId, sessionId, cartItemId, dto.Quantity);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{cartItemId}")]
        public async Task<ActionResult<Cart>> RemoveFromCart(int cartItemId)
        {
            try
            {
                var userId = GetUserId();
                var sessionId = GetSessionId();
                var cart = await _cartService.RemoveFromCartAsync(userId, sessionId, cartItemId);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var userId = GetUserId();
                var sessionId = GetSessionId();
                await _cartService.ClearCartAsync(userId, sessionId);
                return Ok(new { message = "Cart cleared" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
