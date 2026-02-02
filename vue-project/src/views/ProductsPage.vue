<script setup>
import { ref, computed, onMounted } from 'vue';
import { RouterLink, useRouter } from 'vue-router';
import {
  Search,
  Grid3x3,
  List,
  Star,
  Heart,
  ShoppingCart,
  ChevronDown,
  X,
  Filter,
  Loader2,
  AlertCircle,
  Sparkles,
  TrendingUp
} from 'lucide-vue-next';
import { useWishlistStore } from '../stores/Wishlist.js';
import { productAPI } from '../api/productsApi';
import { cartApi } from '../api/cartApi';

// Stores
const wishlistStore = useWishlistStore();
const router = useRouter();

// State
const viewMode = ref('grid');
const searchQuery = ref('');
const selectedCategory = ref('All');
const priceRange = ref([0, 10000]);
const sortBy = ref('featured');
const showFilters = ref(false);
const loading = ref(false);
const error = ref(null);
const allProducts = ref([]);


const categories = ref(['All', 'Electronics', 'Fashion', 'Sports', 'Home']);
const uniqueCategories = [...new Set(allProducts.value.map(p => p.category))];
categories.value = ['All', ...uniqueCategories];

const sortOptions = [
  { value: 'featured', label: 'Featured' },
  { value: 'price-low', label: 'Price: Low to High' },
  { value: 'price-high', label: 'Price: High to Low' },
  { value: 'rating', label: 'Highest Rated' },
  { value: 'newest', label: 'Newest First' },
];

// Computed
const filteredProducts = computed(() => {
  let filtered = allProducts.value.filter(product => {
    const matchesSearch = product.name.toLowerCase().includes(searchQuery.value.toLowerCase());
    const matchesCategory = selectedCategory.value === 'All' || product.category === selectedCategory.value;
    const matchesPrice = product.price >= priceRange.value[0] && product.price <= priceRange.value[1];
    return matchesSearch && matchesCategory && matchesPrice;
  });

  // Sort
  switch (sortBy.value) {
    case 'price-low':
      filtered.sort((a, b) => a.price - b.price);
      break;
    case 'price-high':
      filtered.sort((a, b) => b.price - a.price);
      break;
    case 'rating':
      filtered.sort((a, b) => b.rating - a.rating);
      break;
    case 'newest':
      filtered.sort((a, b) => (b.badge === 'New' ? 1 : 0) - (a.badge === 'New' ? 1 : 0));
      break;
  }

  return filtered;
});

const hasActiveFilters = computed(() => {
  return searchQuery.value || selectedCategory.value !== 'All' || priceRange.value[0] > 0 || priceRange.value[1] < 500;
});

// Methods
const handleAddToWishlist = (product, event) => {
  event.preventDefault();
  event.stopPropagation();
  wishlistStore.addToWishlist(product);
};

const addToCart = async (productId, event) => {
  event.preventDefault();
  event.stopPropagation();
  console.log('Adding to cart:', productId);
  
  try {
    await cartApi.addToCart(productId, 1);
    router.push('/cart');
  } catch (err) {
    console.error('Failed to add to cart', err);
    // You might want to show a toast here instead of alert
    alert('Failed to add item to cart. Please try again.');
  }
};

const clearFilters = () => {
  searchQuery.value = '';
  selectedCategory.value = 'All';
  priceRange.value = [0, 500];
  sortBy.value = 'featured';
};

const fetchProducts = async () => {
  try {
    loading.value = true;
    error.value = null;

    console.log('Fetching products...');
    const response = await productAPI.getAllProducts({
      page: 1,
      pageSize: 10,
      isActive: true
    });
    console.log('API Response:', response);

    // Handle API response structure
    let productsData = [];

    if (response.data?.items) {
      productsData = response.data.items;
    } else if (Array.isArray(response.data)) {
      productsData = response.data;
    } else {
      console.warn('Unexpected API response structure:', response);
      productsData = [];
    }

    console.log('Products Data:', productsData);
    console.log('Products Count:', productsData.length);

    if (!Array.isArray(productsData) || productsData.length === 0) {
      console.warn('No products found or invalid format');
      allProducts.value = [];
      return;
    }

    allProducts.value = productsData.map(p => ({
      id: p.productId, // API uses 'productId'
      name: p.productName, // API uses 'productName'
      price: p.basePrice, // API uses 'basePrice'
      originalPrice: null, // Add if you have discount logic
      category: p.categories?.[0]?.categoryName || 'Uncategorized', // Get first category
      badge: p.featured ? 'Featured' : (p.stockStatus === 'new' ? 'New' : null),
      rating: 4.0 + Math.random(), // Generate a random rating or add to API
      reviews: Math.floor(Math.random() * 500),
      image: p.images?.[0]?.imageUrl || 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=500&h=500&fit=crop',
      inStock: p.stockStatus !== 'out-of-stock' && p.stock > 0,
      stock: p.stock,
      stockStatus: p.stockStatus,
      description: p.description
    }));

    console.log('Processed Products:', allProducts.value);
  } catch (err) {
    error.value = 'Failed to load products';
    console.error('Error fetching products:', err);
    console.error('Error details:', {
      message: err.message,
      response: err.response,
      stack: err.stack
    });
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  fetchProducts();
});

