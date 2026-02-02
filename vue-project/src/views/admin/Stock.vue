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
const getStockClass = (s) => s === 0 ? 'bg-red-100 text-red-600' : s <= 10 ? 'bg-orange-100 text-orange-600' : 'bg-green-100 text-green-600'
const getStockBar = (s) => s === 0 ? 'bg-red-500' : s <= 10 ? 'bg-orange-500' : 'bg-green-500'

onMounted(fetchProducts)
</script>

<template>
  <div class="w-full">
    <div class="flex flex-wrap items-center justify-between gap-4 mb-8">
      <div>
        <div
          class="inline-flex items-center gap-2 px-3 py-1.5 bg-neutral-900 rounded-full text-white text-xs font-semibold mb-2">
          <Database class="w-3.5 h-3.5" /><span>Stock Inventory</span>
        </div>
        <h1 class="text-2xl font-bold text-neutral-900">Inventory</h1>
        <p class="text-neutral-500 text-sm">Real-time warehouse monitoring</p>
      </div>
      <button @click="fetchProducts" :disabled="loading"
        class="flex items-center gap-2 px-5 py-2.5 bg-neutral-900 rounded-xl text-sm font-semibold text-white hover:bg-neutral-800">
        <RefreshCw class="w-4 h-4" :class="{ 'animate-spin': loading }" />Sync
      </button>
    </div>

    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5 mb-8">
      <div
        class="bg-white rounded-2xl p-5 border border-neutral-200 hover:-translate-y-1 hover:shadow-lg transition-all">
        <div class="flex items-center gap-4">
          <div class="w-12 h-12 flex items-center justify-center bg-neutral-900 rounded-xl text-white">
            <Package class="w-5 h-5" />
          </div>
          <div>
            <p class="text-[10px] font-bold text-neutral-400 uppercase">SKUs</p>
            <h3 class="text-2xl font-bold text-neutral-900">{{ stats.totalItems }}</h3>
          </div>
        </div>
      </div>
      <div
        class="bg-white rounded-2xl p-5 border border-neutral-200 hover:-translate-y-1 hover:shadow-lg transition-all">
        <div class="flex items-center gap-4">
          <div class="w-12 h-12 flex items-center justify-center bg-orange-500 rounded-xl text-white">
            <AlertTriangle class="w-5 h-5" />
          </div>
          <div>
            <p class="text-[10px] font-bold text-neutral-400 uppercase">Low Stock</p>
            <h3 class="text-2xl font-bold text-neutral-900">{{ stats.lowStock }}</h3>
          </div>
        </div>
      </div>
      <div
        class="bg-white rounded-2xl p-5 border border-neutral-200 hover:-translate-y-1 hover:shadow-lg transition-all">
        <div class="flex items-center gap-4">
          <div class="w-12 h-12 flex items-center justify-center bg-red-500 rounded-xl text-white">
            <TrendingDown class="w-5 h-5" />
          </div>
          <div>
            <p class="text-[10px] font-bold text-neutral-400 uppercase">Out of Stock</p>
            <h3 class="text-2xl font-bold text-neutral-900">{{ stats.outOfStock }}</h3>
          </div>
        </div>
      </div>
      <div
        class="bg-white rounded-2xl p-5 border border-neutral-200 hover:-translate-y-1 hover:shadow-lg transition-all">
        <div class="flex items-center gap-4">
          <div class="w-12 h-12 flex items-center justify-center bg-green-500 rounded-xl text-white">
            <BarChart3 class="w-5 h-5" />
          </div>
          <div>
            <p class="text-[10px] font-bold text-neutral-400 uppercase">Value</p>
            <h3 class="text-2xl font-bold text-neutral-900">${{ stats.totalValue.toLocaleString() }}</h3>
          </div>
        </div>
      </div>
    </div>

    <div
      class="bg-white rounded-2xl p-5 mb-8 border border-neutral-200 flex flex-col md:flex-row md:items-center gap-4">
      <div class="relative flex-1">
        <Search class="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-neutral-400" /><input v-model="searchTerm"
          type="text" placeholder="Search..."
          class="w-full pl-10 pr-4 py-3 bg-neutral-50 border border-neutral-200 rounded-xl text-sm focus:bg-white focus:border-orange-500 outline-none" />
      </div>
      <select v-model="selectedStockStatus"
        class="px-4 py-3 bg-neutral-50 border border-neutral-200 rounded-xl text-xs font-bold text-neutral-600">
        <option value="all">All</option>
        <option value="healthy">Healthy</option>
        <option value="low">Low</option>
        <option value="out">Critical</option>
      </select>
    </div>

    <div class="bg-white rounded-2xl border border-neutral-200 overflow-hidden">
      <div v-if="loading" class="flex items-center justify-center py-4 text-xs text-neutral-400">
        <Loader2 class="w-4 h-4 animate-spin mr-2" />Loading...
      </div>
      <div class="lg:hidden grid grid-cols-1 sm:grid-cols-2 gap-4 p-4">
        <div v-for="p in filteredProducts" :key="p.id"
          class="bg-neutral-50 rounded-xl p-5 hover:bg-neutral-100 transition-colors">
          <div class="flex items-center gap-3 mb-4">
            <div class="w-12 h-12 flex items-center justify-center bg-white rounded-xl text-neutral-400">
              <Package class="w-6 h-6" />
            </div>
            <div class="flex-1">
              <p class="text-sm font-bold text-neutral-800 truncate">{{ p.name }}</p><span
                class="text-[10px] text-neutral-400">{{ p.category }}</span>
            </div>
          </div>
          <div class="bg-white rounded-xl p-4 mb-4">
            <div class="flex justify-between mb-2">
              <p class="text-[10px] font-bold text-neutral-400 uppercase">Stock</p>
              <p class="text-xl font-bold text-neutral-900">{{ p.stock }}</p>
            </div>
            <div class="h-2 bg-neutral-200 rounded-full overflow-hidden">
              <div :class="['h-full rounded-full', getStockBar(p.stock)]"
                :style="{ width: Math.min((p.stock / 100) * 100, 100) + '%' }"></div>
            </div>
          </div>
          <div class="flex items-center justify-between">
            <div>
              <p class="text-[10px] font-bold text-neutral-400">Price</p>
              <p class="text-sm font-bold text-neutral-800">${{ p.price }}</p>
            </div><button @click="openUpdateModal(p)"
              class="flex items-center gap-1.5 px-4 py-2 bg-neutral-900 text-white rounded-lg text-xs font-semibold">
              <Edit2 class="w-4 h-4" />Edit
            </button>
          </div>
        </div>
      </div>
      <table class="hidden lg:table w-full">
        <thead>
          <tr class="bg-neutral-50/50">
            <th class="px-6 py-4 text-left"><button @click="toggleSort('name')"
                class="flex items-center gap-1.5 text-[10px] font-bold text-neutral-400 uppercase hover:text-orange-500">Product
                <ArrowUpDown class="w-3 h-3" />
              </button></th>
            <th class="px-6 py-4 text-left text-[10px] font-bold text-neutral-400 uppercase">Category</th>
            <th class="px-6 py-4 text-left"><button @click="toggleSort('price')"
                class="flex items-center gap-1.5 text-[10px] font-bold text-neutral-400 uppercase hover:text-orange-500">Price
                <ArrowUpDown class="w-3 h-3" />
              </button></th>
            <th class="px-6 py-4 text-left"><button @click="toggleSort('stock')"
                class="flex items-center gap-1.5 text-[10px] font-bold text-neutral-400 uppercase hover:text-orange-500">Stock
                <ArrowUpDown class="w-3 h-3" />
              </button></th>
            <th class="px-6 py-4 text-right text-[10px] font-bold text-neutral-400 uppercase">Action</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="p in filteredProducts" :key="p.id"
            class="border-b border-neutral-50 hover:bg-neutral-50/50 transition-colors group">
            <td class="px-6 py-4">
              <div class="flex items-center gap-3">
                <div class="w-11 h-11 flex items-center justify-center bg-white rounded-xl text-neutral-400 shadow-sm">
                  <Package class="w-5 h-5" />
                </div>
                <p class="text-sm font-bold text-neutral-800">{{ p.name }}</p>
              </div>
            </td>
            <td class="px-6 py-4 text-sm text-neutral-500">{{ p.category }}</td>
            <td class="px-6 py-4 text-sm font-bold text-neutral-800">${{ p.price }}</td>
            <td class="px-6 py-4">
              <div class="flex items-center gap-3">
                <div class="w-20 h-1.5 bg-neutral-200 rounded-full overflow-hidden">
                  <div :class="['h-full rounded-full', getStockBar(p.stock)]"
                    :style="{ width: Math.min((p.stock / 100) * 100, 100) + '%' }"></div>
                </div><span
                  :class="['px-2.5 py-1 rounded-lg text-[10px] font-bold uppercase', getStockClass(p.stock)]">{{ p.stock
                  }}</span>
              </div>
            </td>
            <td class="px-6 py-4">
              <div class="flex justify-end opacity-0 group-hover:opacity-100"><button @click="openUpdateModal(p)"
                  class="p-2 rounded-lg text-neutral-400 hover:bg-orange-100 hover:text-orange-600">
                  <Edit2 class="w-4 h-4" />
                </button></div>
            </td>
          </tr>
        </tbody>
      </table>
      <div v-if="filteredProducts.length === 0 && !loading" class="flex flex-col items-center py-16 text-center">
        <div class="w-20 h-20 flex items-center justify-center bg-neutral-100 rounded-2xl text-neutral-400 mb-6">
          <Database class="w-10 h-10" />
        </div>
        <h3 class="text-xl font-bold text-neutral-800 mb-2">No Items</h3>
        <p class="text-neutral-500 text-sm mb-6">No products match criteria.</p><button
          @click="searchTerm = ''; selectedStockStatus = 'all'"
          class="px-6 py-3 bg-neutral-900 rounded-xl text-sm font-semibold text-white">Clear</button>
      </div>
    </div>

    <Teleport to="body">
      <Transition name="modal">
        <div v-if="showUpdateModal"
          class="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center p-6 z-50"
          @click.self="showUpdateModal = false">
          <div class="bg-white rounded-2xl w-full max-w-sm shadow-2xl overflow-hidden">
            <div class="flex items-center justify-between p-6 border-b border-neutral-100">
              <div class="flex items-center gap-4">
                <div class="w-11 h-11 flex items-center justify-center bg-neutral-900 rounded-xl text-white">
                  <Database class="w-5 h-5" />
                </div>
                <div>
                  <h3 class="text-lg font-bold text-neutral-800">Update Stock</h3>
                  <p class="text-[10px] font-semibold text-neutral-400 uppercase">{{ currentProduct?.name }}</p>
                </div>
              </div><button @click="showUpdateModal = false"
                class="p-2 rounded-lg text-neutral-400 hover:bg-neutral-100">
                <X class="w-5 h-5" />
              </button>
            </div>
            <div class="p-6"><label
                class="block text-[10px] font-bold text-neutral-400 uppercase mb-3">Quantity</label><input
                v-model.number="newStockValue" type="number" min="0"
                class="w-full px-4 py-4 bg-neutral-50 border-2 border-transparent rounded-xl text-lg font-bold text-neutral-800 focus:bg-white focus:border-orange-500 outline-none" />
            </div>
            <div class="flex gap-3 p-6 bg-neutral-50 border-t border-neutral-100"><button
                @click="showUpdateModal = false"
                class="flex-1 py-3.5 bg-white border border-neutral-200 rounded-xl text-sm font-semibold text-neutral-600">Cancel</button><button
                @click="handleUpdateStock" :disabled="updating"
                class="flex-1 py-3.5 bg-neutral-900 rounded-xl text-sm font-semibold text-white flex items-center justify-center gap-2">
                <Loader2 v-if="updating" class="w-4 h-4 animate-spin" />{{ updating ? 'Saving...' : 'Confirm' }}
              </button></div>
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
</style>
