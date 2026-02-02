import api from './api'

export const authAPI = {

    async register(userData) {
        // Map role string to roleId (1=admin, 2=customer)
        let roleId = 2; // Default to customer
        if (userData.roleId === 'admin' || userData.roleId === 1) {
            roleId = 1;
        }
        
        const response = await api.post('/Auth/register', {
            username: userData.username || '',
            email: userData.email || '',
            password: userData.password || '',
            roleId: roleId,
            firstName: userData.firstName || userData.firstname || '',
            lastName: userData.lastName || userData.lastname || ''
        });
        return response;
    },

    async login(credentials) {
        // Backend expects UsernameOrEmail field
        const usernameOrEmail = credentials.email || credentials.username || credentials.emailOrUsername || '';
        
        const response = await api.post('/Auth/login', {
            usernameOrEmail: usernameOrEmail,
            password: credentials.password || ''
        });

        // Store token if login successful
        // NOTE: Side effects removed. Persistence is now handled by the Auth Store.
        /* 
        if (response.data) {
             // ... code moved to Auth Store ...
        }
        */

        return response;
    },

    // async logout() {
    //     localStorage.removeItem('accessToken');
    //     localStorage.removeItem('refreshToken');
    //     localStorage.removeItem('user');
    // },

    // async refreshToken(refreshToken) {
    //     return api.post('/Auth/refresh', { refreshToken });
    // },

    // async getCurrentUser() {
    //     return api.get('/Auth/me');
    // },
}