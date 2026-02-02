import { defineStore } from 'pinia'
import { authAPI } from '../api/authApi'


export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: JSON.parse(localStorage.getItem('user') || 'null'),
    token: localStorage.getItem('accessToken')
  }),

  getters: {
    isAuthenticated: (state) => !!state.token,
    userRole: (state) => state.user?.roleName || state.user?.role || null
  },

  actions: {
    async login(payload) {
      // Extract remember me flag, default to true if not provided (safe default)
      const remember = payload.remember !== undefined ? payload.remember : true
      
      const res = await authAPI.login(payload)
      
      // Handle different response structures
      const data = res.data || {}
      this.token = data.accessToken || data.token
      this.user = data.user
      
      if (this.token) {
        if (remember) {
          localStorage.setItem('accessToken', this.token)
        } else {
          sessionStorage.setItem('accessToken', this.token)
        }
      }
      
      if (this.user) {
        if (remember) {
          localStorage.setItem('user', JSON.stringify(this.user))
        } else {
          sessionStorage.setItem('user', JSON.stringify(this.user))
        }
      }
      
      return res
    },

    async register(payload) {
      const res = await authAPI.register(payload)
      return res
    },

    logout() {
      this.user = null
      this.token = null
      
      // Clear both storage types to be safe
      localStorage.removeItem('accessToken')
      localStorage.removeItem('refreshToken')
      localStorage.removeItem('user')
      
      sessionStorage.removeItem('accessToken')
      sessionStorage.removeItem('refreshToken')
      sessionStorage.removeItem('user')
    },

    // Initialize from localStorage OR sessionStorage on app start
    initializeAuth() {
      // Check localStorage first (persistent), then sessionStorage
      let token = localStorage.getItem('accessToken') || sessionStorage.getItem('accessToken')
      let user = localStorage.getItem('user') || sessionStorage.getItem('user')
      
      if (token) {
        this.token = token
      }
      
      if (user) {
        try {
          this.user = JSON.parse(user)
        } catch (e) {
          this.user = null
        }
      }
    }
  }
})