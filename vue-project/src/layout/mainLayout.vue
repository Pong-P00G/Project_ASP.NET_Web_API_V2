<script setup>
import { ref, onMounted, onUnmounted } from 'vue';
import Navbar from '../components/Navbar.vue';
import Footer from '../components/Footer.vue';

// Scroll to top on route change
const scrollToTop = () => {
    window.scrollTo({ top: 0, behavior: 'smooth' });
};

// Track scroll position for effects
const scrollY = ref(0);

const handleScroll = () => {
    scrollY.value = window.scrollY;
};

onMounted(() => {
    window.addEventListener('scroll', handleScroll);
});

onUnmounted(() => {
    window.removeEventListener('scroll', handleScroll);
});
</script>

<template>
    <div class="flex flex-col min-h-screen w-full bg-gray-50">
        <!-- Navbar -->
        <Navbar />
        <!-- Main Content -->
        <main class="flex-1 w-full mt-16" role="main">
            <router-view v-slot="{ Component }">
                <transition name="fade" mode="out-in">
                    <component :is="Component" />
                </transition>
            </router-view>
        </main>
        <!-- Footer -->
        <Footer />
        <!-- Scroll to Top Button (Optional) -->
        <transition name="fade">
            <button v-if="scrollY > 300" @click="scrollToTop"
                class="fixed bottom-8 right-8 z-40 w-10 h-10 bg-black text-white rounded-full shadow-2xl hover:shadow-violet-600/50 hover:scale-110 transition-all duration-300 flex items-center justify-center group"
                aria-label="Scroll to top">
                <svg class="w-6 h-6 group-hover:-translate-y-1 transition-transform" fill="none" stroke="currentColor"
                    viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                        d="M5 10l7-7m0 0l7 7m-7-7v18" />
                </svg>
            </button>
        </transition>
    </div>
</template>

<style scoped>
/* Page Transition */
.fade-enter-active,
.fade-leave-active {
    transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
    opacity: 0;
}

/* Smooth scrolling */
html {
    scroll-behavior: smooth;
}

/* Prevent horizontal scroll */
body {
    overflow-x: hidden;
}
</style>