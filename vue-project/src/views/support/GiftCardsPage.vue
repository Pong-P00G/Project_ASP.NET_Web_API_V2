<script setup>
import { ref } from 'vue';
import { RouterLink } from 'vue-router';
import { Gift, CreditCard, Mail, Sparkles } from 'lucide-vue-next';

const giftCards = ref([
    { amount: 25, popular: false },
    { amount: 50, popular: true },
    { amount: 100, popular: false },
    { amount: 200, popular: false },
]);

const selectedAmount = ref(50);
const customAmount = ref('');
const recipientEmail = ref('');
const senderName = ref('');
const message = ref('');
</script>

<template>
    <div class="min-h-screen bg-white">
        <!-- Hero -->
        <section class="bg-black py-24 md:py-32 relative overflow-hidden">
            <div class="absolute inset-0 opacity-[0.03]"
                style="background-image: url('data:image/svg+xml,%3Csvg viewBox=%220 0 256 256%22 xmlns=%22http://www.w3.org/2000/svg%22%3E%3Cfilter id=%22noise%22%3E%3CfeTurbulence baseFrequency=%220.9%22 numOctaves=%224%22 stitchTiles=%22stitch%22/%3E%3C/filter%3E%3Crect width=%22100%25%22 height=%22100%25%22 filter=%22url(%23noise)%22/%3E%3C/svg%3E');">
            </div>
            <div class="relative max-w-screen-xl mx-auto px-6 text-center">
                <p class="text-[11px] font-medium tracking-[0.35em] uppercase text-white/40 mb-6">— Gift Cards</p>
                <h1 class="text-4xl md:text-6xl font-light text-white tracking-[-0.02em] mb-6">The Perfect
                    <span class="italic font-normal">Gift</span>
                </h1>
                <p class="text-white/40 text-base max-w-lg mx-auto leading-relaxed font-light">
                    Give the gift of choice. Our gift cards let them pick exactly what they love.
                </p>
            </div>
        </section>

        <!-- Gift Card Selection -->
        <section class="py-24 md:py-32">
            <div class="max-w-screen-lg mx-auto px-6">
                <div class="text-center mb-16">
                    <p class="text-[10px] font-medium tracking-[0.35em] uppercase text-black/30 mb-4">— Choose Amount
                    </p>
                    <h2 class="text-3xl font-light text-black tracking-[-0.01em]">Select a Value</h2>
                </div>

                <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-12">
                    <button v-for="card in giftCards" :key="card.amount" @click="selectedAmount = card.amount"
                        class="relative p-8 border transition-all duration-300 text-center group"
                        :class="selectedAmount === card.amount ? 'border-black bg-black text-white' : 'border-black/10 hover:border-black/30'">
                        <span v-if="card.popular"
                            class="absolute top-3 right-3 text-[9px] tracking-[0.2em] uppercase font-medium"
                            :class="selectedAmount === card.amount ? 'text-white/60' : 'text-black/30'">Popular</span>
                        <p class="text-2xl font-light">${{ card.amount }}</p>
                    </button>
                </div>

                <!-- Custom Amount -->
                <div class="border border-black/10 p-8 mb-12">
                    <h3 class="text-sm font-medium tracking-wider uppercase text-black/50 mb-4">Custom Amount</h3>
                    <div class="flex items-center gap-4">
                        <span class="text-2xl font-light text-black/30">$</span>
                        <input v-model="customAmount" type="number" min="10" max="1000" placeholder="Enter amount"
                            class="flex-1 text-2xl font-light bg-transparent border-b border-black/10 pb-2 focus:outline-none focus:border-black transition-colors"
                            @focus="selectedAmount = null" />
                    </div>
                </div>

                <!-- Recipient Info -->
                <div class="border border-black/10 p-8 space-y-6">
                    <h3 class="text-sm font-medium tracking-wider uppercase text-black/50 mb-2">Recipient Details</h3>
                    <div class="grid md:grid-cols-2 gap-6">
                        <div>
                            <label class="text-xs text-black/40 tracking-wider uppercase mb-2 block">Recipient
                                Email</label>
                            <input v-model="recipientEmail" type="email" placeholder="email@example.com"
                                class="w-full bg-transparent border-b border-black/10 pb-2 focus:outline-none focus:border-black transition-colors text-sm" />
                        </div>
                        <div>
                            <label class="text-xs text-black/40 tracking-wider uppercase mb-2 block">Your Name</label>
                            <input v-model="senderName" type="text" placeholder="Your name"
                                class="w-full bg-transparent border-b border-black/10 pb-2 focus:outline-none focus:border-black transition-colors text-sm" />
                        </div>
                    </div>
                    <div>
                        <label class="text-xs text-black/40 tracking-wider uppercase mb-2 block">Personal
                            Message</label>
                        <textarea v-model="message" rows="3" placeholder="Add a personal message (optional)"
                            class="w-full bg-transparent border-b border-black/10 pb-2 focus:outline-none focus:border-black transition-colors text-sm resize-none"></textarea>
                    </div>
                </div>

                <!-- Add to Cart Button -->
                <div class="mt-12 text-center">
                    <button
                        class="inline-flex items-center gap-3 px-12 py-4 bg-black text-white text-sm font-medium tracking-wider uppercase hover:bg-black/90 transition-all duration-300">
                        <Gift class="w-4 h-4" />
                        Add Gift Card to Cart
                    </button>
                </div>
            </div>
        </section>

        <!-- Features -->
        <section class="py-20 bg-neutral-50 border-t border-black/5">
            <div class="max-w-screen-lg mx-auto px-6">
                <div class="grid grid-cols-1 md:grid-cols-3 gap-12 text-center">
                    <div>
                        <Mail class="w-5 h-5 text-black/30 mx-auto mb-4" />
                        <h3 class="text-sm font-medium mb-2">Instant Delivery</h3>
                        <p class="text-xs text-black/40 leading-relaxed">Sent directly to their inbox within minutes
                        </p>
                    </div>
                    <div>
                        <CreditCard class="w-5 h-5 text-black/30 mx-auto mb-4" />
                        <h3 class="text-sm font-medium mb-2">Never Expires</h3>
                        <p class="text-xs text-black/40 leading-relaxed">No expiration date, use it whenever you're
                            ready</p>
                    </div>
                    <div>
                        <Sparkles class="w-5 h-5 text-black/30 mx-auto mb-4" />
                        <h3 class="text-sm font-medium mb-2">Easy to Use</h3>
                        <p class="text-xs text-black/40 leading-relaxed">Simply enter the code at checkout to redeem</p>
                    </div>
                </div>
            </div>
        </section>
    </div>
</template>
