<script setup>
import { Truck, Clock, Globe, ShieldCheck } from 'lucide-vue-next';

const shippingOptions = [
    { name: 'Standard Shipping', time: '5-7 Business Days', price: 'Free over $50', icon: Truck },
    { name: 'Express Shipping', time: '2-3 Business Days', price: '$12.99', icon: Clock },
    { name: 'Next Day Delivery', time: '1 Business Day', price: '$24.99', icon: ShieldCheck },
    { name: 'International', time: '7-14 Business Days', price: 'From $19.99', icon: Globe },
];

const faqs = [
    { q: 'How do I track my order?', a: 'Once your order ships, you will receive an email with a tracking number. You can also check your order status on our Track Order page.' },
    { q: 'Do you ship internationally?', a: 'Yes! We ship to over 50 countries worldwide. Shipping times and costs vary by destination.' },
    { q: 'What if my package is lost?', a: 'Contact our support team within 30 days. We will investigate and send a replacement or issue a full refund.' },
    { q: 'Can I change my shipping address?', a: 'You can update your shipping address within 2 hours of placing your order by contacting our support team.' },
];
</script>

<template>
    <div class="min-h-screen bg-white">
        <!-- Hero -->
        <section class="bg-black py-24 md:py-32 relative">
            <div class="relative max-w-screen-xl mx-auto px-6 text-center">
                <p class="text-[11px] font-medium tracking-[0.35em] uppercase text-white/40 mb-6">— Shipping</p>
                <h1 class="text-4xl md:text-6xl font-light text-white tracking-[-0.02em] mb-6">Shipping
                    <span class="italic font-normal">Information</span>
                </h1>
                <p class="text-white/40 text-base max-w-lg mx-auto leading-relaxed font-light">
                    Fast, reliable delivery to your doorstep. Free shipping on orders over $50.
                </p>
            </div>
        </section>

        <!-- Shipping Options -->
        <section class="py-24 md:py-32">
            <div class="max-w-screen-lg mx-auto px-6">
                <div class="text-center mb-16">
                    <p class="text-[10px] font-medium tracking-[0.35em] uppercase text-black/30 mb-4">— Delivery
                        Options</p>
                    <h2 class="text-3xl font-light text-black tracking-[-0.01em]">Choose Your Speed</h2>
                </div>
                <div class="grid md:grid-cols-2 gap-4">
                    <div v-for="option in shippingOptions" :key="option.name"
                        class="flex items-start gap-6 p-8 border border-black/5 hover:border-black/15 transition-colors duration-300">
                        <component :is="option.icon" class="w-5 h-5 text-black/30 mt-0.5 shrink-0" />
                        <div>
                            <h3 class="text-sm font-medium mb-1">{{ option.name }}</h3>
                            <p class="text-xs text-black/40">{{ option.time }}</p>
                            <p class="text-xs font-medium text-black/60 mt-2">{{ option.price }}</p>
                        </div>
                    </div>
                </div>
            </div>
        </section>

        <!-- FAQs -->
        <section class="py-24 md:py-32 bg-neutral-50 border-t border-black/5">
            <div class="max-w-screen-lg mx-auto px-6">
                <div class="text-center mb-16">
                    <p class="text-[10px] font-medium tracking-[0.35em] uppercase text-black/30 mb-4">—
                        Questions</p>
                    <h2 class="text-3xl font-light text-black tracking-[-0.01em]">Common Questions</h2>
                </div>
                <div class="space-y-4">
                    <div v-for="faq in faqs" :key="faq.q" class="bg-white p-8 border border-black/5">
                        <h3 class="text-sm font-medium mb-3">{{ faq.q }}</h3>
                        <p class="text-xs text-black/40 leading-relaxed">{{ faq.a }}</p>
                    </div>
                </div>
            </div>
        </section>
    </div>
</template>
