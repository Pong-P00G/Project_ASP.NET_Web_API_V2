<script setup>
import { ref, computed } from 'vue'
import { useToast } from '../composables/useToast'
import { useRouter, useRoute } from 'vue-router'
import {
  LayoutDashboard,
  Package,
  Users,
  Database,
  LogOut,
  Menu,
  X,
  Search,
  Bell,
  ShoppingBag,
  ChevronLeft,
  Sparkles,
  HelpCircle,
  ExternalLink,
  CheckCircle,
  AlertCircle,
  AlertTriangle,
  Info
} from 'lucide-vue-next'
import { useAuthStore } from '../stores/Auth'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

const isSidebarOpen = ref(true)
const isMobileSidebarOpen = ref(false)
const searchQuery = ref('')
const { toasts, remove: removeToast } = useToast()

const toastIcons = {
  success: CheckCircle,
  error: AlertCircle,
  warning: AlertTriangle,
  info: Info
}

const toastStyles = {
  success: 'bg-green-500',
  error: 'bg-red-500',
  warning: 'bg-orange-500',
  info: 'bg-blue-500'
}

const navigation = [
  { name: 'Dashboard', to: '/admin/dashboard', icon: LayoutDashboard },
  { name: 'Products', to: '/admin/product', icon: Package },
  { name: 'Users', to: '/admin/user', icon: Users },
  { name: 'Orders', to: '/admin/orders', icon: ShoppingBag },
  { name: 'Stock', to: '/admin/stock', icon: Database },
]

const currentPage = computed(() => {
  const found = navigation.find(item => route.path.startsWith(item.to))
  return found?.name || 'Dashboard'
})

const handleLogout = async () => {
  try {
    await authStore.logout()
    router.push('/login')
  } catch (error) {
    console.error('Logout failed:', error)
  }
}

const toggleSidebar = () => { isSidebarOpen.value = !isSidebarOpen.value }
const toggleMobileSidebar = () => { isMobileSidebarOpen.value = !isMobileSidebarOpen.value }
const closeMobileSidebar = () => { isMobileSidebarOpen.value = false }

const userInitials = computed(() => {
  const username = authStore.user?.username || 'Admin'
  return username.substring(0, 2).toUpperCase()
})
</script>

