<script setup>
import { ref, computed, onMounted } from 'vue';
import { RouterLink, useRouter } from 'vue-router';
import {
  Search, Grid3x3, List, Star, Heart, ShoppingCart,
  ChevronDown, ChevronLeft, ChevronRight, X, Filter,
  Loader2, AlertCircle, SlidersHorizontal
} from 'lucide-vue-next';
import { useWishlistStore } from '../stores/Wishlist.js';
import { productAPI } from '../api/productsApi';
import { cartApi } from '../api/cartApi';

const wishlistStore = useWishlistStore();
const router = useRouter();

// State
const viewMode = ref('grid');
const searchQuery = ref('');
const selectedCategory = ref(null);
const sortBy = ref('featured');
const loading = ref(false);
const error = ref(null);
const allProducts = ref([]);
const categories = ref([]);

// Pagination
const currentPage = ref(1);
const pageSize = ref(12);
const totalPages = ref(1);
const totalItems = ref(0);

const sortOptions = [
  { value: 'featured', label: 'Recommended' },
  { value: 'price-low', label: 'Price: Low to High' },
  { value: 'price-high', label: 'Price: High to Low' },
  { value: 'newest', label: 'Newest' },
];

// Computed
const filteredProducts = computed(() => {
  let filtered = [...allProducts.value];
  if (searchQuery.value) {
    filtered = filtered.filter(p =>
      p.name.toLowerCase().includes(searchQuery.value.toLowerCase())
    );
  }
  if (selectedCategory.value) {
    filtered = filtered.filter(p => p.category === selectedCategory.value);
  }
  if (sortBy.value === 'price-low') filtered.sort((a, b) => a.price - b.price);
  else if (sortBy.value === 'price-high') filtered.sort((a, b) => b.price - a.price);
  else if (sortBy.value === 'newest') filtered.sort((a, b) => b.id - a.id);
  return filtered;
});

const startItem = computed(() => (currentPage.value - 1) * pageSize.value + 1);
const endItem = computed(() => Math.min(currentPage.value * pageSize.value, totalItems.value));

// Methods
const handleAddToWishlist = async (product, event) => {
  event.preventDefault();
  event.stopPropagation();
  if (wishlistStore.isInWishlist(product.id)) {
    await wishlistStore.removeFromWishlist(product.id);
  } else {
    await wishlistStore.addToWishlist(product);
  }
};

const addToCart = async (productId, event) => {
  event.preventDefault();
  event.stopPropagation();
  try {
    await cartApi.addToCart(productId, 1);
    router.push('/cart');
  } catch (err) {
    console.error('Failed to add to cart', err);
  }
};

const clearAllFilters = () => {
  selectedCategory.value = null;
  searchQuery.value = '';
};

const fetchProducts = async () => {
  try {
    loading.value = true;
    error.value = null;
    const response = await productAPI.getAllProducts({
      page: currentPage.value,
      pageSize: pageSize.value,
      isActive: true
    });

    const data = response.data;
    const productsData = data?.items || data || [];
    totalPages.value = data?.totalPages || 1;
    totalItems.value = data?.totalItems || productsData.length;

    // Build categories from real data
    const catMap = {};
    productsData.forEach(p => {
      const catName = p.categories?.[0]?.categoryName || 'Uncategorized';
      catMap[catName] = (catMap[catName] || 0) + 1;
    });
    categories.value = Object.entries(catMap).map(([name, count]) => ({ name, count }));

    allProducts.value = productsData.map((p, i) => ({
      id: p.productId,
      name: p.productName,
      price: p.basePrice,
      discountPrice: p.variants?.[0]?.discountPrice || null,
      category: p.categories?.[0]?.categoryName || 'Uncategorized',
      badge: p.featured ? 'New' : (p.variants?.[0]?.discountPrice ? 'Sale' : null),
      rating: 4.0 + (Math.random() * 0.9),
      reviews: Math.floor(Math.random() * 500) + 10,
      image: p.images?.[0]?.imageUrl || 'https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=500&h=500&fit=crop',
      brand: p.supplier || 'Brand',
      inStock: p.stock > 0,
      stock: p.stock
    }));

  } catch (err) {
    console.error('Error fetching products:', err);
    error.value = 'Failed to load products';
  } finally {
    loading.value = false;
  }
};

