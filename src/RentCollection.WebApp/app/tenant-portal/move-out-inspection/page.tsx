'use client'

import { useState, useEffect } from 'react'
import { useAuth } from '@/contexts/AuthContext'
import { moveOutInspectionService } from '@/lib/services/moveOutInspectionService'
import type { MoveOutInspection } from '@/lib/types'
import { MoveOutInspectionStatus } from '@/lib/types'
import { Calendar, CheckCircle2, ClipboardList, Clock } from 'lucide-react'

export default function TenantMoveOutInspectionPage() {
  const { user } = useAuth()
  const [inspections, setInspections] = useState<MoveOutInspection[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    if (user) {
      loadInspections()
    }
  }, [user])

  const loadInspections = async () => {
    try {
      setLoading(true)
      const data = await moveOutInspectionService.getMyInspections()
      setInspections(data)
    } catch (error) {
      console.error('Failed to load inspections:', error)
      setError('Failed to load your move-out inspection.')
    } finally {
      setLoading(false)
    }
  }

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-KE', {
      style: 'currency',
      currency: 'KES',
      minimumFractionDigits: 0,
    }).format(amount)
  }

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-KE', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    })
  }

  const getStatusBadge = (status: MoveOutInspectionStatus) => {
    const badges: Record<MoveOutInspectionStatus, string> = {
      [MoveOutInspectionStatus.Scheduled]: 'bg-blue-100 text-blue-800',
      [MoveOutInspectionStatus.InProgress]: 'bg-yellow-100 text-yellow-800',
      [MoveOutInspectionStatus.Completed]: 'bg-purple-100 text-purple-800',
      [MoveOutInspectionStatus.Reviewed]: 'bg-indigo-100 text-indigo-800',
      [MoveOutInspectionStatus.DisputeRaised]: 'bg-red-100 text-red-800',
      [MoveOutInspectionStatus.Settled]: 'bg-green-100 text-green-800',
      [MoveOutInspectionStatus.RefundProcessed]: 'bg-teal-100 text-teal-800',
    }
    return badges[status] || 'bg-gray-100 text-gray-800'
  }

  if (loading) {
    return (
      <div className="p-6">
        <div className="flex items-center justify-center h-64">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        </div>
      </div>
    )
  }

  return (
    <div className="p-6 max-w-4xl mx-auto">
      <h1 className="text-3xl font-bold text-gray-900 mb-6 flex items-center gap-2">
        <ClipboardList className="text-blue-600 w-5 h-5" />
        My Move-Out Inspection
      </h1>

      {error && (
        <div className="mb-6 bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg">
          {error}
        </div>
      )}

      {inspections.length === 0 ? (
        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-12 text-center">
          <ClipboardList className="mx-auto w-12 h-12 text-gray-300 mb-4" />
          <h3 className="text-xl font-semibold text-gray-700 mb-2">
            No Move-Out Inspection Scheduled
          </h3>
          <p className="text-gray-500">
            You don't have any move-out inspections scheduled at this time.
          </p>
        </div>
      ) : (
        <div className="space-y-6">
          {inspections.map((inspection) => (
            <div
              key={inspection.id}
              className="bg-white rounded-lg shadow-sm border border-gray-200 p-6"
            >
              {/* Header */}
              <div className="flex items-center justify-between mb-6">
                <div>
                  <h2 className="text-xl font-semibold text-gray-900">
                    {inspection.propertyName} - {inspection.unitNumber}
                  </h2>
                  <p className="text-gray-500 text-sm mt-1">
                    Inspection ID: #{inspection.id}
                  </p>
                </div>
                <span
                  className={`px-3 py-1 rounded-full text-sm font-medium ${getStatusBadge(
                    inspection.status
                  )}`}
                >
                  {inspection.statusDisplay}
                </span>
              </div>

              {/* Dates */}
              <div className="grid grid-cols-2 gap-4 mb-6">
                <div className="bg-gray-50 rounded-lg p-4">
                  <div className="text-sm text-gray-600 mb-1">Move-Out Date</div>
                  <div className="text-lg font-semibold text-gray-900 flex items-center gap-2">
                    <Calendar className="text-blue-600 w-4 h-4" />
                    {formatDate(inspection.moveOutDate)}
                  </div>
                </div>
                <div className="bg-gray-50 rounded-lg p-4">
                  <div className="text-sm text-gray-600 mb-1">Inspection Date</div>
                  <div className="text-lg font-semibold text-gray-900 flex items-center gap-2">
                    <Clock className="text-purple-600 w-4 h-4" />
                    {formatDate(inspection.inspectionDate)}
                  </div>
                </div>
              </div>

              {/* Financial Summary */}
              {inspection.status >= MoveOutInspectionStatus.Completed && (
                <div className="border-t border-gray-200 pt-6">
                  <h3 className="text-lg font-semibold text-gray-900 mb-4">
                    Security Deposit Settlement
                  </h3>
                  <div className="space-y-3">
                    <div className="flex justify-between">
                      <span className="text-gray-600">Security Deposit Held:</span>
                      <span className="font-semibold">
                        {formatCurrency(inspection.securityDepositHeld)}
                      </span>
                    </div>
                    {inspection.cleaningCharges > 0 && (
                      <div className="flex justify-between">
                        <span className="text-gray-600">Cleaning Charges:</span>
                        <span className="text-red-600">
                          -{formatCurrency(inspection.cleaningCharges)}
                        </span>
                      </div>
                    )}
                    {inspection.repairCharges > 0 && (
                      <div className="flex justify-between">
                        <span className="text-gray-600">Repair Charges:</span>
                        <span className="text-red-600">
                          -{formatCurrency(inspection.repairCharges)}
                        </span>
                      </div>
                    )}
                    {inspection.unpaidRent > 0 && (
                      <div className="flex justify-between">
                        <span className="text-gray-600">Unpaid Rent:</span>
                        <span className="text-red-600">
                          -{formatCurrency(inspection.unpaidRent)}
                        </span>
                      </div>
                    )}
                    {inspection.unpaidUtilities > 0 && (
                      <div className="flex justify-between">
                        <span className="text-gray-600">Unpaid Utilities:</span>
                        <span className="text-red-600">
                          -{formatCurrency(inspection.unpaidUtilities)}
                        </span>
                      </div>
                    )}
                    {inspection.otherCharges > 0 && (
                      <div className="flex justify-between">
                        <span className="text-gray-600">Other Charges:</span>
                        <span className="text-red-600">
                          -{formatCurrency(inspection.otherCharges)}
                        </span>
                      </div>
                    )}
                    <div className="border-t border-gray-200 pt-3 flex justify-between">
                      <span className="font-semibold text-gray-900">Total Deductions:</span>
                      <span className="font-semibold text-red-600">
                        -{formatCurrency(inspection.totalDeductions)}
                      </span>
                    </div>
                    <div className="bg-green-50 rounded-lg p-4 flex justify-between items-center">
                      <span className="font-semibold text-gray-900">Refund Amount:</span>
                      <span className="text-2xl font-bold text-green-600">
                        {formatCurrency(inspection.refundAmount)}
                      </span>
                    </div>
                    {inspection.tenantOwes > 0 && (
                      <div className="bg-red-50 rounded-lg p-4 flex justify-between items-center">
                        <span className="font-semibold text-gray-900">Amount You Owe:</span>
                        <span className="text-2xl font-bold text-red-600">
                          {formatCurrency(inspection.tenantOwes)}
                        </span>
                      </div>
                    )}
                  </div>
                </div>
              )}

              {/* Refund Status */}
              {inspection.refundProcessed && (
                <div className="mt-6 bg-teal-50 border border-teal-200 rounded-lg p-4">
                  <div className="flex items-center gap-2 text-teal-800 font-semibold mb-2">
                    <CheckCircle2 className="w-4 h-4" />
                    Refund Processed
                  </div>
                  <div className="text-sm text-teal-700">
                    <div>Method: {inspection.refundMethod}</div>
                    {inspection.refundReference && (
                      <div>Reference: {inspection.refundReference}</div>
                    )}
                    {inspection.refundDate && (
                      <div>Date: {formatDate(inspection.refundDate)}</div>
                    )}
                  </div>
                </div>
              )}

              {/* Notes */}
              {inspection.generalNotes && (
                <div className="mt-6 border-t border-gray-200 pt-6">
                  <h3 className="text-lg font-semibold text-gray-900 mb-2">Notes</h3>
                  <p className="text-gray-600">{inspection.generalNotes}</p>
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
