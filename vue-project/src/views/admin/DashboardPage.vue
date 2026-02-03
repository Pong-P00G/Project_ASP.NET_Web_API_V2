<script setup>
import { ref, computed, onMounted } from 'vue'
import { Users, Package, TrendingUp, ArrowRight, Clock, DollarSign, Activity, ShoppingBag, Box, Sparkles, Zap, MoreHorizontal, Star, ArrowUpRight } from 'lucide-vue-next'
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
    DELIVERED: 'bg-emerald-500/20 text-emerald-400 border border-emerald-500/30',
    SHIPPED: 'bg-blue-500/20 text-blue-400 border border-blue-500/30',
    PENDING: 'bg-amber-500/20 text-amber-400 border border-amber-500/30',
    PROCESSING: 'bg-orange-500/20 text-orange-400 border border-orange-500/30',
    CANCELLED: 'bg-red-500/20 text-red-400 border border-red-500/30'
  }
  return map[status] || 'bg-neutral-500/20 text-neutral-400 border border-neutral-500/30'
}

const getActivityClass = (type) => {
  const map = {
    order: 'bg-gradient-to-br from-orange-500 to-pink-500 text-white',
    user: 'bg-gradient-to-br from-blue-500 to-cyan-500 text-white',
    stock: 'bg-gradient-to-br from-emerald-500 to-teal-500 text-white'
  }
  return map[type] || 'bg-neutral-700 text-neutral-300'
}

onMounted(() => fetchDashboardData())
</script>

