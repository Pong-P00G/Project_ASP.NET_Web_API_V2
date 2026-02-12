<script setup>
import { ref, onMounted } from 'vue'
import { useAuthStore } from '../../stores/Auth'
import { profileApi } from '../../api/profileApi'
import { orderApi } from '../../api/orderApi'
import { useRouter, useRoute } from 'vue-router'
import {
    User, Mail, Lock, MapPin, CreditCard, Save, Plus, Edit2, Trash2, X,
    Loader2, CheckCircle, AlertCircle, Camera, Eye, EyeOff, ShoppingBag,
    Package, ChevronRight, ArrowRight, Phone, Shield, Truck
} from 'lucide-vue-next'

const authStore = useAuthStore()
const router = useRouter()
const route = useRoute()

const loading = ref(false)
const saving = ref(false)
const error = ref(null)
const success = ref(null)
const isLoaded = ref(false)

// Active tab
const activeTab = ref('profile')

// Profile data
const profileData = ref({
    name: '',
    email: '',
    phone: '',
    avatar: null
})

// Password change
const passwordData = ref({
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
})
const showPasswords = ref({
    current: false,
    new: false,
    confirm: false
})

// Addresses
const addresses = ref([])
const showAddressModal = ref(false)
const editingAddress = ref(null)
const addressForm = ref({
    street: '', city: '', state: '', zipCode: '', country: '', isDefault: false
})

// Payment methods
const paymentMethods = ref([])
const showPaymentModal = ref(false)
const editingPayment = ref(null)
const paymentForm = ref({
    cardNumber: '', cardHolder: '', expiryMonth: '', expiryYear: '', cvv: '', isDefault: false
})

// Orders
const orders = ref([])
const loadingOrders = ref(false)

// Avatar upload
const avatarInput = ref(null)
const uploadingAvatar = ref(false)

const tabs = [
    { id: 'profile', label: 'Personal Info', icon: User },
    { id: 'addresses', label: 'Addresses', icon: MapPin },
    { id: 'security', label: 'Security', icon: Lock },
    { id: 'payments', label: 'Payment Methods', icon: CreditCard },
    { id: 'orders', label: 'Order History', icon: ShoppingBag }
]

// ===== FETCH DATA =====
const fetchProfile = async () => {
    try {
        loading.value = true
        error.value = null
        const response = await profileApi.getProfile()
        const data = response.data.data || response.data
        profileData.value = {
            name: data.name || data.username || '',
            email: data.email || '',
            phone: data.phone || '',
            avatar: data.avatar || null
        }
    } catch (err) {
        error.value = err.response?.data?.message || 'Failed to load profile'
        profileData.value = {
            name: authStore.user?.name || authStore.user?.username || '',
            email: authStore.user?.email || '',
            phone: authStore.user?.phone || '',
            avatar: authStore.user?.avatar || null
        }
    } finally {
        loading.value = false
    }
}

const fetchAddresses = async () => {
    try {
        const response = await profileApi.getAddresses()
        addresses.value = response.data.data || response.data || []
    } catch (err) {
        console.error('Failed to load addresses:', err)
        addresses.value = []
    }
}

const fetchPaymentMethods = async () => {
    try {
        const response = await profileApi.getPaymentMethods()
        paymentMethods.value = response.data.data || response.data || []
    } catch (err) {
        console.error('Failed to load payment methods:', err)
        paymentMethods.value = []
    }
}

const fetchOrders = async () => {
    try {
        loadingOrders.value = true
        const response = await orderApi.getOrders()
        orders.value = response.data.data || response.data || []
    } catch (err) {
        console.error('Failed to load orders:', err)
        orders.value = []
    } finally {
        loadingOrders.value = false
    }
}

// ===== PROFILE UPDATE =====
const triggerAvatarUpload = () => {
    avatarInput.value?.click()
}

const handleAvatarChange = async (event) => {
    const file = event.target.files[0]
    if (!file) return
    if (!file.type.startsWith('image/')) {
        error.value = 'Please select an image file'
        return
    }
    if (file.size > 5 * 1024 * 1024) {
        error.value = 'File too large. Maximum 5MB.'
        return
    }
    try {
        uploadingAvatar.value = true
        error.value = null
        const response = await profileApi.uploadAvatar(file)
        profileData.value.avatar = response.data.data?.avatar || response.data.avatar
        if (authStore.user) authStore.user.avatar = profileData.value.avatar
        showSuccess('Avatar updated successfully!')
    } catch (err) {
        error.value = err.response?.data?.message || 'Failed to upload avatar'
    } finally {
        uploadingAvatar.value = false
        if (avatarInput.value) avatarInput.value.value = ''
    }
}

const handleUpdateProfile = async () => {
    try {
        saving.value = true
        error.value = null
        await profileApi.updateProfile({
            name: profileData.value.name,
            email: profileData.value.email,
            phone: profileData.value.phone
        })
        showSuccess('Profile updated successfully!')
        await fetchProfile()
    } catch (err) {
        error.value = err.response?.data?.message || 'Failed to update profile'
    } finally {
        saving.value = false
    }
}

