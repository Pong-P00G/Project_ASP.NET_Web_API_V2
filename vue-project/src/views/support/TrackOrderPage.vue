<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { orderApi } from '../../api/orderApi'
import {
    Search, Package, CheckCircle, Truck, MapPin, XCircle,
    ChevronRight, Loader2, Clock, ArrowRight, ShoppingBag, CreditCard, Phone
} from 'lucide-vue-next'

const route = useRoute()
const router = useRouter()

const orderInput = ref('')
const isSearching = ref(false)
const order = ref(null)
const error = ref(null)
const isLoaded = ref(false)

// Status pipeline
const allStatuses = ['PENDING', 'PROCESSING', 'SHIPPED', 'DELIVERED']

const statusConfig = {
    PENDING: { label: 'Order Placed', icon: Package, desc: 'Your order has been confirmed' },
    PROCESSING: { label: 'Processing', icon: Clock, desc: 'We are preparing your order' },
    SHIPPED: { label: 'Shipped', icon: Truck, desc: 'Your order is on the way' },
    DELIVERED: { label: 'Delivered', icon: MapPin, desc: 'Package delivered successfully' },
    CANCELLED: { label: 'Cancelled', icon: XCircle, desc: 'This order was cancelled' }
}

const isCancelled = computed(() => order.value?.orderStatus === 'CANCELLED')

const currentStatusIndex = computed(() => {
    if (!order.value) return -1
    return allStatuses.indexOf(order.value.orderStatus)
})

const isStepCompleted = (index) => {
    if (isCancelled.value) return false
    return index <= currentStatusIndex.value
}

const isStepCurrent = (index) => {
    if (isCancelled.value) return false
    return index === currentStatusIndex.value
}

const getStepDate = (index) => {
    if (!order.value) return ''
    if (index === 0) {
        return formatDate(order.value.createdAt)
    }
    if (isStepCompleted(index) && index === currentStatusIndex.value) {
        return formatDate(order.value.updatedAt || order.value.createdAt)
    }
    if (isStepCompleted(index)) return ''
    return 'Pending'
}

const formatDate = (dateStr) => {
    if (!dateStr) return ''
    const date = new Date(dateStr)
    return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })
        + ' · ' + date.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' })
}

const formatShortDate = (dateStr) => {
    if (!dateStr) return ''
    return new Date(dateStr).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })
}

// Search / fetch
const searchOrder = async () => {
    const input = orderInput.value.trim()
    if (!input) return

    try {
        isSearching.value = true
        error.value = null
        order.value = null

        // Try as order ID (number)
        if (/^\d+$/.test(input)) {
            const res = await orderApi.getOrderById(parseInt(input))
            order.value = res.data.data || res.data
        } else {
            // Try fetching all user orders and find by order number
            const res = await orderApi.getOrders()
            const orders = res.data.data || res.data || []
            const found = orders.find(o => o.orderNumber === input || o.orderNumber?.includes(input))
            if (found) {
                order.value = found
            } else {
                error.value = 'Order not found. Please check the order number.'
            }
        }
    } catch (err) {
        error.value = err.response?.data?.message || 'Order not found. Please check the order number and try again.'
    } finally {
        isSearching.value = false
    }
}

const fetchOrderById = async (id) => {
    try {
        isSearching.value = true
        error.value = null
        const res = await orderApi.getOrderById(parseInt(id))
        order.value = res.data.data || res.data
    } catch (err) {
        error.value = 'Could not load order details.'
    } finally {
        isSearching.value = false
    }
}

onMounted(() => {
    setTimeout(() => { isLoaded.value = true }, 100)
    const orderId = route.query.orderId
    if (orderId) {
        orderInput.value = orderId
        fetchOrderById(orderId)
    }
})
</script>

