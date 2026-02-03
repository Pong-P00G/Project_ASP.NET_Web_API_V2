<script setup>
import { ref } from 'vue';
import {
    Mail,
    Phone,
    MapPin,
    Clock,
    Send,
    ArrowRight
} from 'lucide-vue-next';

// Form State
const formData = ref({
    name: '',
    email: '',
    message: ''
});

const formSubmitted = ref(false);
const isSubmitting = ref(false);

// Contact Information
const contactInfo = ref([
    {
        icon: Phone,
        title: 'Call Us',
        details: '+855 (000) 000-000',
        description: 'Mon-Fri from 8am to 6pm',
    },
    {
        icon: Mail,
        title: 'Email Us',
        details: 'alieeshop@example.com',
        description: 'We reply within 24 hours',
    },
    {
        icon: MapPin,
        title: 'Visit Us',
        details: '123 St, Phnom Penh, Cambodia',
        description: 'Come say hello',
    },
]);

// Form Submission
const submitForm = async () => {
    isSubmitting.value = true;

    // Simulate API call
    await new Promise(resolve => setTimeout(resolve, 1500));

    console.log('Form submitted:', formData.value);

    formSubmitted.value = true;
    isSubmitting.value = false;

    // Reset form
    formData.value = {
        name: '',
        email: '',
        message: ''
    };

    // Reset success message after 5 seconds
    setTimeout(() => {
        formSubmitted.value = false;
    }, 5000);
};
</script>

<template>
    <div class="min-h-screen bg-white">
        <!-- Hero Section -->
        <section class="py-32 bg-white overflow-hidden mt-6">
            <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                <div class="text-center space-y-8 max-w-4xl mx-auto">
                    <div class="inline-flex items-center gap-2 px-3 py-1 bg-gray-100 rounded-full text-gray-600 text-xs font-bold tracking-wider uppercase">
                        <Mail class="w-3 h-3" />
                        Let's Connect
                    </div>
                    <h1 class="text-5xl md:text-6xl font-light text-gray-900 tracking-tight">
                        Let's <span class="font-bold italic">Connect</span>
                    </h1>
                    <p class="text-xl text-gray-500 leading-relaxed font-light max-w-3xl mx-auto">
                        Have a question or need assistance? Our team is here to help you.
                    </p>
                </div>
            </div>
        </section>

        <!-- Contact Section -->
        <section class="py-32 bg-white">
            <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                <div class="grid lg:grid-cols-2 gap-24">
                    <div class="space-y-12">
                        <div class="space-y-4">
                            <h2 class="text-5xl font-light text-gray-900 tracking-tight">
                                Get in <span class="font-bold">Touch</span>
                            </h2>
                            <p class="text-gray-500 text-xl font-light">
                                We'd love to hear from you. Send us a message and we'll respond as soon as possible.
                            </p>
                        </div>
                        <div class="space-y-8">
                            <div v-for="info in contactInfo" :key="info.title" class="flex items-center gap-6 group">
                                <div class="w-14 h-14 rounded-2xl bg-gray-50 flex items-center justify-center text-gray-400 group-hover:bg-violet-50 group-hover:text-violet-600 transition-all duration-300">
                                    <component :is="info.icon" class="w-7 h-7" />
                                </div>
                                <div>
                                    <div class="text-sm text-gray-400 uppercase tracking-widest font-bold mb-1">{{ info.title }}</div>
                                    <div class="text-xl font-bold text-gray-900">{{ info.details }}</div>
                                    <div class="text-sm text-gray-500 mt-1">{{ info.description }}</div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="bg-gray-50/50 rounded-[3rem] p-12 md:p-16">
                        <form @submit.prevent="submitForm" class="space-y-6">
                            <!-- Success Message -->
                            <transition name="fade">
                                <div v-if="formSubmitted"
                                    class="mb-6 p-5 bg-green-50 border border-green-200 rounded-2xl flex items-center gap-4">
                                    <div class="w-6 h-6 bg-green-500 rounded-full flex items-center justify-center shrink-0">
                                        <div class="w-2 h-2 bg-white rounded-full"></div>
                                    </div>
                                    <p class="text-green-800 font-semibold">Thank you! Your message has been sent successfully.</p>
                                </div>
                            </transition>

                            <div class="grid md:grid-cols-2 gap-6">
                                <div class="space-y-2">
                                    <label class="text-xs font-bold uppercase tracking-widest text-gray-400 ml-4">Name</label>
                                    <input v-model="formData.name" type="text" placeholder="John Doe" required
                                        class="w-full bg-white border-0 rounded-2xl px-6 py-4 focus:ring-2 focus:ring-violet-500 transition-all shadow-sm" />
                                </div>
                                <div class="space-y-2">
                                    <label class="text-xs font-bold uppercase tracking-widest text-gray-400 ml-4">Email</label>
                                    <input v-model="formData.email" type="email" placeholder="john@example.com" required
                                        class="w-full bg-white border-0 rounded-2xl px-6 py-4 focus:ring-2 focus:ring-violet-500 transition-all shadow-sm" />
                                </div>
                            </div>
                            <div class="space-y-2">
                                <label class="text-xs font-bold uppercase tracking-widest text-gray-400 ml-4">Message</label>
                                <textarea v-model="formData.message" placeholder="How can we help?" rows="5" required
                                    class="w-full bg-white border-0 rounded-2xl px-6 py-4 focus:ring-2 focus:ring-violet-500 transition-all shadow-sm resize-none"></textarea>
                            </div>
                            <button type="submit" :disabled="isSubmitting"
                                class="w-full bg-gray-900 text-white font-bold py-5 rounded-2xl hover:bg-black transition-all duration-300 flex items-center justify-center gap-3 group shadow-xl disabled:opacity-50 disabled:cursor-not-allowed">
                                <span v-if="!isSubmitting">Send Message</span>
                                <span v-else>Sending...</span>
                                <ArrowRight class="w-5 h-5 group-hover:translate-x-1 transition-transform" />
                            </button>
                        </form>
                    </div>
                </div>
            </div>
        </section>

        <!-- Map Section -->
        <section class="py-24 bg-gray-50/50">
            <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                <div class="bg-white rounded-3xl overflow-hidden shadow-sm border border-gray-100">
                    <div class="aspect-video bg-gray-200 relative">
                        <iframe
                            src="https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d125421.41243287174!2d104.8134!3d11.5564!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x3109513dc76a6be3%3A0x9c010ee85ab525bb!2sPhnom%20Penh%2C%20Cambodia!5e0!3m2!1sen!2s!4v1234567890"
                            width="100%" height="100%" style="border:0;" allowfullscreen="" loading="lazy"
                            class="absolute inset-0"></iframe>
                    </div>
                    <div class="p-8">
                        <h3 class="text-2xl font-bold text-gray-900 mb-2">Visit Our Office</h3>
                        <p class="text-gray-500">
                            123 St, Phnom Penh, Cambodia
                        </p>
                    </div>
                </div>
            </div>
        </section>
    </div>
</template>

<style scoped>
.fade-enter-active,
.fade-leave-active {
    transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
    opacity: 0;
}
</style>
