'use client'

import { useEffect, useState } from 'react'
import Link from 'next/link'
import { LoadingSpinner } from '@/components/common'
import { tenantPaymentService } from '@/lib/services/tenantPaymentService'
import { PaymentInstructions, TenantPaymentHistory } from '@/lib/types/tenantPayment.types'
import { FaMoneyBillWave, FaClock, FaCheckCircle, FaExclamationTriangle, FaReceipt } from 'react-icons/fa'

export default function TenantPortalPage() {
  const [instructions, setInstructions] = useState<PaymentInstructions | null>(null)
  const [recentPayments, setRecentPayments] = useState<TenantPaymentHistory[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true)
        const [instructionsData, historyData] = await Promise.all([
          tenantPaymentService.getPaymentInstructions(),
          tenantPaymentService.getPaymentHistory(),
        ])
        setInstructions(instructionsData)
        setRecentPayments(historyData.slice(0, 5)) // Show only 5 most recent
        setError(null)
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to load tenant data')
      } finally {
        setLoading(false)
      }
    }

    fetchData()
  }, [])

  if (loading) {
    return (
      <div className="flex items-center justify-center h-96">
        <LoadingSpinner size="lg" text="Loading your dashboard..." />
      </div>
    )
  }

  if (error) {
    return (
      <div className="text-center py-12">
        <p className="text-red-600">{error}</p>
      </div>
    )
  }

  if (!instructions) return null

  const pendingPayments = recentPayments.filter((p) => p.status === 'Pending').length
  const lastPayment = recentPayments.find((p) => p.status === 'Completed')

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'Completed':
        return <FaCheckCircle className="text-green-600" />
      case 'Pending':
        return <FaClock className="text-yellow-600" />
      case 'Rejected':
        return <FaExclamationTriangle className="text-red-600" />
      default:
        return <FaReceipt className="text-gray-600" />
    }
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Completed':
        return 'bg-green-100 text-green-800'
      case 'Pending':
        return 'bg-yellow-100 text-yellow-800'
      case 'Rejected':
        return 'bg-red-100 text-red-800'
      default:
        return 'bg-gray-100 text-gray-800'
    }
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-serif font-normal text-primary tracking-wide">Tenant Portal</h1>
        <p className="text-primary/60 mt-2 tracking-wide">Welcome, {instructions.tenantName}</p>
      </div>

      {/* Tenant Info Card */}
      <div className="bg-white rounded-lg shadow-sm border border-primary/10 p-6">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div>
            <p className="text-sm text-primary/60 tracking-wide">Unit</p>
            <p className="text-lg font-medium text-primary mt-1">{instructions.unitNumber}</p>
          </div>
          <div>
            <p className="text-sm text-primary/60 tracking-wide">Property</p>
            <p className="text-lg font-medium text-primary mt-1">{instructions.propertyName}</p>
          </div>
          <div>
            <p className="text-sm text-primary/60 tracking-wide">Monthly Rent</p>
            <p className="text-lg font-medium text-primary mt-1">
              KSh {instructions.monthlyRent.toLocaleString()}
            </p>
          </div>
        </div>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="bg-white rounded-lg shadow-sm border border-primary/10 p-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-primary/60 tracking-wide">Pending Payments</p>
              <p className="text-2xl font-medium text-primary mt-2">{pendingPayments}</p>
            </div>
            <div className="p-3 bg-yellow-100 rounded-lg">
              <FaClock className="text-yellow-600 text-2xl" />
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow-sm border border-primary/10 p-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-primary/60 tracking-wide">Last Payment</p>
              <p className="text-2xl font-medium text-primary mt-2">
                {lastPayment ? `KSh ${lastPayment.amount.toLocaleString()}` : 'N/A'}
              </p>
              {lastPayment && (
                <p className="text-xs text-primary/60 mt-1">
                  {new Date(lastPayment.paymentDate).toLocaleDateString()}
                </p>
              )}
            </div>
            <div className="p-3 bg-green-100 rounded-lg">
              <FaCheckCircle className="text-green-600 text-2xl" />
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow-sm border border-primary/10 p-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-primary/60 tracking-wide">Total Payments</p>
              <p className="text-2xl font-medium text-primary mt-2">{recentPayments.length}</p>
            </div>
            <div className="p-3 bg-blue-100 rounded-lg">
              <FaReceipt className="text-blue-600 text-2xl" />
            </div>
          </div>
        </div>
      </div>

      {/* Quick Actions */}
      <div className="bg-white rounded-lg shadow-sm border border-primary/10 p-6">
        <h2 className="text-xl font-serif font-normal text-primary tracking-wide mb-4">
          Quick Actions
        </h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <Link
            href="/tenant-portal/payment-instructions"
            className="flex items-center gap-3 p-4 border border-primary/20 rounded-lg hover:bg-primary/5 transition-colors"
          >
            <FaMoneyBillWave className="text-blue-600 text-xl" />
            <div>
              <p className="font-medium text-primary">Payment Instructions</p>
              <p className="text-sm text-primary/60">View how to pay rent</p>
            </div>
          </Link>

          <Link
            href="/tenant-portal/record-payment"
            className="flex items-center gap-3 p-4 border border-primary/20 rounded-lg hover:bg-primary/5 transition-colors"
          >
            <FaReceipt className="text-green-600 text-xl" />
            <div>
              <p className="font-medium text-primary">Record Payment</p>
              <p className="text-sm text-primary/60">Submit payment details</p>
            </div>
          </Link>

          <Link
            href="/tenant-portal/history"
            className="flex items-center gap-3 p-4 border border-primary/20 rounded-lg hover:bg-primary/5 transition-colors"
          >
            <FaClock className="text-purple-600 text-xl" />
            <div>
              <p className="font-medium text-primary">Payment History</p>
              <p className="text-sm text-primary/60">View all payments</p>
            </div>
          </Link>
        </div>
      </div>

      {/* Recent Payments */}
      <div className="bg-white rounded-lg shadow-sm border border-primary/10 p-6">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-xl font-serif font-normal text-primary tracking-wide">
            Recent Payments
          </h2>
          <Link
            href="/tenant-portal/history"
            className="text-sm text-primary/60 hover:text-primary transition-colors"
          >
            View all →
          </Link>
        </div>

        {recentPayments.length === 0 ? (
          <p className="text-primary/60 text-center py-8">No payments recorded yet</p>
        ) : (
          <div className="space-y-3">
            {recentPayments.map((payment) => (
              <div
                key={payment.id}
                className="flex items-center justify-between p-4 border border-primary/10 rounded-lg"
              >
                <div className="flex items-center gap-3">
                  {getStatusIcon(payment.status)}
                  <div>
                    <p className="font-medium text-primary">
                      KSh {payment.amount.toLocaleString()}
                    </p>
                    <p className="text-sm text-primary/60">
                      {new Date(payment.paymentDate).toLocaleDateString()} •{' '}
                      {payment.paymentMethod}
                    </p>
                    {payment.transactionReference && (
                      <p className="text-xs text-primary/60 mt-1">
                        Ref: {payment.transactionReference}
                      </p>
                    )}
                  </div>
                </div>
                <span
                  className={`px-3 py-1 rounded-full text-xs font-medium ${getStatusColor(
                    payment.status
                  )}`}
                >
                  {payment.status}
                </span>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  )
}
