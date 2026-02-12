<script setup>
import { ref, computed, onMounted } from 'vue';
import { RouterLink, useRouter } from 'vue-router';
import { ShoppingBag, Heart, ChevronLeft, ChevronRight } from 'lucide-vue-next';
import { productAPI } from '../api/productsApi';
import { cartApi } from '../api/cartApi';
import { useWishlistStore } from '../stores/Wishlist.js';

const router = useRouter();
const wishlistStore = useWishlistStore();

const products = ref([]);
const loading = ref(true);
const currentIndex = ref(0);
const itemsPerView = 4;

const maxIndex = computed(() => Math.max(0, products.value.length - itemsPerView));
const canGoNext = computed(() => currentIndex.value < maxIndex.value);
const canGoPrev = computed(() => currentIndex.value > 0);

const translateX = computed(() => {
    return -(currentIndex.value * (100 / itemsPerView));
});

const nextSlide = () => {
    if (canGoNext.value) currentIndex.value++;
};

const prevSlide = () => {
    if (canGoPrev.value) currentIndex.value--;
};

const addToWishlist = (product) => {
    wishlistStore.addToWishlist(product);
};

const addToCart = async (productId) => {
    try {
        await cartApi.addToCart(productId, 1);
        router.push('/cart');
    } catch (err) {
        console.error('Failed to add to cart', err);
    }
};

const fetchProducts = async () => {
    try {
        loading.value = true;
        const response = await productAPI.getAllProducts({ page: 1, pageSize: 12, isActive: true });
        const data = response.data;
        const items = data?.items || data || [];

        products.value = items.map(p => ({
            id: p.productId,
            name: p.productName,
            price: p.basePrice,
            discountPrice: p.variants?.[0]?.discountPrice || null,
            category: p.categories?.[0]?.categoryName || 'Product',
            image: p.images?.[0]?.imageUrl || 'https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=400&h=400&fit=crop',
            badge: p.featured ? 'New' : (p.variants?.[0]?.discountPrice ? 'Sale' : ''),
            inStock: p.stock > 0,
        }));
    } catch (err) {
        console.error('Error fetching products:', err);
    } finally {
        loading.value = false;
    }
};

onMounted(() => {
    fetchProducts();
});
</script>

