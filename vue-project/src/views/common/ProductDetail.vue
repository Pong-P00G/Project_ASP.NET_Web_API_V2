<script setup>
import { ref, onMounted, computed, watch } from 'vue'
import { useRoute, useRouter, RouterLink } from 'vue-router'
import { useWishlistStore } from '../../stores/Wishlist'
import { productAPI } from '../../api/productsApi'
import { cartApi } from '../../api/cartApi'
import {
    Star, Heart, Minus, Plus, ChevronDown, ChevronUp,
    ShoppingBag, Truck, RefreshCcw, ShieldCheck, ChevronRight
} from 'lucide-vue-next'

const route = useRoute()
const router = useRouter()
const wishlist = useWishlistStore()

// State
const product = ref(null)
const loading = ref(true)
const error = ref(null)
const selectedImage = ref(null)
const quantity = ref(1)
const addingToCart = ref(false)
const relatedProducts = ref([])

// Dynamic Variants
const variantGroups = ref({})
const selectedVariants = ref({})
const selectedVariantData = ref(null)

// Accordion
const activeAccordion = ref(null)
const toggleAccordion = (id) => {
    activeAccordion.value = activeAccordion.value === id ? null : id
}

const fetchProduct = async (id) => {
    try {
        loading.value = true
        error.value = null

        const response = await productAPI.getProductsById(id)
        const p = response.data

        if (!p) throw new Error('Product not found')

        product.value = {
            id: p.productId,
            name: p.productName,
            price: p.basePrice,
            description: p.description,
            images: p.images || [],
            image: p.images?.[0]?.imageUrl || null,
            inStock: p.stockStatus !== 'out-of-stock' && p.stock > 0,
            stock: p.stock,
            stockStatus: p.stockStatus,
            rating: 4.8,
            reviews: 128,
            featured: p.featured,
            sku: p.sku,
            supplier: p.supplier,
            categories: p.categories || [],
            category: p.categories?.[0]?.categoryName || null,
            rawVariants: p.variants || []
        }

        // Set image
        if (product.value.images.length > 0) {
            selectedImage.value = product.value.images.find(img => img.isPrimary) || product.value.images[0]
        }

        // Build variant groups
        const groups = {}
        if (p.variants?.length) {
            p.variants.forEach(v => {
                if (v.options) {
                    v.options.forEach(opt => {
                        if (!groups[opt.variant]) groups[opt.variant] = new Set()
                        groups[opt.variant].add(opt.value)
                    })
                }
            })
        }
        const built = {}
        Object.keys(groups).forEach(key => { built[key] = Array.from(groups[key]) })
        variantGroups.value = built

        const defaults = {}
        Object.keys(built).forEach(key => { defaults[key] = built[key][0] })
        selectedVariants.value = defaults
        updateSelectedVariant()

        fetchRelatedProducts(p.productId)
    } catch (err) {
        console.error('Error fetching product:', err)
        error.value = 'Failed to load product details'
    } finally {
        loading.value = false
    }
}

const updateSelectedVariant = () => {
    if (!product.value?.rawVariants?.length) return
    const match = product.value.rawVariants.find(v => {
        if (!v.options) return false
        return v.options.every(opt => selectedVariants.value[opt.variant] === opt.value)
    })
    selectedVariantData.value = match || null
}

const displayPrice = computed(() => {
    return selectedVariantData.value?.price || product.value?.price || 0
})

const displayDiscountPrice = computed(() => {
    return selectedVariantData.value?.discountPrice || null
})

const selectVariant = (group, value) => {
    selectedVariants.value = { ...selectedVariants.value, [group]: value }
    updateSelectedVariant()
}

const fetchRelatedProducts = async (currentId) => {
    try {
        const response = await productAPI.getAllProducts({ page: 1, pageSize: 5, isActive: true })
        const items = response.data?.items || response.data || []
        relatedProducts.value = items
            .filter(p => p.productId !== currentId)
            .slice(0, 4)
            .map(p => ({
                id: p.productId,
                name: p.productName,
                price: p.basePrice,
                image: p.images?.[0]?.imageUrl || 'https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=400&h=400&fit=crop',
                category: p.categories?.[0]?.categoryName || 'Product'
            }))
    } catch (err) {
        console.error('Failed to fetch related products:', err)
    }
}

