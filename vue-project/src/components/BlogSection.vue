<script setup>
import { RouterLink } from 'vue-router';
import { ArrowRight } from 'lucide-vue-next';

const stories = [
    {
        id: 1,
        title: 'The Colors Defining Spring 2026',
        category: 'Trends',
        date: 'Feb 10, 2026',
        image: 'https://images.unsplash.com/photo-1490481651871-ab68de25d43d?q=80&w=2070&auto=format&fit=crop',
        description: 'Explore the palette that will dominate the upcoming season, from vibrant corals to soothing pastels.'
    },
    {
        id: 2,
        title: 'Why Eco-Friendly Fabrics Matter',
        category: 'Sustainability',
        date: 'Feb 05, 2026',
        image: 'https://images.unsplash.com/photo-1529139574466-a302d2789bab?q=80&w=2070&auto=format&fit=crop',
        description: 'How choosing organic cotton and recycled materials can make a massive impact on our planet.'
    },
    {
        id: 3,
        title: '5 Essentials for a Minimalist Wardrobe',
        category: 'Style Guide',
        date: 'Jan 28, 2026',
        image: 'https://images.unsplash.com/photo-1434389677669-e08b4cac3105?q=80&w=2005&auto=format&fit=crop',
        description: 'Building a versatile collection with key pieces that never go out of style.'
    }
];
</script>

<template>
    <section class="py-24 md:py-32 bg-white">
        <div class="max-w-screen-2xl mx-auto px-6 sm:px-8 lg:px-12">
            <div class="flex items-end justify-between mb-16">
                <div>
                    <p class="text-[10px] font-medium tracking-[0.35em] uppercase text-black/30 mb-4">— Journal</p>
                    <h2 class="text-3xl md:text-4xl font-light text-black tracking-[-0.01em]">Latest Stories</h2>
                </div>
                <RouterLink to="/blog"
                    class="hidden md:flex items-center gap-2 text-xs font-medium tracking-[0.15em] uppercase transition-colors duration-500 group"
                    style="color: #C8A97E;">
                    View All
                    <ArrowRight class="w-3.5 h-3.5 group-hover:translate-x-1 transition-transform duration-300" />
                </RouterLink>
            </div>

            <div class="grid grid-cols-1 md:grid-cols-3 gap-px bg-black/5">
                <article v-for="story in stories" :key="story.id"
                    class="blog-card group cursor-pointer bg-white p-6 md:p-8">
                    <div class="relative overflow-hidden mb-6">
                        <img :src="story.image" :alt="story.title"
                            class="w-full h-72 object-cover grayscale group-hover:grayscale-0 group-hover:scale-105 transition-all duration-700" />
                        <!-- Overlay on hover -->
                        <div
                            class="absolute inset-0 bg-black/0 group-hover:bg-black/20 transition-all duration-500 flex items-end p-5">
                            <span
                                class="text-xs font-medium tracking-wider uppercase opacity-0 group-hover:opacity-100 translate-y-4 group-hover:translate-y-0 transition-all duration-500 flex items-center gap-2"
                                style="color: #C8A97E;">
                                Read Article
                                <ArrowRight class="w-3.5 h-3.5" />
                            </span>
                        </div>
                    </div>
                    <div>
                        <div
                            class="flex items-center gap-3 text-[10px] font-medium tracking-[0.2em] uppercase text-black/30 mb-4">
                            <span class="text-black/60">{{ story.category }}</span>
                            <span class="w-3 h-px bg-black/15"></span>
                            <span>{{ story.date }}</span>
                        </div>
                        <h3
                            class="text-lg font-medium text-black mb-3 group-hover:underline underline-offset-4 transition-all leading-snug">
                            {{ story.title }}
                        </h3>
                        <p class="text-sm text-black/30 leading-relaxed line-clamp-2">
                            {{ story.description }}
                        </p>
                    </div>
                </article>
            </div>
        </div>
    </section>
</template>

<style scoped></style>
