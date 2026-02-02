<script setup>
import { ref, onMounted, computed } from 'vue'
import { orderApi } from '../../api/orderApi'
import { Package, Search, Eye, Truck, CheckCircle, X, Clock, Calendar, DollarSign, User, RefreshCw, Loader2, ShoppingBag, MapPin } from 'lucide-vue-next'

const orders = ref([])
const loading = ref(true)
const searchQuery = ref('')
const statusFilter = ref('ALL')
const selectedOrder = ref(null)
const isDetailModalOpen = ref(false)

const statusOptions = ['PENDING', 'PROCESSING', 'SHIPPED', 'DELIVERED', 'CANCELLED']

const fetchOrders = async () => {
    try { loading.value = true; const response = await orderApi.getAllOrdersForAdmin(); orders.value = response.data } catch (error) { console.error('Failed to fetch orders:', error) } finally { loading.value = false }
}

const updateStatus = async (orderId, newStatus) => {
    try { await orderApi.updateOrderStatus(orderId, newStatus); const orderIndex = orders.value.findIndex(o => o.orderId === orderId); if (orderIndex !== -1) orders.value[orderIndex].orderStatus = newStatus; if (selectedOrder.value?.orderId === orderId) selectedOrder.value.orderStatus = newStatus } catch (error) { console.error('Failed to update status:', error) }
}

const viewDetails = (order) => { selectedOrder.value = order; isDetailModalOpen.value = true }
const closeModal = () => { isDetailModalOpen.value = false; selectedOrder.value = null }

const filteredOrders = computed(() => {
    let result = orders.value
    if (statusFilter.value !== 'ALL') result = result.filter(o => o.orderStatus === statusFilter.value)
    if (searchQuery.value) { const query = searchQuery.value.toLowerCase(); result = result.filter(o => o.orderNumber.toLowerCase().includes(query) || o.shippingAddress.toLowerCase().includes(query)) }
    return result
})

const stats = computed(() => ({ total: orders.value.length, pending: orders.value.filter(o => o.orderStatus === 'PENDING').length, processing: orders.value.filter(o => o.orderStatus === 'PROCESSING').length, revenue: orders.value.reduce((sum, o) => sum + o.totalAmount, 0) }))

const getStatusClass = (status) => {
    const map = { DELIVERED: 'bg-green-100 text-green-600', SHIPPED: 'bg-neutral-200 text-neutral-700', PROCESSING: 'bg-orange-100 text-orange-600', CANCELLED: 'bg-red-100 text-red-600' }
    return map[status] || 'bg-neutral-100 text-neutral-600'
}

const getStatusIcon = (status) => { const map = { DELIVERED: CheckCircle, SHIPPED: Truck, PROCESSING: Clock, CANCELLED: X }; return map[status] || Clock }

onMounted(fetchOrders)
</script>

