import { createWebHistory, createRouter } from "vue-router";
import MainLayout from "../layout/mainLayout.vue";
import DashLayout from "../layout/dashLayout.vue";
import { useAuthStore } from "../stores/Auth.js";

const routes = [
    {
        path: "/",
        component: MainLayout,
        children: [
            {
                name: "Homepage",
                path: "/",
                component: () => import("../views/HomePage.vue"),
                meta: {
                    // requiresAuth: true,
                    // role: 'customer',
                    title: 'Welcome to AlieeShop - Your One-Stop Online Store',
                    description: 'Discover a wide range of products at unbeatable prices. Shop now and enjoy fast shipping and excellent customer service at AlieeShop!',
                }
            },
            {
                name: "Product",
                path: "/product",
                component: () => import("../views/ProductsPage.vue")
            },
            {
                path: '/product/:id',
                name: 'ProductDetail',
                component: () => import('../views/ProductDetail.vue'),
                meta: {
                    title: 'Product Details - AlieeShop'
                }
            },
            {
                name: "About",
                path: "/about",
                component: () => import("../views/AboutPage.vue")
            },
            {
                name: "Contact",
                path: "/contact",
                component: () => import("../views/ContactPage.vue")
            },
            {
                name: 'Cart',
                path: '/cart',
                component: () => import("../views/ShoppingCart.vue")
            },
            {
                name: 'Checkout',
                path: '/checkout',
                component: () => import("../views/CheckoutPage.vue"),
                meta: {
                    title: 'Secure Checkout - AlieeShop'
                }
            },
            {
                name: 'OrderSuccess',
                path: '/order-success/:id',
                component: () => import("../views/OrderSuccessPage.vue"),
                meta: {
                    title: 'Order Confirmed - AlieeShop',
                    requiresAuth: true
                }
            },
            {
                name: 'Wishlist',
                path: '/wishlist',
                component: () => import('../views/WishListPage.vue')
            },
            {
                name: 'Profile',
                path: '/profile',
                component: () => import('../views/ProfilePage.vue'),
                // meta: { requiresAuth: true }
            },
            {
                name: 'Settings',
                path: '/settings',
                component: () => import('../views/SettingsPage.vue'),
                // meta: { requiresAuth: true }
            },
            {
                path: '/login',
                name: 'Login',
                component: () => import("../views/auth/Login.vue"),
                // meta: { guest: true } // Only accessible when not logged in
            },
            {
                path: '/register',
                name: 'Register',
                component: () => import("../views/auth/Register.vue"),
                // meta: { guest: true }
            },
            {
                path: '/forgot-password',
                name: 'ForgotPassword',
                component: () => import("../views/auth/ForgetPassword.vue"),
                meta: { guest: true }
            },
            {
                path: '/reset-password',
                name: 'ResetPassword',
                component: () => import("../views/auth/ResetPassword.vue"),
                meta: { guest: true }
            },
            {
                path: '/terms',
                name: 'TermsAndPrivacy',
                component: () => import('../views/auth/TermsAndPrivacy.vue')
            },
            {
                path: '/contact-support',
                name: 'ContactSupport',
                component: () => import('../views/support/ContactSupport.vue')
            },
            {
                path: '/gift-cards',
                name: 'GiftCards',
                component: () => import('../views/GiftCardsPage.vue'),
                meta: { title: 'Gift Cards - AlieeShop' }
            },
            {
                path: '/track-order',
                name: 'TrackOrder',
                component: () => import('../views/TrackOrderPage.vue'),
                meta: { title: 'Track Your Order - AlieeShop' }
            },
            {
                path: '/returns',
                name: 'Returns',
                component: () => import('../views/ReturnsPage.vue'),
                meta: { title: 'Returns & Exchanges - AlieeShop' }
            },
            {
                path: '/shipping',
                name: 'ShippingInfo',
                component: () => import('../views/ShippingInfoPage.vue'),
                meta: { title: 'Shipping Information - AlieeShop' }
            },
            {
                path: '/faq',
                name: 'FAQ',
                component: () => import('../views/FaqPage.vue'),
                meta: { title: 'FAQ - AlieeShop' }
            },
            {
                path: '/careers',
                name: 'Careers',
                component: () => import('../views/CareersPage.vue'),
                meta: { title: 'Careers - AlieeShop' }
            },
            {
                path: '/press',
                name: 'Press',
                component: () => import('../views/PressPage.vue'),
                meta: { title: 'Press - AlieeShop' }
            },
            {
                path: '/:pathMatch(.*)*',
                name: 'NotFound',
                component: () => import('../views/NotFound.vue'),
                meta: {
                    title: '404 - Page Not Found'
                }
            }
        ]
    },
    {
        path: "/admin",
        component: DashLayout,
        meta: { requiresAuth: true, role: 'admin' },
        children: [
            {
                path: '/admin/dashboard',
                name: 'AdminDashboard',
                component: () => import('../views/admin/DashboardPage.vue'),
            },
            {
                path: '/admin/product',
                name: 'AdminProducts',
                component: () => import('../views/admin/ManageProduct.vue'),
            },
            {
                path: '/admin/user',
                name: 'AdminUsers',
                component: () => import('../views/admin/ManageUser.vue'),
            },
            {
                path: '/admin/orders',
                name: 'AdminOrders',
                component: () => import('../views/admin/OrderManagement.vue'),
            },
            {
                path: '/admin/stock',
                name: 'AdminStock',
                component: () => import('../views/admin/Stock.vue'),
            }
        ]
    }
];

const router = createRouter({
    history: createWebHistory(),
    routes,
    scrollBehavior() {
        return {
            top: 0,
            behavior: 'smooth'
        };
    }
});

// Navigation guards
router.beforeEach((to, from, next) => {
    const authStore = useAuthStore();
    // Initialize auth state from local storage if needed
    if (!authStore.user) {
        authStore.initializeAuth();
    }
    
    const isAuthenticated = authStore.isAuthenticated;
    // Use the getter from the store, or fallback to properties
    const userRole = authStore.userRole || authStore.user?.roleName || authStore.user?.role;
    
    // Normalize role for comparison (backend might send 'Admin', route expects 'admin')
    const normalizedUserRole = userRole ? String(userRole).toLowerCase() : null;

    // Update page title
    if (to.meta.title) {
        document.title = to.meta.title;
    } else {
        document.title = 'AlieeShop'; // Default title
    }
    // Update meta description
    if (to.meta.description) {
        const metaDescription = document.querySelector('meta[name="description"]');
        if (metaDescription) {
            metaDescription.setAttribute('content', to.meta.description);
        }
    }
    // Guest only pages (login, register, forgot password)
    if (to.meta.guest && isAuthenticated) {
        // If user is already logged in, redirect based on role
        if (normalizedUserRole === 'admin') {
            return next('/admin/dashboard');
        } else {
            return next('/');
        }
    }

    // Protected routes
    if (to.meta.requiresAuth && !isAuthenticated) {
        return next('/login');
    }

    // Role-based access
    if (to.meta.role) {
        const requiredRole = String(to.meta.role).toLowerCase();
        
        if (normalizedUserRole !== requiredRole) {
            // User doesn't have the required role
            if (normalizedUserRole === 'admin') {
                return next('/admin/dashboard');
            } else if (normalizedUserRole === 'customer') {
                return next('/');
            } else {
                return next('/login');
            }
        }
    }

    next();
});

export default router;