const handleChangePassword = async () => {
    if (passwordData.value.newPassword !== passwordData.value.confirmPassword) {
        error.value = 'New passwords do not match'
        return
    }
    if (passwordData.value.newPassword.length < 6) {
        error.value = 'Password must be at least 6 characters'
        return
    }
    try {
        saving.value = true
        error.value = null
        await profileApi.changePassword({
            currentPassword: passwordData.value.currentPassword,
            newPassword: passwordData.value.newPassword
        })
        showSuccess('Password changed successfully!')
        passwordData.value = { currentPassword: '', newPassword: '', confirmPassword: '' }
    } catch (err) {
        error.value = err.response?.data?.message || 'Failed to change password'
    } finally {
        saving.value = false
    }
}

// ===== ADDRESS MANAGEMENT =====
const openAddressModal = (address = null) => {
    editingAddress.value = address
    addressForm.value = address
        ? { ...address }
        : { street: '', city: '', state: '', zipCode: '', country: '', isDefault: false }
    showAddressModal.value = true
}

const closeAddressModal = () => {
    showAddressModal.value = false
    editingAddress.value = null
}

const handleSaveAddress = async () => {
    try {
        saving.value = true
        error.value = null
        if (editingAddress.value) {
            await profileApi.updateAddress(editingAddress.value.id, addressForm.value)
            showSuccess('Address updated!')
        } else {
            await profileApi.addAddress(addressForm.value)
            showSuccess('Address added!')
        }
        closeAddressModal()
        await fetchAddresses()
    } catch (err) {
        error.value = err.response?.data?.message || 'Failed to save address'
    } finally {
        saving.value = false
    }
}

const handleDeleteAddress = async (id) => {
    if (!confirm('Delete this address?')) return
    try {
        saving.value = true
        await profileApi.deleteAddress(id)
        showSuccess('Address deleted!')
        await fetchAddresses()
    } catch (err) {
        error.value = err.response?.data?.message || 'Failed to delete address'
    } finally {
        saving.value = false
    }
}

// ===== PAYMENT MANAGEMENT =====
const openPaymentModal = (payment = null) => {
    editingPayment.value = payment
    paymentForm.value = payment
        ? { cardNumber: payment.cardNumber || '', cardHolder: payment.cardHolder || '', expiryMonth: payment.expiryMonth || '', expiryYear: payment.expiryYear || '', cvv: '', isDefault: payment.isDefault || false }
        : { cardNumber: '', cardHolder: '', expiryMonth: '', expiryYear: '', cvv: '', isDefault: false }
    showPaymentModal.value = true
}

const closePaymentModal = () => {
    showPaymentModal.value = false
    editingPayment.value = null
}

const handleSavePayment = async () => {
    try {
        saving.value = true
        error.value = null
        const data = { ...paymentForm.value, cardNumber: paymentForm.value.cardNumber.replace(/\s/g, '') }
        if (editingPayment.value) {
            await profileApi.updatePaymentMethod(editingPayment.value.id, data)
            showSuccess('Payment method updated!')
        } else {
            await profileApi.addPaymentMethod(data)
            showSuccess('Payment method added!')
        }
        closePaymentModal()
        await fetchPaymentMethods()
    } catch (err) {
        error.value = err.response?.data?.message || 'Failed to save payment method'
    } finally {
        saving.value = false
    }
}

const handleDeletePayment = async (id) => {
    if (!confirm('Delete this payment method?')) return
    try {
        saving.value = true
        await profileApi.deletePaymentMethod(id)
        showSuccess('Payment method deleted!')
        await fetchPaymentMethods()
    } catch (err) {
        error.value = err.response?.data?.message || 'Failed to delete payment method'
    } finally {
        saving.value = false
    }
}

// ===== HELPERS =====
const showSuccess = (msg) => {
    success.value = msg
    setTimeout(() => { success.value = null }, 3000)
}

onMounted(async () => {
    if (route.query.tab) activeTab.value = route.query.tab
    await fetchProfile()
    await fetchAddresses()
    await fetchPaymentMethods()
    await fetchOrders()
    setTimeout(() => { isLoaded.value = true }, 100)
})
</script>

