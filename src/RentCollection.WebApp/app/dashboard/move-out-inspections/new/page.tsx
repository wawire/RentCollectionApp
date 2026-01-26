'use client'

import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { moveOutInspectionService } from '@/lib/services/moveOutInspectionService'
import { apiClient } from '@/lib/services/api'
import type { CreateMoveOutInspectionDto, Tenant } from '@/lib/types'
import { ArrowLeft, Calendar, Save } from 'lucide-react'
import Link from 'next/link'
import ProtectedRoute from '@/components/auth/ProtectedRoute'

export default function ScheduleInspectionPage() {
  const router = useRouter()
  const [tenants, setTenants] = useState<Tenant[]>([])
  const [loading, setLoading] = useState(false)
  const [loadingTenants, setLoadingTenants] = useState(true)
  const [error, setError] = useState('')

  const [formData, setFormData] = useState<CreateMoveOutInspectionDto>({
    tenantId: 0,
    moveOutDate: '',
    inspectionDate: '',
    notes: '',
  })

  useEffect(() => {
    loadActiveTenants()
  }, [])

  const loadActiveTenants = async () => {
    try {
      setLoadingTenants(true)
      const response = await apiClient.get<Tenant[]>('/tenants')
      setTenants(response.data)
    } catch (error) {
      console.error('Failed to load tenants:', error)
      setError('Failed to load tenants. Please try again.')
    } finally {
      setLoadingTenants(false)
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')

    if (!formData.tenantId) {
      setError('Please select a tenant')
      return
    }

    if (!formData.moveOutDate || !formData.inspectionDate) {
      setError('Please select both move-out and inspection dates')
      return
    }

    try {
      setLoading(true)
      const inspection = await moveOutInspectionService.scheduleInspection(formData)
      router.push(`/dashboard/move-out-inspections/${inspection.id}`)
    } catch (error: any) {
      console.error('Failed to schedule inspection:', error)
      setError(error.response?.data?.error || 'Failed to schedule inspection. Please try again.')
    } finally {
      setLoading(false)
    }
  }

  const selectedTenant = tenants.find((t) => t.id === formData.tenantId)

  return (
    <ProtectedRoute allowedRoles={['Landlord', 'Manager', 'Caretaker']}>
      <div className="p-6 max-w-4xl mx-auto">
      {/* Header */}
      <div className="mb-6">
        <Link href="/dashboard/move-out-inspections">
          <button className="flex items-center gap-2 text-blue-600 hover:text-blue-800 transition mb-4">
            <ArrowLeft className="w-4 h-4" />
            Back to Inspections
          </button>
        </Link>
        <h1 className="text-3xl font-bold text-gray-900 flex items-center gap-2">
          <Calendar className="text-blue-600 w-5 h-5" />
          Schedule Move-Out Inspection
        </h1>
        <p className="text-gray-600 mt-1">Create a new move-out inspection for a tenant</p>
      </div>

      {/* Form Card */}
      <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
        {error && (
          <div className="mb-6 bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-6">
          {/* Tenant Selection */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Tenant <span className="text-red-500">*</span>
            </label>
            {loadingTenants ? (
              <div className="text-gray-500">Loading tenants...</div>
            ) : (
              <select
                value={formData.tenantId}
                onChange={(e) =>
                  setFormData({ ...formData, tenantId: parseInt(e.target.value) })
                }
                required
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              >
                <option value={0}>Select a tenant</option>
                {tenants.map((tenant) => (
                  <option key={tenant.id} value={tenant.id}>
                    {tenant.firstName} {tenant.lastName} - {tenant.email}
                  </option>
                ))}
              </select>
            )}
            <p className="mt-1 text-sm text-gray-500">
              Select the tenant who is moving out
            </p>
          </div>

          {/* Tenant Info Display */}
          {selectedTenant && (
            <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
              <h3 className="font-semibold text-blue-900 mb-2">Tenant Information</h3>
              <div className="grid grid-cols-2 gap-3 text-sm">
                <div>
                  <span className="text-blue-600 font-medium">Name:</span>{' '}
                  {selectedTenant.firstName} {selectedTenant.lastName}
                </div>
                <div>
                  <span className="text-blue-600 font-medium">Email:</span>{' '}
                  {selectedTenant.email}
                </div>
                <div>
                  <span className="text-blue-600 font-medium">Phone:</span>{' '}
                  {selectedTenant.phoneNumber}
                </div>
                <div>
                  <span className="text-blue-600 font-medium">Monthly Rent:</span> KSh{' '}
                  {selectedTenant.monthlyRent?.toLocaleString()}
                </div>
                {selectedTenant.securityDeposit && (
                  <div className="col-span-2">
                    <span className="text-blue-600 font-medium">Security Deposit:</span> KSh{' '}
                    {selectedTenant.securityDeposit.toLocaleString()}
                  </div>
                )}
              </div>
            </div>
          )}

          {/* Move-Out Date */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Move-Out Date <span className="text-red-500">*</span>
            </label>
            <input
              type="date"
              value={formData.moveOutDate}
              onChange={(e) => setFormData({ ...formData, moveOutDate: e.target.value })}
              required
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
            <p className="mt-1 text-sm text-gray-500">
              The date when the tenant will vacate the unit
            </p>
          </div>

          {/* Inspection Date */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Inspection Date <span className="text-red-500">*</span>
            </label>
            <input
              type="date"
              value={formData.inspectionDate}
              onChange={(e) => setFormData({ ...formData, inspectionDate: e.target.value })}
              required
              min={formData.moveOutDate || undefined}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
            <p className="mt-1 text-sm text-gray-500">
              When you'll conduct the move-out inspection (must be on or after move-out date)
            </p>
          </div>

          {/* Notes */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Notes (Optional)
            </label>
            <textarea
              value={formData.notes}
              onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
              rows={4}
              placeholder="Any additional notes or special instructions..."
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent resize-none"
            />
          </div>

          {/* Action Buttons */}
          <div className="flex gap-4 pt-4">
            <button
              type="submit"
              disabled={loading}
              className="flex-1 bg-blue-600 text-white px-6 py-3 rounded-lg hover:bg-blue-700 transition disabled:bg-gray-400 disabled:cursor-not-allowed font-medium flex items-center justify-center gap-2"
            >
              {loading ? (
                <>
                  <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-white"></div>
                  Scheduling...
                </>
              ) : (
                <>
                  <Save className="w-4 h-4" />
                  Schedule Inspection
                </>
              )}
            </button>
            <Link href="/dashboard/move-out-inspections" className="flex-1">
              <button
                type="button"
                className="w-full bg-gray-200 text-gray-700 px-6 py-3 rounded-lg hover:bg-gray-300 transition font-medium"
              >
                Cancel
              </button>
            </Link>
          </div>
        </form>
      </div>
      </div>
    </ProtectedRoute>
  )
}
