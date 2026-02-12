<script setup>
import { RouterLink } from 'vue-router';
import HeroSection from '../components/HeroSection.vue';
import ProductCarousel from '../components/ProductCarousel.vue';
import BlogSection from '../components/BlogSection.vue';
import { ArrowRight, Truck, Headphones, ShieldCheck, RotateCcw } from 'lucide-vue-next';

// Brands
const brands = [
    'VOGUE', 'Elle', 'HYPEBEAST', 'GQ', "HARPER'S", 'DAZED', 'i-D', 'NYLON'
];

// Categories
const categories = [
    {
        name: 'Women',
        subtitle: 'Spring Collection',
        image: 'https://images.unsplash.com/photo-1596821639943-718bfcb3088b?q=80&w=1974&auto=format&fit=crop',
        link: '/product?category=women'
    },
    {
        name: 'Men',
        subtitle: 'Essential Wardrobe',
        image: 'https://images.unsplash.com/photo-1550246140-29f40b909e5a?q=80&w=1964&auto=format&fit=crop',
        link: '/product?category=men'
    },
    {
        name: 'Accessories',
        subtitle: 'Refined Details',
        image: 'https://images.unsplash.com/photo-1541810575775-4c000d6a2a0a?q=80&w=1974&auto=format&fit=crop',
        link: '/product?category=accessories'
    },
    {
        name: 'New Arrivals',
        subtitle: 'Just Landed',
        image: 'https://images.unsplash.com/photo-1483985988355-763728e1935b?q=80&w=2070&auto=format&fit=crop',
        link: '/product?filter=new'
    }
];

// Features
const features = [
    { icon: Truck, title: 'Free Shipping', description: 'On orders over $50' },
    { icon: Headphones, title: '24/7 Support', description: 'Always here for you' },
    { icon: ShieldCheck, title: 'Secure Checkout', description: '100% encrypted' },
    { icon: RotateCcw, title: 'Easy Returns', description: '30-day guarantee' }
];
</script>

