<script setup>
import { ref, computed, onMounted } from 'vue'
import {
  Package, Plus, Search, Edit2, Trash2, X, Loader2, Download, Eye, AlertCircle, CheckCircle, RefreshCw, Tag, DollarSign, Boxes, ArrowUpDown, Upload, Image
} from 'lucide-vue-next'
import { productAPI } from '../../api/productsApi'
import { useToast } from '../../composables/useToast'

const toast = useToast()

const products = ref([])
const loading = ref(false)
const searchTerm = ref('')
const selectedCategory = ref('all')
const sortBy = ref('name')
const sortOrder = ref('asc')
const showAddModal = ref(false)
const showEditModal = ref(false)
const showViewModal = ref(false)
const showFullDescription = ref(false)
const currentProduct = ref(null)
const formData = ref({ name: '', description: '', price: 0, stock: 0, category: '', images: [], discount: 0, supplier: '', sku: '' })
const categories = ref([])
const uploadingImage = ref(false)
const mainImageIndex = ref(0) // Index of the main/primary image

// Category input
const selectedCategories = ref([])
const newCategoryInput = ref('')

// Variant options (dynamic - can add custom)
const selectedColors = ref([])
const selectedSizes = ref([])
const colorOptions = ref([
  { label: 'Black', value: 'Black' },
  { label: 'White', value: 'White' },
  { label: 'Red', value: 'Red' },
  { label: 'Blue', value: 'Blue' },
  { label: 'Green', value: 'Green' }
])
const sizeOptions = ref([
  { label: 'Small (S)', value: 'S' },
  { label: 'Medium (M)', value: 'M' },
  { label: 'Large (L)', value: 'L' },
  { label: 'XL', value: 'XL' },
])
const newColorInput = ref('')
const newSizeInput = ref('')

// Per-variant pricing - stores individual price and discount for each variant
const variantPrices = ref({})

// Generate variant combinations
const generatedVariants = computed(() => {
  const variants = []
  const colors = selectedColors.value.length > 0 ? selectedColors.value : [null]
  const sizes = selectedSizes.value.length > 0 ? selectedSizes.value : [null]

  for (const color of colors) {
    for (const size of sizes) {
      const options = []
      let label = ''
      if (color) {
        options.push({ variant: 'Color', value: color })
        label += color
      }
      if (size) {
        options.push({ variant: 'Size', value: size })
        label += (label ? ' - ' : '') + size
      }
      if (options.length > 0) {
        const key = label || 'Default'
        // Initialize price data if not exists
        if (!variantPrices.value[key]) {
          variantPrices.value[key] = {
            price: formData.value.price || 0,
            discountPrice: null,
            stockQuantity: formData.value.stock || 0
          }
        }
        variants.push({
          key,
          label: label || 'Default',
          options,
          ...variantPrices.value[key]
        })
      }
    }
  }
  return variants
})

// Update variant price
const updateVariantPrice = (key, field, value) => {
  if (!variantPrices.value[key]) {
    variantPrices.value[key] = { price: 0, discountPrice: null, stockQuantity: 0 }
  }
  variantPrices.value[key][field] = value
}

// Delete a variant by removing its color/size from selections
const deleteVariant = (variant) => {
  // Remove from variantPrices
  delete variantPrices.value[variant.key]

  // Find which color and size this variant has and remove them if they only belong to this variant
  const variantOptions = variant.options

  for (const opt of variantOptions) {
    if (opt.variant === 'Color') {
      // Check if this color is used by other variants
      const otherVariantsWithColor = generatedVariants.value.filter(
        v => v.key !== variant.key && v.options.some(o => o.variant === 'Color' && o.value === opt.value)
      )
      if (otherVariantsWithColor.length === 0) {
        selectedColors.value = selectedColors.value.filter(c => c !== opt.value)
      }
    }
    if (opt.variant === 'Size') {
      // Check if this size is used by other variants
      const otherVariantsWithSize = generatedVariants.value.filter(
        v => v.key !== variant.key && v.options.some(o => o.variant === 'Size' && o.value === opt.value)
      )
      if (otherVariantsWithSize.length === 0) {
        selectedSizes.value = selectedSizes.value.filter(s => s !== opt.value)
      }
    }
  }
}

const stats = computed(() => ({
  total: products.value.length,
  active: products.value.filter(p => p.stock > 0).length,
  outOfStock: products.value.filter(p => p.stock === 0).length,
  totalValue: products.value.reduce((sum, p) => sum + (p.price * p.stock), 0)
}))

const filteredProducts = computed(() => {
  let filtered = products.value
  if (searchTerm.value) filtered = filtered.filter(p => p.name.toLowerCase().includes(searchTerm.value.toLowerCase()))
  if (selectedCategory.value !== 'all') filtered = filtered.filter(p => p.category === selectedCategory.value)
  return [...filtered].sort((a, b) => {
    const av = a[sortBy.value], bv = b[sortBy.value];
    if (typeof av === 'number') return sortOrder.value === 'asc' ? av - bv : bv - av;
    return sortOrder.value === 'asc' ? String(av).localeCompare(String(bv)) : String(bv).localeCompare(String(av))
  })
})

const fetchProducts = async () => {
  try {
    loading.value = true;
    const res = await productAPI.getAllProducts({ pageSize: 100 });
    products.value = (res.data.items || []).map(p => ({
      id: p.productId,
      name: p.productName,
      description: p.description,
      category: p.categories?.[0]?.categoryName || 'N/A',
      allCategories: p.categories?.map(c => c.categoryName) || [],
      price: p.basePrice,
      stock: p.stock,
      image: p.images?.[0]?.imageUrl || '',
      images: p.images?.map(img => img.imageUrl) || [],
      sku: p.sku || '',
      supplier: p.supplier || '',
      discount: p.discount || 0,
      variants: p.variants || []
    }));
    categories.value = [...new Set(products.value.map(p => p.category))].filter(c => c && c !== 'N/A')
  } catch (err) { toast.error('Failed to load products') } finally { loading.value = false }
}

const openAddModal = () => {
  formData.value = { name: '', description: '', price: 0, stock: 0, category: '', images: [], discount: 0 };
  mainImageIndex.value = 0;
  selectedColors.value = [];
  selectedSizes.value = [];
  selectedCategories.value = [];
  newCategoryInput.value = '';
  newColorInput.value = '';
  newSizeInput.value = '';
  showAddModal.value = true
}
const openEditModal = (p) => {
  currentProduct.value = p;
  formData.value = {
    ...p,
    sku: p.sku || '',
    supplier: p.supplier || '',
    images: p.images && p.images.length > 0
      ? p.images.map(url => ({ url, preview: url }))
      : (p.image ? [{ url: p.image, preview: p.image }] : []),
    discount: p.discount || 0
  };
  mainImageIndex.value = 0;

  // Populate categories
  selectedCategories.value = p.allCategories && p.allCategories.length > 0
    ? [...p.allCategories]
    : (p.category && p.category !== 'N/A' ? [p.category] : []);

  // Populate variants
  selectedColors.value = [];
  selectedSizes.value = [];
  variantPrices.value = {};

  if (p.variants && p.variants.length > 0) {
    p.variants.forEach(v => {
      // Extract options
      const colorOpt = v.options.find(o => o.variant === 'Color');
      const sizeOpt = v.options.find(o => o.variant === 'Size');

      const color = colorOpt ? colorOpt.value : null;
      const size = sizeOpt ? sizeOpt.value : null;

      if (color && !selectedColors.value.includes(color)) selectedColors.value.push(color);
      if (size && !selectedSizes.value.includes(size)) selectedSizes.value.push(size);

      // Construct key consistent with generatedVariants logic
      let label = '';
      if (color) label += color;
      if (size) label += (label ? ' - ' : '') + size;
      const key = label || 'Default';

      // Populate prices
      variantPrices.value[key] = {
        price: v.price,
        discountPrice: v.discountPrice,
        stockQuantity: v.stockQuantity
      };
    });
  }

  showEditModal.value = true
}
const openViewModal = (p) => {
  currentProduct.value = p;
  showFullDescription.value = false;
  showViewModal.value = true
}
const closeModals = () => {
  showAddModal.value = false;
  showEditModal.value = false;
  showViewModal.value = false;
  currentProduct.value = null;
  formData.value.images = [];
  formData.value.supplier = '';
  formData.value.sku = '';
  mainImageIndex.value = 0;
  selectedColors.value = [];
  selectedSizes.value = [];
  selectedCategories.value = [];
  newCategoryInput.value = '';
  newColorInput.value = '';
  newSizeInput.value = '';
  variantPrices.value = {};
}

