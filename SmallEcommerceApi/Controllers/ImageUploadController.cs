using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmallEcommerceApi.Controllers
{
    [ApiController]
    [Route("api/images")]
    public class ImageUploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;
        private readonly ILogger<ImageUploadController> _logger;

        // Allowed image extensions
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        
        // Max file size: 5MB
        private const long MaxFileSize = 5 * 1024 * 1024;

        public ImageUploadController(
            IWebHostEnvironment env, 
            IConfiguration config,
            ILogger<ImageUploadController> logger)
        {
            _env = env;
            _config = config;
            _logger = logger;
        }

        /// Upload product image(s)
        [HttpPost("upload")]
        [Authorize(Roles = "ADMIN,Admin,admin")]
        public async Task<IActionResult> UploadImage([FromForm] List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return BadRequest(new { message = "No files uploaded" });

            var uploadedUrls = new List<string>();
            var errors = new List<string>();

            // Get the Vue project's public/images folder path
            var vueProjectPath = _config["ImageUpload:VueProjectPath"] 
                ?? Path.Combine(Directory.GetParent(_env.ContentRootPath)?.FullName ?? "", "vue-project");
            var imagesFolder = Path.Combine(vueProjectPath, "public", "images");

            // Ensure directory exists
            if (!Directory.Exists(imagesFolder))
            {
                Directory.CreateDirectory(imagesFolder);
            }

            foreach (var file in files)
            {
                try
                {
                    // Validate file size
                    if (file.Length > MaxFileSize)
                    {
                        errors.Add($"{file.FileName}: File exceeds 5MB limit");
                        continue;
                    }

                    // Validate file extension
                    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!_allowedExtensions.Contains(extension))
                    {
                        errors.Add($"{file.FileName}: Invalid file type. Allowed: {string.Join(", ", _allowedExtensions)}");
                        continue;
                    }

                    // Generate unique filename
                    var uniqueFileName = $"product-{Guid.NewGuid():N}{extension}";
                    var filePath = Path.Combine(imagesFolder, uniqueFileName);

                    // Save the file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Return relative URL path for frontend use
                    var imageUrl = $"/images/{uniqueFileName}";
                    uploadedUrls.Add(imageUrl);

                    _logger.LogInformation($"Image uploaded successfully: {uniqueFileName}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error uploading file: {file.FileName}");
                    errors.Add($"{file.FileName}: Upload failed");
                }
            }

            if (uploadedUrls.Count == 0)
            {
                return BadRequest(new { message = "No files were uploaded successfully", errors });
            }

            return Ok(new 
            { 
                message = $"{uploadedUrls.Count} image(s) uploaded successfully",
                urls = uploadedUrls,
                errors = errors.Count > 0 ? errors : null
            });
        }

        /// Upload single image
        [HttpPost("upload-single")]
        [Authorize(Roles = "ADMIN,Admin,admin")]
        public async Task<IActionResult> UploadSingleImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded" });

            // Validate file size
            if (file.Length > MaxFileSize)
                return BadRequest(new { message = "File exceeds 5MB limit" });

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                return BadRequest(new { message = $"Invalid file type. Allowed: {string.Join(", ", _allowedExtensions)}" });

            try
            {
                // Get the Vue project's public/images folder path
                var vueProjectPath = _config["ImageUpload:VueProjectPath"] 
                    ?? Path.Combine(Directory.GetParent(_env.ContentRootPath)?.FullName ?? "", "vue-project");
                var imagesFolder = Path.Combine(vueProjectPath, "public", "images");

                // Ensure directory exists
                if (!Directory.Exists(imagesFolder))
                {
                    Directory.CreateDirectory(imagesFolder);
                }

                // Generate unique filename
                var uniqueFileName = $"product-{Guid.NewGuid():N}{extension}";
                var filePath = Path.Combine(imagesFolder, uniqueFileName);

                // Save the file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var imageUrl = $"/images/{uniqueFileName}";
                
                _logger.LogInformation($"Image uploaded successfully: {uniqueFileName}");

                return Ok(new 
                { 
                    message = "Image uploaded successfully",
                    url = imageUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image");
                return StatusCode(500, new { message = "Failed to upload image" });
            }
        }

        /// Delete an image
        [HttpDelete("{fileName}")]
        [Authorize(Roles = "ADMIN,Admin,admin")]
        public IActionResult DeleteImage(string fileName)
        {
            try
            {
                var vueProjectPath = _config["ImageUpload:VueProjectPath"] 
                    ?? Path.Combine(Directory.GetParent(_env.ContentRootPath)?.FullName ?? "", "vue-project");
                var filePath = Path.Combine(vueProjectPath, "public", "images", fileName);

                if (!System.IO.File.Exists(filePath))
                    return NotFound(new { message = "Image not found" });

                System.IO.File.Delete(filePath);
                
                _logger.LogInformation($"Image deleted: {fileName}");
                
                return Ok(new { message = "Image deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting image: {fileName}");
                return StatusCode(500, new { message = "Failed to delete image" });
            }
        }
    }
}
