<script setup>
import { ref, computed } from 'vue';
import { useRouter } from 'vue-router';
import { ShoppingBag, Heart, Star, ChevronLeft, ChevronRight } from 'lucide-vue-next';
import { cartApi } from '../api/cartApi';

const currentIndex = ref(0);

const products = [
    { id: 1, name: 'Wireless Headphones', category: 'Electronics', price: 129.99, rating: 4.5, reviews: 234, image: 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400&h=400&fit=crop', badge: 'Best Seller' },
    { id: 2, name: 'Smart Watch Pro', category: 'Electronics', price: 299.99, rating: 4.8, reviews: 456, image: 'https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400&h=400&fit=crop', badge: 'New' },
    { id: 3, name: 'Designer Backpack', category: 'Fashion', price: 79.99, rating: 4.3, reviews: 189, image: 'https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=400&h=400&fit=crop', badge: '' },
    { id: 4, name: 'Running Shoes', category: 'Sports', price: 149.99, rating: 4.6, reviews: 567, image: 'https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=400&h=400&fit=crop', badge: 'Hot' },
    { id: 5, name: 'Coffee Maker', category: 'Home', price: 89.99, rating: 4.4, reviews: 312, image: 'https://images.unsplash.com/photo-1517668808822-9ebb02f2a0e6?w=400&h=400&fit=crop', badge: 'Sale' },
    { id: 6, name: 'Yoga Mat Premium', category: 'Sports', price: 45.99, rating: 4.7, reviews: 423, image: 'https://images.unsplash.com/photo-1601925260368-ae2f83cf8b7f?w=400&h=400&fit=crop', badge: '' },
    { id: 7, name: 'Leather Wallet', category: 'Fashion', price: 59.99, rating: 4.5, reviews: 278, image: 'https://images.unsplash.com/photo-1627123424574-724758594e93?w=400&h=400&fit=crop', badge: 'New' },
    { id: 8, name: 'Desk Lamp Modern', category: 'Home', price: 69.99, rating: 4.2, reviews: 156, image: 'https://images.unsplash.com/photo-1507473885765-e6ed057f782c?w=400&h=400&fit=crop', badge: '' },
];

const itemsPerView = 4;
const maxIndex = Math.max(0, products.length - itemsPerView);

const canGoNext = computed(() => currentIndex.value < maxIndex);
const canGoPrev = computed(() => currentIndex.value > 0);

const translateX = computed(() => {
    return -(currentIndex.value * (100 / itemsPerView));
});

const nextSlide = () => {
    if (canGoNext.value) {
        currentIndex.value++;
    }
};

const prevSlide = () => {
    if (canGoPrev.value) {
        currentIndex.value--;
    }
};

const goToSlide = (index) => {
    currentIndex.value = index;
};

const router = useRouter();

const addToWishlist = (productId) => {
    console.log('Added to wishlist:', productId);
};

const addToCart = async (productId) => {
    try {
        await cartApi.addToCart(productId, 1);
        router.push('/cart');
    } catch (err) {
        console.error('Failed to add to cart', err);
        // Fallback for demo purposes: just redirect if API fails (e.g. invalid ID)
        // or show error. For now, let's alert.
        alert('Failed to add demo product to cart. It might not exist in the database.');
    }
};
</script>

<template>
    <div class="relative px-12">
        <!-- Carousel Container -->
        <div class="overflow-hidden">
            <div 
                class="flex transition-transform duration-500 ease-out"
                :style="{ transform: `translateX(${translateX}%)` }"
            >
                <div 
                    v-for="product in products" 
                    :key="product.id"
                    class="w-1/4 shrink-0 px-3"
                >
                    <div class="group bg-white rounded-3xl overflow-hidden shadow-sm hover:shadow-2xl transition-all duration-700 border border-gray-100 h-full">
                        <!-- Product Image -->
                        <div class="relative overflow-hidden aspect-4/5 bg-gray-50">
                            <img :src="product.image" :alt="product.name"
                                class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-700" />
                            <!-- Badge -->
                            <div v-if="product.badge"
                                class="absolute top-4 left-4 px-3 py-1 bg-white/90 backdrop-blur-sm text-gray-900 text-[10px] font-bold uppercase tracking-widest rounded-full shadow-sm">
                                {{ product.badge }}
                            </div>
                            <!-- Quick Actions -->
                            <div class="absolute top-4 right-4 flex flex-col gap-2 opacity-0 group-hover:opacity-100 transition-all duration-500 translate-x-4 group-hover:translate-x-0">
                                <button @click="addToWishlist(product.id)"
                                    class="w-10 h-10 bg-white rounded-full flex items-center justify-center shadow-lg hover:bg-violet-600 hover:text-white transition-all duration-300"
                                    aria-label="Add to wishlist">
                                    <Heart class="w-5 h-5 transition-colors" />
                                </button>
                            </div>
                            <!-- Add to Cart Overlay -->
                            <div class="absolute inset-x-4 bottom-4 opacity-0 group-hover:opacity-100 transition-all duration-500 translate-y-4 group-hover:translate-y-0">
                                <button @click="addToCart(product.id)"
                                    class="w-full py-4 bg-gray-900 text-white text-sm font-bold rounded-2xl hover:bg-violet-600 transition-colors flex items-center justify-center gap-2 shadow-2xl">
                                    <ShoppingBag class="w-4 h-4" />
                                    Add to Cart
                                </button>
                            </div>
                        </div>
                        <!-- Product Info -->
                        <div class="p-6 space-y-4">
                            <div>
                                <span class="text-[10px] text-gray-400 font-bold uppercase tracking-widest">{{ product.category }}</span>
                                <h3 class="font-bold text-gray-900 line-clamp-1 group-hover:text-violet-600 transition-colors text-lg mt-1">
                                    {{ product.name }}
                                </h3>
                            </div>
                            
                            <div class="flex items-center justify-between">
                                <div class="flex flex-col">
                                    <span class="text-xl font-black text-gray-900">${{ product.price }}</span>
                                    <div class="flex items-center gap-1 mt-1">
                                        <Star class="w-3 h-3 text-amber-400 fill-amber-400" />
                                        <span class="text-[10px] text-gray-500 font-bold">{{ product.rating }} ({{ product.reviews }})</span>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Navigation Arrows -->
        <button 
            @click="prevSlide"
            :disabled="!canGoPrev"
            class="absolute left-0 top-1/2 -translate-y-1/2 w-14 h-14 bg-white rounded-full shadow-xl flex items-center justify-center transition-all duration-500 z-10 group disabled:opacity-0 disabled:pointer-events-none hover:scale-110 border border-gray-100"
            aria-label="Previous products"
        >
            <ChevronLeft class="w-6 h-6 text-gray-900 group-hover:text-violet-600 transition-colors" />
        </button>
        <button 
            @click="nextSlide"
            :disabled="!canGoNext"
            class="absolute right-0 top-1/2 -translate-y-1/2 w-14 h-14 bg-white rounded-full shadow-xl flex items-center justify-center transition-all duration-500 z-10 group disabled:opacity-0 disabled:pointer-events-none hover:scale-110 border border-gray-100"
            aria-label="Next products"
        >
            <ChevronRight class="w-6 h-6 text-gray-900 group-hover:text-violet-600 transition-colors" />
        </button>

        <!-- Progress Indicators -->
        <div class="flex items-center justify-center gap-1 mt-12">
            <button
                v-for="i in (maxIndex + 1)" 
                :key="i"
                @click="goToSlide(i - 1)"
                class="transition-all duration-500 rounded-full cursor-pointer"
                :class="currentIndex === (i - 1) ? 'w-12 h-1 bg-gray-900' : 'w-4 h-1 bg-gray-200 hover:bg-gray-300'"
                :aria-label="`Go to slide ${i}`"
            ></button>
        </div>
    </div>
</template>

<style scoped>
/* Smooth carousel transitions */
</style>