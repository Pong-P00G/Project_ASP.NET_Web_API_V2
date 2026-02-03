<script setup>
import { ref, computed, onMounted } from 'vue'
import { Users, Plus, Search, Edit2, Trash2, X, Loader2, Download, Eye, EyeOff, Lock, AlertCircle, CheckCircle, RefreshCw, UserPlus, Shield, Mail, Calendar } from 'lucide-vue-next'
import { authAPI } from '../../api/authApi.js'
import api from '../../api/api.js'
import { useToast } from '../../composables/useToast'

const toast = useToast()

const users = ref([])
const loading = ref(false)
const searchTerm = ref('')
const selectedRole = ref('all')
const selectedStatus = ref('all')

const showAddModal = ref(false)
const showEditModal = ref(false)
const showViewModal = ref(false)
const showDeleteModal = ref(false)
const currentUser = ref(null)
const userToDelete = ref(null)

const formData = ref({
  username: '',
  firstName: '',
  midName: '',
  lastName: '',
  email: '',
  password: '',
  confirmPassword: '',
  role: '',
  status: ''
})
const roles = ref(['Admin', 'Customer'])
const showPassword = ref(false)
const showConfirmPassword = ref(false)

const passwordsMatch = computed(() => formData.value.password === formData.value.confirmPassword)
const passwordStrong = computed(() => formData.value.password.length >= 8)
const isAddFormValid = computed(() =>
  formData.value.username &&
  formData.value.firstName &&
  formData.value.lastName &&
  formData.value.email &&
  formData.value.password &&
  formData.value.confirmPassword &&
  formData.value.role &&
  formData.value.status &&
  passwordsMatch.value &&
  passwordStrong.value
)
const isEditFormValid = computed(() =>
  formData.value.firstName &&
  formData.value.lastName &&
  formData.value.email &&
  formData.value.role &&
  formData.value.status
)

const filteredUsers = computed(() => {
  let filtered = users.value
  if (searchTerm.value) filtered = filtered.filter(u => u.name.toLowerCase().includes(searchTerm.value.toLowerCase()) || u.email.toLowerCase().includes(searchTerm.value.toLowerCase()))
  if (selectedRole.value !== 'all') filtered = filtered.filter(u => u.role === selectedRole.value)
  if (selectedStatus.value !== 'all') filtered = filtered.filter(u => u.status === selectedStatus.value)
  return filtered
})

const stats = computed(() => ({
  total: users.value.length,
  active: users.value.filter(u => u.status === 'active').length,
  inactive: users.value.filter(u => u.status === 'inactive').length,
  admins: users.value.filter(u => u.role === 'Admin').length
}))

const fetchUsers = async () => {
  try {
    loading.value = true
    const response = await api.get('/users')
    users.value = (response.data || []).map(u => ({
      id: u.userId,
      username: u.username,
      name: u.fullName || `${u.firstName || ''} ${u.midName || ''} ${u.lastName || ''}`.replace(/\s+/g, ' ').trim(),
      firstName: u.firstName,
      midName: u.midName,
      lastName: u.lastName,
      email: u.email,
      role: u.roleName || (u.roleId === 1 ? 'Admin' : 'Customer'),
      status: u.isActive ? 'active' : 'inactive',
      phone: 'N/A',
      createdAt: new Date(u.createdAt).toLocaleDateString()
    }))
  } catch (err) { toast.error(err.response?.data?.message || 'Failed to load users') } finally { loading.value = false }
}

const openAddModal = () => {
  formData.value = { username: '', firstName: '', midName: '', lastName: '', email: '', password: '', confirmPassword: '', role: '', status: 'active' }
  showPassword.value = false
  showConfirmPassword.value = false
  showAddModal.value = true
}
const openEditModal = (user) => {
  currentUser.value = user
  formData.value = { username: user.username || '', firstName: user.firstName || '', midName: user.midName || '', lastName: user.lastName || '', email: user.email, password: '', confirmPassword: '', role: user.role, status: user.status }
  showEditModal.value = true
}
const openViewModal = (user) => { currentUser.value = user; showViewModal.value = true }
const closeModals = () => { showAddModal.value = false; showEditModal.value = false; showViewModal.value = false; currentUser.value = null }

