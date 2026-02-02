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
const currentProduct = ref(null)
const formData = ref({ name: '', description: '', price: 0, stock: 0, category: '', images: [], discount: 0 })
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
  { label: 'XS', value: 'XS' },
  { label: 'Small (S)', value: 'S' },
  { label: 'Medium (M)', value: 'M' },
  { label: 'Large (L)', value: 'L' },
  { label: 'XL', value: 'XL' },
  { label: 'XXL', value: 'XXL' }
])
const newColorInput = ref('')
const newSizeInput = ref('')

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
        variants.push({
          label: label || 'Default',
          options
        })
      }
    }
  }
  return variants
})

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
      price: p.basePrice,
      stock: p.stock,
      image: p.images?.[0]?.imageUrl || ''
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
    images: p.image ? [{ url: p.image, preview: p.image }] : [],
    discount: p.discount || 0
  };
  mainImageIndex.value = 0;
  selectedCategories.value = p.category && p.category !== 'N/A' ? [p.category] : [];
  selectedColors.value = [];
  selectedSizes.value = [];
  newCategoryInput.value = '';
  newColorInput.value = '';
  newSizeInput.value = '';
  showEditModal.value = true
}
const openViewModal = (p) => {
  currentProduct.value = p;
  showViewModal.value = true
}
const closeModals = () => {
  showAddModal.value = false;
  showEditModal.value = false;
  showViewModal.value = false;
  currentProduct.value = null;
  formData.value.images = [];
  mainImageIndex.value = 0;
  selectedColors.value = [];
  selectedSizes.value = [];
  selectedCategories.value = [];
  newCategoryInput.value = '';
  newColorInput.value = '';
  newSizeInput.value = '';
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
        price: formData.value.price,
        stockQuantity: formData.value.stock,
        isActive: true,
        options: v.options
      }));
    } else {
      // Default variant if no options selected
      variants = [{
        sku: `SKU-${Date.now()}`,
        price: formData.value.price,
        stockQuantity: formData.value.stock,
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
        price: formData.value.price,
        stockQuantity: formData.value.stock,
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
    <div class="flex flex-wrap items-center justify-between gap-4 mb-8">
      <div>
        <div
          class="inline-flex items-center gap-2 px-3 py-1.5 bg-neutral-900 rounded-full text-white text-xs font-semibold mb-2">
          <Package class="w-3.5 h-3.5" /><span>Product Management</span>
        </div>
        <h1 class="text-2xl font-bold text-neutral-900">Products</h1>
        <p class="text-neutral-500 text-sm">Manage your product catalog</p>
      </div>
      <div class="flex gap-3">
        <button @click="fetchProducts" :disabled="loading"
          class="flex items-center gap-2 px-5 py-2.5 bg-white border border-neutral-200 rounded-xl text-sm font-semibold text-neutral-600 hover:bg-neutral-50">
          <RefreshCw class="w-4 h-4" :class="{ 'animate-spin': loading }" />Sync
        </button>
        <button @click="openAddModal"
          class="flex items-center gap-2 px-5 py-2.5 bg-neutral-900 rounded-xl text-sm font-semibold text-white hover:bg-neutral-800">
          <Plus class="w-4 h-4" />New
        </button>
      </div>
    </div>

    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5 mb-8">
      <div
        class="bg-white rounded-2xl p-5 border border-neutral-200 hover:-translate-y-1 hover:shadow-lg transition-all">
        <div class="flex items-center gap-4">
          <div class="w-12 h-12 flex items-center justify-center bg-neutral-900 rounded-xl text-white">
            <Package class="w-5 h-5" />
          </div>
          <div>
            <p class="text-[10px] font-bold text-neutral-400 uppercase">Total</p>
            <h3 class="text-2xl font-bold text-neutral-900">{{ stats.total }}</h3>
          </div>
        </div>
      </div>
      <div
        class="bg-white rounded-2xl p-5 border border-neutral-200 hover:-translate-y-1 hover:shadow-lg transition-all">
        <div class="flex items-center gap-4">
          <div class="w-12 h-12 flex items-center justify-center bg-green-500 rounded-xl text-white">
            <Boxes class="w-5 h-5" />
          </div>
          <div>
            <p class="text-[10px] font-bold text-neutral-400 uppercase">In Stock</p>
            <h3 class="text-2xl font-bold text-neutral-900">{{ stats.active }}</h3>
          </div>
        </div>
      </div>
      <div
        class="bg-white rounded-2xl p-5 border border-neutral-200 hover:-translate-y-1 hover:shadow-lg transition-all">
        <div class="flex items-center gap-4">
          <div class="w-12 h-12 flex items-center justify-center bg-red-500 rounded-xl text-white">
            <AlertCircle class="w-5 h-5" />
          </div>
          <div>
            <p class="text-[10px] font-bold text-neutral-400 uppercase">Out of Stock</p>
            <h3 class="text-2xl font-bold text-neutral-900">{{ stats.outOfStock }}</h3>
          </div>
        </div>
      </div>
      <div
        class="bg-white rounded-2xl p-5 border border-neutral-200 hover:-translate-y-1 hover:shadow-lg transition-all">
        <div class="flex items-center gap-4">
          <div class="w-12 h-12 flex items-center justify-center bg-orange-500 rounded-xl text-white">
            <DollarSign class="w-5 h-5" />
          </div>
          <div>
            <p class="text-[10px] font-bold text-neutral-400 uppercase">Value</p>
            <h3 class="text-2xl font-bold text-neutral-900">${{ stats.totalValue.toLocaleString() }}</h3>
          </div>
        </div>
      </div>
    </div>

    <div class="bg-white rounded-2xl border border-neutral-200 overflow-hidden">
      <div
        class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4 p-5 bg-neutral-50 border-b border-neutral-100">
        <div class="relative flex-1 max-w-sm">
          <Search class="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-neutral-400" /><input
            v-model="searchTerm" type="text" placeholder="Search..."
            class="w-full pl-10 pr-4 py-3 bg-white border border-neutral-200 rounded-xl text-sm focus:border-orange-500 outline-none" />
        </div>
        <div class="flex flex-wrap gap-3">
          <select v-model="selectedCategory"
            class="px-4 py-3 bg-white border border-neutral-200 rounded-xl text-xs font-semibold text-neutral-600">
            <option value="all">All</option>
            <option v-for="c in categories" :key="c" :value="c">{{ c }}</option>
          </select>
          <button @click="exportToCSV"
            class="flex items-center gap-2 px-4 py-3 bg-neutral-900 text-white rounded-xl text-xs font-semibold">
            <Download class="w-4 h-4" />Export
          </button>
        </div>
      </div>

      <div v-if="loading && products.length === 0" class="p-8 space-y-3">
        <div v-for="i in 5" :key="i" class="h-16 bg-neutral-100 rounded-xl animate-pulse"></div>
      </div>
      <div v-else-if="filteredProducts.length === 0" class="flex flex-col items-center py-16 text-center">
        <div class="w-20 h-20 flex items-center justify-center bg-neutral-100 rounded-2xl text-neutral-400 mb-6">
          <Package class="w-10 h-10" />
        </div>
        <h3 class="text-xl font-bold text-neutral-800 mb-2">No Products</h3>
        <p class="text-neutral-500 text-sm mb-6">Adjust filters or add new</p><button @click="openAddModal"
          class="flex items-center gap-2 px-6 py-3 bg-neutral-900 rounded-xl text-sm font-semibold text-white">
          <Plus class="w-4 h-4" />Add
        </button>
      </div>

      <div class="lg:hidden grid grid-cols-1 sm:grid-cols-2 gap-4 p-4">
        <div v-for="p in filteredProducts" :key="p.id"
          class="bg-neutral-50 rounded-xl p-5 hover:bg-neutral-100 transition-colors">
          <div class="flex items-center gap-3 mb-4">
            <div class="w-14 h-14 rounded-xl overflow-hidden bg-white shadow-sm"><img v-if="p.image" :src="p.image"
                class="w-full h-full object-cover" />
              <div v-else class="w-full h-full flex items-center justify-center text-neutral-300">
                <Package class="w-6 h-6" />
              </div>
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-sm font-bold text-neutral-800 truncate">{{ p.name }}</p><span
                class="inline-block text-[10px] font-semibold text-orange-600 bg-orange-100 px-2 py-0.5 rounded mt-1">{{
                  p.category }}</span>
            </div>
          </div>
          <div class="flex justify-between items-center mb-4">
            <div>
              <p class="text-[10px] text-neutral-400 uppercase">Price</p>
              <p class="text-lg font-bold text-neutral-800">${{ p.price }}</p>
            </div>
            <div class="text-right">
              <p class="text-[10px] text-neutral-400 uppercase">Stock</p>
              <p :class="['text-lg font-bold', p.stock > 0 ? 'text-green-600' : 'text-red-600']">{{ p.stock }}</p>
            </div>
          </div>
          <div class="flex gap-2"><button @click="openViewModal(p)"
              class="flex-1 py-2 bg-white border border-neutral-200 rounded-lg text-xs font-semibold text-neutral-600 flex items-center justify-center gap-1">
              <Eye class="w-3.5 h-3.5" />View
            </button><button @click="openEditModal(p)"
              class="flex-1 py-2 bg-neutral-900 rounded-lg text-xs font-semibold text-white flex items-center justify-center gap-1">
              <Edit2 class="w-3.5 h-3.5" />Edit
            </button></div>
        </div>
      </div>

      <table class="hidden lg:table w-full">
        <thead>
          <tr class="bg-neutral-50/50">
            <th class="px-6 py-4 text-left"><button @click="toggleSort('name')"
                class="flex items-center gap-1.5 text-[10px] font-bold text-neutral-400 uppercase hover:text-orange-500">Product
                <ArrowUpDown class="w-3 h-3" />
              </button></th>
            <th class="px-6 py-4 text-left text-[10px] font-bold text-neutral-400 uppercase">Category</th>
            <th class="px-6 py-4 text-left"><button @click="toggleSort('price')"
                class="flex items-center gap-1.5 text-[10px] font-bold text-neutral-400 uppercase hover:text-orange-500">Price
                <ArrowUpDown class="w-3 h-3" />
              </button></th>
            <th class="px-6 py-4 text-left"><button @click="toggleSort('stock')"
                class="flex items-center gap-1.5 text-[10px] font-bold text-neutral-400 uppercase hover:text-orange-500">Stock
                <ArrowUpDown class="w-3 h-3" />
              </button></th>
            <th class="px-6 py-4 text-right text-[10px] font-bold text-neutral-400 uppercase">Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="p in filteredProducts" :key="p.id"
            class="border-b border-neutral-50 hover:bg-neutral-50/50 transition-colors group">
            <td class="px-6 py-4">
              <div class="flex items-center gap-3">
                <div class="w-11 h-11 rounded-xl overflow-hidden bg-neutral-100 shadow-sm"><img v-if="p.image"
                    :src="p.image" class="w-full h-full object-cover" />
                  <div v-else class="w-full h-full flex items-center justify-center text-neutral-300">
                    <Package class="w-5 h-5" />
                  </div>
                </div>
                <div>
                  <p class="text-sm font-semibold text-neutral-800">{{ p.name }}</p>
                  <p class="text-xs text-neutral-500 max-w-[200px] truncate">{{ p.description || 'No description' }}</p>
                </div>
              </div>
            </td>
            <td class="px-6 py-4"><span
                class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-[10px] font-semibold bg-orange-100 text-orange-600">
                <Tag class="w-3 h-3" />{{ p.category }}
              </span></td>
            <td class="px-6 py-4 text-sm font-bold text-neutral-800">${{ p.price }}</td>
            <td class="px-6 py-4"><span
                :class="['px-3 py-1 rounded-full text-[10px] font-bold uppercase', p.stock > 10 ? 'bg-green-100 text-green-600' : p.stock > 0 ? 'bg-orange-100 text-orange-600' : 'bg-red-100 text-red-600']">{{
                  p.stock }} units</span></td>
            <td class="px-6 py-4">
              <div class="flex items-center justify-end gap-1 opacity-0 group-hover:opacity-100"><button
                  @click="openViewModal(p)"
                  class="p-2 rounded-lg text-neutral-400 hover:bg-neutral-100 hover:text-neutral-600">
                  <Eye class="w-4 h-4" />
                </button><button @click="openEditModal(p)"
                  class="p-2 rounded-lg text-neutral-400 hover:bg-orange-100 hover:text-orange-600">
                  <Edit2 class="w-4 h-4" />
                </button><button @click="handleDeleteProduct(p.id)"
                  class="p-2 rounded-lg text-neutral-400 hover:bg-red-100 hover:text-red-600">
                  <Trash2 class="w-4 h-4" />
                </button></div>
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
                    <span class="text-[10px] font-bold text-white/80 uppercase tracking-wider">{{ showAddModal ?'NewProduct' : 'Editing' }}</span>
                  </div>
                  <h2 class="text-xl sm:text-2xl font-bold text-white mb-1">{{ showAddModal ? 'Create Product' :'UpdateProduct' }}</h2>
                  <p class="text-sm text-white/50">{{ showAddModal ? 'Add a new item to your catalog' :'Makechangestoyour product' }}</p>
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

                    <!-- Generated Variants Preview -->
                    <Transition name="fade">
                      <div v-if="generatedVariants.length > 0"
                        class="p-4 bg-linear-to-br from-neutral-50 to-neutral-100/50 rounded-2xl border border-neutral-200/50">
                        <div class="flex items-center justify-between mb-3">
                          <p class="text-[10px] font-bold text-neutral-500 uppercase tracking-wider">Generated Variants
                          </p>
                          <span class="px-2 py-0.5 bg-orange-500 text-white text-[10px] font-bold rounded-full">{{
                            generatedVariants.length }}</span>
                        </div>
                        <div class="flex flex-wrap gap-2">
                          <span v-for="(variant, idx) in generatedVariants" :key="idx"
                            class="inline-flex items-center gap-1.5 px-3 py-1.5 bg-white border border-neutral-200 rounded-lg text-[11px] font-semibold text-neutral-700 shadow-sm">
                            <span class="w-1.5 h-1.5 bg-orange-500 rounded-full"></span>
                            {{ variant.label }}
                          </span>
                        </div>
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
          class="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center p-6 z-50"
          @click.self="closeModals">
          <div class="bg-white rounded-2xl w-full max-w-sm shadow-2xl overflow-hidden">
            <div class="relative h-48 bg-neutral-900"><button @click="closeModals"
                class="absolute top-4 right-4 p-2 bg-white/10 rounded-lg text-white/70 hover:bg-white/20">
                <X class="w-5 h-5" />
              </button>
              <div v-if="currentProduct.image" class="w-full h-full"><img :src="currentProduct.image"
                  class="w-full h-full object-cover" /></div>
              <div v-else class="w-full h-full flex items-center justify-center text-white/50">
                <Package class="w-16 h-16" />
              </div>
            </div>
            <div class="p-6"><span
                class="inline-block text-[10px] font-semibold text-orange-600 bg-orange-100 px-2 py-1 rounded mb-2">{{
                  currentProduct.category }}</span>
              <h3 class="text-xl font-bold text-neutral-800 mb-2">{{ currentProduct.name }}</h3>
              <p class="text-sm text-neutral-500 mb-6">{{ currentProduct.description || 'No description' }}</p>
              <div class="grid grid-cols-2 gap-4 mb-6">
                <div class="bg-neutral-50 rounded-xl p-4 text-center">
                  <p class="text-[10px] font-bold text-neutral-400 uppercase mb-1">Price</p>
                  <p class="text-2xl font-bold text-neutral-800">${{ currentProduct.price }}</p>
                </div>
                <div class="bg-neutral-50 rounded-xl p-4 text-center">
                  <p class="text-[10px] font-bold text-neutral-400 uppercase mb-1">Stock</p>
                  <p :class="['text-2xl font-bold', currentProduct.stock > 0 ? 'text-green-600' : 'text-red-600']">{{
                    currentProduct.stock }}</p>
                </div>
              </div>
              <div class="flex gap-3"><button @click="closeModals"
                  class="flex-1 py-3 bg-neutral-100 rounded-xl text-sm font-semibold text-neutral-600">Close</button><button
                  @click="openEditModal(currentProduct)"
                  class="flex-1 py-3 bg-neutral-900 rounded-xl text-sm font-semibold text-white flex items-center justify-center gap-2">
                  <Edit2 class="w-4 h-4" />Edit
                </button></div>
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