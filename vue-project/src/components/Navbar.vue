<script setup>
import { ref, computed, onMounted, onUnmounted, watch } from 'vue';
import '../assets/Navbar.css';
import { RouterLink, useRoute, useRouter } from 'vue-router';
import { ShoppingCart, Search, User, Heart, Menu, LogOut, Settings, ChevronDown } from 'lucide-vue-next';
import Icons from '../assets/icons/icons.vue';
import { useAuthStore } from '../stores/Auth';

const route = useRoute();
const router = useRouter();
const authStore = useAuthStore();

const isMenuOpen = ref(false);
const isSearchOpen = ref(false);
const isProfileOpen = ref(false);
const scrolled = ref(false);
const cartCount = ref(''); // Example cart count

const toggleMenu = () => {
  isMenuOpen.value = !isMenuOpen.value;
  document.body.style.overflow = isMenuOpen.value ? 'hidden' : '';
};

const closeMenu = () => {
  isMenuOpen.value = false;
  document.body.style.overflow = '';
};

const toggleSearch = () => {
  isSearchOpen.value = !isSearchOpen.value;
};

const toggleProfile = () => {
  isProfileOpen.value = !isProfileOpen.value;
};

const logout = () => {
  authStore.logout();
  isProfileOpen.value = false;
  closeMenu();
  router.push('/login');
};

const Navlinks = ref([
  { to: '/', label: 'Home',icon: 'Home', ariaLabel: 'Home' },
  { to: '/product', label: 'Product', icon: 'Box', ariaLabel: 'Browse Product' },
  { to: '/contact', label: 'Contact', icon: 'Phone', ariaLabel: 'Contact' },
  { to: '/about', label: 'About', icon: 'Info', ariaLabel: 'About' },
]);

const isActiveRoute = computed(() => (linkTo) =>
  route.path === linkTo || route.path.startsWith(linkTo + '/')
);

const handleScroll = () => {
  scrolled.value = window.scrollY > 20;
};

const handleEscapeKey = (event) => {
  if (event.key === 'Escape') {
    if (isMenuOpen.value) closeMenu();
    if (isSearchOpen.value) isSearchOpen.value = false;
    if (isProfileOpen.value) isProfileOpen.value = false;
  }
};

// Close profile dropdown when clicking outside
const closeOnClickOutside = (event) => {
    if (isProfileOpen.value && !event.target.closest('.profile-dropdown-container')) {
        isProfileOpen.value = false;
    }
};

onMounted(() => {
    authStore.initializeAuth(); // Ensure auth state is restored
    document.addEventListener('keydown', handleEscapeKey);
    document.addEventListener('click', closeOnClickOutside);
    window.addEventListener('scroll', handleScroll);
});

onUnmounted(() => {
    document.removeEventListener('keydown', handleEscapeKey);
    document.removeEventListener('click', closeOnClickOutside);
    window.removeEventListener('scroll', handleScroll);
    document.body.style.overflow = '';
});

watch(route, () => {
  closeMenu();
  isProfileOpen.value = false;
});

// Computed properties for user display
const userDisplayName = computed(() => {
    if (!authStore.user) return 'User';
    return authStore.user.firstName || authStore.user.username || 'User';
});

const userAvatarInitial = computed(() => {
    const name = userDisplayName.value;
    return name ? name.charAt(0).toUpperCase() : 'U';
});

</script>

