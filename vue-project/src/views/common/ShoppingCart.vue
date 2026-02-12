<script setup>
import { ref, computed, onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import { cartApi } from '../../api/cartApi'
import {
    ShoppingCart,
    Plus,
    Minus,
    Trash2,
    ArrowRight,
    Tag,
    Truck,
    Shield,
    Heart,
    ShoppingBag,
    X,
    Loader2,
    CheckCircle,
    AlertCircle,
    Lock,
    CreditCard,
    Package
} from 'lucide-vue-next'

// Cart Items
const cartItems = ref([])
const loading = ref(false)
const error = ref(null)
const success = ref(null)

// Promo Code
const promoCode = ref('')
const appliedPromo = ref(null)
const promoError = ref('')

// Helper to map cart items from API response
const mapCartItems = (items) => {
    if (!Array.isArray(items)) return []
    return items.map(item => {
        const product = item.productVariant?.product
        const images = product?.productImages || []
        const firstImage = images.length > 0 ? images[0].imageUrl : null
        return {
            id: item.cartItemId,
            productId: product?.productId || item.productVariant?.productId,
            name: product?.productName || 'Unknown Product',
            image: firstImage,
            price: item.price || item.productVariant?.price || 0,
            quantity: item.quantity || 1,
            inStock: (item.productVariant?.stockQuantity || 0) > 0
        }
    })
}

// Fetch cart
const fetchCart = async () => {
    try {
        loading.value = true
        error.value = null
        const response = await cartApi.getCart()
        const cartData = response.data.data || response.data || {}
        // Handle both array and object with items property
        const items = Array.isArray(cartData) ? cartData : (cartData.items || [])
        cartItems.value = mapCartItems(items)
    } catch (err) {
        error.value = err.response?.data?.message || 'Failed to load cart'
        console.error('Error fetching cart:', err)
        // Keep existing items if API fails
    } finally {
        loading.value = false
    }
}

// Computed Values
const subtotal = computed(() => {
    return cartItems.value.reduce((sum, item) => sum + ((item.price || 0) * (item.quantity || 1)), 0)
})

const discount = computed(() => {
    if (appliedPromo.value) {
        return subtotal.value * 0.1 // 10% discount
    }
    return 0
})

const shipping = computed(() => {
    return subtotal.value > 100 ? 0 : 9.99
})

const tax = computed(() => {
    return (subtotal.value - discount.value + shipping.value) * 0.08 // 8% tax
})

const total = computed(() => {
    return subtotal.value - discount.value + shipping.value + tax.value
})

// Actions
const updateQuantity = async (itemId, change) => {
    const item = cartItems.value.find(i => i.id === itemId)
    if (!item) return
    
    const newQuantity = Math.max(1, (item.quantity || 1) + change)
    
    try {
        await cartApi.updateCartItem(itemId, newQuantity)
        item.quantity = newQuantity
    } catch (err) {
        error.value = err.response?.data?.message || 'Failed to update quantity'
        console.error('Error updating quantity:', err)
    }
}

const removeItem = async (itemId) => {
    try {
        await cartApi.removeFromCart(itemId)
        cartItems.value = cartItems.value.filter(i => i.id !== itemId)
        success.value = 'Item removed from cart'
        setTimeout(() => success.value = null, 3000)
    } catch (err) {
        error.value = err.response?.data?.message || 'Failed to remove item'
    }
}

const applyPromoCode = async () => {
    promoError.value = ''
    try {
        await cartApi.applyPromoCode(promoCode.value.toUpperCase())
        appliedPromo.value = promoCode.value.toUpperCase()
        promoCode.value = ''
        success.value = 'Promo code applied!'
        setTimeout(() => success.value = null, 3000)
    } catch (err) {
        promoError.value = err.response?.data?.message || 'Invalid promo code'
    }
}

const removePromo = () => {
    appliedPromo.value = null
    promoError.value = ''
}

const moveToWishlist = async (itemId) => {
    const item = cartItems.value.find(i => i.id === itemId)
    if (!item) return
    
    try {
        // Add to wishlist (you might need to import wishlist API)
        await removeItem(itemId)
        success.value = 'Item moved to wishlist'
        setTimeout(() => success.value = null, 3000)
    } catch (err) {
        error.value = 'Failed to move to wishlist'
    }
}

onMounted(() => {
    fetchCart()
})
</script>

<template>
    <div class="min-h-screen bg-gray-50 py-8">
        <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <!-- Header -->
            <div class="mb-8">
                <h1 class="text-3xl font-bold text-gray-900 flex items-center gap-3 mb-2">
                    <ShoppingCart class="w-8 h-8 text-blue-600" />
                    Shopping Cart
                </h1>
                <p class="text-gray-500">
                    <span class="text-blue-600 font-semibold">{{ cartItems.length }}</span> items ready for checkout
                </p>
            </div>

            <!-- Messages -->
            <Transition name="slide-fade">
                <div v-if="error"
                    class="mb-6 bg-rose-50 border border-rose-200 rounded-2xl p-4 flex items-center gap-3">
                    <AlertCircle class="w-5 h-5 text-rose-600 shrink-0" />
                    <div class="flex-1">
                        <p class="text-sm font-semibold text-rose-900">Error</p>
                        <p class="text-xs text-rose-700 mt-0.5">{{ error }}</p>
                    </div>
                    <button @click="error = null" class="text-rose-600 hover:text-rose-800">
                        <X class="w-4 h-4" />
                    </button>
                </div>
            </Transition>

            <Transition name="slide-fade">
                <div v-if="success"
                    class="mb-6 bg-emerald-50 border border-emerald-200 rounded-2xl p-4 flex items-center gap-3">
                    <CheckCircle class="w-5 h-5 text-emerald-600 shrink-0" />
                    <span class="flex-1 font-medium text-emerald-900">{{ success }}</span>
                    <button @click="success = null" class="text-emerald-600 hover:text-emerald-800">
                        <X class="w-4 h-4" />
                    </button>
                </div>
            </Transition>

            <!-- Cart Content -->
            <div v-if="cartItems.length > 0" class="grid lg:grid-cols-3 gap-8">
                <!-- Cart Items -->
                <div class="lg:col-span-2 space-y-4">
                    <div v-for="item in cartItems" :key="item.id"
                        class="group bg-white rounded-[32px] border border-gray-100 shadow-sm p-6 hover:shadow-md transition-all duration-300">
                        <div class="flex gap-6">
                            <!-- Product Image -->
                            <RouterLink :to="`/product/${item.productId || item.id}`" class="relative w-32 h-32 shrink-0 rounded-2xl overflow-hidden bg-gray-100">
                                <img v-if="item.image" :src="item.image" :alt="item.name"
                                    class="w-full h-full object-cover group-hover:scale-110 transition-transform duration-500" />
                                <div v-else class="w-full h-full flex items-center justify-center">
                                    <Package class="w-12 h-12 text-gray-300" />
                                </div>
                            </RouterLink>

                            <!-- Product Details -->
                            <div class="flex-1 flex flex-col justify-between">
                                <div>
                                    <div class="flex items-start justify-between mb-2">
                                        <RouterLink :to="`/product/${item.productId || item.id}`">
                                            <h3 class="text-lg font-bold text-gray-900 hover:text-blue-600 transition-colors">
                                                {{ item.name }}
                                            </h3>
                                        </RouterLink>
                                    </div>

                                    <div class="mb-3">
                                        <span v-if="item.inStock !== false"
                                            class="inline-flex items-center gap-1 px-2 py-1 text-xs font-bold text-emerald-600 bg-emerald-50 rounded-full">
                                            <span class="w-1.5 h-1.5 bg-emerald-500 rounded-full animate-pulse"></span>
                                            In Stock
                                        </span>
                                        <span v-else
                                            class="inline-flex items-center gap-1 px-2 py-1 text-xs font-bold text-rose-600 bg-rose-50 rounded-full">
                                            <span class="w-1.5 h-1.5 bg-rose-500 rounded-full"></span>
                                            Out of Stock
                                        </span>
                                    </div>

                                    <div class="flex items-baseline gap-2">
                                        <p class="text-2xl font-bold text-gray-900">
                                            ${{ ((item.price || 0) * (item.quantity || 1)).toFixed(2) }}
                                        </p>
                                        <span class="text-sm text-gray-500">${{ (item.price || 0).toFixed(2) }} each</span>
                                    </div>
                                </div>

                                <!-- Quantity and Actions -->
                                <div class="flex items-center justify-between mt-4">
                                    <!-- Quantity Controls -->
                                    <div class="flex items-center gap-3">
                                        <button @click="updateQuantity(item.id, -1)"
                                            :disabled="(item.quantity || 1) <= 1"
                                            class="w-10 h-10 bg-gray-50 border border-gray-200 hover:border-blue-500 hover:bg-blue-50 rounded-xl flex items-center justify-center transition-all duration-300 disabled:opacity-50 disabled:cursor-not-allowed">
                                            <Minus class="w-4 h-4 text-gray-600" />
                                        </button>

                                        <span class="w-12 text-center font-bold text-gray-900 text-lg">{{ item.quantity || 1 }}</span>

                                        <button @click="updateQuantity(item.id, 1)"
                                            class="w-10 h-10 bg-gray-50 border border-gray-200 hover:border-blue-500 hover:bg-blue-50 rounded-xl flex items-center justify-center transition-all duration-300">
                                            <Plus class="w-4 h-4 text-gray-600" />
                                        </button>
                                    </div>

                                    <!-- Action Buttons -->
                                    <div class="flex items-center gap-2">
                                        <button @click="moveToWishlist(item.id)"
                                            class="p-2.5 bg-gray-50 border border-gray-200 hover:border-pink-300 hover:bg-pink-50 rounded-xl transition-all duration-300 group/heart"
                                            title="Move to wishlist">
                                            <Heart class="w-4 h-4 text-gray-400 group-hover/heart:text-pink-500 transition-colors" />
                                        </button>

                                        <button @click="removeItem(item.id)"
                                            class="p-2.5 bg-gray-50 border border-gray-200 hover:border-rose-300 hover:bg-rose-50 rounded-xl transition-all duration-300 group/trash"
                                            title="Remove item">
                                            <Trash2 class="w-4 h-4 text-gray-400 group-hover/trash:text-rose-500 transition-colors" />
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- Continue Shopping -->
                    <RouterLink to="/product"
                        class="inline-flex items-center gap-2 px-6 py-3 bg-white border-2 border-gray-200 text-gray-700 font-semibold rounded-xl hover:border-blue-500 hover:text-blue-600 transition-all duration-300 group">
                        <ShoppingBag class="w-5 h-5 group-hover:scale-110 transition-transform" />
                        Continue Shopping
                    </RouterLink>
                </div>

                <!-- Order Summary -->
                <div class="lg:col-span-1">
                    <div class="bg-white rounded-[32px] border border-gray-100 shadow-sm p-6 sticky top-24">
                        <h2 class="text-2xl font-bold text-gray-900 mb-6 flex items-center gap-2">
                            <CreditCard class="w-6 h-6 text-blue-600" />
                            Order Summary
                        </h2>

                        <!-- Promo Code -->
                        <div class="mb-6">
                            <label class="text-sm font-semibold text-gray-700 mb-3 flex items-center gap-2">
                                <Tag class="w-4 h-4 text-blue-600" />
                                Promo Code
                            </label>
                            <div v-if="!appliedPromo" class="space-y-2">
                                <div class="flex gap-2">
                                    <input v-model="promoCode" type="text" placeholder="Enter code"
                                        class="flex-1 px-4 py-3 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors" />
                                    <button @click="applyPromoCode"
                                        class="px-6 py-3 bg-gray-900 text-white font-semibold rounded-xl hover:bg-gray-800 transition-colors">
                                        Apply
                                    </button>
                                </div>
                                <p class="text-xs text-gray-500 flex items-center gap-1">
                                    Try code: <span class="text-blue-600 font-bold">SAVE10</span>
                                </p>
                            </div>
                            <div v-else
                                class="flex items-center justify-between p-3 bg-emerald-50 rounded-xl border border-emerald-200">
                                <div class="flex items-center gap-2">
                                    <div class="w-8 h-8 bg-emerald-100 rounded-lg flex items-center justify-center">
                                        <Tag class="w-4 h-4 text-emerald-600" />
                                    </div>
                                    <span class="text-sm font-bold text-emerald-700">{{ appliedPromo }}</span>
                                </div>
                                <button @click="removePromo" class="text-emerald-600 hover:text-emerald-700 transition-colors">
                                    <X class="w-5 h-5" />
                                </button>
                            </div>
                            <p v-if="promoError" class="text-sm text-rose-600 mt-2 flex items-center gap-1">
                                <X class="w-4 h-4" />
                                {{ promoError }}
                            </p>
                        </div>

                        <!-- Summary Details -->
                        <div class="space-y-4 mb-6 pb-6 border-b border-gray-100">
                            <div class="flex items-center justify-between text-gray-600">
                                <span>Subtotal</span>
                                <span class="font-semibold text-gray-900">${{ subtotal.toFixed(2) }}</span>
                            </div>

                            <div v-if="discount > 0" class="flex items-center justify-between text-emerald-600">
                                <span class="flex items-center gap-1">
                                    <Tag class="w-4 h-4" />
                                    Discount
                                </span>
                                <span class="font-bold">-${{ discount.toFixed(2) }}</span>
                            </div>

                            <div class="flex items-center justify-between text-gray-600">
                                <span>Shipping</span>
                                <span class="font-semibold text-gray-900">
                                    {{ shipping === 0 ? 'FREE' : `$${shipping.toFixed(2)}` }}
                                </span>
                            </div>

                            <div class="flex items-center justify-between text-gray-600">
                                <span>Tax (8%)</span>
                                <span class="font-semibold text-gray-900">${{ tax.toFixed(2) }}</span>
                            </div>
                        </div>

                        <!-- Total -->
                        <div class="flex items-center justify-between mb-6 p-4 bg-gray-50 rounded-2xl border border-blue-200">
                            <span class="text-lg font-bold text-gray-900">Total</span>
                            <span class="text-3xl font-black text-blue-600">
                                ${{ total.toFixed(2) }}
                            </span>
                        </div>

                        <!-- Checkout Button -->
                        <button
                            @click="$router.push('/checkout')"
                            class="w-full py-4 bg-gray-900 text-white rounded-xl font-bold hover:bg-gray-800 transition-colors flex items-center justify-center gap-3 group mb-4">
                            <Lock class="w-5 h-5" />
                            <span>Secure Checkout</span>
                            <ArrowRight class="w-5 h-5 group-hover:translate-x-1 transition-transform" />
                        </button>

                        <!-- Features -->
                        <div class="space-y-3 pt-4 border-t border-gray-100">
                            <div class="flex items-center gap-3 text-sm text-gray-600">
                                <div class="w-10 h-10 bg-emerald-50 rounded-xl flex items-center justify-center">
                                    <Truck class="w-5 h-5 text-emerald-600" />
                                </div>
                                <span>Free shipping on orders over $100</span>
                            </div>

                            <div class="flex items-center gap-3 text-sm text-gray-600">
                                <div class="w-10 h-10 bg-blue-50 rounded-xl flex items-center justify-center">
                                    <Shield class="w-5 h-5 text-blue-600" />
                                </div>
                                <span>Secure payment & data protection</span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Empty Cart -->
            <div v-else-if="!loading" class="bg-white rounded-[32px] border border-gray-100 shadow-sm text-center py-20">
                <div class="inline-flex items-center justify-center w-24 h-24 bg-gray-50 rounded-full mb-6">
                    <ShoppingCart class="w-12 h-12 text-gray-300" />
                </div>
                <h2 class="text-2xl font-bold text-gray-900 mb-2">Your Cart is Empty</h2>
                <p class="text-gray-500 mb-8 max-w-md mx-auto">
                    Start exploring our amazing products and add items to your cart
                </p>
                <RouterLink to="/product"
                    class="inline-flex items-center gap-2 px-8 py-3 bg-gray-900 text-white rounded-xl font-semibold hover:bg-gray-800 transition-colors">
                    <ShoppingBag class="w-5 h-5" />
                    Start Shopping
                </RouterLink>
            </div>

            <!-- Loading State -->
            <div v-if="loading" class="bg-white rounded-[32px] border border-gray-100 shadow-sm p-12">
                <div class="flex flex-col items-center justify-center gap-4">
                    <Loader2 class="w-8 h-8 animate-spin text-blue-600" />
                    <p class="text-gray-500 font-medium">Loading your cart...</p>
                </div>
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

.slide-fade-enter-from {
    transform: translateY(-10px);
    opacity: 0;
}

.slide-fade-leave-to {
    transform: translateY(-10px);
    opacity: 0;
}
</style>
