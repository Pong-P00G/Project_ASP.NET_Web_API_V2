<script setup>
import { ref, onMounted, onUnmounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useWishlistStore } from '../stores/Wishlist'
import { productAPI } from '../api/productsApi'
import { cartApi } from '../api/cartApi'

const route = useRoute()
const router = useRouter()
const wishlist = useWishlistStore()
const product = ref(null)
const loading = ref(true)
const error = ref(null)
const selectedImage = ref(null)
const selectedVariants = ref({})
const quantity = ref(1)
const addingToCart = ref(false)
const showFullDescription = ref(false)
let autoSlideInterval = null

const stopAutoSlide = () => {
    if (autoSlideInterval) {
        clearInterval(autoSlideInterval)
        autoSlideInterval = null
    }
}

const startAutoSlide = () => {
    stopAutoSlide()
    if (product.value?.images?.length > 1) {
        autoSlideInterval = setInterval(() => {
            if (!selectedImage.value) return
            const currentIndex = product.value.images.findIndex(img => img.imageId === selectedImage.value.imageId)
            const nextIndex = (currentIndex + 1) % product.value.images.length
            selectedImage.value = product.value.images[nextIndex]
        }, 3000)
    }
}

const fetchProduct = async (id) => {
    try {
        loading.value = true
        error.value = null

        console.log('Fetching product with ID:', id)
        const response = await productAPI.getProductsById(id)
        console.log('Product API Response:', response)

        // Handle API response structure
        const productData = response.data

        if (!productData) {
            throw new Error('Product not found')
        }

        // Map API response to component format
        product.value = {
            id: productData.productId,
            name: productData.productName,
            basePrice: productData.basePrice,
            originalPrice: productData.originalPrice || productData.basePrice,
            discountPercentage: productData.discountPercentage || 0,
            description: productData.description || 'No description available',
            images: productData.images || [],
            inStock: productData.stockStatus !== 'out-of-stock' && productData.stock > 0,
            stock: productData.stock,
            stockStatus: productData.stockStatus,
            category: productData.categories?.[0]?.categoryName || 'Uncategorized',
            allCategories: productData.categories || [],
            sku: productData.sku || 'N/A',
            supplier: productData.supplier || 'N/A',
            featured: productData.featured || false,
            variants: productData.variants || []
        }

        // Set the first image as selected by default
        if (product.value.images && product.value.images.length > 0) {
            selectedImage.value = product.value.images[0]
            startAutoSlide()
        }

        console.log('Processed product:', product.value)
    } catch (err) {
        console.error('Error fetching product:', err)
        error.value = err.message || 'Failed to load product'
        product.value = null
    } finally {
        loading.value = false
    }
}

const selectImage = (image) => {
    selectedImage.value = image
    startAutoSlide() // Reset timer on manual selection
}

const groupedVariants = computed(() => {
    if (!product.value || !product.value.variants) return []

    // Group variants by their type (e.g., Color, Size)
    const grouped = {}

    product.value.variants.forEach(variant => {
        variant.options.forEach(option => {
            if (!grouped[option.variant]) {
                grouped[option.variant] = {
                    name: option.variant,
                    options: []
                }
            }

            // Check if option already exists to avoid duplicates
            const exists = grouped[option.variant].options.some(opt => opt.value === option.value)
            if (!exists) {
                grouped[option.variant].options.push({
                    value: option.value
                })
            }
        })
    })

    return Object.values(grouped)
})

const selectVariant = (variantName, value) => {
    selectedVariants.value[variantName] = value
}

// Find the selected variant based on user's selections
const selectedVariant = computed(() => {
    if (!product.value || !product.value.variants || product.value.variants.length === 0) return null

    const selectionKeys = Object.keys(selectedVariants.value)
    if (selectionKeys.length === 0) return product.value.variants[0] // Default to first variant

    // Find variant that matches all selected options
    return product.value.variants.find(variant => {
        return selectionKeys.every(key => {
            const selectedValue = selectedVariants.value[key]
            return variant.options.some(opt => opt.variant === key && opt.value === selectedValue)
        })
    }) || product.value.variants[0]
})

// Display price based on selected variant
const displayPrice = computed(() => {
    if (selectedVariant.value) {
        // Use discount price if available, otherwise regular price
        const variant = selectedVariant.value
        return variant.discountPrice && variant.discountPrice < variant.price
            ? variant.discountPrice
            : variant.price
    }
    return product.value?.basePrice || 0
})

// Original price for comparison
const originalPrice = computed(() => {
    if (selectedVariant.value) {
        return selectedVariant.value.price
    }
    return product.value?.basePrice || 0
})

