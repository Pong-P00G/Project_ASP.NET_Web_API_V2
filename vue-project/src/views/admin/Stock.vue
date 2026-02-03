<script setup>
import { ref, computed, onMounted } from 'vue'
import { Package, Search, Edit2, Loader2, AlertCircle, CheckCircle, TrendingDown, RefreshCw, X, Database, ArrowUpDown, AlertTriangle, BarChart3 } from 'lucide-vue-next'
import { productAPI } from '../../api/productsApi'
import { useToast } from '../../composables/useToast'

const toast = useToast()

const products = ref([])
const loading = ref(false)
const searchTerm = ref('')
const selectedStockStatus = ref('all')
const sortBy = ref('name')
const sortOrder = ref('asc')
const showUpdateModal = ref(false)
const currentProduct = ref(null)
const newStockValue = ref(0)
const updating = ref(false)

const stats = computed(() => ({ totalItems: products.value.length, lowStock: products.value.filter(p => p.stock > 0 && p.stock <= 10).length, outOfStock: products.value.filter(p => p.stock === 0).length, totalValue: products.value.reduce((sum, p) => sum + (p.stock * p.price), 0) }))

const filteredProducts = computed(() => {
  let filtered = products.value
  if (searchTerm.value) { const term = searchTerm.value.toLowerCase(); filtered = filtered.filter(p => p.name.toLowerCase().includes(term)) }
  if (selectedStockStatus.value === 'low') filtered = filtered.filter(p => p.stock > 0 && p.stock <= 10)
  else if (selectedStockStatus.value === 'out') filtered = filtered.filter(p => p.stock === 0)
  else if (selectedStockStatus.value === 'healthy') filtered = filtered.filter(p => p.stock > 10)
  return [...filtered].sort((a, b) => sortOrder.value === 'asc' ? a[sortBy.value] - b[sortBy.value] : b[sortBy.value] - a[sortBy.value])
})

const fetchProducts = async () => {
  try { loading.value = true; const response = await productAPI.getAllProducts({ pageSize: 100 }); products.value = (response.data.items || []).map(p => ({ id: p.productId, name: p.productName, category: p.categories?.[0]?.categoryName || 'N/A', sku: p.sku, price: p.basePrice, stock: p.stock })) } catch (err) { toast.error('Failed to fetch products') } finally { loading.value = false }
}

const openUpdateModal = (p) => { currentProduct.value = p; newStockValue.value = p.stock; showUpdateModal.value = true }

const handleUpdateStock = async () => {
  try { updating.value = true; await productAPI.updateStock(currentProduct.value.id, newStockValue.value); const idx = products.value.findIndex(p => p.id === currentProduct.value.id); if (idx !== -1) products.value[idx].stock = newStockValue.value; toast.success('Stock updated successfully!'); showUpdateModal.value = false } catch (err) { toast.error('Failed to update stock') } finally { updating.value = false }
}

const toggleSort = (field) => { if (sortBy.value === field) sortOrder.value = sortOrder.value === 'asc' ? 'desc' : 'asc'; else { sortBy.value = field; sortOrder.value = 'asc' } }
const getStockClass = (s) => s === 0 ? 'bg-red-500/20 text-red-400 border border-red-500/30' : s <= 10 ? 'bg-orange-500/20 text-orange-400 border border-orange-500/30' : 'bg-emerald-500/20 text-emerald-400 border border-emerald-500/30'
const getStockBar = (s) => s === 0 ? 'bg-red-500' : s <= 10 ? 'bg-orange-500' : 'bg-emerald-500'

onMounted(fetchProducts)
</script>