// Multi-image upload handler
const handleImageSelect = async (event) => {
  const files = Array.from(event.target.files)
  if (!files.length) return

  const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp']

  for (const file of files) {
    // Validate file type
    if (!allowedTypes.includes(file.type)) {
      toast.error(`Invalid file type: ${file.name}`)
      continue
    }

    // Validate file size (5MB max)
    if (file.size > 5 * 1024 * 1024) {
      toast.error(`File too large: ${file.name}`)
      continue
    }

    // Create local preview first
    const reader = new FileReader()
    reader.onload = async (e) => {
      const tempImage = { preview: e.target.result, url: '', uploading: true }
      formData.value.images.push(tempImage)
      const imageIndex = formData.value.images.length - 1

      // Upload image
      try {
        uploadingImage.value = true
        const res = await productAPI.uploadImage(file)
        formData.value.images[imageIndex].url = res.data.url
        formData.value.images[imageIndex].uploading = false
      } catch (err) {
        toast.error(`Failed to upload: ${file.name}`)
        formData.value.images.splice(imageIndex, 1)
      } finally {
        uploadingImage.value = false
      }
    }
    reader.readAsDataURL(file)
  }

  // Reset file input
  event.target.value = ''
}

// Remove image at index
const removeImage = (index) => {
  formData.value.images.splice(index, 1)
  // Adjust main image index if needed
  if (mainImageIndex.value >= formData.value.images.length) {
    mainImageIndex.value = Math.max(0, formData.value.images.length - 1)
  } else if (index < mainImageIndex.value) {
    mainImageIndex.value--
  }
}

// Set image as main
const setMainImage = (index) => {
  mainImageIndex.value = index
}

// Add custom color
const addCustomColor = () => {
  const color = newColorInput.value.trim()
  if (color && !colorOptions.value.find(c => c.value.toLowerCase() === color.toLowerCase())) {
    colorOptions.value.push({ label: color, value: color })
    selectedColors.value.push(color)
    newColorInput.value = ''
  }
}

// Add custom size
const addCustomSize = () => {
  const size = newSizeInput.value.trim()
  if (size && !sizeOptions.value.find(s => s.value.toLowerCase() === size.toLowerCase())) {
    sizeOptions.value.push({ label: size, value: size })
    selectedSizes.value.push(size)
    newSizeInput.value = ''
  }
}

// Add custom category
const addCustomCategory = () => {
  const category = newCategoryInput.value.trim()
  if (category && !selectedCategories.value.includes(category)) {
    selectedCategories.value.push(category)
    newCategoryInput.value = ''
  }
}

// Remove category
const removeCategory = (category) => {
  selectedCategories.value = selectedCategories.value.filter(c => c !== category)
}

const handleAddProduct = async () => {
  try {
    loading.value = true;

    // Generate variants based on selections
    let variants = [];
    if (generatedVariants.value.length > 0) {
      variants = generatedVariants.value.map((v, idx) => ({
        sku: `SKU-${Date.now()}-${idx}`,
        price: v.price || formData.value.price,
        stockQuantity: v.stockQuantity || formData.value.stock,
        discountPrice: v.discountPrice || null,
        isActive: true,
        options: v.options
      }));
    } else {
      // Default variant if no options selected
      variants = [{
        sku: `SKU-${Date.now()}`,
        price: formData.value.price,
        stockQuantity: formData.value.stock,
        discountPrice: null,
        isActive: true,
        options: [{ variant: 'Default', value: 'Standard' }]
      }];
    }

    // Build image URLs with main image first
    const imageUrls = formData.value.images.length > 0
      ? [
        formData.value.images[mainImageIndex.value]?.url,
        ...formData.value.images.filter((_, i) => i !== mainImageIndex.value).map(img => img.url)
      ].filter(Boolean)
      : [];

    await productAPI.createProducts({
      productName: formData.value.name,
      description: formData.value.description,
      basePrice: formData.value.price,
      stock: formData.value.stock,
      supplier: formData.value.supplier || null,
      sku: formData.value.sku || null,
      imageUrls,
      categoryNames: selectedCategories.value.length > 0 ? selectedCategories.value : null,
      variants
    });
    await fetchProducts();
    toast.success('Product added successfully!')
    closeModals()
  }
  catch (err) { toast.error(err.response?.data?.message || 'Failed to add product') }
  finally { loading.value = false }
}

const handleUpdateProduct = async () => {
  try {
    loading.value = true;

    // Generate variants payload (only if user added/changed variants)
    let variants = null;
    if (generatedVariants.value.length > 0) {
      variants = generatedVariants.value.map((v, idx) => ({
        sku: `SKU-${Date.now()}-${idx}`,
        price: v.price || formData.value.price,
        stockQuantity: v.stockQuantity || formData.value.stock,
        discountPrice: v.discountPrice || null,
        isActive: true,
        options: v.options.map(opt => ({
          variant: opt.variant,
          value: opt.value
        }))
      }));
    }

    // Build image URLs with main image first
    const imageUrls = formData.value.images.length > 0
      ? [
        formData.value.images[mainImageIndex.value]?.url,
        ...formData.value.images.filter((_, i) => i !== mainImageIndex.value).map(img => img.url)
      ].filter(Boolean)
      : [];

    await productAPI.updateProducts(currentProduct.value.id, {
      productName: formData.value.name,
      description: formData.value.description,
      basePrice: formData.value.price,
      stock: formData.value.stock,
      sku: formData.value.sku || null,
      supplier: formData.value.supplier || null,
      imageUrls,
      categoryNames: selectedCategories.value.length > 0 ? selectedCategories.value : null,
      variants
    });
    await fetchProducts();
    toast.success('Product updated successfully!')
    closeModals()
  } catch (err) {
    toast.error(err.response?.data?.message || 'Failed to update product')
  } finally {
    loading.value = false
  }
}

const handleDeleteProduct = async (id) => {
  if (!confirm('Delete this product?')) return
  try {
    loading.value = true;
    await productAPI.hardDelete(id);
    products.value = products.value.filter(p => p.id !== id);
    toast.success('Product deleted successfully!')
  }
  catch (err) { toast.error('Failed to delete product') }
  finally { loading.value = false }
}

const toggleSort = (f) => {
  if (sortBy.value === f) sortOrder.value = sortOrder.value === 'asc' ? 'desc' : 'asc';
  else { sortBy.value = f; sortOrder.value = 'asc' }
}
const exportToCSV = () => {
  const csv = [['ID', 'Name', 'Category', 'Price', 'Stock'],
  ...products.value.map(p => [p.id, p.name, p.category, p.price, p.stock])].map(r => r.join(',')).join('\n');
  const b = new Blob([csv], { type: 'text/csv' });
  const a = document.createElement('a'); a.href = URL.createObjectURL(b); a.download = 'products.csv'; a.click()
}

onMounted(fetchProducts)

</script>