<template>
  <div class="min-h-screen bg-neutral-100 relative" style="font-family: 'Playfair Display', serif;">
    <!-- Black Header Background -->
    <div class="absolute top-0 left-0 right-0 h-80 bg-neutral-900 z-0"></div>

    <div class="flex relative z-10 min-h-screen">
      <!-- Mobile Overlay -->
      <Transition name="fade">
        <div v-if="isMobileSidebarOpen" class="fixed inset-0 bg-black/60 backdrop-blur-sm z-40 lg:hidden"
          @click="closeMobileSidebar"></div>
      </Transition>

      <!-- Sidebar -->
      <aside :class="[
        'fixed top-0 left-0 bottom-0 z-50 flex flex-col bg-white shadow-xl transition-all duration-300 ease-out',
        'lg:sticky lg:top-4 lg:left-0 lg:h-[calc(100vh-2rem)] lg:m-4 lg:rounded-2xl lg:translate-x-0',
        isSidebarOpen ? 'w-72' : 'w-22',
        isMobileSidebarOpen ? 'translate-x-0' : '-translate-x-full lg:translate-x-0'
      ]">
        <!-- Logo -->
        <div class="h-20 flex items-center justify-between px-5 border-b border-neutral-100 shrink-0">
          <div class="flex items-center gap-3">
            <div class="w-10 h-10 flex items-center justify-center bg-neutral-900 rounded-xl text-white shrink-0">
              <Sparkles class="w-5 h-5" />
            </div>
            <Transition name="fade">
              <div v-if="isSidebarOpen" class="flex flex-col">
                <span class="text-base font-bold text-neutral-900 tracking-wide">ADMIN</span>
                <span class="text-[10px] font-medium text-neutral-400 uppercase tracking-widest">Dashboard</span>
              </div>
            </Transition>
          </div>
          <button @click="toggleSidebar"
            class="hidden lg:flex w-8 h-8 items-center justify-center bg-neutral-50 hover:bg-neutral-100 rounded-lg text-neutral-500 hover:text-neutral-700 transition-colors">
            <ChevronLeft :class="['w-4 h-4 transition-transform duration-300', !isSidebarOpen && 'rotate-180']" />
          </button>
          <button @click="closeMobileSidebar"
            class="lg:hidden w-8 h-8 flex items-center justify-center text-neutral-500">
            <X class="w-5 h-5" />
          </button>
        </div>
        <!-- Navigation -->
        <nav class="flex-1 justify-center overflow-y-auto py-6 px-6">
          <div v-if="isSidebarOpen"
            class="text-[15px] font-bold text-neutral-900 uppercase tracking-[0.15em] px-3 mb-4">MENU</div>
          <router-link v-for="item in navigation" :key="item.name" :to="item.to" @click="closeMobileSidebar" :class="[
            'flex items-center gap-3.5 p-3.5 mb-1 rounded-xl transition-all duration-200 relative group',
            route.path.startsWith(item.to)
              ? 'bg-transparent text-neutral-900'
              : 'text-neutral-500 hover:bg-neutral-50 hover:text-neutral-700'
          ]">
            <div :class="[
              'w-10 h-10 flex items-center justify-center rounded-xl transition-all duration-200 shrink-0',
              route.path.startsWith(item.to)
                ? 'bg-neutral-900 text-white'
                : 'bg-neutral-100 group-hover:bg-neutral-200'
            ]">
              <component :is="item.icon" class="w-5 h-5" />
            </div>
            <Transition name="fade">
              <span v-if="isSidebarOpen" class="text-sm font-semibold whitespace-nowrap">{{ item.name }}</span>
            </Transition>
          </router-link>
        </nav>
        <!-- Sidebar Footer -->
        <div class="p-4 border-t border-neutral-100 shrink-0">
          <!-- User Profile -->
          <div class="flex items-center gap-3 p-3 bg-neutral-50 rounded-xl">
            <div @click="toggleSidebar"
              class="w-10 h-10 flex items-center justify-center bg-neutral-900 text-white text-sm font-bold rounded-xl cursor-pointer transition-transform hover:scale-105 shrink-0">
              {{ userInitials }}
            </div>
            <Transition name="fade">
              <div v-if="isSidebarOpen" class="flex-1 min-w-0">
                <p class="text-sm font-bold text-neutral-800 truncate">{{ authStore.user?.username || 'Admin' }}</p>
                <button @click="handleLogout"
                  class="inline-flex items-center gap-1 text-xs font-semibold text-orange-500 hover:underline">
                  <LogOut class="w-3 h-3" />
                  Logout
                </button>
              </div>
            </Transition>
          </div>
        </div>
      </aside>
      <!-- Main Content -->
      <div class="flex-1 flex flex-col min-w-0 min-h-screen">
        <!-- Top Header -->
        <header class="flex items-center justify-between px-4 lg:px-8 py-5 gap-4">
          <div class="flex items-center gap-4">
            <button @click="toggleMobileSidebar"
              class="lg:hidden w-10 h-10 flex items-center justify-center bg-white/10 backdrop-blur-sm rounded-xl text-white">
              <Menu class="w-5 h-5" />
            </button>
            <div class="flex items-center gap-2 text-sm text-white/70">
              <span>Pages</span>
              <span class="opacity-50">/</span>
              <span class="text-white font-semibold">{{ currentPage }}</span>
            </div>
          </div>

          <div class="flex items-center gap-3">
            <!-- Search -->
            <div class="relative hidden md:block">
              <Search class="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-white/50" />
              <input v-model="searchQuery" type="text" placeholder="Search..."
                class="w-52 focus:w-72 pl-10 pr-4 py-3 bg-white/10 border border-white/10 rounded-xl text-sm text-white placeholder-white/50 backdrop-blur-sm outline-none transition-all duration-300 focus:bg-white focus:border-white focus:text-neutral-800 focus:placeholder-neutral-400" />
            </div>

            <!-- Actions -->
            <div class="flex items-center gap-2">
              <button
                class="relative w-10 h-10 flex items-center justify-center bg-white/10 backdrop-blur-sm rounded-xl text-white hover:bg-white/20 transition-colors">
                <Bell class="w-5 h-5" />
                <span
                  class="absolute top-2 right-2 w-2 h-2 bg-orange-500 rounded-full border-2 border-neutral-900"></span>
              </button>
              <button @click="handleLogout"
                class="flex items-center gap-2 px-4 py-2.5 bg-white/10 backdrop-blur-sm rounded-xl text-sm font-semibold text-white hover:bg-white/20 transition-colors">
                <LogOut class="w-4 h-4" />
                <span class="hidden sm:inline">Sign Out</span>
              </button>
            </div>
          </div>
        </header>

        <!-- Page Content -->
        <main class="flex-1 px-4 lg:px-8 pb-8 overflow-y-auto">
          <router-view v-slot="{ Component }">
            <Transition name="page" mode="out-in">
              <div :key="route.path" class="w-full">
                <component :is="Component" />
              </div>
            </Transition>
          </router-view>
        </main>
      </div>
    </div>

    <!-- Toast Container -->
    <Teleport to="body">
      <div class="fixed top-6 right-6 z-9999 flex flex-col gap-3 pointer-events-none">
        <TransitionGroup name="toast">
          <div v-for="toast in toasts" :key="toast.id"
            class="flex items-center gap-3 px-5 py-4 bg-white rounded-2xl shadow-2xl border border-neutral-100 pointer-events-auto min-w-[320px] max-w-[420px]"
            style="box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);">
            <div
              :class="['w-10 h-10 flex items-center justify-center rounded-xl text-white shrink-0', toastStyles[toast.type]]">
              <component :is="toastIcons[toast.type]" class="w-5 h-5" />
            </div>
            <p class="flex-1 text-sm font-medium text-neutral-800">{{ toast.message }}</p>
            <button @click="removeToast(toast.id)"
              class="p-1.5 rounded-lg text-neutral-400 hover:bg-neutral-100 hover:text-neutral-600 transition-colors">
              <X class="w-4 h-4" />
            </button>
          </div>
        </TransitionGroup>
      </div>
    </Teleport>
  </div>
</template>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Nunito:ital,wght@0,200..1000;1,200..1000&family=Roboto:ital,wght@0,100..900;1,100..900&display=swap');


.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

.page-enter-active,
.page-leave-active {
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.page-enter-from {
  opacity: 0;
  transform: translateY(10px);
}

.page-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}

.w-22 {
  width: 88px;
}

/* Toast animations */
.toast-enter-active {
  animation: toast-in 0.4s cubic-bezier(0.16, 1, 0.3, 1);
}

.toast-leave-active {
  animation: toast-out 0.3s cubic-bezier(0.4, 0, 1, 1);
}

.toast-move {
  transition: transform 0.3s ease;
}

@keyframes toast-in {
  0% {
    opacity: 0;
    transform: translateX(100%) scale(0.9);
  }

  100% {
    opacity: 1;
    transform: translateX(0) scale(1);
  }
}

@keyframes toast-out {
  0% {
    opacity: 1;
    transform: translateX(0) scale(1);
  }

  100% {
    opacity: 0;
    transform: translateX(100%) scale(0.9);
  }
}
</style>
