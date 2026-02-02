import api from "./api";

export const productAPI = {
    async getAllProducts(params = {}) {
        return await api.get("/products", { params });
    },

    async getProductsById(id) {
        return await api.get(`/products/${id}`);
    },

    async createProducts(payload) {
        return await api.post("/products", payload);
    },

    async updateProducts(id, payload) {
        return await api.put(`/products/${id}`, payload);
    },

    async softDelete(id) {
        return await api.delete(`/products/${id}/soft`);
    },

    async hardDelete(id) {
        return await api.delete(`/products/${id}/hard`);
    },

    async updateStock(id, quantity) {
        return await api.patch(`/products/${id}/stock`, { quantity });
    },

    // Upload single image
    async uploadImage(file) {
        const formData = new FormData();
        formData.append('file', file);
        return await api.post('/images/upload-single', formData, {
            headers: {
                'Content-Type': 'multipart/form-data'
            }
        });
    },

    // Upload multiple images
    async uploadImages(files) {
        const formData = new FormData();
        files.forEach(file => formData.append('files', file));
        return await api.post('/images/upload', formData, {
            headers: {
                'Content-Type': 'multipart/form-data'
            }
        });
    },

    // Delete an image
    async deleteImage(fileName) {
        return await api.delete(`/images/${fileName}`);
    }
};
