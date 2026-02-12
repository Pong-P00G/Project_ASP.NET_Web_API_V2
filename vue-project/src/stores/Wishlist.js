import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { profileApi } from '../api/profileApi';
import { useAuthStore } from './Auth';

export const useWishlistStore = defineStore('wishlist', () => {
    // State
    const wishlistItems = ref([]);
    const loading = ref(false);
    const error = ref(null);

    // Getters
    const wishlistCount = computed(() => wishlistItems.value.length);

    const totalValue = computed(() => {
        return wishlistItems.value.reduce((sum, item) => sum + (item.price || 0), 0);
    });

    const inStockCount = computed(() => {
        return wishlistItems.value.filter(item => item.inStock !== false).length;
    });

    const isInWishlist = computed(() => {
        return (productId) => {
            return wishlistItems.value.some(
                item => item.id === productId || item.productId === productId
            );
        };
    });

    // ---- API-based actions (used when logged in) ----

    // Fetch wishlist from backend API
    async function fetchWishlist() {
        const authStore = useAuthStore();
        if (!authStore.isAuthenticated) {
            loadLocalWishlist();
            return;
        }

        try {
            loading.value = true;
            error.value = null;
            const response = await profileApi.getWishlist();
            const data = response.data?.data || response.data || [];
            wishlistItems.value = Array.isArray(data) ? data : [];
        } catch (err) {
            console.error('Error fetching wishlist:', err);
            // Fallback to localStorage if API fails
            loadLocalWishlist();
        } finally {
            loading.value = false;
        }
    }

    // Normalize product data from any source (ProductDetail vs ProductsPage)
    function normalizeProduct(product) {
        const productId = product.id || product.productId;
        // Extract image: prefer string field, fallback to images array
        const image = product.image
            || product.images?.[0]?.imageUrl
            || product.images?.[0]
            || null;
        // Extract category: prefer string field, fallback to categories array
        const category = product.category
            || product.categories?.[0]?.categoryName
            || product.categories?.[0]
            || null;

        return {
            id: productId,
            productId: productId,
            name: product.name || product.productName,
            price: product.price || product.basePrice || 0,
            image,
            category,
            inStock: product.inStock !== false,
            stock: product.stock
        };
    }

    // Add item to wishlist
    async function addToWishlist(product) {
        const normalized = normalizeProduct(product);

        // Already in wishlist?
        if (isInWishlist.value(normalized.productId)) {
            return { success: false, message: 'Already in wishlist' };
        }

        const authStore = useAuthStore();
        if (authStore.isAuthenticated) {
            try {
                await profileApi.addToWishlist(normalized.productId);
                // Add to local state immediately for fast UI
                wishlistItems.value.push(normalized);
                return { success: true };
            } catch (err) {
                console.error('Failed to add to wishlist:', err);
                error.value = err.response?.data?.message || 'Failed to add to wishlist';
                return { success: false, message: error.value };
            }
        } else {
            // Guest mode: use localStorage
            wishlistItems.value.push(normalized);
            saveLocalWishlist();
            return { success: true };
        }
    }

    // Remove item from wishlist
    async function removeFromWishlist(productId) {
        const authStore = useAuthStore();
        if (authStore.isAuthenticated) {
            try {
                await profileApi.removeFromWishlist(productId);
                wishlistItems.value = wishlistItems.value.filter(
                    item => item.id !== productId && item.productId !== productId
                );
                return { success: true };
            } catch (err) {
                console.error('Failed to remove from wishlist:', err);
                error.value = err.response?.data?.message || 'Failed to remove item';
                return { success: false, message: error.value };
            }
        } else {
            wishlistItems.value = wishlistItems.value.filter(
                item => item.id !== productId && item.productId !== productId
            );
            saveLocalWishlist();
            return { success: true };
        }
    }

    // Toggle wishlist
    async function toggleWishlist(product) {
        const productId = product.id || product.productId;
        if (isInWishlist.value(productId)) {
            return await removeFromWishlist(productId);
        } else {
            return await addToWishlist(product);
        }
    }

    // Clear entire wishlist
    async function clearWishlist() {
        const authStore = useAuthStore();
        if (authStore.isAuthenticated) {
            try {
                const promises = wishlistItems.value.map(item =>
                    profileApi.removeFromWishlist(item.productId || item.id)
                );
                await Promise.all(promises);
                wishlistItems.value = [];
                return { success: true };
            } catch (err) {
                console.error('Failed to clear wishlist:', err);
                error.value = 'Failed to clear wishlist';
                return { success: false };
            }
        } else {
            wishlistItems.value = [];
            saveLocalWishlist();
            return { success: true };
        }
    }

    // ---- localStorage helpers (guest mode fallback) ----

    function loadLocalWishlist() {
        try {
            const saved = localStorage.getItem('wishlist');
            if (saved) {
                wishlistItems.value = JSON.parse(saved);
            }
        } catch (err) {
            console.error('Error loading wishlist from localStorage:', err);
            wishlistItems.value = [];
        }
    }

    function saveLocalWishlist() {
        try {
            localStorage.setItem('wishlist', JSON.stringify(wishlistItems.value));
        } catch (err) {
            console.error('Error saving wishlist to localStorage:', err);
        }
    }

    // Initialize: fetch from API if logged in, else from localStorage
    function initWishlist() {
        fetchWishlist();
    }

    function clearError() {
        error.value = null;
    }

    return {
        // State
        wishlistItems,
        loading,
        error,
        // Getters
        wishlistCount,
        totalValue,
        inStockCount,
        isInWishlist,
        // Actions
        fetchWishlist,
        addToWishlist,
        removeFromWishlist,
        toggleWishlist,
        clearWishlist,
        initWishlist,
        clearError
    };
});
