<script setup>
import { onMounted, computed } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import { useWishlistStore } from '../../stores/Wishlist.js'
import { cartApi } from '../../api/cartApi'
import {
    Heart,
    ShoppingCart,
    Trash2,
    AlertCircle,
    X,
    Eye,
    Package,
    CheckCircle,
    TrendingUp,
    ArrowRight
} from 'lucide-vue-next'
import { ref } from 'vue'

const wishlistStore = useWishlistStore()
const router = useRouter()

// Local UI state
const success = ref(null)
const localError = ref(null)
const removingItems = ref(new Set())
const addingToCart = ref(new Set())

// Use store's reactive data
const wishlistItems = computed(() => wishlistStore.wishlistItems)
const loading = computed(() => wishlistStore.loading)
const totalValue = computed(() => wishlistStore.totalValue)
const inStockCount = computed(() => wishlistStore.inStockCount)

// Remove item
const removeItem = async (productId) => {
    removingItems.value.add(productId)
    try {
        const result = await wishlistStore.removeFromWishlist(productId)
        if (result.success) {
            success.value = 'Item removed from wishlist'
            setTimeout(() => success.value = null, 3000)
        } else {
            localError.value = result.message || 'Failed to remove item'
        }
    } catch (err) {
        localError.value = 'Failed to remove item'
    } finally {
        removingItems.value.delete(productId)
    }
}

// Add single item to cart
const addToCart = async (item) => {
    const productId = item.productId || item.id
    addingToCart.value.add(productId)
    try {
        await cartApi.addToCart(productId, 1)
        success.value = 'Item added to cart!'
        setTimeout(() => success.value = null, 3000)
    } catch (err) {
        localError.value = err.response?.data?.message || 'Failed to add to cart'
    } finally {
        addingToCart.value.delete(productId)
    }
}

// Move all to cart
const moveAllToCart = async () => {
    if (!wishlistItems.value.length) return
    try {
        const inStockItems = wishlistItems.value.filter(item => item.inStock !== false)
        if (inStockItems.length === 0) {
            localError.value = 'No items in stock to add to cart'
            return
        }
        const promises = inStockItems.map(item =>
            cartApi.addToCart(item.productId || item.id, 1)
        )
        await Promise.all(promises)
        success.value = `${inStockItems.length} item(s) added to cart!`
        setTimeout(() => success.value = null, 3000)
    } catch (err) {
        localError.value = 'Failed to add items to cart'
    }
}

// Clear wishlist
const clearWishlist = async () => {
    if (!confirm('Are you sure you want to clear your entire wishlist?')) return
    const result = await wishlistStore.clearWishlist()
    if (result.success) {
        success.value = 'Wishlist cleared!'
        setTimeout(() => success.value = null, 3000)
    }
}

const getProductId = (item) => item.productId || item.id

onMounted(() => {
    wishlistStore.fetchWishlist()
})
</script>

