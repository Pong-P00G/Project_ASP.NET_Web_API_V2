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
    const map = { DELIVERED: 'bg-emerald-500/20 text-emerald-400 border border-emerald-500/30', SHIPPED: 'bg-blue-500/20 text-blue-400 border border-blue-500/30', PROCESSING: 'bg-orange-500/20 text-orange-400 border border-orange-500/30', CANCELLED: 'bg-red-500/20 text-red-400 border border-red-500/30' }
    return map[status] || 'bg-white/10 text-neutral-400 border border-white/20'
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
                    class="inline-flex items-center gap-2 px-4 py-2 bg-linear-to-r from-purple-500/20 to-blue-500/20 backdrop-blur-sm border border-purple-500/20 rounded-full text-purple-400 text-xs font-semibold mb-3">
                    <ShoppingBag class="w-3.5 h-3.5" /><span>Order Management</span>
                </div>
                <h1 class="text-3xl font-black text-white tracking-tight">Orders</h1>
                <p class="text-neutral-400 text-sm mt-1">Track and process customer orders</p>
            </div>
            <button @click="fetchOrders" :disabled="loading"
                class="flex items-center gap-2 px-5 py-3 bg-white/5 backdrop-blur-sm border border-white/10 rounded-xl text-sm font-semibold text-white hover:bg-white/10 transition-all">
                <RefreshCw class="w-4 h-4" :class="{ 'animate-spin': loading }" />Sync
            </button>
        </div>

        <!-- Stats -->
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5 mb-8">
            <div
                class="stat-card group relative bg-white/5 backdrop-blur-xl rounded-2xl p-6 border border-white/10 hover:border-purple-500/50 hover:-translate-y-2 hover:shadow-2xl hover:shadow-purple-500/10 transition-all duration-500">
                <div
                    class="absolute inset-0 bg-linear-to-br from-purple-500/10 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-500 rounded-2xl">
                </div>
                <div class="relative z-10 flex items-center gap-4">
                    <div
                        class="w-14 h-14 flex items-center justify-center bg-linear-to-br from-purple-400 to-purple-600 rounded-2xl text-white shadow-lg shadow-purple-500/30 group-hover:scale-110 transition-transform duration-300">
                        <ShoppingBag class="w-6 h-6" />
                    </div>
                    <div>
                        <p class="text-xs font-semibold text-neutral-400 uppercase tracking-widest">Total Orders</p>
                        <h3 class="text-3xl font-black text-white tracking-tight">{{ stats.total }}</h3>
                    </div>
                </div>
            </div>
            <div
                class="stat-card group relative bg-white/5 backdrop-blur-xl rounded-2xl p-6 border border-white/10 hover:border-orange-500/50 hover:-translate-y-2 hover:shadow-2xl hover:shadow-orange-500/10 transition-all duration-500">
                <div
                    class="absolute inset-0 bg-linear-to-br from-orange-500/10 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-500 rounded-2xl">
                </div>
                <div class="relative z-10 flex items-center gap-4">
                    <div
                        class="w-14 h-14 flex items-center justify-center bg-linear-to-br from-orange-400 to-orange-600 rounded-2xl text-white shadow-lg shadow-orange-500/30 group-hover:scale-110 transition-transform duration-300">
                        <Clock class="w-6 h-6" />
                    </div>
                    <div>
                        <p class="text-xs font-semibold text-neutral-400 uppercase tracking-widest">Pending</p>
                        <h3 class="text-3xl font-black text-white tracking-tight">{{ stats.pending }}</h3>
                    </div>
                </div>
            </div>
            <div
                class="stat-card group relative bg-white/5 backdrop-blur-xl rounded-2xl p-6 border border-white/10 hover:border-blue-500/50 hover:-translate-y-2 hover:shadow-2xl hover:shadow-blue-500/10 transition-all duration-500">
                <div
                    class="absolute inset-0 bg-linear-to-br from-blue-500/10 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-500 rounded-2xl">
                </div>
                <div class="relative z-10 flex items-center gap-4">
                    <div
                        class="w-14 h-14 flex items-center justify-center bg-linear-to-br from-blue-400 to-blue-600 rounded-2xl text-white shadow-lg shadow-blue-500/30 group-hover:scale-110 transition-transform duration-300">
                        <Truck class="w-6 h-6" />
                    </div>
                    <div>
                        <p class="text-xs font-semibold text-neutral-400 uppercase tracking-widest">Processing</p>
                        <h3 class="text-3xl font-black text-white tracking-tight">{{ stats.processing }}</h3>
                    </div>
                </div>
            </div>
            <div
                class="stat-card group relative bg-white/5 backdrop-blur-xl rounded-2xl p-6 border border-white/10 hover:border-emerald-500/50 hover:-translate-y-2 hover:shadow-2xl hover:shadow-emerald-500/10 transition-all duration-500">
                <div
                    class="absolute inset-0 bg-linear-to-br from-emerald-500/10 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-500 rounded-2xl">
                </div>
                <div class="relative z-10 flex items-center gap-4">
                    <div
                        class="w-14 h-14 flex items-center justify-center bg-linear-to-br from-emerald-400 to-emerald-600 rounded-2xl text-white shadow-lg shadow-emerald-500/30 group-hover:scale-110 transition-transform duration-300">
                        <DollarSign class="w-6 h-6" />
                    </div>
                    <div>
                        <p class="text-xs font-semibold text-neutral-400 uppercase tracking-widest">Revenue</p>
                        <h3 class="text-3xl font-black text-white tracking-tight">${{ stats.revenue.toLocaleString() }}
                        </h3>
                    </div>
                </div>
            </div>
        </div>

        <!-- Filters -->
        <div
            class="glass-card bg-white/5 backdrop-blur-xl rounded-2xl p-5 mb-8 border border-white/10 flex flex-col lg:flex-row lg:items-center gap-4">
            <div class="relative flex-1 max-w-sm">
                <Search class="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-neutral-500" />
                <input v-model="searchQuery" type="text" placeholder="Search order #, address..."
                    class="w-full pl-10 pr-4 py-3 bg-white/5 border border-white/10 rounded-xl text-sm text-white placeholder-neutral-500 focus:bg-white/10 focus:border-purple-500/50 outline-none transition-all" />
            </div>
            <div class="flex flex-wrap gap-2">
                <button v-for="status in ['ALL', ...statusOptions]" :key="status" @click="statusFilter = status"
                    :class="['px-4 py-2.5 rounded-xl text-xs font-bold uppercase tracking-wide transition-all', statusFilter === status ? 'bg-linear-to-r from-purple-500 to-blue-500 text-white shadow-lg shadow-purple-500/25' : 'bg-white/5 border border-white/10 text-neutral-400 hover:bg-white/10 hover:text-white']">{{
                        status }}</button>
            </div>
        </div>

        <!-- Table -->
        <div class="glass-card bg-white/5 backdrop-blur-xl rounded-3xl border border-white/10 overflow-hidden">
            <div v-if="loading" class="flex flex-col items-center justify-center py-16">
                <Loader2 class="w-8 h-8 animate-spin text-purple-500" />
                <p class="mt-4 text-xs font-bold text-neutral-400 uppercase tracking-widest">Loading Orders...</p>
            </div>
            <div v-else-if="filteredOrders.length === 0"
                class="flex flex-col items-center justify-center py-16 text-center">
                <div
                    class="w-20 h-20 flex items-center justify-center bg-white/5 rounded-2xl text-neutral-500 mb-6 border border-white/10">
                    <Package class="w-10 h-10" />
                </div>
                <h3 class="text-xl font-bold text-white mb-2">No Orders Found</h3>
                <p class="text-neutral-400 text-sm">Try adjusting your search or filters</p>
            </div>
            <div v-else class="overflow-x-auto">
                <table class="w-full">
                    <thead>
                        <tr class="bg-white/5">
                            <th
                                class="px-6 py-4 text-left text-xs font-semibold text-neutral-400 uppercase tracking-wider">
                                Order</th>
                            <th
                                class="px-6 py-4 text-left text-xs font-semibold text-neutral-400 uppercase tracking-wider">
                                Customer</th>
                            <th
                                class="px-6 py-4 text-left text-xs font-semibold text-neutral-400 uppercase tracking-wider">
                                Date</th>
                            <th
                                class="px-6 py-4 text-left text-xs font-semibold text-neutral-400 uppercase tracking-wider">
                                Total</th>
                            <th
                                class="px-6 py-4 text-left text-xs font-semibold text-neutral-400 uppercase tracking-wider">
                                Status</th>
                            <th
                                class="px-6 py-4 text-right text-xs font-semibold text-neutral-400 uppercase tracking-wider">
                                Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="order in filteredOrders" :key="order.orderId"
                            class="border-b border-white/5 hover:bg-white/5 transition-colors group">
                            <td class="px-6 py-4">
                                <span
                                    class="font-mono text-sm font-bold text-white bg-white/10 px-3 py-1.5 rounded-lg">#{{
                                    order.orderNumber }}</span>
                            </td>
                            <td class="px-6 py-4">
                                <div class="flex items-center gap-3">
                                    <div
                                        class="w-8 h-8 flex items-center justify-center bg-linear-to-br from-purple-500 to-blue-500 text-white font-bold text-xs rounded-lg shadow-lg shadow-purple-500/20 group-hover:scale-110 transition-transform duration-300">
                                        {{ order.shippingAddress.slice(0, 1) }}</div>
                                    <span
                                        class="text-sm text-neutral-300 max-w-[150px] truncate group-hover:text-white transition-colors">{{
                                        order.shippingAddress }}</span>
                                </div>
                            </td>
                            <td class="px-6 py-4 text-sm text-neutral-400">{{ new
                                Date(order.createdAt).toLocaleDateString() }}</td>
                            <td class="px-6 py-4 text-sm font-bold text-white">${{ order.totalAmount.toFixed(2) }}</td>
                            <td class="px-6 py-4">
                                <span
                                    :class="['inline-flex items-center gap-1.5 px-3 py-1.5 rounded-full text-xs font-bold uppercase', getStatusClass(order.orderStatus)]">
                                    <component :is="getStatusIcon(order.orderStatus)" class="w-3 h-3" />{{
                                    order.orderStatus }}
                                </span>
                            </td>
                            <td class="px-6 py-4">
                                <div class="flex justify-end opacity-0 group-hover:opacity-100 transition-opacity">
                                    <button @click="viewDetails(order)"
                                        class="p-2 rounded-lg text-neutral-400 hover:bg-purple-500/20 hover:text-purple-400 transition-all">
                                        <Eye class="w-4 h-4" />
                                    </button>
                                </div>
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
                    class="fixed inset-0 bg-black/70 backdrop-blur-md flex items-center justify-center p-6 z-50"
                    @click.self="closeModal">
                    <div
                        class="bg-neutral-900 border border-white/10 rounded-3xl w-full max-w-2xl max-h-[90vh] overflow-hidden shadow-2xl flex flex-col">
                        <div
                            class="flex items-center gap-4 p-6 bg-linear-to-r from-purple-500/20 to-blue-500/20 border-b border-white/10">
                            <div
                                class="w-11 h-11 flex items-center justify-center bg-linear-to-br from-purple-500 to-blue-500 rounded-xl text-white shadow-lg shadow-purple-500/25">
                                <ShoppingBag class="w-5 h-5" />
                            </div>
                            <div class="flex-1">
                                <h3 class="text-lg font-bold text-white">Order #{{ selectedOrder.orderNumber }}</h3>
                                <p class="flex items-center gap-1.5 text-xs text-neutral-400">
                                    <Calendar class="w-3 h-3" />{{ new Date(selectedOrder.createdAt).toLocaleString() }}
                                </p>
                            </div>
                            <span
                                :class="['px-3 py-1.5 rounded-full text-xs font-bold uppercase', getStatusClass(selectedOrder.orderStatus)]">{{
                                selectedOrder.orderStatus }}</span>
                            <button @click="closeModal"
                                class="p-2 rounded-lg text-neutral-400 hover:bg-white/10 hover:text-white transition-all">
                                <X class="w-5 h-5" />
                            </button>
                        </div>
                        <div class="flex-1 overflow-y-auto p-6 space-y-6">
                            <!-- Info -->
                            <div class="bg-white/5 rounded-xl p-5 border border-white/10">
                                <h4 class="text-xs font-bold text-neutral-400 uppercase tracking-widest mb-4">Delivery
                                    Information</h4>
                                <div class="grid sm:grid-cols-2 gap-4">
                                    <div class="flex items-start gap-3">
                                        <div
                                            class="w-9 h-9 flex items-center justify-center bg-linear-to-br from-purple-500 to-purple-600 rounded-lg text-white shadow-lg shadow-purple-500/20">
                                            <User class="w-4 h-4" />
                                        </div>
                                        <div>
                                            <span class="text-xs font-semibold text-neutral-500">Customer</span>
                                            <p class="text-sm font-semibold text-white">User #{{ selectedOrder.userId }}
                                            </p>
                                        </div>
                                    </div>
                                    <div class="flex items-start gap-3">
                                        <div
                                            class="w-9 h-9 flex items-center justify-center bg-linear-to-br from-blue-500 to-blue-600 rounded-lg text-white shadow-lg shadow-blue-500/20">
                                            <MapPin class="w-4 h-4" />
                                        </div>
                                        <div>
                                            <span class="text-xs font-semibold text-neutral-500">Address</span>
                                            <p class="text-sm font-semibold text-white">{{ selectedOrder.shippingAddress
                                                }}</p>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <!-- Items -->
                            <div>
                                <h4 class="text-xs font-bold text-neutral-400 uppercase tracking-widest mb-4">Order
                                    Items</h4>
                                <div class="space-y-3">
                                    <div v-for="item in selectedOrder.items" :key="item.orderItemId"
                                        class="flex items-center justify-between p-4 bg-white/5 rounded-xl border border-white/10 hover:bg-white/10 transition-colors">
                                        <div class="flex items-center gap-3">
                                            <div
                                                class="w-11 h-11 flex items-center justify-center bg-white/10 rounded-lg overflow-hidden">
                                                <img v-if="item.productImage" :src="item.productImage"
                                                    class="w-full h-full object-cover" />
                                                <Package v-else class="w-5 h-5 text-neutral-500" />
                                            </div>
                                            <div>
                                                <p class="text-sm font-semibold text-white">{{ item.productName }}</p>
                                                <p class="text-xs text-neutral-500">Qty: {{ item.quantity }} × ${{
                                                    item.unitPrice.toFixed(2) }}</p>
                                            </div>
                                        </div>
                                        <p class="text-sm font-bold text-white">${{ item.totalPrice.toFixed(2) }}</p>
                                    </div>
                                </div>
                                <div class="mt-4 pt-4 border-t border-dashed border-white/10 space-y-2">
                                    <div class="flex justify-between text-sm text-neutral-400"><span>Tax</span><span>${{
                                            selectedOrder.tax.toFixed(2) }}</span></div>
                                    <div class="flex justify-between text-sm text-neutral-400">
                                        <span>Shipping</span><span>${{ selectedOrder.shippingCost.toFixed(2) }}</span>
                                    </div>
                                    <div
                                        class="flex justify-between items-center p-4 mt-2 bg-linear-to-r from-purple-500 to-blue-500 rounded-xl text-white font-bold">
                                        <span>Total</span><span class="text-lg">${{ selectedOrder.totalAmount.toFixed(2)
                                            }}</span>
                                    </div>
                                </div>
                            </div>
                            <!-- Status Update -->
                            <div>
                                <h4 class="text-xs font-bold text-neutral-400 uppercase tracking-widest mb-4">Update
                                    Status</h4>
                                <div class="grid grid-cols-2 sm:grid-cols-3 gap-2">
                                    <button v-for="status in statusOptions" :key="status"
                                        @click="updateStatus(selectedOrder.orderId, status)"
                                        :class="['flex items-center justify-center gap-2 py-3 rounded-xl text-xs font-bold uppercase transition-all', selectedOrder.orderStatus === status ? 'bg-linear-to-r from-purple-500 to-blue-500 text-white shadow-lg shadow-purple-500/25' : 'bg-white/5 border border-white/10 text-neutral-400 hover:bg-white/10 hover:text-white']">
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

.glass-card {
    box-shadow: 0 0 0 1px rgba(255, 255, 255, 0.05), 0 25px 50px -12px rgba(0, 0, 0, 0.25);
}

.stat-card {
    box-shadow: 0 0 0 1px rgba(255, 255, 255, 0.05), 0 10px 40px -10px rgba(0, 0, 0, 0.3);
}
</style>
