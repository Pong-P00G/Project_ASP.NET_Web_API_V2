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
  <div class="min-h-screen bg-[#1A1A1A] relative font-sans">
    <!-- Black Header Background -->
    <div class="absolute top-0 left-0 right-0 h-80 bg-[#0A0A0A] z-0"></div>
    <div class="flex relative z-10 min-h-screen">
      <!-- Mobile Overlay -->
      <Transition name="fade">
        <div v-if="isMobileSidebarOpen" class="fixed inset-0 bg-black/60 backdrop-blur-sm z-40 lg:hidden"
          @click="closeMobileSidebar">
        </div>
      </Transition>
      <!-- Sidebar -->
      <aside :class="['fixed top-0 left-0 bottom-0 z-50 flex flex-col transition-all duration-500 ease-out bg-[linear-gradient(180deg,rgba(15,15,15,0.98)_0%,rgba(10,10,10,0.95)_100%)] backdrop-blur-[20px] shadow-[0_0_0_1px_rgba(255,255,255,0.05),0_25px_50px_-12px_rgba(0,0,0,0.5),0_0_100px_rgba(200,169,126,0.05)]',
        'lg:sticky lg:top-4 lg:left-0 lg:h-[calc(100vh-2rem)] lg:m-4 lg:rounded-3xl lg:translate-x-0',
        isSidebarOpen ? 'w-72' : 'w-22',
        isMobileSidebarOpen ? 'translate-x-0' : '-translate-x-full lg:translate-x-0'
      ]">
        <!-- Linear Border Effect -->
        <div
          class="absolute inset-0 rounded-3xl p-px bg-linear-to-b from-white/20 via-transparent to-white/5 pointer-events-none">
        </div>

        <!-- Logo -->
        <div class="h-24 flex items-center justify-between px-6 border-b border-white/10 shrink-0 relative">
          <div class="flex items-center gap-4">
            <div class="w-12 h-12 flex items-center justify-center rounded-2xl shrink-0 relative overflow-hidden"
              :class="['animate-[pulse_3s_ease-in-out_infinite]', !isSidebarOpen && 'hidden']">
              <div class="absolute inset-0 bg-[linear-gradient(135deg,#C8A97E,#A88B5E)]"></div>
              <div
                class="absolute inset-0 opacity-0 hover:opacity-100 transition-opacity duration-300 bg-[linear-gradient(135deg,#D4B98E,#C8A97E)]">
              </div>
              <Sparkles class="w-6 h-6 relative z-10 text-white animate-[pulse_3s_ease-in-out_infinite]" />
            </div>
            <Transition name="slide-fade">
              <div v-if="isSidebarOpen" class="flex flex-col">
                <span class="text-lg font-black text-white tracking-wider">ADMIN</span>
                <span class="text-[10px] font-semibold text-white/50 uppercase tracking-[0.2em]">Dashboard</span>
              </div>
            </Transition>
          </div>
          <button @click="toggleSidebar"
            class="hidden lg:flex w-9 h-9 items-center justify-center bg-white/5 hover:bg-white/10 rounded-xl text-white/50 hover:text-white transition-all duration-300 hover:scale-110">
            <ChevronLeft
              :class="['w-4 h-4 transition-transform duration-500', !isSidebarOpen && 'rotate-180 animate-[pulse_3s_ease-in-out_infinite]']" />
          </button>
          <button @click="closeMobileSidebar"
            class="lg:hidden w-9 h-9 flex items-center justify-center text-white/70 hover:text-white transition-colors">
            <X class="w-5 h-5" />
          </button>
        </div>

        <!-- Navigation -->
        <nav class="flex-1 overflow-y-auto py-8 px-3 scrollbar-hide">
          <div v-if="isSidebarOpen" class="text-[11px] font-bold text-white/40 uppercase tracking-[0.2em] px-4 mb-6">
            Navigation</div>
          <router-link v-for="item in navigation" :key="item.name" :to="item.to" @click="closeMobileSidebar" :class="[
            'flex items-center gap-4 p-3 mb-2 rounded-2xl transition-all duration-300 relative group',
            route.path.startsWith(item.to)
              ? ''
              : 'hover:bg-white/5'
          ]">
            <!-- Active Indicator -->
            <div v-if="route.path.startsWith(item.to)"
              class="absolute left-0 top-1/2 -translate-y-1/2 w-1 h-8 rounded-r-full shadow-[0_0_20px_rgba(200,169,126,0.5),0_0_40px_rgba(200,169,126,0.2)] bg-[linear-gradient(180deg,#C8A97E,#A88B5E)]">
            </div>

            <div :class="[
              'w-11 h-11 flex items-center justify-center rounded-xl transition-all duration-300 shrink-0',
              route.path.startsWith(item.to)
                ? 'text-white shadow-[0_4px_15px_rgba(200,169,126,0.3)] bg-[linear-gradient(135deg,#C8A97E,#A88B5E)]'
                : 'bg-white/5 text-white/60 group-hover:bg-white/10 group-hover:text-white group-hover:scale-110'
            ]">
              <component :is="item.icon" class="w-5 h-5" />
            </div>
            <Transition name="slide-fade">
              <span v-if="isSidebarOpen" :class="[
                'text-sm font-semibold whitespace-nowrap transition-colors duration-300',
                route.path.startsWith(item.to) ? 'text-white' : 'text-white/60 group-hover:text-white'
              ]">{{ item.name }}</span>
            </Transition>
          </router-link>
        </nav>

        <!-- Sidebar Footer -->
        <div class="p-4 border-t border-white/10 shrink-0">
          <!-- User Profile -->
          <div class="flex items-center gap-3 p-2 rounded-2xl transition-all duration-300 group cursor-pointer">
            <div class="relative">
              <div @click="toggleSidebar"
                class="w-11 h-11 flex items-center justify-center text-white text-sm font-bold rounded-xl cursor-pointer transition-all duration-300 group-hover:scale-110 group-hover:shadow-lg shrink-0 shadow-[0_4px_15px_rgba(200,169,126,0.25)] bg-[linear-gradient(135deg,#C8A97E,#A88B5E)]">
                {{ userInitials }}
              </div>
              <!-- Online Status -->
              <div
                class="absolute -bottom-0.5 -right-0.5 w-3.5 h-3.5 bg-emerald-400 rounded-full border-2 border-[#0A0A0A] shadow-[0_0_10px_rgba(52,211,153,0.5)]">
              </div>
            </div>
            <Transition name="slide-fade">
              <div v-if="isSidebarOpen" class="flex-1 min-w-0">
                <p class="text-sm font-bold text-white truncate">{{ authStore.user?.username || 'Admin' }}</p>
                <button @click="handleLogout"
                  class="inline-flex items-center gap-1.5 text-xs font-semibold transition-colors mt-0.5 text-[#C8A97E] hover:text-[#D4B98E]">
                  <LogOut class="w-3 h-3" />
                  Sign out
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
                class="w-52 focus:w-72 pl-10 pr-4 py-3 bg-white/10 border border-white/10 rounded-xl text-sm text-white placeholder-white/50 backdrop-blur-sm outline-none transition-all duration-300 focus:bg-white focus:border-white focus:text-[#0A0A0A] focus:placeholder-neutral-400" />
            </div>

            <!-- Actions -->
            <div class="flex items-center gap-2">
              <button
                class="relative w-10 h-10 flex items-center justify-center bg-white/10 backdrop-blur-sm rounded-xl text-white hover:bg-white/20 transition-colors">
                <Bell class="w-5 h-5" />
                <span class="absolute top-2 right-2 w-2 h-2 rounded-full border-2 border-[#0A0A0A] bg-[#C8A97E]"></span>
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
      <div class="fixed top-6 right-6 z-[9999] flex flex-col gap-3 pointer-events-none">
        <TransitionGroup name="toast">
          <div v-for="toast in toasts" :key="toast.id"
            class="flex items-center gap-3 px-5 py-4 bg-white rounded-2xl shadow-[0_25px_50px_-12px_rgba(0,0,0,0.25)] border border-neutral-100 pointer-events-auto min-w-[320px] max-w-[420px]">
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
@import url('https://fonts.googleapis.com/css2?family=Roboto:ital,wght@0,100..900;1,100..900&display=swap');

/* Vue Transitions */
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

/* Slide fade animation */
.slide-fade-enter-active {
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.slide-fade-leave-active {
  transition: all 0.2s cubic-bezier(0.4, 0, 1, 1);
}

.slide-fade-enter-from,
.slide-fade-leave-to {
  opacity: 0;
  transform: translateX(-10px);
}

.w-22 {
  width: 88px;
}
</style>
