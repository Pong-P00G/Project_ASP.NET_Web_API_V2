<script setup>
import { ref, computed, onMounted } from 'vue'
import { Users, Package, TrendingUp, ArrowRight, Clock, DollarSign, Activity, ShoppingBag, Box, Sparkles, Zap, MoreHorizontal, Star } from 'lucide-vue-next'
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
    DELIVERED: 'bg-green-100 text-green-600',
    SHIPPED: 'bg-neutral-200 text-neutral-600',
    PENDING: 'bg-orange-100 text-orange-600',
    PROCESSING: 'bg-orange-100 text-orange-600',
    CANCELLED: 'bg-red-100 text-red-600'
  }
  return map[status] || 'bg-neutral-100 text-neutral-600'
}

const getActivityClass = (type) => {
  const map = {
    order: 'bg-orange-100 text-orange-600',
    user: 'bg-neutral-200 text-neutral-700',
    stock: 'bg-green-100 text-green-600'
  }
  return map[type] || 'bg-neutral-100 text-neutral-600'
}

onMounted(() => fetchDashboardData())
</script>

<template>
  <div class="w-full">
    <!-- Header -->
    <div class="flex flex-wrap items-center justify-between gap-4 mb-8">
      <div>
        <div
          class="inline-flex items-center gap-2 px-3 py-1.5 bg-neutral-900 rounded-full text-white text-xs font-semibold mb-2">
          <Sparkles class="w-3.5 h-3.5" /><span>Dashboard Overview</span>
        </div>
        <h1 class="text-2xl font-bold text-white">Welcome back! 👋</h1>
        <p class="text-neutral-500 text-sm">Here's what's happening with your store today.</p>
      </div>
      <button @click="refreshData" :disabled="refreshing"
        class="flex items-center gap-2 px-5 py-2.5 bg-neutral-900 rounded-xl text-sm font-semibold text-white hover:bg-neutral-800 transition-all">
        <Activity class="w-4 h-4" :class="{ 'animate-spin': refreshing }" />
        {{ refreshing ? 'Refreshing...' : 'Refresh Data' }}
      </button>
    </div>

    <!-- Stats -->
    <div v-if="loading" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5 mb-8">
      <div v-for="i in 4" :key="i" class="h-28 bg-neutral-200 rounded-2xl animate-pulse"></div>
    </div>
    <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5 mb-8">
      <!-- Revenue -->
      <div
        class="relative bg-white rounded-2xl p-5 border border-neutral-200 hover:-translate-y-1 hover:shadow-lg transition-all overflow-hidden group">
        <div class="flex items-center justify-between mb-4">
          <div class="w-12 h-12 flex items-center justify-center bg-green-500 rounded-xl text-white">
            <DollarSign class="w-5 h-5" />
          </div>
          <div class="flex items-center gap-1 px-2 py-1 bg-green-100 text-green-600 rounded-full text-[10px] font-bold">
            <TrendingUp class="w-3 h-3" />{{ stats.revenueTrend }}
          </div>
        </div>
        <h3 class="text-2xl font-bold text-neutral-900">${{ stats.totalRevenue.toLocaleString() }}</h3>
        <p class="text-[10px] font-bold text-neutral-400 uppercase tracking-wider mt-1">Total Revenue</p>
      </div>
      <!-- Users -->
      <div
        class="relative bg-white rounded-2xl p-5 border border-neutral-200 hover:-translate-y-1 hover:shadow-lg transition-all overflow-hidden group">
        <div class="flex items-center justify-between mb-4">
          <div class="w-12 h-12 flex items-center justify-center bg-neutral-900 rounded-xl text-white">
            <Users class="w-5 h-5" />
          </div>
          <div
            class="flex items-center gap-1 px-2 py-1 bg-neutral-200 text-neutral-700 rounded-full text-[10px] font-bold">
            <TrendingUp class="w-3 h-3" />{{ stats.usersTrend }}
          </div>
        </div>
        <h3 class="text-2xl font-bold text-neutral-900">{{ stats.totalUsers }}</h3>
        <p class="text-[10px] font-bold text-neutral-400 uppercase tracking-wider mt-1">Total Users</p>
      </div>
      <!-- Products -->
      <div
        class="relative bg-white rounded-2xl p-5 border border-neutral-200 hover:-translate-y-1 hover:shadow-lg transition-all overflow-hidden group">
        <div class="flex items-center justify-between mb-4">
          <div class="w-12 h-12 flex items-center justify-center bg-orange-500 rounded-xl text-white">
            <Package class="w-5 h-5" />
          </div>
          <div
            class="flex items-center gap-1 px-2 py-1 bg-orange-100 text-orange-600 rounded-full text-[10px] font-bold">
            <Zap class="w-3 h-3" />+{{ stats.productsTrend }}
          </div>
        </div>
        <h3 class="text-2xl font-bold text-neutral-900">{{ stats.totalProducts }}</h3>
        <p class="text-[10px] font-bold text-neutral-400 uppercase tracking-wider mt-1">Total Products</p>
      </div>
      <!-- Orders -->
      <div
        class="relative bg-white rounded-2xl p-5 border border-neutral-200 hover:-translate-y-1 hover:shadow-lg transition-all overflow-hidden group">
        <div class="flex items-center justify-between mb-4">
          <div class="w-12 h-12 flex items-center justify-center bg-neutral-900 rounded-xl text-white">
            <ShoppingBag class="w-5 h-5" />
          </div>
          <div class="flex items-center gap-1 px-2 py-1 bg-green-100 text-green-600 rounded-full text-[10px] font-bold">
            <TrendingUp class="w-3 h-3" />+24%
          </div>
        </div>
        <h3 class="text-2xl font-bold text-neutral-900">{{ orders.length }}</h3>
        <p class="text-[10px] font-bold text-neutral-400 uppercase tracking-wider mt-1">Recent Orders</p>
      </div>
    </div>

    <!-- Main Grid -->
    <div class="grid grid-cols-1 xl:grid-cols-3 gap-6 mb-8">
      <!-- Chart -->
      <div class="xl:col-span-2 bg-white rounded-2xl p-6 border border-neutral-200">
        <div class="flex flex-wrap items-center justify-between gap-4 mb-6">
          <div>
            <h2 class="text-lg font-bold text-neutral-900">Sales Overview</h2>
            <p class="flex items-center gap-1.5 text-xs text-neutral-500">
              <TrendingUp class="w-4 h-4 text-green-500" /><strong class="text-green-500">+12.5%</strong> from last
              period
            </p>
          </div>
          <div class="flex gap-1 bg-neutral-100 rounded-lg p-1">
            <button class="px-4 py-1.5 text-xs font-semibold bg-neutral-900 text-white rounded-md">Week</button>
            <button class="px-4 py-1.5 text-xs font-semibold text-neutral-500 hover:text-neutral-700">Month</button>
            <button class="px-4 py-1.5 text-xs font-semibold text-neutral-500 hover:text-neutral-700">Year</button>
          </div>
        </div>
        <div class="relative h-52">
          <svg class="w-full h-full" viewBox="0 0 1000 300" preserveAspectRatio="none">
            <defs>
              <linearGradient id="chartGradient" x1="0" x2="0" y1="0" y2="1">
                <stop offset="0%" stop-color="#f97316" stop-opacity="0.3" />
                <stop offset="100%" stop-color="#f97316" stop-opacity="0" />
              </linearGradient>
            </defs>
            <g stroke="#e5e5e5" stroke-width="1">
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
              fill="none" stroke="#f97316" stroke-width="3" stroke-linecap="round" vector-effect="non-scaling-stroke" />
            <g>
              <circle cx="200" cy="160" r="6" fill="#f97316" stroke="#fff" stroke-width="2" />
              <circle cx="400" cy="180" r="6" fill="#f97316" stroke="#fff" stroke-width="2" />
              <circle cx="600" cy="80" r="6" fill="#f97316" stroke="#fff" stroke-width="2" />
              <circle cx="800" cy="100" r="6" fill="#f97316" stroke="#fff" stroke-width="2" />
              <circle cx="1000" cy="30" r="6" fill="#f97316" stroke="#fff" stroke-width="2" />
            </g>
          </svg>
        </div>
        <div class="flex justify-between mt-3 text-[10px] font-semibold text-neutral-400 uppercase px-4">
          <span>Mon</span><span>Tue</span><span>Wed</span><span>Thu</span><span>Fri</span><span>Sat</span><span>Sun</span>
        </div>
      </div>

      <!-- Activity Feed -->
      <div class="bg-white rounded-2xl p-6 border border-neutral-200">
        <div class="flex items-center justify-between mb-6">
          <div>
            <h2 class="text-lg font-bold text-neutral-900">Recent Activity</h2>
            <p class="text-xs text-neutral-500">Latest updates from your store</p>
          </div>
          <button @click="clearActivities" class="text-xs font-semibold text-neutral-400 hover:text-neutral-600">Clear
            All</button>
        </div>
        <div v-if="recentActivities.length === 0" class="flex flex-col items-center justify-center py-12 text-center">
          <div class="w-14 h-14 flex items-center justify-center bg-neutral-100 rounded-2xl text-neutral-400 mb-4">
            <Activity class="w-6 h-6" />
          </div>
          <p class="text-sm text-neutral-500">No recent activities</p>
        </div>
        <div v-else class="space-y-4 max-h-72 overflow-y-auto pr-2">
          <div v-for="activity in recentActivities" :key="activity.id"
            class="flex items-start gap-3 p-3 bg-neutral-50 rounded-xl hover:bg-neutral-100 transition-colors">
            <div
              :class="['w-9 h-9 flex items-center justify-center rounded-xl shrink-0', getActivityClass(activity.type)]">
              <ShoppingBag v-if="activity.type === 'order'" class="w-4 h-4" />
              <Users v-else-if="activity.type === 'user'" class="w-4 h-4" />
              <Box v-else-if="activity.type === 'stock'" class="w-4 h-4" />
              <Activity v-else class="w-4 h-4" />
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-sm font-medium text-neutral-700 leading-snug">{{ activity.message }}</p>
              <span class="flex items-center gap-1 text-[10px] text-neutral-400 mt-1">
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
      <div class="bg-white rounded-2xl p-6 border border-neutral-200">
        <div class="flex items-center justify-between mb-6">
          <div>
            <h2 class="text-lg font-bold text-neutral-900">Recent Orders</h2>
            <p class="text-xs text-neutral-500">{{ orders.length }} orders this month</p>
          </div>
          <router-link to="/admin/orders"
            class="flex items-center gap-1.5 text-xs font-semibold text-orange-500 hover:text-orange-600">View All
            <ArrowRight class="w-4 h-4" />
          </router-link>
        </div>
        <div v-if="orders.length === 0" class="flex flex-col items-center justify-center py-12 text-center">
          <ShoppingBag class="w-8 h-8 text-neutral-300 mb-3" />
          <p class="text-sm text-neutral-500">No recent orders found</p>
        </div>
        <div v-else class="space-y-3">
          <div v-for="order in orders.slice(0, 5)" :key="order.id"
            class="flex items-center justify-between p-4 bg-neutral-50 rounded-xl hover:bg-neutral-100 transition-colors">
            <div>
              <p class="text-sm font-bold text-neutral-900"><span class="text-neutral-400">#</span>{{ order.orderNumber
                }}</p>
              <p class="text-[10px] text-neutral-500 mt-0.5">{{ order.items }} items • {{ formatDate(order.date) }}</p>
            </div>
            <div class="text-right">
              <p class="text-sm font-bold text-neutral-900">${{ order.total.toFixed(2) }}</p>
              <span
                :class="['inline-block mt-1 px-2 py-0.5 rounded-full text-[9px] font-bold uppercase', getStatusClass(order.status)]">{{
                order.status }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Top Products -->
      <div class="bg-white rounded-2xl p-6 border border-neutral-200">
        <div class="flex items-center justify-between mb-6">
          <div>
            <h2 class="text-lg font-bold text-neutral-900">Top Products</h2>
            <p class="text-xs text-neutral-500">Best performers this week</p>
          </div>
          <button class="p-2 rounded-lg text-neutral-400 hover:bg-neutral-100 hover:text-neutral-600">
            <MoreHorizontal class="w-4 h-4" />
          </button>
        </div>
        <div v-if="products.length === 0" class="flex flex-col items-center justify-center py-12 text-center">
          <Package class="w-8 h-8 text-neutral-300 mb-3" />
          <p class="text-sm text-neutral-500">No products found</p>
        </div>
        <div v-else class="space-y-3">
          <div v-for="(product, index) in products.slice(0, 5)" :key="product.id"
            class="flex items-center gap-3 p-4 bg-neutral-50 rounded-xl hover:bg-neutral-100 transition-colors">
            <div
              :class="['w-8 h-8 flex items-center justify-center rounded-lg text-xs font-bold', index === 0 ? 'bg-orange-100 text-orange-600' : index === 1 ? 'bg-neutral-200 text-neutral-600' : index === 2 ? 'bg-neutral-300 text-neutral-700' : 'bg-neutral-100 text-neutral-500']">
              <Star v-if="index < 3" class="w-3 h-3" />
              <span v-else>{{ index + 1 }}</span>
            </div>
            <div
              class="w-10 h-10 flex items-center justify-center bg-neutral-900 text-white font-bold text-sm rounded-xl">
              {{ product.name.charAt(0) }}</div>
            <div class="flex-1 min-w-0">
              <p class="text-sm font-semibold text-neutral-800 truncate">{{ product.name }}</p>
              <p class="text-[10px] text-neutral-500">Stock: {{ product.stock }}</p>
            </div>
            <p class="text-sm font-bold text-neutral-900">${{ product.price }}</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>