<template>
  <div class="w-full">
    <!-- Header -->
    <div class="flex flex-wrap items-center justify-between gap-4 mb-8">
      <div>
        <div
          class="inline-flex items-center gap-2 px-4 py-2 bg-linear-to-r from-orange-500/20 to-pink-500/20 backdrop-blur-sm border border-orange-500/20 rounded-full text-orange-400 text-xs font-semibold mb-3">
          <Package class="w-3.5 h-3.5" /><span>Product Management</span>
        </div>
        <h1 class="text-3xl font-black text-white tracking-tight">Products</h1>
        <p class="text-neutral-400 text-sm mt-1">Manage your product catalog</p>
      </div>
      <div class="flex gap-3">
        <button @click="fetchProducts" :disabled="loading"
          class="flex items-center gap-2 px-5 py-3 bg-white/5 backdrop-blur-sm border border-white/10 rounded-xl text-sm font-semibold text-white hover:bg-white/10 transition-all">
          <RefreshCw class="w-4 h-4" :class="{ 'animate-spin': loading }" />Sync
        </button>
        <button @click="openAddModal"
          class="group flex items-center gap-2 px-6 py-3 bg-linear-to-r from-orange-500 to-pink-500 rounded-xl text-sm font-bold text-white shadow-lg shadow-orange-500/25 hover:shadow-orange-500/40 hover:scale-105 transition-all duration-300">
          <Plus class="w-4 h-4" />New Product
        </button>
      </div>
    </div>

    <!-- Stats -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5 mb-8">
      <div
        class="stat-card group relative bg-white/5 backdrop-blur-xl rounded-2xl p-6 border border-white/10 hover:border-orange-500/50 hover:-translate-y-2 hover:shadow-2xl hover:shadow-orange-500/10 transition-all duration-500">
        <div
          class="absolute inset-0 bg-linear-to-br from-orange-500/10 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-500 rounded-2xl">
        </div>
        <div class="relative z-10 flex items-center gap-4">
          <div
            class="w-14 h-14 flex items-center justify-center bg-linear-to-br from-orange-400 to-pink-500 rounded-2xl text-white shadow-lg shadow-orange-500/30 group-hover:scale-110 transition-transform duration-300">
            <Package class="w-6 h-6" />
          </div>
          <div>
            <p class="text-xs font-semibold text-neutral-400 uppercase tracking-widest">Total Products</p>
            <h3 class="text-3xl font-black text-white tracking-tight">{{ stats.total }}</h3>
          </div>
        </div>
      </div>
      <div
        class="stat-card group relative bg-white/5 backdrop-blur-xl rounded-2xl p-6 border border-white/10 hover:border-emerald-500/50 hover:-translate-y-2 hover:shadow-2xl hover:shadow-emerald-500/10 transition-all duration-500">
        <div
          class="absolute inset-0 bg-linear-to-br from-emerald-500/10 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-500 rounded-2xl">
        </div>
        <div class="relative z-10 flex items-center gap-4">
          <div
            class="w-14 h-14 flex items-center justify-center bg-linear-to-br from-emerald-400 to-emerald-600 rounded-2xl text-white shadow-lg shadow-emerald-500/30 group-hover:scale-110 transition-transform duration-300">
            <Boxes class="w-6 h-6" />
          </div>
          <div>
            <p class="text-xs font-semibold text-neutral-400 uppercase tracking-widest">In Stock</p>
            <h3 class="text-3xl font-black text-white tracking-tight">{{ stats.active }}</h3>
          </div>
        </div>
      </div>
      <div
        class="stat-card group relative bg-white/5 backdrop-blur-xl rounded-2xl p-6 border border-white/10 hover:border-red-500/50 hover:-translate-y-2 hover:shadow-2xl hover:shadow-red-500/10 transition-all duration-500">
        <div
          class="absolute inset-0 bg-linear-to-br from-red-500/10 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-500 rounded-2xl">
        </div>
        <div class="relative z-10 flex items-center gap-4">
          <div
            class="w-14 h-14 flex items-center justify-center bg-linear-to-br from-red-400 to-red-600 rounded-2xl text-white shadow-lg shadow-red-500/30 group-hover:scale-110 transition-transform duration-300">
            <AlertCircle class="w-6 h-6" />
          </div>
          <div>
            <p class="text-xs font-semibold text-neutral-400 uppercase tracking-widest">Out of Stock</p>
            <h3 class="text-3xl font-black text-white tracking-tight">{{ stats.outOfStock }}</h3>
          </div>
        </div>
      </div>
      <div
        class="stat-card group relative bg-white/5 backdrop-blur-xl rounded-2xl p-6 border border-white/10 hover:border-purple-500/50 hover:-translate-y-2 hover:shadow-2xl hover:shadow-purple-500/10 transition-all duration-500">
        <div
          class="absolute inset-0 bg-linear-to-br from-purple-500/10 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-500 rounded-2xl">
        </div>
        <div class="relative z-10 flex items-center gap-4">
          <div
            class="w-14 h-14 flex items-center justify-center bg-linear-to-br from-purple-400 to-purple-600 rounded-2xl text-white shadow-lg shadow-purple-500/30 group-hover:scale-110 transition-transform duration-300">
            <DollarSign class="w-6 h-6" />
          </div>
          <div>
            <p class="text-xs font-semibold text-neutral-400 uppercase tracking-widest">Total Value</p>
            <h3 class="text-3xl font-black text-white tracking-tight">${{ stats.totalValue.toLocaleString() }}</h3>
          </div>
        </div>
      </div>
    </div>

    <!-- Table Card -->
    <div class="glass-card bg-white/5 backdrop-blur-xl rounded-3xl border border-white/10 overflow-hidden">
      <!-- Filters -->
      <div
        class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4 p-5 bg-white/5 border-b border-white/10">
        <div class="relative flex-1 max-w-sm">
          <Search class="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-neutral-500" />
          <input v-model="searchTerm" type="text" placeholder="Search products..."
            class="w-full pl-10 pr-4 py-3 bg-white/5 border border-white/10 rounded-xl text-sm text-white placeholder-neutral-500 focus:bg-white/10 focus:border-orange-500/50 outline-none transition-all" />
        </div>
        <div class="flex flex-wrap gap-3">
          <select v-model="selectedCategory"
            class="px-4 py-3 bg-white/5 border border-white/10 rounded-xl text-xs font-semibold text-neutral-300 focus:border-orange-500/50 outline-none">
            <option value="all" class="bg-neutral-900">All Categories</option>
            <option v-for="c in categories" :key="c" :value="c" class="bg-neutral-900">{{ c }}</option>
          </select>
          <button @click="exportToCSV"
            class="flex items-center gap-2 px-4 py-3 bg-linear-to-r from-orange-500 to-pink-500 text-white rounded-xl text-xs font-bold hover:shadow-lg hover:shadow-orange-500/25 transition-all">
            <Download class="w-4 h-4" />Export
          </button>
        </div>
      </div>

      <!-- Loading -->
      <div v-if="loading && products.length === 0" class="p-8 space-y-3">
        <div v-for="i in 5" :key="i" class="h-16 bg-white/5 rounded-xl animate-pulse"></div>
      </div>

      <!-- Empty -->
      <div v-else-if="filteredProducts.length === 0"
        class="flex flex-col items-center justify-center py-16 text-center">
        <div
          class="w-20 h-20 flex items-center justify-center bg-white/5 rounded-2xl text-neutral-500 mb-6 border border-white/10">
          <Package class="w-10 h-10" />
        </div>
        <h3 class="text-xl font-bold text-white mb-2">No Products Found</h3>
        <p class="text-neutral-400 text-sm mb-6">Try adjusting your filters or add a new product</p>
        <button @click="openAddModal"
          class="flex items-center gap-2 px-6 py-3 bg-linear-to-r from-orange-500 to-pink-500 rounded-xl text-sm font-bold text-white shadow-lg shadow-orange-500/25">
          <Plus class="w-4 h-4" />Add New Product
        </button>
      </div>

      <!-- Mobile Cards -->
      <div class="lg:hidden grid grid-cols-1 sm:grid-cols-2 gap-4 p-4">
        <div v-for="p in filteredProducts" :key="p.id"
          class="bg-white/5 rounded-xl p-5 hover:bg-white/10 border border-white/10 transition-all">
          <div class="flex items-center gap-3 mb-4">
            <div class="w-14 h-14 rounded-xl overflow-hidden bg-white/10 shadow-sm">
              <img v-if="p.image" :src="p.image" class="w-full h-full object-cover" />
              <div v-else class="w-full h-full flex items-center justify-center text-neutral-500">
                <Package class="w-6 h-6" />
              </div>
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-sm font-bold text-white truncate">{{ p.name }}</p>
              <span
                class="inline-block text-[10px] font-semibold text-orange-400 bg-orange-500/20 px-2 py-0.5 rounded mt-1">{{
                  p.category }}</span>
            </div>
          </div>
          <div class="flex justify-between items-center mb-4">
            <div>
              <p class="text-[10px] text-neutral-500 uppercase">Price</p>
              <p class="text-lg font-bold text-white">${{ p.price }}</p>
            </div>
            <div class="text-right">
              <p class="text-[10px] text-neutral-500 uppercase">Stock</p>
              <p :class="['text-lg font-bold', p.stock > 0 ? 'text-emerald-400' : 'text-red-400']">{{ p.stock }}</p>
            </div>
          </div>
          <div class="flex gap-2">
            <button @click="openViewModal(p)"
              class="flex-1 py-2 bg-white/5 border border-white/10 rounded-lg text-xs font-semibold text-neutral-300 flex items-center justify-center gap-1 hover:bg-white/10 transition-all">
              <Eye class="w-3.5 h-3.5" />View
            </button>
            <button @click="openEditModal(p)"
              class="flex-1 py-2 bg-linear-to-r from-orange-500 to-pink-500 rounded-lg text-xs font-semibold text-white flex items-center justify-center gap-1">
              <Edit2 class="w-3.5 h-3.5" />Edit
            </button>
          </div>
        </div>
      </div>

      <!-- Table -->
      <table v-if="filteredProducts.length > 0" class="hidden lg:table w-full">
        <thead>
          <tr class="bg-white/5">
            <th class="px-6 py-4 text-left">
              <button @click="toggleSort('name')"
                class="flex items-center gap-1.5 text-xs font-semibold text-neutral-400 uppercase tracking-wider hover:text-orange-400 transition-colors">
                Product
                <ArrowUpDown class="w-3 h-3" />
              </button>
            </th>
            <th class="px-6 py-4 text-left text-xs font-semibold text-neutral-400 uppercase tracking-wider">Category
            </th>
            <th class="px-6 py-4 text-left">
              <button @click="toggleSort('price')"
                class="flex items-center gap-1.5 text-xs font-semibold text-neutral-400 uppercase tracking-wider hover:text-orange-400 transition-colors">
                Price
                <ArrowUpDown class="w-3 h-3" />
              </button>
            </th>
            <th class="px-6 py-4 text-left">
              <button @click="toggleSort('stock')"
                class="flex items-center gap-1.5 text-xs font-semibold text-neutral-400 uppercase tracking-wider hover:text-orange-400 transition-colors">
                Stock
                <ArrowUpDown class="w-3 h-3" />
              </button>
            </th>
            <th class="px-6 py-4 text-right text-xs font-semibold text-neutral-400 uppercase tracking-wider">Actions
            </th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="p in filteredProducts" :key="p.id"
            class="border-b border-white/5 hover:bg-white/5 transition-colors group">
            <td class="px-6 py-4">
              <div class="flex items-center gap-3">
                <div
                  class="w-11 h-11 rounded-xl overflow-hidden bg-white/10 shadow-lg group-hover:scale-110 transition-transform duration-300">
                  <img v-if="p.image" :src="p.image" class="w-full h-full object-cover" />
                  <div v-else class="w-full h-full flex items-center justify-center text-neutral-500">
                    <Package class="w-5 h-5" />
                  </div>
                </div>
                <div>
                  <p class="text-sm font-semibold text-white group-hover:text-orange-400 transition-colors">{{ p.name }}
                  </p>
                  <p class="text-xs text-neutral-500 max-w-[200px] truncate">{{ p.description || 'No description' }}</p>
                </div>
              </div>
            </td>
            <td class="px-6 py-4">
              <div class="flex flex-wrap gap-1">
                <span v-for="cat in (p.allCategories?.length ? p.allCategories : [p.category])" :key="cat"
                  class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-semibold bg-orange-500/20 text-orange-400 border border-orange-500/30">
                  <Tag class="w-3 h-3" />{{ cat }}
                </span>
              </div>
            </td>
            <td class="px-6 py-4 text-sm font-bold text-white">${{ p.price }}</td>
            <td class="px-6 py-4">
              <span
                :class="['px-3 py-1.5 rounded-full text-xs font-bold uppercase', p.stock > 10 ? 'bg-emerald-500/20 text-emerald-400 border border-emerald-500/30' : p.stock > 0 ? 'bg-orange-500/20 text-orange-400 border border-orange-500/30' : 'bg-red-500/20 text-red-400 border border-red-500/30']">{{
                  p.stock }} units</span>
            </td>
            <td class="px-6 py-4">
              <div class="flex items-center justify-end gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                <button @click="openViewModal(p)"
                  class="p-2 rounded-lg text-neutral-400 hover:bg-white/10 hover:text-white transition-all">
                  <Eye class="w-4 h-4" />
                </button>
                <button @click="openEditModal(p)"
                  class="p-2 rounded-lg text-neutral-400 hover:bg-orange-500/20 hover:text-orange-400 transition-all">
                  <Edit2 class="w-4 h-4" />
                </button>
                <button @click="handleDeleteProduct(p.id)"
                  class="p-2 rounded-lg text-neutral-400 hover:bg-red-500/20 hover:text-red-400 transition-all">
                  <Trash2 class="w-4 h-4" />
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <Teleport to="body">
      <Transition name="modal">
        <div v-if="showAddModal || showEditModal"
          class="fixed inset-0 bg-linear-to-br from-black/70 via-black/60 to-neutral-900/70 backdrop-blur-md flex items-center justify-center p-4 z-50"
          @click.self="closeModals">

          <!-- Modal Container -->
          <div
            class="bg-white rounded-2xl sm:rounded-3xl w-full max-w-4xl shadow-2xl overflow-hidden animate-modalSlideIn max-h-[95vh] sm:max-h-[90vh] flex flex-col">

            <!-- Two Column Layout -->
            <div class="flex flex-col lg:flex-row min-h-0 flex-1 overflow-hidden">

              <!-- Left Column: Image Section -->
              <div
                class="lg:w-2/5 bg-linear-to-br from-neutral-900 via-neutral-800 to-neutral-900 p-4 sm:p-6 lg:p-8 flex flex-col relative overflow-hidden shrink-0 lg:min-h-[500px]">
                <!-- Decorative Elements -->
                <div class="absolute top-0 right-0 w-64 h-64 bg-orange-500/10 rounded-full blur-3xl"></div>
                <div class="absolute bottom-0 left-0 w-48 h-48 bg-orange-500/5 rounded-full blur-2xl"></div>

                <!-- Header -->
                <div class="relative z-10 mb-4 lg:mb-6">
                  <div
                    class="inline-flex items-center gap-2 px-3 py-1.5 bg-white/10 backdrop-blur-sm rounded-full mb-4">
                    <div class="w-2 h-2 bg-orange-500 rounded-full animate-pulse"></div>
                    <span class="text-[10px] font-bold text-white/80 uppercase tracking-wider">{{ showAddModal
                      ? 'NewProduct' : 'Editing' }}</span>
                  </div>
                  <h2 class="text-xl sm:text-2xl font-bold text-white mb-1">{{ showAddModal ? 'Create Product'
                    : 'UpdateProduct' }}</h2>
                  <p class="text-sm text-white/50">{{ showAddModal ? 'Add a new item to your catalog'
                    : 'Makechangestoyour product' }}</p>
                </div>
                <!-- Multi-Image Upload Area -->
                <div class="flex-1 relative z-10 flex flex-col">
                  <label class="block text-[10px] font-bold text-white/40 uppercase tracking-wider mb-3">Product Images
                    <span class="text-orange-400">({{ formData.images.length }})</span></label>

                  <!-- Main Image Display -->
                  <div v-if="formData.images.length > 0" class="mb-3">
                    <div class="relative rounded-2xl overflow-hidden bg-white/5 aspect-video">
                      <img :src="formData.images[mainImageIndex]?.preview || formData.images[mainImageIndex]?.url"
                        class="w-full h-full object-cover" alt="Main image" />
                      <div v-if="formData.images[mainImageIndex]?.uploading"
                        class="absolute inset-0 bg-black/50 flex items-center justify-center">
                        <Loader2 class="w-8 h-8 animate-spin text-orange-500" />
                      </div>
                      <div
                        class="absolute top-2 left-2 px-2 py-1 bg-orange-500/90 text-white text-[10px] font-bold rounded-lg">
                        MAIN</div>
                    </div>
                  </div>

                  <!-- Thumbnail Gallery -->
                  <div v-if="formData.images.length > 0" class="grid grid-cols-4 gap-2 mb-3">
                    <div v-for="(img, idx) in formData.images" :key="idx"
                      class="relative aspect-square rounded-lg overflow-hidden cursor-pointer group"
                      :class="idx === mainImageIndex ? 'ring-2 ring-orange-500' : 'ring-1 ring-white/20'"
                      @click="setMainImage(idx)">
                      <img :src="img.preview || img.url" class="w-full h-full object-cover" />
                      <div v-if="img.uploading" class="absolute inset-0 bg-black/50 flex items-center justify-center">
                        <Loader2 class="w-4 h-4 animate-spin text-white" />
                      </div>
                      <button @click.stop="removeImage(idx)" type="button"
                        class="absolute top-1 right-1 p-1 bg-black/60 rounded-full opacity-0 group-hover:opacity-100 hover:bg-red-500 transition-all">
                        <X class="w-3 h-3 text-white" />
                      </button>
                      <div v-if="idx === mainImageIndex"
                        class="absolute bottom-1 left-1 px-1 py-0.5 bg-orange-500 text-white text-[8px] font-bold rounded">
                        ★</div>
                    </div>
                  </div>

                  <!-- Upload Button -->
                  <div
                    class="border-2 border-dashed border-white/20 rounded-xl p-4 flex items-center justify-center cursor-pointer hover:border-orange-500/50 hover:bg-white/5 transition-all group"
                    @click="$refs.fileInput.click()">
                    <input ref="fileInput" type="file" accept="image/*" multiple class="hidden"
                      @change="handleImageSelect" />
                    <Upload class="w-5 h-5 text-white/40 group-hover:text-orange-400 mr-2" />
                    <span class="text-sm text-white/50 group-hover:text-white/70">Add images</span>
                  </div>

                  <p v-if="formData.images.length > 0" class="text-[10px] text-white/30 mt-2 text-center">Click
                    thumbnail to set as main</p>
                </div>

                <!-- Close button for mobile -->
                <button @click="closeModals"
                  class="lg:hidden absolute top-4 right-4 p-2 bg-white/10 rounded-xl text-white/70 hover:bg-white/20">
                  <X class="w-5 h-5" />
                </button>
              </div>

              <!-- Right Column: Form Section -->
              <div class="lg:w-3/5 flex flex-col min-h-0 overflow-hidden">
                <!-- Form Header -->
                <div class="hidden lg:flex items-center justify-between p-6 border-b border-neutral-100">
                  <div class="flex items-center gap-3">
                    <div
                      class="w-10 h-10 flex items-center justify-center bg-linear-to-br from-orange-500 to-orange-600 rounded-xl text-white shadow-lg shadow-orange-500/25">
                      <Package class="w-5 h-5" />
                    </div>
                    <div>
                      <h3 class="text-base font-bold text-neutral-800">Product Details</h3>
                      <p class="text-[10px] font-medium text-neutral-400">Fill in the information below</p>
                    </div>
                  </div>
                  <button @click="closeModals"
                    class="p-2 rounded-xl text-neutral-400 hover:bg-neutral-100 hover:text-neutral-600 transition-colors">
                    <X class="w-5 h-5" />
                  </button>
                </div>

                <!-- Form Content -->
                <div class="flex-1 overflow-y-auto p-4 sm:p-6 space-y-4 sm:space-y-5">

                  <!-- Product Name -->
                  <div class="group">
                    <label
                      class="block text-[10px] font-bold text-neutral-400 uppercase tracking-wider mb-2 group-focus-within:text-orange-500 transition-colors">
                      Product Name <span class="text-red-400">*</span>
                    </label>
                    <div class="relative">
                      <input v-model="formData.name" type="text"
                        class="w-full px-4 py-3.5 bg-neutral-50 border-2 border-neutral-100 rounded-xl text-sm font-medium text-neutral-800 placeholder-neutral-400 focus:bg-white focus:border-orange-500 focus:ring-4 focus:ring-orange-500/10 outline-none transition-all duration-200"
                        placeholder="Enter product name" />
                    </div>
                  </div>

                  <!-- Description -->
                  <div class="group">
                    <label
                      class="block text-[10px] font-bold text-neutral-400 uppercase tracking-wider mb-2 group-focus-within:text-orange-500 transition-colors">
                      Description
                    </label>
                    <textarea v-model="formData.description" rows="3"
                      class="w-full px-4 py-3.5 bg-neutral-50 border-2 border-neutral-100 rounded-xl text-sm font-medium text-neutral-800 placeholder-neutral-400 focus:bg-white focus:border-orange-500 focus:ring-4 focus:ring-orange-500/10 outline-none resize-none transition-all duration-200"
                      placeholder="Describe your product..."></textarea>
                  </div>

                  <!-- SKU & Supplier Row -->
                  <div class="grid grid-cols-2 gap-4">
                    <div class="group">
                      <label
                        class="block text-[10px] font-bold text-neutral-400 uppercase tracking-wider mb-2 group-focus-within:text-orange-500 transition-colors">
                        SKU
                      </label>
                      <input v-model="formData.sku" type="text"
                        class="w-full px-4 py-3.5 bg-neutral-50 border-2 border-neutral-100 rounded-xl text-sm font-medium text-neutral-800 placeholder-neutral-400 focus:bg-white focus:border-orange-500 focus:ring-4 focus:ring-orange-500/10 outline-none transition-all duration-200"
                        placeholder="e.g. PRD-001" />
                    </div>
                    <div class="group">
                      <label
                        class="block text-[10px] font-bold text-neutral-400 uppercase tracking-wider mb-2 group-focus-within:text-orange-500 transition-colors">
                        Supplier
                      </label>
                      <input v-model="formData.supplier" type="text"
                        class="w-full px-4 py-3.5 bg-neutral-50 border-2 border-neutral-100 rounded-xl text-sm font-medium text-neutral-800 placeholder-neutral-400 focus:bg-white focus:border-orange-500 focus:ring-4 focus:ring-orange-500/10 outline-none transition-all duration-200"
                        placeholder="Supplier name" />
                    </div>
                  </div>

                  <!-- Categories Section -->
                  <div class="group">
                    <label
                      class="block text-[10px] font-bold text-neutral-400 uppercase tracking-wider mb-2 group-focus-within:text-orange-500 transition-colors">
                      Categories
                    </label>
                    <div class="flex flex-wrap gap-2 mb-2">
                      <span v-for="cat in selectedCategories" :key="cat"
                        class="inline-flex items-center gap-1.5 px-3 py-1.5 bg-orange-100 text-orange-700 rounded-lg text-xs font-semibold">
                        {{ cat }}
                        <button @click="removeCategory(cat)" type="button" class="hover:text-orange-900">
                          <X class="w-3 h-3" />
                        </button>
                      </span>
                    </div>
                    <div class="flex gap-2">
                      <input v-model="newCategoryInput" type="text"
                        class="flex-1 px-4 py-2.5 bg-neutral-50 border-2 border-neutral-100 rounded-xl text-sm font-medium text-neutral-800 placeholder-neutral-400 focus:bg-white focus:border-orange-500 focus:ring-4 focus:ring-orange-500/10 outline-none transition-all duration-200"
                        placeholder="Type category name..." @keyup.enter="addCustomCategory" />
                      <button @click="addCustomCategory" type="button"
                        class="px-4 py-2.5 bg-orange-500 text-white rounded-xl text-sm font-bold hover:bg-orange-600 transition-colors">
                        <Plus class="w-4 h-4" />
                      </button>
                    </div>
                  </div>

                  <!-- Price, Stock, and Discount Row -->
                  <div class="grid grid-cols-3 gap-3">
                    <div class="group">
                      <label
                        class="block text-[10px] font-bold text-neutral-400 uppercase tracking-wider mb-2 group-focus-within:text-orange-500 transition-colors">
                        Price <span class="text-red-400">*</span>
                      </label>
                      <div class="relative">
                        <div class="absolute left-4 top-1/2 -translate-y-1/2 text-neutral-400 font-bold">$</div>
                        <input v-model.number="formData.price" type="number" min="0" step="0.01"
                          class="w-full pl-8 pr-4 py-3.5 bg-neutral-50 border-2 border-neutral-100 rounded-xl text-sm font-bold text-neutral-800 focus:bg-white focus:border-orange-500 focus:ring-4 focus:ring-orange-500/10 outline-none transition-all duration-200"
                          placeholder="0.00" />
                      </div>
                    </div>
                    <div class="group">
                      <label
                        class="block text-[10px] font-bold text-neutral-400 uppercase tracking-wider mb-2 group-focus-within:text-orange-500 transition-colors">
                        Discount
                      </label>
                      <div class="relative">
                        <input v-model.number="formData.discount" type="number" min="0" max="100" step="1"
                          class="w-full pl-4 pr-8 py-3.5 bg-neutral-50 border-2 border-neutral-100 rounded-xl text-sm font-bold text-neutral-800 focus:bg-white focus:border-orange-500 focus:ring-4 focus:ring-orange-500/10 outline-none transition-all duration-200"
                          placeholder="0" />
                        <div class="absolute right-4 top-1/2 -translate-y-1/2 text-neutral-400 font-bold">%</div>
                      </div>
                    </div>
                    <div class="group">
                      <label
                        class="block text-[10px] font-bold text-neutral-400 uppercase tracking-wider mb-2 group-focus-within:text-orange-500 transition-colors">
                        Stock
                      </label>
                      <div class="relative">
                        <Boxes class="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-neutral-400" />
                        <input v-model.number="formData.stock" type="number" min="0"
                          class="w-full pl-10 pr-4 py-3.5 bg-neutral-50 border-2 border-neutral-100 rounded-xl text-sm font-bold text-neutral-800 focus:bg-white focus:border-orange-500 focus:ring-4 focus:ring-orange-500/10 outline-none transition-all duration-200"
                          placeholder="0" />
                      </div>
                    </div>
                  </div>

                  <!-- Variants Section -->
                  <div class="pt-4">
                    <div class="flex items-center gap-3 mb-4">
                      <div class="h-px flex-1 bg-linear-to-r from-transparent via-neutral-200 to-transparent"></div>
                      <span class="text-[10px] font-bold text-neutral-400 uppercase tracking-wider">Variants</span>
                      <div class="h-px flex-1 bg-linear-to-r from-transparent via-neutral-200 to-transparent"></div>
                    </div>

                    <!-- Color Options -->
                    <div class="mb-4">
                      <p class="text-xs font-bold text-neutral-600 mb-3 flex items-center gap-2">
                        <span class="w-4 h-4 rounded-full bg-linear-to-r from-red-500 via-black to-white"></span>
                        Colors
                      </p>
                      <div class="flex flex-wrap gap-2 mb-2">
                        <label v-for="color in colorOptions" :key="color.value"
                          class="relative flex items-center gap-2.5 px-4 py-2.5 rounded-xl cursor-pointer transition-all duration-200 border-2"
                          :class="selectedColors.includes(color.value)
                            ? 'bg-neutral-900 text-white border-neutral-900 shadow-lg'
                            : 'bg-white text-neutral-700 border-neutral-200 hover:border-neutral-300 hover:shadow-md'">
                          <input type="checkbox" :value="color.value" v-model="selectedColors" class="hidden" />
                          <span class="w-4 h-4 rounded-full shadow-inner border-2" :style="{
                            backgroundColor: color.value.toLowerCase(),
                            borderColor: selectedColors.includes(color.value) ? 'white' : (color.value === 'White' ? '#e5e5e5' : color.value.toLowerCase())
                          }">
                            <span class="w-4 h-4 rounded-full bg-linear-to-r from-red-500 via-black to-white"></span>
                          </span>
                          <span class="text-xs font-semibold">{{ color.label }}</span>
                          <CheckCircle v-if="selectedColors.includes(color.value)"
                            class="w-3.5 h-3.5 text-orange-400 absolute -top-1 -right-1" />
                        </label>
                      </div>
                      <!-- Add Custom Color -->
                      <div class="flex gap-2 mt-2">
                        <input v-model="newColorInput" type="text"
                          class="flex-1 px-3 py-2 bg-neutral-50 border-2 border-neutral-100 rounded-lg text-xs font-medium text-neutral-800 placeholder-neutral-400 focus:bg-white focus:border-orange-500 outline-none transition-all"
                          placeholder="Add custom color..." @keyup.enter="addCustomColor" />
                        <button @click="addCustomColor" type="button"
                          class="px-3 py-2 bg-neutral-100 text-neutral-600 rounded-lg text-xs font-bold hover:bg-neutral-200 transition-colors">
                          <Plus class="w-3.5 h-3.5" />
                        </button>
                      </div>
                    </div>

                    <!-- Size Options -->
                    <div class="mb-4">
                      <p class="text-xs font-bold text-neutral-600 mb-3 flex items-center gap-2">
                        <span
                          class="flex items-center justify-center w-4 h-4 bg-neutral-200 rounded text-[8px] font-black">S</span>
                        Sizes
                      </p>
                      <div class="flex flex-wrap gap-2 mb-2">
                        <label v-for="size in sizeOptions" :key="size.value"
                          class="relative flex items-center justify-center min-w-[60px] px-3 py-2.5 rounded-xl cursor-pointer transition-all duration-200 border-2"
                          :class="selectedSizes.includes(size.value)
                            ? 'bg-linear-to-r from-orange-500 to-orange-600 text-white border-orange-500 shadow-lg shadow-orange-500/25'
                            : 'bg-white text-neutral-700 border-neutral-200 hover:border-neutral-300 hover:shadow-md'">
                          <input type="checkbox" :value="size.value" v-model="selectedSizes" class="hidden" />
                          <span class="text-xs font-bold">{{ size.label }}</span>
                        </label>
                      </div>
                      <!-- Add Custom Size -->
                      <div class="flex gap-2 mt-2">
                        <input v-model="newSizeInput" type="text"
                          class="flex-1 px-3 py-2 bg-neutral-50 border-2 border-neutral-100 rounded-lg text-xs font-medium text-neutral-800 placeholder-neutral-400 focus:bg-white focus:border-orange-500 outline-none transition-all"
                          placeholder="Add custom size..." @keyup.enter="addCustomSize" />
                        <button @click="addCustomSize" type="button"
                          class="px-3 py-2 bg-neutral-100 text-neutral-600 rounded-lg text-xs font-bold hover:bg-neutral-200 transition-colors">
                          <Plus class="w-3.5 h-3.5" />
                        </button>
                      </div>
                    </div>

                    <!-- Generated Variants Preview with Pricing -->
                    <Transition name="fade">
                      <div v-if="generatedVariants.length > 0"
                        class="p-4 bg-linear-to-br from-neutral-50 to-neutral-100/50 rounded-2xl border border-neutral-200/50">
                        <div class="flex items-center justify-between mb-4">
                          <p class="text-[10px] font-bold text-neutral-500 uppercase tracking-wider">Variant Pricing
                          </p>
                          <span class="px-2 py-0.5 bg-orange-500 text-white text-[10px] font-bold rounded-full">{{
                            generatedVariants.length }} variant{{ generatedVariants.length > 1 ? 's' : '' }}</span>
                        </div>

                        <!-- Variant Cards -->
                        <div class="space-y-3">
                          <div v-for="(variant) in generatedVariants" :key="variant.key"
                            class="bg-white rounded-xl p-3 border border-neutral-200 shadow-sm hover:shadow-md transition-shadow">

                            <!-- Variant Header -->
                            <div class="flex items-center gap-2 mb-3">
                              <span class="w-2 h-2 bg-orange-500 rounded-full"></span>
                              <span class="text-xs font-bold text-neutral-800">{{ variant.label }}</span>
                              <span v-if="variant.discountPrice && variant.discountPrice < variant.price"
                                class="px-2 py-0.5 bg-green-100 text-green-700 text-[9px] font-bold rounded-full">
                                ON SALE
                              </span>
                              <button @click="deleteVariant(variant)" type="button"
                                class="ml-auto p-1.5 text-neutral-400 hover:text-red-500 hover:bg-red-50 rounded-lg transition-colors"
                                title="Remove this variant">
                                <X class="w-3.5 h-3.5" />
                              </button>
                            </div>

                            <!-- Pricing Row -->
                            <div class="grid grid-cols-3 gap-2">
                              <!-- Regular Price -->
                              <div>
                                <label
                                  class="text-[9px] font-semibold text-neutral-400 uppercase mb-1 block">Price</label>
                                <div class="relative">
                                  <span
                                    class="absolute left-2 top-1/2 -translate-y-1/2 text-neutral-400 text-xs">$</span>
                                  <input type="number" :value="variant.price"
                                    @input="updateVariantPrice(variant.key, 'price', parseFloat($event.target.value) || 0)"
                                    min="0" step="0.01"
                                    class="w-full pl-5 pr-2 py-2 bg-neutral-50 border border-neutral-200 rounded-lg text-xs font-semibold text-neutral-800 focus:bg-white focus:border-orange-500 outline-none transition-all"
                                    placeholder="0.00" />
                                </div>
                              </div>

                              <!-- Discount Price -->
                              <div>
                                <label class="text-[9px] font-semibold text-neutral-400 uppercase mb-1 block">Sale
                                  Price</label>
                                <div class="relative">
                                  <span
                                    class="absolute left-2 top-1/2 -translate-y-1/2 text-neutral-400 text-xs">$</span>
                                  <input type="number" :value="variant.discountPrice"
                                    @input="updateVariantPrice(variant.key, 'discountPrice', $event.target.value ? parseFloat($event.target.value) : null)"
                                    min="0" step="0.01"
                                    class="w-full pl-5 pr-2 py-2 bg-neutral-50 border border-neutral-200 rounded-lg text-xs font-semibold text-neutral-800 focus:bg-white focus:border-green-500 outline-none transition-all"
                                    placeholder="Optional" />
                                </div>
                              </div>

                              <!-- Stock -->
                              <div>
                                <label
                                  class="text-[9px] font-semibold text-neutral-400 uppercase mb-1 block">Stock</label>
                                <input type="number" :value="variant.stockQuantity"
                                  @input="updateVariantPrice(variant.key, 'stockQuantity', parseInt($event.target.value) || 0)"
                                  min="0"
                                  class="w-full px-2 py-2 bg-neutral-50 border border-neutral-200 rounded-lg text-xs font-semibold text-neutral-800 focus:bg-white focus:border-orange-500 outline-none transition-all"
                                  placeholder="0" />
                              </div>
                            </div>

                            <!-- Price Summary -->
                            <div v-if="variant.discountPrice && variant.discountPrice < variant.price"
                              class="mt-2 pt-2 border-t border-neutral-100">
                              <div class="flex items-center gap-2 text-[10px]">
                                <span class="text-neutral-400">Final price:</span>
                                <span class="line-through text-neutral-400">${{ variant.price.toFixed(2) }}</span>
                                <span class="font-bold text-green-600">${{ variant.discountPrice.toFixed(2) }}</span>
                                <span class="ml-auto px-1.5 py-0.5 bg-green-50 text-green-600 font-bold rounded">
                                  -{{ Math.round((1 - variant.discountPrice / variant.price) * 100) }}%
                                </span>
                              </div>
                            </div>
                          </div>
                        </div>

                        <!-- Info tip -->
                        <p class="text-[10px] text-neutral-400 mt-3 text-center">
                          Set individual prices for each variant. Leave "Sale Price" empty for no discount.
                        </p>
                      </div>
                    </Transition>
                  </div>

                </div>

                <!-- Form Footer -->
                <div class="p-4 sm:p-6 bg-linear-to-t from-neutral-50 to-white border-t border-neutral-100 shrink-0">
                  <div class="flex flex-col sm:flex-row gap-2 sm:gap-3">
                    <button @click="closeModals"
                      class="flex-1 py-3 sm:py-3.5 bg-white border-2 border-neutral-200 rounded-xl text-sm font-bold text-neutral-600 hover:bg-neutral-50 hover:border-neutral-300 transition-all duration-200 order-2 sm:order-1">
                      Cancel
                    </button>
                    <button @click="showAddModal ? handleAddProduct() : handleUpdateProduct()"
                      :disabled="loading || !formData.name || !formData.price"
                      class="flex-1 py-3 sm:py-3.5 bg-linear-to-r from-neutral-900 to-neutral-800 rounded-xl text-sm font-bold text-white disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2 hover:from-neutral-800 hover:to-neutral-700 transition-all duration-200 shadow-lg shadow-neutral-900/20 order-1 sm:order-2">
                      <Loader2 v-if="loading" class="w-4 h-4 animate-spin" />
                      <Plus v-else class="w-4 h-4" />
                      {{ loading ? 'Processing...' : (showAddModal ? 'Create' : 'Save') }}
                    </button>
                  </div>
                </div>

              </div>
            </div>
          </div>
        </div>
      </Transition>
      <Transition name="modal">
        <div v-if="showViewModal && currentProduct"
          class="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4 z-50"
          @click.self="closeModals">
          <div class="bg-white rounded-2xl w-full max-w-3xl shadow-2xl overflow-hidden max-h-[90vh] flex flex-col">

            <!-- Header -->
            <div class="bg-neutral-900 px-6 py-4 flex items-center justify-between shrink-0">
              <div class="flex items-center gap-3">
                <div class="w-10 h-10 rounded-xl bg-white/10 flex items-center justify-center">
                  <Package class="w-5 h-5 text-white" />
                </div>
                <div>
                  <h3 class="text-lg font-bold text-white">{{ currentProduct.name }}</h3>
                  <p class="text-xs text-neutral-400">Product Details</p>
                </div>
              </div>
              <button @click="closeModals"
                class="p-2 rounded-lg bg-white/10 hover:bg-white/20 text-white/70 transition-colors">
                <X class="w-5 h-5" />
              </button>
            </div>

            <!-- Content -->
            <div class="flex-1 overflow-y-auto">
              <div class="flex flex-col lg:flex-row">

                <!-- Left: Images -->
                <div class="lg:w-2/5 bg-neutral-50 p-6">
                  <!-- Main Image -->
                  <div class="aspect-square bg-white rounded-xl overflow-hidden mb-3 border border-neutral-200">
                    <img v-if="currentProduct.image" :src="currentProduct.image" class="w-full h-full object-contain" />
                    <div v-else class="w-full h-full flex items-center justify-center text-neutral-300">
                      <Package class="w-20 h-20" />
                    </div>
                  </div>
                  <!-- Thumbnail Images -->
                  <div v-if="currentProduct.images && currentProduct.images.length > 1"
                    class="flex gap-2 overflow-x-auto pb-2">
                    <div v-for="(img, idx) in currentProduct.images" :key="idx"
                      class="w-16 h-16 shrink-0 rounded-lg overflow-hidden border-2 border-neutral-200 bg-white">
                      <img :src="img" class="w-full h-full object-cover" />
                    </div>
                  </div>
                </div>

                <!-- Right: Details -->
                <div class="lg:w-3/5 p-6 space-y-5">

                  <!-- Category & Status -->
                  <div class="flex items-center gap-2 flex-wrap">
                    <span
                      v-for="cat in (currentProduct.allCategories?.length ? currentProduct.allCategories : ['Uncategorized'])"
                      :key="cat" class="px-3 py-1 text-xs font-semibold bg-orange-100 text-orange-700 rounded-lg">
                      {{ cat }}
                    </span>
                    <span :class="['px-3 py-1 text-xs font-semibold rounded-lg',
                      currentProduct.stock > 0 ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700']">
                      {{ currentProduct.stock > 0 ? 'In Stock' : 'Out of Stock' }}
                    </span>
                  </div>

                  <!-- Description -->
                  <div>
                    <h4 class="text-xs font-bold text-neutral-400 uppercase tracking-wider mb-2">Description</h4>
                    <div class="relative">
                      <p
                        :class="['text-sm text-neutral-600 leading-relaxed transition-all', !showFullDescription ? 'line-clamp-2' : '']">
                        {{ currentProduct.description || 'No description available' }}
                      </p>
                      <button v-if="currentProduct.description && currentProduct.description.length > 100"
                        @click="showFullDescription = !showFullDescription"
                        class="text-xs font-bold text-orange-500 hover:text-orange-600 mt-1 focus:outline-none">
                        {{ showFullDescription ? 'Show less' : 'Read more' }}
                      </button>
                    </div>
                  </div>

                  <!-- SKU & Supplier -->
                  <div class="grid grid-cols-2 gap-4">
                    <div class="bg-neutral-50 rounded-xl p-4">
                      <h4 class="text-xs font-bold text-neutral-400 uppercase tracking-wider mb-1">SKU</h4>
                      <p class="text-sm font-semibold text-neutral-800">{{ currentProduct.sku || 'N/A' }}</p>
                    </div>
                    <div class="bg-neutral-50 rounded-xl p-4">
                      <h4 class="text-xs font-bold text-neutral-400 uppercase tracking-wider mb-1">Supplier</h4>
                      <p class="text-sm font-semibold text-neutral-800">{{ currentProduct.supplier || 'N/A' }}</p>
                    </div>
                  </div>

                  <!-- Price & Stock -->
                  <div class="grid grid-cols-3 gap-3">
                    <div class="bg-neutral-900 rounded-xl p-4 text-center">
                      <h4 class="text-xs font-bold text-neutral-400 uppercase tracking-wider mb-1">Price</h4>
                      <p class="text-xl font-bold text-white">${{ currentProduct.price }}</p>
                    </div>
                    <div class="bg-neutral-50 rounded-xl p-4 text-center">
                      <h4 class="text-xs font-bold text-neutral-400 uppercase tracking-wider mb-1">Stock</h4>
                      <p
                        :class="['text-xl font-bold', currentProduct.stock > 10 ? 'text-green-600' : currentProduct.stock > 0 ? 'text-orange-500' : 'text-red-600']">
                        {{ currentProduct.stock }}
                      </p>
                    </div>
                    <div class="bg-neutral-50 rounded-xl p-4 text-center">
                      <h4 class="text-xs font-bold text-neutral-400 uppercase tracking-wider mb-1">Discount</h4>
                      <p class="text-xl font-bold text-neutral-800">{{ currentProduct.discount || 0 }}%</p>
                    </div>
                  </div>

                  <!-- Variants -->
                  <div v-if="currentProduct.variants && currentProduct.variants.length > 0">
                    <h4 class="text-xs font-bold text-neutral-400 uppercase tracking-wider mb-3">Variants ({{
                      currentProduct.variants.length }})</h4>
                    <div class="space-y-2 max-h-40 overflow-y-auto">
                      <div v-for="(variant, idx) in currentProduct.variants" :key="idx"
                        class="flex items-center justify-between p-3 bg-neutral-50 rounded-lg text-sm">
                        <div class="flex items-center gap-2">
                          <span class="font-medium text-neutral-800">{{ variant.sku || 'Variant ' + (idx + 1) }}</span>
                          <span v-for="opt in variant.options" :key="opt.variant"
                            class="px-2 py-0.5 bg-white border border-neutral-200 rounded text-xs text-neutral-600">
                            {{ opt.variant }}: {{ opt.value }}
                          </span>
                        </div>
                        <div class="flex items-center gap-4">
                          <span class="font-bold text-neutral-800">${{ variant.price }}</span>
                          <span v-if="variant.discountPrice" class="text-green-600 text-xs font-medium">
                            Sale: ${{ variant.discountPrice }}
                          </span>
                          <span class="text-xs text-neutral-500">{{ variant.stockQuantity }} in stock</span>
                        </div>
                      </div>
                    </div>
                  </div>

                </div>
              </div>
            </div>

            <!-- Footer -->
            <div class="px-6 py-4 bg-neutral-50 border-t border-neutral-100 flex gap-3 shrink-0">
              <button @click="closeModals"
                class="flex-1 py-3 bg-white border-2 border-neutral-200 rounded-xl text-sm font-semibold text-neutral-600 hover:bg-neutral-100 transition-colors">
                Close
              </button>
              <button @click="openEditModal(currentProduct)"
                class="flex-1 py-3 bg-neutral-900 rounded-xl text-sm font-semibold text-white flex items-center justify-center gap-2 hover:bg-neutral-800 transition-colors">
                <Edit2 class="w-4 h-4" />Edit Product
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<style scoped>
.modal-enter-active,
.modal-leave-active {
  transition: opacity 0.4s cubic-bezier(0.4, 0, 0.2, 1);
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}

.modal-enter-active .animate-modalSlideIn {
  animation: modalSlideIn 0.5s cubic-bezier(0.34, 1.56, 0.64, 1);
}

.modal-leave-active .animate-modalSlideIn {
  animation: modalSlideOut 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

@keyframes modalSlideIn {
  from {
    opacity: 0;
    transform: scale(0.9) translateY(20px);
  }

  to {
    opacity: 1;
    transform: scale(1) translateY(0);
  }
}

@keyframes modalSlideOut {
  from {
    opacity: 1;
    transform: scale(1) translateY(0);
  }

  to {
    opacity: 0;
    transform: scale(0.95) translateY(10px);
  }
}

.fade-enter-active,
.fade-leave-active {
  transition: all 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
  transform: translateY(-8px);
}

/* Custom scrollbar for form */
.overflow-y-auto::-webkit-scrollbar {
  width: 6px;
}

.overflow-y-auto::-webkit-scrollbar-track {
  background: transparent;
}

.overflow-y-auto::-webkit-scrollbar-thumb {
  background: #e5e5e5;
  border-radius: 3px;
}

.overflow-y-auto::-webkit-scrollbar-thumb:hover {
  background: #d4d4d4;
}
</style>