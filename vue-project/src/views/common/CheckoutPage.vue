<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { cartApi } from '../../api/cartApi'
import { orderApi } from '../../api/orderApi'
import {
    CheckCircle,
    CreditCard,
    Truck,
    MapPin,
    User,
    Mail,
    Phone,
    ArrowLeft,
    Loader2,
    QrCode,
    Banknote,
    ShoppingBag,
    Plus,
    Minus,
    Trash2,
    ShieldCheck,
    ChevronRight,
    Edit2
} from 'lucide-vue-next'

const router = useRouter()
const loading = ref(false)
const processing = ref(false)
const cartItems = ref([])
const error = ref(null)

const steps = [
    { id: 'shipping', label: 'Shipping' },
    { id: 'payment', label: 'Payment' },
    { id: 'review', label: 'Review' }
]

const currentStep = ref(0)
const stepsList = ['Shipping', 'Payment', 'Review']

// Expanded State for detailed form
const form = ref({
    fullName: '',
    email: '',
    phone: '',
    address: '',
    city: '',
    postalCode: '',
    paymentMethod: 'card'
})

// Helper to map cart items from API response
const mapCartItems = (items) => {
    if (!Array.isArray(items)) return []
    return items.map(item => {
        const product = item.productVariant?.product
        const images = product?.productImages || []
        const firstImage = images.length > 0 ? images[0].imageUrl : null
        return {
            id: item.cartItemId,
            productId: product?.productId || item.productVariant?.productId,
            name: product?.productName || 'Unknown Product',
            image: firstImage,
            price: item.price || item.productVariant?.price || 0,
            quantity: item.quantity || 1,
            variantName: item.productVariant?.product?.name // Simplified
        }
    })
}

const fetchCart = async () => {
    try {
        loading.value = true
        const response = await cartApi.getCart()
        const cartData = response.data.data || response.data || {}
        const items = Array.isArray(cartData) ? cartData : (cartData.items || [])
        cartItems.value = mapCartItems(items)
    } catch (err) {
        error.value = 'Failed to load cart items'
        console.error(err)
    } finally {
        loading.value = false
    }
}

const subtotal = computed(() => {
    return cartItems.value.reduce((sum, item) => sum + ((item.price || 0) * (item.quantity || 1)), 0)
})

const shippingCost = computed(() => subtotal.value > 100 ? 0 : 15)
const tax = computed(() => subtotal.value * 0.08)
const total = computed(() => subtotal.value + shippingCost.value + tax.value)

const nextStep = () => {
    // Validation for Step 0 (Shipping)
    if (currentStep.value === 0) {
        if (!form.value.fullName || !form.value.email || !form.value.phone || !form.value.address || !form.value.city || !form.value.postalCode) {
            alert('Please fill in all shipping details.')
            return
        }
    }

    if (currentStep.value < stepsList.length - 1) {
        currentStep.value++
    }
}

const prevStep = () => {
    if (currentStep.value > 0) {
        currentStep.value--
    }
}

const placeOrder = async () => {
    try {
        processing.value = true

        // Concatenate address for backend compatibility
        const fullLocation = `${form.value.address}, ${form.value.city}, ${form.value.postalCode}. Name: ${form.value.fullName}. Email: ${form.value.email}`

        // Call real Order API
        const response = await orderApi.createOrder({
            paymentMethod: form.value.paymentMethod,
            phone: form.value.phone,
            location: fullLocation
        })

        const order = response.data

        // Clear cart session on frontend
        cartApi.clearSession()

        router.push(`/order-success/${order.orderId}`)
    } catch (err) {
        console.error('Order error:', err)
        if (err.response?.status === 401) {
            alert('Please login to place an order')
            router.push('/login')
        } else {
            alert(err.response?.data?.message || 'Failed to place order. Please try again.')
        }
    } finally {
        processing.value = false
    }
}

onMounted(() => {
    fetchCart()
})
</script>

