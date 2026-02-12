using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmallEcommerceApi.Db;
using SmallEcommerceApi.DTOs.Auth;
using SmallEcommerceApi.DTOs.Profile;
using SmallEcommerceApi.Models;
using SmallEcommerceApi.Models.Users;
using SmallEcommerceApi.Services.Interfaces;
using System.Security.Claims;
using SmallEcommerceApi.Security.Api.Security;

namespace SmallEcommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;
        private readonly IUserService _userService;
        private readonly AppDbContext _context;

        public ProfileController(IWishlistService wishlistService, IUserService userService, AppDbContext context)
        {
            _wishlistService = wishlistService;
            _userService = userService;
            _context = context;
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

        // GET: api/profile
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetUserId();
            if (!userId.HasValue) return Unauthorized();

            var user = await _userService.GetUserByIdAsync(userId.Value);
            var userProfile = await _userService.GetUserProfileAsync(userId.Value);

            if (user == null) return NotFound("User not found");

            // Combine User and UserProfile data
            var result = new
            {
                uid = user.UserId,
                username = user.Username,
                email = user.Email,
                name = user.FullName ?? $"{user.FirstName} {user.LastName}".Trim(),
                firstName = user.FirstName,
                lastName = user.LastName,
                phone = userProfile?.Phone,
                avatar = userProfile?.AvatarUrl,
                bio = userProfile?.Bio,
                dateOfBirth = userProfile?.DateOfBirth,
                gender = userProfile?.Gender,
                role = user.RoleName,
                createdAt = user.CreatedAt
            };

            return Ok(new { data = result });
        }

        // PUT: api/profile
        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = GetUserId();
            if (!userId.HasValue) return Unauthorized();

            // 1. Update Core User Info (Email, Name)
            // Note: In a real app, changing email might require verification.
            var existingUser = await _userService.GetUserByIdAsync(userId.Value);
            if (existingUser != null)
            {
                var updateUser = new UpdateUserDto
                {
                   Email = request.Email,
                   FirstName = request.Name // Simple mapping
                };
                
                // If name has spaces, try to split
                if (!string.IsNullOrEmpty(request.Name) && request.Name.Contains(" "))
                {
                    var parts = request.Name.Split(" ", 2);
                    updateUser.FirstName = parts[0];
                    updateUser.LastName = parts[1];
                }

                await _userService.UpdateUserAsync(userId.Value, updateUser);
            }

            // 2. Update Profile Info (Phone, Avatar)
            var updateProfile = new UpdateUserProfileDto
            {
                Phone = request.Phone,
                AvatarUrl = request.Avatar // Frontend might send avatar url here if handled separately
            };

            await _userService.UpdateUserProfileAsync(userId.Value, updateProfile);

            return await GetProfile();
        }

        // PUT: api/profile/password
        [HttpPut("password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            var userId = GetUserId();
            if (!userId.HasValue) return Unauthorized();

            var result = await _userService.ChangePasswordAsync(userId.Value, request);
            
            if (result)
            {
                return Ok(new { message = "Password updated successfully" });
            }
            
            return BadRequest(new { message = "Failed to update password. Check current password." });
        }

        // POST: api/profile/avatar
        [HttpPost("avatar")]
        public async Task<IActionResult> UploadAvatar([FromForm] IFormFile avatar)
        {
            var userId = GetUserId();
            if (!userId.HasValue) return Unauthorized();

            if (avatar == null || avatar.Length == 0)
                return BadRequest(new { message = "No file uploaded" });

            // File validation
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(avatar.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { message = "Invalid file type. Allowed: jpg, jpeg, png, gif, webp" });

            if (avatar.Length > 5 * 1024 * 1024) // 5MB limit
                return BadRequest(new { message = "File too large. Max 5MB" });

            // Save to vue-project/public/user_profile
            var uploadsFolder = Path.Combine(
                Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? "",
                "vue-project", "public", "user_profile");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Delete old avatar if exists
            var userProfile = await _userService.GetUserProfileAsync(userId.Value);
            if (!string.IsNullOrEmpty(userProfile?.AvatarUrl))
            {
                var oldFile = Path.Combine(
                    Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? "",
                    "vue-project", "public", userProfile.AvatarUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldFile))
                    System.IO.File.Delete(oldFile);
            }

            var fileName = $"{userId}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await avatar.CopyToAsync(stream);
            }

            // Update user profile with relative URL
            var avatarUrl = $"/user_profile/{fileName}";
            await _userService.UpdateUserProfileAsync(userId.Value, new UpdateUserProfileDto { AvatarUrl = avatarUrl });

            return Ok(new { data = new { avatar = avatarUrl } });
        }

        // GET: api/profile/addresses
        [HttpGet("addresses")]
        public async Task<IActionResult> GetAddresses()
        {
            var userId = GetUserId();
            if (!userId.HasValue) return Unauthorized();

            var addresses = await _context.Addresses
                .Where(a => a.UserId == userId.Value && a.IsActive)
                .OrderByDescending(a => a.IsDefault)
                .Select(a => new AddressDto
                {
                    Id = a.AddressId,
                    Street = a.AddressLine1, // Mapping AddressLine1 to Street
                    City = a.City,
                    State = a.State,
                    ZipCode = a.PostalCode,
                    Country = a.Country,
                    IsDefault = a.IsDefault
                })
                .ToListAsync();

            return Ok(new { data = addresses });
        }

        // POST: api/profile/addresses
        [HttpPost("addresses")]
        public async Task<IActionResult> AddAddress([FromBody] CreateAddressDto request)
        {
            var userId = GetUserId();
            if (!userId.HasValue) return Unauthorized();

            // If this is default, unset others
            if (request.IsDefault)
            {
                var existingDefaults = await _context.Addresses.Where(a => a.UserId == userId.Value && a.IsDefault).ToListAsync();
                foreach (var addr in existingDefaults)
                {
                    addr.IsDefault = false;
                }
            }

            var newAddress = new Address
            {
                UserId = userId.Value,
                AddressType = "Shipping", // Default
                AddressLine1 = request.Street,
                City = request.City,
                State = request.State,
                PostalCode = request.ZipCode,
                Country = request.Country,
                IsDefault = request.IsDefault,
                IsActive = true,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Addresses.Add(newAddress);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Address added" });
        }

        // PUT: api/profile/addresses/{id}
        [HttpPut("addresses/{id}")]
        public async Task<IActionResult> UpdateAddress(int id, [FromBody] UpdateAddressDto request)
        {
            var userId = GetUserId();
            if (!userId.HasValue) return Unauthorized();

            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.AddressId == id && a.UserId == userId.Value);
            if (address == null) return NotFound(new { message = "Address not found" });

             if (request.IsDefault && !address.IsDefault)
            {
                var existingDefaults = await _context.Addresses.Where(a => a.UserId == userId.Value && a.IsDefault).ToListAsync();
                foreach (var addr in existingDefaults)
                {
                    addr.IsDefault = false;
                }
            }

            address.AddressLine1 = request.Street;
            address.City = request.City;
            address.State = request.State;
            address.PostalCode = request.ZipCode;
            address.Country = request.Country;
            address.IsDefault = request.IsDefault;
            address.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Address updated" });
        }

        // DELETE: api/profile/addresses/{id}
        [HttpDelete("addresses/{id}")]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var userId = GetUserId();
            if (!userId.HasValue) return Unauthorized();

            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.AddressId == id && a.UserId == userId.Value);
            if (address == null) return NotFound(new { message = "Address not found" });

            address.IsActive = false; // Soft delete
            await _context.SaveChangesAsync();

            return Ok(new { message = "Address deleted" });
        }

        // PUT: api/profile/addresses/{id}/default
        [HttpPut("addresses/{id}/default")]
        public async Task<IActionResult> SetDefaultAddress(int id)
        {
             var userId = GetUserId();
            if (!userId.HasValue) return Unauthorized();

            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.AddressId == id && a.UserId == userId.Value);
            if (address == null) return NotFound(new { message = "Address not found" });

             var existingDefaults = await _context.Addresses.Where(a => a.UserId == userId.Value && a.IsDefault).ToListAsync();
             foreach (var addr in existingDefaults)
             {
                 addr.IsDefault = false;
             }

             address.IsDefault = true;
             await _context.SaveChangesAsync();
             
             return Ok(new { message = "Default address updated" });
        }

        // Payment Methods (MOCK)
        // Since we don't have a robust payment method table, we'll return an empty list or mock data
        // to prevent frontend errors.
        [HttpGet("payment-methods")]
        public IActionResult GetPaymentMethods()
        {
             // Mock data or empty
             return Ok(new { data = new List<object>() });
        }
        
        [HttpPost("payment-methods")]
        public IActionResult AddPaymentMethod([FromBody] object data)
        {
            return Ok(new { message = "Payment method added (Mock)" });
        }
        
        [HttpPut("payment-methods/{id}")]
        public IActionResult UpdatePaymentMethod(int id, [FromBody] object data)
        {
            return Ok(new { message = "Payment method updated (Mock)" });
        }
        
        [HttpDelete("payment-methods/{id}")]
        public IActionResult DeletePaymentMethod(int id)
        {
            return Ok(new { message = "Payment method deleted (Mock)" });
        }
         
        [HttpPut("payment-methods/{id}/default")]
        public IActionResult SetDefaultPaymentMethod(int id)
        {
            return Ok(new { message = "Default payment method updated (Mock)" });
        }



        // GET: api/profile/wishlist
        [HttpGet("wishlist")]
        public async Task<IActionResult> GetWishlist()
        {
            try
            {
                var userId = GetUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(new { message = "Please login to view your wishlist" });
                }

                var wishlistItems = await _wishlistService.GetWishlistAsync(userId.Value);

                // Map to frontend-friendly format (null-safe)
                var result = wishlistItems
                    .Where(item => item.Product != null) // Skip items with deleted products
                    .Select(item => new
                    {
                        id = item.WishlistItemId,
                        productId = item.ProductId,
                        name = item.Product!.ProductName,
                        price = item.Product.BasePrice,
                        image = item.Product.ProductImages?.FirstOrDefault()?.ImageUrl,
                        stock = item.Product.Stock,
                        inStock = item.Product.Stock > 0,
                        category = item.Product.ProductCategories?.FirstOrDefault()?.Category?.CategoryName,
                        addedAt = item.AddedAt
                    });

                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, inner = ex.InnerException?.Message });
            }
        }

        // POST: api/profile/wishlist
        [HttpPost("wishlist")]
        public async Task<IActionResult> AddToWishlist([FromBody] AddToWishlistRequest request)
        {
            try
            {
                var userId = GetUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(new { message = "Please login to add items to your wishlist" });
                }

                var wishlistItem = await _wishlistService.AddToWishlistAsync(userId.Value, request.ProductId);

                return Ok(new
                {
                    message = "Product added to wishlist",
                    data = new
                    {
                        id = wishlistItem.WishlistItemId,
                        productId = wishlistItem.ProductId,
                        name = wishlistItem.Product?.ProductName,
                        addedAt = wishlistItem.AddedAt
                    }
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // DELETE: api/profile/wishlist/{productId}
        [HttpDelete("wishlist/{productId}")]
        public async Task<IActionResult> RemoveFromWishlist(int productId)
        {
            try
            {
                var userId = GetUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(new { message = "Please login to manage your wishlist" });
                }

                var removed = await _wishlistService.RemoveFromWishlistAsync(userId.Value, productId);

                if (!removed)
                {
                    return NotFound(new { message = "Product not found in your wishlist" });
                }

                return Ok(new { message = "Product removed from wishlist" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }

    public class AddToWishlistRequest
    {
        public int ProductId { get; set; }
    }

    public class UpdateProfileRequest
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Avatar { get; set; }
    }
}