<template>
    <div class="w-full">
        <!-- Header -->
        <div class="flex flex-wrap items-center justify-between gap-4 mb-8">
            <div>
                <div
                    class="inline-flex items-center gap-2 px-3 py-1.5 bg-neutral-900 rounded-full text-white text-xs font-semibold mb-2">
                    <ShoppingBag class="w-3.5 h-3.5" /><span>Order Management</span>
                </div>
                <h1 class="text-2xl font-bold text-neutral-900">Orders</h1>
                <p class="text-neutral-500 text-sm">Track and process customer orders</p>
            </div>
            <button @click="fetchOrders" :disabled="loading"
                class="flex items-center gap-2 px-5 py-2.5 bg-neutral-900 rounded-xl text-sm font-semibold text-white hover:bg-neutral-800 transition-all">
                <RefreshCw class="w-4 h-4" :class="{ 'animate-spin': loading }" />Sync
            </button>
        </div>

        <!-- Stats -->
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5 mb-8">
            <div
                class="bg-white rounded-2xl p-5 border border-neutral-200 hover:-translate-y-1 hover:shadow-lg transition-all">
                <div class="flex items-center gap-4">
                    <div class="w-12 h-12 flex items-center justify-center bg-neutral-900 rounded-xl text-white">
                        <ShoppingBag class="w-5 h-5" />
                    </div>
                    <div>
                        <p class="text-[10px] font-bold text-neutral-400 uppercase tracking-wider">Total Orders</p>
                        <h3 class="text-2xl font-bold text-neutral-900">{{ stats.total }}</h3>
                    </div>
                </div>
            </div>
            <div
                class="bg-white rounded-2xl p-5 border border-neutral-200 hover:-translate-y-1 hover:shadow-lg transition-all">
                <div class="flex items-center gap-4">
                    <div class="w-12 h-12 flex items-center justify-center bg-orange-500 rounded-xl text-white">
                        <Clock class="w-5 h-5" />
                    </div>
                    <div>
                        <p class="text-[10px] font-bold text-neutral-400 uppercase tracking-wider">Pending</p>
                        <h3 class="text-2xl font-bold text-neutral-900">{{ stats.pending }}</h3>
                    </div>
                </div>
            </div>
            <div
                class="bg-white rounded-2xl p-5 border border-neutral-200 hover:-translate-y-1 hover:shadow-lg transition-all">
                <div class="flex items-center gap-4">
                    <div class="w-12 h-12 flex items-center justify-center bg-neutral-700 rounded-xl text-white">
                        <Truck class="w-5 h-5" />
                    </div>
                    <div>
                        <p class="text-[10px] font-bold text-neutral-400 uppercase tracking-wider">Processing</p>
                        <h3 class="text-2xl font-bold text-neutral-900">{{ stats.processing }}</h3>
                    </div>
                </div>
            </div>
            <div
                class="bg-white rounded-2xl p-5 border border-neutral-200 hover:-translate-y-1 hover:shadow-lg transition-all">
                <div class="flex items-center gap-4">
                    <div class="w-12 h-12 flex items-center justify-center bg-green-500 rounded-xl text-white">
                        <DollarSign class="w-5 h-5" />
                    </div>
                    <div>
                        <p class="text-[10px] font-bold text-neutral-400 uppercase tracking-wider">Revenue</p>
                        <h3 class="text-2xl font-bold text-neutral-900">${{ stats.revenue.toLocaleString() }}</h3>
                    </div>
                </div>
            </div>
        </div>

        <!-- Filters -->
        <div
            class="bg-white rounded-2xl p-5 mb-8 border border-neutral-200 flex flex-col lg:flex-row lg:items-center gap-4">
            <div class="relative flex-1 max-w-sm">
                <Search class="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-neutral-400" />
                <input v-model="searchQuery" type="text" placeholder="Search order #, address..."
                    class="w-full pl-10 pr-4 py-3 bg-neutral-50 border border-neutral-200 rounded-xl text-sm focus:bg-white focus:border-orange-500 outline-none transition-all" />
            </div>
            <div class="flex flex-wrap gap-2">
                <button v-for="status in ['ALL', ...statusOptions]" :key="status" @click="statusFilter = status"
                    :class="['px-4 py-2 rounded-xl text-[10px] font-bold uppercase tracking-wide transition-all', statusFilter === status ? 'bg-neutral-900 text-white' : 'bg-white border border-neutral-200 text-neutral-500 hover:border-neutral-300']">{{
                    status }}</button>
            </div>
        </div>

        <!-- Table -->
        <div class="bg-white rounded-2xl border border-neutral-200 overflow-hidden">
            <div v-if="loading" class="flex flex-col items-center justify-center py-16">
                <Loader2 class="w-8 h-8 animate-spin text-orange-500" />
                <p class="mt-4 text-xs font-bold text-neutral-400 uppercase tracking-widest">Loading Orders...</p>
            </div>
            <div v-else-if="filteredOrders.length === 0"
                class="flex flex-col items-center justify-center py-16 text-center">
                <div
                    class="w-20 h-20 flex items-center justify-center bg-neutral-100 rounded-2xl text-neutral-400 mb-6">
                    <Package class="w-10 h-10" />
                </div>
                <h3 class="text-xl font-bold text-neutral-800 mb-2">No Orders Found</h3>
                <p class="text-neutral-500 text-sm">Try adjusting your search or filters</p>
            </div>
            <div v-else class="overflow-x-auto">
                <table class="w-full">
                    <thead>
                        <tr class="bg-neutral-50/50">
                            <th
                                class="px-6 py-4 text-left text-[10px] font-bold text-neutral-400 uppercase tracking-wider">
                                Order</th>
                            <th
                                class="px-6 py-4 text-left text-[10px] font-bold text-neutral-400 uppercase tracking-wider">
                                Customer</th>
                            <th
                                class="px-6 py-4 text-left text-[10px] font-bold text-neutral-400 uppercase tracking-wider">
                                Date</th>
                            <th
                                class="px-6 py-4 text-left text-[10px] font-bold text-neutral-400 uppercase tracking-wider">
                                Total</th>
                            <th
                                class="px-6 py-4 text-left text-[10px] font-bold text-neutral-400 uppercase tracking-wider">
                                Status</th>
                            <th
                                class="px-6 py-4 text-right text-[10px] font-bold text-neutral-400 uppercase tracking-wider">
                                Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="order in filteredOrders" :key="order.orderId"
                            class="border-b border-neutral-50 hover:bg-neutral-50/50 transition-colors group">
                            <td class="px-6 py-4"><span
                                    class="font-mono text-sm font-bold text-neutral-800 bg-neutral-100 px-3 py-1.5 rounded-lg">#{{
                                    order.orderNumber }}</span></td>
                            <td class="px-6 py-4">
                                <div class="flex items-center gap-3">
                                    <div
                                        class="w-8 h-8 flex items-center justify-center bg-neutral-200 text-neutral-600 font-bold text-xs rounded-lg">
                                        {{ order.shippingAddress.slice(0, 1) }}</div>
                                    <span class="text-sm text-neutral-600 max-w-[150px] truncate">{{
                                        order.shippingAddress }}</span>
                                </div>
                            </td>
                            <td class="px-6 py-4 text-sm text-neutral-500">{{ new
                                Date(order.createdAt).toLocaleDateString() }}</td>
                            <td class="px-6 py-4 text-sm font-bold text-neutral-800">${{ order.totalAmount.toFixed(2) }}
                            </td>
                            <td class="px-6 py-4">
                                <span
                                    :class="['inline-flex items-center gap-1.5 px-3 py-1.5 rounded-full text-[10px] font-bold uppercase', getStatusClass(order.orderStatus)]">
                                    <component :is="getStatusIcon(order.orderStatus)" class="w-3 h-3" />{{
                                    order.orderStatus }}
                                </span>
                            </td>
                            <td class="px-6 py-4">
                                <div class="flex justify-end opacity-0 group-hover:opacity-100 transition-opacity">
                                    <button @click="viewDetails(order)"
                                        class="p-2 rounded-lg text-neutral-400 hover:bg-orange-100 hover:text-orange-600">
                                        <Eye class="w-4 h-4" />
                                    </button></div>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>

        <!-- Order Detail Modal -->
        <Teleport to="body">
            <Transition name="modal">
                <div v-if="isDetailModalOpen && selectedOrder"
                    class="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center p-6 z-50"
                    @click.self="closeModal">
                    <div
                        class="bg-white rounded-2xl w-full max-w-2xl max-h-[90vh] overflow-hidden shadow-2xl flex flex-col">
                        <div class="flex items-center gap-4 p-6 bg-neutral-50 border-b border-neutral-100">
                            <div
                                class="w-11 h-11 flex items-center justify-center bg-neutral-900 rounded-xl text-white">
                                <ShoppingBag class="w-5 h-5" />
                            </div>
                            <div class="flex-1">
                                <h3 class="text-lg font-bold text-neutral-800">Order #{{ selectedOrder.orderNumber }}
                                </h3>
                                <p class="flex items-center gap-1.5 text-xs text-neutral-400">
                                    <Calendar class="w-3 h-3" />{{ new Date(selectedOrder.createdAt).toLocaleString() }}
                                </p>
                            </div>
                            <span
                                :class="['px-3 py-1.5 rounded-full text-[10px] font-bold uppercase', getStatusClass(selectedOrder.orderStatus)]">{{
                                selectedOrder.orderStatus }}</span>
                            <button @click="closeModal" class="p-2 rounded-lg text-neutral-400 hover:bg-neutral-100">
                                <X class="w-5 h-5" />
                            </button>
                        </div>
                        <div class="flex-1 overflow-y-auto p-6 space-y-6">
                            <!-- Info -->
                            <div class="bg-neutral-50 rounded-xl p-5">
                                <h4 class="text-[10px] font-bold text-neutral-400 uppercase tracking-widest mb-4">
                                    Delivery Information</h4>
                                <div class="grid sm:grid-cols-2 gap-4">
                                    <div class="flex items-start gap-3">
                                        <div
                                            class="w-9 h-9 flex items-center justify-center bg-white rounded-lg text-orange-500 shadow-sm">
                                            <User class="w-4 h-4" />
                                        </div>
                                        <div><span class="text-[10px] font-semibold text-neutral-400">Customer</span>
                                            <p class="text-sm font-semibold text-neutral-800">User #{{
                                                selectedOrder.userId }}</p>
                                        </div>
                                    </div>
                                    <div class="flex items-start gap-3">
                                        <div
                                            class="w-9 h-9 flex items-center justify-center bg-white rounded-lg text-orange-500 shadow-sm">
                                            <MapPin class="w-4 h-4" />
                                        </div>
                                        <div><span class="text-[10px] font-semibold text-neutral-400">Address</span>
                                            <p class="text-sm font-semibold text-neutral-800">{{
                                                selectedOrder.shippingAddress }}</p>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <!-- Items -->
                            <div>
                                <h4 class="text-[10px] font-bold text-neutral-400 uppercase tracking-widest mb-4">Order
                                    Items</h4>
                                <div class="space-y-3">
                                    <div v-for="item in selectedOrder.items" :key="item.orderItemId"
                                        class="flex items-center justify-between p-4 bg-neutral-50 rounded-xl hover:bg-neutral-100 transition-colors">
                                        <div class="flex items-center gap-3">
                                            <div
                                                class="w-11 h-11 flex items-center justify-center bg-white rounded-lg overflow-hidden">
                                                <img v-if="item.productImage" :src="item.productImage"
                                                    class="w-full h-full object-cover" />
                                                <Package v-else class="w-5 h-5 text-neutral-300" />
                                            </div>
                                            <div>
                                                <p class="text-sm font-semibold text-neutral-800">{{ item.productName }}
                                                </p>
                                                <p class="text-xs text-neutral-500">Qty: {{ item.quantity }} × ${{
                                                    item.unitPrice.toFixed(2) }}</p>
                                            </div>
                                        </div>
                                        <p class="text-sm font-bold text-neutral-800">${{ item.totalPrice.toFixed(2) }}
                                        </p>
                                    </div>
                                </div>
                                <div class="mt-4 pt-4 border-t border-dashed border-neutral-200 space-y-2">
                                    <div class="flex justify-between text-sm text-neutral-500"><span>Tax</span><span>${{
                                            selectedOrder.tax.toFixed(2) }}</span></div>
                                    <div class="flex justify-between text-sm text-neutral-500">
                                        <span>Shipping</span><span>${{ selectedOrder.shippingCost.toFixed(2) }}</span>
                                    </div>
                                    <div
                                        class="flex justify-between items-center p-4 mt-2 bg-neutral-900 rounded-xl text-white font-bold">
                                        <span>Total</span><span class="text-lg">${{ selectedOrder.totalAmount.toFixed(2)
                                            }}</span></div>
                                </div>
                            </div>
                            <!-- Status Update -->
                            <div>
                                <h4 class="text-[10px] font-bold text-neutral-400 uppercase tracking-widest mb-4">Update
                                    Status</h4>
                                <div class="grid grid-cols-2 sm:grid-cols-3 gap-2">
                                    <button v-for="status in statusOptions" :key="status"
                                        @click="updateStatus(selectedOrder.orderId, status)"
                                        :class="['flex items-center justify-center gap-2 py-3 rounded-xl text-[10px] font-bold uppercase transition-all', selectedOrder.orderStatus === status ? 'bg-neutral-900 text-white' : 'bg-white border border-neutral-200 text-neutral-500 hover:border-orange-500 hover:text-orange-500']">
                                        <component :is="getStatusIcon(status)" class="w-3.5 h-3.5" />{{ status }}
                                    </button>
                                </div>
                            </div>
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
</style>