<template>
    <div class="relative px-12">
        <!-- Loading Skeleton -->
        <div v-if="loading" class="grid grid-cols-4 gap-5">
            <div v-for="n in 4" :key="n" class="animate-pulse">
                <div class="aspect-[4/5] bg-neutral-100 mb-4"></div>
                <div class="h-3 bg-neutral-100 w-1/3 mb-3"></div>
                <div class="h-4 bg-neutral-100 w-2/3 mb-3"></div>
                <div class="h-4 bg-neutral-100 w-1/4"></div>
            </div>
        </div>

        <!-- Carousel -->
        <template v-else>
            <div class="overflow-hidden">
                <div class="flex transition-transform duration-700 ease-out"
                    :style="{ transform: `translateX(${translateX}%)` }">
                    <div v-for="product in products" :key="product.id" class="w-1/4 shrink-0 px-2.5">
                        <div class="product-card group h-full">
                            <!-- Product Image -->
                            <RouterLink :to="`/product/${product.id}`"
                                class="block relative overflow-hidden aspect-[4/5] bg-neutral-50 mb-5">
                                <img :src="product.image" :alt="product.name"
                                    class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-700" />
                                <!-- Badge -->
                                <div v-if="product.badge"
                                    class="absolute top-4 left-4 px-3 py-1 text-[9px] font-medium tracking-[0.15em] uppercase text-white"
                                    style="background-color: #C8A97E;">
                                    {{ product.badge }}
                                </div>
                                <!-- Quick Actions -->
                                <div
                                    class="absolute top-4 right-4 opacity-0 group-hover:opacity-100 transition-all duration-500 translate-y-2 group-hover:translate-y-0">
                                    <button @click.stop.prevent="addToWishlist(product)"
                                        class="gold-hover-bg w-10 h-10 bg-white flex items-center justify-center hover:text-white transition-all duration-300"
                                        aria-label="Add to wishlist">
                                        <Heart class="w-4 h-4" />
                                    </button>
                                </div>
                                <!-- Add to Cart Overlay -->
                                <div
                                    class="absolute inset-x-0 bottom-0 opacity-0 group-hover:opacity-100 transition-all duration-500 translate-y-full group-hover:translate-y-0">
                                    <button @click.stop.prevent="addToCart(product.id)"
                                        class="w-full py-4 text-white text-xs font-medium tracking-[0.15em] uppercase hover:opacity-90 transition-colors flex items-center justify-center gap-2"
                                        style="background-color: #C8A97E;">
                                        <ShoppingBag class="w-3.5 h-3.5" />
                                        Add to Cart
                                    </button>
                                </div>
                                <!-- Thin border -->
                                <div class="absolute inset-0 border border-black/5 pointer-events-none"></div>
                            </RouterLink>
                            <!-- Product Info -->
                            <RouterLink :to="`/product/${product.id}`" class="block space-y-2">
                                <span class="text-[10px] text-black/30 font-medium tracking-[0.2em] uppercase">{{
                                    product.category }}</span>
                                <h3
                                    class="text-sm font-medium text-black line-clamp-1 group-hover:underline underline-offset-4 transition-all">
                                    {{ product.name }}
                                </h3>
                                <div class="flex items-center gap-3">
                                    <span class="text-sm font-medium text-black">${{ product.price?.toFixed(2) }}</span>
                                    <span v-if="product.discountPrice" class="text-xs text-black/30 line-through">
                                        ${{ (product.price * 1.2).toFixed(2) }}
                                    </span>
                                </div>
                            </RouterLink>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Navigation Arrows -->
            <button v-if="products.length > itemsPerView" @click="prevSlide" :disabled="!canGoPrev"
                class="gold-hover-bg absolute left-0 top-[40%] -translate-y-1/2 w-12 h-12 bg-white border border-black/10 flex items-center justify-center transition-all duration-500 z-10 disabled:opacity-0 disabled:pointer-events-none hover:text-white hover:border-transparent"
                aria-label="Previous products">
                <ChevronLeft class="w-5 h-5" />
            </button>
            <button v-if="products.length > itemsPerView" @click="nextSlide" :disabled="!canGoNext"
                class="gold-hover-bg absolute right-0 top-[40%] -translate-y-1/2 w-12 h-12 bg-white border border-black/10 flex items-center justify-center transition-all duration-500 z-10 disabled:opacity-0 disabled:pointer-events-none hover:text-white hover:border-transparent"
                aria-label="Next products">
                <ChevronRight class="w-5 h-5" />
            </button>

            <!-- Progress Indicators -->
            <div v-if="products.length > itemsPerView" class="flex items-center justify-center gap-2 mt-12">
                <button v-for="i in (maxIndex + 1)" :key="i" @click="currentIndex = i - 1"
                    class="transition-all duration-500 cursor-pointer"
                    :class="currentIndex === (i - 1) ? 'w-8 h-[2px]' : 'w-4 h-[2px] hover:opacity-60'"
                    :style="currentIndex === (i - 1) ? 'background-color: #C8A97E;' : 'background-color: rgba(0,0,0,0.12);'"
                    :aria-label="`Go to slide ${i}`"></button>
            </div>

            <!-- Empty State -->
            <div v-if="!loading && products.length === 0" class="text-center py-16">
                <p class="text-black/30 text-sm">No products available at the moment.</p>
            </div>
        </template>
    </div>
</template>

<style scoped>
.product-card {
    cursor: pointer;
}

.gold-hover-bg:hover {
    background-color: #C8A97E;
}
</style>