<template>
    <div class="min-h-screen bg-[#F5F5F5] pb-16">
        <!-- Hero -->
        <section class="relative h-[280px] bg-[#0A0A0A] flex items-center justify-center text-center overflow-hidden">
            <div class="absolute inset-0 bg-[linear-gradient(135deg,rgba(200,169,126,0.08)_0%,transparent_50%)]"></div>
            <div class="relative px-8 transition-all duration-600 ease-[cubic-bezier(0.16,1,0.3,1)] delay-200"
                :class="isLoaded ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'">
                <p class="text-[11px] font-medium tracking-[0.35em] uppercase text-white/40 mb-4">— Order Tracking</p>
                <h1 class="text-[clamp(2rem,5vw,3.5rem)] font-light text-white tracking-tight mb-3">
                    Track Your <em class="italic font-normal">Order</em>
                </h1>
                <p class="text-[15px] text-white/40 font-light">Enter your order number to get real-time shipping
                    updates.</p>
            </div>
        </section>

        <div class="max-w-[1000px] mx-auto -mt-8 px-6 relative z-10">
            <!-- Search Card -->
            <div class="bg-white border border-[#E8E8E8] rounded-2xl p-8 mb-6 transition-all duration-500 ease-[cubic-bezier(0.16,1,0.3,1)] delay-400"
                :class="isLoaded ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-3'">
                <h2 class="text-base font-bold text-[#0A0A0A] mb-5">Find Your Order</h2>
                <form @submit.prevent="searchOrder" class="flex gap-3">
                    <div class="flex-1 relative flex items-center">
                        <Search :size="18" class="absolute left-3.5 text-[#999] pointer-events-none" />
                        <input v-model="orderInput" type="text" placeholder="Enter Order ID or Order Number"
                            class="w-full pl-11 pr-4 py-3 border border-[#E0E0E0] rounded-xl text-sm text-[#0A0A0A] outline-none transition-all duration-200 focus:border-[#C8A97E] focus:shadow-[0_0_0_3px_rgba(200,169,126,0.12)] placeholder:text-[#ccc]" />
                    </div>
                    <button type="submit" :disabled="isSearching || !orderInput.trim()"
                        class="inline-flex items-center justify-center gap-2 px-6 py-3 bg-[#0A0A0A] text-white rounded-xl text-[13px] font-bold tracking-wide transition-all duration-300 hover:bg-[#1A1A1A] disabled:opacity-50 disabled:cursor-not-allowed whitespace-nowrap">
                        <Loader2 v-if="isSearching" :size="16" class="animate-spin" />
                        <Search v-else :size="16" />
                        <span>{{ isSearching ? 'Searching...' : 'Track' }}</span>
                    </button>
                </form>

                <!-- Error -->
                <div v-if="error"
                    class="flex items-center gap-2 mt-4 p-3 rounded-xl bg-[#FFF1F0] text-[#E53935] text-[13px] font-medium">
                    <XCircle :size="16" />
                    <span>{{ error }}</span>
                </div>
            </div>

            <!-- Order Result -->
            <transition name="fade-up">
                <div v-if="order">
                    <!-- Status Banner -->
                    <div class="flex items-center gap-5 p-6 rounded-2xl mb-6 text-white"
                        :class="isCancelled ? 'bg-[linear-gradient(135deg,#5C1A1A,#3A1010)]' : 'bg-[linear-gradient(135deg,#0A0A0A,#222)]'">
                        <div class="w-[52px] h-[52px] rounded-full flex items-center justify-center shrink-0"
                            :class="isCancelled ? 'bg-[#E53935]/20 text-[#EF9A9A]' : 'bg-[#C8A97E]/15 text-[#C8A97E]'">
                            <component :is="statusConfig[order.orderStatus]?.icon || Package" :size="28" />
                        </div>
                        <div>
                            <p class="text-lg font-bold mb-0.5">
                                {{ statusConfig[order.orderStatus]?.label || order.orderStatus }}
                            </p>
                            <p class="text-[13px] text-white/50">
                                {{ statusConfig[order.orderStatus]?.desc || '' }}
                            </p>
                        </div>
                    </div>

                    <div class="grid grid-cols-1 lg:grid-cols-[1fr_340px] gap-6">
                        <!-- LEFT: Timeline + Items -->
                        <div class="flex flex-col gap-5">
                            <!-- Timeline -->
                            <div v-if="!isCancelled" class="bg-white border border-[#E8E8E8] rounded-2xl p-7">
                                <h3
                                    class="flex items-center gap-2 text-[14px] font-bold text-[#0A0A0A] uppercase tracking-widest mb-5">
                                    Shipping Progress</h3>
                                <div class="flex flex-col">
                                    <div v-for="(status, index) in allStatuses" :key="status" class="flex gap-4">
                                        <div class="flex flex-col items-center">
                                            <div class="w-8 h-8 rounded-full flex items-center justify-center border-2 shrink-0 transition-all duration-300"
                                                :class="{
                                                    'bg-[#0A0A0A] border-[#0A0A0A] text-[#C8A97E]': isStepCompleted(index),
                                                    'bg-[#C8A97E] border-[#C8A97E] text-[#0A0A0A] shadow-[0_0_0_4px_rgba(200,169,126,0.2)]': isStepCurrent(index),
                                                    'bg-white border-[#E0E0E0] text-[#ccc]': !isStepCompleted(index) && !isStepCurrent(index)
                                                }">
                                                <component :is="statusConfig[status].icon" :size="14" />
                                            </div>
                                            <div v-if="index < allStatuses.length - 1"
                                                class="w-0.5 h-10 transition-colors duration-300"
                                                :class="isStepCompleted(index) ? 'bg-[#0A0A0A]' : 'bg-[#E0E0E0]'">
                                            </div>
                                        </div>
                                        <div class="pt-1 pb-6 text-left">
                                            <p class="text-[14px] font-semibold mb-0.5"
                                                :class="isStepCompleted(index) ? 'text-[#0A0A0A]' : 'text-[#ccc]'">
                                                {{ statusConfig[status].label }}
                                            </p>
                                            <p class="text-[12px] text-[#999]">{{ getStepDate(index) }}</p>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <!-- Cancelled message -->
                            <div v-else class="bg-white border border-[#FFCDD2] rounded-2xl p-7">
                                <div
                                    class="flex flex-col items-center justify-center py-8 text-center text-[#E53935] gap-2">
                                    <XCircle :size="40" />
                                    <h3 class="text-lg font-bold">Order Cancelled</h3>
                                    <p class="text-[13px] text-[#666]">This order has been cancelled.</p>
                                </div>
                            </div>

                            <!-- Order Items -->
                            <div class="bg-white border border-[#E8E8E8] rounded-2xl p-7">
                                <h3
                                    class="flex items-center gap-2 text-[14px] font-bold text-[#0A0A0A] uppercase tracking-widest mb-5">
                                    <ShoppingBag :size="16" />
                                    Items ({{ order.items?.length || 0 }})
                                </h3>
                                <div class="flex flex-col gap-3">
                                    <div v-for="item in order.items" :key="item.orderItemId"
                                        class="flex items-center gap-4 p-3.5 border border-[#F0F0F0] rounded-xl transition-colors duration-200 hover:border-[#C8A97E]">
                                        <div
                                            class="w-[52px] h-[52px] rounded-lg overflow-hidden bg-[#F5F5F5] flex items-center justify-center text-[#999] shrink-0">
                                            <img v-if="item.productImage" :src="item.productImage"
                                                :alt="item.productName" class="w-full h-full object-cover" />
                                            <Package v-else :size="20" />
                                        </div>
                                        <div class="flex-1 min-w-0">
                                            <p class="text-[14px] font-semibold text-[#0A0A0A] truncate">{{
                                                item.productName }}</p>
                                            <p class="text-[12px] text-[#999] mt-0.5">Qty: {{ item.quantity }}</p>
                                        </div>
                                        <p class="text-[14px] font-bold text-[#0A0A0A] whitespace-nowrap">${{
                                            item.totalPrice?.toFixed(2) }}</p>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <!-- RIGHT: Shipment Details -->
                        <div class="flex flex-col gap-5">
                            <!-- Shipment Info -->
                            <div class="bg-white border border-[#E8E8E8] rounded-2xl p-7">
                                <h3
                                    class="flex items-center gap-2 text-[14px] font-bold text-[#0A0A0A] uppercase tracking-widest mb-5">
                                    Shipment Details</h3>
                                <div class="grid grid-cols-2 gap-4">
                                    <div class="flex flex-col gap-1">
                                        <span class="text-[10px] font-bold uppercase tracking-widest text-[#999]">Order
                                            Number</span>
                                        <span class="text-[13px] font-semibold text-[#0A0A0A] break-all font-mono">{{
                                            order.orderNumber }}</span>
                                    </div>
                                    <div class="flex flex-col gap-1">
                                        <span class="text-[10px] font-bold uppercase tracking-widest text-[#999]">Date
                                            Placed</span>
                                        <span class="text-[13px] font-semibold text-[#0A0A0A]">{{
                                            formatShortDate(order.createdAt) }}</span>
                                    </div>
                                    <div class="flex flex-col gap-1">
                                        <span
                                            class="text-[10px] font-bold uppercase tracking-widest text-[#999]">Payment</span>
                                        <span
                                            class="text-[13px] font-semibold text-[#0A0A0A] capitalize flex items-center gap-1">
                                            <CreditCard :size="13" />
                                            {{ order.paymentMethod }}
                                        </span>
                                    </div>
                                    <div class="flex flex-col gap-1">
                                        <span
                                            class="text-[10px] font-bold uppercase tracking-widest text-[#999]">Phone</span>
                                        <span class="text-[13px] font-semibold text-[#0A0A0A] flex items-center gap-1">
                                            <Phone :size="13" />
                                            {{ order.phone }}
                                        </span>
                                    </div>
                                </div>

                                <div class="h-px bg-[#F0F0F0] my-5"></div>

                                <div class="flex flex-col gap-1">
                                    <span class="text-[10px] font-bold uppercase tracking-widest text-[#999]">Shipping
                                        Address</span>
                                    <span class="text-[13px] font-semibold text-[#0A0A0A]">{{ order.shippingAddress
                                    }}</span>
                                </div>
                            </div>

                            <!-- Order Summary -->
                            <div class="bg-white border border-[#E8E8E8] rounded-2xl p-7">
                                <h3
                                    class="flex items-center gap-2 text-[14px] font-bold text-[#0A0A0A] uppercase tracking-widest mb-5">
                                    Order Summary</h3>
                                <div class="flex flex-col gap-3">
                                    <div class="flex justify-between text-[13px] text-[#666]">
                                        <span>Subtotal</span>
                                        <span class="font-medium text-[#0A0A0A]">${{ order.subtotal?.toFixed(2)
                                        }}</span>
                                    </div>
                                    <div class="flex justify-between text-[13px] text-[#666]">
                                        <span>Shipping</span>
                                        <span class="font-medium text-[#0A0A0A]">${{ order.shippingCost?.toFixed(2)
                                        }}</span>
                                    </div>
                                    <div class="flex justify-between text-[13px] text-[#666]">
                                        <span>Tax</span>
                                        <span class="font-medium text-[#0A0A0A]">${{ order.tax?.toFixed(2) }}</span>
                                    </div>
                                    <div class="h-px bg-[#F0F0F0] my-1"></div>
                                    <div class="flex justify-between text-[15px] font-bold text-[#0A0A0A]">
                                        <span>Total</span>
                                        <span>${{ order.totalAmount?.toFixed(2) }}</span>
                                    </div>
                                </div>
                            </div>

                            <!-- Actions -->
                            <div>
                                <router-link :to="`/order-success/${order.orderId}`"
                                    class="w-full inline-flex items-center justify-center gap-2 px-6 py-3 bg-transparent border border-[#DDD] text-[#0A0A0A] rounded-xl text-[13px] font-bold tracking-wide transition-all duration-300 hover:border-[#C8A97E] hover:text-[#C8A97E]">
                                    View Receipt
                                    <ArrowRight :size="14" />
                                </router-link>
                            </div>
                        </div>
                    </div>
                </div>
            </transition>
        </div>
    </div>
</template>

<style scoped>
/* Vue Transitions */
.fade-up-enter-active,
.fade-up-leave-active {
    transition: all 0.5s cubic-bezier(0.16, 1, 0.3, 1);
}

.fade-up-enter-from,
.fade-up-leave-to {
    opacity: 0;
    transform: translateY(20px);
}
</style>
