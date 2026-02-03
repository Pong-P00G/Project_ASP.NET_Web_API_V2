<script setup>
import { ref } from 'vue';
import { RouterLink } from 'vue-router';
import {
    Award,
    Target,
    Heart,
    Users,
    TrendingUp,
    Shield,
    Zap,
    Globe,
    Package,
    Star,
    Check,
    Sparkles,
    Truck,
    Clock,
    ArrowRight
} from 'lucide-vue-next';

// Company Stats
const stats = ref([
    { icon: Users, value: '50K+', label: 'Happy Customers' },
    { icon: Package, value: '100K+', label: 'Products Delivered' },
    { icon: Globe, value: '25+', label: 'Countries Served' },
    { icon: Star, value: '4.9/5', label: 'Customer Rating' },
]);

// Core Values
const values = ref([
    {
        icon: Award,
        title: 'Quality First',
        description: 'We carefully curate every product to ensure premium quality and lasting value for our customers.',
    },
    {
        icon: Heart,
        title: 'Customer Care',
        description: 'Your satisfaction is our priority. We go above and beyond to provide exceptional service.',
    },
    {
        icon: Shield,
        title: 'Trust & Security',
        description: 'Shop with confidence knowing your data and transactions are protected with enterprise-grade security.',
    },
    {
        icon: Zap,
        title: 'Innovation',
        description: 'We continuously evolve to bring you the latest products and shopping experiences.',
    },
]);

// Team Members
const team = ref([
    {
        name: 'Sarah Johnson',
        role: 'Founder & CEO',
        image: 'https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?w=400&h=400&fit=crop',
        bio: 'Visionary leader with 15+ years in e-commerce'
    },
    {
        name: 'Michael Chen',
        role: 'Chief Technology Officer',
        image: 'https://images.unsplash.com/photo-1556157382-97eda2f9e2bf?w=400&h=400&fit=crop',
        bio: 'Tech innovator passionate about seamless experiences'
    },
    {
        name: 'Emily Rodriguez',
        role: 'Head of Marketing',
        image: 'https://images.unsplash.com/photo-1580489944761-15a19d654956?w=400&h=400&fit=crop',
        bio: 'Creative strategist connecting brands with customers'
    },
    {
        name: 'David Kim',
        role: 'Operations Director',
        image: 'https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?w=400&h=400&fit=crop',
        bio: 'Logistics expert ensuring fast and reliable delivery'
    },
]);

// Milestones
const milestones = ref([
    { year: '2020', title: 'Company Founded', description: 'Started with a vision to revolutionize online shopping' },
    { year: '2021', title: 'Reached 10K Customers', description: 'Milestone achieved through quality and service' },
    { year: '2022', title: 'International Expansion', description: 'Expanded to 25+ countries worldwide' },
    { year: '2023', title: 'Award Recognition', description: 'Named Best E-commerce Platform of the Year' },
    { year: '2024', title: 'Innovation Launch', description: 'Introduced AI-powered shopping assistant' },
]);
</script>