// Check if variant is on sale
const isOnSale = computed(() => {
    if (selectedVariant.value) {
        const variant = selectedVariant.value
        return variant.discountPrice && variant.discountPrice < variant.price
    }
    return false
})

// Calculate discount percentage
const discountPercentage = computed(() => {
    if (isOnSale.value && selectedVariant.value) {
        const variant = selectedVariant.value
        return Math.round((1 - variant.discountPrice / variant.price) * 100)
    }
    return 0
})

// Get stock quantity for selected variant
const variantStock = computed(() => {
    if (selectedVariant.value) {
        return selectedVariant.value.stockQuantity
    }
    return product.value?.stock || 0
})

const currentCategories = computed(() => {
    if (product.value && product.value.allCategories && product.value.allCategories.length > 0) {
        return product.value.allCategories;
    }
    return [{ categoryId: 'default', categoryName: 'Uncategorized' }];
})

const increaseQuantity = () => {
    if (variantStock.value && quantity.value < variantStock.value) {
        quantity.value++
    }
}

const decreaseQuantity = () => {
    if (quantity.value > 1) {
        quantity.value--
    }
}

const addToCart = async (prod) => {
    if (variantStock.value <= 0) {
        alert('This variant is out of stock!')
        return
    }

    try {
        addingToCart.value = true
        await cartApi.addToCart(prod.id, quantity.value)
        router.push('/cart')
    } catch (err) {
        console.error('Error adding to cart:', err)
        alert('Failed to add to cart. Please try again.')
    } finally {
        addingToCart.value = false
    }
}

const addToWishlist = (prod) => {
    if (!prod.inStock) {
        alert('This product is out of stock!')
        return
    }

    wishlist.addToWishlist(prod)
    alert(`${prod.name} added to wishlist!`)
}

onMounted(() => {
    const productId = route.params.id
    if (productId) {
        fetchProduct(productId)
    } else {
        loading.value = false
        error.value = 'No product ID provided'
    }
})

onUnmounted(() => {
    stopAutoSlide()
})
</script>