const handleAddToCart = async () => {
    if (!product.value?.inStock) return
    try {
        addingToCart.value = true
        await cartApi.addToCart(product.value.id, quantity.value)
        router.push('/cart')
    } catch (err) {
        console.error('Add to cart failed', err)
        alert('Failed to add to cart')
    } finally {
        addingToCart.value = false
    }
}

const handleAddToWishlist = async () => {
    if (product.value) {
        if (wishlist.isInWishlist(product.value.id)) {
            await wishlist.removeFromWishlist(product.value.id)
        } else {
            await wishlist.addToWishlist(product.value)
        }
    }
}

watch(() => route.params.id, (newId) => {
    if (newId) fetchProduct(newId)
})

onMounted(() => {
    if (route.params.id) fetchProduct(route.params.id)
})
</script>

<template>
    <div class="min-h-screen bg-white font-sans text-gray-900 pb-20">
        <!-- Breadcrumbs -->
        <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 pt-6 pb-2">
            <nav class="flex items-center text-sm text-gray-400">
                <RouterLink to="/" class="hover:text-gray-900 transition-colors">Home</RouterLink>
                <ChevronRight class="w-3.5 h-3.5 mx-1.5" />
                <RouterLink to="/product" class="hover:text-gray-900 transition-colors">Shop</RouterLink>
                <ChevronRight class="w-3.5 h-3.5 mx-1.5" />
                <span class="font-semibold text-gray-900 truncate max-w-[200px]">{{ product?.name || 'Loading...'
                    }}</span>
            </nav>
        </div>

        <!-- Loading -->
        <div v-if="loading" class="flex justify-center py-32">
            <div class="w-10 h-10 border-[3px] border-gray-100 border-t-blue-600 rounded-full animate-spin"></div>
        </div>

        <!-- Error -->
        <div v-else-if="error" class="flex flex-col items-center justify-center py-32 text-gray-400">
            <p class="mb-4">{{ error }}</p>
            <button @click="fetchProduct(route.params.id)"
                class="px-6 py-2.5 bg-blue-600 text-white text-sm font-semibold rounded-lg hover:bg-blue-700 transition-colors">
                Try Again
            </button>
        </div>

        <div v-else-if="product" class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div class="grid grid-cols-1 lg:grid-cols-2 gap-10 lg:gap-16">

                <!-- Left: Images -->
                <div class="lg:sticky lg:top-24 lg:h-fit">
                    <!-- Main Image -->
                    <div
                        class="relative bg-gray-50 rounded-2xl overflow-hidden aspect-square flex items-center justify-center group">
                        <img v-if="selectedImage" :src="selectedImage.imageUrl" :alt="product.name"
                            class="w-full h-full object-contain p-8 group-hover:scale-105 transition-transform duration-500" />
                        <div v-else class="text-gray-400">No Image Available</div>
                        <span v-if="displayDiscountPrice"
                            class="absolute top-4 left-4 bg-gray-900 text-white text-[11px] font-bold uppercase tracking-wider px-3.5 py-1.5 rounded-full">
                            Sale
                        </span>
                    </div>
                    <!-- Thumbnails -->
                    <div class="grid grid-cols-4 gap-3 mt-4" v-if="product.images.length > 1">
                        <button v-for="img in product.images" :key="img.imageId" @click="selectedImage = img"
                            class="aspect-square rounded-xl overflow-hidden border-2 bg-gray-50 transition-all cursor-pointer"
                            :class="selectedImage?.imageId === img.imageId ? 'border-gray-900 opacity-100' : 'border-transparent opacity-50 hover:opacity-90'">
                            <img :src="img.imageUrl" class="w-full h-full object-contain p-2" />
                        </button>
                    </div>
                </div>

                <!-- Right: Info -->
                <div class="pt-2">
                    <!-- Meta -->
                    <div class="flex flex-wrap gap-2 mb-3">
                        <span v-if="product.categories.length"
                            class="text-[11px] font-semibold uppercase tracking-widest text-blue-600 bg-blue-50 px-3 py-1 rounded-md">
                            {{ product.categories[0].categoryName }}
                        </span>
                        <span v-if="product.sku"
                            class="text-[11px] font-medium text-gray-400 bg-gray-100 px-2.5 py-1 rounded-md">
                            SKU: {{ product.sku }}
                        </span>
                    </div>

                    <h1 class="text-3xl sm:text-4xl font-extrabold tracking-tight leading-tight mb-4">
                        {{ product.name }}
                    </h1>

                    <!-- Price & Rating -->
                    <div class="flex items-center justify-between flex-wrap gap-3 mb-3">
                        <div class="flex items-baseline gap-3">
                            <span class="text-3xl font-light text-gray-900">${{ displayPrice?.toFixed(2) }}</span>
                            <span v-if="displayDiscountPrice" class="text-lg font-bold text-green-600">
                                ${{ displayDiscountPrice?.toFixed(2) }}
                            </span>
                        </div>
                        <div
                            class="flex items-center gap-1.5 bg-gray-50 border border-gray-100 px-3.5 py-1.5 rounded-full text-sm font-semibold">
                            <Star class="w-4 h-4 fill-yellow-400 text-yellow-400" />
                            {{ product.rating }}
                            <span class="text-gray-400 font-normal">({{ product.reviews }})</span>
                        </div>
                    </div>

                    <!-- Stock -->
                    <div class="flex items-center gap-2 text-sm font-semibold mb-4"
                        :class="product.inStock ? 'text-green-600' : 'text-red-500'">
                        <span class="w-2 h-2 rounded-full"
                            :class="product.inStock ? 'bg-green-500' : 'bg-red-500'"></span>
                        {{ product.inStock ? `In Stock (${product.stock} available)` : 'Out of Stock' }}
                    </div>

                    <p class="text-gray-500 text-base leading-relaxed mb-7">{{ product.description }}</p>

                    <!-- Dynamic Variants -->
                    <div v-if="Object.keys(variantGroups).length > 0"
                        class="border-t border-gray-100 pt-6 mb-6 space-y-5">
                        <div v-for="(values, groupName) in variantGroups" :key="groupName">
                            <label
                                class="flex items-center gap-2 text-xs font-bold uppercase tracking-widest text-gray-900 mb-3">
                                {{ groupName }}
                                <span class="font-normal text-gray-400 normal-case tracking-normal">{{
                                    selectedVariants[groupName] }}</span>
                            </label>
                            <div class="flex flex-wrap gap-2">
                                <button v-for="val in values" :key="val" @click="selectVariant(groupName, val)"
                                    class="px-5 py-2.5 border-2 rounded-xl text-sm font-semibold transition-all cursor-pointer"
                                    :class="selectedVariants[groupName] === val
                                        ? 'border-blue-500 bg-blue-50 text-blue-700'
                                        : 'border-gray-200 text-gray-500 hover:border-gray-300'">
                                    {{ val }}
                                </button>
                            </div>
                        </div>
                    </div>

                    <!-- Actions -->
                    <div class="flex items-center gap-3 mb-7 flex-wrap">
                        <!-- Quantity -->
                        <div class="flex items-center gap-5 border-2 border-gray-200 rounded-xl px-4 py-3">
                            <button @click="quantity > 1 ? quantity-- : null"
                                class="text-gray-400 hover:text-gray-900 transition-colors cursor-pointer">
                                <Minus class="w-4 h-4" />
                            </button>
                            <span class="font-bold text-base min-w-[20px] text-center">{{ quantity }}</span>
                            <button @click="quantity < product.stock ? quantity++ : null"
                                class="text-gray-400 hover:text-gray-900 transition-colors cursor-pointer">
                                <Plus class="w-4 h-4" />
                            </button>
                        </div>

                        <!-- Add to Cart -->
                        <button @click="handleAddToCart" :disabled="!product.inStock || addingToCart"
                            class="flex-1 flex items-center justify-center gap-2.5 bg-blue-600 hover:bg-blue-700 text-white font-bold text-sm uppercase tracking-wider py-4 rounded-xl shadow-lg shadow-blue-600/30 transition-all hover:-translate-y-0.5 disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer">
                            <ShoppingBag class="w-5 h-5" />
                            {{ addingToCart ? 'Adding...' : (product.inStock ? 'Add to Cart' : 'Out of Stock') }}
                        </button>

                        <!-- Wishlist -->
                        <button @click="handleAddToWishlist"
                            class="w-[52px] h-[52px] border-2 rounded-xl flex items-center justify-center transition-all cursor-pointer"
                            :class="wishlist.isInWishlist(product?.id)
                                ? 'border-red-200 bg-red-50 text-red-500'
                                : 'border-gray-200 text-gray-400 hover:text-red-500 hover:border-red-200 hover:bg-red-50'">
                            <Heart class="w-5 h-5" :class="{ 'fill-current': wishlist.isInWishlist(product?.id) }" />
                        </button>
                    </div>

                    <!-- Trust Badges -->
                    <div class="flex flex-wrap gap-6 py-5 border-t border-b border-gray-100 mb-6">
                        <div class="flex items-center gap-2 text-xs font-semibold text-gray-500">
                            <Truck class="w-4 h-4 text-gray-400" /> Free Shipping
                        </div>
                        <div class="flex items-center gap-2 text-xs font-semibold text-gray-500">
                            <RefreshCcw class="w-4 h-4 text-gray-400" /> 30-Day Returns
                        </div>
                        <div class="flex items-center gap-2 text-xs font-semibold text-gray-500">
                            <ShieldCheck class="w-4 h-4 text-gray-400" /> Secure Checkout
                        </div>
                    </div>

                    <!-- Accordions -->
                    <div>
                        <div v-for="item in [
                            { id: 'details', title: 'Product Details', content: product.description || 'No details available.' },
                            { id: 'shipping', title: 'Shipping & Returns', content: 'Free shipping on orders over $50. Returns accepted within 30 days.' },
                            { id: 'sustainability', title: 'Sustainability', content: 'We use 100% recyclable packaging and ethically sourced ingredients.' }
                        ]" :key="item.id" class="border-b border-gray-100 last:border-b-0">
                            <button @click="toggleAccordion(item.id)"
                                class="w-full py-5 flex items-center justify-between text-left group cursor-pointer">
                                <span
                                    class="text-sm font-bold text-gray-900 group-hover:text-blue-600 transition-colors">
                                    {{ item.title }}
                                </span>
                                <component :is="activeAccordion === item.id ? ChevronUp : ChevronDown"
                                    class="w-4 h-4 text-gray-400" />
                            </button>
                            <div v-show="activeAccordion === item.id"
                                class="pb-5 text-sm text-gray-500 leading-relaxed">
                                {{ item.content }}
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Related Products -->
            <div v-if="relatedProducts.length > 0" class="mt-20 border-t border-gray-100 pt-12">
                <h2 class="text-2xl font-extrabold text-gray-900 mb-8">You might also like</h2>
                <div class="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-4 gap-5">
                    <RouterLink v-for="item in relatedProducts" :key="item.id" :to="`/product/${item.id}`"
                        class="group block">
                        <div class="aspect-square bg-gray-50 rounded-2xl overflow-hidden mb-3 relative">
                            <img :src="item.image" :alt="item.name"
                                class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500" />
                            <button
                                class="absolute bottom-3 right-3 bg-white w-9 h-9 rounded-full shadow-lg flex items-center justify-center opacity-0 group-hover:opacity-100 transition-all translate-y-2 group-hover:translate-y-0">
                                <ShoppingBag class="w-4 h-4 text-gray-900" />
                            </button>
                        </div>
                        <h3 class="font-bold text-sm text-gray-900 group-hover:text-blue-600 transition-colors">{{
                            item.name }}</h3>
                        <p class="text-xs text-gray-400 mt-0.5">{{ item.category }}</p>
                        <span class="font-bold text-sm text-gray-900 mt-1 block">${{ item.price?.toFixed(2) }}</span>
                    </RouterLink>
                </div>
            </div>
        </div>
    </div>
</template>
