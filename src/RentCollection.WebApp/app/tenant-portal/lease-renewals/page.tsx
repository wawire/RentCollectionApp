'use client'

import { useState, useEffect } from 'react'
import { leaseRenewalService } from '@/lib/services/leaseRenewalService'
import { LeaseRenewal, LeaseRenewalStatus, TenantResponseDto } from '@/lib/types/leaseRenewal.types'

export default function TenantLeaseRenewalsPage() {
  const [renewals, setRenewals] = useState<LeaseRenewal[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [showResponseModal, setShowResponseModal] = useState(false)
  const [selectedRenewal, setSelectedRenewal] = useState<LeaseRenewal | null>(null)
  const [responseData, setResponseData] = useState({ accept: true, rejectionReason: '' })

  useEffect(() => {
    fetchRenewals()
  }, [])

  const fetchRenewals = async () => {
    try {
      setLoading(true)
      const data = await leaseRenewalService.getMyRenewals()
      setRenewals(data)
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch lease renewals')
    } finally {
      setLoading(false)
    }
  }

  const handleRespond = async () => {
    if (!selectedRenewal) return

    try {
      const dto: TenantResponseDto = {
        accept: responseData.accept,
        rejectionReason: responseData.accept ? undefined : responseData.rejectionReason,
      }
      await leaseRenewalService.tenantRespond(selectedRenewal.id, dto)
      setShowResponseModal(false)
      setSelectedRenewal(null)
      setResponseData({ accept: true, rejectionReason: '' })
      await fetchRenewals()
    } catch (err: any) {
      alert(err.response?.data?.message || 'Failed to submit response')
    }
  }

  const getStatusColor = (status: LeaseRenewalStatus) => {
    switch (status) {
      case LeaseRenewalStatus.Pending:
        return 'bg-yellow-100 text-yellow-800'
      case LeaseRenewalStatus.TenantAccepted:
        return 'bg-blue-100 text-blue-800'
      case LeaseRenewalStatus.TenantRejected:
        return 'bg-red-100 text-red-800'
      case LeaseRenewalStatus.LandlordApproved:
        return 'bg-green-100 text-green-800'
      case LeaseRenewalStatus.Completed:
        return 'bg-green-100 text-green-800'
      default:
        return 'bg-gray-100 text-gray-800'
    }
  }

  return (
    <div className="max-w-7xl mx-auto">
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-gray-900">Lease Renewals</h1>
        <p className="mt-2 text-gray-600">View and respond to your lease renewal requests</p>
      </div>

      {error && (
        <div className="mb-6 bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
          {error}
        </div>
      )}

      {loading ? (
        <div className="text-center py-12">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">Loading renewals...</p>
        </div>
      ) : renewals.length === 0 ? (
        <div className="bg-white shadow rounded-lg p-12 text-center">
          <p className="text-gray-600">No lease renewal requests found</p>
        </div>
      ) : (
        <div className="space-y-4">
          {renewals.map((renewal) => (
            <div key={renewal.id} className="bg-white shadow rounded-lg p-6 hover:shadow-lg transition-shadow">
              <div className="flex justify-between items-start mb-4">
                <div className="flex-1">
                  <div className="flex items-center space-x-3 mb-2">
                    <h3 className="text-lg font-semibold text-gray-900">
                      Lease Renewal - {renewal.unitNumber}
                    </h3>
                    <span className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(renewal.status)}`}>
                      {renewal.statusName}
                    </span>
                    {renewal.isExpiringSoon && (
                      <span className="px-2 py-1 rounded-full text-xs font-medium bg-orange-100 text-orange-800">
                        Expiring in {renewal.daysUntilExpiry} days
                      </span>
                    )}
                  </div>

                  <div className="grid grid-cols-2 gap-4 text-sm mt-4">
                    <div>
                      <span className="text-gray-500">Current Lease End:</span>{' '}
                      <span className="font-medium">{new Date(renewal.currentLeaseEndDate).toLocaleDateString()}</span>
                    </div>
                    <div>
                      <span className="text-gray-500">Proposed End Date:</span>{' '}
                      <span className="font-medium">{new Date(renewal.proposedLeaseEndDate).toLocaleDateString()}</span>
                    </div>
                    <div>
                      <span className="text-gray-500">Current Rent:</span>{' '}
                      <span className="font-medium">KES {renewal.currentRentAmount.toLocaleString()}/month</span>
                    </div>
                    <div>
                      <span className="text-gray-500">Proposed Rent:</span>{' '}
                      <span className="font-medium">
                        KES {renewal.proposedRentAmount.toLocaleString()}/month
                        {renewal.rentIncreasePercentage && (
                          <span className="text-orange-600 ml-2">
                            (+{renewal.rentIncreasePercentage.toFixed(1)}%)
                          </span>
                        )}
                      </span>
                    </div>
                    {renewal.rentIncrease > 0 && (
                      <div className="col-span-2">
                        <span className="text-gray-500">Rent Increase:</span>{' '}
                        <span className="font-medium text-orange-600">
                          KES {renewal.rentIncrease.toLocaleString()}/month
                        </span>
                      </div>
                    )}
                  </div>

                  {renewal.landlordTerms && (
                    <div className="mt-4 p-3 bg-blue-50 rounded-md">
                      <p className="text-sm text-gray-700">
                        <strong>Terms:</strong> {renewal.landlordTerms}
                      </p>
                    </div>
                  )}

                  {renewal.rejectionReason && (
                    <div className="mt-4 p-3 bg-red-50 rounded-md">
                      <p className="text-sm text-gray-700">
                        <strong>Rejection Reason:</strong> {renewal.rejectionReason}
                      </p>
                    </div>
                  )}
                </div>
              </div>

              {renewal.status === LeaseRenewalStatus.Pending && (
                <div className="flex gap-2 mt-4">
                  <button
                    onClick={() => {
                      setSelectedRenewal(renewal)
                      setResponseData({ accept: true, rejectionReason: '' })
                      setShowResponseModal(true)
                    }}
                    className="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700"
                  >
                    Accept Renewal
                  </button>
                  <button
                    onClick={() => {
                      setSelectedRenewal(renewal)
                      setResponseData({ accept: false, rejectionReason: '' })
                      setShowResponseModal(true)
                    }}
                    className="px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700"
                  >
                    Reject Renewal
                  </button>
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {/* Response Modal */}
      {showResponseModal && selectedRenewal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 max-w-md w-full">
            <h3 className="text-lg font-semibold mb-4">
              {responseData.accept ? 'Accept' : 'Reject'} Lease Renewal
            </h3>
            <p className="text-gray-600 mb-4">
              {responseData.accept
                ? 'Are you sure you want to accept this lease renewal?'
                : 'Please provide a reason for rejecting this lease renewal:'}
            </p>
            {!responseData.accept && (
              <textarea
                placeholder="Rejection reason..."
                value={responseData.rejectionReason}
                onChange={(e) => setResponseData({ ...responseData, rejectionReason: e.target.value })}
                className="w-full px-3 py-2 border rounded-md mb-4"
                rows={3}
                required
              />
            )}
            <div className="flex justify-end space-x-2">
              <button
                onClick={() => {
                  setShowResponseModal(false)
                  setSelectedRenewal(null)
                  setResponseData({ accept: true, rejectionReason: '' })
                }}
                className="px-4 py-2 bg-gray-200 rounded-md hover:bg-gray-300"
              >
                Cancel
              </button>
              <button
                onClick={handleRespond}
                disabled={!responseData.accept && !responseData.rejectionReason}
                className={`px-4 py-2 text-white rounded-md ${
                  responseData.accept ? 'bg-green-600 hover:bg-green-700' : 'bg-red-600 hover:bg-red-700'
                } disabled:bg-gray-400`}
              >
                Confirm
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
