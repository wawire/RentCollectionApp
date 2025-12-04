'use client'

import { useEffect, useState } from 'react'
import Link from 'next/link'
import { LoadingSpinner } from '@/components/common'
import { tenantPaymentService } from '@/lib/services/tenantPaymentService'
import { TenantPaymentHistory } from '@/lib/types/tenantPayment.types'
import { FaArrowLeft, FaCheckCircle, FaClock, FaExclamationTriangle, FaTimesCircle } from 'react-icons/fa'

export default function PaymentHistoryPage() {
  const [payments, setPayments] = useState<TenantPaymentHistory[]>([])
  const [filteredPayments, setFilteredPayments] = useState<TenantPaymentHistory[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [statusFilter, setStatusFilter] = useState<string>('all')

  useEffect(() => {
    const fetchPayments = async () => {
      try {
        setLoading(true)
        const data = await tenantPaymentService.getPaymentHistory()
        setPayments(data)
        setFilteredPayments(data)
        setError(null)
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to load payment history')
      } finally {
        setLoading(false)
      }
    }

    fetchPayments()
  }, [])

  useEffect(() => {
    if (statusFilter === 'all') {
      setFilteredPayments(payments)
    } else {
      setFilteredPayments(payments.filter((p) => p.status === statusFilter))
    }
  }, [statusFilter, payments])

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'Completed':
        return <FaCheckCircle className="text-green-600 text-xl" />
      case 'Pending':
        return <FaClock className="text-yellow-600 text-xl" />
      case 'Rejected':
        return <FaExclamationTriangle className="text-red-600 text-xl" />
      case 'Failed':
        return <FaTimesCircle className="text-red-600 text-xl" />
      default:
        return <FaClock className="text-gray-600 text-xl" />
    }
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Completed':
        return 'bg-green-100 text-green-800 border-green-200'
      case 'Pending':
        return 'bg-yellow-100 text-yellow-800 border-yellow-200'
      case 'Rejected':
        return 'bg-red-100 text-red-800 border-red-200'
      case 'Failed':
        return 'bg-red-100 text-red-800 border-red-200'
      default:
        return 'bg-gray-100 text-gray-800 border-gray-200'
    }
  }

  const totalPaid = payments
    .filter((p) => p.status === 'Completed')
    .reduce((sum, p) => sum + p.amount, 0)

  const pendingAmount = payments
    .filter((p) => p.status === 'Pending')
    .reduce((sum, p) => sum + p.amount, 0)

  if (loading) {
    return (
      <div className="flex items-center justify-center h-96">
        <LoadingSpinner size="lg" text="Loading payment history..." />
      </div>
    )
  }

  if (error) {
    return (
      <div className="text-center py-12">
        <p className="text-red-600">{error}</p>
        <Link href="/tenant-portal" className="text-primary hover:underline mt-4 inline-block">
          ← Back to Dashboard
        </Link>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <Link
          href="/tenant-portal"
          className="inline-flex items-center gap-2 text-primary/60 hover:text-primary transition-colors mb-4"
        >
          <FaArrowLeft />
          Back to Dashboard
        </Link>
        <h1 className="text-3xl font-serif font-normal text-primary tracking-wide">
          Payment History
        </h1>
        <p className="text-primary/60 mt-2 tracking-wide">
          View all your rent payment records
        </p>
      </div>

      {/* Summary Stats */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="bg-white rounded-lg shadow-sm border border-primary/10 p-6">
          <p className="text-sm text-primary/60 tracking-wide">Total Payments</p>
          <p className="text-2xl font-medium text-primary mt-2">{payments.length}</p>
        </div>
        <div className="bg-white rounded-lg shadow-sm border border-primary/10 p-6">
          <p className="text-sm text-primary/60 tracking-wide">Total Paid</p>
          <p className="text-2xl font-medium text-green-600 mt-2">
            KSh {totalPaid.toLocaleString()}
          </p>
        </div>
        <div className="bg-white rounded-lg shadow-sm border border-primary/10 p-6">
          <p className="text-sm text-primary/60 tracking-wide">Pending Amount</p>
          <p className="text-2xl font-medium text-yellow-600 mt-2">
            KSh {pendingAmount.toLocaleString()}
          </p>
        </div>
      </div>

      {/* Filter */}
      <div className="bg-white rounded-lg shadow-sm border border-primary/10 p-4">
        <div className="flex items-center gap-4">
          <label className="text-sm font-medium text-primary/80">Filter by status:</label>
          <div className="flex gap-2 flex-wrap">
            <button
              onClick={() => setStatusFilter('all')}
              className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
                statusFilter === 'all'
                  ? 'bg-primary text-white'
                  : 'bg-primary/10 text-primary hover:bg-primary/20'
              }`}
            >
              All ({payments.length})
            </button>
            <button
              onClick={() => setStatusFilter('Completed')}
              className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
                statusFilter === 'Completed'
                  ? 'bg-green-600 text-white'
                  : 'bg-green-100 text-green-800 hover:bg-green-200'
              }`}
            >
              Completed ({payments.filter((p) => p.status === 'Completed').length})
            </button>
            <button
              onClick={() => setStatusFilter('Pending')}
              className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
                statusFilter === 'Pending'
                  ? 'bg-yellow-600 text-white'
                  : 'bg-yellow-100 text-yellow-800 hover:bg-yellow-200'
              }`}
            >
              Pending ({payments.filter((p) => p.status === 'Pending').length})
            </button>
            <button
              onClick={() => setStatusFilter('Rejected')}
              className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
                statusFilter === 'Rejected'
                  ? 'bg-red-600 text-white'
                  : 'bg-red-100 text-red-800 hover:bg-red-200'
              }`}
            >
              Rejected ({payments.filter((p) => p.status === 'Rejected').length})
            </button>
          </div>
        </div>
      </div>

      {/* Payments List */}
      <div className="bg-white rounded-lg shadow-sm border border-primary/10">
        {filteredPayments.length === 0 ? (
          <div className="text-center py-12">
            <p className="text-primary/60">
              {statusFilter === 'all'
                ? 'No payment records found'
                : `No ${statusFilter.toLowerCase()} payments found`}
            </p>
            <Link
              href="/tenant-portal/record-payment"
              className="inline-flex items-center justify-center px-6 py-3 bg-primary text-white rounded-lg hover:bg-primary/90 transition-colors mt-4"
            >
              Record Your First Payment
            </Link>
          </div>
        ) : (
          <div className="divide-y divide-primary/10">
            {filteredPayments.map((payment) => (
              <div key={payment.id} className="p-6 hover:bg-primary/5 transition-colors">
                <div className="flex items-start justify-between">
                  <div className="flex items-start gap-4">
                    <div className="mt-1">{getStatusIcon(payment.status)}</div>
                    <div>
                      <div className="flex items-center gap-3 mb-2">
                        <h3 className="text-lg font-medium text-primary">
                          KSh {payment.amount.toLocaleString()}
                        </h3>
                        <span
                          className={`px-3 py-1 rounded-full text-xs font-medium border ${getStatusColor(
                            payment.status
                          )}`}
                        >
                          {payment.status}
                        </span>
                      </div>
                      <div className="space-y-1">
                        <p className="text-sm text-primary/80">
                          <span className="font-medium">Payment Date:</span>{' '}
                          {new Date(payment.paymentDate).toLocaleDateString('en-KE', {
                            year: 'numeric',
                            month: 'long',
                            day: 'numeric',
                          })}
                        </p>
                        <p className="text-sm text-primary/80">
                          <span className="font-medium">Method:</span> {payment.paymentMethod}
                        </p>
                        {payment.transactionReference && (
                          <p className="text-sm text-primary/80">
                            <span className="font-medium">Reference:</span>{' '}
                            {payment.transactionReference}
                          </p>
                        )}
                        {payment.periodStart && payment.periodEnd && (
                          <p className="text-sm text-primary/80">
                            <span className="font-medium">Period:</span>{' '}
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
                        )}
                        {payment.confirmedAt && (
                          <p className="text-sm text-green-600">
                            <span className="font-medium">Confirmed:</span>{' '}
                            {new Date(payment.confirmedAt).toLocaleDateString('en-KE', {
                              year: 'numeric',
                              month: 'long',
                              day: 'numeric',
                            })}
                            {payment.confirmedByUserName && ` by ${payment.confirmedByUserName}`}
                          </p>
                        )}
                        {payment.notes && (
                          <p className="text-sm text-primary/60 mt-2">
                            <span className="font-medium">Notes:</span> {payment.notes}
                          </p>
                        )}
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Action Button */}
      {filteredPayments.length > 0 && (
        <div className="flex justify-center">
          <Link
            href="/tenant-portal/record-payment"
            className="inline-flex items-center justify-center px-6 py-3 bg-primary text-white rounded-lg hover:bg-primary/90 transition-colors"
          >
            Record New Payment
          </Link>
        </div>
      )}
    </div>
  )
}
