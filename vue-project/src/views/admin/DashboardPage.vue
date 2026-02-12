<script setup>
import { ref, computed, onMounted } from 'vue'
import { Users, Package, TrendingUp, ArrowRight, Clock, DollarSign, Activity, ShoppingBag, Box, Sparkles, Zap, MoreHorizontal, Star, ArrowUpRight, BarChart3, AlertTriangle, CalendarDays } from 'lucide-vue-next'
import { dashboardApi } from '../../api/dashboardApi.js'

const loading = ref(false)
const refreshing = ref(false)
const users = ref([])
const products = ref([])
const orders = ref([])
const dashboardData = ref(null)
const recentActivities = ref([])

const formatDate = (dateString) => {
  if (!dateString) return 'N/A'
  return new Date(dateString).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })
}

const formatRelativeTime = (dateString) => {
  if (!dateString) return 'Unknown'
  const diff = Math.floor((new Date() - new Date(dateString)) / 1000)
  if (diff < 60) return `${diff}s ago`
  if (diff < 3600) return `${Math.floor(diff / 60)}m ago`
  if (diff < 86400) return `${Math.floor(diff / 3600)}h ago`
  return `${Math.floor(diff / 86400)}d ago`
}

const stats = computed(() => ({
  totalUsers: dashboardData.value?.totalUsers || users.value.length,
  totalProducts: dashboardData.value?.totalProducts || products.value.length,
  lowStock: dashboardData.value?.lowStockProducts || 0,
  totalRevenue: dashboardData.value?.totalRevenue || dashboardData.value?.revenue || 0,
  revenueTrend: dashboardData.value?.revenueTrend || '+12.5%',
  usersTrend: dashboardData.value?.usersTrend || '+8.2%',
  productsTrend: dashboardData.value?.productsTrend || '+5'
}))

const fetchDashboardData = async (showRefreshing = false) => {
  try {
    if (showRefreshing) refreshing.value = true
    else loading.value = true
    const response = await dashboardApi.getAdminDashboard()
    dashboardData.value = response.data
    users.value = (response.data.users || []).map(u => ({ ...u, id: u.userId, role: u.role || 'User' }))
    orders.value = (response.data.orders || []).map(o => ({
      id: o.orderId,
      orderNumber: o.orderNumber,
      total: o.totalAmount,
      status: o.orderStatus,
      items: o.itemCount,
      date: o.createdAt
    }))
    products.value = (response.data.products || []).map(p => ({
      ...p,
      id: p.productId,
      name: p.productName,
      price: p.basePrice,
      category: p.categories?.[0]?.categoryName || 'General'
    }))
    recentActivities.value = response.data.recentActivities || response.data.activities || []
  } catch (err) {
    console.error('Dashboard error:', err)
  } finally {
    loading.value = false
    refreshing.value = false
  }
}

const refreshData = () => fetchDashboardData(true)
const clearActivities = () => { recentActivities.value = [] }

const getStatusClass = (status) => {
  const map = {
    DELIVERED: 'bg-emerald-500/15 text-emerald-400',
    SHIPPED: 'bg-blue-500/15 text-blue-400',
    PENDING: 'bg-amber-500/15 text-amber-400',
    PROCESSING: 'bg-orange-500/15 text-orange-400',
    CANCELLED: 'bg-red-500/15 text-red-400'
  }
  return map[status] || 'bg-white/10 text-white/50'
}

const getActivityIcon = (type) => {
  const map = { order: ShoppingBag, user: Users, stock: Box }
  return map[type] || Activity
}

onMounted(() => fetchDashboardData())
</script>

