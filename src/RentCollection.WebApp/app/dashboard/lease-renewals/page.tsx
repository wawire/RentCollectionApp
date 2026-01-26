'use client'

import { useState, useEffect } from 'react'
import { leaseRenewalService } from '@/lib/services/leaseRenewalService'
import {
  LeaseRenewal,
  LeaseRenewalStatus,
  CreateLeaseRenewalDto,
  RejectLeaseRenewalDto,
} from '@/lib/types/leaseRenewal.types'
import ProtectedRoute from '@/components/auth/ProtectedRoute'

export default function LeaseRenewalsDashboardPage() {
  const [renewals, setRenewals] = useState<LeaseRenewal[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [showCreateModal, setShowCreateModal] = useState(false)
  const [createData, setCreateData] = useState({
    tenantId: '',
    proposedLeaseEndDate: '',
    proposedRentAmount: '',
    landlordTerms: '',
    notes: '',
  })

  useEffect(() => {
    fetchRenewals()
  }, [])

  const fetchRenewals = async () => {
    try {
      setLoading(true)
      const data = await leaseRenewalService.getAll()
      setRenewals(data)
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch lease renewals')
    } finally {
      setLoading(false)
    }
  }

  const handleCreate = async () => {
    try {
      const dto: CreateLeaseRenewalDto = {
        tenantId: parseInt(createData.tenantId),
        proposedLeaseEndDate: createData.proposedLeaseEndDate,
        proposedRentAmount: parseFloat(createData.proposedRentAmount),
        landlordTerms: createData.landlordTerms || undefined,
        notes: createData.notes || undefined,
      }
      await leaseRenewalService.create(dto)
      setShowCreateModal(false)
      setCreateData({ tenantId: '', proposedLeaseEndDate: '', proposedRentAmount: '', landlordTerms: '', notes: '' })
      await fetchRenewals()
    } catch (err: any) {
      alert(err.response?.data?.message || 'Failed to create lease renewal')
    }
  }

  const handleApprove = async (id: number) => {
    if (!confirm('Approve this lease renewal?')) return
    try {
      await leaseRenewalService.landlordApprove(id)
      await fetchRenewals()
    } catch (err: any) {
      alert(err.response?.data?.message || 'Failed to approve renewal')
    }
  }

  const handleReject = async (id: number) => {
    const reason = prompt('Enter rejection reason:')
    if (!reason) return
    try {
      await leaseRenewalService.landlordReject(id, { reason })
      await fetchRenewals()
    } catch (err: any) {
      alert(err.response?.data?.message || 'Failed to reject renewal')
    }
  }

  const handleComplete = async (id: number) => {
    if (!confirm('Complete this renewal? This will update the tenant\'s lease.')) return
    try {
      await leaseRenewalService.complete(id)
      await fetchRenewals()
    } catch (err: any) {
      alert(err.response?.data?.message || 'Failed to complete renewal')
    }
  }

  const handleDelete = async (id: number) => {
    if (!confirm('Delete this lease renewal?')) return
    try {
      await leaseRenewalService.delete(id)
      await fetchRenewals()
    } catch (err: any) {
      alert(err.response?.data?.message || 'Failed to delete renewal')
    }
  }

  const getStatusColor = (status: LeaseRenewalStatus) => {
    const colors = {
      [LeaseRenewalStatus.Pending]: 'bg-yellow-100 text-yellow-800',
      [LeaseRenewalStatus.TenantAccepted]: 'bg-blue-100 text-blue-800',
      [LeaseRenewalStatus.TenantRejected]: 'bg-red-100 text-red-800',
      [LeaseRenewalStatus.LandlordApproved]: 'bg-green-100 text-green-800',
      [LeaseRenewalStatus.LandlordRejected]: 'bg-red-100 text-red-800',
      [LeaseRenewalStatus.Completed]: 'bg-green-100 text-green-800',
      [LeaseRenewalStatus.Cancelled]: 'bg-gray-100 text-gray-800',
    }
    return colors[status] || 'bg-gray-100 text-gray-800'
  }

  const stats = {
    total: renewals.length,
    pending: renewals.filter((r) => r.status === LeaseRenewalStatus.Pending).length,
    tenantAccepted: renewals.filter((r) => r.status === LeaseRenewalStatus.TenantAccepted).length,
    completed: renewals.filter((r) => r.status === LeaseRenewalStatus.Completed).length,
    expiringSoon: renewals.filter((r) => r.isExpiringSoon).length,
  }

  return (
    <ProtectedRoute allowedRoles={['PlatformAdmin', 'Landlord', 'Manager', 'Caretaker']}>
      <div className="max-w-7xl mx-auto">
      <div className="mb-6 flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Lease Renewals</h1>
          <p className="mt-2 text-gray-600">Manage tenant lease renewals</p>
        </div>
        <button
          onClick={() => setShowCreateModal(true)}
          className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
        >
          Create Renewal
        </button>
      </div>

      {error && (
        <div className="mb-6 bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">{error}</div>
      )}

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-5 gap-4 mb-6">
        <div className="bg-white rounded-lg shadow p-4">
          <p className="text-sm text-gray-600">Total</p>
          <p className="text-2xl font-bold text-gray-900">{stats.total}</p>
        </div>
        <div className="bg-yellow-50 rounded-lg shadow p-4">
          <p className="text-sm text-yellow-600">Pending</p>
          <p className="text-2xl font-bold text-yellow-900">{stats.pending}</p>
        </div>
        <div className="bg-blue-50 rounded-lg shadow p-4">
          <p className="text-sm text-blue-600">Tenant Accepted</p>
          <p className="text-2xl font-bold text-blue-900">{stats.tenantAccepted}</p>
        </div>
        <div className="bg-green-50 rounded-lg shadow p-4">
          <p className="text-sm text-green-600">Completed</p>
          <p className="text-2xl font-bold text-green-900">{stats.completed}</p>
        </div>
        <div className="bg-orange-50 rounded-lg shadow p-4">
          <p className="text-sm text-orange-600">Expiring Soon</p>
          <p className="text-2xl font-bold text-orange-900">{stats.expiringSoon}</p>
        </div>
      </div>

      {loading ? (
        <div className="text-center py-12">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">Loading renewals...</p>
        </div>
      ) : renewals.length === 0 ? (
        <div className="bg-white shadow rounded-lg p-12 text-center">
          <p className="text-gray-600">No lease renewals found</p>
        </div>
      ) : (
        <div className="space-y-4">
          {renewals.map((renewal) => (
            <div key={renewal.id} className="bg-white shadow rounded-lg p-6">
              <div className="flex justify-between items-start mb-4">
                <div className="flex-1">
                  <div className="flex items-center space-x-3 mb-2">
                    <h3 className="text-lg font-semibold text-gray-900">{renewal.tenantName}</h3>
                    <span className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(renewal.status)}`}>
                      {renewal.statusName}
                    </span>
                    {renewal.isExpiringSoon && (
                      <span className="px-2 py-1 rounded-full text-xs font-medium bg-orange-100 text-orange-800">
                        {renewal.daysUntilExpiry}d left
                      </span>
                    )}
                  </div>
                  <div className="grid grid-cols-2 md:grid-cols-3 gap-4 text-sm mt-4">
                    <div>
                      <span className="text-gray-500">Unit:</span>{' '}
                      <span className="font-medium">{renewal.unitNumber}</span>
                    </div>
                    <div>
                      <span className="text-gray-500">Property:</span>{' '}
                      <span className="font-medium">{renewal.propertyName}</span>
                    </div>
                    <div>
                      <span className="text-gray-500">Current End:</span>{' '}
                      <span className="font-medium">{new Date(renewal.currentLeaseEndDate).toLocaleDateString()}</span>
                    </div>
                    <div>
                      <span className="text-gray-500">Proposed End:</span>{' '}
                      <span className="font-medium">{new Date(renewal.proposedLeaseEndDate).toLocaleDateString()}</span>
                    </div>
                    <div>
                      <span className="text-gray-500">Current Rent:</span>{' '}
                      <span className="font-medium">KES {renewal.currentRentAmount.toLocaleString()}</span>
                    </div>
                    <div>
                      <span className="text-gray-500">Proposed Rent:</span>{' '}
                      <span className="font-medium">
                        KES {renewal.proposedRentAmount.toLocaleString()}
                        {renewal.rentIncreasePercentage && <span className="text-orange-600 ml-1">(+{renewal.rentIncreasePercentage.toFixed(1)}%)</span>}
                      </span>
                    </div>
                  </div>
                </div>
              </div>

              <div className="flex flex-wrap gap-2 mt-4">
                {renewal.status === LeaseRenewalStatus.TenantAccepted && (
                  <>
                    <button onClick={() => handleApprove(renewal.id)} className="px-3 py-1 bg-green-600 text-white rounded-md hover:bg-green-700 text-sm">
                      Approve
                    </button>
                    <button onClick={() => handleReject(renewal.id)} className="px-3 py-1 bg-red-600 text-white rounded-md hover:bg-red-700 text-sm">
                      Reject
                    </button>
                  </>
                )}
                {renewal.status === LeaseRenewalStatus.LandlordApproved && (
                  <button onClick={() => handleComplete(renewal.id)} className="px-3 py-1 bg-blue-600 text-white rounded-md hover:bg-blue-700 text-sm">
                    Complete Renewal
                  </button>
                )}
                {renewal.status !== LeaseRenewalStatus.Completed && (
                  <button onClick={() => handleDelete(renewal.id)} className="px-3 py-1 bg-red-600 text-white rounded-md hover:bg-red-700 text-sm">
                    Delete
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Create Modal */}
      {showCreateModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 max-w-md w-full max-h-screen overflow-y-auto">
            <h3 className="text-lg font-semibold mb-4">Create Lease Renewal</h3>
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Tenant ID *</label>
                <input type="number" value={createData.tenantId} onChange={(e) => setCreateData({ ...createData, tenantId: e.target.value })} className="w-full px-3 py-2 border rounded-md" required />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Proposed End Date *</label>
                <input type="date" value={createData.proposedLeaseEndDate} onChange={(e) => setCreateData({ ...createData, proposedLeaseEndDate: e.target.value })} className="w-full px-3 py-2 border rounded-md" required />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Proposed Rent (KES) *</label>
                <input type="number" step="0.01" value={createData.proposedRentAmount} onChange={(e) => setCreateData({ ...createData, proposedRentAmount: e.target.value })} className="w-full px-3 py-2 border rounded-md" required />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Terms (Optional)</label>
                <textarea value={createData.landlordTerms} onChange={(e) => setCreateData({ ...createData, landlordTerms: e.target.value })} className="w-full px-3 py-2 border rounded-md" rows={3} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Notes (Optional)</label>
                <textarea value={createData.notes} onChange={(e) => setCreateData({ ...createData, notes: e.target.value })} className="w-full px-3 py-2 border rounded-md" rows={2} />
              </div>
            </div>
            <div className="flex justify-end space-x-2 mt-4">
              <button onClick={() => setShowCreateModal(false)} className="px-4 py-2 bg-gray-200 rounded-md hover:bg-gray-300">Cancel</button>
              <button onClick={handleCreate} disabled={!createData.tenantId || !createData.proposedLeaseEndDate || !createData.proposedRentAmount} className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:bg-gray-400">Create</button>
            </div>
          </div>
        </div>
      )}
      </div>
    </ProtectedRoute>
  )
}

