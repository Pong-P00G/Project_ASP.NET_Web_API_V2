<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { orderApi } from '../api/orderApi'
import { 
    CheckCircle, 
    Download, 
    ShoppingBag, 
    ArrowRight,
    Printer,
    Package,
    Calendar,
    MapPin,
    CreditCard
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
        order.value = response.data
    } catch (err) {
        console.error('Error fetching order:', err)
        error.value = 'Failed to load order details'
    } finally {
        loading.value = false
    }
}

const downloadReceipt = () => {
    window.print()
}

onMounted(() => {
    fetchOrder()
})
</script>

<template>
    <div class="min-h-screen bg-gray-50 py-12 print:bg-white print:py-0">
        <div class="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8">
            <div v-if="loading" class="flex flex-col items-center justify-center py-24">
                <div class="w-12 h-12 border-4 border-gray-200 border-t-blue-600 rounded-full animate-spin"></div>
                <p class="mt-4 text-gray-500 font-medium">Loading your order...</p>
            </div>

            <div v-else-if="error" class="text-center py-24">
                <div class="inline-flex items-center justify-center w-16 h-16 bg-red-50 rounded-full mb-6">
                    <span class="text-2xl">⚠️</span>
                </div>
                <h2 class="text-2xl font-bold text-gray-900 mb-2">Something went wrong</h2>
                <p class="text-gray-500 mb-8">{{ error }}</p>
                <button 
                    @click="router.push('/')"
                    class="px-6 py-3 bg-gray-900 text-white rounded-xl font-medium hover:bg-gray-800 transition-colors"
                >
                    Return Home
                </button>
            </div>

            <div v-else-if="order" class="space-y-6">
                <!-- Success Header (Hidden when printing) -->
                <div class="text-center mb-8 print:hidden">
                    <div class="inline-flex items-center justify-center w-20 h-20 bg-green-50 rounded-full mb-6 animate-bounce-slow">
                        <CheckCircle class="w-10 h-10 text-green-600" />
                    </div>
                    <h1 class="text-3xl font-bold text-gray-900 mb-2">Order Confirmed!</h1>
                    <p class="text-gray-500 text-lg">Thank you for your purchase. Your order has been received.</p>
                </div>

                <!-- Receipt Card -->
                <div id="printable-receipt" class="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
                    <!-- Receipt Header -->
                    <div class="bg-gray-900 text-white p-8 print:bg-gray-900 print:text-white">
                        <div class="flex justify-between items-start">
                            <div>
                                <h2 class="text-2xl font-bold mb-1">AlieeShop</h2>
                                <p class="text-white/70 print:text-white/70 text-sm">Receipt</p>
                            </div>
                            <div class="text-right">
                                <p class="text-white/70 print:text-white/70 text-sm mb-1">Order #</p>
                                <p class="text-xl font-mono font-bold">{{ order.orderNumber }}</p>
                            </div>
                        </div>
                    </div>

                    <div class="p-8 space-y-8">
                        <!-- Order Meta -->
                        <div class="grid grid-cols-2 gap-8 pb-8 border-b border-gray-100">
                            <div class="space-y-1">
                                <p class="text-sm text-gray-500 flex items-center gap-2">
                                    <Calendar class="w-4 h-4" /> Date
                                </p>
                                <p class="font-medium text-gray-900">
                                    {{ new Date(order.createdAt).toLocaleDateString() }}
                                </p>
                            </div>
                            <div class="space-y-1">
                                <p class="text-sm text-gray-500 flex items-center gap-2">
                                    <CreditCard class="w-4 h-4" /> Payment Method
                                </p>
                                <p class="font-medium text-gray-900 capitalize">
                                    {{ order.paymentMethod || 'Card' }}
                                </p>
                            </div>
                            <div class="col-span-2 space-y-1">
                                <p class="text-sm text-gray-500 flex items-center gap-2">
                                    <MapPin class="w-4 h-4" /> Shipping Address
                                </p>
                                <p class="font-medium text-gray-900 max-w-md">
                                    {{ order.shippingAddress }}
                                </p>
                            </div>
                        </div>

                        <!-- Order Items -->
                        <div>
                            <h3 class="font-bold text-gray-900 mb-4 flex items-center gap-2">
                                <Package class="w-5 h-5 text-gray-400" />
                                Order Items
                            </h3>
                            <div class="space-y-4">
                                <div v-for="item in order.items" :key="item.orderItemId" 
                                    class="flex items-center justify-between py-2 border-b border-gray-50 last:border-0"
                                >
                                    <div class="flex items-center gap-4">
                                        <div class="w-8 h-8 rounded-lg bg-gray-100 flex items-center justify-center text-sm font-bold text-gray-600 print:bg-gray-100 print:text-gray-600">
                                            {{ item.quantity }}x
                                        </div>
                                        <div>
                                            <p class="font-medium text-gray-900">{{ item.productName }}</p>
                                            <p class="text-sm text-gray-500">${{ item.unitPrice.toFixed(2) }}</p>
                                        </div>
                                    </div>
                                    <p class="font-bold text-gray-900">${{ item.totalPrice.toFixed(2) }}</p>
                                </div>
                            </div>
                        </div>

                        <!-- Totals -->
                        <div class="bg-gray-50 rounded-xl p-6 space-y-3 print:bg-gray-50">
                            <div class="flex justify-between text-gray-600">
                                <span>Subtotal</span>
                                <span>${{ order.subtotal.toFixed(2) }}</span>
                            </div>
                            <div class="flex justify-between text-gray-600">
                                <span>Shipping</span>
                                <span>${{ order.shippingCost.toFixed(2) }}</span>
                            </div>
                            <div class="flex justify-between text-gray-600">
                                <span>Tax</span>
                                <span>${{ order.tax.toFixed(2) }}</span>
                            </div>
                            <div class="flex justify-between text-xl font-bold text-gray-900 pt-3 border-t border-gray-200">
                                <span>Total</span>
                                <span>${{ order.totalAmount.toFixed(2) }}</span>
                            </div>
                        </div>
                    </div>

                    <!-- Footer -->
                    <div class="bg-gray-50 p-6 text-center text-gray-500 text-sm print:hidden">
                        Need help? Contact us at support@alieeshop.com
                    </div>
                </div>

                <!-- Actions (Hidden when printing) -->
                <div class="flex flex-col sm:flex-row gap-4 justify-center pt-4 print:hidden">
                    <button 
                        @click="downloadReceipt"
                        class="px-8 py-3 bg-white border border-gray-200 text-gray-900 font-bold rounded-xl hover:bg-gray-50 hover:border-gray-300 transition-all flex items-center justify-center gap-2 shadow-sm"
                    >
                        <Printer class="w-5 h-5" />
                        Download PDF / Print
                    </button>
                    <button 
                        @click="router.push('/product')"
                        class="px-8 py-3 bg-gray-900 text-white font-bold rounded-xl hover:bg-gray-800 transition-all flex items-center justify-center gap-2 shadow-lg shadow-gray-900/20"
                    >
                        <ShoppingBag class="w-5 h-5" />
                        Continue Shopping
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>

<style>
@media print {
    /* Hide everything on the page */
    body {
        visibility: hidden;
        background: white;
    }
    
    /* Ensure print colors are accurate */
    * {
        -webkit-print-color-adjust: exact !important;
        print-color-adjust: exact !important;
    }

    /* Target the specific receipt card */
    #printable-receipt {
        visibility: visible;
        position: absolute; /* Fixed repeats on every page, Absolute does not */
        left: 0;
        top: 0;
        width: 100%;
        margin: 0;
        padding: 20px;
        box-shadow: none !important;
        border: none !important;
        border-radius: 0 !important;
        background: white !important;
        z-index: 9999;
    }

    /* Make all children of the receipt visible */
    #printable-receipt * {
        visibility: visible;
    }

    /* Explicitly hide elements marked as print:hidden */
    .print\:hidden, .min-h-screen > *:not(#printable-receipt) {
        display: none !important;
    }
    
    /* Collapse the main container height/margins */
    .min-h-screen {
        min-height: 0 !important;
        padding: 0 !important;
        margin: 0 !important;
    }
}
</style>