<template>
    <div class="min-h-screen bg-gray-50/50 pb-20 font-sans text-gray-900">
        <!-- Minimal Header -->
        <header class="bg-white border-b border-gray-200 sticky top-0 z-50">
            <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between">
                <div class="flex items-center gap-2">
                    <div class="bg-blue-600 text-white p-1.5 rounded-lg">
                        <ShoppingBag class="w-5 h-5" />
                    </div>
                    <span class="font-bold text-xl tracking-tight">ShopEase</span>
                </div>

                <h1 class="text-lg font-medium hidden md:block">Checkout</h1>

                <div class="w-8"></div> <!-- Spacer for balance -->
            </div>
        </header>

        <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10">

            <!-- Stepper -->
            <div class="max-w-3xl mx-auto mb-12">
                <div class="flex items-center justify-between relative">
                    <!-- Progress Bar Background -->
                    <div class="absolute left-0 top-5 w-full h-0.5 bg-gray-200 -z-10"></div>
                    <!-- Active Progress Bar -->
                    <div class="absolute left-0 top-5 h-0.5 bg-blue-600 -z-10 transition-all duration-500"
                        :style="{ width: `${(currentStep / (stepsList.length - 1)) * 100}%` }"></div>

                    <div v-for="(step, index) in stepsList" :key="step"
                        class="flex flex-col items-center gap-2 bg-white px-2">
                        <div class="w-10 h-10 rounded-full flex items-center justify-center border-2 transition-all duration-300 z-10"
                            :class="index <= currentStep
                                ? 'border-blue-600 bg-blue-600 text-white'
                                : 'border-gray-200 bg-white text-gray-400'">
                            <CheckCircle v-if="index < currentStep" class="w-6 h-6 fill-blue-600 text-white" />
                            <span v-else class="font-bold text-sm">{{ index + 1 }}</span>
                        </div>
                        <span class="text-xs font-bold uppercase tracking-wider"
                            :class="index <= currentStep ? 'text-blue-600' : 'text-gray-400'">
                            {{ step }}
                        </span>
                    </div>
                </div>
            </div>

            <div v-if="loading" class="flex justify-center py-20">
                <Loader2 class="w-8 h-8 animate-spin text-blue-600" />
            </div>

            <div v-else class="grid lg:grid-cols-12 gap-8 lg:gap-12">

                <!-- Main Form Column -->
                <div class="lg:col-span-7 space-y-8">

                    <!-- STEP 1: SHIPPING INFO -->
                    <div v-if="currentStep === 0"
                        class="bg-white rounded-2xl shadow-sm border border-gray-100 p-6 md:p-8 animate-fade-in-up">
                        <div class="flex items-center gap-3 mb-6 pb-6 border-b border-gray-100">
                            <div class="bg-blue-50 p-2 rounded-lg">
                                <Truck class="w-5 h-5 text-blue-600" />
                            </div>
                            <h2 class="text-xl font-bold text-gray-900">Shipping Information</h2>
                        </div>

                        <div class="space-y-6">
                            <div class="grid md:grid-cols-2 gap-6">
                                <div class="space-y-2">
                                    <label class="text-sm font-semibold text-gray-700">Full Name</label>
                                    <div class="relative">
                                        <User class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                                        <input v-model="form.fullName" type="text" placeholder="John Doe"
                                            class="w-full pl-10 pr-4 py-3 border border-gray-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-all">
                                    </div>
                                </div>
                                <div class="space-y-2">
                                    <label class="text-sm font-semibold text-gray-700">Email Address</label>
                                    <div class="relative">
                                        <Mail class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                                        <input v-model="form.email" type="email" placeholder="john@example.com"
                                            class="w-full pl-10 pr-4 py-3 border border-gray-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-all">
                                    </div>
                                </div>
                            </div>

                            <div class="space-y-2">
                                <label class="text-sm font-semibold text-gray-700">Phone Number</label>
                                <div class="relative">
                                    <Phone class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                                    <input v-model="form.phone" type="tel" placeholder="+1 (555) 000-0000"
                                        class="w-full pl-10 pr-4 py-3 border border-gray-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-all">
                                </div>
                            </div>

                            <div class="space-y-2">
                                <label class="text-sm font-semibold text-gray-700">Street Address</label>
                                <div class="relative">
                                    <MapPin class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                                    <input v-model="form.address" type="text" placeholder="123 Main St, Apt 4B"
                                        class="w-full pl-10 pr-4 py-3 border border-gray-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-all">
                                </div>
                            </div>

                            <div class="grid md:grid-cols-2 gap-6">
                                <div class="space-y-2">
                                    <label class="text-sm font-semibold text-gray-700">City</label>
                                    <input v-model="form.city" type="text" placeholder="New York"
                                        class="w-full px-4 py-3 border border-gray-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-all">
                                </div>
                                <div class="space-y-2">
                                    <label class="text-sm font-semibold text-gray-700">Postal Code</label>
                                    <input v-model="form.postalCode" type="text" placeholder="10001"
                                        class="w-full px-4 py-3 border border-gray-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-all">
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- STEP 2: PAYMENT METHOD -->
                    <div v-if="currentStep === 1"
                        class="bg-white rounded-2xl shadow-sm border border-gray-100 p-6 md:p-8 animate-fade-in-up">
                        <div class="flex items-center gap-3 mb-6 pb-6 border-b border-gray-100">
                            <div class="bg-blue-50 p-2 rounded-lg">
                                <CreditCard class="w-5 h-5 text-blue-600" />
                            </div>
                            <h2 class="text-xl font-bold text-gray-900">Payment Method</h2>
                        </div>

                        <div class="space-y-4">
                            <label
                                class="flex items-center p-4 border-2 rounded-xl cursor-pointer transition-all hover:bg-gray-50"
                                :class="form.paymentMethod === 'card' ? 'border-blue-500 bg-blue-50/50' : 'border-gray-100'">
                                <input type="radio" v-model="form.paymentMethod" value="card"
                                    class="w-5 h-5 text-blue-600 border-gray-300 focus:ring-blue-500">
                                <div class="ml-4 flex-1">
                                    <div class="flex items-center justify-between">
                                        <span class="font-bold text-gray-900">Credit / Debit Card</span>
                                        <div class="flex gap-2">
                                            <!-- Icons mockup -->
                                            <div class="h-6 w-9 bg-gray-200 rounded"></div>
                                            <div class="h-6 w-9 bg-gray-200 rounded"></div>
                                        </div>
                                    </div>
                                    <p class="text-sm text-gray-500 mt-1">Safe money transfer using your bank account.
                                    </p>
                                </div>
                            </label>

                            <label
                                class="flex items-center p-4 border-2 rounded-xl cursor-pointer transition-all hover:bg-gray-50"
                                :class="form.paymentMethod === 'qr' ? 'border-blue-500 bg-blue-50/50' : 'border-gray-100'">
                                <input type="radio" v-model="form.paymentMethod" value="qr"
                                    class="w-5 h-5 text-blue-600 border-gray-300 focus:ring-blue-500">
                                <div class="ml-4 flex-1">
                                    <span class="font-bold text-gray-900">QR Payment</span>
                                    <p class="text-sm text-gray-500 mt-1">Scan QR code with your banking app.</p>
                                </div>
                                <QrCode class="w-6 h-6 text-gray-400" />
                            </label>

                            <label
                                class="flex items-center p-4 border-2 rounded-xl cursor-pointer transition-all hover:bg-gray-50"
                                :class="form.paymentMethod === 'cash' ? 'border-blue-500 bg-blue-50/50' : 'border-gray-100'">
                                <input type="radio" v-model="form.paymentMethod" value="cash"
                                    class="w-5 h-5 text-blue-600 border-gray-300 focus:ring-blue-500">
                                <div class="ml-4 flex-1">
                                    <span class="font-bold text-gray-900">Cash on Delivery</span>
                                    <p class="text-sm text-gray-500 mt-1">Pay when you receive the package.</p>
                                </div>
                                <Banknote class="w-6 h-6 text-gray-400" />
                            </label>
                        </div>
                    </div>

                    <!-- STEP 3: REVIEW -->
                    <div v-if="currentStep === 2" class="space-y-6 animate-fade-in-up">
                        <div class="bg-white rounded-2xl shadow-sm border border-gray-100 p-6 md:p-8">
                            <div class="flex items-center justify-between mb-6">
                                <h2 class="text-xl font-bold text-gray-900">Review Order Details</h2>
                                <button @click="currentStep = 0"
                                    class="text-sm text-blue-600 font-semibold hover:text-blue-700">Edit</button>
                            </div>

                            <div class="space-y-6">
                                <div class="flex gap-4 p-4 bg-gray-50 rounded-xl">
                                    <MapPin class="w-5 h-5 text-gray-500 shrink-0" />
                                    <div>
                                        <p class="text-sm text-gray-500 font-medium uppercase tracking-wider mb-1">
                                            Shipping To</p>
                                        <p class="font-bold text-gray-900">{{ form.fullName }}</p>
                                        <p class="text-gray-600">{{ form.address }}</p>
                                        <p class="text-gray-600">{{ form.city }}, {{ form.postalCode }}</p>
                                        <p class="text-gray-600 mt-1">{{ form.phone }}</p>
                                    </div>
                                </div>

                                <div class="flex gap-4 p-4 bg-gray-50 rounded-xl">
                                    <CreditCard class="w-5 h-5 text-gray-500 shrink-0" />
                                    <div>
                                        <p class="text-sm text-gray-500 font-medium uppercase tracking-wider mb-1">
                                            Payment Method</p>
                                        <p class="font-bold text-gray-900 capitalize">{{ form.paymentMethod === 'card' ?
                                            'Credit / Debit Card' : form.paymentMethod }}</p>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div class="bg-white rounded-2xl shadow-sm border border-gray-100 p-6 md:p-8">
                            <h3 class="font-bold text-gray-900 mb-4">Items in Order</h3>
                            <div class="space-y-4">
                                <div v-for="item in cartItems" :key="item.id"
                                    class="flex items-center gap-4 py-4 border-b border-gray-100 last:border-0">
                                    <div class="w-16 h-16 bg-gray-100 rounded-lg overflow-hidden shrink-0">
                                        <img :src="item.image || '/placeholder.png'" class="w-full h-full object-cover">
                                    </div>
                                    <div class="flex-1">
                                        <h4 class="font-bold text-gray-900">{{ item.name }}</h4>
                                        <p class="text-sm text-gray-500">Qty: {{ item.quantity }}</p>
                                    </div>
                                    <p class="font-bold text-gray-900">${{ (item.price * item.quantity).toFixed(2) }}
                                    </p>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- Navigation -->
                    <div class="flex items-center justify-between pt-4">
                        <button v-if="currentStep > 0" @click="prevStep"
                            class="px-6 py-3 text-gray-600 font-bold hover:text-gray-900 flex items-center gap-2 transition-colors">
                            <ArrowLeft class="w-4 h-4" /> Return
                        </button>
                        <div v-else class="px-6 py-3"></div> <!-- Spacer -->

                        <button v-if="currentStep < stepsList.length - 1" @click="nextStep"
                            class="px-8 py-3 bg-blue-600 text-white font-bold rounded-xl shadow-lg shadow-blue-600/20 hover:bg-blue-700 transform hover:-translate-y-0.5 transition-all flex items-center gap-2">
                            Continue to {{ stepsList[currentStep + 1] }}
                            <ChevronRight class="w-4 h-4" />
                        </button>

                        <button v-else @click="placeOrder" :disabled="processing"
                            class="px-8 py-3 bg-blue-600 text-white font-bold rounded-xl shadow-lg shadow-blue-600/20 hover:bg-blue-700 transform hover:-translate-y-0.5 transition-all flex items-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed">
                            <Loader2 v-if="processing" class="w-4 h-4 animate-spin" />
                            {{ processing ? 'Processing...' : 'Place Order' }}
                        </button>
                    </div>

                </div>

                <!-- Sidebar Summary -->
                <div class="lg:col-span-5 space-y-8">
                    <div class="bg-white rounded-2xl shadow-sm border border-gray-100 p-6 md:p-8 lg:sticky lg:top-24">
                        <h2 class="text-xl font-bold text-gray-900 mb-6">Order Summary</h2>

                        <div class="space-y-4 mb-6 max-h-64 overflow-y-auto pr-2">
                            <div v-for="item in cartItems" :key="item.id" class="flex gap-4">
                                <div class="w-16 h-16 bg-gray-100 rounded-lg overflow-hidden shrink-0 relative">
                                    <img :src="item.image || '/placeholder.png'" class="w-full h-full object-cover">
                                    <span
                                        class="absolute top-0 right-0 bg-gray-900 text-white text-[10px] w-5 h-5 flex items-center justify-center rounded-bl-lg font-bold">{{
                                        item.quantity }}</span>
                                </div>
                                <div class="flex-1">
                                    <h4 class="font-bold text-gray-900 text-sm line-clamp-2">{{ item.name }}</h4>
                                    <p class="text-sm text-gray-500 mt-1">${{ item.price.toFixed(2) }}</p>
                                </div>
                                <p class="font-bold text-gray-900 text-sm">${{ (item.price * item.quantity).toFixed(2)
                                    }}</p>
                            </div>
                        </div>

                        <div class="space-y-3 pt-6 border-t border-gray-100">
                            <div class="flex justify-between text-gray-600">
                                <span>Subtotal</span>
                                <span>${{ subtotal.toFixed(2) }}</span>
                            </div>
                            <div class="flex justify-between text-gray-600">
                                <span>Shipping</span>
                                <span v-if="shippingCost === 0" class="text-green-600 font-bold">Free</span>
                                <span v-else>${{ shippingCost.toFixed(2) }}</span>
                            </div>
                            <div class="flex justify-between text-gray-600">
                                <span>Taxes</span>
                                <span>${{ tax.toFixed(2) }}</span>
                            </div>
                        </div>

                        <div class="flex justify-between items-end pt-6 mt-6 border-t border-gray-100">
                            <span class="text-lg font-bold text-gray-900">Total</span>
                            <div class="text-right">
                                <span class="text-2xl font-black text-blue-600 block">${{ total.toFixed(2) }}</span>
                                <span class="text-xs text-gray-400 font-medium">USD</span>
                            </div>
                        </div>

                        <!-- Promo Code (Mock) -->
                        <div class="mt-8 flex gap-2">
                            <input type="text" placeholder="Promo code"
                                class="flex-1 px-4 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-blue-500">
                            <button
                                class="px-4 py-2 bg-gray-100 text-gray-700 font-bold text-sm rounded-lg hover:bg-gray-200">Apply</button>
                        </div>
                    </div>
                </div>

            </div>

        </div>
    </div>
</template>

<style scoped>
@keyframes fadeInUp {
    from {
        opacity: 0;
        transform: translateY(20px);
    }

    to {
        opacity: 1;
        transform: translateY(0);
    }
}

.animate-fade-in-up {
    animation: fadeInUp 0.5s ease-out forwards;
}
</style>
