'use client'

import { useState } from 'react'
import { useGetTenants } from '@/lib/hooks'
import TenantList from '@/components/tenants/TenantList'
import { Button, SearchBar, Select, LoadingSpinner, Card, PageHeader } from '@/components/common'
import Link from 'next/link'
import { Plus } from 'lucide-react'

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
        <p className="text-state-error">Error loading tenants: {error.message}</p>
        <Button onClick={refetch} className="mt-4">
          Retry
        </Button>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Tenants & Leases"
        subtitle="Manage tenant profiles, lease status, and payment health."
        breadcrumbs={[
          { label: 'Dashboard', href: '/dashboard' },
          { label: 'Tenants & Leases' },
        ]}
        actions={
          <Link href="/tenants/new">
            <Button>
              <Plus className="mr-2 w-4 h-4" />
              Add Tenant
            </Button>
          </Link>
        }
      />

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

      <TenantList tenants={filteredTenants} onUpdate={refetch} />

      {tenants.length > 0 && (
        <Card padding="md">
          <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4 text-center">
            <div>
              <p className="text-2xl font-semibold text-text-primary">{tenants.length}</p>
              <p className="text-sm text-text-muted">Total Tenants</p>
            </div>
            <div>
              <p className="text-2xl font-semibold text-state-success">{activeTenants}</p>
              <p className="text-sm text-text-muted">Active</p>
            </div>
            <div>
              <p className="text-2xl font-semibold text-text-muted">{inactiveTenants}</p>
              <p className="text-sm text-text-muted">Inactive</p>
            </div>
            <div className="border-l border-border-muted pl-4">
              <p className="text-2xl font-semibold text-state-success">{paidTenants}</p>
              <p className="text-sm text-text-muted">Paid</p>
            </div>
            <div>
              <p className="text-2xl font-semibold text-state-error">{overdueTenants}</p>
              <p className="text-sm text-text-muted">Overdue</p>
            </div>
            <div>
              <p className="text-2xl font-semibold text-state-warning">{pendingTenants}</p>
              <p className="text-sm text-text-muted">Pending</p>
            </div>
          </div>
        </Card>
      )}
    </div>
  )
}
