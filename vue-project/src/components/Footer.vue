<script setup>
import { ref } from 'vue';
import { RouterLink } from 'vue-router';
import {
    Mail,
    Phone,
    MapPin,
    Facebook,
    Instagram,
    Twitter,
    ArrowUp
} from 'lucide-vue-next';

const currentYear = new Date().getFullYear();

const scrollToTop = () => {
    window.scrollTo({ top: 0, behavior: 'smooth' });
};

const footerLinks = {
    shop: [
        { name: 'New Arrivals', to: '/product?filter=new' },
        { name: 'Best Sellers', to: '/product?filter=bestsellers' },
        { name: 'Sale', to: '/product?filter=sale' },
        { name: 'Gift Cards', to: '/gift-cards' },
    ],
    help: [
        { name: 'Track Order', to: '/track-order' },
        { name: 'Returns & Exchanges', to: '/returns' },
        { name: 'Shipping Info', to: '/shipping' },
        { name: 'FAQ', to: '/faq' },
    ],
    company: [
        { name: 'About Us', to: '/about' },
        { name: 'Contact', to: '/contact' },
        { name: 'Careers', to: '/careers' },
        { name: 'Press', to: '/press' },
    ],
};

const socialLinks = [
    { icon: Facebook, href: 'https://facebook.com', label: 'Facebook', color: 'hover:bg-blue-600' },
    { icon: Instagram, href: 'https://instagram.com', label: 'Instagram', color: 'hover:bg-gradient-to-br hover:from-purple-600 hover:to-pink-600' },
    { icon: Twitter, href: 'https://twitter.com', label: 'Twitter', color: 'hover:bg-sky-500' },
];

</script>

<template>
    <footer class="bg-gray-100/50 pt-20 pb-10 border-t border-gray-100">
        <div class="max-w-screen-2xl mx-auto px-4 sm:px-6 lg:px-8">
            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-12 mb-20">
                <!-- Brand -->
                <div class="space-y-6">
                    <RouterLink to="/" class="inline-block">
                        <span class="text-2xl font-black tracking-tighter text-gray-900">Aliee<span
                                style="color: #C8A97E;">Shop</span></span>
                    </RouterLink>
                    <p class="text-gray-500 text-sm leading-relaxed max-w-xs">
                        Redefining modern fashion with quality, sustainability, and timeless style. Join us in shaping
                        the future of retail.
                    </p>
                    <div class="flex gap-4">
                        <a v-for="social in socialLinks" :key="social.label" :href="social.href" target="_blank"
                            class="text-gray-400 hover:text-gray-900 transition-colors">
                            <component :is="social.icon" class="w-5 h-5" />
                        </a>
                    </div>
                </div>

                <!-- Links -->
                <div>
                    <h4 class="font-bold text-gray-900 mb-6">Shop</h4>
                    <ul class="space-y-4 text-sm text-gray-500">
                        <li v-for="link in footerLinks.shop" :key="link.name">
                            <RouterLink :to="link.to" class="hover:text-gray-900 transition-colors">{{ link.name }}
                            </RouterLink>
                        </li>
                    </ul>
                </div>

                <div>
                    <h4 class="font-bold text-gray-900 mb-6">Company</h4>
                    <ul class="space-y-4 text-sm text-gray-500">
                        <li v-for="link in footerLinks.company" :key="link.name">
                            <RouterLink :to="link.to" class="hover:text-gray-900 transition-colors">{{ link.name }}
                            </RouterLink>
                        </li>
                    </ul>
                </div>

                <div>
                    <h4 class="font-bold text-gray-900 mb-6">Support</h4>
                    <ul class="space-y-4 text-sm text-gray-500">
                        <li v-for="link in footerLinks.help" :key="link.name">
                            <RouterLink :to="link.to" class="hover:text-gray-900 transition-colors">{{ link.name }}
                            </RouterLink>
                        </li>
                    </ul>
                </div>
            </div>

            <!-- Bottom -->
            <div class="pt-8 border-t border-gray-200 flex flex-col md:flex-row justify-between items-center gap-4">
                <p class="text-xs text-gray-500">© {{ currentYear }} AlieeShop Inc. All rights reserved.</p>
                <div class="flex gap-6">
                    <a href="#" class="text-xs text-gray-500 hover:text-gray-900 transition-colors">Privacy Policy</a>
                    <a href="#" class="text-xs text-gray-500 hover:text-gray-900 transition-colors">Terms of Service</a>
                    <a href="#" class="text-xs text-gray-500 hover:text-gray-900 transition-colors">Cookies Settings</a>
                </div>
            </div>
        </div>
    </footer>
</template>

<style scoped>
/* Smooth scroll behavior */
html {
    scroll-behavior: smooth;
}
</style>