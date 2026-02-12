<script setup>
import { ref, onMounted } from 'vue';
import { RouterLink } from 'vue-router';
import { ArrowRight, Play } from 'lucide-vue-next';

const isLoaded = ref(false);

onMounted(() => {
    setTimeout(() => {
        isLoaded.value = true;
    }, 100);
});
</script>

<template>
    <section class="relative h-screen w-full overflow-hidden bg-black">
        <!-- Background Image -->
        <div class="absolute inset-0">
            <img src="https://images.unsplash.com/photo-1441984904996-e0b6ba687e04?q=80&w=2070&auto=format&fit=crop"
                alt="Fashion Collection"
                class="w-full h-full object-cover opacity-60 scale-105 transition-transform duration-[2000ms]"
                :class="isLoaded ? 'scale-100' : 'scale-110'" />
            <!-- Gradient Overlays -->
            <div class="absolute inset-0 bg-gradient-to-t from-black via-black/40 to-black/20"></div>
            <div class="absolute inset-0 bg-gradient-to-r from-black/60 via-transparent to-transparent"></div>
        </div>

        <!-- Content -->
        <div
            class="relative h-full max-w-screen-xl mx-auto px-6 sm:px-8 lg:px-12 flex flex-col justify-end pb-24 md:pb-32">

            <!-- Subtitle -->
            <p class="text-[11px] font-medium tracking-[0.4em] uppercase mb-6 transition-all duration-1000 delay-300"
                :class="isLoaded ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-6'" style="color: #C8A97E;">
                — New Collection 2026
            </p>

            <!-- Main Heading -->
            <h1 class="text-5xl md:text-7xl lg:text-8xl font-light text-white leading-[1.05] tracking-[-0.03em] mb-6 max-w-3xl transition-all duration-1000 delay-500"
                :class="isLoaded ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'">
                Elevate Your<br />
                <span class="italic font-normal" style="color: #C8A97E;">Everyday</span> Style
            </h1>

            <!-- Description -->
            <p class="text-white/50 text-base md:text-lg max-w-xl mb-10 leading-relaxed font-light tracking-wide transition-all duration-1000 delay-700"
                :class="isLoaded ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-6'">
                Discover thoughtfully designed pieces that blend timeless elegance with modern sophistication.
            </p>

            <!-- CTA Buttons -->
            <div class="flex flex-wrap items-center gap-4 transition-all duration-1000 delay-900"
                :class="isLoaded ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-6'">
                <RouterLink to="/product"
                    class="group inline-flex items-center gap-3 px-8 py-4 text-sm font-medium tracking-[0.15em] uppercase text-black hover:opacity-90 transition-all duration-500"
                    style="background-color: #C8A97E;">
                    Shop Collection
                    <ArrowRight class="w-4 h-4 group-hover:translate-x-1 transition-transform duration-300" />
                </RouterLink>
                <RouterLink to="/about"
                    class="inline-flex items-center gap-3 px-8 py-4 border text-sm font-medium tracking-[0.15em] uppercase text-white hover:bg-white hover:text-black transition-all duration-500"
                    style="border-color: rgba(255,255,255,0.25);">
                    Our Story
                </RouterLink>
            </div>

            <!-- Bottom Bar -->
            <div
                class="absolute bottom-0 left-0 right-0 h-px bg-gradient-to-r from-transparent via-white/10 to-transparent">
            </div>
        </div>

        <!-- Scroll Indicator -->
        <div class="absolute bottom-8 left-1/2 -translate-x-1/2 flex flex-col items-center gap-2 transition-all duration-1000 delay-[1200ms]"
            :class="isLoaded ? 'opacity-100' : 'opacity-0'">
            <span class="text-[9px] tracking-[0.3em] uppercase text-white/30 font-medium">Scroll</span>
            <div class="w-px h-8 bg-gradient-to-b from-white/30 to-transparent scroll-line"></div>
        </div>
    </section>
</template>

<style scoped>
.scroll-line {
    animation: scrollPulse 2s ease-in-out infinite;
}

@keyframes scrollPulse {

    0%,
    100% {
        opacity: 0.3;
        transform: scaleY(1);
    }

    50% {
        opacity: 0.8;
        transform: scaleY(1.3);
    }
}
</style>
