'use client'

import { useEffect, useState } from 'react'
import Link from 'next/link'
import { LoadingSpinner } from '@/components/common'
import ProtectedRoute from '@/components/auth/ProtectedRoute'
import { UserRole } from '@/lib/types/auth.types'
import { paymentService } from '@/lib/services/paymentService'
import { Payment } from '@/lib/types/payment.types'
import { ArrowLeft, CheckCircle2, Clock, Info, XCircle } from 'lucide-react'

export default function PendingPaymentsPage() {
  const [payments, setPayments] = useState<Payment[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [actionLoading, setActionLoading] = useState<number | null>(null)
  const [showConfirmModal, setShowConfirmModal] = useState<{ show: boolean; payment: Payment | null }>({ show: false, payment: null })
  const [showRejectModal, setShowRejectModal] = useState<{ show: boolean; payment: Payment | null }>({ show: false, payment: null })
  const [rejectReason, setRejectReason] = useState('')
  const [alert, setAlert] = useState<{ type: 'success' | 'error'; message: string } | null>(null)

  const fetchPendingPayments = async () => {
    try {
      setLoading(true)
      const data = await paymentService.getPendingPayments()
      setPayments(data)
      setError(null)
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load pending payments')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchPendingPayments()
  }, [])

  const handleConfirmClick = (payment: Payment) => {
    setShowConfirmModal({ show: true, payment })
  }

  const handleRejectClick = (payment: Payment) => {
    setShowRejectModal({ show: true, payment })
    setRejectReason('')
  }

  const handleConfirmPayment = async () => {
    if (!showConfirmModal.payment) return

    try {
      setActionLoading(showConfirmModal.payment.id)
      await paymentService.confirmPayment(showConfirmModal.payment.id)
      setAlert({ type: 'success', message: 'Payment confirmed successfully!' })
      setShowConfirmModal({ show: false, payment: null })
      await fetchPendingPayments() // Refresh list
      setTimeout(() => setAlert(null), 3000)
    } catch (err: any) {
      setAlert({ type: 'error', message: err.response?.data?.message || 'Failed to confirm payment' })
      setTimeout(() => setAlert(null), 3000)
    } finally {
      setActionLoading(null)
    }
  }

  const handleRejectPayment = async () => {
    if (!showRejectModal.payment || !rejectReason.trim()) {
      setAlert({ type: 'error', message: 'Please provide a reason for rejection' })
      setTimeout(() => setAlert(null), 3000)
      return
    }

    try {
      setActionLoading(showRejectModal.payment.id)
      await paymentService.rejectPayment(showRejectModal.payment.id, rejectReason)
      setAlert({ type: 'success', message: 'Payment rejected successfully' })
      setShowRejectModal({ show: false, payment: null })
      await fetchPendingPayments() // Refresh list
      setTimeout(() => setAlert(null), 3000)
    } catch (err: any) {
      setAlert({ type: 'error', message: err.response?.data?.message || 'Failed to reject payment' })
      setTimeout(() => setAlert(null), 3000)
    } finally {
      setActionLoading(null)
    }
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center h-96">
        <LoadingSpinner size="lg" text="Loading pending payments..." />
      </div>
    )
  }

  if (error) {
    return (
      <div className="text-center py-12">
        <p className="text-red-600">{error}</p>
        <Link href="/payments" className="text-primary hover:underline mt-4 inline-block">
          ← Back to Payments
        </Link>
      </div>
    )
  }

  return (
    <ProtectedRoute allowedRoles={[UserRole.PlatformAdmin, UserRole.Landlord, UserRole.Manager]}>
      <div className="space-y-6">
      {/* Header */}
      <div>
        <Link
          href="/payments"
          className="inline-flex items-center gap-2 text-primary/60 hover:text-primary transition-colors mb-4"
        >
          <ArrowLeft className="w-4 h-4" />
          Back to All Payments
        </Link>
        <h1 className="text-3xl font-serif font-normal text-primary tracking-wide">
          Pending Payments
        </h1>
        <p className="text-primary/60 mt-2 tracking-wide">
          Review and confirm tenant payment submissions
        </p>
      </div>

      {/* Alert */}
      {alert && (
        <div
          className={`p-4 rounded-lg border ${
            alert.type === 'success'
              ? 'bg-green-50 border-green-200 text-green-800'
              : 'bg-red-50 border-red-200 text-red-800'
          }`}
        >
          <p>{alert.message}</p>
        </div>
      )}

      {/* Summary */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="bg-white rounded-lg shadow-sm border border-primary/10 p-6">
          <p className="text-sm text-primary/60 tracking-wide">Pending Payments</p>
          <p className="text-2xl font-medium text-primary mt-2">{payments.length}</p>
        </div>
        <div className="bg-white rounded-lg shadow-sm border border-primary/10 p-6">
          <p className="text-sm text-primary/60 tracking-wide">Total Amount</p>
          <p className="text-2xl font-medium text-yellow-600 mt-2">
            KSh {payments.reduce((sum, p) => sum + p.amount, 0).toLocaleString()}
          </p>
        </div>
        <div className="bg-white rounded-lg shadow-sm border border-primary/10 p-6">
          <p className="text-sm text-primary/60 tracking-wide">Awaiting Action</p>
          <div className="flex items-center gap-2 mt-2">
            <Clock className="text-yellow-600 w-5 h-5" />
            <p className="text-lg font-medium text-primary">
              {payments.length} {payments.length === 1 ? 'payment' : 'payments'}
            </p>
          </div>
        </div>
      </div>

      {/* Payments List */}
      <div className="bg-white rounded-lg shadow-sm border border-primary/10">
        {payments.length === 0 ? (
          <div className="text-center py-12">
            <CheckCircle2 className="text-green-600 w-12 h-12 mx-auto mb-4" />
            <p className="text-primary/60 text-lg">No pending payments</p>
            <p className="text-primary/60 text-sm mt-2">
              All tenant payments have been reviewed
            </p>
            <Link
              href="/payments"
              className="inline-flex items-center justify-center px-6 py-3 bg-primary text-white rounded-lg hover:bg-primary/90 transition-colors mt-4"
            >
              View All Payments
            </Link>
          </div>
        ) : (
          <div className="divide-y divide-primary/10">
            {payments.map((payment) => (
              <div key={payment.id} className="p-6 hover:bg-primary/5 transition-colors">
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                  <div className="flex items-center gap-3 mb-3">
                      <Clock className="text-yellow-600 w-5 h-5" />
                      <h3 className="text-xl font-medium text-primary">
                        KSh {payment.amount.toLocaleString()}
                      </h3>
                      <span className="px-3 py-1 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800 border border-yellow-200">
                        Pending Review
                      </span>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 mb-4">
                      <div>
                        <p className="text-sm text-primary/60">Tenant</p>
                        <p className="text-sm font-medium text-primary">{payment.tenantName}</p>
                      </div>
                      <div>
                        <p className="text-sm text-primary/60">Unit</p>
                        <p className="text-sm font-medium text-primary">
                          {payment.unitNumber} - {payment.propertyName}
                        </p>
                      </div>
                      <div>
                        <p className="text-sm text-primary/60">Payment Date</p>
                        <p className="text-sm font-medium text-primary">
                          {new Date(payment.paymentDate).toLocaleDateString('en-KE', {
                            year: 'numeric',
                            month: 'long',
                            day: 'numeric',
                          })}
                        </p>
                      </div>
                      <div>
                        <p className="text-sm text-primary/60">Payment Method</p>
                        <p className="text-sm font-medium text-primary">{payment.paymentMethod}</p>
                      </div>
                      {payment.transactionReference && (
                        <div>
                          <p className="text-sm text-primary/60">Transaction Reference</p>
                          <p className="text-sm font-medium text-primary font-mono">
                            {payment.transactionReference}
                          </p>
                        </div>
                      )}
                      {payment.mPesaPhoneNumber && (
                        <div>
                          <p className="text-sm text-primary/60">M-Pesa Phone</p>
                          <p className="text-sm font-medium text-primary">
                            {payment.mPesaPhoneNumber}
                          </p>
                        </div>
                      )}
                    </div>

                    {payment.periodStart && payment.periodEnd && (
                      <div className="bg-blue-50 border border-blue-200 rounded-lg p-3 mb-4">
                      <div className="flex items-start gap-2">
                          <Info className="text-blue-600 mt-0.5 w-4 h-4" />
                          <div>
                            <p className="text-sm font-medium text-blue-900">Payment Period</p>
                            <p className="text-sm text-blue-800">
                              {new Date(payment.periodStart).toLocaleDateString('en-KE', {
                                month: 'short',
                                day: 'numeric',
                              })}{' '}
                              -{' '}
                              {new Date(payment.periodEnd).toLocaleDateString('en-KE', {
                                month: 'short',
                                day: 'numeric',
                                year: 'numeric',
                              })}
                            </p>
                          </div>
                        </div>
                      </div>
                    )}

                    {payment.notes && (
                      <div className="bg-gray-50 border border-gray-200 rounded-lg p-3">
                        <p className="text-sm font-medium text-primary/80">Tenant Notes:</p>
                        <p className="text-sm text-primary/60 mt-1">{payment.notes}</p>
                      </div>
                    )}
                  </div>

                  <div className="flex gap-2 ml-4">
                    <button
                      onClick={() => handleConfirmClick(payment)}
                      disabled={actionLoading === payment.id}
                      className="flex items-center gap-2 px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      <CheckCircle2 className="w-4 h-4" />
                      Confirm
                    </button>
                    <button
                      onClick={() => handleRejectClick(payment)}
                      disabled={actionLoading === payment.id}
                      className="flex items-center gap-2 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      <XCircle className="w-4 h-4" />
                      Reject
                    </button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Confirm Modal */}
      {showConfirmModal.show && showConfirmModal.payment && (
        <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4">
          <div className="bg-white rounded-lg shadow-xl max-w-md w-full p-6">
            <div className="flex items-center gap-3 mb-4">
              <CheckCircle2 className="text-green-600 w-6 h-6" />
              <h2 className="text-xl font-serif font-normal text-primary tracking-wide">
                Confirm Payment
              </h2>
            </div>
            <p className="text-primary/80 mb-4">
              Confirm payment of <strong>KSh {showConfirmModal.payment.amount.toLocaleString()}</strong> from{' '}
              <strong>{showConfirmModal.payment.tenantName}</strong>?
            </p>
            <div className="flex gap-3">
              <button
                onClick={() => setShowConfirmModal({ show: false, payment: null })}
                disabled={actionLoading !== null}
                className="flex-1 px-4 py-2 border border-primary/20 text-primary rounded-lg hover:bg-primary/5 transition-colors disabled:opacity-50"
              >
                Cancel
              </button>
              <button
                onClick={handleConfirmPayment}
                disabled={actionLoading !== null}
                className="flex-1 px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors disabled:opacity-50"
              >
                {actionLoading !== null ? 'Confirming...' : 'Confirm'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Reject Modal */}
      {showRejectModal.show && showRejectModal.payment && (
        <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4">
          <div className="bg-white rounded-lg shadow-xl max-w-md w-full p-6">
            <div className="flex items-center gap-3 mb-4">
              <XCircle className="text-red-600 w-6 h-6" />
              <h2 className="text-xl font-serif font-normal text-primary tracking-wide">
                Reject Payment
              </h2>
            </div>
            <p className="text-primary/80 mb-4">
              Reject payment of <strong>KSh {showRejectModal.payment.amount.toLocaleString()}</strong> from{' '}
              <strong>{showRejectModal.payment.tenantName}</strong>?
            </p>
            <div className="mb-4">
              <label className="block text-sm font-medium text-primary/80 mb-2">
                Reason for Rejection <span className="text-red-500">*</span>
              </label>
              <textarea
                value={rejectReason}
                onChange={(e) => setRejectReason(e.target.value)}
                rows={3}
                required
                className="w-full px-4 py-2 border border-primary/20 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/50"
                placeholder="Explain why this payment is being rejected..."
              />
            </div>
            <div className="flex gap-3">
              <button
                onClick={() => setShowRejectModal({ show: false, payment: null })}
                disabled={actionLoading !== null}
                className="flex-1 px-4 py-2 border border-primary/20 text-primary rounded-lg hover:bg-primary/5 transition-colors disabled:opacity-50"
              >
                Cancel
              </button>
              <button
                onClick={handleRejectPayment}
                disabled={actionLoading !== null}
                className="flex-1 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors disabled:opacity-50"
              >
                {actionLoading !== null ? 'Rejecting...' : 'Reject'}
              </button>
            </div>
          </div>
        </div>
      )}
      </div>
    </ProtectedRoute>
  )
}