<template>
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div v-if="loading" class="flex justify-center items-center h-64">
            <div class="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-blue-500"></div>
        </div>

        <div v-else-if="product" class="grid grid-cols-1 md:grid-cols-2 gap-8">
            <!-- Product Images Section -->
            <div class="space-y-4">
                <!-- Main Image -->
                <div class="aspect-square bg-transparent rounded-lg overflow-hidden relative">
                    <img v-if="selectedImage" :src="selectedImage.imageUrl" :alt="product.name"
                        class="w-full h-full object-contain">
                    <img v-else-if="product.images && product.images.length > 0" :src="product.images[0].imageUrl"
                        :alt="product.name" class="w-full h-full object-contain">
                    <div v-else class="w-full h-full flex items-center justify-center text-gray-400">
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-16 w-16" fill="none" viewBox="0 0 24 24"
                            stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                                d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                        </svg>
                    </div>
                    <div v-if="product.featured"
                        class="absolute top-2 left-2 bg-red-500 text-white text-xs font-bold px-2 py-1 rounded">
                        Featured
                    </div>
                </div>

                <!-- Thumbnail Images -->
                <div v-if="product.images && product.images.length > 0" class="flex flex-wrap gap-2 pt-2">
                    <div v-for="(image, index) in product.images" :key="image.imageId" @click="selectImage(image)"
                        class="shrink-0 w-20 h-20 rounded-md overflow-hidden cursor-pointer border-2"
                        :class="selectedImage && selectedImage.imageId === image.imageId ? 'border-blue-500' : 'border-gray-200'">
                        <img :src="image.imageUrl" :alt="`${product.name} ${index + 1}`"
                            class="w-full h-full object-cover">
                    </div>
                </div>
            </div>

            <!-- Product Information Section -->
            <div class="space-y-6">
                <div>
                    <div class="flex items-center gap-2 mb-2 flex-wrap">
                        <span v-for="cat in currentCategories" :key="cat.categoryId"
                            class="text-sm font-semibold text-blue-600 bg-blue-50 px-3 py-1 rounded-full">
                            {{ cat.categoryName }}
                        </span>
                        <span v-if="product.featured"
                            class="text-sm font-semibold text-red-600 bg-red-50 px-3 py-1 rounded-full">
                            Featured
                        </span>
                    </div>
                    <h1 class="text-4xl font-extrabold text-neutral-900 tracking-tight mb-2">{{ product.name }}</h1>
                    <div class="mt-2 flex items-center">
                        <div class="flex items-center">
                            <!-- Rating stars would go here -->
                            <div class="flex items-center">
                                <svg v-for="i in 5" :key="i" xmlns="http://www.w3.org/2000/svg"
                                    class="h-5 w-5 text-yellow-400" viewBox="0 0 20 20" fill="currentColor">
                                    <path
                                        d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                                </svg>
                                <span class="ml-2 text-sm text-gray-600">4.5 (128 reviews)</span>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Price Section -->
                <div class="mt-6 pb-6 border-b border-neutral-200">
                    <div class="flex items-baseline gap-4 flex-wrap">
                        <!-- Current/Sale Price -->
                        <p class="text-4xl font-light tracking-tight text-neutral-900">
                            ${{ displayPrice.toFixed(2) }}
                        </p>

                        <!-- Original Price (crossed out if on sale) -->
                        <span v-if="isOnSale" class="text-xl text-neutral-400 line-through font-light">
                            ${{ originalPrice.toFixed(2) }}
                        </span>

                        <!-- Discount Badge -->
                        <span v-if="isOnSale"
                            class="px-3 py-1 text-xs font-semibold tracking-wider uppercase bg-neutral-900 text-white">
                            Save {{ discountPercentage }}%
                        </span>
                    </div>

                    <!-- Selected Variant Info -->
                    <div v-if="selectedVariant && Object.keys(selectedVariants).length > 0"
                        class="mt-3 text-sm text-neutral-500 flex items-center gap-2">
                        <span v-for="(value, key) in selectedVariants" :key="key" class="text-neutral-700">
                            {{ key }}: <span class="font-medium">{{ value }}</span>
                        </span>
                    </div>
                </div>

                <!-- Stock Status -->
                <div class="mt-4">
                    <div class="flex items-center gap-3">
                        <span v-if="variantStock > 0" class="text-sm text-neutral-600">
                            <span class="inline-block w-2 h-2 bg-green-500 rounded-full mr-2"></span>
                            In Stock
                        </span>
                        <span v-else class="text-sm text-red-500">
                            <span class="inline-block w-2 h-2 bg-red-500 rounded-full mr-2"></span>
                            Out of Stock
                        </span>
                        <span class="text-sm text-neutral-400">
                            {{ variantStock }} available
                        </span>
                    </div>
                </div>

                <!-- Product Description -->
                <div class="mt-8 pt-8 border-t border-neutral-200">
                    <h3 class="text-sm font-bold text-gray-900 uppercase tracking-wider mb-4">Description</h3>
                    <div class="prose prose-sm text-gray-500 max-w-none">
                        <div class="relative">
                            <p :class="['leading-relaxed transition-all', !showFullDescription ? 'line-clamp-3' : '']">
                                {{ product.description || 'No description available for this product.' }}
                            </p>
                            <button v-if="product.description && product.description.length > 150"
                                @click="showFullDescription = !showFullDescription"
                                class="text-xs font-bold text-blue-600 hover:text-blue-700 mt-2 focus:outline-none flex items-center gap-1">
                                {{ showFullDescription ? 'Show less' : 'Read more' }}
                                <svg xmlns="http://www.w3.org/2000/svg" class="h-3 w-3"
                                    :class="{ 'rotate-180': showFullDescription }" fill="none" viewBox="0 0 24 24"
                                    stroke="currentColor">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                                        d="M19 9l-7 7-7-7" />
                                </svg>
                            </button>
                        </div>
                    </div>
                </div>

                <!-- Product Variants -->
                <div v-if="product.variants && product.variants.length > 0" class="mt-8">
                    <div class="space-y-6">
                        <div v-for="(variantGroup, index) in groupedVariants" :key="index">
                            <h4 class="text-xs font-semibold text-neutral-500 uppercase tracking-widest mb-3">
                                {{ variantGroup.name }}
                            </h4>
                            <div class="flex flex-wrap gap-2">
                                <button v-for="(option, optionIndex) in variantGroup.options" :key="optionIndex"
                                    @click="selectVariant(variantGroup.name, option.value)"
                                    class="min-w-[60px] px-5 py-3 text-sm rounded-2xl font-medium border transition-all duration-150"
                                    :class="selectedVariants[variantGroup.name] === option.value
                                        ? 'border-neutral-900 bg-neutral-900 text-white'
                                        : 'border-neutral-300 text-neutral-700 bg-white hover:border-neutral-900'">
                                    {{ option.value }}
                                </button>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Quantity Selector -->
                <div class="mt-8">
                    <h4 class="text-xs font-semibold text-neutral-500 uppercase tracking-widest mb-3">Quantity</h4>
                    <div class="flex items-center gap-4">
                        <div class="inline-flex items-center border border-neutral-300 rounded-lg">
                            <button @click="decreaseQuantity"
                                class="w-10 h-10 flex items-center justify-center text-neutral-600 hover:bg-neutral-100 transition-colors"
                                :disabled="quantity <= 1" :class="quantity <= 1 ? 'opacity-30 cursor-not-allowed' : ''">
                                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
                                        d="M20 12H4" />
                                </svg>
                            </button>
                            <input type="number" v-model="quantity" min="1" :max="variantStock"
                                class="w-14 h-10 text-center text-sm font-medium border-x border-neutral-300 focus:outline-none">
                            <button @click="increaseQuantity"
                                class="w-10 h-10 flex items-center justify-center text-neutral-600 hover:bg-neutral-100 transition-colors"
                                :disabled="quantity >= variantStock"
                                :class="quantity >= variantStock ? 'opacity-30 cursor-not-allowed' : ''">
                                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
                                        d="M12 4v16m8-8H4" />
                                </svg>
                            </button>
                        </div>
                    </div>
                </div>

                <!-- Action Buttons -->
                <div class="mt-10 space-y-3">
                    <button @click="addToCart(product)" :disabled="variantStock <= 0 || addingToCart"
                        class="w-full py-3.5 text-sm font-semibold rounded-xl tracking-wider uppercase transition-all duration-150 flex items-center justify-center gap-3"
                        :class="variantStock > 0
                            ? 'bg-neutral-900 text-white hover:bg-neutral-800'
                            : 'bg-neutral-200 text-neutral-400 cursor-not-allowed'">
                        <span v-if="addingToCart">
                            <svg class="animate-spin h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none"
                                viewBox="0 0 24 24">
                                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor"
                                    stroke-width="4"></circle>
                                <path class="opacity-75" fill="currentColor"
                                    d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z">
                                </path>
                            </svg>
                        </span>
                        <span v-if="variantStock > 0">{{ addingToCart ? 'Adding to Cart...' : 'Add to Cart' }}</span>
                        <span v-else>Out of Stock</span>
                    </button>

                    <button @click="addToWishlist(product)"
                        class="w-full py-3.5 text-sm font-semibold rounded-xl tracking-wider uppercase border border-neutral-900 text-neutral-900 bg-white hover:text-red-500 hover:border-red-500 hover:bg-neutral-50 transition-colors flex items-center justify-center gap-3">
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24"
                            stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
                                d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
                        </svg>
                        Add to Wishlist
                    </button>
                </div>
                <!-- Product Specifications -->
                <!-- <div class="mt-8 border-t border-neutral-200 pt-8 hidden">
                    <h3 class="text-sm font-bold text-gray-900 uppercase tracking-wider mb-4">Specifications</h3>
                    <div class="grid grid-cols-1 gap-6 sm:grid-cols-2">
                        <div class="border-b border-neutral-100 pb-2">
                            <span
                                class="block text-xs font-medium text-neutral-500 uppercase tracking-wide">Supplier</span>
                            <span class="block mt-1 text-sm font-medium text-neutral-900">{{ product.supplier || 'N/A'
                                }}</span>
                        </div>
                        <div class="border-b border-neutral-100 pb-2">
                            <span class="block text-xs font-medium text-neutral-500 uppercase tracking-wide">SKU</span>
                            <span class="block mt-1 text-sm font-medium text-neutral-900">{{ product.sku || 'N/A'
                                }}</span>
                        </div>
                        <div class="border-b border-neutral-100 pb-2 sm:col-span-2">
                            <span
                                class="block text-xs font-medium text-neutral-500 uppercase tracking-wide">Categories</span>
                            <div class="mt-2 flex flex-wrap gap-2">
                                <span v-for="cat in currentCategories" :key="cat.categoryId"
                                    class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-neutral-100 text-neutral-800">
                                    {{ cat.categoryName }}
                                </span>
                            </div>
                        </div>
                    </div>
                </div> -->
            </div>
        </div>

        <div v-else class="text-center py-12">
            <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                    d="M9.172 16.172a4 4 0 015.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <h3 class="mt-2 text-lg font-medium text-gray-900">Product not found</h3>
            <p class="mt-1 text-gray-500">The product you're looking for doesn't exist or is no longer available.</p>
            <div class="mt-6">
                <router-link to="/products" class="text-blue-600 hover:text-blue-500 font-medium">
                    Browse all products &rarr;
                </router-link>
            </div>
        </div>
    </div>
</template>
