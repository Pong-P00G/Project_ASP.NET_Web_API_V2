<script setup>
import { ref } from 'vue';
import { RouterLink } from 'vue-router';
import { ChevronDown, ArrowRight } from 'lucide-vue-next';

const categories = [
    {
        name: 'Orders & Shipping',
        questions: [
            { q: 'How long does shipping take?', a: 'Standard shipping takes 5-7 business days. Express shipping is 2-3 business days. Next-day delivery is available for orders placed before 2 PM.' },
            { q: 'How can I track my order?', a: 'Once your order ships, you\'ll receive a tracking email. You can also visit our Track Order page with your order ID and email.' },
            { q: 'Do you offer free shipping?', a: 'Yes! Free standard shipping is available on all orders over $50. No promo code needed.' },
            { q: 'Can I change or cancel my order?', a: 'Orders can be modified or cancelled within 2 hours of placement. After that, the order enters processing and cannot be changed.' },
        ]
    },
    {
        name: 'Returns & Refunds',
        questions: [
            { q: 'What is your return policy?', a: 'We offer a 30-day return policy for most items. Items must be in original condition with tags attached.' },
            { q: 'How do I initiate a return?', a: 'Log in to your account, go to order history, and select "Return Item." We\'ll provide a prepaid shipping label.' },
            { q: 'How long do refunds take?', a: 'Refunds are processed within 5-7 business days after we receive your return. The credit will appear on your original payment method.' },
        ]
    },
    {
        name: 'Account & Security',
        questions: [
            { q: 'How do I reset my password?', a: 'Click "Forgot Password" on the login page. Enter your email and we\'ll send you a reset link.' },
            { q: 'Is my payment information secure?', a: 'Absolutely. We use industry-standard SSL encryption and never store your full credit card details.' },
            { q: 'Can I delete my account?', a: 'Yes. Contact our support team and we\'ll process your account deletion within 48 hours.' },
        ]
    },
    {
        name: 'Products & Sizing',
        questions: [
            { q: 'How do I find my size?', a: 'Each product page includes a detailed size guide. If you\'re between sizes, we recommend sizing up for a more comfortable fit.' },
            { q: 'Are your products authentic?', a: 'Yes. We only sell 100% authentic products sourced directly from brands or authorized distributors.' },
            { q: 'Do you offer gift wrapping?', a: 'Yes! Gift wrapping is available at checkout for an additional $5 per item. Include a personalized message too.' },
        ]
    }
];

const openItems = ref(new Set());

const toggle = (key) => {
    if (openItems.value.has(key)) {
        openItems.value.delete(key);
    } else {
        openItems.value.add(key);
    }
};
</script>

<template>
    <div class="min-h-screen bg-white">
        <!-- Hero -->
        <section class="bg-black py-24 md:py-32 relative">
            <div class="relative max-w-screen-xl mx-auto px-6 text-center">
                <p class="text-[11px] font-medium tracking-[0.35em] uppercase text-white/40 mb-6">— Help Center</p>
                <h1 class="text-4xl md:text-6xl font-light text-white tracking-[-0.02em] mb-6">Frequently Asked
                    <span class="italic font-normal">Questions</span>
                </h1>
                <p class="text-white/40 text-base max-w-lg mx-auto leading-relaxed font-light">
                    Find answers to common questions about orders, shipping, returns, and more.
                </p>
            </div>
        </section>

        <!-- FAQ Accordion -->
        <section class="py-24 md:py-32">
            <div class="max-w-screen-lg mx-auto px-6">
                <div v-for="(category, cIdx) in categories" :key="category.name" class="mb-16 last:mb-0">
                    <p class="text-[10px] font-medium tracking-[0.35em] uppercase text-black/30 mb-6">— {{
                        category.name }}</p>
                    <div class="border-t border-black/5">
                        <div v-for="(item, qIdx) in category.questions" :key="qIdx" class="border-b border-black/5">
                            <button @click="toggle(`${cIdx}-${qIdx}`)"
                                class="w-full flex items-center justify-between py-6 text-left group">
                                <span class="text-sm font-medium pr-8 group-hover:text-black/70 transition-colors">{{
                                    item.q }}</span>
                                <ChevronDown class="w-4 h-4 text-black/30 shrink-0 transition-transform duration-300"
                                    :class="openItems.has(`${cIdx}-${qIdx}`) ? 'rotate-180' : ''" />
                            </button>
                            <transition name="accordion">
                                <div v-if="openItems.has(`${cIdx}-${qIdx}`)" class="pb-6 -mt-2">
                                    <p class="text-sm text-black/40 leading-relaxed max-w-2xl">{{ item.a }}</p>
                                </div>
                            </transition>
                        </div>
                    </div>
                </div>
            </div>
        </section>

        <!-- CTA -->
        <section class="py-20 bg-neutral-50 border-t border-black/5">
            <div class="max-w-screen-lg mx-auto px-6 text-center">
                <h2 class="text-2xl font-light text-black mb-3">Still have questions?</h2>
                <p class="text-sm text-black/40 mb-8">Our support team is here to help</p>
                <RouterLink to="/contact-support"
                    class="inline-flex items-center gap-2 px-8 py-4 bg-black text-white text-sm font-medium tracking-wider uppercase hover:bg-black/90 transition-all duration-300">
                    Contact Support
                    <ArrowRight class="w-4 h-4" />
                </RouterLink>
            </div>
        </section>
    </div>
</template>

<style scoped>
.accordion-enter-active,
.accordion-leave-active {
    transition: all 0.3s ease;
    overflow: hidden;
}

.accordion-enter-from,
.accordion-leave-to {
    opacity: 0;
    max-height: 0;
}

.accordion-enter-to,
.accordion-leave-from {
    opacity: 1;
    max-height: 200px;
}
</style>
