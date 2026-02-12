<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useAuthStore } from '../../stores/Auth'
import { profileApi } from '../../api/profileApi'
import {
    User, Mail, Phone, Calendar, Edit3, Camera, Loader2,
    CheckCircle, AlertCircle, X, Save, Shield, Heart,
    Package, MapPin
} from 'lucide-vue-next'

const authStore = useAuthStore()
const loading = ref(false)
const saving = ref(false)
const uploadingAvatar = ref(false)
const error = ref(null)
const success = ref(null)
const profile = ref(null)
const isEditing = ref(false)
const editForm = ref({})
const avatarInput = ref(null)
const avatarPreview = ref(null)
const isLoaded = ref(false)

const fetchProfile = async () => {
    try {
        loading.value = true
        error.value = null
        const response = await profileApi.getProfile()
        profile.value = response.data.data || response.data
    } catch (err) {
        error.value = err.response?.data?.message || 'Failed to load profile'
        profile.value = authStore.user
    } finally {
        loading.value = false
    }
}

const startEditing = () => {
    isEditing.value = true
    editForm.value = {
        name: profile.value?.name || '',
        email: profile.value?.email || '',
        phone: profile.value?.phone || ''
    }
}

const cancelEditing = () => {
    isEditing.value = false
    editForm.value = {}
}

const saveProfile = async () => {
    try {
        saving.value = true
        error.value = null
        const response = await profileApi.updateProfile(editForm.value)
        profile.value = response.data.data || response.data
        isEditing.value = false
        showSuccess('Profile updated successfully')
    } catch (err) {
        error.value = err.response?.data?.message || 'Failed to update profile'
    } finally {
        saving.value = false
    }
}

const triggerAvatarUpload = () => {
    avatarInput.value?.click()
}

const handleAvatarChange = async (event) => {
    const file = event.target.files[0]
    if (!file) return

    // Validate
    const maxSize = 5 * 1024 * 1024
    if (file.size > maxSize) {
        error.value = 'File too large. Maximum 5MB.'
        return
    }

    const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp']
    if (!allowedTypes.includes(file.type)) {
        error.value = 'Invalid file type. Allowed: JPG, PNG, GIF, WebP'
        return
    }

    // Preview
    const reader = new FileReader()
    reader.onload = (e) => {
        avatarPreview.value = e.target.result
    }
    reader.readAsDataURL(file)

    // Upload
    try {
        uploadingAvatar.value = true
        error.value = null
        const response = await profileApi.uploadAvatar(file)
        const avatarUrl = response.data?.data?.avatar
        if (avatarUrl && profile.value) {
            profile.value.avatar = avatarUrl
        }
        avatarPreview.value = null
        showSuccess('Avatar updated successfully')
    } catch (err) {
        error.value = err.response?.data?.message || 'Failed to upload avatar'
        avatarPreview.value = null
    } finally {
        uploadingAvatar.value = false
        // Reset the file input
        if (avatarInput.value) avatarInput.value.value = ''
    }
}

const showSuccess = (msg) => {
    success.value = msg
    setTimeout(() => { success.value = null }, 3000)
}

const avatarSrc = computed(() => {
    if (avatarPreview.value) return avatarPreview.value
    if (profile.value?.avatar) return profile.value.avatar
    return null
})

const initials = computed(() => {
    const name = profile.value?.name || profile.value?.username || 'U'
    return name.charAt(0).toUpperCase()
})

const memberSince = computed(() => {
    if (!profile.value?.createdAt) return 'N/A'
    return new Date(profile.value.createdAt).toLocaleDateString('en-US', {
        year: 'numeric', month: 'long', day: 'numeric'
    })
})

onMounted(async () => {
    await fetchProfile()
    setTimeout(() => { isLoaded.value = true }, 100)
})
</script>