const goToPage = (page) => {
  if (page >= 1 && page <= totalPages.value) {
    currentPage.value = page;
    fetchProducts();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
};

const paginationPages = computed(() => {
  const pages = [];
  const total = totalPages.value;
  const current = currentPage.value;
  if (total <= 7) {
    for (let i = 1; i <= total; i++) pages.push(i);
  } else {
    pages.push(1);
    if (current > 3) pages.push('...');
    for (let i = Math.max(2, current - 1); i <= Math.min(total - 1, current + 1); i++) {
      pages.push(i);
    }
    if (current < total - 2) pages.push('...');
    pages.push(total);
  }
  return pages;
});

onMounted(() => {
  fetchProducts();
});
</script>

<template>
  <div class="min-h-screen bg-gray-50/50 font-sans text-gray-900 pb-20">
    <!-- Breadcrumbs -->
    <div class="max-w-[1400px] mx-auto px-4 sm:px-6 lg:px-8 pt-6 pb-2">
      <nav class="flex items-center text-sm text-gray-400">
        <RouterLink to="/" class="hover:text-gray-900 transition-colors">Home</RouterLink>
        <ChevronRight class="w-3.5 h-3.5 mx-1.5" />
        <span class="font-semibold text-gray-900">Shop</span>
      </nav>
    </div>

    <div class="max-w-[1400px] mx-auto px-4 sm:px-6 lg:px-8 flex gap-8">
      <!-- Sidebar -->
      <aside class="w-[260px] shrink-0 hidden lg:block sticky top-24 h-fit">
        <div class="bg-white rounded-xl border border-gray-100 p-5">
          <!-- Header -->
          <div class="flex items-center gap-2.5 pb-4 mb-5 border-b border-gray-100">
            <SlidersHorizontal class="w-[18px] h-[18px] text-gray-400" />
            <h3 class="text-xs font-bold uppercase tracking-widest text-gray-900">Filters</h3>
          </div>

          <!-- Search -->
          <div class="relative mb-6">
            <Search class="w-4 h-4 absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
            <input v-model="searchQuery" type="text" placeholder="Search products..."
              class="w-full pl-9 pr-3 py-2.5 text-sm bg-gray-50 border border-gray-100 rounded-lg outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-500/10 transition-all" />
          </div>

          <!-- Categories -->
          <div class="mb-6">
            <h4 class="text-[11px] font-bold uppercase tracking-widest text-gray-400 mb-3">Categories</h4>
            <ul class="space-y-0.5">
              <li @click="selectedCategory = null"
                class="flex justify-between items-center px-3 py-2.5 rounded-lg cursor-pointer text-sm transition-all"
                :class="!selectedCategory ? 'bg-blue-50 text-blue-600 font-semibold' : 'text-gray-500 hover:bg-gray-50 hover:text-gray-900'">
                <span>All Products</span>
                <span class="text-[11px] px-2 py-0.5 rounded-full"
                  :class="!selectedCategory ? 'bg-blue-100/70 text-blue-600' : 'bg-gray-100 text-gray-400'">
                  {{ totalItems }}
                </span>
              </li>
              <li v-for="cat in categories" :key="cat.name" @click="selectedCategory = cat.name"
                class="flex justify-between items-center px-3 py-2.5 rounded-lg cursor-pointer text-sm transition-all"
                :class="selectedCategory === cat.name ? 'bg-blue-50 text-blue-600 font-semibold' : 'text-gray-500 hover:bg-gray-50 hover:text-gray-900'">
                <span>{{ cat.name }}</span>
                <span class="text-[11px] px-2 py-0.5 rounded-full"
                  :class="selectedCategory === cat.name ? 'bg-blue-100/70 text-blue-600' : 'bg-gray-100 text-gray-400'">
                  {{ cat.count }}
                </span>
              </li>
            </ul>
          </div>

          <!-- Clear -->
          <button @click="clearAllFilters"
            class="flex items-center gap-1.5 text-xs text-gray-400 hover:text-red-500 transition-colors cursor-pointer">
            <X class="w-3.5 h-3.5" />
            Clear all filters
          </button>
        </div>
      </aside>

      <!-- Main -->
      <main class="flex-1 min-w-0">
        <!-- Header -->
        <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-7">
          <div>
            <h1 class="text-2xl sm:text-3xl font-extrabold text-gray-900 tracking-tight">
              {{ selectedCategory || 'All Products' }}
            </h1>
            <p class="text-sm text-gray-400 mt-1">Showing {{ startItem }}-{{ endItem }} of {{ totalItems }} results</p>
          </div>
          <div class="flex items-center gap-3">
            <!-- Sort -->
            <div class="relative">
              <select v-model="sortBy"
                class="appearance-none bg-white border border-gray-100 text-gray-600 text-sm font-medium pl-3.5 pr-9 py-2.5 rounded-lg outline-none hover:border-gray-200 focus:border-blue-500 transition-colors cursor-pointer">
                <option v-for="opt in sortOptions" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
              </select>
              <ChevronDown
                class="absolute right-3 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-gray-400 pointer-events-none" />
            </div>
            <!-- View -->
            <div class="flex bg-white border border-gray-100 rounded-lg p-0.5">
              <button @click="viewMode = 'grid'" class="p-2 rounded-md transition-all"
                :class="viewMode === 'grid' ? 'bg-blue-600 text-white shadow-md shadow-blue-600/30' : 'text-gray-400 hover:text-gray-600'">
                <Grid3x3 class="w-[18px] h-[18px]" />
              </button>
              <button @click="viewMode = 'list'" class="p-2 rounded-md transition-all"
                :class="viewMode === 'list' ? 'bg-blue-600 text-white shadow-md shadow-blue-600/30' : 'text-gray-400 hover:text-gray-600'">
                <List class="w-[18px] h-[18px]" />
              </button>
            </div>
          </div>
        </div>

        <!-- Loading -->
        <div v-if="loading" class="flex flex-col items-center justify-center py-24 text-gray-400">
          <div class="w-10 h-10 border-[3px] border-gray-100 border-t-blue-600 rounded-full animate-spin mb-4"></div>
          <p class="text-sm">Loading products...</p>
        </div>

        <!-- Error -->
        <div v-else-if="error" class="flex flex-col items-center justify-center py-24 text-gray-400">
          <AlertCircle class="w-12 h-12 mb-4" />
          <p class="text-sm mb-4">{{ error }}</p>
          <button @click="fetchProducts" class="px-6 py-2.5 bg-blue-600 text-white text-sm font-semibold rounded-lg hover:bg-blue-700 transition-colors">
            Try Again
          </button>
        </div>

        <!-- Product Grid -->
        <div v-else-if="filteredProducts.length > 0" :class="viewMode === 'grid' ? 'grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-3 gap-4 sm:gap-6' : 'flex flex-col gap-4'">
          <div v-for="product in filteredProducts" :key="product.id"
            class="group bg-white rounded-xl border border-transparent hover:border-gray-200 hover:shadow-xl hover:-translate-y-1 transition-all duration-300">
            <RouterLink :to="`/product/${product.id}`" class="block" :class="viewMode === 'list' ? 'flex' : ''">
              <!-- Image -->
              <div class="relative bg-gray-50 overflow-hidden rounded-t-xl"
                :class="viewMode === 'list' ? 'w-[200px] h-[200px] shrink-0 rounded-l-xl rounded-tr-none' : 'aspect-square'">
                <img :src="product.image" :alt="product.name" class="w-full h-full object-contain p-6 group-hover:scale-105 transition-transform duration-500 mix-blend-multiply" />
                <!-- Badge -->
                <span v-if="product.badge" class="absolute top-3 left-3 px-2.5 py-1 text-[10px] font-bold uppercase tracking-wider text-white rounded-md" :class="product.badge === 'New' ? 'bg-blue-600' : 'bg-gray-900'">
                  {{ product.badge }}
                </span>
                <!-- Quick Actions -->
                <div class="absolute bottom-3 right-3 flex flex-col gap-2 opacity-0 translate-y-2 group-hover:opacity-100 group-hover:translate-y-0 transition-all duration-300">
                  <button @click="handleAddToWishlist(product, $event)" class="w-9 h-9 rounded-full shadow-md flex items-center justify-center transition-colors cursor-pointer"
                    :class="wishlistStore.isInWishlist(product.id) ? 'bg-red-50 text-red-500' : 'bg-white text-gray-400 hover:text-red-500 hover:bg-red-50'">
                    <Heart class="w-4 h-4" :class="{ 'fill-current': wishlistStore.isInWishlist(product.id) }" />
                  </button>
                  <button @click="addToCart(product.id, $event)"
                    class="w-9 h-9 rounded-full bg-blue-600 shadow-md shadow-blue-600/30 flex items-center justify-center text-white hover:bg-blue-700 hover:scale-110 transition-all cursor-pointer">
                    <ShoppingCart class="w-4 h-4" />
                  </button>
                </div>
              </div>
              <!-- Content -->
              <div class="p-3.5 sm:p-4" :class="viewMode === 'list' ? 'flex flex-col justify-center' : ''">
                <p class="text-[10px] font-semibold uppercase tracking-widest text-gray-400 mb-1">{{ product.brand }}
                </p>
                <h3 class="font-bold text-gray-900 text-sm sm:text-[15px] mb-2 leading-snug group-hover:text-blue-600 transition-colors line-clamp-2">
                  {{ product.name }}
                </h3>
                <div class="flex items-center gap-1.5 mb-2">
                  <div class="flex">
                    <Star v-for="s in 5" :key="s" class="w-3 h-3" :class="s <= Math.floor(product.rating) ? 'text-yellow-400 fill-yellow-400' : 'text-gray-200'" />
                  </div>
                  <span class="text-[10px] text-gray-400">({{ product.reviews }})</span>
                </div>
                <div class="flex items-center gap-2">
                  <span class="text-base font-extrabold text-gray-900">${{ product.price?.toFixed(2) }}</span>
                  <span v-if="product.discountPrice" class="text-xs text-gray-400 line-through">
                    ${{ (product.price * 1.2).toFixed(2) }}
                  </span>
                </div>
                <span v-if="!product.inStock" class="inline-block mt-2 text-[10px] font-semibold text-red-500 bg-red-50 px-2 py-0.5 rounded-md">
                  Out of Stock
                </span>
              </div>
            </RouterLink>
          </div>
        </div>

        <!-- Empty -->
        <div v-else class="flex flex-col items-center justify-center py-24 text-gray-400">
          <Search class="w-12 h-12 mb-4" />
          <h3 class="font-bold text-gray-900 text-lg mb-1">No products found</h3>
          <p class="text-sm mb-4">Try adjusting your filters or search query</p>
          <button @click="clearAllFilters" class="px-6 py-2.5 bg-blue-600 text-white text-sm font-semibold rounded-lg hover:bg-blue-700 transition-colors">
            Clear Filters
          </button>
        </div>

        <!-- Pagination -->
        <div v-if="totalPages > 1 && !loading" class="flex justify-center items-center gap-1.5 mt-12">
          <button @click="goToPage(currentPage - 1)" :disabled="currentPage === 1"
            class="w-10 h-10 flex items-center justify-center border border-gray-100 bg-white rounded-lg text-gray-400 hover:border-blue-500 hover:text-blue-600 transition-all disabled:opacity-30 disabled:cursor-not-allowed">
            <ChevronLeft class="w-4 h-4" />
          </button>
          <template v-for="page in paginationPages">
            <span v-if="page === '...'" :key="'dots-' + page" class="w-10 text-center text-gray-400 text-sm">...</span>
            <button v-else :key="page" @click="goToPage(page)"
              class="w-10 h-10 flex items-center justify-center rounded-lg text-sm font-semibold transition-all" :class="page === currentPage
                ? 'bg-blue-600 text-white shadow-md shadow-blue-600/30'
                : 'border border-gray-100 bg-white text-gray-500 hover:border-blue-500 hover:text-blue-600'">
              {{ page }}
            </button>
          </template>
          <button @click="goToPage(currentPage + 1)" :disabled="currentPage === totalPages"
            class="w-10 h-10 flex items-center justify-center border border-gray-100 bg-white rounded-lg text-gray-400 hover:border-blue-500 hover:text-blue-600 transition-all disabled:opacity-30 disabled:cursor-not-allowed">
            <ChevronRight class="w-4 h-4" />
          </button>
        </div>
      </main>
    </div>
  </div>
</template>