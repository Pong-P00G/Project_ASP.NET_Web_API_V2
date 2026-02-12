<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { orderApi } from '../../api/orderApi'
import {
    CheckCircle,
    Download,
    ShoppingBag,
    ArrowRight,
    Printer,
    Package,
    Calendar,
    MapPin,
    CreditCard,
    Loader2,
    Truck
} from 'lucide-vue-next'

const route = useRoute()
const router = useRouter()
const order = ref(null)
const loading = ref(true)
const error = ref(null)

const fetchOrder = async () => {
    try {
        const orderId = route.params.id
        if (!orderId) throw new Error('Order ID validation failed')

        loading.value = true
        const response = await orderApi.getOrderById(orderId)
        order.value = response.data.data || response.data
    } catch (err) {
        console.error('Error fetching order:', err)
        error.value = 'Failed to load order details'
    } finally {
        loading.value = false
    }
}

const printReceipt = () => {
    window.print()
}

const downloadReceipt = () => {
    if (!order.value) return

    const o = order.value
    const date = new Date(o.createdAt).toLocaleDateString('en-US', {
        year: 'numeric', month: 'long', day: 'numeric'
    })

    let itemsText = ''
    if (o.items && o.items.length > 0) {
        o.items.forEach(item => {
            itemsText += `${item.quantity}x  ${item.productName.padEnd(35)}  $${item.totalPrice.toFixed(2)}\n`
        })
    }

    const receipt = `
═══════════════════════════════════════════════
                   ALIEE SHOP
               ORDER RECEIPT
═══════════════════════════════════════════════

Order #:     ${o.orderNumber}
Date:        ${date}
Status:      ${o.orderStatus || 'PAID'}
Payment:     ${o.paymentMethod || 'Card'}
Phone:       ${o.phone || 'N/A'}

───────────────────────────────────────────────
SHIPPING ADDRESS
───────────────────────────────────────────────
${o.shippingAddress || 'N/A'}

───────────────────────────────────────────────
ITEMS
───────────────────────────────────────────────
${itemsText}
───────────────────────────────────────────────
Subtotal:                          $${o.subtotal?.toFixed(2) || '0.00'}
Shipping:                          $${o.shippingCost?.toFixed(2) || '0.00'}
Tax:                               $${o.tax?.toFixed(2) || '0.00'}
───────────────────────────────────────────────
TOTAL:                             $${o.totalAmount?.toFixed(2) || '0.00'}
═══════════════════════════════════════════════

            Thank you for your order!
              www.alieeshop.com

`.trim()

    const blob = new Blob([receipt], { type: 'text/plain;charset=utf-8' })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `Receipt_${o.orderNumber || o.orderId}.txt`
    document.body.appendChild(a)
    a.click()
    document.body.removeChild(a)
    URL.revokeObjectURL(url)
}

const formatDate = (dateStr) => {
    if (!dateStr) return ''
    return new Date(dateStr).toLocaleDateString('en-US', {
        month: 'short', day: 'numeric', year: 'numeric'
    })
}

onMounted(() => {
    fetchOrder()
})
</script>