</script>

<template>
  <div class="min-h-screen bg-white">
    <!-- Hero Header -->
    <section class="pt-32 pb-16 bg-white overflow-hidden">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 relative">
        <!-- Decorative Elements -->
        <div class="absolute -top-24 -right-24 w-96 h-96 bg-violet-100 rounded-full blur-3xl opacity-50"></div>
        <div class="absolute -bottom-24 -left-24 w-96 h-96 bg-amber-100 rounded-full blur-3xl opacity-50"></div>

        <div class="relative text-center space-y-8 max-w-3xl mx-auto">
          <div class="inline-flex items-center gap-2 px-3 py-1 bg-gray-100 rounded-full text-gray-600 text-xs font-bold tracking-wider uppercase">
            <Sparkles class="w-3 h-3" />
            Curated Collection
          </div>
          <h1 class="text-5xl md:text-7xl font-light text-gray-900 tracking-tight leading-tight">
            Our Premium <br />
            <span class="font-bold italic">Collection</span>
          </h1>
          <p class="text-xl text-gray-500 font-light leading-relaxed">
            Discover our carefully selected range of high-quality products, designed to elevate your lifestyle.
          </p>
        </div>
      </div>
    </section>

    <!-- Main Content -->
    <section class="py-12">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <!-- Enhanced Controls Bar -->
        <div class="mb-12 space-y-6">
          <div class="flex flex-col lg:flex-row gap-6 items-center">
            <!-- Enhanced Search -->
            <div class="flex-1 relative w-full group">
              <Search class="absolute left-6 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400 group-focus-within:text-gray-900 transition-colors" />
              <input
                  v-model="searchQuery"
                  type="text"
                  placeholder="Search for products..."
                  class="w-full pl-14 pr-6 py-4 bg-gray-50 border-0 rounded-full text-gray-900 focus:ring-2 focus:ring-gray-900 transition-all placeholder:text-gray-400 font-medium"
              />
            </div>

            <div class="flex flex-wrap items-center gap-4 w-full lg:w-auto">
              <!-- Enhanced Sort Dropdown -->
              <div class="relative min-w-50 flex-1 lg:flex-none">
                <select
                    v-model="sortBy"
                    class="w-full pl-6 pr-12 py-4 bg-gray-50 border-0 rounded-full text-gray-900 focus:ring-2 focus:ring-gray-900 transition-all appearance-none cursor-pointer font-bold text-sm uppercase tracking-wider"
                >
                  <option v-for="option in sortOptions" :key="option.value" :value="option.value">
                    {{ option.label }}
                  </option>
                </select>
                <ChevronDown class="absolute right-5 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
              </div>

              <!-- View Toggle -->
              <div class="flex gap-1 bg-gray-50 rounded-full p-1 border border-gray-100">
                <button
                    @click="viewMode = 'grid'"
                    :class="viewMode === 'grid' ? 'bg-white shadow-sm text-gray-900' : 'text-gray-400 hover:text-gray-600'"
                    class="p-3 rounded-full transition-all duration-300"
                >
                  <Grid3x3 class="w-5 h-5" />
                </button>
                <button
                    @click="viewMode = 'list'"
                    :class="viewMode === 'list' ? 'bg-white shadow-sm text-gray-900' : 'text-gray-400 hover:text-gray-600'"
                    class="p-3 rounded-full transition-all duration-300"
                >
                  <List class="w-5 h-5" />
                </button>
              </div>

              <!-- Filter Button -->
              <button
                  @click="showFilters = !showFilters"
                  class="px-8 py-4 bg-gray-900 text-white rounded-full font-bold text-sm uppercase tracking-wider hover:bg-black transition-all duration-300 flex items-center gap-3 shadow-lg hover:shadow-violet-200"
              >
                <Filter class="w-4 h-4" />
                <span>Filters</span>
                <div v-if="hasActiveFilters" class="w-2 h-2 bg-violet-400 rounded-full"></div>
              </button>
            </div>
          </div>

          <!-- Active Filters -->
          <transition name="fade">
            <div v-if="hasActiveFilters" class="flex flex-wrap items-center gap-3">
              <span class="text-xs text-gray-400 font-bold uppercase tracking-widest mr-2">
                  Active Filters:
              </span>
              <button
                  v-if="searchQuery"
                  @click="searchQuery = ''"
                  class="px-4 py-2 bg-gray-100 text-gray-900 rounded-full text-xs font-bold uppercase tracking-wider flex items-center gap-2 hover:bg-gray-200 transition-all duration-300"
              >
                Search: {{ searchQuery }}
                <X class="w-3 h-3" />
              </button>
              <button
                  v-if="selectedCategory !== 'All'"
                  @click="selectedCategory = 'All'"
                  class="px-4 py-2 bg-gray-100 text-gray-900 rounded-full text-xs font-bold uppercase tracking-wider flex items-center gap-2 hover:bg-gray-200 transition-all duration-300"
              >
                {{ selectedCategory }}
                <X class="w-3 h-3" />
              </button>
              <button
                  @click="clearFilters"
                  class="ml-auto text-xs font-bold uppercase tracking-widest text-gray-400 hover:text-red-500 transition-colors"
              >
                Clear All
              </button>
            </div>
          </transition>

          <!-- Filter Panel -->
          <transition name="slide-fade">
            <div v-if="showFilters" class="p-10 bg-gray-50/50 rounded-[2.5rem] border border-gray-100 space-y-10">
              <!-- Category Filter -->
              <div class="space-y-6">
                <label class="text-xs font-bold uppercase tracking-[0.2em] text-gray-900 block">
                  Shop by Category
                </label>
                <div class="flex flex-wrap gap-3">
                  <button
                      v-for="category in categories"
                      :key="category"
                      @click="selectedCategory = category"
                      class="px-6 py-3 rounded-full text-sm font-bold transition-all duration-300"
                      :class="selectedCategory === category
                      ? 'bg-gray-900 text-white shadow-xl scale-105'
                      : 'bg-white text-gray-500 hover:text-gray-900 border border-gray-100'">
                    {{ category }}
                  </button>
                </div>
              </div>

              <!-- Price Range -->
              <div class="space-y-6">
                <div class="flex items-center justify-between">
                  <label class="text-xs font-bold uppercase tracking-[0.2em] text-gray-900">
                    Price Range
                  </label>
                  <span class="text-sm font-black text-gray-900">${{ priceRange[0] }} — ${{ priceRange[1] }}</span>
                </div>
                <div class="space-y-4 px-2">
                  <input
                      v-model.number="priceRange[0]"
                      type="range"
                      min="0"
                      max="10000"
                      class="w-full h-1 bg-gray-200 rounded-lg appearance-none cursor-pointer accent-gray-900"
                  />
                  <input
                      v-model.number="priceRange[1]"
                      type="range"
                      min="0"
                      max="10000"
                      class="w-full h-1 bg-gray-200 rounded-lg appearance-none cursor-pointer accent-gray-900"
                  />
                </div>
              </div>

              <!-- Results Summary -->
              <div class="pt-8 border-t border-gray-100">
                <p class="text-sm text-gray-500 font-medium">
                  Showing <span class="text-gray-900 font-black">{{ filteredProducts.length }}</span>
                  of <span class="text-gray-900 font-black">{{ allProducts.length }}</span> items found
                </p>
              </div>
            </div>
          </transition>
        </div>

        <!-- Loading State -->
        <div v-if="loading" class="flex flex-col items-center justify-center py-48 gap-8">
          <div class="relative">
            <div class="w-16 h-16 border-2 border-gray-100 rounded-full"></div>
            <div class="absolute inset-0 w-16 h-16 border-t-2 border-gray-900 rounded-full animate-spin"></div>
          </div>
          <p class="text-gray-400 font-bold uppercase tracking-widest text-xs">Discovering products...</p>
        </div>

        <!-- Error State -->
        <div v-else-if="error" class="bg-linear-to-br from-red-50 to-red-100 border-2 border-red-200 p-12 rounded-3xl text-center shadow-xl">
          <div class="inline-flex items-center justify-center w-20 h-20 bg-red-200 rounded-full mb-6">
            <AlertCircle class="w-10 h-10 text-red-600" />
          </div>
          <h3 class="text-2xl font-bold text-red-900 mb-3">{{ error }}</h3>
          <p class="text-red-700 mb-6">Something went wrong while loading products</p>
          <button
              @click="fetchProducts"
              class="px-8 py-3 bg-linear-to-r from-red-600 to-red-700 text-white rounded-2xl font-semibold hover:from-red-700 hover:to-red-800 transition-all duration-300 shadow-lg hover:shadow-xl transform hover:-translate-y-0.5"
          >
            Try Again
          </button>
        </div>

        <!-- Products Display -->
        <div v-else-if="filteredProducts.length > 0">
          <!-- Grid View -->
          <div v-if="viewMode === 'grid'" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
            <div
                v-for="product in filteredProducts"
                :key="product.id"
                class="group bg-white rounded-3xl overflow-hidden shadow-md hover:shadow-2xl transition-all duration-500 border border-gray-100 transform hover:-translate-y-2"
            >
              <RouterLink :to="`/product/${product.id}`" class="block">
                <div class="relative overflow-hidden aspect-square bg-linear-to-br from-gray-100 to-gray-200">
                  <img
                      :src="product.image"
                      :alt="product.name"
                      class="w-full h-full object-cover group-hover:scale-110 transition-transform duration-700"
                  />

                  <!-- Badges -->
                  <div class="absolute top-4 left-4 flex flex-col gap-2">
                    <div v-if="product.badge" class="px-4 py-1.5 bg-linear-to-r from-violet-600 to-purple-600 text-white text-xs font-bold rounded-full shadow-lg backdrop-blur-sm">
                      {{ product.badge }}
                    </div>
                    <div v-if="!product.inStock" class="px-4 py-1.5 bg-red-500 text-white text-xs font-bold rounded-full shadow-lg">
                      Out of Stock
                    </div>
                  </div>

                  <!-- Wishlist Button -->
                  <div class="absolute top-4 right-4 opacity-0 group-hover:opacity-100 transition-all duration-300 transform scale-90 group-hover:scale-100">
                    <button
                        @click="(e) => handleAddToWishlist(product, e)"
                        class="w-11 h-11 bg-white/90 backdrop-blur-sm rounded-full flex items-center justify-center shadow-lg hover:bg-white transition-all duration-300 hover:scale-110"
                    >
                      <Heart class="w-5 h-5 text-gray-700 hover:text-red-500 transition-colors" />
                    </button>
                  </div>

                  <!-- Quick Add Overlay -->
                  <div class="absolute inset-x-0 bottom-0 p-4 bg-linear-to-t from-black/80 via-black/40 to-transparent opacity-0 group-hover:opacity-100 transition-all duration-500 transform translate-y-4 group-hover:translate-y-0">
                    <button
                        @click="(e) => addToCart(product.id, e)"
                        :disabled="!product.inStock"
                        class="w-full py-3 bg-white text-gray-900 font-bold rounded-xl hover:bg-gray-50 transition-all duration-300 flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed shadow-lg"
                    >
                      <ShoppingCart class="w-5 h-5" />
                      {{ product.inStock ? 'Quick Add' : 'Out of Stock' }}
                    </button>
                  </div>
                </div>
              </RouterLink>

              <!-- Product Info -->
              <div class="p-5 space-y-3">
                <RouterLink :to="`/product/${product.id}`" class="block">
                  <h3 class="font-bold text-gray-900 text-lg line-clamp-2 group-hover:text-violet-600 transition-colors leading-snug">
                    {{ product.name }}
                  </h3>
                </RouterLink>

                <div class="flex items-center gap-2">
                  <div class="flex items-center gap-0.5">
                    <Star
                        v-for="i in 5"
                        :key="i"
                        class="w-4 h-4"
                        :class="i <= Math.floor(product.rating) ? 'text-yellow-400 fill-yellow-400' : 'text-gray-300'"
                    />
                  </div>
                  <span class="text-sm text-gray-600 font-medium">({{ product.reviews }})</span>
                </div>

                <div class="flex items-center justify-between pt-2">
                  <div class="flex items-baseline gap-2">
                    <span class="text-2xl font-black text-gray-900">${{ product.price }}</span>
                    <span v-if="product.originalPrice" class="text-sm text-gray-400 line-through">${{ product.originalPrice }}</span>
                  </div>
                  <span class="text-xs font-semibold text-gray-600 bg-gray-100 px-3 py-1.5 rounded-full">
                    {{ product.category }}
                  </span>
                </div>
              </div>
            </div>
          </div>

          <!-- List View -->
          <div v-else class="space-y-6">
            <div
                v-for="product in filteredProducts"
                :key="product.id"
                class="group bg-white rounded-3xl overflow-hidden shadow-md hover:shadow-2xl transition-all duration-500 border border-gray-100"
            >
              <div class="flex flex-col md:flex-row">
                <RouterLink :to="`/product/${product.id}`" class="block md:w-80 shrink-0">
                  <div class="relative w-full h-64 md:h-full overflow-hidden bg-linear-to-br from-gray-100 to-gray-200">
                    <img
                        :src="product.image"
                        :alt="product.name"
                        class="w-full h-full object-cover group-hover:scale-110 transition-transform duration-700"
                    />
                    <div v-if="product.badge" class="absolute top-4 left-4 px-4 py-1.5 bg-linear-to-r from-violet-600 to-purple-600 text-white text-xs font-bold rounded-full shadow-lg">
                      {{ product.badge }}
                    </div>
                  </div>
                </RouterLink>

                <div class="flex-1 p-8 flex flex-col justify-between">
                  <div class="space-y-4">
                    <div class="flex items-start justify-between gap-4">
                      <RouterLink :to="`/product/${product.id}`">
                        <h3 class="text-3xl font-bold text-gray-900 group-hover:text-violet-600 transition-colors">
                          {{ product.name }}
                        </h3>
                      </RouterLink>
                      <button
                          @click="(e) => handleAddToWishlist(product, e)"
                          class="w-12 h-12 rounded-2xl flex items-center justify-center hover:bg-gray-100 transition-all duration-300 shrink-0"
                      >
                        <Heart class="w-6 h-6 text-gray-700 hover:text-red-500 transition-colors" />
                      </button>
                    </div>

                    <div class="flex items-center gap-4 flex-wrap">
                      <div class="flex items-center gap-1.5">
                        <Star
                            v-for="i in 5"
                            :key="i"
                            class="w-5 h-5"
                            :class="i <= Math.floor(product.rating) ? 'text-yellow-400 fill-yellow-400' : 'text-gray-300'"
                        />
                      </div>
                      <span class="text-sm text-gray-600 font-medium">({{ product.reviews }} reviews)</span>
                      <span class="text-sm font-semibold text-gray-600 bg-gray-100 px-4 py-1.5 rounded-full ml-auto">
                                                {{ product.category }}
                                            </span>
                    </div>

                    <p class="text-gray-600 leading-relaxed">
                      Experience premium quality with this exceptional product. Perfect for everyday use with meticulous attention to detail and superior craftsmanship.
                    </p>
                  </div>

                  <div class="flex items-center justify-between pt-6 border-t border-gray-100 mt-6">
                    <div class="flex items-baseline gap-3">
                      <span class="text-4xl font-black text-gray-900">${{ product.price }}</span>
                      <span v-if="product.originalPrice" class="text-lg text-gray-400 line-through">${{ product.originalPrice }}</span>
                      <span v-if="!product.inStock" class="text-sm text-red-500 font-bold ml-2">
                                                Out of Stock
                                            </span>
                    </div>
                    <button
                        @click="(e) => addToCart(product.id, e)"
                        :disabled="!product.inStock"
                        class="px-8 py-4 bg-linear-to-r from-violet-600 to-purple-600 text-white font-bold rounded-2xl hover:shadow-2xl hover:shadow-violet-600/50 transition-all duration-300 flex items-center gap-3 disabled:opacity-50 disabled:cursor-not-allowed transform hover:-translate-y-1"
                    >
                      <ShoppingCart class="w-5 h-5" />
                      {{ product.inStock ? 'Add to Cart' : 'Unavailable' }}
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- No Results -->
        <div v-else class="text-center py-32">
          <div class="inline-flex items-center justify-center w-24 h-24 bg-linear-to-br from-gray-100 to-gray-200 rounded-full mb-8 shadow-lg">
            <Search class="w-12 h-12 text-gray-400" />
          </div>
          <h3 class="text-3xl font-bold text-gray-900 mb-3">No products found</h3>
          <p class="text-gray-600 text-lg mb-8">Try adjusting your filters or search terms to find what you're looking for</p>
          <button
              @click="clearFilters"
              class="px-8 py-4 bg-linear-to-r from-violet-600 to-purple-600 text-white font-bold rounded-2xl hover:shadow-2xl hover:shadow-violet-600/50 transition-all duration-300 transform hover:-translate-y-1"
          >
            Clear All Filters
          </button>
        </div>
      </div>
    </section>
  </div>
</template>

<style scoped>
.slide-fade-enter-active {
  transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
}

.slide-fade-leave-active {
  transition: all 0.3s cubic-bezier(0.4, 0, 1, 1);
}

.slide-fade-enter-from,
.slide-fade-leave-to {
  transform: translateY(-20px);
  opacity: 0;
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity;
}
</style>