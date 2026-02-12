import axios from "axios"
import { useAuthStore } from "../stores/Auth"

const api = axios.create({
    baseURL: import.meta.env.VITE_API_BASE_URL || "https://localhost:5001/api",
    timeout: 30000, // Increased to 30 seconds
    headers: {
        "Content-Type": "application/json"
    },
    withCredentials: true
})

// REQUEST INTERCEPTOR
api.interceptors.request.use(
    (config) => {
        console.log('🚀 Making request to:', config.baseURL + config.url)
        console.log('🚀 Full URL:', config.url)
        console.log('🚀 Method:', config.method)

        const token = localStorage.getItem("accessToken") || sessionStorage.getItem("accessToken")
        if (token) {
            console.log('🔑 Adding token to request')
            config.headers.Authorization = `Bearer ${token}`
        } else {
            console.log('⚠️ No token found')
        }
        return config
    },
    (error) => {
        console.error('❌ Request setup error:', error)
        return Promise.reject(error)
    }
)

// RESPONSE INTERCEPTOR
api.interceptors.response.use(
    (response) => {
        return response
    },
    async (error) => {
        // Handle 401 Unauthorized - token expired or invalid
        if (error.response?.status === 401) {
            const originalRequest = error.config

            // Don't redirect for auth endpoints (login, register, refresh)
            if (!originalRequest.url.includes('/auth/')) {
                console.warn('⚠️ 401 Unauthorized - token may be expired. Please log in again.')
                // Clear stored auth data
                localStorage.removeItem('accessToken')
                localStorage.removeItem('user')
                sessionStorage.removeItem('accessToken')
                sessionStorage.removeItem('user')

                // Only redirect if not already on login page
                if (!window.location.pathname.includes('/login')) {
                    window.location.href = '/login'
                }
            }
        }

        return Promise.reject(error)
    }
)

export default api