const handleAddUser = async () => {
  try {
    loading.value = true
    await api.post('/users', { username: formData.value.username, email: formData.value.email, password: formData.value.password, firstName: formData.value.firstName, midName: formData.value.midName || null, lastName: formData.value.lastName, roleId: formData.value.role === 'Admin' ? 1 : 2, isActive: formData.value.status === 'active' })
    await fetchUsers()
    toast.success('User created successfully!')
    closeModals()
  } catch (err) { toast.error(err.response?.data?.message || 'Failed to add user') } finally { loading.value = false }
}

const handleUpdateUser = async () => {
  try {
    loading.value = true
    await api.put(`/users/${currentUser.value.id}`, { firstName: formData.value.firstName, midName: formData.value.midName || null, lastName: formData.value.lastName, email: formData.value.email, roleId: formData.value.role === 'Admin' ? 1 : 2, isActive: formData.value.status === 'active' })
    await fetchUsers()
    toast.success('User updated successfully!')
    closeModals()
  } catch (err) { toast.error(err.response?.data?.message || 'Failed to update user') } finally { loading.value = false }
}

const handleDeleteUser = async (id) => { userToDelete.value = users.value.find(u => u.id === id); showDeleteModal.value = true }

const confirmDelete = async () => {
  if (!userToDelete.value) return
  try {
    loading.value = true
    await api.delete(`/users/${userToDelete.value.id}`)
    users.value = users.value.filter(u => u.id !== userToDelete.value.id)
    toast.success('User deleted successfully!')
    showDeleteModal.value = false
    userToDelete.value = null
  } catch (err) { toast.error(err.response?.data?.message || 'Failed to delete user') } finally { loading.value = false }
}

const cancelDelete = () => { showDeleteModal.value = false; userToDelete.value = null }

const exportToCSV = () => {
  const csv = [['ID', 'Name', 'Email', 'Role', 'Status', 'Created At'], ...users.value.map(u => [u.id, u.name, u.email, u.role, u.status, u.createdAt])].map(row => row.join(',')).join('\n')
  const blob = new Blob([csv], { type: 'text/csv' })
  const a = document.createElement('a'); a.href = URL.createObjectURL(blob); a.download = 'users.csv'; a.click()
}

onMounted(fetchUsers)
</script>