<template>
    <div class="min-h-screen bg-[#F5F5F5] py-12 px-6 flex items-start justify-center print:bg-white print:p-0">
        <!-- Loading -->
        <div v-if="loading" class="flex flex-col items-center py-24 text-[#999] gap-4">
            <Loader2 :size="36" class="animate-spin" />
            <p class="text-[14px] font-medium">Retrieving order details...</p>
        </div>

        <!-- Error -->
        <div v-else-if="error" class="text-center py-16 px-8 bg-white rounded-[20px] max-w-[500px] w-full">
            <div class="text-[2.5rem] mb-4">⚠️</div>
            <h2 class="text-[22px] font-bold mb-2 text-[#0A0A0A]">Order Not Found</h2>
            <p class="text-[14px] text-[#666] mb-8">{{ error }}</p>
            <button @click="router.push('/')"
                class="inline-flex items-center gap-2 px-5 py-3 border-none rounded-[10px] text-[13px] font-semibold cursor-pointer transition-all duration-300 whitespace-nowrap bg-[#0A0A0A] text-white hover:bg-[#1A1A1A] hover:-translate-y-px">
                Return to Home
            </button>
        </div>

        <!-- Receipt -->
        <div v-else-if="order" class="w-full max-w-[640px] animate-[slideUp_0.5s_cubic-bezier(0.16,1,0.3,1)]">
            <!-- Success Header -->
            <div class="text-center mb-8 print:hidden">
                <div
                    class="inline-flex items-center justify-center w-[72px] h-[72px] rounded-full bg-[#0A0A0A] text-[#C8A97E] mb-5 animate-[bounceIn_0.6s_ease_0.3s_both]">
                    <CheckCircle :size="40" />
                </div>
                <h1 class="text-[28px] font-extrabold text-[#0A0A0A] tracking-tight mb-2">Order Confirmed!</h1>
                <p class="text-[15px] text-[#666]">
                    Your order <span class="font-mono font-bold text-[#C8A97E]">#{{ order.orderNumber }}</span> has been
                    placed
                    successfully.
                </p>
            </div>

            <!-- Receipt Card -->
            <div id="printable-area"
                class="bg-white border border-[#E8E8E8] rounded-2xl overflow-hidden mb-6 print:border-none print:shadow-none print:rounded-none">
                <!-- Receipt Header -->
                <div class="flex justify-between items-start p-7 pl-8 border-b border-[#F0F0F0] sm:p-5">
                    <div>
                        <h3 class="text-[16px] font-extrabold text-[#0A0A0A] tracking-widest">ALIEE SHOP</h3>
                        <p class="text-[12px] text-[#999] mt-0.5">Order Receipt</p>
                    </div>
                    <div>
                        <span
                            class="inline-block px-3 py-1 rounded-full text-[10px] font-bold tracking-widest uppercase bg-[#C8A97E]/12 text-[#C8A97E]">
                            {{ order.orderStatus || 'PAID' }}
                        </span>
                    </div>
                </div>

                <!-- Info Grid -->
                <div class="grid grid-cols-2 gap-5 p-7 pl-8 border-b border-[#F0F0F0] sm:grid-cols-1 sm:p-5">
                    <div>
                        <p
                            class="flex items-center gap-1.5 text-[10px] font-bold uppercase tracking-widest text-[#999] mb-1">
                            <Calendar :size="12" /> Order Date
                        </p>
                        <p class="text-[13px] font-semibold text-[#0A0A0A] break-words">{{ formatDate(order.createdAt)
                            }}</p>
                    </div>
                    <div>
                        <p
                            class="flex items-center gap-1.5 text-[10px] font-bold uppercase tracking-widest text-[#999] mb-1">
                            <Package :size="12" /> Order Number
                        </p>
                        <p class="text-[13px] font-semibold text-[#C8A97E] font-mono break-words">{{ order.orderNumber
                            }}</p>
                    </div>
                    <div>
                        <p
                            class="flex items-center gap-1.5 text-[10px] font-bold uppercase tracking-widest text-[#999] mb-1">
                            <CreditCard :size="12" /> Payment
                        </p>
                        <p class="text-[13px] font-semibold text-[#0A0A0A] capitalize">{{ order.paymentMethod }}</p>
                    </div>
                    <div>
                        <p
                            class="flex items-center gap-1.5 text-[10px] font-bold uppercase tracking-widest text-[#999] mb-1">
                            <MapPin :size="12" /> Ship To
                        </p>
                        <p class="text-[13px] font-semibold text-[#0A0A0A] break-words">{{ order.shippingAddress }}</p>
                    </div>
                </div>

                <!-- Items -->
                <div class="p-7 pl-8 border-b border-[#F0F0F0] sm:p-5">
                    <h4 class="text-[10px] font-bold uppercase tracking-widest text-[#999] mb-4">Items Ordered</h4>
                    <div v-for="item in order.items" :key="item.orderItemId"
                        class="flex items-center justify-between py-3 border-t border-[#FAFAFA] first:border-0">
                        <div class="flex items-center gap-3 flex-1 min-w-0">
                            <div
                                class="w-10 h-10 rounded-lg overflow-hidden bg-[#F5F5F5] flex items-center justify-center text-[#999] shrink-0">
                                <img v-if="item.productImage" :src="item.productImage" :alt="item.productName"
                                    class="w-full h-full object-cover" />
                                <Package v-else :size="16" />
                            </div>
                            <div>
                                <p
                                    class="text-[13px] font-semibold text-[#0A0A0A] whitespace-nowrap overflow-hidden text-ellipsis">
                                    {{ item.productName }}</p>
                                <p class="text-[11px] text-[#999] mt-0.5">Qty: {{ item.quantity }} × ${{
                                    item.unitPrice?.toFixed(2) }}</p>
                            </div>
                        </div>
                        <p class="text-[14px] font-bold text-[#0A0A0A] shrink-0 ml-4">${{ item.totalPrice?.toFixed(2) }}
                        </p>
                    </div>
                </div>

                <!-- Totals -->
                <div class="p-7 pl-8 bg-[#FAFAFA] sm:p-5">
                    <div class="flex justify-between text-[13px] text-[#666] py-1.5">
                        <span>Subtotal</span>
                        <span>${{ order.subtotal?.toFixed(2) }}</span>
                    </div>
                    <div class="flex justify-between text-[13px] text-[#666] py-1.5">
                        <span>Shipping</span>
                        <span>${{ order.shippingCost?.toFixed(2) }}</span>
                    </div>
                    <div class="flex justify-between text-[13px] text-[#666] py-1.5">
                        <span>Tax</span>
                        <span>${{ order.tax?.toFixed(2) }}</span>
                    </div>
                    <div
                        class="flex justify-between text-[18px] font-extrabold text-[#0A0A0A] border-t border-[#E8E8E8] mt-3 pt-4">
                        <span>Total</span>
                        <span>${{ order.totalAmount?.toFixed(2) }}</span>
                    </div>
                </div>
            </div>

            <!-- Actions -->
            <div class="flex flex-wrap gap-3 justify-center print:hidden sm:flex-col">
                <button @click="downloadReceipt"
                    class="inline-flex items-center justify-center gap-2 px-5 py-3 bg-white border border-[#DDD] text-[#0A0A0A] rounded-[10px] text-[13px] font-semibold cursor-pointer transition-all duration-300 hover:border-[#C8A97E] hover:text-[#C8A97E] whitespace-nowrap">
                    <Download :size="16" /> Download Receipt
                </button>
                <button @click="printReceipt"
                    class="inline-flex items-center justify-center gap-2 px-5 py-3 bg-white border border-[#DDD] text-[#0A0A0A] rounded-[10px] text-[13px] font-semibold cursor-pointer transition-all duration-300 hover:border-[#C8A97E] hover:text-[#C8A97E] whitespace-nowrap">
                    <Printer :size="16" /> Print
                </button>
                <router-link :to="`/track-order?orderId=${order.orderId}`"
                    class="inline-flex items-center justify-center gap-2 px-5 py-3 bg-white border border-[#DDD] text-[#0A0A0A] rounded-[10px] text-[13px] font-semibold cursor-pointer transition-all duration-300 hover:border-[#C8A97E] hover:text-[#C8A97E] whitespace-nowrap">
                    <Truck :size="16" /> Track Order
                </router-link>
                <button @click="router.push('/product')"
                    class="inline-flex items-center justify-center gap-2 px-5 py-3 bg-[#0A0A0A] text-white rounded-[10px] text-[13px] font-semibold cursor-pointer transition-all duration-300 hover:bg-[#1A1A1A] hover:-translate-y-px whitespace-nowrap">
                    Continue Shopping
                    <ArrowRight :size="16" />
                </button>
            </div>
        </div>
    </div>
</template>

<style scoped>
@keyframes slideUp {
    from {
        opacity: 0;
        transform: translateY(20px);
    }

    to {
        opacity: 1;
        transform: translateY(0);
    }
}

@keyframes bounceIn {
    0% {
        transform: scale(0);
    }

    60% {
        transform: scale(1.15);
    }

    100% {
        transform: scale(1);
    }
}
</style>
