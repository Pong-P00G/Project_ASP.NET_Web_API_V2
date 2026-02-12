<script setup>
import { RouterLink } from 'vue-router';
import { ArrowRight, Mail, Download } from 'lucide-vue-next';

const pressReleases = [
    { date: 'Feb 5, 2026', title: 'AlieeShop Launches Sustainable Fashion Line', excerpt: 'Our new eco-friendly collection uses 100% recycled materials, setting a new standard in sustainable retail.' },
    { date: 'Jan 20, 2026', title: 'AlieeShop Expands to 30 New Markets', excerpt: 'We are excited to announce international expansion across Southeast Asia and Europe.' },
    { date: 'Dec 15, 2025', title: 'Record-Breaking Holiday Season', excerpt: 'AlieeShop achieved a 200% year-over-year growth during the 2025 holiday shopping season.' },
    { date: 'Nov 1, 2025', title: 'Partnership with Leading Fashion Brands', excerpt: 'Strategic partnerships bring exclusive collections from top global designers to our platform.' },
];

const mediaFeatures = [
    'VOGUE', 'HYPEBEAST', 'GQ', "HARPER'S BAZAAR", 'ELLE', 'DAZED'
];
</script>

<template>
    <div class="min-h-screen bg-white">
        <!-- Hero -->
        <section class="bg-black py-24 md:py-32 relative">
            <div class="relative max-w-screen-xl mx-auto px-6 text-center">
                <p class="text-[11px] font-medium tracking-[0.35em] uppercase text-white/40 mb-6">— Press</p>
                <h1 class="text-4xl md:text-6xl font-light text-white tracking-[-0.02em] mb-6">In the
                    <span class="italic font-normal">News</span>
                </h1>
                <p class="text-white/40 text-base max-w-lg mx-auto leading-relaxed font-light">
                    The latest news, announcements, and media coverage about AlieeShop.
                </p>
            </div>
        </section>

        <!-- As Seen In -->
        <section class="py-16 border-b border-black/5">
            <div class="max-w-screen-lg mx-auto px-6">
                <p class="text-[10px] font-medium tracking-[0.35em] uppercase text-black/20 text-center mb-10">— As
                    Featured In</p>
                <div class="flex flex-wrap justify-center items-center gap-10 md:gap-16">
                    <span v-for="media in mediaFeatures" :key="media"
                        class="text-lg md:text-xl font-light tracking-[0.15em] uppercase text-black/15 select-none">
                        {{ media }}
                    </span>
                </div>
            </div>
        </section>

        <!-- Press Releases -->
        <section class="py-24 md:py-32">
            <div class="max-w-screen-lg mx-auto px-6">
                <div class="text-center mb-16">
                    <p class="text-[10px] font-medium tracking-[0.35em] uppercase text-black/30 mb-4">— Latest</p>
                    <h2 class="text-3xl font-light text-black tracking-[-0.01em]">Press Releases</h2>
                </div>
                <div class="space-y-0 border-t border-black/5">
                    <div v-for="release in pressReleases" :key="release.title"
                        class="py-10 border-b border-black/5 group cursor-pointer hover:pl-4 transition-all duration-300">
                        <p class="text-[10px] font-medium tracking-[0.3em] uppercase text-black/30 mb-3">{{
                            release.date }}</p>
                        <h3 class="text-lg font-light mb-3 group-hover:text-black/70 transition-colors duration-300">
                            {{ release.title }}</h3>
                        <p class="text-sm text-black/40 leading-relaxed max-w-2xl">{{ release.excerpt }}</p>
                    </div>
                </div>
            </div>
        </section>

        <!-- Media Contact -->
        <section class="py-24 md:py-32 bg-neutral-50 border-t border-black/5">
            <div class="max-w-screen-sm mx-auto px-6 text-center">
                <p class="text-[10px] font-medium tracking-[0.35em] uppercase text-black/30 mb-4">— Media Inquiries
                </p>
                <h2 class="text-3xl font-light text-black tracking-[-0.01em] mb-6">Media Contact</h2>
                <p class="text-sm text-black/40 leading-relaxed mb-8">
                    For press inquiries, interview requests, or media assets, please reach out to our communications
                    team.
                </p>
                <div class="flex flex-col sm:flex-row items-center justify-center gap-4">
                    <a href="mailto:press@alieeshop.com"
                        class="inline-flex items-center gap-2 px-8 py-4 bg-black text-white text-sm font-medium tracking-wider uppercase hover:bg-black/90 transition-all duration-300">
                        <Mail class="w-4 h-4" />
                        press@alieeshop.com
                    </a>
                    <button
                        class="inline-flex items-center gap-2 px-8 py-4 border border-black/15 text-sm font-medium tracking-wider uppercase hover:border-black/40 transition-all duration-300">
                        <Download class="w-4 h-4" />
                        Press Kit
                    </button>
                </div>
            </div>
        </section>
    </div>
</template>