<template>
  <div class="w-full">
    <!-- Header -->
    <div class="flex flex-wrap items-center justify-between gap-4 mb-8">
      <div>
        <div
          class="inline-flex items-center gap-2 px-4 py-2 bg-gradient-to-r from-blue-500/20 to-cyan-500/20 backdrop-blur-sm border border-blue-500/20 rounded-full text-blue-400 text-xs font-semibold mb-3">
          <Users class="w-3.5 h-3.5" /><span>User Management</span>
        </div>
        <h1 class="text-3xl font-black text-white tracking-tight">Team Members</h1>
        <p class="text-neutral-400 text-sm mt-1">Manage system access and user roles</p>
      </div>
      <div class="flex gap-3">
        <button @click="fetchUsers" :disabled="loading"
          class="flex items-center gap-2 px-5 py-3 bg-white/5 backdrop-blur-sm border border-white/10 rounded-xl text-sm font-semibold text-white hover:bg-white/10 transition-all">
          <RefreshCw class="w-4 h-4" :class="{ 'animate-spin': loading }" />Sync
        </button>
        <button @click="openAddModal"
          class="group flex items-center gap-2 px-6 py-3 bg-gradient-to-r from-blue-500 to-cyan-500 rounded-xl text-sm font-bold text-white shadow-lg shadow-blue-500/25 hover:shadow-blue-500/40 hover:scale-105 transition-all duration-300">
          <UserPlus class="w-4 h-4" />New User
        </button>
      </div>
    </div>

    <!-- Stats -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5 mb-8">
      <div
        class="stat-card group relative bg-white/5 backdrop-blur-xl rounded-2xl p-6 border border-white/10 hover:border-blue-500/50 hover:-translate-y-2 hover:shadow-2xl hover:shadow-blue-500/10 transition-all duration-500">
        <div
          class="absolute inset-0 bg-gradient-to-br from-blue-500/10 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-500 rounded-2xl">
        </div>
        <div class="relative z-10 flex items-center gap-4">
          <div
            class="w-14 h-14 flex items-center justify-center bg-gradient-to-br from-blue-400 to-blue-600 rounded-2xl text-white shadow-lg shadow-blue-500/30 group-hover:scale-110 transition-transform duration-300">
            <Users class="w-6 h-6" />
          </div>
          <div>
            <p class="text-xs font-semibold text-neutral-400 uppercase tracking-widest">Total Users</p>
            <h3 class="text-3xl font-black text-white tracking-tight">{{ stats.total }}</h3>
          </div>
        </div>
      </div>
      <div
        class="stat-card group relative bg-white/5 backdrop-blur-xl rounded-2xl p-6 border border-white/10 hover:border-emerald-500/50 hover:-translate-y-2 hover:shadow-2xl hover:shadow-emerald-500/10 transition-all duration-500">
        <div
          class="absolute inset-0 bg-gradient-to-br from-emerald-500/10 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-500 rounded-2xl">
        </div>
        <div class="relative z-10 flex items-center gap-4">
          <div
            class="w-14 h-14 flex items-center justify-center bg-gradient-to-br from-emerald-400 to-emerald-600 rounded-2xl text-white shadow-lg shadow-emerald-500/30 group-hover:scale-110 transition-transform duration-300">
            <CheckCircle class="w-6 h-6" />
          </div>
          <div>
            <p class="text-xs font-semibold text-neutral-400 uppercase tracking-widest">Active Users</p>
            <h3 class="text-3xl font-black text-white tracking-tight">{{ stats.active }}</h3>
          </div>
        </div>
      </div>
      <div
        class="stat-card group relative bg-white/5 backdrop-blur-xl rounded-2xl p-6 border border-white/10 hover:border-red-500/50 hover:-translate-y-2 hover:shadow-2xl hover:shadow-red-500/10 transition-all duration-500">
        <div
          class="absolute inset-0 bg-gradient-to-br from-red-500/10 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-500 rounded-2xl">
        </div>
        <div class="relative z-10 flex items-center gap-4">
          <div
            class="w-14 h-14 flex items-center justify-center bg-gradient-to-br from-red-400 to-red-600 rounded-2xl text-white shadow-lg shadow-red-500/30 group-hover:scale-110 transition-transform duration-300">
            <AlertCircle class="w-6 h-6" />
          </div>
          <div>
            <p class="text-xs font-semibold text-neutral-400 uppercase tracking-widest">Inactive</p>
            <h3 class="text-3xl font-black text-white tracking-tight">{{ stats.inactive }}</h3>
          </div>
        </div>
      </div>
      <div
        class="stat-card group relative bg-white/5 backdrop-blur-xl rounded-2xl p-6 border border-white/10 hover:border-orange-500/50 hover:-translate-y-2 hover:shadow-2xl hover:shadow-orange-500/10 transition-all duration-500">
        <div
          class="absolute inset-0 bg-gradient-to-br from-orange-500/10 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-500 rounded-2xl">
        </div>
        <div class="relative z-10 flex items-center gap-4">
          <div
            class="w-14 h-14 flex items-center justify-center bg-gradient-to-br from-orange-400 to-pink-500 rounded-2xl text-white shadow-lg shadow-orange-500/30 group-hover:scale-110 transition-transform duration-300">
            <Shield class="w-6 h-6" />
          </div>
          <div>
            <p class="text-xs font-semibold text-neutral-400 uppercase tracking-widest">Administrators</p>
            <h3 class="text-3xl font-black text-white tracking-tight">{{ stats.admins }}</h3>
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
          <input v-model="searchTerm" type="text" placeholder="Search users..."
            class="w-full pl-10 pr-4 py-3 bg-white/5 border border-white/10 rounded-xl text-sm text-white placeholder-neutral-500 focus:bg-white/10 focus:border-blue-500/50 outline-none transition-all" />
        </div>
        <div class="flex flex-wrap gap-3">
          <select v-model="selectedRole"
            class="px-4 py-3 bg-white/5 border border-white/10 rounded-xl text-xs font-semibold text-neutral-300 focus:border-blue-500/50 outline-none">
            <option value="all" class="bg-neutral-900">All Roles</option>
            <option v-for="role in roles" :key="role" :value="role" class="bg-neutral-900">{{ role }}</option>
          </select>
          <select v-model="selectedStatus"
            class="px-4 py-3 bg-white/5 border border-white/10 rounded-xl text-xs font-semibold text-neutral-300 focus:border-blue-500/50 outline-none">
            <option value="all" class="bg-neutral-900">Any Status</option>
            <option value="active" class="bg-neutral-900">Active</option>
            <option value="inactive" class="bg-neutral-900">Inactive</option>
          </select>
          <button @click="exportToCSV"
            class="flex items-center gap-2 px-4 py-3 bg-gradient-to-r from-blue-500 to-cyan-500 text-white rounded-xl text-xs font-bold hover:shadow-lg hover:shadow-blue-500/25 transition-all">
            <Download class="w-4 h-4" />Export
          </button>
        </div>
      </div>

      <!-- Loading -->
      <div v-if="loading && users.length === 0" class="p-8 space-y-3">
        <div v-for="i in 5" :key="i" class="h-16 bg-white/5 rounded-xl animate-pulse"></div>
      </div>

      <!-- Empty -->
      <div v-else-if="filteredUsers.length === 0" class="flex flex-col items-center justify-center py-16 text-center">
        <div
          class="w-20 h-20 flex items-center justify-center bg-white/5 rounded-2xl text-neutral-500 mb-6 border border-white/10">
          <Users class="w-10 h-10" />
        </div>
        <h3 class="text-xl font-bold text-white mb-2">No Users Found</h3>
        <p class="text-neutral-400 text-sm mb-6">Try adjusting your filters or add a new user</p>
        <button @click="openAddModal"
          class="flex items-center gap-2 px-6 py-3 bg-gradient-to-r from-blue-500 to-cyan-500 rounded-xl text-sm font-bold text-white shadow-lg shadow-blue-500/25">
          <UserPlus class="w-4 h-4" />Add New User
        </button>
      </div>

      <!-- Table -->
      <table v-else class="w-full">
        <thead>
          <tr class="bg-white/5">
            <th class="px-6 py-4 text-left text-xs font-semibold text-neutral-400 uppercase tracking-wider">User</th>
            <th class="px-6 py-4 text-left text-xs font-semibold text-neutral-400 uppercase tracking-wider">Role</th>
            <th class="px-6 py-4 text-left text-xs font-semibold text-neutral-400 uppercase tracking-wider">Status</th>
            <th class="px-6 py-4 text-left text-xs font-semibold text-neutral-400 uppercase tracking-wider">Joined</th>
            <th class="px-6 py-4 text-right text-xs font-semibold text-neutral-400 uppercase tracking-wider">Actions
            </th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="user in filteredUsers" :key="user.id"
            class="border-b border-white/5 hover:bg-white/5 transition-colors group">
            <td class="px-6 py-4">
              <div class="flex items-center gap-3">
                <div
                  class="w-11 h-11 flex items-center justify-center bg-gradient-to-br from-blue-500 to-cyan-500 text-white font-bold text-sm rounded-xl shadow-lg shadow-blue-500/20 group-hover:scale-110 transition-transform duration-300">
                  {{ user.name.charAt(0) }}
                </div>
                <div>
                  <p class="text-sm font-semibold text-white group-hover:text-blue-400 transition-colors">{{ user.name
                    }}</p>
                  <p class="text-xs text-neutral-500">{{ user.email }}</p>
                </div>
              </div>
            </td>
            <td class="px-6 py-4">
              <span
                :class="['inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-semibold', user.role === 'Admin' ? 'bg-orange-500/20 text-orange-400 border border-orange-500/30' : 'bg-white/10 text-neutral-300 border border-white/10']">
                <Shield v-if="user.role === 'Admin'" class="w-3 h-3" />{{ user.role }}
              </span>
            </td>
            <td class="px-6 py-4">
              <span
                :class="['px-3 py-1.5 rounded-full text-xs font-bold uppercase', user.status === 'active' ? 'bg-emerald-500/20 text-emerald-400 border border-emerald-500/30' : 'bg-red-500/20 text-red-400 border border-red-500/30']">{{
                user.status }}</span>
            </td>
            <td class="px-6 py-4 text-sm text-neutral-400">{{ user.createdAt }}</td>
            <td class="px-6 py-4">
              <div class="flex items-center justify-end gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                <button @click="openViewModal(user)"
                  class="p-2 rounded-lg text-neutral-400 hover:bg-white/10 hover:text-white transition-all">
                  <Eye class="w-4 h-4" />
                </button>
                <button @click="openEditModal(user)"
                  class="p-2 rounded-lg text-neutral-400 hover:bg-blue-500/20 hover:text-blue-400 transition-all">
                  <Edit2 class="w-4 h-4" />
                </button>
                <button @click="handleDeleteUser(user.id)"
                  class="p-2 rounded-lg text-neutral-400 hover:bg-red-500/20 hover:text-red-400 transition-all">
                  <Trash2 class="w-4 h-4" />
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Modals -->
    <Teleport to="body">
      <Transition name="modal">
        <div v-if="showAddModal || showEditModal"
          class="fixed inset-0 bg-black/70 backdrop-blur-md flex items-center justify-center p-6 z-50"
          @click.self="closeModals">
          <div class="bg-neutral-900 border border-white/10 rounded-3xl w-full max-w-lg shadow-2xl">
            <div class="flex items-center justify-between p-6 border-b border-white/10">
              <div class="flex items-center gap-4">
                <div
                  class="w-11 h-11 flex items-center justify-center bg-gradient-to-br from-blue-500 to-cyan-500 rounded-xl text-white shadow-lg shadow-blue-500/25">
                  <UserPlus class="w-5 h-5" />
                </div>
                <div>
                  <h3 class="text-lg font-bold text-white">{{ showAddModal ? 'Create User' : 'Edit User' }}</h3>
                  <p class="text-xs font-semibold text-neutral-500 uppercase tracking-wider">Account Configuration</p>
                </div>
              </div>
              <button @click="closeModals"
                class="p-2 rounded-lg text-neutral-400 hover:bg-white/10 hover:text-white transition-all">
                <X class="w-5 h-5" />
              </button>
            </div>
            <form @submit.prevent="showAddModal ? handleAddUser() : handleUpdateUser()">
              <div class="p-6 space-y-5 max-h-[60vh] overflow-y-auto">
                <div v-if="showAddModal">
                  <label class="block text-xs font-semibold text-neutral-400 uppercase tracking-wider mb-2">Username
                    *</label>
                  <input v-model="formData.username" type="text"
                    class="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-xl text-sm text-white placeholder-neutral-500 focus:bg-white/10 focus:border-blue-500/50 outline-none transition-all"
                    placeholder="johndoe" />
                </div>
                <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
                  <div>
                    <label class="block text-xs font-semibold text-neutral-400 uppercase tracking-wider mb-2">First Name
                      *</label>
                    <input v-model="formData.firstName" type="text"
                      class="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-xl text-sm text-white placeholder-neutral-500 focus:bg-white/10 focus:border-blue-500/50 outline-none transition-all"
                      placeholder="John" />
                  </div>
                  <div>
                    <label class="block text-xs font-semibold text-neutral-400 uppercase tracking-wider mb-2">Middle
                      Name</label>
                    <input v-model="formData.midName" type="text"
                      class="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-xl text-sm text-white placeholder-neutral-500 focus:bg-white/10 focus:border-blue-500/50 outline-none transition-all"
                      placeholder="William" />
                  </div>
                  <div>
                    <label class="block text-xs font-semibold text-neutral-400 uppercase tracking-wider mb-2">Last Name
                      *</label>
                    <input v-model="formData.lastName" type="text"
                      class="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-xl text-sm text-white placeholder-neutral-500 focus:bg-white/10 focus:border-blue-500/50 outline-none transition-all"
                      placeholder="Doe" />
                  </div>
                </div>
                <div>
                  <label class="block text-xs font-semibold text-neutral-400 uppercase tracking-wider mb-2">Email
                    *</label>
                  <input v-model="formData.email" type="email"
                    class="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-xl text-sm text-white placeholder-neutral-500 focus:bg-white/10 focus:border-blue-500/50 outline-none transition-all"
                    placeholder="john@example.com" />
                </div>
                <div v-if="showAddModal" class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div>
                    <label class="block text-xs font-semibold text-neutral-400 uppercase tracking-wider mb-2">Password
                      *</label>
                    <div class="relative">
                      <Lock class="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-neutral-500" />
                      <input v-model="formData.password" :type="showPassword ? 'text' : 'password'"
                        class="w-full pl-11 pr-11 py-3 bg-white/5 border border-white/10 rounded-xl text-sm text-white placeholder-neutral-500 focus:bg-white/10 focus:border-blue-500/50 outline-none transition-all"
                        placeholder="••••••••" />
                      <button type="button" @click="showPassword = !showPassword"
                        class="absolute right-4 top-1/2 -translate-y-1/2 text-neutral-500 hover:text-white transition-colors">
                        <Eye v-if="showPassword" class="w-4 h-4" />
                        <EyeOff v-else class="w-4 h-4" />
                      </button>
                    </div>
                    <p v-if="formData.password && !passwordStrong" class="mt-1 text-xs text-orange-400">Min 8 characters
                    </p>
                  </div>
                  <div>
                    <label class="block text-xs font-semibold text-neutral-400 uppercase tracking-wider mb-2">Confirm
                      Password *</label>
                    <div class="relative">
                      <Lock class="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-neutral-500" />
                      <input v-model="formData.confirmPassword" :type="showConfirmPassword ? 'text' : 'password'"
                        class="w-full pl-11 pr-11 py-3 bg-white/5 border border-white/10 rounded-xl text-sm text-white placeholder-neutral-500 focus:bg-white/10 focus:border-blue-500/50 outline-none transition-all"
                        placeholder="••••••••" />
                      <button type="button" @click="showConfirmPassword = !showConfirmPassword"
                        class="absolute right-4 top-1/2 -translate-y-1/2 text-neutral-500 hover:text-white transition-colors">
                        <Eye v-if="showConfirmPassword" class="w-4 h-4" />
                        <EyeOff v-else class="w-4 h-4" />
                      </button>
                    </div>
                    <p v-if="formData.confirmPassword && !passwordsMatch" class="mt-1 text-xs text-red-400">Passwords do
                      not match</p>
                  </div>
                </div>
                <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div>
                    <label class="block text-xs font-semibold text-neutral-400 uppercase tracking-wider mb-2">Role
                      *</label>
                    <select v-model="formData.role"
                      class="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-xl text-sm text-white focus:bg-white/10 focus:border-blue-500/50 outline-none transition-all">
                      <option value="" class="bg-neutral-900">Select Role</option>
                      <option v-for="role in roles" :key="role" :value="role" class="bg-neutral-900">{{ role }}</option>
                    </select>
                  </div>
                  <div>
                    <label class="block text-xs font-semibold text-neutral-400 uppercase tracking-wider mb-2">Status
                      *</label>
                    <select v-model="formData.status"
                      class="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-xl text-sm text-white focus:bg-white/10 focus:border-blue-500/50 outline-none transition-all">
                      <option value="" class="bg-neutral-900">Select Status</option>
                      <option value="active" class="bg-neutral-900">Active</option>
                      <option value="inactive" class="bg-neutral-900">Inactive</option>
                    </select>
                  </div>
                </div>
              </div>
              <div class="flex gap-3 p-6 bg-white/5 border-t border-white/10">
                <button type="button" @click="closeModals"
                  class="flex-1 py-3 bg-white/5 border border-white/10 rounded-xl text-sm font-semibold text-neutral-300 hover:bg-white/10 transition-all">Cancel</button>
                <button type="submit" :disabled="loading || (showAddModal ? !isAddFormValid : !isEditFormValid)"
                  class="flex-1 py-3 bg-gradient-to-r from-blue-500 to-cyan-500 rounded-xl text-sm font-bold text-white disabled:opacity-50 hover:shadow-lg hover:shadow-blue-500/25 transition-all flex items-center justify-center gap-2">
                  <Loader2 v-if="loading" class="w-4 h-4 animate-spin" />{{ loading ? 'Processing...' : (showAddModal ?
                    'Create User' : 'Update User') }}
                </button>
              </div>
            </form>
          </div>
        </div>
      </Transition>

      <Transition name="modal">
        <div v-if="showViewModal && currentUser"
          class="fixed inset-0 bg-black/70 backdrop-blur-md flex items-center justify-center p-6 z-50"
          @click.self="closeModals">
          <div class="bg-neutral-900 border border-white/10 rounded-3xl w-full max-w-sm shadow-2xl overflow-hidden">
            <div class="relative p-8 bg-gradient-to-br from-blue-500/20 to-cyan-500/20 text-center">
              <button @click="closeModals"
                class="absolute top-4 right-4 p-2 bg-white/10 rounded-lg text-white/70 hover:bg-white/20 hover:text-white transition-all">
                <X class="w-5 h-5" />
              </button>
              <div
                class="w-20 h-20 flex items-center justify-center bg-gradient-to-br from-blue-500 to-cyan-500 text-white text-3xl font-bold rounded-2xl mx-auto mb-4 shadow-lg shadow-blue-500/30">
                {{ currentUser.name.charAt(0) }}
              </div>
              <h3 class="text-xl font-bold text-white">{{ currentUser.name }}</h3>
              <span
                class="inline-block mt-2 px-3 py-1 bg-gradient-to-r from-orange-500 to-pink-500 rounded-full text-xs font-semibold text-white">{{
                currentUser.role }}</span>
            </div>
            <div class="p-6 space-y-4">
              <div class="flex items-center gap-4 py-3 border-b border-white/10">
                <Mail class="w-5 h-5 text-blue-400" />
                <div class="flex-1 flex justify-between"><span
                    class="text-xs font-semibold text-neutral-500 uppercase">Email</span><span
                    class="text-sm font-semibold text-white">{{ currentUser.email }}</span></div>
              </div>
              <div class="flex items-center gap-4 py-3 border-b border-white/10">
                <CheckCircle class="w-5 h-5 text-blue-400" />
                <div class="flex-1 flex justify-between items-center"><span
                    class="text-xs font-semibold text-neutral-500 uppercase">Status</span><span
                    :class="['px-2 py-0.5 rounded-full text-xs font-bold', currentUser.status === 'active' ? 'bg-emerald-500/20 text-emerald-400' : 'bg-red-500/20 text-red-400']">{{
                    currentUser.status }}</span></div>
              </div>
              <div class="flex items-center gap-4 py-3">
                <Calendar class="w-5 h-5 text-blue-400" />
                <div class="flex-1 flex justify-between"><span
                    class="text-xs font-semibold text-neutral-500 uppercase">Joined</span><span
                    class="text-sm font-semibold text-white">{{ currentUser.createdAt }}</span></div>
              </div>
              <div class="flex gap-3 pt-4">
                <button @click="closeModals"
                  class="flex-1 py-3 bg-white/5 border border-white/10 rounded-xl text-sm font-semibold text-neutral-300 hover:bg-white/10 transition-all">Close</button>
                <button @click="openEditModal(currentUser)"
                  class="flex-1 py-3 bg-gradient-to-r from-blue-500 to-cyan-500 rounded-xl text-sm font-bold text-white flex items-center justify-center gap-2 hover:shadow-lg hover:shadow-blue-500/25 transition-all">
                  <Edit2 class="w-4 h-4" />Edit Profile
                </button>
              </div>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- Delete Confirmation Modal -->
    <Teleport to="body">
      <Transition name="modal">
        <div v-if="showDeleteModal"
          class="fixed inset-0 bg-black/70 backdrop-blur-md flex items-center justify-center p-6 z-50"
          @click.self="cancelDelete">
          <div class="bg-neutral-900 border border-white/10 rounded-3xl w-full max-w-sm shadow-2xl overflow-hidden">
            <div class="p-6 text-center">
              <div
                class="w-16 h-16 flex items-center justify-center bg-red-500/20 rounded-full mx-auto mb-4 border border-red-500/30">
                <Trash2 class="w-8 h-8 text-red-400" />
              </div>
              <h3 class="text-xl font-bold text-white mb-2">Delete User?</h3>
              <p class="text-sm text-neutral-400 mb-2">Are you sure you want to delete this user? This action cannot be
                undone.</p>
              <div v-if="userToDelete" class="bg-white/5 border border-white/10 rounded-xl p-4 mt-4">
                <div class="flex items-center gap-3">
                  <div
                    class="w-10 h-10 flex items-center justify-center bg-gradient-to-br from-blue-500 to-cyan-500 text-white rounded-full text-sm font-bold">
                    {{ userToDelete.name?.charAt(0) || 'U' }}
                  </div>
                  <div class="text-left">
                    <p class="text-sm font-bold text-white">{{ userToDelete.name }}</p>
                    <p class="text-xs text-neutral-500">{{ userToDelete.email }}</p>
                  </div>
                </div>
              </div>
            </div>
            <div class="flex gap-3 p-6 pt-0">
              <button @click="cancelDelete"
                class="flex-1 py-3.5 bg-white/5 border border-white/10 rounded-xl text-sm font-semibold text-neutral-300 hover:bg-white/10 transition-all">Cancel</button>
              <button @click="confirmDelete" :disabled="loading"
                class="flex-1 py-3.5 bg-gradient-to-r from-red-500 to-pink-500 rounded-xl text-sm font-bold text-white hover:shadow-lg hover:shadow-red-500/25 transition-all flex items-center justify-center gap-2">
                <Loader2 v-if="loading" class="w-4 h-4 animate-spin" />
                <Trash2 v-else class="w-4 h-4" />
                {{ loading ? 'Deleting...' : 'Delete' }}
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
  transition: opacity 0.3s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}

.glass-card {
  box-shadow: 0 0 0 1px rgba(255, 255, 255, 0.05), 0 25px 50px -12px rgba(0, 0, 0, 0.25);
}

.stat-card {
  box-shadow: 0 0 0 1px rgba(255, 255, 255, 0.05), 0 10px 40px -10px rgba(0, 0, 0, 0.3);
}
</style>