<template>
    <div class="min-h-screen bg-[#F5F5F5] pb-16">
        <!-- Hero -->
        <div class="relative h-[240px] bg-[#0A0A0A] flex items-end overflow-hidden md:h-[200px]">
            <div class="absolute inset-0 bg-[linear-gradient(135deg,rgba(200,169,126,0.08)_0%,transparent_50%)]"></div>
            <div class="relative max-w-[1100px] w-full mx-auto px-8 pb-10">
                <nav class="flex items-center gap-2 text-[12px] text-white/35 mb-3 transition-all duration-600 ease-out delay-100"
                    :class="isLoaded ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-2'">
                    <router-link to="/"
                        class="text-white/50 no-underline transition-colors hover:text-[#C8A97E]">Home</router-link>
                    <ChevronRight :size="14" />
                    <router-link to="/profile"
                        class="text-white/50 no-underline transition-colors hover:text-[#C8A97E]">Account</router-link>
                    <ChevronRight :size="14" />
                    <span class="text-[#C8A97E] font-semibold">Settings</span>
                </nav>
                <h1 class="text-[clamp(2rem,4vw,3rem)] font-light text-white tracking-tight transition-all duration-600 ease-out delay-300"
                    :class="isLoaded ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-3'">Settings</h1>
            </div>
        </div>

        <div class="max-w-[1100px] mx-auto -mt-6 px-6 relative z-10">
            <!-- Notifications -->
            <Transition name="slide-fade">
                <div v-if="error"
                    class="flex items-center gap-3 px-5 py-3.5 rounded-xl mb-4 text-[13px] font-medium bg-[#FFF1F0] text-[#E53935] border border-[#FFCDD2]">
                    <AlertCircle :size="18" />
                    <span>{{ error }}</span>
                    <button @click="error = null"
                        class="ml-auto bg-transparent border-none cursor-pointer opacity-60 text-inherit hover:opacity-100">
                        <X :size="16" />
                    </button>
                </div>
            </Transition>
            <Transition name="slide-fade">
                <div v-if="success"
                    class="flex items-center gap-3 px-5 py-3.5 rounded-xl mb-4 text-[13px] font-medium bg-[#F1F8E9] text-[#43A047] border border-[#C8E6C9]">
                    <CheckCircle :size="18" />
                    <span>{{ success }}</span>
                </div>
            </Transition>

            <div class="grid grid-cols-[220px_1fr] gap-6 transition-all duration-500 ease-out delay-400 md:grid-cols-1"
                :class="isLoaded ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'">
                <!-- Sidebar -->
                <aside class="sticky top-24 self-start md:static md:top-auto">
                    <nav class="bg-white border border-[#E8E8E8] rounded-2xl p-2 flex flex-col gap-0.5">
                        <button v-for="tab in tabs" :key="tab.id" @click="activeTab = tab.id"
                            class="w-full flex items-center gap-2.5 px-3.5 py-2.5 rounded-[10px] border-none bg-none text-[13px] font-medium text-[#666] cursor-pointer transition-all duration-200 text-left hover:bg-[#F5F5F5] hover:text-[#333]"
                            :class="{ 'bg-[#0A0A0A] text-[#C8A97E] font-semibold hover:bg-[#1A1A1A] hover:text-[#C8A97E]': activeTab === tab.id }">
                            <component :is="tab.icon" :size="18" />
                            <span>{{ tab.label }}</span>
                        </button>
                    </nav>
                </aside>

                <!-- Main Content -->
                <main class="flex flex-col gap-5">

                    <!-- ===== PROFILE TAB ===== -->
                    <div v-if="activeTab === 'profile'" class="flex flex-col gap-5">
                        <!-- Avatar Card -->
                        <div class="bg-white border border-[#E8E8E8] rounded-2xl p-8">
                            <div class="flex items-start justify-between mb-6 gap-4">
                                <div>
                                    <h2 class="text-[16px] font-bold text-[#0A0A0A] tracking-tight mb-1">Profile</h2>
                                    <p class="text-[13px] text-[#999]">Public profile information</p>
                                </div>
                            </div>
                            <div class="flex items-center gap-6">
                                <div class="cursor-pointer shrink-0" @click="triggerAvatarUpload">
                                    <div
                                        class="relative w-20 h-20 rounded-full overflow-hidden border-[3px] border-[#C8A97E] transition-all duration-300 hover:scale-105 hover:shadow-[0_0_0_4px_rgba(200,169,126,0.2)] group">
                                        <img v-if="profileData.avatar" :src="profileData.avatar" :alt="profileData.name"
                                            class="w-full h-full object-cover" />
                                        <div v-else
                                            class="w-full h-full bg-[linear-gradient(135deg,#0A0A0A,#1A1A1A)] flex items-center justify-center text-[1.75rem] font-semibold text-[#C8A97E]">
                                            {{ (profileData.name || 'U').charAt(0).toUpperCase() }}
                                        </div>
                                        <div
                                            class="absolute inset-0 bg-black/50 flex items-center justify-center text-white opacity-0 transition-opacity duration-300 group-hover:opacity-100">
                                            <Loader2 v-if="uploadingAvatar" :size="20" class="animate-spin" />
                                            <Camera v-else :size="20" />
                                        </div>
                                    </div>
                                    <input ref="avatarInput" type="file" accept="image/*" class="hidden"
                                        @change="handleAvatarChange" />
                                </div>
                                <div>
                                    <button @click="triggerAvatarUpload"
                                        class="inline-flex items-center gap-2 px-4 py-2 bg-transparent text-[#333] border border-[#DDD] rounded-[10px] text-[12px] font-semibold cursor-pointer transition-all hover:border-[#333]">
                                        Change photo
                                    </button>
                                    <p class="text-[11px] text-[#999] mt-1.5">JPG, PNG, GIF or WebP. Max 5MB.</p>
                                </div>
                            </div>
                        </div>

                        <!-- Personal Info Form -->
                        <div class="bg-white border border-[#E8E8E8] rounded-2xl p-8">
                            <h2 class="text-[16px] font-bold text-[#0A0A0A] tracking-tight mb-1 mb-6">Personal
                                Information</h2>
                            <div class="flex flex-col gap-5 w-full">
                                <div class="flex flex-col gap-1.5">
                                    <label
                                        class="text-[11px] font-semibold uppercase tracking-[0.12em] text-[#666]">Full
                                        Name</label>
                                    <div class="relative flex items-center">
                                        <User :size="16" class="absolute left-3.5 text-[#999] pointer-events-none" />
                                        <input v-model="profileData.name" type="text"
                                            class="w-full pl-[40px] pr-3.5 py-2.5 border border-[#E0E0E0] rounded-[10px] text-[14px] text-[#0A0A0A] bg-white outline-none transition-all duration-200 focus:border-[#C8A97E] focus:shadow-[0_0_0_3px_rgba(200,169,126,0.12)] placeholder:text-[#ccc]"
                                            placeholder="Your full name" />
                                    </div>
                                </div>
                                <div class="flex flex-col gap-1.5">
                                    <label
                                        class="text-[11px] font-semibold uppercase tracking-[0.12em] text-[#666]">Email
                                        Address</label>
                                    <div class="relative flex items-center">
                                        <Mail :size="16" class="absolute left-3.5 text-[#999] pointer-events-none" />
                                        <input v-model="profileData.email" type="email"
                                            class="w-full pl-[40px] pr-3.5 py-2.5 border border-[#E0E0E0] rounded-[10px] text-[14px] text-[#0A0A0A] bg-white outline-none transition-all duration-200 focus:border-[#C8A97E] focus:shadow-[0_0_0_3px_rgba(200,169,126,0.12)] placeholder:text-[#ccc]"
                                            placeholder="your@email.com" />
                                        <CheckCircle v-if="profileData.email" :size="16"
                                            class="absolute right-3.5 text-[#43A047]" />
                                    </div>
                                </div>
                                <div class="flex flex-col gap-1.5">
                                    <label
                                        class="text-[11px] font-semibold uppercase tracking-[0.12em] text-[#666]">Phone
                                        Number</label>
                                    <div class="relative flex items-center">
                                        <Phone :size="16" class="absolute left-3.5 text-[#999] pointer-events-none" />
                                        <input v-model="profileData.phone" type="tel"
                                            class="w-full pl-[40px] pr-3.5 py-2.5 border border-[#E0E0E0] rounded-[10px] text-[14px] text-[#0A0A0A] bg-white outline-none transition-all duration-200 focus:border-[#C8A97E] focus:shadow-[0_0_0_3px_rgba(200,169,126,0.12)] placeholder:text-[#ccc]"
                                            placeholder="+1 (555) 000-0000" />
                                    </div>
                                </div>
                            </div>
                            <div class="flex justify-end mt-6 pt-6 border-t border-[#F0F0F0]">
                                <button @click="handleUpdateProfile" :disabled="saving"
                                    class="inline-flex items-center gap-2 px-6 py-2.5 bg-[#C8A97E] text-[#0A0A0A] border-none rounded-[10px] text-[13px] font-semibold tracking-wide cursor-pointer transition-all duration-300 hover:bg-[#D4B98E] disabled:opacity-50 disabled:cursor-not-allowed">
                                    <Loader2 v-if="saving" :size="16" class="animate-spin" />
                                    <Save v-else :size="16" />
                                    <span>{{ saving ? 'Saving...' : 'Save Changes' }}</span>
                                </button>
                            </div>
                        </div>
                    </div>

                    <!-- ===== ADDRESSES TAB ===== -->
                    <div v-if="activeTab === 'addresses'" class="flex flex-col gap-5">
                        <div class="bg-white border border-[#E8E8E8] rounded-2xl p-8">
                            <div class="flex items-start justify-between mb-6 gap-4">
                                <div>
                                    <h2 class="text-[16px] font-bold text-[#0A0A0A] tracking-tight mb-1">Saved Addresses
                                    </h2>
                                    <p class="text-[13px] text-[#999]">Manage shipping and billing addresses</p>
                                </div>
                                <button @click="openAddressModal()"
                                    class="inline-flex items-center gap-2 px-4 py-2 bg-[#C8A97E] text-[#0A0A0A] border-none rounded-[10px] text-[12px] font-semibold tracking-wide cursor-pointer transition-all duration-300 hover:bg-[#D4B98E]">
                                    <Plus :size="16" /> Add Address
                                </button>
                            </div>

                            <div class="grid grid-cols-[repeat(auto-fill,minmax(260px,1fr))] gap-4">
                                <div v-for="address in addresses" :key="address.id"
                                    class="p-5 border border-[#E8E8E8] rounded-xl transition-colors duration-300 hover:border-[#C8A97E] group">
                                    <div class="flex justify-between items-center mb-3">
                                        <span v-if="address.isDefault"
                                            class="inline-block px-2.5 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-widest bg-[#C8A97E]/15 text-[#C8A97E]">Default</span>
                                        <span v-else
                                            class="inline-block px-2.5 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-widest bg-[#F5F5F5] text-[#666]">Address</span>
                                        <div
                                            class="flex gap-1 opacity-0 transition-opacity duration-200 group-hover:opacity-100">
                                            <button @click="openAddressModal(address)"
                                                class="bg-transparent border-none p-1.5 rounded-lg cursor-pointer text-[#999] transition-all hover:bg-[#F5F5F5] hover:text-[#333]">
                                                <Edit2 :size="14" />
                                            </button>
                                            <button @click="handleDeleteAddress(address.id)"
                                                class="bg-transparent border-none p-1.5 rounded-lg cursor-pointer text-[#999] transition-all hover:bg-[#FFF1F0] hover:text-[#E53935]">
                                                <Trash2 :size="14" />
                                            </button>
                                        </div>
                                    </div>
                                    <p class="text-[13px] text-[#666] leading-[1.6]">
                                        {{ address.street }}<br />
                                        {{ address.city }}, {{ address.state }} {{ address.zipCode }}<br />
                                        {{ address.country }}
                                    </p>
                                </div>

                                <!-- Add placeholder -->
                                <button @click="openAddressModal()"
                                    class="flex flex-col items-center justify-center gap-2 p-8 border-2 border-dashed border-[#DDD] rounded-xl text-[#999] text-[13px] font-semibold cursor-pointer transition-all duration-300 bg-transparent min-h-[150px] hover:border-[#C8A97E] hover:text-[#C8A97E]">
                                    <MapPin :size="28" />
                                    <span>Add a new address</span>
                                </button>
                            </div>
                        </div>
                    </div>

                    <!-- ===== SECURITY TAB ===== -->
                    <div v-if="activeTab === 'security'" class="flex flex-col gap-5">
                        <div class="bg-white border border-[#E8E8E8] rounded-2xl p-8">
                            <div class="flex items-start justify-between mb-6 gap-4">
                                <div>
                                    <h2 class="text-[16px] font-bold text-[#0A0A0A] tracking-tight mb-1">Security</h2>
                                    <p class="text-[13px] text-[#999]">Manage your password</p>
                                </div>
                            </div>
                            <form @submit.prevent="handleChangePassword">
                                <div class="flex flex-col gap-5 max-w-[480px]">
                                    <div class="flex flex-col gap-1.5">
                                        <label
                                            class="text-[11px] font-semibold uppercase tracking-[0.12em] text-[#666]">Current
                                            Password</label>
                                        <div class="relative flex items-center">
                                            <Lock :size="16"
                                                class="absolute left-3.5 text-[#999] pointer-events-none" />
                                            <input v-model="passwordData.currentPassword"
                                                :type="showPasswords.current ? 'text' : 'password'"
                                                class="w-full pl-[40px] pr-3.5 py-2.5 border border-[#E0E0E0] rounded-[10px] text-[14px] text-[#0A0A0A] bg-white outline-none transition-all duration-200 focus:border-[#C8A97E] focus:shadow-[0_0_0_3px_rgba(200,169,126,0.12)] placeholder:text-[#ccc]"
                                                placeholder="••••••••" />
                                            <button @click="showPasswords.current = !showPasswords.current"
                                                class="absolute right-3.5 bg-transparent border-none cursor-pointer text-[#999] transition-colors hover:text-[#333]"
                                                type="button">
                                                <Eye v-if="!showPasswords.current" :size="16" />
                                                <EyeOff v-else :size="16" />
                                            </button>
                                        </div>
                                    </div>
                                    <div class="flex flex-col gap-1.5">
                                        <label
                                            class="text-[11px] font-semibold uppercase tracking-[0.12em] text-[#666]">New
                                            Password</label>
                                        <div class="relative flex items-center">
                                            <Lock :size="16"
                                                class="absolute left-3.5 text-[#999] pointer-events-none" />
                                            <input v-model="passwordData.newPassword"
                                                :type="showPasswords.new ? 'text' : 'password'"
                                                class="w-full pl-[40px] pr-3.5 py-2.5 border border-[#E0E0E0] rounded-[10px] text-[14px] text-[#0A0A0A] bg-white outline-none transition-all duration-200 focus:border-[#C8A97E] focus:shadow-[0_0_0_3px_rgba(200,169,126,0.12)] placeholder:text-[#ccc]"
                                                placeholder="••••••••" />
                                            <button @click="showPasswords.new = !showPasswords.new"
                                                class="absolute right-3.5 bg-transparent border-none cursor-pointer text-[#999] transition-colors hover:text-[#333]"
                                                type="button">
                                                <Eye v-if="!showPasswords.new" :size="16" />
                                                <EyeOff v-else :size="16" />
                                            </button>
                                        </div>
                                    </div>
                                    <div class="flex flex-col gap-1.5">
                                        <label
                                            class="text-[11px] font-semibold uppercase tracking-[0.12em] text-[#666]">Confirm
                                            New Password</label>
                                        <div class="relative flex items-center">
                                            <Lock :size="16"
                                                class="absolute left-3.5 text-[#999] pointer-events-none" />
                                            <input v-model="passwordData.confirmPassword"
                                                :type="showPasswords.confirm ? 'text' : 'password'"
                                                class="w-full pl-[40px] pr-3.5 py-2.5 border border-[#E0E0E0] rounded-[10px] text-[14px] text-[#0A0A0A] bg-white outline-none transition-all duration-200 focus:border-[#C8A97E] focus:shadow-[0_0_0_3px_rgba(200,169,126,0.12)] placeholder:text-[#ccc]"
                                                placeholder="••••••••" />
                                            <button @click="showPasswords.confirm = !showPasswords.confirm"
                                                class="absolute right-3.5 bg-transparent border-none cursor-pointer text-[#999] transition-colors hover:text-[#333]"
                                                type="button">
                                                <Eye v-if="!showPasswords.confirm" :size="16" />
                                                <EyeOff v-else :size="16" />
                                            </button>
                                        </div>
                                    </div>
                                </div>
                                <div class="flex justify-end mt-6 pt-6 border-t border-[#F0F0F0]">
                                    <button type="submit" :disabled="saving"
                                        class="inline-flex items-center gap-2 px-6 py-2.5 bg-[#0A0A0A] text-white border-none rounded-[10px] text-[13px] font-semibold tracking-wide cursor-pointer transition-all duration-300 hover:bg-[#1A1A1A] disabled:opacity-50 disabled:cursor-not-allowed">
                                        <Loader2 v-if="saving" :size="16" class="animate-spin" />
                                        <Shield v-else :size="16" />
                                        <span>{{ saving ? 'Updating...' : 'Update Password' }}</span>
                                    </button>
                                </div>
                            </form>
                        </div>
                    </div>

                    <!-- ===== PAYMENTS TAB ===== -->
                    <div v-if="activeTab === 'payments'" class="flex flex-col gap-5">
                        <div class="bg-white border border-[#E8E8E8] rounded-2xl p-8">
                            <div class="flex items-start justify-between mb-6 gap-4">
                                <div>
                                    <h2 class="text-[16px] font-bold text-[#0A0A0A] tracking-tight mb-1">Payment Methods
                                    </h2>
                                    <p class="text-[13px] text-[#999]">Manage saved cards</p>
                                </div>
                                <button @click="openPaymentModal()"
                                    class="inline-flex items-center gap-2 px-4 py-2 bg-[#C8A97E] text-[#0A0A0A] border-none rounded-[10px] text-[12px] font-semibold tracking-wide cursor-pointer transition-all duration-300 hover:bg-[#D4B98E]">
                                    <Plus :size="16" /> Add Card
                                </button>
                            </div>
                            <div class="flex flex-col gap-3">
                                <div v-for="payment in paymentMethods" :key="payment.id"
                                    class="flex items-center justify-between p-4 border border-[#E8E8E8] rounded-xl transition-colors duration-300 hover:border-[#C8A97E] group">
                                    <div class="flex items-center gap-4">
                                        <div
                                            class="w-10 h-7 bg-[#F5F5F5] rounded-md flex items-center justify-center text-[#666]">
                                            <CreditCard :size="20" />
                                        </div>
                                        <div>
                                            <p class="text-[14px] font-semibold text-[#0A0A0A]">•••• •••• •••• {{
                                                payment.cardNumber?.slice(-4) }}</p>
                                            <p class="text-[11px] text-[#999]">Expires {{ payment.expiryMonth }}/{{
                                                payment.expiryYear }}</p>
                                        </div>
                                        <span v-if="payment.isDefault"
                                            class="inline-block px-2.5 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-widest bg-[#C8A97E]/15 text-[#C8A97E]">Default</span>
                                    </div>
                                    <button @click="handleDeletePayment(payment.id)"
                                        class="bg-transparent border-none p-1.5 rounded-lg cursor-pointer text-[#999] transition-all hover:bg-[#FFF1F0] hover:text-[#E53935] opacity-0 group-hover:opacity-100">
                                        <Trash2 :size="14" />
                                    </button>
                                </div>
                                <div v-if="paymentMethods.length === 0"
                                    class="flex flex-col items-center gap-2 py-12 px-8 text-[#999] text-center">
                                    <CreditCard :size="40" />
                                    <p>No payment methods saved</p>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- ===== ORDERS TAB ===== -->
                    <div v-if="activeTab === 'orders'" class="flex flex-col gap-5">
                        <div class="bg-white border border-[#E8E8E8] rounded-2xl p-8">
                            <div class="flex items-start justify-between mb-6 gap-4">
                                <div>
                                    <h2 class="text-[16px] font-bold text-[#0A0A0A] tracking-tight mb-1">Order History
                                    </h2>
                                    <p class="text-[13px] text-[#999]">Track your recent purchases</p>
                                </div>
                            </div>

                            <div v-if="loadingOrders"
                                class="flex flex-col items-center gap-4 py-12 text-[#666] text-[13px]">
                                <Loader2 :size="28" class="animate-spin" />
                                <p>Loading orders...</p>
                            </div>

                            <div v-else-if="orders.length === 0"
                                class="flex flex-col items-center gap-2 py-12 px-8 text-[#999] text-center">
                                <Package :size="48" />
                                <p class="text-[16px] font-bold text-[#0A0A0A]">No orders yet</p>
                                <p class="text-[13px] text-[#999] mb-4">Start shopping to see your orders here.</p>
                                <button @click="router.push('/product')"
                                    class="inline-flex items-center gap-2 px-6 py-2.5 bg-[#C8A97E] text-[#0A0A0A] border-none rounded-[10px] text-[13px] font-semibold tracking-wide cursor-pointer transition-all duration-300 hover:bg-[#D4B98E]">Browse
                                    Products</button>
                            </div>

                            <div v-else class="flex flex-col gap-3">
                                <div v-for="order in orders" :key="order.orderId"
                                    class="p-5 border border-[#E8E8E8] rounded-xl transition-colors duration-300 hover:border-[#C8A97E]">
                                    <div class="flex justify-between items-start gap-4 flex-wrap">
                                        <div>
                                            <p class="text-[14px] font-bold text-[#0A0A0A]">Order #{{ order.orderNumber
                                                }}</p>
                                            <p class="text-[12px] text-[#999] mt-0.5">{{ new Date(order.createdAt).toLocaleDateString('en-US', {
                                                    month: 'short',
                                                    day: 'numeric', year: 'numeric'
                                                }) }}</p>
                                        </div>
                                        <div class="flex items-center gap-3">
                                            <span
                                                class="inline-block px-2.5 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-widest bg-[#E8F5E9] text-[#2E7D32]">{{
                                                    order.orderStatus || 'Completed'
                                                }}</span>
                                            <p class="text-[16px] font-bold text-[#0A0A0A]">${{
                                                order.totalAmount?.toFixed(2) }}</p>
                                        </div>
                                    </div>
                                    <div class="flex justify-end gap-6 mt-4 pt-3 border-t border-[#F0F0F0]">
                                        <router-link :to="`/track-order?orderId=${order.orderId}`"
                                            class="inline-flex items-center gap-1.5 text-[13px] font-semibold text-[#C8A97E] no-underline transition-opacity duration-200 hover:opacity-70">
                                            Track Order
                                            <Truck :size="14" />
                                        </router-link>
                                        <router-link :to="`/order-success/${order.orderId}`"
                                            class="inline-flex items-center gap-1.5 text-[13px] font-semibold text-[#C8A97E] no-underline transition-opacity duration-200 hover:opacity-70">
                                            View Receipt
                                            <ArrowRight :size="14" />
                                        </router-link>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </main>
            </div>
        </div>

        <!-- Address Modal -->
        <Teleport to="body">
            <Transition name="fade">
                <div v-if="showAddressModal"
                    class="fixed inset-0 bg-black/50 z-[100] flex items-center justify-center p-4"
                    @click.self="closeAddressModal">
                    <div class="bg-white rounded-[20px] max-w-[480px] w-full shadow-[0_25px_50px_rgba(0,0,0,0.2)]">
                        <div class="flex items-center justify-between px-8 py-6 border-b border-[#F0F0F0]">
                            <h3 class="text-[16px] font-bold text-[#0A0A0A]">{{ editingAddress ? 'Edit Address' : 'Add New Address' }}</h3>
                            <button @click="closeAddressModal"
                                class="bg-transparent border-none p-1.5 rounded-lg cursor-pointer text-[#999] transition-all hover:bg-[#F5F5F5] hover:text-[#333]">
                                <X :size="18" />
                            </button>
                        </div>
                        <div class="px-8 py-6 flex flex-col gap-4">
                            <div class="flex flex-col gap-1.5">
                                <label class="text-[11px] font-semibold uppercase tracking-[0.12em] text-[#666]">Street
                                    Address</label>
                                <input v-model="addressForm.street"
                                    class="w-full px-3.5 py-2.5 border border-[#E0E0E0] rounded-[10px] text-[14px] text-[#0A0A0A] bg-white outline-none transition-all duration-200 focus:border-[#C8A97E] focus:shadow-[0_0_0_3px_rgba(200,169,126,0.12)]"
                                    placeholder="123 Main St" />
                            </div>
                            <div class="grid grid-cols-2 gap-4">
                                <div class="flex flex-col gap-1.5">
                                    <label
                                        class="text-[11px] font-semibold uppercase tracking-[0.12em] text-[#666]">City</label>
                                    <input v-model="addressForm.city"
                                        class="w-full px-3.5 py-2.5 border border-[#E0E0E0] rounded-[10px] text-[14px] text-[#0A0A0A] bg-white outline-none transition-all duration-200 focus:border-[#C8A97E] focus:shadow-[0_0_0_3px_rgba(200,169,126,0.12)]"
                                        placeholder="City" />
                                </div>
                                <div class="flex flex-col gap-1.5">
                                    <label
                                        class="text-[11px] font-semibold uppercase tracking-[0.12em] text-[#666]">State</label>
                                    <input v-model="addressForm.state"
                                        class="w-full px-3.5 py-2.5 border border-[#E0E0E0] rounded-[10px] text-[14px] text-[#0A0A0A] bg-white outline-none transition-all duration-200 focus:border-[#C8A97E] focus:shadow-[0_0_0_3px_rgba(200,169,126,0.12)]"
                                        placeholder="State" />
                                </div>
                            </div>
                            <div class="grid grid-cols-2 gap-4">
                                <div class="flex flex-col gap-1.5">
                                    <label class="text-[11px] font-semibold uppercase tracking-[0.12em] text-[#666]">Zip
                                        Code</label>
                                    <input v-model="addressForm.zipCode"
                                        class="w-full px-3.5 py-2.5 border border-[#E0E0E0] rounded-[10px] text-[14px] text-[#0A0A0A] bg-white outline-none transition-all duration-200 focus:border-[#C8A97E] focus:shadow-[0_0_0_3px_rgba(200,169,126,0.12)]"
                                        placeholder="10001" />
                                </div>
                                <div class="flex flex-col gap-1.5">
                                    <label
                                        class="text-[11px] font-semibold uppercase tracking-[0.12em] text-[#666]">Country</label>
                                    <input v-model="addressForm.country"
                                        class="w-full px-3.5 py-2.5 border border-[#E0E0E0] rounded-[10px] text-[14px] text-[#0A0A0A] bg-white outline-none transition-all duration-200 focus:border-[#C8A97E] focus:shadow-[0_0_0_3px_rgba(200,169,126,0.12)]"
                                        placeholder="Country" />
                                </div>
                            </div>
                            <label class="flex items-center gap-2 text-[13px] text-[#333] cursor-pointer">
                                <input type="checkbox" v-model="addressForm.isDefault"
                                    class="w-4 h-4 accent-[#C8A97E]" />
                                <span>Set as default address</span>
                            </label>
                        </div>
                        <div class="flex justify-end gap-3 px-8 py-5 border-t border-[#F0F0F0]">
                            <button @click="closeAddressModal"
                                class="px-6 py-2.5 bg-transparent text-[#333] border border-[#DDD] rounded-[10px] text-[13px] font-semibold cursor-pointer transition-all hover:border-[#333]">Cancel</button>
                            <button @click="handleSaveAddress"
                                class="px-6 py-2.5 bg-[#C8A97E] text-[#0A0A0A] border-none rounded-[10px] text-[13px] font-semibold cursor-pointer transition-all hover:bg-[#D4B98E] disabled:opacity-50"
                                :disabled="saving">
                                {{ saving ? 'Saving...' : 'Save' }}
                            </button>
                        </div>
                    </div>
                </div>
            </Transition>
        </Teleport>

        <!-- Payment Modal -->
        <Teleport to="body">
            <Transition name="fade">
                <div v-if="showPaymentModal"
                    class="fixed inset-0 bg-black/50 z-[100] flex items-center justify-center p-4"
                    @click.self="closePaymentModal">
                    <div class="bg-white rounded-[20px] max-w-[480px] w-full shadow-[0_25px_50px_rgba(0,0,0,0.2)]">
                        <div class="flex items-center justify-between px-8 py-6 border-b border-[#F0F0F0]">
                            <h3 class="text-[16px] font-bold text-[#0A0A0A]">{{ editingPayment ? 'Edit Card' : 'Add New Card' }}</h3>
                            <button @click="closePaymentModal"
                                class="bg-transparent border-none p-1.5 rounded-lg cursor-pointer text-[#999] transition-all hover:bg-[#F5F5F5] hover:text-[#333]">
                                <X :size="18" />
                            </button>
                        </div>
                        <div class="px-8 py-6 flex flex-col gap-4">
                            <div class="flex flex-col gap-1.5">
                                <label class="text-[11px] font-semibold uppercase tracking-[0.12em] text-[#666]">Card
                                    Number</label>
                                <input v-model="paymentForm.cardNumber"
                                    class="w-full px-3.5 py-2.5 border border-[#E0E0E0] rounded-[10px] text-[14px] text-[#0A0A0A] bg-white outline-none transition-all duration-200 focus:border-[#C8A97E] focus:shadow-[0_0_0_3px_rgba(200,169,126,0.12)]"
                                    placeholder="4242 4242 4242 4242" />
                            </div>
                            <div class="flex flex-col gap-1.5">
                                <label class="text-[11px] font-semibold uppercase tracking-[0.12em] text-[#666]">Card
                                    Holder</label>
                                <input v-model="paymentForm.cardHolder"
                                    class="w-full px-3.5 py-2.5 border border-[#E0E0E0] rounded-[10px] text-[14px] text-[#0A0A0A] bg-white outline-none transition-all duration-200 focus:border-[#C8A97E] focus:shadow-[0_0_0_3px_rgba(200,169,126,0.12)]"
                                    placeholder="John Doe" />
                            </div>
                            <div class="grid grid-cols-2 gap-4">
                                <div class="flex flex-col gap-1.5">
                                    <label
                                        class="text-[11px] font-semibold uppercase tracking-[0.12em] text-[#666]">Month</label>
                                    <input v-model="paymentForm.expiryMonth"
                                        class="w-full px-3.5 py-2.5 border border-[#E0E0E0] rounded-[10px] text-[14px] text-[#0A0A0A] bg-white outline-none transition-all duration-200 focus:border-[#C8A97E] focus:shadow-[0_0_0_3px_rgba(200,169,126,0.12)]"
                                        placeholder="MM" />
                                </div>
                                <div class="flex flex-col gap-1.5">
                                    <label
                                        class="text-[11px] font-semibold uppercase tracking-[0.12em] text-[#666]">Year</label>
                                    <input v-model="paymentForm.expiryYear"
                                        class="w-full px-3.5 py-2.5 border border-[#E0E0E0] rounded-[10px] text-[14px] text-[#0A0A0A] bg-white outline-none transition-all duration-200 focus:border-[#C8A97E] focus:shadow-[0_0_0_3px_rgba(200,169,126,0.12)]"
                                        placeholder="YY" />
                                </div>
                            </div>
                            <label class="flex items-center gap-2 text-[13px] text-[#333] cursor-pointer">
                                <input type="checkbox" v-model="paymentForm.isDefault"
                                    class="w-4 h-4 accent-[#C8A97E]" />
                                <span>Set as default</span>
                            </label>
                        </div>
                        <div class="flex justify-end gap-3 px-8 py-5 border-t border-[#F0F0F0]">
                            <button @click="closePaymentModal"
                                class="px-6 py-2.5 bg-transparent text-[#333] border border-[#DDD] rounded-[10px] text-[13px] font-semibold cursor-pointer transition-all hover:border-[#333]">Cancel</button>
                            <button @click="handleSavePayment"
                                class="px-6 py-2.5 bg-[#C8A97E] text-[#0A0A0A] border-none rounded-[10px] text-[13px] font-semibold cursor-pointer transition-all hover:bg-[#D4B98E] disabled:opacity-50"
                                :disabled="saving">
                                {{ saving ? 'Saving...' : 'Save' }}
                            </button>
                        </div>
                    </div>
                </div>
            </Transition>
        </Teleport>
    </div>
</template>

<style scoped>
/* Vue Transitions */
.slide-fade-enter-active {
    transition: all 0.3s ease-out;
}

.slide-fade-leave-active {
    transition: all 0.2s ease-in;
}

.slide-fade-enter-from,
.slide-fade-leave-to {
    transform: translateY(-10px);
    opacity: 0;
}

.fade-enter-active {
    transition: all 0.3s ease;
}

.fade-leave-active {
    transition: all 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
    opacity: 0;
}
</style>
