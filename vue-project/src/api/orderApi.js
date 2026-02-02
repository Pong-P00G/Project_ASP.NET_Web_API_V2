import api from './api.js'

// Get or create a cart session ID (shared with cartApi)
const getCartSessionId = () => {
    let sessionId = localStorage.getItem('cartSessionId')
    if (!sessionId) {
        sessionId = crypto.randomUUID()
        localStorage.setItem('cartSessionId', sessionId)
    }
    return sessionId
}

export const orderApi = {
    // Create a new order from cart
    createOrder: (orderData) => api.post('/order', orderData, {
        headers: { 'X-Cart-Session': getCartSessionId() }
    }),
    
    // Get all orders for the current user
    getOrders: () => api.get('/order'),
    
    // Get a specific order by ID
    getOrderById: (orderId) => api.get(`/order/${orderId}`),
    
    // Update order status (admin only)
    updateOrderStatus: (orderId, status) => api.put(`/order/${orderId}/status`, { status }),

    // Admin: Get all orders
    getAllOrdersForAdmin: () => api.get('/order/admin/all'),

    // Admin: Get order details
    getOrderByIdForAdmin: (orderId) => api.get(`/order/admin/${orderId}`)
}
