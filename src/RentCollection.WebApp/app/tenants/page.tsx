'use client'

import { useState } from 'react'
import { useGetTenants } from '@/lib/hooks'
import TenantList from '@/components/tenants/TenantList'
import { Button, SearchBar, Select, LoadingSpinner } from '@/components/common'
import Link from 'next/link'
import { FaPlus } from 'react-icons/fa'

export default function TenantsPage() {
  const { data: tenants, loading, error, refetch } = useGetTenants()
  const [searchTerm, setSearchTerm] = useState('')
  const [filterStatus, setFilterStatus] = useState<string>('all')

  const filteredTenants = tenants.filter((tenant) => {
    const matchesSearch =
      tenant.fullName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      tenant.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
      tenant.phoneNumber.includes(searchTerm) ||
      tenant.unitNumber.toLowerCase().includes(searchTerm.toLowerCase())

    const matchesStatus =
      filterStatus === 'all' ||
      (filterStatus === 'active' && tenant.isActive) ||
      (filterStatus === 'inactive' && !tenant.isActive)

    return matchesSearch && matchesStatus
  })

  const activeTenants = tenants.filter(t => t.isActive).length
  const inactiveTenants = tenants.filter(t => !t.isActive).length
  const paidTenants = tenants.filter(t => t.isActive && t.paymentStatus === 'Paid').length
  const overdueTenants = tenants.filter(t => t.isActive && t.paymentStatus === 'Overdue').length
  const pendingTenants = tenants.filter(t => t.isActive && t.paymentStatus === 'Pending').length

  if (loading) {
    return (
      <div className="flex items-center justify-center h-96">
        <LoadingSpinner size="lg" text="Loading tenants..." />
      </div>
    )
  }

  if (error) {
    return (
      <div className="text-center py-12">
        <p className="text-red-600">Error loading tenants: {error.message}</p>
        <Button onClick={refetch} className="mt-4">
          Retry
        </Button>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-3xl font-serif font-normal text-primary tracking-wide">Tenants</h1>
          <p className="text-primary/60 mt-2 tracking-wide">Manage tenant information and leases</p>
        </div>
        <Link href="/tenants/new">
          <Button variant="primary">
            <FaPlus className="mr-2" />
            Add Tenant
          </Button>
        </Link>
      </div>

      {/* Search and Filters */}
      <div className="flex flex-col sm:flex-row gap-4">
        <div className="flex-1">
          <SearchBar
            value={searchTerm}
            onChange={setSearchTerm}
            placeholder="Search by name, email, phone, or unit..."
          />
        </div>
        <div className="w-full sm:w-48">
          <Select
            value={filterStatus}
            onChange={(e) => setFilterStatus(e.target.value)}
            options={[
              { value: 'all', label: 'All Tenants' },
              { value: 'active', label: 'Active Only' },
              { value: 'inactive', label: 'Inactive Only' },
            ]}
          />
        </div>
      </div>

      {/* Tenants List */}
      <TenantList tenants={filteredTenants} onUpdate={refetch} />

      {/* Stats */}
      {tenants.length > 0 && (
        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4 text-center">
            <div>
              <p className="text-2xl font-bold text-gray-900">{tenants.length}</p>
              <p className="text-sm text-gray-600">Total Tenants</p>
            </div>
            <div>
              <p className="text-2xl font-bold text-green-600">{activeTenants}</p>
              <p className="text-sm text-gray-600">Active</p>
            </div>
            <div>
              <p className="text-2xl font-bold text-gray-400">{inactiveTenants}</p>
              <p className="text-sm text-gray-600">Inactive</p>
            </div>
            <div className="border-l border-gray-200 pl-4">
              <p className="text-2xl font-bold text-green-600">{paidTenants}</p>
              <p className="text-sm text-gray-600">Paid</p>
            </div>
            <div>
              <p className="text-2xl font-bold text-red-600">{overdueTenants}</p>
              <p className="text-sm text-gray-600">Overdue</p>
            </div>
            <div>
              <p className="text-2xl font-bold text-yellow-600">{pendingTenants}</p>
              <p className="text-sm text-gray-600">Pending</p>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
