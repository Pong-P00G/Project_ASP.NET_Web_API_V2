<script setup>
import { RouterLink } from 'vue-router';
import { MapPin, Briefcase, ArrowRight, Users, Heart, Zap } from 'lucide-vue-next';

const values = [
    { icon: Users, title: 'Collaboration', description: 'We believe the best work happens when diverse minds come together.' },
    { icon: Heart, title: 'Passion', description: 'We are driven by our love for fashion, technology, and innovation.' },
    { icon: Zap, title: 'Growth', description: 'We invest in our people with mentorship, learning, and career development.' },
];

const openings = [
    { title: 'Senior Frontend Developer', team: 'Engineering', location: 'Remote', type: 'Full-time' },
    { title: 'UX/UI Designer', team: 'Design', location: 'Bangkok, TH', type: 'Full-time' },
    { title: 'Marketing Manager', team: 'Marketing', location: 'Remote', type: 'Full-time' },
    { title: 'Customer Success Associate', team: 'Support', location: 'Bangkok, TH', type: 'Full-time' },
    { title: 'Data Analyst', team: 'Product', location: 'Remote', type: 'Contract' },
];
</script>

<template>
    <div class="min-h-screen bg-white">
        <!-- Hero -->
        <section class="bg-black py-24 md:py-32 relative">
            <div class="relative max-w-screen-xl mx-auto px-6 text-center">
                <p class="text-[11px] font-medium tracking-[0.35em] uppercase text-white/40 mb-6">— Careers</p>
                <h1 class="text-4xl md:text-6xl font-light text-white tracking-[-0.02em] mb-6">Join Our
                    <span class="italic font-normal">Team</span>
                </h1>
                <p class="text-white/40 text-base max-w-lg mx-auto leading-relaxed font-light">
                    Help us shape the future of modern retail. We're looking for passionate people who love what they
                    do.
                </p>
            </div>
        </section>

        <!-- Values -->
        <section class="py-24 md:py-32">
            <div class="max-w-screen-lg mx-auto px-6">
                <div class="text-center mb-16">
                    <p class="text-[10px] font-medium tracking-[0.35em] uppercase text-black/30 mb-4">— Our Values</p>
                    <h2 class="text-3xl font-light text-black tracking-[-0.01em]">What We Stand For</h2>
                </div>
                <div class="grid md:grid-cols-3 gap-8">
                    <div v-for="value in values" :key="value.title"
                        class="text-center p-10 border border-black/5 hover:border-black/15 transition-colors duration-300">
                        <component :is="value.icon" class="w-6 h-6 text-black/30 mx-auto mb-5" />
                        <h3 class="text-sm font-medium mb-3">{{ value.title }}</h3>
                        <p class="text-xs text-black/40 leading-relaxed">{{ value.description }}</p>
                    </div>
                </div>
            </div>
        </section>

        <!-- Open Positions -->
        <section class="py-24 md:py-32 bg-neutral-50 border-t border-black/5">
            <div class="max-w-screen-lg mx-auto px-6">
                <div class="text-center mb-16">
                    <p class="text-[10px] font-medium tracking-[0.35em] uppercase text-black/30 mb-4">— Open
                        Positions</p>
                    <h2 class="text-3xl font-light text-black tracking-[-0.01em]">Current Openings</h2>
                </div>
                <div class="space-y-3">
                    <div v-for="job in openings" :key="job.title"
                        class="bg-white border border-black/5 p-6 md:p-8 flex flex-col md:flex-row md:items-center justify-between gap-4 hover:border-black/15 transition-colors duration-300 group cursor-pointer">
                        <div>
                            <h3 class="text-sm font-medium mb-2">{{ job.title }}</h3>
                            <div class="flex flex-wrap items-center gap-4 text-xs text-black/40">
                                <span class="flex items-center gap-1.5">
                                    <Briefcase class="w-3.5 h-3.5" /> {{ job.team }}
                                </span>
                                <span class="flex items-center gap-1.5">
                                    <MapPin class="w-3.5 h-3.5" /> {{ job.location }}
                                </span>
                                <span class="px-2 py-0.5 border border-black/10 text-[10px] tracking-wider uppercase">{{
                                    job.type }}</span>
                            </div>
                        </div>
                        <ArrowRight
                            class="w-4 h-4 text-black/20 group-hover:text-black group-hover:translate-x-1 transition-all duration-300" />
                    </div>
                </div>
            </div>
        </section>

        <!-- CTA -->
        <section class="py-20 border-t border-black/5">
            <div class="max-w-screen-lg mx-auto px-6 text-center">
                <p class="text-black/40 text-sm mb-6">Don't see a role that fits? We're always looking for talent.</p>
                <RouterLink to="/contact"
                    class="inline-flex items-center gap-2 px-8 py-4 bg-black text-white text-sm font-medium tracking-wider uppercase hover:bg-black/90 transition-all duration-300">
                    Get in Touch
                    <ArrowRight class="w-4 h-4" />
                </RouterLink>
            </div>
        </section>
    </div>
</template>