<template>
    <div class="min-h-screen bg-white">
        <!-- Hero Header -->
        <div class="bg-black py-16 md:py-20">
            <div class="max-w-screen-xl mx-auto px-6 sm:px-8 lg:px-12 text-center">
                <p class="text-[10px] font-medium tracking-[0.35em] uppercase mb-4" style="color: #C8A97E;">— My
                    Collection</p>
                <h1 class="text-3xl md:text-5xl font-light text-white tracking-[-0.01em] mb-3">Wishlist</h1>
                <p class="text-white/40 text-sm">
                    {{ wishlistItems.length }} item{{ wishlistItems.length !== 1 ? 's' : '' }}
                    <span v-if="wishlistItems.length > 0"> • ${{ totalValue.toFixed(2) }} total value</span>
                </p>
            </div>
        </div>

        <div class="max-w-screen-xl mx-auto px-6 sm:px-8 lg:px-12 py-12">
            <!-- Messages -->
            <Transition name="slide-fade">
                <div v-if="localError" class="mb-8 bg-red-50 border border-red-100 p-4 flex items-center gap-3">
                    <AlertCircle class="w-4 h-4 text-red-500 shrink-0" />
                    <span class="flex-1 text-sm text-red-700">{{ localError }}</span>
                    <button @click="localError = null" class="text-red-400 hover:text-red-600">
                        <X class="w-4 h-4" />
                    </button>
                </div>
            </Transition>

            <Transition name="slide-fade">
                <div v-if="success" class="mb-8 bg-emerald-50 border border-emerald-100 p-4 flex items-center gap-3">
                    <CheckCircle class="w-4 h-4 text-emerald-500 shrink-0" />
                    <span class="flex-1 text-sm font-medium text-emerald-700">{{ success }}</span>
                    <button @click="success = null" class="text-emerald-400 hover:text-emerald-600">
                        <X class="w-4 h-4" />
                    </button>
                </div>
            </Transition>

            <!-- Actions Bar -->
            <div v-if="wishlistItems.length > 0 && !loading"
                class="flex flex-wrap items-center justify-between gap-4 mb-10 pb-8 border-b border-black/5">
                <div class="flex items-center gap-6 text-xs text-black/30">
                    <span class="flex items-center gap-2">
                        <Package class="w-3.5 h-3.5" />
                        {{ wishlistItems.length }} items
                    </span>
                    <span class="flex items-center gap-2">
                        <CheckCircle class="w-3.5 h-3.5" />
                        {{ inStockCount }} in stock
                    </span>
                    <span class="flex items-center gap-2">
                        <TrendingUp class="w-3.5 h-3.5" />
                        ${{ totalValue.toFixed(2) }}
                    </span>
                </div>
                <div class="flex gap-3">
                    <button @click="moveAllToCart" :disabled="inStockCount === 0"
                        class="px-6 py-3 text-xs font-medium tracking-[0.1em] uppercase text-white disabled:opacity-30 disabled:cursor-not-allowed hover:opacity-90 transition-all flex items-center gap-2"
                        style="background-color: #C8A97E;">
                        <ShoppingCart class="w-3.5 h-3.5" />
                        Add All to Cart
                    </button>
                    <button @click="clearWishlist"
                        class="px-6 py-3 border border-black/10 text-xs font-medium tracking-[0.1em] uppercase text-black/40 hover:border-red-300 hover:text-red-500 transition-all flex items-center gap-2">
                        <Trash2 class="w-3.5 h-3.5" />
                        Clear All
                    </button>
                </div>
            </div>

            <!-- Loading State -->
            <div v-if="loading" class="flex flex-col items-center justify-center py-24">
                <div class="w-8 h-8 border-2 border-black/10 border-t-black rounded-full animate-spin mb-4"></div>
                <p class="text-black/30 text-sm">Loading your wishlist...</p>
            </div>

            <!-- Empty State -->
            <div v-else-if="wishlistItems.length === 0" class="text-center py-24">
                <div class="w-20 h-20 mx-auto mb-6 border border-black/5 flex items-center justify-center">
                    <Heart class="w-8 h-8 text-black/15" />
                </div>
                <h3 class="text-xl font-light text-black mb-2">Your wishlist is empty</h3>
                <p class="text-sm text-black/30 mb-8 max-w-sm mx-auto">
                    Start adding items you love to build your perfect collection.
                </p>
                <RouterLink to="/product"
                    class="inline-flex items-center gap-2 px-8 py-4 text-xs font-medium tracking-[0.15em] uppercase text-white hover:opacity-90 transition-all"
                    style="background-color: #C8A97E;">
                    <Eye class="w-4 h-4" />
                    Browse Products
                </RouterLink>
            </div>

            <!-- Wishlist Grid -->
            <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-5">
                <div v-for="item in wishlistItems" :key="getProductId(item)" class="group">
                    <!-- Image -->
                    <RouterLink :to="`/product/${getProductId(item)}`"
                        class="block relative overflow-hidden aspect-[4/5] bg-neutral-50 mb-4">
                        <img v-if="item.image" :src="item.image" :alt="item.name"
                            class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-700" />
                        <div v-else class="w-full h-full flex items-center justify-center">
                            <Package class="w-12 h-12 text-black/10" />
                        </div>

                        <!-- Out of Stock Overlay -->
                        <div v-if="item.inStock === false || item.stock === 0"
                            class="absolute inset-0 bg-white/80 flex items-center justify-center">
                            <span class="text-[10px] font-medium tracking-[0.2em] uppercase text-black/40">Out
                                of Stock</span>
                        </div>

                        <!-- Remove Button -->
                        <button @click.stop.prevent="removeItem(getProductId(item))"
                            :disabled="removingItems.has(getProductId(item))"
                            class="absolute top-3 right-3 w-9 h-9 bg-white/90 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-all duration-300 hover:bg-red-50 disabled:opacity-50"
                            aria-label="Remove from wishlist">
                            <Trash2 class="w-3.5 h-3.5 text-black/30 hover:text-red-500" />
                        </button>

                        <!-- Border -->
                        <div class="absolute inset-0 border border-black/5 pointer-events-none"></div>
                    </RouterLink>

                    <!-- Info -->
                    <div class="space-y-2">
                        <span v-if="item.category"
                            class="text-[10px] text-black/30 font-medium tracking-[0.2em] uppercase">
                            {{ item.category }}
                        </span>
                        <RouterLink :to="`/product/${getProductId(item)}`">
                            <h3
                                class="text-sm font-medium text-black line-clamp-1 group-hover:underline underline-offset-4 transition-all">
                                {{ item.name || 'Product' }}
                            </h3>
                        </RouterLink>
                        <p class="text-sm font-medium text-black">${{ (item.price || 0).toFixed(2) }}</p>

                        <!-- Add to Cart -->
                        <button @click="addToCart(item)"
                            :disabled="item.inStock === false || addingToCart.has(getProductId(item))"
                            class="w-full mt-2 py-3 bg-black text-white text-xs font-medium tracking-[0.12em] uppercase hover:bg-neutral-800 disabled:opacity-30 disabled:cursor-not-allowed transition-all flex items-center justify-center gap-2">
                            <ShoppingCart class="w-3.5 h-3.5" />
                            {{ addingToCart.has(getProductId(item)) ? 'Adding...' : 'Add to Cart' }}
                        </button>
                    </div>
                </div>
            </div>

            <!-- Bottom CTA -->
            <div v-if="wishlistItems.length > 0 && !loading" class="text-center mt-16 pt-12 border-t border-black/5">
                <RouterLink to="/product"
                    class="inline-flex items-center gap-2 text-xs font-medium tracking-[0.15em] uppercase transition-colors duration-500 group"
                    style="color: #C8A97E;">
                    Continue Shopping
                    <ArrowRight class="w-3.5 h-3.5 group-hover:translate-x-1 transition-transform duration-300" />
                </RouterLink>
            </div>
        </div>
    </div>
</template>

<style scoped>
.slide-fade-enter-active {
    transition: all 0.3s ease-out;
}

.slide-fade-leave-active {
    transition: all 0.2s ease-in;
}

.slide-fade-enter-from,
.slide-fade-leave-to {
    transform: translateY(-10px);
    opacity: 0;
}
</style>
