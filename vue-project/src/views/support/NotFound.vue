<script setup>
import { RouterLink } from 'vue-router';
import { Home, Search, ArrowLeft } from 'lucide-vue-next';
</script>

<template>
    <div class="min-h-screen bg-linear-to-b from-gray-50 to-white flex items-center justify-center px-4">
        <div class="text-center max-w-2xl mx-auto">
            <!-- 404 Animation -->
            <div class="mb-8 relative">
                <div class="text-[12rem] md:text-[16rem] font-black text-gray-200 leading-none">
                    404
                </div>
                <div class="absolute inset-0 flex items-center justify-center">
                    <div
                        class="w-32 h-32 bg-linear-to-br from-violet-600 to-purple-600 rounded-full flex items-center justify-center shadow-2xl animate-bounce">
                        <Search class="w-16 h-16 text-white" />
                    </div>
                </div>
            </div>
            <!-- Message -->
            <h1 class="text-4xl md:text-5xl font-black text-gray-900 mb-4">
                Oops! Page <span
                    class="bg-linear-to-r from-violet-600 to-purple-600 bg-clip-text text-transparent">Not
                    Found</span>
            </h1>
            <p class="text-xl text-gray-600 mb-8 leading-relaxed">
                The page you're looking for seems to have wandered off.
                Don't worry, even our best products get lost sometimes!
            </p>
            <!-- Action Buttons -->
            <div class="flex flex-col sm:flex-row gap-4 justify-center items-center">
                <RouterLink to="/"
                    class="inline-flex items-center gap-2 px-8 py-4 bg-linear-to-r from-violet-600 to-purple-600 text-white font-bold rounded-xl hover:shadow-2xl hover:shadow-violet-600/50 hover:scale-105 transition-all duration-300 group">
                    <Home class="w-5 h-5" />
                    Back to Home
                </RouterLink>
                <RouterLink to="/product"
                    class="inline-flex items-center gap-2 px-8 py-4 bg-white text-gray-900 font-bold rounded-xl border-2 border-gray-200 hover:border-violet-600 hover:text-violet-600 transition-all duration-300 group">
                    <Search class="w-5 h-5" />
                    Browse Products
                </RouterLink>
            </div>
            <!-- Helpful Links -->
            <div class="mt-12 pt-8 border-t border-gray-200">
                <p class="text-sm text-gray-500 mb-4">You might be interested in:</p>
                <div class="flex flex-wrap gap-3 justify-center">
                    <RouterLink to="/"
                        class="text-sm text-violet-600 hover:text-violet-700 font-medium hover:underline">
                        Home
                    </RouterLink>
                    <span class="text-gray-300">•</span>
                    <RouterLink to="/product"
                        class="text-sm text-violet-600 hover:text-violet-700 font-medium hover:underline">
                        Products
                    </RouterLink>
                    <span class="text-gray-300">•</span>
                    <RouterLink to="/about"
                        class="text-sm text-violet-600 hover:text-violet-700 font-medium hover:underline">
                        About Us
                    </RouterLink>
                    <span class="text-gray-300">•</span>
                    <RouterLink to="/contact"
                        class="text-sm text-violet-600 hover:text-violet-700 font-medium hover:underline">
                        Contact
                    </RouterLink>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
@keyframes bounce {

    0%,
    100% {
        transform: translateY(0);
    }

    50% {
        transform: translateY(-20px);
    }
}

.animate-bounce {
    animation: bounce 2s ease-in-out infinite;
}
</style>