<template>
    <div class="min-h-screen bg-white">
        <!-- Hero Section -->
        <section class="py-32 bg-white overflow-hidden mt-6">
            <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                <div class="text-center space-y-8 max-w-4xl mx-auto">
                    <div class="inline-flex items-center gap-2 px-3 py-1 bg-gray-100 rounded-full text-gray-600 text-xs font-bold tracking-wider uppercase">
                        <Sparkles class="w-3 h-3" />
                        About AlieeShop
                    </div>
                    <h1 class="text-5xl md:text-6xl font-light text-gray-900 tracking-tight">
                        Elevating your <br />
                        <span class="font-bold italic">Shopping Experience</span>
                    </h1>
                    <p class="text-xl text-gray-500 leading-relaxed font-light max-w-3xl mx-auto">
                        We're on a mission to create the most delightful shopping experience by combining quality products, exceptional service, and innovative technology.
                    </p>
                </div>
            </div>
        </section>

        <!-- Stats Section -->
        <section class="py-24 bg-white border-y border-gray-100">
            <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                <div class="grid grid-cols-2 md:grid-cols-4 gap-12">
                    <div v-for="stat in stats" :key="stat.label" class="flex flex-col items-center text-center space-y-4">
                        <div class="w-16 h-16 rounded-2xl bg-gray-50 flex items-center justify-center text-gray-400 group-hover:bg-violet-50 group-hover:text-violet-600 transition-colors duration-300">
                            <component :is="stat.icon" class="w-8 h-8" />
                        </div>
                        <div>
                            <div class="text-4xl font-black text-gray-900 mb-1 tracking-tight">{{ stat.value }}</div>
                            <div class="text-gray-500 font-medium uppercase text-xs tracking-widest">{{ stat.label }}</div>
                        </div>
                    </div>
                </div>
            </div>
        </section>

        <!-- Our Story -->
        <section class="py-32 bg-white overflow-hidden">
            <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                <div class="grid lg:grid-cols-2 gap-24 items-center">
                    <div class="relative">
                        <div class="absolute -top-12 -left-12 w-64 h-64 bg-violet-100 rounded-full blur-3xl opacity-50"></div>
                        <div class="absolute -bottom-12 -right-12 w-64 h-64 bg-amber-100 rounded-full blur-3xl opacity-50"></div>
                        
                        <div class="relative grid grid-cols-2 gap-4">
                            <img src="https://images.unsplash.com/photo-1556742049-0cfed4f6a45d?w=400&h=600&fit=crop"
                                alt="Our Story" class="rounded-3xl shadow-2xl w-full h-80 object-cover mt-12" />
                            <img src="https://images.unsplash.com/photo-1441986300917-64674bd600d8?w=400&h=600&fit=crop"
                                alt="Storefront" class="rounded-3xl shadow-2xl w-full h-80 object-cover" />
                        </div>
                        <div class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 bg-white p-8 rounded-3xl shadow-2xl border border-gray-100 text-center">
                            <div class="text-4xl font-black text-gray-900">4.9/5</div>
                            <div class="flex justify-center gap-1 my-2">
                                <Star v-for="i in 5" :key="i" class="w-4 h-4 text-amber-400 fill-amber-400" />
                            </div>
                            <div class="text-sm text-gray-500 font-medium">Customer Satisfaction</div>
                        </div>
                    </div>
                    <div class="space-y-8">
                        <div class="inline-flex items-center gap-2 px-3 py-1 bg-violet-50 rounded-full text-violet-600 text-xs font-bold tracking-wider uppercase">
                            <Target class="w-3 h-3" />
                            Our Story
                        </div>
                        <h2 class="text-5xl font-light text-gray-900 leading-tight tracking-tight">
                            Building Trust Through <br />
                            <span class="font-bold italic">Quality & Service</span>
                        </h2>
                        <div class="space-y-4 text-lg text-gray-500 leading-relaxed font-light">
                            <p>
                                Founded in 2020, AlieeShop began with a simple idea: make online shopping better. We noticed that customers wanted more than just products—they wanted an experience built on trust, quality, and genuine care.
                            </p>
                            <p>
                                Today, we've grown into a global platform serving over 50,000 happy customers across 25+ countries. Our success comes from never compromising on our core values and always putting customers first.
                            </p>
                            <p>
                                Every product we offer is carefully selected, every customer interaction is meaningful, and every innovation we pursue is designed to make your life easier and more enjoyable.
                            </p>
                        </div>
                        <div class="grid sm:grid-cols-2 gap-6">
                            <div class="space-y-2">
                                <div class="w-10 h-10 bg-green-50 text-green-600 rounded-lg flex items-center justify-center">
                                    <Shield class="w-6 h-6" />
                                </div>
                                <h4 class="font-bold text-gray-900">Guaranteed Quality</h4>
                                <p class="text-gray-500 text-sm">Every product is hand-checked by our experts.</p>
                            </div>
                            <div class="space-y-2">
                                <div class="w-10 h-10 bg-blue-50 text-blue-600 rounded-lg flex items-center justify-center">
                                    <Truck class="w-6 h-6" />
                                </div>
                                <h4 class="font-bold text-gray-900">Worldwide Shipping</h4>
                                <p class="text-gray-500 text-sm">Fast, reliable delivery to over 150 countries.</p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </section>

        <!-- Core Values -->
        <section class="py-32 bg-gray-50/50">
            <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                <div class="text-center mb-16 space-y-4">
                    <div class="inline-flex items-center gap-2 px-3 py-1 bg-amber-100/50 rounded-full text-amber-600 text-xs font-bold tracking-wider uppercase">
                        <Heart class="w-3 h-3" />
                        Our Values
                    </div>
                    <h2 class="text-4xl md:text-5xl font-light text-gray-900 tracking-tight">
                        What We <span class="font-bold italic">Stand For</span>
                    </h2>
                    <p class="text-gray-500 max-w-2xl mx-auto text-lg font-light">
                        Our core values guide everything we do and shape the experience we create for our customers
                    </p>
                    <div class="w-24 h-1 bg-amber-500 mx-auto rounded-full"></div>
                </div>

                <div class="grid md:grid-cols-2 lg:grid-cols-4 gap-8">
                    <div v-for="value in values" :key="value.title"
                        class="bg-white p-8 rounded-3xl shadow-sm hover:shadow-2xl transition-all duration-700 border border-gray-100 group">
                        <div class="w-14 h-14 rounded-2xl bg-gray-50 flex items-center justify-center text-gray-400 group-hover:bg-violet-50 group-hover:text-violet-600 transition-colors duration-300 mb-6">
                            <component :is="value.icon" class="w-7 h-7" />
                        </div>
                        <h3 class="text-xl font-bold text-gray-900 mb-3">{{ value.title }}</h3>
                        <p class="text-gray-500 leading-relaxed text-sm">{{ value.description }}</p>
                    </div>
                </div>
            </div>
        </section>

        <!-- Timeline -->
        <section class="py-32 bg-white">
            <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                <div class="text-center mb-16 space-y-4">
                    <div class="inline-flex items-center gap-2 px-3 py-1 bg-gray-100 rounded-full text-gray-600 text-xs font-bold tracking-wider uppercase">
                        <TrendingUp class="w-3 h-3" />
                        Our Journey
                    </div>
                    <h2 class="text-4xl md:text-5xl font-light text-gray-900 tracking-tight">
                        Milestones & <span class="font-bold italic">Achievements</span>
                    </h2>
                    <div class="w-24 h-1 bg-amber-500 mx-auto rounded-full"></div>
                </div>

                <div class="space-y-16">
                    <div v-for="(milestone) in milestones" :key="milestone.year" 
                        class="flex flex-col md:flex-row items-start gap-8 group">
                        <div class="flex-shrink-0 w-32">
                            <div class="text-5xl font-black text-gray-900 tracking-tight">{{ milestone.year }}</div>
                        </div>
                        <div class="flex-1 bg-gray-50/50 rounded-3xl p-8 hover:shadow-xl transition-all duration-500 border border-gray-100">
                            <h3 class="text-2xl font-bold text-gray-900 mb-3">{{ milestone.title }}</h3>
                            <p class="text-gray-500 leading-relaxed">{{ milestone.description }}</p>
                        </div>
                    </div>
                </div>
            </div>
        </section>

        <!-- Team Section -->
        <section class="py-32 bg-gray-50/50">
            <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                <div class="text-center mb-16 space-y-4">
                    <div class="inline-flex items-center gap-2 px-3 py-1 bg-gray-100 rounded-full text-gray-600 text-xs font-bold tracking-wider uppercase">
                        <Users class="w-3 h-3" />
                        Our Team
                    </div>
                    <h2 class="text-4xl md:text-5xl font-light text-gray-900 tracking-tight">
                        Meet the <span class="font-bold italic">Dream Team</span>
                    </h2>
                    <p class="text-gray-500 max-w-2xl mx-auto text-lg font-light">
                        Passionate individuals working together to create exceptional shopping experiences
                    </p>
                    <div class="w-24 h-1 bg-amber-500 mx-auto rounded-full"></div>
                </div>

                <div class="grid md:grid-cols-2 lg:grid-cols-4 gap-8">
                    <div v-for="member in team" :key="member.name" class="group">
                        <div class="relative h-96 overflow-hidden rounded-3xl bg-gray-100 shadow-sm hover:shadow-2xl transition-all duration-700 mb-6">
                            <img :src="member.image" :alt="member.name"
                                class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-1000 grayscale-[0.2] group-hover:grayscale-0" />
                            <div class="absolute inset-0 bg-linear-to-t from-black/80 via-black/20 to-transparent opacity-60 group-hover:opacity-80 transition-opacity duration-500"></div>
                            <div class="absolute inset-0 flex flex-col justify-end p-6">
                                <h3 class="text-xl font-bold text-white mb-1">{{ member.name }}</h3>
                                <p class="text-white/70 text-sm font-medium mb-2">{{ member.role }}</p>
                                <p class="text-white/90 text-sm">{{ member.bio }}</p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </section>

        <!-- CTA Section -->
        <section class="py-24 bg-white">
            <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                <div class="relative bg-gray-900 rounded-[3rem] overflow-hidden p-12 md:p-24 text-center">
                    <div class="absolute top-0 left-0 w-full h-full">
                        <div class="absolute top-0 right-0 w-1/2 h-full bg-linear-to-bl from-violet-600/20 to-transparent"></div>
                        <div class="absolute bottom-0 left-0 w-1/2 h-full bg-linear-to-tr from-amber-600/10 to-transparent"></div>
                    </div>
                    <div class="relative z-10 max-w-2xl mx-auto space-y-8">
                        <h2 class="text-4xl md:text-6xl font-light text-white tracking-tight">
                            Ready to Experience the <span class="font-bold italic text-violet-400">Difference?</span>
                        </h2>
                        <p class="text-gray-400 text-lg md:text-xl font-light">
                            Join thousands of satisfied customers and discover why AlieeShop is trusted worldwide
                        </p>
                        <div class="flex flex-col sm:flex-row gap-4 justify-center">
                            <RouterLink to="/product"
                                class="px-8 py-4 bg-white text-gray-900 font-bold rounded-full hover:bg-violet-400 hover:text-white transition-all duration-300 flex items-center justify-center gap-3 group">
                                Start Shopping
                                <ArrowRight class="w-5 h-5 group-hover:translate-x-1 transition-transform" />
                            </RouterLink>
                            <RouterLink to="/contact"
                                class="px-8 py-4 bg-white/10 backdrop-blur-md text-white font-bold rounded-full border-2 border-white/30 hover:bg-white/20 transition-all duration-300">
                                Contact Us
                            </RouterLink>
                        </div>
                    </div>
                </div>
            </div>
        </section>
    </div>
</template>
