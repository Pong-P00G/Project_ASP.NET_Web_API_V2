<script setup>
import { ref, onMounted, computed } from 'vue'
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
            sku: productData.sku || 'N/A',
            supplier: productData.supplier || 'N/A',
            featured: productData.featured || false,
            variants: productData.variants || []
        }

        // Set the first image as selected by default
        if (product.value.images && product.value.images.length > 0) {
            selectedImage.value = product.value.images[0]
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

const increaseQuantity = () => {
    if (product.value && quantity.value < product.value.stock) {
        quantity.value++
    }
}

const decreaseQuantity = () => {
    if (quantity.value > 1) {
        quantity.value--
    }
}

const addToCart = async (prod) => {
    if (!prod.inStock) {
        alert('This product is out of stock!')
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
                <div class="aspect-square bg-gray-100 rounded-lg overflow-hidden relative">
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
                <div v-if="product.images && product.images.length > 1" class="flex gap-2 overflow-x-auto pb-2">
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
                    <h1 class="text-3xl font-bold text-gray-900">{{ product.name }}</h1>
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

                <!-- Price -->
                <div class="mt-4">
                    <div class="flex items-center">
                        <p class="text-3xl font-bold text-gray-900">${{ product.basePrice }}</p>
                        <span v-if="product.originalPrice && product.originalPrice > product.basePrice"
                            class="ml-4 text-lg text-gray-500 line-through">${{ product.originalPrice }}</span>
                        <span v-if="product.discountPercentage > 0"
                            class="ml-4 inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-red-100 text-red-800">
                            {{ product.discountPercentage }}% OFF
                        </span>
                    </div>
                </div>

                <!-- Stock Status -->
                <div class="mt-2">
                    <div class="flex items-center">
                        <span v-if="product.inStock"
                            class="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-green-100 text-green-800">
                            <svg class="mr-1.5 h-2 w-2 text-green-400" fill="currentColor" viewBox="0 0 8 8">
                                <circle cx="4" cy="4" r="3" />
                            </svg>
                            In Stock
                        </span>
                        <span v-else
                            class="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-red-100 text-red-800">
                            <svg class="mr-1.5 h-2 w-2 text-red-400" fill="currentColor" viewBox="0 0 8 8">
                                <circle cx="4" cy="4" r="3" />
                            </svg>
                            Out of Stock
                        </span>
                        <span v-if="product.stock" class="ml-4 text-sm text-gray-500">({{ product.stock }}
                            available)</span>
                    </div>
                </div>

                <!-- Product Description -->
                <div class="mt-6">
                    <h3 class="text-lg font-medium text-gray-900">Description</h3>
                    <div class="mt-2 text-gray-600">
                        <p>{{ product.description || 'No description available for this product.' }}</p>
                    </div>
                </div>

                <!-- Product Variants -->
                <div v-if="product.variants && product.variants.length > 0" class="mt-6">
                    <h3 class="text-lg font-medium text-gray-900">Options</h3>
                    <div class="mt-4 space-y-4">
                        <div v-for="(variantGroup, index) in groupedVariants" :key="index"
                            class="border-b border-gray-200 pb-4">
                            <h4 class="text-sm font-medium text-gray-900 capitalize">{{ variantGroup.name }}</h4>
                            <div class="mt-2 flex flex-wrap gap-2">
                                <button v-for="(option, optionIndex) in variantGroup.options" :key="optionIndex"
                                    @click="selectVariant(variantGroup.name, option.value)"
                                    class="px-3 py-1.5 text-sm border rounded-md" :class="selectedVariants[variantGroup.name] === option.value
                                        ? 'border-blue-500 bg-blue-50 text-blue-700'
                                        : 'border-gray-300 text-gray-700 hover:bg-gray-50'">
                                    {{ option.value }}
                                </button>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Quantity Selector -->
                <div class="mt-6">
                    <h3 class="text-lg font-medium text-gray-900">Quantity</h3>
                    <div class="mt-2 flex items-center">
                        <button @click="decreaseQuantity"
                            class="px-3 py-1 border border-gray-300 rounded-l-md bg-gray-50 text-gray-600 hover:bg-gray-100"
                            :disabled="quantity <= 1">
                            -
                        </button>
                        <input type="number" v-model="quantity" min="1" :max="product.stock"
                            class="w-16 text-center border-y border-gray-300">
                        <button @click="increaseQuantity"
                            class="px-3 py-1 border border-gray-300 rounded-r-md bg-gray-50 text-gray-600 hover:bg-gray-100"
                            :disabled="quantity >= product.stock">
                            +
                        </button>
                        <span v-if="product.stock" class="ml-4 text-sm text-gray-500">Max: {{ product.stock }}</span>
                    </div>
                </div>

                <!-- Action Buttons -->
                <div class="mt-8 flex flex-col sm:flex-row gap-3">
                    <button @click="addToCart(product)" :disabled="!product.inStock || addingToCart"
                        class="flex-1 px-6 py-3 border border-transparent text-base font-medium rounded-md text-white bg-blue-600 shadow-sm hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center">
                        <span v-if="addingToCart" class="mr-2">
                            <svg class="animate-spin h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none"
                                viewBox="0 0 24 24">
                                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor"
                                    stroke-width="4"></circle>
                                <path class="opacity-75" fill="currentColor"
                                    d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z">
                                </path>
                            </svg>
                        </span>
                        <span v-if="product.inStock">{{ addingToCart ? 'Adding...' : 'Add to Cart' }}</span>
                        <span v-else>Out of Stock</span>
                    </button>

                    <button @click="addToWishlist(product)"
                        class="flex-1 px-6 py-3 border border-gray-300 text-base font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 flex items-center justify-center">
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 mr-2" fill="none" viewBox="0 0 24 24"
                            stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                                d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
                        </svg>
                        Wishlist
                    </button>
                </div>

                <!-- Product Meta Information -->
                <div class="mt-8 border-t border-gray-200 pt-6">
                    <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                        <div>
                            <h4 class="text-sm font-medium text-gray-900">Category</h4>
                            <p class="mt-1 text-sm text-gray-600">{{ product.category || 'Uncategorized' }}</p>
                        </div>
                        <div>
                            <h4 class="text-sm font-medium text-gray-900">SKU</h4>
                            <p class="mt-1 text-sm text-gray-600">{{ product.sku || 'N/A' }}</p>
                        </div>
                        <div>
                            <h4 class="text-sm font-medium text-gray-900">Supplier</h4>
                            <p class="mt-1 text-sm text-gray-600">{{ product.supplier || 'N/A' }}</p>
                        </div>
                    </div>
                </div>
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
