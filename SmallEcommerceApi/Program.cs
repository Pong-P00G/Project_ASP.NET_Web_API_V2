using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using SmallEcommerceApi.Db;
using SmallEcommerceApi.Models.Users;
using SmallEcommerceApi.Security.Api.Security;
using SmallEcommerceApi.Services;
using SmallEcommerceApi.Services.Interfaces;
using SmallEcommerceApi.Settings;
using System.Text;
using System.Text.Json;

namespace SmallEcommerceApi
{
    public partial class Program
    {
        private static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Database connection string
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection")));

            // Add controllers with JSON options
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                });

            // OpenAPI configuration
            builder.Services.AddOpenApi();

            // JWT Settings
            var jwtSettings = new JwtSettings();
            builder.Configuration.GetSection("JwtSettings").Bind(jwtSettings);
            builder.Services.AddSingleton(jwtSettings);

            // Service registration (Register interfaces with implementations)
            builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<IWishlistService, WishlistService>();
            builder.Services.AddScoped<IOrderService, OrderService>();

            // Backward compatibility: Also register concrete classes
            builder.Services.AddScoped<JwtService>();
            builder.Services.AddScoped<TokenService>();
            builder.Services.AddScoped<PasswordHasher>();

            // CORS configuration
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowVueApp", policy =>
                {
                    policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
                });
            });

            // JWT Authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings.SecretKey ?? string.Empty)),
                        ClockSkew = TimeSpan.Zero,
                        RoleClaimType = ClaimTypes.Role,
                        NameClaimType = ClaimTypes.Name
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                            logger.LogError($"Authentication Failed: {context.Exception.Message}");
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                            logger.LogInformation("Token Validated Successfully");
                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                            logger.LogWarning($"OnChallenge: {context.Error}, {context.ErrorDescription}");
                            return Task.CompletedTask;
                        }
                    };
                });

            // Authorization policies
            builder.Services.AddAuthorizationBuilder()
                .AddPolicy("AdminOnly", policy => policy.RequireClaim(ClaimTypes.Role, "ADMIN"))
                .AddPolicy("CustomerOrAdmin", policy => policy.RequireClaim(ClaimTypes.Role, "ADMIN", "CUSTOMER"));

            // Build the app
            var app = builder.Build();

            // Ensure database is created (Development only)
            if (app.Environment.IsDevelopment())
            {
                using (var scope = app.Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    try
                    {
                        dbContext.Database.EnsureCreated();

                        // Seed Roles if not exist
                        if (!dbContext.UserRoles.Any())
                        {
                            dbContext.UserRoles.AddRange(
                                new UserRole { RoleName = "ADMIN", Description = "Administrator role" },
                                new UserRole { RoleName = "CUSTOMER", Description = "Standard customer role" }
                            );
                            dbContext.SaveChanges();
                        }


                    }
                    catch (Exception ex)
                    {
                        // Log error but don't crash the app
                        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "An error occurred while creating the database.");
                    }
                }

                // Map OpenAPI endpoint
                app.MapOpenApi();
                
                // Map Scalar API documentation UI
                app.MapScalarApiReference(options =>
                {
                    options
                        .WithTitle("SmallEcommerce API")
                        .WithTheme(ScalarTheme.Purple)
                        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
                });
            }

            // Configure middleware pipeline (order matters!)
            
            // Serve static files (product images) from Vue project's public/images folder
            var vueImagesPath = Path.Combine(
                Directory.GetParent(app.Environment.ContentRootPath)?.FullName ?? "", 
                "vue-project", "public", "images");
            
            if (!Directory.Exists(vueImagesPath))
            {
                Directory.CreateDirectory(vueImagesPath);
            }
            
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(vueImagesPath),
                RequestPath = "/images"
            });

            app.UseCors("AllowVueApp");
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}