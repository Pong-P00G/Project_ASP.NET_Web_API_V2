<script setup>
import { RouterLink } from 'vue-router';
import { RotateCcw, Clock, CheckCircle, Package, ArrowRight } from 'lucide-vue-next';

const steps = [
    { icon: Package, title: 'Initiate Return', description: 'Log in and go to your order history. Select the item you want to return.' },
    { icon: RotateCcw, title: 'Ship It Back', description: 'Pack the item securely. Use the prepaid shipping label we provide.' },
    { icon: CheckCircle, title: 'Get Your Refund', description: 'Once received and inspected, your refund will be processed within 5-7 business days.' },
];

const policies = [
    { title: '30-Day Window', description: 'Return most items within 30 days of delivery for a full refund.' },
    { title: 'Original Condition', description: 'Items must be unworn, unwashed, with tags attached and in original packaging.' },
    { title: 'Free Returns', description: 'We provide a prepaid return shipping label. No return shipping costs for you.' },
    { title: 'Exchanges', description: 'Want a different size or color? We offer free exchanges on all eligible items.' },
];
</script>

<template>
    <div class="min-h-screen bg-white">
        <!-- Hero -->
        <section class="bg-black py-24 md:py-32 relative">
            <div class="relative max-w-screen-xl mx-auto px-6 text-center">
                <p class="text-[11px] font-medium tracking-[0.35em] uppercase text-white/40 mb-6">— Returns &
                    Exchanges</p>
                <h1 class="text-4xl md:text-6xl font-light text-white tracking-[-0.02em] mb-6">Hassle-Free
                    <span class="italic font-normal">Returns</span>
                </h1>
                <p class="text-white/40 text-base max-w-lg mx-auto leading-relaxed font-light">
                    We want you to love every purchase. If something isn't right, we make returns easy.
                </p>
            </div>
        </section>

        <!-- How It Works -->
        <section class="py-24 md:py-32">
            <div class="max-w-screen-lg mx-auto px-6">
                <div class="text-center mb-16">
                    <p class="text-[10px] font-medium tracking-[0.35em] uppercase text-black/30 mb-4">— Simple Process
                    </p>
                    <h2 class="text-3xl font-light text-black tracking-[-0.01em]">How It Works</h2>
                </div>
                <div class="grid md:grid-cols-3 gap-8">
                    <div v-for="(step, i) in steps" :key="step.title"
                        class="text-center p-10 border border-black/5 hover:border-black/15 transition-colors duration-300">
                        <div class="text-[10px] font-medium tracking-[0.3em] uppercase text-black/20 mb-6">Step {{ i + 1
                        }}</div>
                        <component :is="step.icon" class="w-6 h-6 text-black/30 mx-auto mb-5" />
                        <h3 class="text-sm font-medium mb-3">{{ step.title }}</h3>
                        <p class="text-xs text-black/40 leading-relaxed">{{ step.description }}</p>
                    </div>
                </div>
            </div>
        </section>

        <!-- Policies -->
        <section class="py-24 md:py-32 bg-neutral-50 border-t border-black/5">
            <div class="max-w-screen-lg mx-auto px-6">
                <div class="text-center mb-16">
                    <p class="text-[10px] font-medium tracking-[0.35em] uppercase text-black/30 mb-4">— Our Promise</p>
                    <h2 class="text-3xl font-light text-black tracking-[-0.01em]">Return Policy</h2>
                </div>
                <div class="grid md:grid-cols-2 gap-6">
                    <div v-for="policy in policies" :key="policy.title" class="bg-white p-8 border border-black/5">
                        <h3 class="text-sm font-medium mb-3">{{ policy.title }}</h3>
                        <p class="text-xs text-black/40 leading-relaxed">{{ policy.description }}</p>
                    </div>
                </div>
            </div>
        </section>

        <!-- CTA -->
        <section class="py-20 border-t border-black/5">
            <div class="max-w-screen-lg mx-auto px-6 text-center">
                <p class="text-black/40 text-sm mb-6">Need help with a return?</p>
                <RouterLink to="/contact-support"
                    class="inline-flex items-center gap-2 px-8 py-4 bg-black text-white text-sm font-medium tracking-wider uppercase hover:bg-black/90 transition-all duration-300">
                    Contact Support
                    <ArrowRight class="w-4 h-4" />
                </RouterLink>
            </div>
        </section>
    </div>
</template>