<template>
    <div class="min-h-screen bg-[#F5F5F5] pb-16">
        <!-- Hero Banner -->
        <div class="relative h-[280px] bg-[#0A0A0A] flex items-end overflow-hidden">
            <div
                class="absolute inset-0 bg-[linear-gradient(135deg,rgba(200,169,126,0.08)_0%,transparent_50%),linear-gradient(to_bottom,transparent_40%,rgba(0,0,0,0.6))]">
            </div>
            <div class="relative max-w-[900px] w-full mx-auto px-8 pb-12">
                <p class="text-[11px] font-medium tracking-[0.4em] uppercase text-[#C8A97E] mb-3 transition-all duration-800 ease-out delay-200"
                    :class="isLoaded ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-3'">— My Account</p>
                <h1 class="text-[clamp(2.5rem,5vw,3.5rem)] font-light text-white tracking-tight leading-[1.1] transition-all duration-800 ease-out delay-400"
                    :class="isLoaded ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'">Profile</h1>
            </div>
        </div>

        <div class="max-w-[900px] mx-auto -mt-8 px-6 relative z-10">
            <!-- Notifications -->
            <Transition name="slide-fade">
                <div v-if="error"
                    class="flex items-center gap-3 px-5 py-4 rounded-xl mb-5 text-[14px] font-medium bg-[#FFF1F0] text-[#E53935] border border-[#FFCDD2]">
                    <AlertCircle class="w-[18px] h-[18px] shrink-0" />
                    <span>{{ error }}</span>
                    <button @click="error = null"
                        class="ml-auto bg-transparent border-none cursor-pointer opacity-60 transition-opacity hover:opacity-100 text-inherit">
                        <X :size="16" />
                    </button>
                </div>
            </Transition>
            <Transition name="slide-fade">
                <div v-if="success"
                    class="flex items-center gap-3 px-5 py-4 rounded-xl mb-5 text-[14px] font-medium bg-[#F1F8E9] text-[#43A047] border border-[#C8E6C9]">
                    <CheckCircle class="w-[18px] h-[18px] shrink-0" />
                    <span>{{ success }}</span>
                </div>
            </Transition>

            <!-- Loading -->
            <div v-if="loading" class="flex flex-col items-center gap-4 py-24 px-8 text-[#666] text-[14px]">
                <Loader2 class="w-7 h-7 animate-spin" />
                <p>Loading profile...</p>
            </div>

            <!-- Profile Content -->
            <div v-else-if="profile" class="flex flex-col gap-5 transition-all duration-600 ease-out delay-300"
                :class="isLoaded ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-5'">

                <!-- Avatar + Name Card -->
                <div
                    class="bg-white border border-[#E8E8E8] rounded-2xl p-8 flex items-center justify-between gap-6 flex-wrap">
                    <div class="flex items-center gap-6">
                        <div class="cursor-pointer shrink-0" @click="triggerAvatarUpload">
                            <div class="relative w-24 h-24 rounded-full overflow-hidden border-[3px] border-[#C8A97E] transition-all duration-300 ease-out hover:scale-105 hover:shadow-[0_0_0_4px_rgba(200,169,126,0.2)]"
                                :class="{ 'opacity-70': uploadingAvatar }">
                                <img v-if="avatarSrc" :src="avatarSrc" :alt="profile.name"
                                    class="w-full h-full object-cover" />
                                <div v-else
                                    class="w-full h-full bg-[linear-gradient(135deg,#0A0A0A_0%,#1A1A1A_100%)] flex items-center justify-center">
                                    <span class="text-[2rem] font-semibold text-[#C8A97E]">{{ initials }}</span>
                                </div>
                                <div
                                    class="absolute inset-0 bg-black/50 flex items-center justify-center text-white opacity-0 transition-opacity duration-300 hover:opacity-100">
                                    <Loader2 v-if="uploadingAvatar" :size="24" class="animate-spin" />
                                    <Camera v-else :size="24" />
                                </div>
                            </div>
                            <input ref="avatarInput" type="file" accept="image/*" class="hidden"
                                @change="handleAvatarChange" />
                        </div>
                        <div class="flex flex-col gap-1">
                            <h2 class="text-[1.5rem] font-semibold text-[#0A0A0A] tracking-tight leading-[1.2]">{{
                                profile.name || profile.username || 'User' }}</h2>
                            <p class="text-[14px] text-[#666]">{{ profile.email || 'No email' }}</p>
                            <div
                                class="inline-flex items-center gap-1.5 mt-2 px-3 py-1 bg-[#0A0A0A] text-[#C8A97E] rounded-full text-[11px] font-semibold uppercase tracking-widest w-fit">
                                <Shield :size="12" />
                                <span>{{ profile.role || 'Customer' }}</span>
                            </div>
                        </div>
                    </div>
                    <button v-if="!isEditing" @click="startEditing"
                        class="inline-flex items-center gap-2 px-6 py-2.5 bg-[#0A0A0A] text-white border-none rounded-[10px] text-[13px] font-semibold tracking-wide cursor-pointer transition-all duration-300 hover:bg-[#C8A97E] hover:text-[#0A0A0A]">
                        <Edit3 :size="16" />
                        <span>Edit Profile</span>
                    </button>
                </div>

                <!-- Edit Form -->
                <Transition name="expand">
                    <div v-if="isEditing" class="bg-white border border-[#E8E8E8] rounded-2xl p-8 overflow-hidden">
                        <h3 class="text-[13px] font-semibold uppercase tracking-[0.15em] text-[#333] mb-6">Edit Profile
                        </h3>
                        <form @submit.prevent="saveProfile" class="flex flex-col gap-5">
                            <div class="flex flex-col gap-2">
                                <label class="text-[11px] font-semibold uppercase tracking-[0.15em] text-[#666]">Full
                                    Name</label>
                                <div class="relative flex items-center">
                                    <User :size="16" class="absolute left-3.5 text-[#999] pointer-events-none" />
                                    <input v-model="editForm.name" type="text"
                                        class="w-full pl-[42px] pr-3.5 py-3 border border-[#E0E0E0] rounded-[10px] text-[14px] text-[#0A0A0A] bg-white transition-all duration-200 outline-none focus:border-[#C8A97E] focus:shadow-[0_0_0_3px_rgba(200,169,126,0.15)] placeholder:text-[#999]"
                                        placeholder="Your full name" />
                                </div>
                            </div>
                            <div class="flex flex-col gap-2">
                                <label class="text-[11px] font-semibold uppercase tracking-[0.15em] text-[#666]">Email
                                    Address</label>
                                <div class="relative flex items-center">
                                    <Mail :size="16" class="absolute left-3.5 text-[#999] pointer-events-none" />
                                    <input v-model="editForm.email" type="email"
                                        class="w-full pl-[42px] pr-3.5 py-3 border border-[#E0E0E0] rounded-[10px] text-[14px] text-[#0A0A0A] bg-white transition-all duration-200 outline-none focus:border-[#C8A97E] focus:shadow-[0_0_0_3px_rgba(200,169,126,0.15)] placeholder:text-[#999]"
                                        placeholder="your@email.com" />
                                </div>
                            </div>
                            <div class="flex flex-col gap-2">
                                <label class="text-[11px] font-semibold uppercase tracking-[0.15em] text-[#666]">Phone
                                    Number</label>
                                <div class="relative flex items-center">
                                    <Phone :size="16" class="absolute left-3.5 text-[#999] pointer-events-none" />
                                    <input v-model="editForm.phone" type="tel"
                                        class="w-full pl-[42px] pr-3.5 py-3 border border-[#E0E0E0] rounded-[10px] text-[14px] text-[#0A0A0A] bg-white transition-all duration-200 outline-none focus:border-[#C8A97E] focus:shadow-[0_0_0_3px_rgba(200,169,126,0.15)] placeholder:text-[#999]"
                                        placeholder="+1 (555) 000-0000" />
                                </div>
                            </div>
                            <div class="flex justify-end gap-3 mt-2">
                                <button type="button" @click="cancelEditing"
                                    class="px-6 py-2.5 bg-transparent text-[#666] border border-[#E0E0E0] rounded-[10px] text-[13px] font-medium cursor-pointer transition-all hover:border-[#333] hover:text-[#333]">Cancel</button>
                                <button type="submit" :disabled="saving"
                                    class="inline-flex items-center gap-2 px-7 py-2.5 bg-[#C8A97E] text-[#0A0A0A] border-none rounded-[10px] text-[13px] font-semibold tracking-wide cursor-pointer transition-all hover:bg-[#D4B98E] disabled:opacity-60 disabled:cursor-not-allowed">
                                    <Loader2 v-if="saving" :size="16" class="animate-spin" />
                                    <Save v-else :size="16" />
                                    <span>{{ saving ? 'Saving...' : 'Save Changes' }}</span>
                                </button>
                            </div>
                        </form>
                    </div>
                </Transition>

                <!-- Info Grid -->
                <div class="grid grid-cols-[repeat(auto-fit,minmax(260px,1fr))] gap-4">
                    <div
                        class="flex items-center gap-4 p-5 bg-white border border-[#E8E8E8] rounded-[14px] transition-all duration-300 hover:border-[#C8A97E] hover:-translate-y-0.5">
                        <div class="w-11 h-11 rounded-xl bg-[#F5F5F5] flex items-center justify-center text-[#999]">
                            <User :size="20" />
                        </div>
                        <div class="flex flex-col gap-0.5">
                            <span class="text-[10px] font-bold uppercase tracking-widest text-[#999]">Full Name</span>
                            <span class="text-[14px] font-semibold text-[#0A0A0A] break-all">
                                {{ profile.name || profile.username || 'Not set' }}
                            </span>
                        </div>
                    </div>

                    <div
                        class="flex items-center gap-4 p-5 bg-white border border-[#E8E8E8] rounded-[14px] transition-all duration-300 hover:border-[#C8A97E] hover:-translate-y-0.5">
                        <div
                            class="w-11 h-11 rounded-xl bg-[#C8A97E]/10 flex items-center justify-center text-[#C8A97E]">
                            <Mail :size="20" />
                        </div>
                        <div class="flex flex-col gap-0.5">
                            <span class="text-[10px] font-bold uppercase tracking-widest text-[#999]">Email
                                Address</span>
                            <span class="text-[14px] font-semibold text-[#0A0A0A] break-all">{{ profile.email || 'Not set' }}</span>
                        </div>
                    </div>

                    <div class="flex items-center gap-4 p-5 bg-white border border-[#E8E8E8] rounded-[14px] transition-all duration-300 hover:border-[#C8A97E] hover:-translate-y-0.5"
                        v-if="profile.phone">
                        <div class="w-11 h-11 rounded-xl bg-[#0A0A0A] flex items-center justify-center text-white">
                            <Phone :size="20" />
                        </div>
                        <div class="flex flex-col gap-0.5">
                            <span class="text-[10px] font-bold uppercase tracking-widest text-[#999]">Phone
                                Number</span>
                            <span class="text-[14px] font-semibold text-[#0A0A0A]">{{ profile.phone }}</span>
                        </div>
                    </div>

                    <div
                        class="flex items-center gap-4 p-5 bg-white border border-[#E8E8E8] rounded-[14px] transition-all duration-300 hover:border-[#C8A97E] hover:-translate-y-0.5">
                        <div class="w-11 h-11 rounded-xl bg-[#999]/10 flex items-center justify-center text-[#666]">
                            <Calendar :size="20" />
                        </div>
                        <div class="flex flex-col gap-0.5">
                            <span class="text-[10px] font-bold uppercase tracking-widest text-[#999]">Member
                                Since</span>
                            <span class="text-[14px] font-semibold text-[#0A0A0A]">{{ memberSince }}</span>
                        </div>
                    </div>
                </div>

                <!-- Quick Links -->
                <div class="grid grid-cols-[repeat(auto-fit,minmax(180px,1fr))] gap-4">
                    <router-link to="/wishlist"
                        class="flex items-center justify-center flex-col gap-3 p-6 bg-white border border-[#E8E8E8] rounded-2xl text-[#0A0A0A] no-underline transition-all duration-300 hover:border-[#C8A97E] hover:-translate-y-1 hover:shadow-[0_10px_30px_rgba(0,0,0,0.05)] group">
                        <Heart :size="20" class="text-[#999] transition-colors group-hover:text-[#C8A97E]" />
                        <span class="text-[13px] font-semibold">My Wishlist</span>
                    </router-link>
                    <router-link to="/orders"
                        class="flex items-center justify-center flex-col gap-3 p-6 bg-white border border-[#E8E8E8] rounded-2xl text-[#0A0A0A] no-underline transition-all duration-300 hover:border-[#C8A97E] hover:-translate-y-1 hover:shadow-[0_10px_30px_rgba(0,0,0,0.05)] group">
                        <Package :size="20" class="text-[#999] transition-colors group-hover:text-[#C8A97E]" />
                        <span class="text-[13px] font-semibold">My Orders</span>
                    </router-link>
                    <router-link to="/settings"
                        class="flex items-center justify-center flex-col gap-3 p-6 bg-white border border-[#E8E8E8] rounded-2xl text-[#0A0A0A] no-underline transition-all duration-300 hover:border-[#C8A97E] hover:-translate-y-1 hover:shadow-[0_10px_30px_rgba(0,0,0,0.05)] group">
                        <MapPin :size="20" class="text-[#999] transition-colors group-hover:text-[#C8A97E]" />
                        <span class="text-[13px] font-semibold">Addresses</span>
                    </router-link>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
/* Vue Transitions */
.slide-fade-enter-active,
.slide-fade-leave-active {
    transition: all 0.3s ease;
}

.slide-fade-enter-from,
.slide-fade-leave-to {
    transform: translateY(-10px);
    opacity: 0;
}

.expand-enter-active,
.expand-leave-active {
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    max-height: 500px;
    overflow: hidden;
}

.expand-enter-from,
.expand-leave-to {
    max-height: 0;
    opacity: 0;
    margin-top: 0;
    padding-top: 0;
    padding-bottom: 0;
}
</style>