<template>
    <div class="bg-white">
        <!-- Hero Section -->
        <HeroSection />
        <!-- Brands Marquee -->
        <section class="py-12 border-b border-black/5 overflow-hidden bg-white">
            <div class="marquee-container">
                <div class="marquee-track">
                    <template v-for="n in 3">
                        <span v-for="brand in brands" :key="brand + n"
                            class="text-lg md:text-xl font-light tracking-[0.15em] uppercase text-black/12 mx-10 md:mx-16 whitespace-nowrap select-none">
                            {{ brand }}
                        </span>
                    </template>
                </div>
            </div>
        </section>
        <!-- Trending Now -->
        <section class="py-24 md:py-32 bg-white">
            <div class="max-w-screen-2xl mx-auto px-6 sm:px-8 lg:px-12">
                <div class="flex justify-between items-end mb-16">
                    <div>
                        <p class="text-[10px] font-medium tracking-[0.35em] uppercase text-black/30 mb-4">— Curated
                            Selection
                        </p>
                        <h2 class="text-3xl md:text-4xl font-light text-black tracking-[-0.01em]">Trending Now</h2>
                    </div>
                    <RouterLink to="/product"
                        class="hidden md:flex items-center gap-2 text-xs font-medium tracking-[0.15em] uppercase transition-colors duration-500 group"
                        style="color: #C8A97E;">
                        View All
                        <ArrowRight class="w-3.5 h-3.5 group-hover:translate-x-1 transition-transform duration-300" />
                    </RouterLink>
                </div>
                <ProductCarousel />
            </div>
        </section>

        <!-- Shop by Category -->
        <section class="py-24 md:py-32 bg-neutral-50">
            <div class="max-w-screen-2xl mx-auto px-6 sm:px-8 lg:px-12">
                <div class="text-center mb-16">
                    <p class="text-[10px] font-medium tracking-[0.35em] uppercase text-black/30 mb-4">— Collections</p>
                    <h2 class="text-3xl md:text-4xl font-light text-black tracking-[-0.01em]">Shop by Category</h2>
                </div>

                <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
                    <RouterLink v-for="category in categories" :key="category.name" :to="category.link"
                        class="group relative h-[520px] overflow-hidden block">
                        <img :src="category.image" :alt="category.name"
                            class="w-full h-full object-cover grayscale group-hover:grayscale-0 transition-all duration-700 group-hover:scale-105" />
                        <!-- Overlay -->
                        <div class="absolute inset-0 bg-black/30 group-hover:bg-black/50 transition-all duration-500">
                        </div>
                        <!-- Thin border -->
                        <div class="absolute inset-0 border border-white/10 pointer-events-none"></div>

                        <div class="absolute bottom-8 left-7 right-7">
                            <p class="text-[10px] font-medium tracking-[0.3em] uppercase text-white/50 mb-2">{{
                                category.subtitle }}</p>
                            <h3 class="text-2xl font-light text-white mb-3 tracking-wide">{{ category.name }}</h3>
                            <div class="flex items-center gap-2 text-xs font-medium tracking-wider uppercase opacity-0 group-hover:opacity-100 translate-y-2 group-hover:translate-y-0 transition-all duration-500"
                                style="color: #C8A97E;">
                                Explore
                                <ArrowRight class="w-3.5 h-3.5" />
                            </div>
                        </div>
                    </RouterLink>
                </div>
            </div>
        </section>

        <!-- Features -->
        <section class="py-20 bg-white border-t border-black/5">
            <div class="max-w-screen-2xl mx-auto px-6 sm:px-8 lg:px-12">
                <div class="grid grid-cols-2 md:grid-cols-4 gap-px bg-black/5">
                    <div v-for="feature in features" :key="feature.title"
                        class="flex flex-col items-center text-center p-10 bg-white group">
                        <component :is="feature.icon"
                            class="w-5 h-5 mb-5 group-hover:scale-110 transition-all duration-500"
                            style="color: #C8A97E;" />
                        <h3 class="text-sm font-medium text-black tracking-wide mb-1.5">{{ feature.title }}</h3>
                        <p class="text-xs text-black/30">{{ feature.description }}</p>
                    </div>
                </div>
            </div>
        </section>

        <!-- Latest Stories -->
        <BlogSection />

        <!-- Newsletter -->
        <section class="py-28 md:py-36 bg-black relative">
            <div
                class="absolute top-0 left-0 right-0 h-px bg-gradient-to-r from-transparent via-white/10 to-transparent">
            </div>

            <div class="max-w-screen-xl mx-auto px-6 text-center">
                <p class="text-[10px] font-medium tracking-[0.35em] uppercase text-white/30 mb-6">— Newsletter</p>
                <h2 class="text-3xl md:text-5xl font-light text-white mb-5 tracking-[-0.01em]">Stay in the Loop</h2>
                <p class="text-white/30 mb-12 max-w-md mx-auto text-sm leading-relaxed">
                    Subscribe for exclusive offers, early access, and curated style inspiration.
                </p>

                <form class="max-w-lg mx-auto flex gap-0" @submit.prevent>
                    <input type="email" placeholder="Your email address"
                        class="flex-1 bg-transparent border border-white/15 border-r-0 px-6 py-4 text-white text-sm placeholder-white/25 focus:outline-none focus:border-white/40 transition-colors" />
                    <button type="submit"
                        class="px-8 py-4 text-black text-xs font-medium tracking-[0.15em] uppercase hover:opacity-90 transition-all duration-300"
                        style="background-color: #C8A97E;">
                        Subscribe
                    </button>
                </form>

                <p class="text-white/15 text-[10px] tracking-wider mt-6">No spam. Unsubscribe anytime.</p>
            </div>
        </section>
    </div>
</template>

<style scoped>
/* Marquee */
.marquee-container {
    width: 100%;
    overflow: hidden;
    position: relative;
}

.marquee-container::before,
.marquee-container::after {
    content: '';
    position: absolute;
    top: 0;
    bottom: 0;
    width: 120px;
    z-index: 2;
    pointer-events: none;
}

.marquee-container::before {
    left: 0;
    background: linear-gradient(to right, white, transparent);
}

.marquee-container::after {
    right: 0;
    background: linear-gradient(to left, white, transparent);
}

.marquee-track {
    display: flex;
    align-items: center;
    animation: marquee 35s linear infinite;
    width: max-content;
}

@keyframes marquee {
    0% {
        transform: translateX(0);
    }

    100% {
        transform: translateX(-33.333%);
    }
}
</style>