<template>
  <div class="w-full">
    <!-- Header -->
    <div class="flex flex-wrap items-center justify-between gap-4 mb-8">
      <div>
        <div
          class="inline-flex items-center gap-1.5 px-3.5 py-1.5 bg-[#C8A97E]/15 border border-[#C8A97E]/25 rounded-full text-[#C8A97E] text-[11px] font-semibold tracking-wide mb-3">
          <BarChart3 :size="13" />
          <span>Dashboard Overview</span>
        </div>
        <h1 class="text-[26px] font-extrabold text-white tracking-tight">Welcome back! 👋</h1>
        <p class="text-[13px] text-white/50 mt-1">Here's what's happening with your store today.</p>
      </div>
      <button @click="refreshData" :disabled="refreshing"
        class="flex items-center gap-2 px-5 py-2.5 bg-[#C8A97E] hover:bg-[#D4B98E] disabled:opacity-50 disabled:cursor-not-allowed text-[#0A0A0A] text-[13px] font-bold rounded-xl transition-all hover:-translate-y-px hover:shadow-[0_8px_25px_rgba(200,169,126,0.3)]">
        <Activity :size="15" :class="{ 'animate-spin': refreshing }" />
        {{ refreshing ? 'Refreshing...' : 'Refresh' }}
      </button>
    </div>

    <!-- Stats Grid -->
    <div v-if="loading" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
      <div v-for="i in 4" :key="i" class="h-[140px] bg-white/5 border border-white/10 rounded-2xl animate-pulse"></div>
    </div>
    <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
      <!-- Revenue -->
      <div
        class="group relative bg-white/5 border border-white/10 hover:border-white/15 rounded-2xl p-6 transition-all duration-400 hover:-translate-y-1 hover:shadow-[0_20px_40px_rgba(0,0,0,0.3)] overflow-hidden">
        <div
          class="absolute top-0 left-0 right-0 h-0.5 bg-linear-to-r from-[#C8A97E] to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300">
        </div>
        <div class="flex items-center justify-between mb-4">
          <div
            class="w-11 h-11 flex items-center justify-center rounded-xl bg-[#C8A97E]/15 text-[#C8A97E] transition-transform duration-300 group-hover:scale-110">
            <DollarSign :size="20" />
          </div>
          <div
            class="flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[11px] font-bold bg-emerald-500/10 text-emerald-400">
            <TrendingUp :size="12" />{{ stats.revenueTrend }}
          </div>
        </div>
        <h3 class="text-[28px] font-extrabold text-white tracking-tight">${{ stats.totalRevenue.toLocaleString() }}</h3>
        <p class="text-[11px] font-semibold text-white/30 uppercase tracking-widest mt-1">Total Revenue</p>
        <div class="h-0.5 bg-white/5 rounded-full mt-4 overflow-hidden">
          <div class="h-full bg-[#C8A97E] rounded-full transition-all duration-1000" style="width: 78%"></div>
        </div>
      </div>

      <!-- Users -->
      <div
        class="group relative bg-white/5 border border-white/10 hover:border-white/15 rounded-2xl p-6 transition-all duration-400 hover:-translate-y-1 hover:shadow-[0_20px_40px_rgba(0,0,0,0.3)] overflow-hidden">
        <div
          class="absolute top-0 left-0 right-0 h-0.5 bg-linear-to-r from-[#C8A97E] to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300">
        </div>
        <div class="flex items-center justify-between mb-4">
          <div
            class="w-11 h-11 flex items-center justify-center rounded-xl bg-blue-500/15 text-blue-400 transition-transform duration-300 group-hover:scale-110">
            <Users :size="20" />
          </div>
          <div
            class="flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[11px] font-bold bg-emerald-500/10 text-emerald-400">
            <TrendingUp :size="12" />{{ stats.usersTrend }}
          </div>
        </div>
        <h3 class="text-[28px] font-extrabold text-white tracking-tight">{{ stats.totalUsers }}</h3>
        <p class="text-[11px] font-semibold text-white/30 uppercase tracking-widest mt-1">Total Users</p>
        <div class="h-0.5 bg-white/5 rounded-full mt-4 overflow-hidden">
          <div class="h-full bg-[#C8A97E] rounded-full transition-all duration-1000" style="width: 62%"></div>
        </div>
      </div>

      <!-- Products -->
      <div
        class="group relative bg-white/5 border border-white/10 hover:border-white/15 rounded-2xl p-6 transition-all duration-400 hover:-translate-y-1 hover:shadow-[0_20px_40px_rgba(0,0,0,0.3)] overflow-hidden">
        <div
          class="absolute top-0 left-0 right-0 h-0.5 bg-linear-to-r from-[#C8A97E] to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300">
        </div>
        <div class="flex items-center justify-between mb-4">
          <div
            class="w-11 h-11 flex items-center justify-center rounded-xl bg-purple-500/15 text-purple-400 transition-transform duration-300 group-hover:scale-110">
            <Package :size="20" />
          </div>
          <div
            class="flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[11px] font-bold bg-emerald-500/10 text-emerald-400">
            <Zap :size="12" />+{{ stats.productsTrend }}
          </div>
        </div>
        <h3 class="text-[28px] font-extrabold text-white tracking-tight">{{ stats.totalProducts }}</h3>
        <p class="text-[11px] font-semibold text-white/30 uppercase tracking-widest mt-1">Total Products</p>
        <div class="h-0.5 bg-white/5 rounded-full mt-4 overflow-hidden">
          <div class="h-full bg-[#C8A97E] rounded-full transition-all duration-1000" style="width: 85%"></div>
        </div>
      </div>

      <!-- Orders -->
      <div
        class="group relative bg-white/5 border border-white/10 hover:border-white/15 rounded-2xl p-6 transition-all duration-400 hover:-translate-y-1 hover:shadow-[0_20px_40px_rgba(0,0,0,0.3)] overflow-hidden">
        <div
          class="absolute top-0 left-0 right-0 h-0.5 bg-linear-to-r from-[#C8A97E] to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300">
        </div>
        <div class="flex items-center justify-between mb-4">
          <div
            class="w-11 h-11 flex items-center justify-center rounded-xl bg-emerald-500/15 text-emerald-400 transition-transform duration-300 group-hover:scale-110">
            <ShoppingBag :size="20" />
          </div>
          <div
            class="flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[11px] font-bold bg-emerald-500/10 text-emerald-400">
            <TrendingUp :size="12" />+24%
          </div>
        </div>
        <h3 class="text-[28px] font-extrabold text-white tracking-tight">{{ orders.length }}</h3>
        <p class="text-[11px] font-semibold text-white/30 uppercase tracking-widest mt-1">Recent Orders</p>
        <div class="h-0.5 bg-white/5 rounded-full mt-4 overflow-hidden">
          <div class="h-full bg-[#C8A97E] rounded-full transition-all duration-1000" style="width: 55%"></div>
        </div>
      </div>
    </div>

    <!-- Main Grid -->
    <div class="grid grid-cols-1 lg:grid-cols-[2fr_1fr] gap-4 mb-6">
      <!-- Chart Area -->
      <div class="bg-white/5 border border-white/10 rounded-2xl p-6">
        <div class="flex items-start justify-between mb-5">
          <div>
            <h2 class="text-base font-bold text-white">Sales Overview</h2>
            <p class="text-[12px] text-white/30 mt-0.5">
              <span class="text-[#C8A97E] font-semibold">
                <TrendingUp :size="13" class="inline align-[-2px]" /> +12.5%
              </span>
              from last period
            </p>
          </div>
          <div class="flex gap-0.5 bg-white/5 rounded-lg p-1">
            <button
              class="px-4 py-1.5 rounded-md text-[11px] font-semibold bg-[#C8A97E] text-[#0A0A0A] transition-all">Week</button>
            <button
              class="px-4 py-1.5 rounded-md text-[11px] font-semibold text-white/30 hover:text-white/50 bg-transparent cursor-pointer transition-all">Month</button>
            <button
              class="px-4 py-1.5 rounded-md text-[11px] font-semibold text-white/30 hover:text-white/50 bg-transparent cursor-pointer transition-all">Year</button>
          </div>
        </div>
        <div class="h-[200px]">
          <svg class="w-full h-full" viewBox="0 0 1000 280" preserveAspectRatio="none">
            <defs>
              <linearGradient id="areaFill" x1="0" x2="0" y1="0" y2="1">
                <stop offset="0%" stop-color="#C8A97E" stop-opacity="0.25" />
                <stop offset="100%" stop-color="#C8A97E" stop-opacity="0" />
              </linearGradient>
              <linearGradient id="lineFill" x1="0" x2="1" y1="0" y2="0">
                <stop offset="0%" stop-color="#C8A97E" />
                <stop offset="100%" stop-color="#E8D5B5" />
              </linearGradient>
            </defs>
            <g stroke="rgba(255,255,255,0.05)" stroke-width="1">
              <line x1="0" y1="56" x2="1000" y2="56" />
              <line x1="0" y1="112" x2="1000" y2="112" />
              <line x1="0" y1="168" x2="1000" y2="168" />
              <line x1="0" y1="224" x2="1000" y2="224" />
            </g>
            <path
              d="M0,260 C80,240 120,170 200,150 C280,130 320,190 400,170 C480,150 520,90 600,70 C680,50 720,110 800,90 C880,70 920,30 1000,20 L1000,280 H0 Z"
              fill="url(#areaFill)" />
            <path
              d="M0,260 C80,240 120,170 200,150 C280,130 320,190 400,170 C480,150 520,90 600,70 C680,50 720,110 800,90 C880,70 920,30 1000,20"
              fill="none" stroke="url(#lineFill)" stroke-width="3" stroke-linecap="round" />
            <g>
              <circle cx="200" cy="150" r="5" fill="#C8A97E" stroke="#0A0A0A" stroke-width="3" />
              <circle cx="400" cy="170" r="5" fill="#C8A97E" stroke="#0A0A0A" stroke-width="3" />
              <circle cx="600" cy="70" r="5" fill="#C8A97E" stroke="#0A0A0A" stroke-width="3" />
              <circle cx="800" cy="90" r="5" fill="#C8A97E" stroke="#0A0A0A" stroke-width="3" />
              <circle cx="1000" cy="20" r="5" fill="#C8A97E" stroke="#0A0A0A" stroke-width="3" />
            </g>
          </svg>
        </div>
        <div class="flex justify-between mt-3 px-4 text-[10px] font-semibold text-white/30 uppercase tracking-widest">
          <span>Mon</span><span>Tue</span><span>Wed</span><span>Thu</span><span>Fri</span><span>Sat</span><span>Sun</span>
        </div>
      </div>

      <!-- Activity Feed -->
      <div class="bg-white/5 border border-white/10 rounded-2xl p-6">
        <div class="flex items-start justify-between mb-5">
          <div>
            <h2 class="text-base font-bold text-white">Recent Activity</h2>
            <p class="text-[12px] text-white/30 mt-0.5">Latest updates</p>
          </div>
          <button @click="clearActivities"
            class="text-[12px] font-semibold text-white/30 hover:text-[#C8A97E] transition-colors">Clear</button>
        </div>
        <div v-if="recentActivities.length === 0"
          class="flex flex-col items-center justify-center py-12 text-white/30 gap-3">
          <Activity :size="28" />
          <p class="text-[13px] font-medium">No recent activities</p>
        </div>
        <div v-else class="flex flex-col gap-2 max-h-[320px] overflow-y-auto pr-1">
          <div v-for="activity in recentActivities" :key="activity.id"
            class="flex items-start gap-3 p-3 rounded-xl hover:bg-white/10 transition-colors">
            <div class="w-8 h-8 flex items-center justify-center rounded-lg bg-[#C8A97E]/15 text-[#C8A97E] shrink-0">
              <component :is="getActivityIcon(activity.type)" :size="14" />
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-[13px] font-medium text-white/70 leading-snug">{{ activity.message }}</p>
              <span class="flex items-center gap-1 text-[11px] text-white/30 mt-1">
                <Clock :size="10" /> {{ formatRelativeTime(activity.time) }}
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Bottom Grid -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">
      <!-- Recent Orders -->
      <div class="bg-white/5 border border-white/10 rounded-2xl p-6">
        <div class="flex items-start justify-between mb-5">
          <div>
            <h2 class="text-base font-bold text-white">Recent Orders</h2>
            <p class="text-[12px] text-white/30 mt-0.5">{{ orders.length }} orders</p>
          </div>
          <router-link to="/admin/orders"
            class="flex items-center gap-1 text-[12px] font-semibold text-[#C8A97E] hover:opacity-80 transition-opacity">
            View All
            <ArrowUpRight :size="13" />
          </router-link>
        </div>
        <div v-if="orders.length === 0" class="flex flex-col items-center justify-center py-12 text-white/30 gap-3">
          <ShoppingBag :size="28" />
          <p class="text-[13px] font-medium">No recent orders</p>
        </div>
        <div v-else class="flex flex-col gap-2">
          <div v-for="order in orders.slice(0, 5)" :key="order.id"
            class="flex items-center justify-between p-3.5 rounded-xl hover:bg-white/10 transition-colors">
            <div>
              <p class="text-[13px] font-bold text-white">#{{ order.orderNumber }}</p>
              <p class="text-[11px] text-white/30 mt-0.5">{{ order.items }} items · {{ formatDate(order.date) }}</p>
            </div>
            <div class="text-right">
              <p class="text-[13px] font-bold text-white">${{ order.total?.toFixed(2) }}</p>
              <span
                :class="['inline-block mt-1 px-2 py-0.5 rounded-[6px] text-[10px] font-bold uppercase tracking-wide', getStatusClass(order.status)]">{{
                order.status }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Top Products -->
      <div class="bg-white/5 border border-white/10 rounded-2xl p-6">
        <div class="flex items-start justify-between mb-5">
          <div>
            <h2 class="text-base font-bold text-white">Top Products</h2>
            <p class="text-[12px] text-white/30 mt-0.5">Best performers</p>
          </div>
          <router-link to="/admin/product"
            class="flex items-center gap-1 text-[12px] font-semibold text-[#C8A97E] hover:opacity-80 transition-opacity">
            View All
            <ArrowUpRight :size="13" />
          </router-link>
        </div>
        <div v-if="products.length === 0" class="flex flex-col items-center justify-center py-12 text-white/30 gap-3">
          <Package :size="28" />
          <p class="text-[13px] font-medium">No products found</p>
        </div>
        <div v-else class="flex flex-col gap-2">
          <div v-for="(product, index) in products.slice(0, 5)" :key="product.id"
            class="flex items-center gap-3 p-3 rounded-xl hover:bg-white/10 transition-colors">
            <div class="w-7 h-7 flex items-center justify-center rounded-lg text-[11px] font-bold shrink-0"
              :class="index === 0 ? 'bg-[#C8A97E]/20 text-[#C8A97E]' : index === 1 ? 'bg-white/15 text-white/70' : index === 2 ? 'bg-orange-700/20 text-orange-600' : 'bg-white/5 text-white/30'">
              <Star v-if="index < 3" :size="12" />
              <span v-else>{{ index + 1 }}</span>
            </div>
            <div
              class="w-10 h-10 flex items-center justify-center rounded-xl bg-[#C8A97E]/15 text-[#C8A97E] text-[15px] font-extrabold shrink-0 transition-transform hover:scale-105">
              {{ product.name?.charAt(0) }}
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-[13px] font-semibold text-white truncate">{{ product.name }}</p>
              <p class="text-[11px] text-white/30 mt-0.5">Stock: {{ product.stock }}</p>
            </div>
            <p class="text-[13px] font-bold text-white">${{ product.price }}</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>