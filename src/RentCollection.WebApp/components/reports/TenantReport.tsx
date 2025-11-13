'use client'

import { useState, useMemo } from 'react'
import { Card, Button, SearchBar, Select, LoadingSpinner, Alert, Badge } from '@/components/common'
import { useGetTenants, useGetProperties } from '@/lib/hooks'
import { useDownloadTenantList } from '@/lib/hooks/useReports'
import { FaDownload, FaUsers, FaCheckCircle, FaTimesCircle } from 'react-icons/fa'

export default function TenantReport() {
  const { data: tenants, loading } = useGetTenants()
  const { data: properties } = useGetProperties()
  const { downloadList, loading: downloading } = useDownloadTenantList()

  const [searchTerm, setSearchTerm] = useState('')
  const [statusFilter, setStatusFilter] = useState<'all' | 'active' | 'inactive'>('all')
  const [propertyFilter, setPropertyFilter] = useState<number | 'all'>('all')
  const [alert, setAlert] = useState<{ type: 'success' | 'error', message: string } | null>(null)

  // Filter tenants
  const filteredTenants = useMemo(() => {
    return tenants.filter(tenant => {
      // Search filter
      const searchLower = searchTerm.toLowerCase()
      const matchesSearch =
        tenant.fullName.toLowerCase().includes(searchLower) ||
        tenant.email.toLowerCase().includes(searchLower) ||
        tenant.phoneNumber.toLowerCase().includes(searchLower) ||
        tenant.unitNumber.toLowerCase().includes(searchLower)

      // Status filter
      const matchesStatus =
        statusFilter === 'all' ||
        (statusFilter === 'active' && tenant.isActive) ||
        (statusFilter === 'inactive' && !tenant.isActive)

      // Property filter
      const matchesProperty =
        propertyFilter === 'all' ||
        tenant.propertyName === properties.find(p => p.id === propertyFilter)?.name

      return matchesSearch && matchesStatus && matchesProperty
    })
  }, [tenants, searchTerm, statusFilter, propertyFilter, properties])

  // Calculate stats
  const stats = useMemo(() => {
    const activeTenants = filteredTenants.filter(t => t.isActive).length
    const inactiveTenants = filteredTenants.filter(t => !t.isActive).length
    const totalMonthlyRent = filteredTenants
      .filter(t => t.isActive)
      .reduce((sum, t) => sum + t.monthlyRent, 0)

    return {
      total: filteredTenants.length,
      active: activeTenants,
      inactive: inactiveTenants,
      totalMonthlyRent,
    }
  }, [filteredTenants])

  const handleDownloadPdf = async () => {
    const success = await downloadList()
    if (success) {
      setAlert({ type: 'success', message: 'Tenant list downloaded successfully!' })
      setTimeout(() => setAlert(null), 3000)
    } else {
      setAlert({ type: 'error', message: 'Failed to download tenant list' })
      setTimeout(() => setAlert(null), 3000)
    }
  }

  const propertyOptions = [
    { value: 'all', label: 'All Properties' },
    ...properties.map(p => ({ value: p.id, label: p.name }))
  ]

  const statusOptions = [
    { value: 'all', label: 'All Statuses' },
    { value: 'active', label: 'Active' },
    { value: 'inactive', label: 'Inactive' },
  ]

  if (loading) {
    return (
      <div className="flex justify-center py-12">
        <LoadingSpinner size="lg" text="Loading tenant data..." />
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold text-gray-900">Tenant Directory</h2>
        <Button variant="primary" onClick={handleDownloadPdf} loading={downloading}>
          <FaDownload className="mr-2" />
          Download PDF
        </Button>
      </div>

      {alert && (
        <Alert
          type={alert.type}
          message={alert.message}
          onClose={() => setAlert(null)}
        />
      )}

      {/* Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card padding="md" className="bg-gradient-to-br from-blue-50 to-blue-100">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-blue-600 font-medium mb-1">Total Tenants</p>
              <p className="text-2xl font-bold text-blue-900">{stats.total}</p>
            </div>
            <FaUsers className="text-3xl text-blue-600" />
          </div>
        </Card>

        <Card padding="md" className="bg-gradient-to-br from-green-50 to-green-100">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-green-600 font-medium mb-1">Active</p>
              <p className="text-2xl font-bold text-green-900">{stats.active}</p>
            </div>
            <FaCheckCircle className="text-3xl text-green-600" />
          </div>
        </Card>

        <Card padding="md" className="bg-gradient-to-br from-orange-50 to-orange-100">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-orange-600 font-medium mb-1">Inactive</p>
              <p className="text-2xl font-bold text-orange-900">{stats.inactive}</p>
            </div>
            <FaTimesCircle className="text-3xl text-orange-600" />
          </div>
        </Card>

        <Card padding="md" className="bg-gradient-to-br from-purple-50 to-purple-100">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-purple-600 font-medium mb-1">Monthly Rent</p>
              <p className="text-2xl font-bold text-purple-900">
                KSh {(stats.totalMonthlyRent / 1000).toFixed(0)}K
              </p>
            </div>
            <FaUsers className="text-3xl text-purple-600" />
          </div>
        </Card>
      </div>

      {/* Filters */}
      <Card padding="md">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <SearchBar
            value={searchTerm}
            onChange={setSearchTerm}
            placeholder="Search by name, email, phone, or unit..."
            className="w-full"
          />
          <Select
            label=""
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value as 'all' | 'active' | 'inactive')}
            options={statusOptions}
            fullWidth
          />
          <Select
            label=""
            value={propertyFilter}
            onChange={(e) => setPropertyFilter(e.target.value === 'all' ? 'all' : Number(e.target.value))}
            options={propertyOptions}
            fullWidth
          />
        </div>

        {(searchTerm || statusFilter !== 'all' || propertyFilter !== 'all') && (
          <div className="flex justify-end mt-3">
            <Button
              variant="secondary"
              size="sm"
              onClick={() => {
                setSearchTerm('')
                setStatusFilter('all')
                setPropertyFilter('all')
              }}
            >
              Clear Filters
            </Button>
          </div>
        )}
      </Card>

      {/* Tenant Table */}
      <Card padding="none">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Name</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Contact</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Unit</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Property</th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">Monthly Rent</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Lease Period</th>
                <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase">Status</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {filteredTenants.map((tenant) => (
                <tr key={tenant.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 whitespace-nowrap">
                    <p className="text-sm font-medium text-gray-900">{tenant.fullName}</p>
                    <p className="text-xs text-gray-500">{tenant.idNumber}</p>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <p className="text-sm text-gray-900">{tenant.email}</p>
                    <p className="text-xs text-gray-500">{tenant.phoneNumber}</p>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <p className="text-sm text-gray-900">{tenant.unitNumber}</p>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <p className="text-sm text-gray-900">{tenant.propertyName}</p>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-right">
                    <p className="text-sm font-semibold text-gray-900">
                      KSh {tenant.monthlyRent.toLocaleString()}
                    </p>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <p className="text-xs text-gray-600">
                      {new Date(tenant.leaseStartDate).toLocaleDateString()} -
                    </p>
                    <p className="text-xs text-gray-600">
                      {tenant.leaseEndDate ? new Date(tenant.leaseEndDate).toLocaleDateString() : 'Ongoing'}
                    </p>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-center">
                    {tenant.isActive ? (
                      <Badge variant="success">Active</Badge>
                    ) : (
                      <Badge variant="default">Inactive</Badge>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          {filteredTenants.length === 0 && (
            <div className="text-center py-12">
              <p className="text-gray-500">No tenants found</p>
            </div>
          )}
        </div>
      </Card>
    </div>
  )
}