<template>
  <header class="fixed top-0 left-0 right-0 z-50 transition-all duration-500" :class="scrolled ? 'py-3' : 'py-5'"
    role="banner">
    <!-- Refined Background -->
    <div class="absolute inset-0 transition-all duration-500" 
      :class="scrolled ? 'bg-white/90 backdrop-blur-xl border-b border-gray-100 shadow-sm' : 'bg-transparent'"></div>
    <div class="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="flex justify-between items-center">
        <!-- Logo Section -->
        <RouterLink to="/" class="flex items-center gap-2 group transition-transform duration-300 hover:scale-[1.02]" aria-label="AlieeShop Home">
          <span class="text-2xl tracking-tight text-gray-900">
            <span class="font-light">Aliee</span><span class="font-black italic">Shop</span>
          </span>
        </RouterLink>
        <!-- Desktop Navigation -->
        <nav class="hidden lg:flex items-center bg-gray-50/50 backdrop-blur-md border border-gray-100 rounded-full px-2 py-1" role="navigation" aria-label="Main navigation">
          <RouterLink v-for="link in Navlinks" :key="link.label" :to="link.to"
            class="relative px-6 py-2 rounded-full text-gray-500 font-medium text-sm transition-all duration-300 group overflow-hidden"
            :class="isActiveRoute(link.to) ? 'text-gray-900' : 'hover:text-gray-900'" :aria-label="link.ariaLabel">
            <span class="relative z-10">{{ link.label }}</span>
            <!-- Active Indicator -->
            <div v-if="isActiveRoute(link.to)"
              class="absolute inset-0 bg-white shadow-sm rounded-full"></div>
            <!-- Hover Slide Effect -->
            <div class="absolute inset-0 bg-white/50 translate-y-full group-hover:translate-y-0 transition-transform duration-300 rounded-full"></div>
          </RouterLink>
        </nav>
        <!-- Desktop Actions -->
        <div class="hidden lg:flex items-center gap-2">
          <!-- Search Button -->
          <button @click="toggleSearch" class="p-3 rounded-full hover:bg-gray-100 transition-all duration-300 text-gray-600 hover:text-gray-900"
            aria-label="Search">
            <Search class="w-5 h-5" />
          </button>
          <!-- Wishlist Button -->
          <RouterLink to="/wishlist" class="p-3 rounded-full hover:bg-gray-100 transition-all duration-300 text-gray-600 hover:text-gray-900" aria-label="Wishlist">
            <Heart class="w-5 h-5" />
          </RouterLink>
          <!-- Cart Button -->
          <RouterLink to="/cart" class="relative p-3 rounded-full hover:bg-gray-100 transition-all duration-300 text-gray-600 hover:text-gray-900"
            aria-label="Shopping cart">
            <ShoppingCart class="w-5 h-5" />
            <span v-if="cartCount > 0"
              class="absolute top-2 right-2 w-4 h-4 bg-gray-900 text-white text-[10px] font-bold rounded-full flex items-center justify-center">
              {{ cartCount }}
            </span>
          </RouterLink>
          <!-- Vertical Divider -->
          <div class="w-px h-4 bg-gray-200 mx-2"></div>
          
          <!-- Auth Section -->
          <div v-if="authStore.isAuthenticated" class="relative profile-dropdown-container">
             <button @click="toggleProfile" class="flex items-center gap-2 pl-2 pr-1 py-1 rounded-full hover:bg-gray-100 transition-all duration-300 border border-transparent hover:border-gray-200">
                <div class="w-8 h-8 rounded-full bg-gray-900 text-white flex items-center justify-center text-sm font-bold shadow-md">
                    {{ userAvatarInitial }}
                </div>
                <ChevronDown class="w-4 h-4 text-gray-500 transition-transform duration-300" :class="{ 'rotate-180': isProfileOpen }" />
             </button>

             <!-- Profile Dropdown -->
             <transition name="slide-fade">
                <div v-if="isProfileOpen" class="absolute top-full right-0 mt-3 w-60 bg-white rounded-2xl shadow-xl border border-gray-100 p-2 overflow-hidden transform origin-top-right">
                    <div class="px-4 py-3 border-b border-gray-50 mb-2 bg-gray-50/50 rounded-xl">
                        <p class="text-xs text-gray-500 font-bold uppercase tracking-wider">Signed in as</p>
                        <p class="text-sm font-bold text-gray-900 truncate">{{ userDisplayName }}</p>
                         <p class="text-xs text-gray-500 truncate">{{ authStore.user?.email }}</p>
                    </div>
                    
                    <RouterLink v-if="authStore.userRole === 'admin'" to="/admin/dashboard" class="flex items-center gap-3 px-4 py-2 text-sm font-medium text-gray-700 rounded-xl hover:bg-gray-50 hover:text-gray-900 transition-colors">
                        <Settings class="w-4 h-4" />
                        Admin Dashboard
                    </RouterLink>
                    
                    <button @click="logout" class="w-full flex items-center gap-3 px-4 py-2 text-sm font-medium text-red-600 rounded-xl hover:bg-red-50 transition-colors text-left">
                        <LogOut class="w-4 h-4" />
                        Sign Out
                    </button>
                </div>
             </transition>
          </div>

          <!-- Login Button (If Not Authenticated) -->
          <RouterLink v-else to="/login"
            class="ml-2 px-8 py-3 text-sm font-bold text-white bg-gray-800 rounded-full hover:bg-black transition-all duration-300 shadow-lg hover:shadow-violet-200 flex items-center gap-2 group">
            <User class="w-4 h-4 group-hover:scale-110 transition-transform" />
            Sign In
          </RouterLink>
        </div>

        <!-- Mobile Menu Button -->
        <button @click="toggleMenu" class="lg:hidden p-3 rounded-full bg-gray-50 hover:bg-gray-100 transition-all duration-300"
          :aria-expanded="isMenuOpen" aria-controls="mobile-menu" aria-label="Toggle menu">
          <Menu class="w-6 h-6 text-gray-900" />
        </button>
      </div>
      <!-- Search Bar (Desktop) -->
      <transition name="slide-fade">
        <div v-if="isSearchOpen" class="hidden lg:block px-4 sm:px-6 lg:px-8 pb-4">
          <div class="relative max-w-2xl mx-auto">
            <input type="text" placeholder="Search products, categories, brands..."
              class="w-full px-5 py-3 pl-12 pr-4 rounded-xl border-2 border-gray-200 focus:border-violet-600 focus:outline-none transition-colors bg-white/80 backdrop-blur-sm"
              autofocus />
            <Search class="absolute left-4 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" />
          </div>
        </div>
      </transition>
    </div>
    <!-- Mobile Menu -->
    <transition name="mobile-menu">
      <div v-if="isMenuOpen" id="mobile-menu" class="lg:hidden fixed inset-0 z-50" aria-label="Mobile navigation"
        role="region">
        <!-- Backdrop -->
        <div class="absolute inset-0 bg-black/20 backdrop-blur-sm" @click="closeMenu" aria-hidden="true"></div>
        <!-- Sidebar -->
        <div class="absolute top-0 right-0 w-80 h-full bg-white shadow-2xl overflow-y-auto flex flex-col">
          <!-- Header -->
          <div
            class="flex justify-between items-center p-6 border-b border-gray-50">
            <div class="flex items-center gap-2">
              <span class="text-xl font-light text-gray-900">Aliee<span class="font-bold italic">Shop</span></span>
            </div>
            <button @click="closeMenu" class="p-2 rounded-full hover:bg-gray-50 transition-colors"
              aria-label="Close navigation menu">
              <Icons name="X" class="w-6 h-6 text-gray-800" />
            </button>
          </div>

          <!-- User Info (Mobile) -->
          <div v-if="authStore.isAuthenticated" class="p-6 bg-gray-50">
             <div class="flex items-center gap-4">
                 <div class="w-12 h-12 rounded-full bg-gray-900 text-white flex items-center justify-center text-lg font-bold shadow-md">
                    {{ userAvatarInitial }}
                </div>
                <div>
                     <p class="font-bold text-gray-900">{{ userDisplayName }}</p>
                     <p class="text-xs text-gray-500">{{ authStore.user?.email }}</p>
                </div>
             </div>
          </div>

          <!-- Mobile Search -->
          <div class="p-6">
            <div class="relative">
              <input type="text" placeholder="Search products..."
                class="w-full px-4 py-3 pl-11 rounded-2xl bg-gray-50 border-0 focus:ring-2 focus:ring-gray-900 focus:outline-none transition-all" />
              <Search class="absolute left-4 top-1/2 transform -translate-y-1/2 w-4 h-4 text-gray-400" />
            </div>
          </div>
          <!-- Navigation Links -->
          <nav class="p-6 pt-0 flex-1">
            <ul class="space-y-2">
              <li v-for="link in Navlinks" :key="link.label + '-mobile'">
                <RouterLink :to="link.to" class="flex items-center gap-4 p-4 rounded-2xl transition-all duration-300 group"
                  :class="isActiveRoute(link.to)
                    ? 'bg-gray-900 text-white shadow-xl'
                    : 'text-gray-600 hover:bg-gray-50'" @click="closeMenu" :aria-label="link.ariaLabel">
                  <div class="w-10 h-10 rounded-xl flex items-center justify-center transition-colors" :class="isActiveRoute(link.to)
                    ? 'bg-white/10'
                    : 'bg-gray-100 group-hover:bg-white'">
                    <Icons :name="link.icon" class="w-5 h-5" />
                  </div>
                  <span class="font-bold">{{ link.label }}</span>
                </RouterLink>
              </li>
              
               <li v-if="authStore.isAuthenticated && authStore.userRole === 'admin'">
                <RouterLink to="/admin/dashboard" class="flex items-center gap-4 p-4 rounded-2xl text-gray-600 hover:bg-gray-50 transition-colors" @click="closeMenu">
                  <div class="w-10 h-10 rounded-xl bg-gray-100 flex items-center justify-center">
                    <Settings class="w-5 h-5" />
                  </div>
                  <span class="font-bold">Admin Dashboard</span>
                </RouterLink>
              </li>
            </ul>
          </nav>
          <!-- Mobile Actions -->
          <div class="p-6 mt-auto space-y-4 border-t border-gray-50">
            <!-- Quick Actions -->
            <div class="grid grid-cols-2 gap-3">
              <RouterLink to="/wishlist" @click="closeMenu"
                class="flex flex-col items-center gap-2 p-4 rounded-2xl bg-gray-50 hover:bg-gray-100 transition-colors">
                <Heart class="w-5 h-5 text-gray-700" />
                <span class="text-[10px] font-bold uppercase tracking-widest text-gray-500">Wishlist</span>
              </RouterLink>
              <RouterLink to="/cart" @click="closeMenu"
                class="flex flex-col items-center gap-2 p-4 rounded-2xl bg-gray-50 hover:bg-gray-100 transition-colors relative">
                <ShoppingCart class="w-5 h-5 text-gray-700" />
                <span class="text-[10px] font-bold uppercase tracking-widest text-gray-500">Cart</span>
                <span v-if="cartCount > 0"
                  class="absolute top-3 right-6 w-4 h-4 bg-gray-900 text-white text-[10px] font-bold rounded-full flex items-center justify-center">
                  {{ cartCount }}
                </span>
              </RouterLink>
            </div>
            
            <!-- Login / Logout Button -->
            <button v-if="authStore.isAuthenticated" @click="logout"
              class="flex items-center justify-center gap-3 w-full px-6 py-4 text-sm font-bold text-red-600 bg-red-50 rounded-2xl hover:bg-red-100 transition-all duration-300">
              <LogOut class="w-5 h-5" />
              Sign Out
            </button>
            <RouterLink v-else to="/login" @click="closeMenu"
              class="flex items-center justify-center gap-3 w-full px-6 py-4 text-sm font-bold text-white bg-gray-900 rounded-2xl hover:bg-violet-600 transition-all duration-300 shadow-xl">
              <User class="w-5 h-5" />
              Sign In / Join Now
            </RouterLink>
          </div>
        </div>
      </div>
    </transition>
  </header>
</template>