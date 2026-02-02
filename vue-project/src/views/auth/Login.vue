<script setup>
import { ref } from 'vue';
import { useAuthStore } from '../../stores/Auth';
import { useRouter } from 'vue-router';
import { RouterLink } from 'vue-router';
import {
  Mail,
  Lock,
  Eye,
  EyeOff,
  ArrowRight,
  Loader2,
  AlertCircle,
  Sparkles
} from 'lucide-vue-next';

const router = useRouter();

const emailOrUsername = ref('');
const password = ref('');
const rememberMe = ref(false);
const showPassword = ref(false);
const error = ref('');
const loading = ref(false);

const handleLogin = async () => {
  error.value = '';

  if (!emailOrUsername.value || !password.value) {
    error.value = 'Please fill in all fields';
    return;
  }

  loading.value = true;

  try {
    const authStore = useAuthStore();
    const response = await authStore.login({
      emailOrUsername: emailOrUsername.value,
      password: password.value,
      remember: rememberMe.value
    });

    // Token is handled by store
    
    // Redirect based on user role
    const user = response.data?.user;
    const userRole = user?.roleName || user?.role || user?.roleId;
    // Normalize logic
    const normalizedRole = String(userRole).toLowerCase();

    if (normalizedRole === 'admin' || userRole === 1) {
      router.push('/admin/dashboard');
    } else {
      router.push('/');
    }

  } catch (err) {
    error.value = err.response?.data?.message || err.response?.data?.error || err.message || 'Login failed. Please try again.';
    console.error('Login error:', err);
  } finally {
    loading.value = false;
  }
};
</script>

<template>
  <div class="min-h-screen bg-white flex items-center justify-center px-4 py-24">
    <div class="w-full max-w-md">
      <!-- Header -->
      <div class="text-center mb-12">
        <div
          class="inline-flex items-center gap-2 px-3 py-1 bg-gray-100 rounded-full text-gray-600 text-xs font-bold tracking-wider uppercase mb-6">
          <Sparkles class="w-3 h-3" />
          Welcome Back
        </div>
        <h1 class="text-4xl md:text-5xl font-light text-gray-900 tracking-tight mb-4">
          Sign in to your <span class="font-bold italic">Account</span>
        </h1>
        <p class="text-gray-500 text-lg font-light">
          Elevate your experience with AlieeShop
        </p>
      </div>

      <!-- Card -->
      <div class="bg-gray-50/50 rounded-[3rem] p-12 md:p-16 shadow-sm border border-gray-100">
        <!-- Error -->
        <div v-if="error" class="mb-6 flex items-center gap-3 rounded-2xl bg-red-50 border border-red-200 px-6 py-4">
          <AlertCircle class="h-5 w-5 shrink-0 text-red-600" />
          <span class="text-sm font-semibold text-red-800">{{ error }}</span>
        </div>

        <form @submit.prevent="handleLogin" class="space-y-6">
          <!-- Email/Username -->
          <div class="space-y-2">
            <label class="text-xs font-bold uppercase tracking-widest text-gray-400 ml-4">
              Email or Username
            </label>
            <div class="relative">
              <Mail class="absolute left-6 top-1/2 -translate-y-1/2 h-5 w-5 text-gray-400" />
              <input v-model="emailOrUsername" type="text" placeholder="Enter your email or username" required
                class="w-full bg-white border-0 rounded-2xl pl-14 pr-6 py-4 text-gray-900 placeholder-gray-400 focus:ring-2 focus:ring-black transition-all shadow-sm outline-none" />
            </div>
          </div>

          <!-- Password -->
          <div class="space-y-2">
            <div class="flex items-center justify-between">
              <label class="text-xs font-bold uppercase tracking-widest text-gray-400 ml-4">
                Password
              </label>
              <RouterLink to="/forgot-password"
                class="text-xs font-bold text-gray-500 hover:text-gray-900 transition-colors">
                Forgot Password?
              </RouterLink>
            </div>
            <div class="relative">
              <Lock class="absolute left-6 top-1/2 -translate-y-1/2 h-5 w-5 text-gray-400" />
              <input v-model="password" :type="showPassword ? 'text' : 'password'" placeholder="Enter your password"
                required
                class="w-full bg-white border-0 rounded-2xl pl-14 pr-14 py-4 text-gray-900 placeholder-gray-400 focus:ring-2 focus:ring-black transition-all shadow-sm outline-none" />
              <button type="button" @click="showPassword = !showPassword"
                class="absolute right-6 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 transition-colors">
                <component :is="showPassword ? EyeOff : Eye" class="h-5 w-5" />
              </button>
            </div>
          </div>

          <!-- Remember Me -->
          <div class="flex items-center gap-3 px-1">
            <label class="relative flex items-center cursor-pointer group">
              <input v-model="rememberMe" type="checkbox" class="peer sr-only" />
              <div
                class="w-5 h-5 border-2 border-gray-300 rounded-lg group-hover:border-gray-900 peer-checked:bg-gray-900 peer-checked:border-gray-900 transition-all duration-200">
              </div>
              <div
                class="absolute inset-0 flex items-center justify-center text-white scale-0 peer-checked:scale-100 transition-transform duration-200">
                <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M5 13l4 4L19 7"></path>
                </svg>
              </div>
            </label>
            <span class="text-sm font-bold text-gray-600">
              Remember Me
            </span>
          </div>

          <!-- Submit Button -->
          <button type="submit" :disabled="loading"
            class="w-full bg-gray-900 text-white font-bold py-5 rounded-2xl hover:bg-black transition-all duration-300 flex items-center justify-center gap-3 group shadow-xl disabled:opacity-50 disabled:cursor-not-allowed">
            <Loader2 v-if="loading" class="h-5 w-5 animate-spin" />
            <span v-else class="flex items-center gap-3">
              Sign In
              <ArrowRight class="w-5 h-5 group-hover:translate-x-1 transition-transform" />
            </span>
          </button>
        </form>
      </div>

      <!-- Footer -->
      <p class="mt-8 text-center text-sm font-bold text-gray-500">
        New to the platform?
        <RouterLink to="/register" class="font-bold text-gray-900 hover:text-violet-600 transition-colors ml-1">
          Create an Account
        </RouterLink>
      </p>
    </div>
  </div>
</template>
