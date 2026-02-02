<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { cartApi } from '../api/cartApi'
import { orderApi } from '../api/orderApi'
import { 
    CheckCircle, 
    CreditCard, 
    Truck, 
    ShieldCheck, 
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
    Trash2
} from 'lucide-vue-next'

const router = useRouter()
const loading = ref(false)
const processing = ref(false)
const cartItems = ref([])
const error = ref(null)

const steps = ['Review', 'Payment', 'Shipping']
const currentStep = ref(0)

const formData = ref({
    phone: '',
    location: '',
    paymentMethod: 'card' // 'card', 'qr', 'cash'
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
            quantity: item.quantity || 1
        }
    })
}

const fetchCart = async () => {
    try {
        loading.value = true
        const response = await cartApi.getCart()
        const cartData = response.data.data || response.data || {}
        // Handle both array and object with items property
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
    if (currentStep.value < steps.length - 1) {
        currentStep.value++
    }
}

const prevStep = () => {
    if (currentStep.value > 0) {
        currentStep.value--
    }
}

const placeOrder = async () => {
    // Validate required fields
    if (!formData.value.phone || !formData.value.location) {
        alert('Please fill in your phone number and delivery location')
        return
    }
    
    try {
        processing.value = true
        
        // Call real Order API
        const response = await orderApi.createOrder({
            paymentMethod: formData.value.paymentMethod,
            phone: formData.value.phone,
            location: formData.value.location
        })
        
        const order = response.data
        alert(`Order ${order.orderNumber} placed successfully!`)
        
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

// Update item quantity
const updateQuantity = async (item, delta) => {
    const newQuantity = item.quantity + delta
    if (newQuantity < 1) {
        await removeItem(item)
        return
    }
    
    try {
        await cartApi.updateCartItem(item.id, newQuantity)
        item.quantity = newQuantity
    } catch (err) {
        console.error('Failed to update quantity:', err)
    }
}

// Remove item from cart
const removeItem = async (item) => {
    try {
        await cartApi.removeFromCart(item.id)
        cartItems.value = cartItems.value.filter(i => i.id !== item.id)
    } catch (err) {
        console.error('Failed to remove item:', err)
    }
}

onMounted(() => {
    fetchCart()
})
</script>

<template>
    <div class="min-h-screen bg-gray-50 py-12">
        <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <!-- Header -->
            <div class="mb-10 text-center">
                <h1 class="text-3xl font-bold text-gray-900">Checkout</h1>
                <p class="mt-2 text-gray-600">Complete your purchase securely</p>
            </div>

            <div v-if="loading" class="flex justify-center py-12">
                <Loader2 class="w-8 h-8 animate-spin text-blue-600" />
            </div>

            <div v-else-if="cartItems.length === 0" class="text-center py-12 bg-white rounded-xl shadow-sm">
                <p class="text-lg text-gray-600">Your cart is empty</p>
                <button 
                    @click="router.push('/product')"
                    class="mt-4 px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
                >
                    Continue Shopping
                </button>
            </div>

            <div v-else class="grid lg:grid-cols-3 gap-8">
                <!-- Main Content -->
                <div class="lg:col-span-2 space-y-6">
                    <!-- Steps Indicator -->
                    <div class="bg-white p-6 rounded-2xl shadow-sm border border-gray-100">
                        <div class="flex justify-between items-center relative">
                            <div class="absolute left-0 top-1/2 w-full h-0.5 bg-gray-200 -z-10"></div>
                            <div 
                                v-for="(step, index) in steps" 
                                :key="step"
                                class="flex flex-col items-center gap-2 bg-white px-2"
                            >
                                <div 
                                    class="w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold transition-colors duration-300"
                                    :class="index <= currentStep ? 'bg-blue-600 text-white' : 'bg-gray-200 text-gray-500'"
                                >
                                    <CheckCircle v-if="index < currentStep" class="w-5 h-5" />
                                    <span v-else>{{ index + 1 }}</span>
                                </div>
                                <span 
                                    class="text-xs font-medium"
                                    :class="index <= currentStep ? 'text-blue-600' : 'text-gray-500'"
                                >
                                    {{ step }}
                                </span>
                            </div>
                        </div>
                    </div>

                    <!-- Step 1: Review Order -->
                    <div v-if="currentStep === 0" class="bg-white p-6 rounded-2xl shadow-sm border border-gray-100 space-y-6">
                        <h2 class="text-xl font-bold flex items-center gap-2">
                            <ShoppingBag class="w-5 h-5 text-blue-600" />
                            Review Your Order
                        </h2>
                        
                        <div class="space-y-4">
                            <div v-for="item in cartItems" :key="item.id" class="flex gap-4 p-4 bg-gray-50 rounded-xl">
                                <img 
                                    :src="item.image || '/placeholder.png'" 
                                    :alt="item.name"
                                    class="w-20 h-20 object-cover rounded-lg"
                                >
                                <div class="flex-1">
                                    <h3 class="font-medium text-gray-900">{{ item.name }}</h3>
                                    <p class="font-semibold text-blue-600 mt-1">${{ (item.price * item.quantity).toFixed(2) }}</p>
                                    
                                    <!-- Quantity Controls -->
                                    <div class="flex items-center gap-3 mt-2">
                                        <button 
                                            @click="updateQuantity(item, -1)"
                                            class="w-8 h-8 flex items-center justify-center rounded-lg border border-gray-300 hover:bg-gray-100 transition-colors"
                                        >
                                            <Minus class="w-4 h-4 text-gray-600" />
                                        </button>
                                        <span class="w-8 text-center font-medium">{{ item.quantity }}</span>
                                        <button 
                                            @click="updateQuantity(item, 1)"
                                            class="w-8 h-8 flex items-center justify-center rounded-lg border border-gray-300 hover:bg-gray-100 transition-colors"
                                        >
                                            <Plus class="w-4 h-4 text-gray-600" />
                                        </button>
                                        <button 
                                            @click="removeItem(item)"
                                            class="ml-auto w-8 h-8 flex items-center justify-center rounded-lg text-red-500 hover:bg-red-50 transition-colors"
                                        >
                                            <Trash2 class="w-4 h-4" />
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div class="border-t pt-4 space-y-2">
                            <div class="flex justify-between text-sm">
                                <span class="text-gray-600">Subtotal</span>
                                <span class="font-medium">${{ subtotal.toFixed(2) }}</span>
                            </div>
                            <div class="flex justify-between text-sm">
                                <span class="text-gray-600">Shipping</span>
                                <span class="font-medium">{{ shippingCost === 0 ? 'FREE' : '$' + shippingCost.toFixed(2) }}</span>
                            </div>
                            <div class="flex justify-between text-sm">
                                <span class="text-gray-600">Tax</span>
                                <span class="font-medium">${{ tax.toFixed(2) }}</span>
                            </div>
                            <div class="flex justify-between text-lg font-bold pt-2 border-t">
                                <span>Total</span>
                                <span class="text-blue-600">${{ total.toFixed(2) }}</span>
                            </div>
                        </div>
                    </div>

                    <!-- Step 2: Payment Method -->
                    <div v-if="currentStep === 1" class="bg-white p-6 rounded-2xl shadow-sm border border-gray-100 space-y-6">
                        <h2 class="text-xl font-bold flex items-center gap-2">
                            <CreditCard class="w-5 h-5 text-blue-600" />
                            Select Payment Method
                        </h2>
                        
                        <div class="space-y-4">
                            <!-- Bank Card Option -->
                            <label 
                                class="flex items-center gap-4 p-4 border-2 rounded-xl cursor-pointer transition-all"
                                :class="formData.paymentMethod === 'card' ? 'border-blue-500 bg-blue-50' : 'border-gray-200 hover:border-gray-300'"
                            >
                                <input 
                                    type="radio" 
                                    v-model="formData.paymentMethod" 
                                    value="card"
                                    class="w-5 h-5 text-blue-600"
                                >
                                <div class="flex items-center gap-3">
                                    <div class="w-12 h-12 bg-blue-100 rounded-xl flex items-center justify-center">
                                        <CreditCard class="w-6 h-6 text-blue-600" />
                                    </div>
                                    <div>
                                        <p class="font-medium text-gray-900">Bank Card</p>
                                        <p class="text-sm text-gray-500">Pay with credit or debit card</p>
                                    </div>
                                </div>
                            </label>

                            <!-- QR Payment Option -->
                            <label 
                                class="flex items-center gap-4 p-4 border-2 rounded-xl cursor-pointer transition-all"
                                :class="formData.paymentMethod === 'qr' ? 'border-blue-500 bg-blue-50' : 'border-gray-200 hover:border-gray-300'"
                            >
                                <input 
                                    type="radio" 
                                    v-model="formData.paymentMethod" 
                                    value="qr"
                                    class="w-5 h-5 text-blue-600"
                                >
                                <div class="flex items-center gap-3">
                                    <div class="w-12 h-12 bg-purple-100 rounded-xl flex items-center justify-center">
                                        <QrCode class="w-6 h-6 text-purple-600" />
                                    </div>
                                    <div>
                                        <p class="font-medium text-gray-900">QR Payment</p>
                                        <p class="text-sm text-gray-500">Scan QR code to pay</p>
                                    </div>
                                </div>
                            </label>

                            <!-- Cash on Delivery Option -->
                            <label 
                                class="flex items-center gap-4 p-4 border-2 rounded-xl cursor-pointer transition-all"
                                :class="formData.paymentMethod === 'cash' ? 'border-blue-500 bg-blue-50' : 'border-gray-200 hover:border-gray-300'"
                            >
                                <input 
                                    type="radio" 
                                    v-model="formData.paymentMethod" 
                                    value="cash"
                                    class="w-5 h-5 text-blue-600"
                                >
                                <div class="flex items-center gap-3">
                                    <div class="w-12 h-12 bg-green-100 rounded-xl flex items-center justify-center">
                                        <Banknote class="w-6 h-6 text-green-600" />
                                    </div>
                                    <div>
                                        <p class="font-medium text-gray-900">Cash on Delivery</p>
                                        <p class="text-sm text-gray-500">Pay when you receive your order</p>
                                    </div>
                                </div>
                            </label>
                        </div>

                        <div class="flex items-center gap-2 text-sm text-gray-500 mt-4">
                            <ShieldCheck class="w-4 h-4 text-green-500" />
                            <span>All payment methods are secure and protected.</span>
                        </div>
                    </div>

                    <!-- Step 3: Shipping Information -->
                    <div v-if="currentStep === 2" class="bg-white p-6 rounded-2xl shadow-sm border border-gray-100 space-y-6">
                        <h2 class="text-xl font-bold flex items-center gap-2">
                            <MapPin class="w-5 h-5 text-blue-600" />
                            Shipping Information
                        </h2>
                        
                        <div class="space-y-4">
                            <div class="space-y-2">
                                <label class="text-sm font-medium text-gray-700">Phone Number</label>
                                <div class="relative">
                                    <Phone class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                                    <input 
                                        v-model="formData.phone"
                                        type="tel" 
                                        class="w-full pl-10 pr-4 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-all"
                                        placeholder="+1 (555) 123-4567"
                                    >
                                </div>
                            </div>
                            
                            <div class="space-y-2">
                                <label class="text-sm font-medium text-gray-700">Delivery Location</label>
                                <div class="relative">
                                    <MapPin class="absolute left-3 top-3 w-4 h-4 text-gray-400" />
                                    <textarea 
                                        v-model="formData.location"
                                        rows="4"
                                        class="w-full pl-10 pr-4 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-all resize-none"
                                        placeholder="Enter your full delivery address..."
                                    ></textarea>
                                </div>
                            </div>
                        </div>

                        <!-- Order Summary in Shipping Step -->
                        <div class="bg-gray-50 p-4 rounded-xl space-y-3">
                            <h3 class="font-medium text-gray-900">Order Summary</h3>
                            <div class="flex justify-between text-sm">
                                <span class="text-gray-600">Payment Method</span>
                                <span class="font-medium capitalize">{{ formData.paymentMethod === 'card' ? 'Bank Card' : formData.paymentMethod === 'qr' ? 'QR Payment' : 'Cash on Delivery' }}</span>
                            </div>
                            <div class="flex justify-between text-sm">
                                <span class="text-gray-600">Items ({{ cartItems.length }})</span>
                                <span class="font-medium">${{ subtotal.toFixed(2) }}</span>
                            </div>
                            <div class="flex justify-between font-bold text-blue-600 pt-2 border-t">
                                <span>Total</span>
                                <span>${{ total.toFixed(2) }}</span>
                            </div>
                        </div>
                    </div>

                    <!-- Navigation Buttons -->
                    <div class="flex justify-between pt-6">
                        <button 
                            v-if="currentStep > 0"
                            @click="prevStep"
                            class="px-6 py-3 border border-gray-300 text-gray-700 font-medium rounded-xl hover:bg-gray-50 transition-colors flex items-center gap-2"
                        >
                            <ArrowLeft class="w-4 h-4" />
                            Back
                        </button>
                        <div v-else></div> <!-- Spacer -->

                        <button 
                            v-if="currentStep < steps.length - 1"
                            @click="nextStep"
                            class="px-8 py-3 bg-blue-600 text-white font-medium rounded-xl hover:bg-blue-700 transition-colors shadow-lg shadow-blue-600/20"
                        >
                            Continue
                        </button>
                        <button 
                            v-else
                            @click="placeOrder"
                            :disabled="processing"
                            class="px-8 py-3 bg-green-600 text-white font-medium rounded-xl hover:bg-green-700 transition-colors shadow-lg shadow-green-600/20 flex items-center gap-2 disabled:opacity-70 disabled:cursor-not-allowed"
                        >
                            <Loader2 v-if="processing" class="w-4 h-4 animate-spin" />
                            {{ processing ? 'Processing...' : 'Place Order' }}
                        </button>
                    </div>
                </div>

                <!-- Order Summary -->
                <div class="lg:col-span-1">
                    <div class="bg-white p-6 rounded-2xl shadow-sm border border-gray-100 sticky top-8">
                        <h2 class="text-lg font-bold text-gray-900 mb-6">Order Summary</h2>
                        
                        <div class="space-y-4 mb-6 max-h-80 overflow-y-auto pr-2 custom-scrollbar">
                            <div v-for="item in cartItems" :key="item.id" class="flex gap-3">
                                <img 
                                    :src="item.image || 'https://via.placeholder.com/60'" 
                                    :alt="item.name"
                                    class="w-16 h-16 object-cover rounded-lg bg-gray-100"
                                >
                                <div class="flex-1">
                                    <h4 class="text-sm font-medium text-gray-900 line-clamp-2">{{ item.name }}</h4>
                                    <div class="flex justify-between mt-1">
                                        <p class="text-xs text-gray-500">Qty: {{ item.quantity }}</p>
                                        <p class="text-sm font-medium text-gray-900">${{ (item.price * item.quantity).toFixed(2) }}</p>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div class="space-y-3 pt-4 border-t border-gray-100">
                            <div class="flex justify-between text-sm text-gray-600">
                                <span>Subtotal</span>
                                <span>${{ subtotal.toFixed(2) }}</span>
                            </div>
                            <div class="flex justify-between text-sm text-gray-600">
                                <span>Shipping</span>
                                <span>{{ shippingCost === 0 ? 'Free' : `$${shippingCost.toFixed(2)}` }}</span>
                            </div>
                            <div class="flex justify-between text-sm text-gray-600">
                                <span>Tax (8%)</span>
                                <span>${{ tax.toFixed(2) }}</span>
                            </div>
                            <div class="flex justify-between text-lg font-bold text-gray-900 pt-3 border-t border-gray-100">
                                <span>Total</span>
                                <span>${{ total.toFixed(2) }}</span>
                            </div>
                        </div>

                        <div class="mt-6 flex items-center gap-2 text-xs text-gray-500 bg-gray-50 p-3 rounded-lg">
                            <ShieldCheck class="w-4 h-4 text-green-600" />
                            <p>Secure Checkout powered by Stripe</p>
                        </div>
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
    background: #f1f1f1;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
    background: #d1d5db;
    border-radius: 4px;
}
</style>