<template>
  <div class="w-full">
    <!-- Header -->
    <div class="flex flex-wrap items-center justify-between gap-4 mb-8">
      <div>
        <div
          class="inline-flex items-center gap-2 px-4 py-2 bg-gradient-to-r from-teal-500/20 to-cyan-500/20 backdrop-blur-sm border border-teal-500/20 rounded-full text-teal-400 text-xs font-semibold mb-3">
          <Database class="w-3.5 h-3.5" /><span>Stock Inventory</span>
        </div>
        <h1 class="text-3xl font-black text-white tracking-tight">Inventory</h1>
        <p class="text-neutral-400 text-sm mt-1">Real-time warehouse monitoring</p>
      </div>
      <button @click="fetchProducts" :disabled="loading"
        class="flex items-center gap-2 px-5 py-3 bg-white/5 backdrop-blur-sm border border-white/10 rounded-xl text-sm font-semibold text-white hover:bg-white/10 transition-all">
        <RefreshCw class="w-4 h-4" :class="{ 'animate-spin': loading }" />Sync
      </button>
    </div>

    <!-- Stats -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5 mb-8">
      <div
        class="stat-card group relative bg-white/5 backdrop-blur-xl rounded-2xl p-6 border border-white/10 hover:border-teal-500/50 hover:-translate-y-2 hover:shadow-2xl hover:shadow-teal-500/10 transition-all duration-500">
        <div
          class="absolute inset-0 bg-gradient-to-br from-teal-500/10 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-500 rounded-2xl">
        </div>
        <div class="relative z-10 flex items-center gap-4">
          <div
            class="w-14 h-14 flex items-center justify-center bg-gradient-to-br from-teal-400 to-cyan-500 rounded-2xl text-white shadow-lg shadow-teal-500/30 group-hover:scale-110 transition-transform duration-300">
            <Package class="w-6 h-6" />
          </div>
          <div>
            <p class="text-xs font-semibold text-neutral-400 uppercase tracking-widest">Total SKUs</p>
            <h3 class="text-3xl font-black text-white tracking-tight">{{ stats.totalItems }}</h3>
          </div>
        </div>
      </div>
      <div
        class="stat-card group relative bg-white/5 backdrop-blur-xl rounded-2xl p-6 border border-white/10 hover:border-orange-500/50 hover:-translate-y-2 hover:shadow-2xl hover:shadow-orange-500/10 transition-all duration-500">
        <div
          class="absolute inset-0 bg-gradient-to-br from-orange-500/10 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-500 rounded-2xl">
        </div>
        <div class="relative z-10 flex items-center gap-4">
          <div
            class="w-14 h-14 flex items-center justify-center bg-gradient-to-br from-orange-400 to-orange-600 rounded-2xl text-white shadow-lg shadow-orange-500/30 group-hover:scale-110 transition-transform duration-300">
            <AlertTriangle class="w-6 h-6" />
          </div>
          <div>
            <p class="text-xs font-semibold text-neutral-400 uppercase tracking-widest">Low Stock</p>
            <h3 class="text-3xl font-black text-white tracking-tight">{{ stats.lowStock }}</h3>
          </div>
        </div>
      </div>
      <div
        class="stat-card group relative bg-white/5 backdrop-blur-xl rounded-2xl p-6 border border-white/10 hover:border-red-500/50 hover:-translate-y-2 hover:shadow-2xl hover:shadow-red-500/10 transition-all duration-500">
        <div
          class="absolute inset-0 bg-gradient-to-br from-red-500/10 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-500 rounded-2xl">
        </div>
        <div class="relative z-10 flex items-center gap-4">
          <div
            class="w-14 h-14 flex items-center justify-center bg-gradient-to-br from-red-400 to-red-600 rounded-2xl text-white shadow-lg shadow-red-500/30 group-hover:scale-110 transition-transform duration-300">
            <TrendingDown class="w-6 h-6" />
          </div>
          <div>
            <p class="text-xs font-semibold text-neutral-400 uppercase tracking-widest">Out of Stock</p>
            <h3 class="text-3xl font-black text-white tracking-tight">{{ stats.outOfStock }}</h3>
          </div>
        </div>
      </div>
      <div
        class="stat-card group relative bg-white/5 backdrop-blur-xl rounded-2xl p-6 border border-white/10 hover:border-emerald-500/50 hover:-translate-y-2 hover:shadow-2xl hover:shadow-emerald-500/10 transition-all duration-500">
        <div
          class="absolute inset-0 bg-gradient-to-br from-emerald-500/10 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-500 rounded-2xl">
        </div>
        <div class="relative z-10 flex items-center gap-4">
          <div
            class="w-14 h-14 flex items-center justify-center bg-gradient-to-br from-emerald-400 to-emerald-600 rounded-2xl text-white shadow-lg shadow-emerald-500/30 group-hover:scale-110 transition-transform duration-300">
            <BarChart3 class="w-6 h-6" />
          </div>
          <div>
            <p class="text-xs font-semibold text-neutral-400 uppercase tracking-widest">Total Value</p>
            <h3 class="text-3xl font-black text-white tracking-tight">${{ stats.totalValue.toLocaleString() }}</h3>
          </div>
        </div>
      </div>
    </div>

    <!-- Filters -->
    <div
      class="glass-card bg-white/5 backdrop-blur-xl rounded-2xl p-5 mb-8 border border-white/10 flex flex-col md:flex-row md:items-center gap-4">
      <div class="relative flex-1">
        <Search class="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-neutral-500" />
        <input v-model="searchTerm" type="text" placeholder="Search products..."
          class="w-full pl-10 pr-4 py-3 bg-white/5 border border-white/10 rounded-xl text-sm text-white placeholder-neutral-500 focus:bg-white/10 focus:border-teal-500/50 outline-none transition-all" />
      </div>
      <select v-model="selectedStockStatus"
        class="px-4 py-3 bg-white/5 border border-white/10 rounded-xl text-xs font-bold text-neutral-300 focus:border-teal-500/50 outline-none">
        <option value="all" class="bg-neutral-900">All Stock</option>
        <option value="healthy" class="bg-neutral-900">Healthy</option>
        <option value="low" class="bg-neutral-900">Low Stock</option>
        <option value="out" class="bg-neutral-900">Critical</option>
      </select>
    </div>

    <!-- Table Card -->
    <div class="glass-card bg-white/5 backdrop-blur-xl rounded-3xl border border-white/10 overflow-hidden">
      <!-- Loading -->
      <div v-if="loading" class="flex items-center justify-center py-8 text-sm text-neutral-400">
        <Loader2 class="w-5 h-5 animate-spin mr-3 text-teal-400" />Loading inventory...
      </div>

      <!-- Mobile Cards -->
      <div class="lg:hidden grid grid-cols-1 sm:grid-cols-2 gap-4 p-4">
        <div v-for="p in filteredProducts" :key="p.id"
          class="bg-white/5 rounded-xl p-5 border border-white/10 hover:bg-white/10 transition-all">
          <div class="flex items-center gap-3 mb-4">
            <div
              class="w-12 h-12 flex items-center justify-center bg-gradient-to-br from-teal-500 to-cyan-500 rounded-xl text-white shadow-lg shadow-teal-500/20">
              <Package class="w-6 h-6" />
            </div>
            <div class="flex-1">
              <p class="text-sm font-bold text-white truncate">{{ p.name }}</p>
              <span class="text-xs text-neutral-500">{{ p.category }}</span>
            </div>
          </div>
          <div class="bg-white/5 rounded-xl p-4 mb-4 border border-white/10">
            <div class="flex justify-between mb-2">
              <p class="text-xs font-bold text-neutral-400 uppercase">Stock Level</p>
              <p class="text-xl font-bold text-white">{{ p.stock }}</p>
            </div>
            <div class="h-2 bg-white/10 rounded-full overflow-hidden">
              <div :class="['h-full rounded-full transition-all duration-500', getStockBar(p.stock)]"
                :style="{ width: Math.min((p.stock / 100) * 100, 100) + '%' }"></div>
            </div>
          </div>
          <div class="flex items-center justify-between">
            <div>
              <p class="text-xs font-bold text-neutral-500">Unit Price</p>
              <p class="text-sm font-bold text-white">${{ p.price }}</p>
            </div>
            <button @click="openUpdateModal(p)"
              class="flex items-center gap-1.5 px-4 py-2 bg-gradient-to-r from-teal-500 to-cyan-500 text-white rounded-lg text-xs font-bold shadow-lg shadow-teal-500/25 hover:shadow-teal-500/40 transition-all">
              <Edit2 class="w-4 h-4" />Update
            </button>
          </div>
        </div>
      </div>

      <!-- Table -->
      <table v-if="filteredProducts.length > 0" class="hidden lg:table w-full">
        <thead>
          <tr class="bg-white/5">
            <th class="px-6 py-4 text-left">
              <button @click="toggleSort('name')"
                class="flex items-center gap-1.5 text-xs font-semibold text-neutral-400 uppercase tracking-wider hover:text-teal-400 transition-colors">
                Product
                <ArrowUpDown class="w-3 h-3" />
              </button>
            </th>
            <th class="px-6 py-4 text-left text-xs font-semibold text-neutral-400 uppercase tracking-wider">Category
            </th>
            <th class="px-6 py-4 text-left">
              <button @click="toggleSort('price')"
                class="flex items-center gap-1.5 text-xs font-semibold text-neutral-400 uppercase tracking-wider hover:text-teal-400 transition-colors">
                Price
                <ArrowUpDown class="w-3 h-3" />
              </button>
            </th>
            <th class="px-6 py-4 text-left">
              <button @click="toggleSort('stock')"
                class="flex items-center gap-1.5 text-xs font-semibold text-neutral-400 uppercase tracking-wider hover:text-teal-400 transition-colors">
                Stock Level
                <ArrowUpDown class="w-3 h-3" />
              </button>
            </th>
            <th class="px-6 py-4 text-right text-xs font-semibold text-neutral-400 uppercase tracking-wider">Action</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="p in filteredProducts" :key="p.id"
            class="border-b border-white/5 hover:bg-white/5 transition-colors group">
            <td class="px-6 py-4">
              <div class="flex items-center gap-3">
                <div
                  class="w-11 h-11 flex items-center justify-center bg-gradient-to-br from-teal-500/50 to-cyan-500/50 rounded-xl text-teal-400 group-hover:scale-110 transition-transform duration-300">
                  <Package class="w-5 h-5" />
                </div>
                <p class="text-sm font-semibold text-white group-hover:text-teal-400 transition-colors">{{ p.name }}</p>
              </div>
            </td>
            <td class="px-6 py-4 text-sm text-neutral-400">{{ p.category }}</td>
            <td class="px-6 py-4 text-sm font-bold text-white">${{ p.price }}</td>
            <td class="px-6 py-4">
              <div class="flex items-center gap-3">
                <div class="w-20 h-1.5 bg-white/10 rounded-full overflow-hidden">
                  <div :class="['h-full rounded-full transition-all duration-500', getStockBar(p.stock)]"
                    :style="{ width: Math.min((p.stock / 100) * 100, 100) + '%' }"></div>
                </div>
                <span :class="['px-2.5 py-1 rounded-lg text-xs font-bold uppercase', getStockClass(p.stock)]">{{ p.stock
                  }}</span>
              </div>
            </td>
            <td class="px-6 py-4">
              <div class="flex justify-end opacity-0 group-hover:opacity-100 transition-opacity">
                <button @click="openUpdateModal(p)"
                  class="p-2 rounded-lg text-neutral-400 hover:bg-teal-500/20 hover:text-teal-400 transition-all">
                  <Edit2 class="w-4 h-4" />
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>

      <!-- Empty State -->
      <div v-if="filteredProducts.length === 0 && !loading"
        class="flex flex-col items-center justify-center py-16 text-center">
        <div
          class="w-20 h-20 flex items-center justify-center bg-white/5 rounded-2xl text-neutral-500 mb-6 border border-white/10">
          <Database class="w-10 h-10" />
        </div>
        <h3 class="text-xl font-bold text-white mb-2">No Items Found</h3>
        <p class="text-neutral-400 text-sm mb-6">No products match your criteria</p>
        <button @click="searchTerm = ''; selectedStockStatus = 'all'"
          class="px-6 py-3 bg-gradient-to-r from-teal-500 to-cyan-500 rounded-xl text-sm font-bold text-white shadow-lg shadow-teal-500/25">Clear
          Filters</button>
      </div>
    </div>

    <!-- Update Stock Modal -->
    <Teleport to="body">
      <Transition name="modal">
        <div v-if="showUpdateModal"
          class="fixed inset-0 bg-black/70 backdrop-blur-md flex items-center justify-center p-6 z-50"
          @click.self="showUpdateModal = false">
          <div class="bg-neutral-900 border border-white/10 rounded-3xl w-full max-w-sm shadow-2xl overflow-hidden">
            <div class="flex items-center justify-between p-6 border-b border-white/10">
              <div class="flex items-center gap-4">
                <div
                  class="w-11 h-11 flex items-center justify-center bg-gradient-to-br from-teal-500 to-cyan-500 rounded-xl text-white shadow-lg shadow-teal-500/25">
                  <Database class="w-5 h-5" />
                </div>
                <div>
                  <h3 class="text-lg font-bold text-white">Update Stock</h3>
                  <p class="text-xs font-semibold text-neutral-400 uppercase tracking-wider">{{ currentProduct?.name }}
                  </p>
                </div>
              </div>
              <button @click="showUpdateModal = false"
                class="p-2 rounded-lg text-neutral-400 hover:bg-white/10 hover:text-white transition-all">
                <X class="w-5 h-5" />
              </button>
            </div>
            <div class="p-6">
              <label class="block text-xs font-bold text-neutral-400 uppercase tracking-wider mb-3">Quantity</label>
              <input v-model.number="newStockValue" type="number" min="0"
                class="w-full px-4 py-4 bg-white/5 border border-white/10 rounded-xl text-lg font-bold text-white focus:bg-white/10 focus:border-teal-500/50 outline-none transition-all" />
            </div>
            <div class="flex gap-3 p-6 bg-white/5 border-t border-white/10">
              <button @click="showUpdateModal = false"
                class="flex-1 py-3.5 bg-white/5 border border-white/10 rounded-xl text-sm font-semibold text-neutral-300 hover:bg-white/10 transition-all">Cancel</button>
              <button @click="handleUpdateStock" :disabled="updating"
                class="flex-1 py-3.5 bg-gradient-to-r from-teal-500 to-cyan-500 rounded-xl text-sm font-bold text-white flex items-center justify-center gap-2 shadow-lg shadow-teal-500/25 disabled:opacity-50">
                <Loader2 v-if="updating" class="w-4 h-4 animate-spin" />{{ updating ? 'Saving...' : 'Confirm Update' }}
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<style scoped>
.modal-enter-active,
.modal-leave-active {
  transition: opacity 0.3s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}

.glass-card {
  box-shadow: 0 0 0 1px rgba(255, 255, 255, 0.05), 0 25px 50px -12px rgba(0, 0, 0, 0.25);
}

.stat-card {
  box-shadow: 0 0 0 1px rgba(255, 255, 255, 0.05), 0 10px 40px -10px rgba(0, 0, 0, 0.3);
}
</style>