<template>
  <div class="w-full">
    <!-- Header -->
    <div class="flex flex-wrap items-center justify-between gap-4 mb-8">
      <div>
        <div
          class="inline-flex items-center gap-2 px-4 py-2 bg-linear-to-r from-orange-500/20 to-pink-500/20 backdrop-blur-sm border border-orange-500/20 rounded-full text-orange-400 text-xs font-semibold mb-3">
          <Sparkles class="w-3.5 h-3.5" /><span>Dashboard Overview</span>
        </div>
        <h1 class="text-3xl font-black text-white tracking-tight">Welcome back! 👋</h1>
        <p class="text-neutral-400 text-sm mt-1">Here's what's happening with your store today.</p>
      </div>
      <button @click="refreshData" :disabled="refreshing"
        class="group flex items-center gap-2 px-6 py-3 bg-linear-to-r from-orange-500 to-pink-500 rounded-xl text-sm font-bold text-white shadow-lg shadow-orange-500/25 hover:shadow-orange-500/40 hover:scale-105 transition-all duration-300 disabled:opacity-50">
        <Activity class="w-4 h-4 transition-transform group-hover:rotate-180 duration-500"
          :class="{ 'animate-spin': refreshing }" />
        {{ refreshing ? 'Refreshing...' : 'Refresh Data' }}
      </button>
    </div>

    <!-- Stats -->
    <div v-if="loading" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5 mb-8">
      <div v-for="i in 4" :key="i"
        class="h-36 bg-white/5 backdrop-blur-sm rounded-2xl animate-pulse border border-white/10"></div>
    </div>
    <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5 mb-8">
      <!-- Revenue -->
      <div
        class="stat-card group relative bg-white/5 backdrop-blur-xl rounded-2xl p-6 border border-white/10 hover:border-emerald-500/50 hover:-translate-y-2 hover:shadow-2xl hover:shadow-emerald-500/10 transition-all duration-500 overflow-hidden">
        <div
          class="absolute inset-0 bg-linear-to-br from-emerald-500/10 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-500">
        </div>
        <div class="relative z-10">
          <div class="flex items-center justify-between mb-4">
            <div
              class="w-14 h-14 flex items-center justify-center bg-linear-to-br from-emerald-400 to-emerald-600 rounded-2xl text-white shadow-lg shadow-emerald-500/30 group-hover:scale-110 transition-transform duration-300">
              <DollarSign class="w-6 h-6" />
            </div>
            <div
              class="flex items-center gap-1.5 px-3 py-1.5 bg-emerald-500/20 text-emerald-400 rounded-full text-xs font-bold border border-emerald-500/30">
              <TrendingUp class="w-3 h-3" />{{ stats.revenueTrend }}
            </div>
          </div>
          <h3 class="text-3xl font-black text-white tracking-tight">${{ stats.totalRevenue.toLocaleString() }}</h3>
          <p class="text-xs font-semibold text-neutral-400 uppercase tracking-widest mt-2">Total Revenue</p>
        </div>
      </div>

      <!-- Users -->
      <div
        class="stat-card group relative bg-white/5 backdrop-blur-xl rounded-2xl p-6 border border-white/10 hover:border-blue-500/50 hover:-translate-y-2 hover:shadow-2xl hover:shadow-blue-500/10 transition-all duration-500 overflow-hidden">
        <div
          class="absolute inset-0 bg-linear-to-br from-blue-500/10 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-500">
        </div>
        <div class="relative z-10">
          <div class="flex items-center justify-between mb-4">
            <div
              class="w-14 h-14 flex items-center justify-center bg-linear-to-br from-blue-400 to-blue-600 rounded-2xl text-white shadow-lg shadow-blue-500/30 group-hover:scale-110 transition-transform duration-300">
              <Users class="w-6 h-6" />
            </div>
            <div
              class="flex items-center gap-1.5 px-3 py-1.5 bg-blue-500/20 text-blue-400 rounded-full text-xs font-bold border border-blue-500/30">
              <TrendingUp class="w-3 h-3" />{{ stats.usersTrend }}
            </div>
          </div>
          <h3 class="text-3xl font-black text-white tracking-tight">{{ stats.totalUsers }}</h3>
          <p class="text-xs font-semibold text-neutral-400 uppercase tracking-widest mt-2">Total Users</p>
        </div>
      </div>

      <!-- Products -->
      <div
        class="stat-card group relative bg-white/5 backdrop-blur-xl rounded-2xl p-6 border border-white/10 hover:border-orange-500/50 hover:-translate-y-2 hover:shadow-2xl hover:shadow-orange-500/10 transition-all duration-500 overflow-hidden">
        <div
          class="absolute inset-0 bg-linear-to-br from-orange-500/10 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-500">
        </div>
        <div class="relative z-10">
          <div class="flex items-center justify-between mb-4">
            <div
              class="w-14 h-14 flex items-center justify-center bg-linear-to-br from-orange-400 to-pink-500 rounded-2xl text-white shadow-lg shadow-orange-500/30 group-hover:scale-110 transition-transform duration-300">
              <Package class="w-6 h-6" />
            </div>
            <div
              class="flex items-center gap-1.5 px-3 py-1.5 bg-orange-500/20 text-orange-400 rounded-full text-xs font-bold border border-orange-500/30">
              <Zap class="w-3 h-3" />+{{ stats.productsTrend }}
            </div>
          </div>
          <h3 class="text-3xl font-black text-white tracking-tight">{{ stats.totalProducts }}</h3>
          <p class="text-xs font-semibold text-neutral-400 uppercase tracking-widest mt-2">Total Products</p>
        </div>
      </div>

      <!-- Orders -->
      <div
        class="stat-card group relative bg-white/5 backdrop-blur-xl rounded-2xl p-6 border border-white/10 hover:border-purple-500/50 hover:-translate-y-2 hover:shadow-2xl hover:shadow-purple-500/10 transition-all duration-500 overflow-hidden">
        <div
          class="absolute inset-0 bg-linear-to-br from-purple-500/10 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-500">
        </div>
        <div class="relative z-10">
          <div class="flex items-center justify-between mb-4">
            <div
              class="w-14 h-14 flex items-center justify-center bg-linear-to-br from-purple-400 to-purple-600 rounded-2xl text-white shadow-lg shadow-purple-500/30 group-hover:scale-110 transition-transform duration-300">
              <ShoppingBag class="w-6 h-6" />
            </div>
            <div
              class="flex items-center gap-1.5 px-3 py-1.5 bg-purple-500/20 text-purple-400 rounded-full text-xs font-bold border border-purple-500/30">
              <TrendingUp class="w-3 h-3" />+24%
            </div>
          </div>
          <h3 class="text-3xl font-black text-white tracking-tight">{{ orders.length }}</h3>
          <p class="text-xs font-semibold text-neutral-400 uppercase tracking-widest mt-2">Recent Orders</p>
        </div>
      </div>
    </div>

    <!-- Main Grid -->
    <div class="grid grid-cols-1 xl:grid-cols-3 gap-6 mb-8">
      <!-- Chart -->
      <div class="xl:col-span-2 glass-card bg-white/5 backdrop-blur-xl rounded-3xl p-6 border border-white/10">
        <div class="flex flex-wrap items-center justify-between gap-4 mb-6">
          <div>
            <h2 class="text-xl font-bold text-white">Sales Overview</h2>
            <p class="flex items-center gap-2 text-sm text-neutral-400 mt-1">
              <span class="flex items-center gap-1 text-emerald-400 font-semibold">
                <TrendingUp class="w-4 h-4" />+12.5%
              </span> from last period
            </p>
          </div>
          <div class="flex gap-1 bg-white/5 backdrop-blur-sm rounded-xl p-1.5 border border-white/10">
            <button
              class="px-5 py-2 text-xs font-bold bg-linear-to-r from-orange-500 to-pink-500 text-white rounded-lg shadow-lg shadow-orange-500/25">Week</button>
            <button
              class="px-5 py-2 text-xs font-semibold text-neutral-400 hover:text-white transition-colors">Month</button>
            <button
              class="px-5 py-2 text-xs font-semibold text-neutral-400 hover:text-white transition-colors">Year</button>
          </div>
        </div>
        <div class="relative h-56">
          <svg class="w-full h-full" viewBox="0 0 1000 300" preserveAspectRatio="none">
            <defs>
              <linearlinear id="chartGradient" x1="0" x2="0" y1="0" y2="1">
                <stop offset="0%" stop-color="#f97316" stop-opacity="0.4" />
                <stop offset="50%" stop-color="#ec4899" stop-opacity="0.2" />
                <stop offset="100%" stop-color="#f97316" stop-opacity="0" />
              </linearlinear>
              <linearGradient id="lineGradient" x1="0" x2="1" y1="0" y2="0">
                <stop offset="0%" stop-color="#f97316" />
                <stop offset="100%" stop-color="#ec4899" />
              </linearGradient>
              <filter id="glow">
                <feGaussianBlur stdDeviation="3" result="coloredBlur" />
                <feMerge>
                  <feMergeNode in="coloredBlur" />
                  <feMergeNode in="SourceGraphic" />
                </feMerge>
              </filter>
            </defs>
            <g stroke="rgba(255,255,255,0.05)" stroke-width="1">
              <line x1="0" y1="60" x2="1000" y2="60" />
              <line x1="0" y1="120" x2="1000" y2="120" />
              <line x1="0" y1="180" x2="1000" y2="180" />
              <line x1="0" y1="240" x2="1000" y2="240" />
            </g>
            <path
              d="M0,280 C80,260 120,180 200,160 C280,140 320,200 400,180 C480,160 520,100 600,80 C680,60 720,120 800,100 C880,80 920,40 1000,30 L1000,300 H0 Z"
              fill="url(#chartGradient)" />
            <path
              d="M0,280 C80,260 120,180 200,160 C280,140 320,200 400,180 C480,160 520,100 600,80 C680,60 720,120 800,100 C880,80 920,40 1000,30"
              fill="none" stroke="url(#lineGradient)" stroke-width="4" stroke-linecap="round" filter="url(#glow)"
              vector-effect="non-scaling-stroke" />
            <g filter="url(#glow)">
              <circle cx="200" cy="160" r="8" fill="#f97316" stroke="#fff" stroke-width="3" />
              <circle cx="400" cy="180" r="8" fill="#f97316" stroke="#fff" stroke-width="3" />
              <circle cx="600" cy="80" r="8" fill="#ec4899" stroke="#fff" stroke-width="3" />
              <circle cx="800" cy="100" r="8" fill="#ec4899" stroke="#fff" stroke-width="3" />
              <circle cx="1000" cy="30" r="8" fill="#ec4899" stroke="#fff" stroke-width="3" />
            </g>
          </svg>
        </div>
        <div class="flex justify-between mt-4 text-xs font-semibold text-neutral-500 uppercase px-4">
          <span>Mon</span><span>Tue</span><span>Wed</span><span>Thu</span><span>Fri</span><span>Sat</span><span>Sun</span>
        </div>
      </div>

      <!-- Activity Feed -->
      <div class="glass-card bg-white/5 backdrop-blur-xl rounded-3xl p-6 border border-white/10">
        <div class="flex items-center justify-between mb-6">
          <div>
            <h2 class="text-xl font-bold text-white">Recent Activity</h2>
            <p class="text-sm text-neutral-400 mt-1">Latest updates from your store</p>
          </div>
          <button @click="clearActivities"
            class="text-xs font-semibold text-neutral-500 hover:text-orange-400 transition-colors">Clear All</button>
        </div>
        <div v-if="recentActivities.length === 0" class="flex flex-col items-center justify-center py-16 text-center">
          <div
            class="w-16 h-16 flex items-center justify-center bg-white/5 rounded-2xl text-neutral-500 mb-4 border border-white/10">
            <Activity class="w-7 h-7" />
          </div>
          <p class="text-neutral-400 font-medium">No recent activities</p>
        </div>
        <div v-else class="space-y-3 max-h-80 overflow-y-auto pr-2 custom-scrollbar">
          <div v-for="activity in recentActivities" :key="activity.id"
            class="flex items-start gap-3 p-4 bg-white/5 rounded-2xl border border-white/5 hover:border-white/10 hover:bg-white/10 transition-all duration-300 group">
            <div
              :class="['w-10 h-10 flex items-center justify-center rounded-xl shrink-0 shadow-lg', getActivityClass(activity.type)]">
              <ShoppingBag v-if="activity.type === 'order'" class="w-4 h-4" />
              <Users v-else-if="activity.type === 'user'" class="w-4 h-4" />
              <Box v-else-if="activity.type === 'stock'" class="w-4 h-4" />
              <Activity v-else class="w-4 h-4" />
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-sm font-medium text-neutral-200 leading-snug group-hover:text-white transition-colors">{{
                activity.message }}</p>
              <span class="flex items-center gap-1.5 text-xs text-neutral-500 mt-1.5">
                <Clock class="w-3 h-3" />{{ formatRelativeTime(activity.time) }}
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Bottom Grid -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <!-- Recent Orders -->
      <div class="glass-card bg-white/5 backdrop-blur-xl rounded-3xl p-6 border border-white/10">
        <div class="flex items-center justify-between mb-6">
          <div>
            <h2 class="text-xl font-bold text-white">Recent Orders</h2>
            <p class="text-sm text-neutral-400 mt-1">{{ orders.length }} orders this month</p>
          </div>
          <router-link to="/admin/orders"
            class="group flex items-center gap-2 text-sm font-semibold text-orange-400 hover:text-orange-300 transition-colors">
            View All
            <ArrowUpRight
              class="w-4 h-4 group-hover:translate-x-0.5 group-hover:-translate-y-0.5 transition-transform" />
          </router-link>
        </div>
        <div v-if="orders.length === 0" class="flex flex-col items-center justify-center py-16 text-center">
          <div
            class="w-16 h-16 flex items-center justify-center bg-white/5 rounded-2xl text-neutral-500 mb-4 border border-white/10">
            <ShoppingBag class="w-7 h-7" />
          </div>
          <p class="text-neutral-400 font-medium">No recent orders found</p>
        </div>
        <div v-else class="space-y-3">
          <div v-for="order in orders.slice(0, 5)" :key="order.id"
            class="flex items-center justify-between p-4 bg-white/5 rounded-2xl border border-white/5 hover:border-white/10 hover:bg-white/10 transition-all duration-300 group">
            <div>
              <p class="text-sm font-bold text-white group-hover:text-orange-400 transition-colors"><span
                  class="text-neutral-500">#</span>{{ order.orderNumber }}</p>
              <p class="text-xs text-neutral-500 mt-1">{{ order.items }} items • {{ formatDate(order.date) }}</p>
            </div>
            <div class="text-right">
              <p class="text-sm font-bold text-white">${{ order.total.toFixed(2) }}</p>
              <span
                :class="['inline-block mt-1.5 px-2.5 py-1 rounded-full text-[10px] font-bold uppercase', getStatusClass(order.status)]">{{
                order.status }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Top Products -->
      <div class="glass-card bg-white/5 backdrop-blur-xl rounded-3xl p-6 border border-white/10">
        <div class="flex items-center justify-between mb-6">
          <div>
            <h2 class="text-xl font-bold text-white">Top Products</h2>
            <p class="text-sm text-neutral-400 mt-1">Best performers this week</p>
          </div>
          <button class="p-2.5 rounded-xl text-neutral-400 hover:bg-white/10 hover:text-white transition-all">
            <MoreHorizontal class="w-5 h-5" />
          </button>
        </div>
        <div v-if="products.length === 0" class="flex flex-col items-center justify-center py-16 text-center">
          <div
            class="w-16 h-16 flex items-center justify-center bg-white/5 rounded-2xl text-neutral-500 mb-4 border border-white/10">
            <Package class="w-7 h-7" />
          </div>
          <p class="text-neutral-400 font-medium">No products found</p>
        </div>
        <div v-else class="space-y-3">
          <div v-for="(product, index) in products.slice(0, 5)" :key="product.id"
            class="flex items-center gap-4 p-4 bg-white/5 rounded-2xl border border-white/5 hover:border-white/10 hover:bg-white/10 transition-all duration-300 group">
            <div :class="['w-8 h-8 flex items-center justify-center rounded-lg text-xs font-bold',
              index === 0 ? 'bg-linear-to-br from-amber-400 to-orange-500 text-white shadow-lg shadow-orange-500/30' :
                index === 1 ? 'bg-linear-to-br from-neutral-300 to-neutral-400 text-neutral-800' :
                  index === 2 ? 'bg-linear-to-br from-amber-600 to-amber-700 text-white' :
                    'bg-white/10 text-neutral-400']">
              <Star v-if="index < 3" class="w-3.5 h-3.5" />
              <span v-else>{{ index + 1 }}</span>
            </div>
            <div
              class="w-12 h-12 flex items-center justify-center bg-linear-to-br from-orange-500 to-pink-500 text-white font-bold text-base rounded-xl shadow-lg shadow-orange-500/20 group-hover:scale-110 transition-transform duration-300">
              {{ product.name.charAt(0) }}
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-sm font-semibold text-white truncate group-hover:text-orange-400 transition-colors">{{
                product.name }}</p>
              <p class="text-xs text-neutral-500 mt-0.5">Stock: {{ product.stock }}</p>
            </div>
            <p class="text-sm font-bold text-white">${{ product.price }}</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
  width: 4px;
}

.custom-scrollbar::-webkit-scrollbar-track {
  background: rgba(255, 255, 255, 0.05);
  border-radius: 10px;
}

.custom-scrollbar::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.2);
  border-radius: 10px;
}

.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background: rgba(255, 255, 255, 0.3);
}

.glass-card {
  box-shadow:
    0 0 0 1px rgba(255, 255, 255, 0.05),
    0 25px 50px -12px rgba(0, 0, 0, 0.25);
}

.stat-card {
  box-shadow:
    0 0 0 1px rgba(255, 255, 255, 0.05),
    0 10px 40px -10px rgba(0, 0, 0, 0.3);
}
</style>