<script setup>
import { ref, computed, onMounted, onUnmounted, watch } from 'vue';
import '../assets/Navbar.css';
import { RouterLink, useRoute, useRouter } from 'vue-router';
import { ShoppingCart, Search, User, Heart, Menu, LogOut, Settings, ChevronDown } from 'lucide-vue-next';
import Icons from '../assets/icons/icons.vue';
import { useAuthStore } from '../stores/Auth';
import { useWishlistStore } from '../stores/Wishlist';
import { profileApi } from '../api/profileApi';

const route = useRoute();
const router = useRouter();
const authStore = useAuthStore();
const wishlistStore = useWishlistStore();

const isMenuOpen = ref(false);
const isSearchOpen = ref(false);
const isProfileOpen = ref(false);
const scrolled = ref(false);
const cartCount = ref(''); // Example cart count
const userAvatar = ref(null);

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
  userAvatar.value = null;
  isProfileOpen.value = false;
  closeMenu();
  router.push('/login');
};

const Navlinks = ref([
  { to: '/', label: 'Home', icon: 'Home', ariaLabel: 'Home' },
  { to: '/product', label: 'Product', icon: 'Box', ariaLabel: 'Browse Product' },
  { to: '/about', label: 'About', icon: 'Info', ariaLabel: 'About' },
  { to: '/contact', label: 'Contact', icon: 'Phone', ariaLabel: 'Contact' },
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

// Fetch user profile to get avatar
const fetchUserProfile = async () => {
  if (!authStore.isAuthenticated) return;
  try {
    const response = await profileApi.getProfile();
    const data = response.data.data || response.data;
    userAvatar.value = data.avatar || null;
    // Update auth store with profile data
    if (authStore.user) {
      authStore.user.avatar = data.avatar;
      authStore.user.name = data.name;
    }
  } catch (err) {
    console.error('Failed to fetch profile for navbar:', err);
  }
};

onMounted(() => {
  authStore.initializeAuth(); // Ensure auth state is restored
  document.addEventListener('keydown', handleEscapeKey);
  document.addEventListener('click', closeOnClickOutside);
  window.addEventListener('scroll', handleScroll);
  fetchUserProfile();
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

// Re-fetch profile when auth state changes (e.g., after login)
watch(() => authStore.isAuthenticated, (isAuth) => {
  if (isAuth) fetchUserProfile();
  else userAvatar.value = null;
});

// Computed properties for user display
const userDisplayName = computed(() => {
  if (!authStore.user) return 'User';
  return authStore.user.name || authStore.user.firstName || authStore.user.username || 'User';
});

const userAvatarInitial = computed(() => {
  const name = userDisplayName.value;
  return name ? name.charAt(0).toUpperCase() : 'U';
});

</script>

<template>
  <header class="fixed top-0 left-0 right-0 z-50 bg-white transition-all duration-300"
    :class="scrolled ? 'shadow-sm py-2' : 'py-4'" role="banner">
    <div class="max-w-screen-2xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="flex items-center justify-between h-10">

        <!-- Logo (Left) -->
        <div class="flex-shrink-0 flex items-center">
          <RouterLink to="/" class="group" aria-label="AlieeShop Home">
            <span
              class="text-2xl font-black tracking-tighter text-gray-900 group-hover:opacity-80 transition-opacity">Aliee
              <span style="color: #C8A97E;">Shop</span>
            </span>
          </RouterLink>
        </div>

        <!-- Desktop Navigation (Center) -->
        <nav class="hidden md:flex space-x-8 lg:space-x-12" role="navigation" aria-label="Main navigation">
          <RouterLink v-for="link in Navlinks" :key="link.label" :to="link.to"
            class="text-sm font-medium text-gray-500 hover:text-gray-900 transition-colors"
            :class="isActiveRoute(link.to) ? 'text-gray-900' : ''">
            {{ link.label }}
          </RouterLink>
        </nav>

        <!-- Icons (Right) -->
        <div class="flex items-center space-x-6">
          <!-- Search -->
          <div class="relative hidden sm:flex items-center">
            <div class="relative">
              <input type="text" placeholder="Search..."
                class="bg-gray-100 rounded-full py-2 pl-4 pr-10 text-sm focus:outline-none focus:ring-1 focus:ring-gray-300 transition-all w-40 focus:w-60" />
              <Search
                class="w-4 h-4 text-gray-400 absolute right-3 top-1/2 transform -translate-y-1/2 pointer-events-none" />
            </div>
          </div>

          <RouterLink to="/wishlist"
            class="relative text-gray-900 hover:text-gray-600 transition-colors hidden sm:block">
            <Heart class="w-5 h-5" />
            <span v-if="wishlistStore.wishlistCount > 0"
              class="absolute -top-2 -right-2 min-w-[18px] h-[18px] flex items-center justify-center text-[10px] font-bold text-white rounded-full"
              style="background-color: #C8A97E;">{{ wishlistStore.wishlistCount }}</span>
          </RouterLink>

          <!-- Cart -->
          <RouterLink to="/cart" class="relative group text-gray-900 hover:text-gray-600 transition-colors">
            <div class="relative">
              <ShoppingCart class="w-5 h-5" />
              <span v-if="cartCount > 0"
                class="absolute -top-1 -right-1 w-3 h-3 bg-red-500 text-white text-[9px] font-bold rounded-full flex items-center justify-center pointer-events-none">
                {{ cartCount }}
              </span>
            </div>
          </RouterLink>

          <!-- User / Profile -->
          <div v-if="authStore.isAuthenticated" class="relative profile-dropdown-container">
            <button @click="toggleProfile"
              class="flex items-center hover:opacity-80 transition-opacity focus:outline-none">
              <div class="navbar-avatar">
                <img v-if="userAvatar" :src="userAvatar" :alt="userDisplayName" class="navbar-avatar__img" />
                <span v-else class="navbar-avatar__initial">{{ userAvatarInitial }}</span>
              </div>
            </button>

            <!-- Profile Dropdown -->
            <transition name="fade">
              <div v-if="isProfileOpen"
                class="absolute top-full right-0 mt-2 w-56 bg-white rounded-xl shadow-xl border border-gray-100 py-1 z-50">
                <div class="px-4 py-3 border-b border-gray-100 flex items-center gap-3">
                  <div class="navbar-avatar navbar-avatar--sm">
                    <img v-if="userAvatar" :src="userAvatar" :alt="userDisplayName" class="navbar-avatar__img" />
                    <span v-else class="navbar-avatar__initial" style="font-size: 12px;">{{ userAvatarInitial }}</span>
                  </div>
                  <div class="min-w-0">
                    <p class="text-sm font-semibold text-gray-900 truncate">{{ userDisplayName }}</p>
                    <p class="text-xs text-gray-400 truncate">{{ authStore.user?.email }}</p>
                  </div>
                </div>

                <RouterLink to="/profile"
                  class="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-50 flex items-center gap-2">
                  <User class="w-4 h-4" /> My Profile
                </RouterLink>
                <RouterLink to="/settings"
                  class="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-50 flex items-center gap-2">
                  <Settings class="w-4 h-4" /> Settings
                </RouterLink>

                <div class="border-t border-gray-100 my-1"></div>

                <RouterLink v-if="authStore.userRole === 'admin'" to="/admin/dashboard"
                  class="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-50 font-bold text-blue-600">
                  Admin Dashboard
                </RouterLink>
                <button @click="logout" class="block w-full text-left px-4 py-2 text-sm text-red-600 hover:bg-gray-50">
                  Sign Out
                </button>
              </div>
            </transition>
          </div>
          <RouterLink v-else to="/login" class="text-gray-900 hover:text-gray-600 transition-colors">
            <User class="w-5 h-5" />
          </RouterLink>

          <!-- Mobile Menu Button -->
          <button @click="toggleMenu" class="md:hidden p-2 text-gray-900">
            <Menu class="w-6 h-6" />
          </button>
        </div>

      </div>
    </div>

    <!-- Mobile Menu -->
    <transition name="slide-fade">
      <div v-if="isMenuOpen"
        class="md:hidden absolute top-full left-0 right-0 bg-white border-b border-gray-100 shadow-lg px-4 py-4 z-40">
        <nav class="flex flex-col space-y-4">
          <RouterLink v-for="link in Navlinks" :key="link.label" :to="link.to"
            class="text-base font-medium text-gray-900" @click="closeMenu">
            {{ link.label }}
          </RouterLink>
          <hr />
          <p class="text-sm text-gray-500">Account</p>
          <RouterLink v-if="authStore.isAuthenticated" to="/profile"
            class="flex items-center gap-2 text-sm font-medium text-gray-900" @click="closeMenu">
            <User class="w-4 h-4" /> My Profile
          </RouterLink>
          <RouterLink v-if="authStore.isAuthenticated" to="/settings"
            class="flex items-center gap-2 text-sm font-medium text-gray-900" @click="closeMenu">
            <Settings class="w-4 h-4" /> Settings
          </RouterLink>
          <RouterLink to="/wishlist" class="flex items-center gap-2 text-sm font-medium text-gray-900"
            @click="closeMenu">
            <Heart class="w-4 h-4" /> Wishlist
          </RouterLink>
          <button v-if="authStore.isAuthenticated" @click="logout"
            class="flex items-center gap-2 text-sm font-medium text-red-600 text-left">
            <LogOut class="w-4 h-4" /> Sign Out
          </button>
          <RouterLink v-else to="/login" class="flex items-center gap-2 text-sm font-medium text-gray-900"
            @click="closeMenu">
            <User class="w-4 h-4" /> Sign In
          </RouterLink>
        </nav>
      </div>
    </transition>
  </header>
</template>