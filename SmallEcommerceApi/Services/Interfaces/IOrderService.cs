using SmallEcommerceApi.DTOs.Orders;
using SmallEcommerceApi.Models.Orders;

namespace SmallEcommerceApi.Services.Interfaces
{
    public interface IOrderService
    {
        Task<OrderResponseDto> CreateOrderAsync(int userId, string? sessionId, CreateOrderDto dto);
        Task<List<OrderResponseDto>> GetOrdersAsync(int userId);
        Task<OrderResponseDto?> GetOrderByIdAsync(int userId, int orderId);
        Task<bool> UpdateOrderStatusAsync(int orderId, string status);
        Task<List<OrderResponseDto>> GetAllOrdersAsync();
        Task<OrderResponseDto?> GetOrderByIdForAdminAsync(int orderId);
    }
}
