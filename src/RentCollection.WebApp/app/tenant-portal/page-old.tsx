'use client'

import { useEffect, useState } from 'react'
import Link from 'next/link'
import { LoadingSpinner } from '@/components/common'
import { tenantPortalService, TenantDashboardDto } from '@/lib/services/tenantPortalService'
import { FaMoneyBillWave, FaClock, FaCheckCircle, FaExclamationTriangle, FaReceipt, FaFileAlt, FaCalendarAlt, FaInfoCircle, FaMobileAlt } from 'react-icons/fa'

export default function TenantPortalPage() {
  const [dashboard, setDashboard] = useState<TenantDashboardDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const fetchDashboard = async () => {
      try {
        setLoading(true)
        const data = await tenantPortalService.getDashboard()
        setDashboard(data)
        setError(null)
      } catch (err: any) {
        setError(err.message || 'Failed to load dashboard')
      } finally {
        setLoading(false)
      }
    }

    fetchDashboard()
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

  if (!dashboard) return null

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'Confirmed':
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
      case 'Confirmed':
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
        <p className="text-primary/60 mt-2 tracking-wide">Welcome, {dashboard.tenantInfo.fullName}</p>
      </div>

      {/* Overdue Alert */}
      {dashboard.hasOverduePayments && (
        <div className="bg-red-50 border-2 border-red-300 rounded-lg p-4">
          <div className="flex items-start gap-3">
            <FaExclamationTriangle className="text-red-600 text-xl mt-1" />
            <div className="flex-1">
              <h3 className="font-medium text-red-900">Overdue Payment</h3>
              <p className="text-sm text-red-700 mt-1">
                You have overdue payments totaling <strong>KSh {dashboard.overdueAmount.toLocaleString()}</strong>.
                {dashboard.daysOverdue > 0 && ` Payment is ${dashboard.daysOverdue} day(s) overdue.`}
              </p>
              <p className="text-xs text-red-600 mt-2">{dashboard.lateFeePolicy}</p>
              <div className="mt-3">
                <Link
                  href="/tenant-portal/pay-now"
                  className="inline-flex items-center gap-2 px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors font-medium"
                >
                  <FaMobileAlt />
                  Pay Now with M-Pesa
                </Link>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Tenant Info Card */}
      <div className="bg-white rounded-lg shadow-sm border border-primary/10 p-6">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div>
            <p className="text-sm text-primary/60 tracking-wide">Unit</p>
            <p className="text-lg font-medium text-primary mt-1">{dashboard.tenantInfo.unitNumber}</p>
          </div>
          <div>
            <p className="text-sm text-primary/60 tracking-wide">Property</p>
            <p className="text-lg font-medium text-primary mt-1">{dashboard.tenantInfo.propertyName}</p>
          </div>
          <div>
            <p className="text-sm text-primary/60 tracking-wide">Monthly Rent</p>
            <p className="text-lg font-medium text-primary mt-1">
              KSh {dashboard.tenantInfo.monthlyRent.toLocaleString()}
            </p>
          </div>
        </div>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="bg-white rounded-lg shadow-sm border border-primary/10 p-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-primary/60 tracking-wide">Current Balance</p>
              <p className="text-2xl font-medium text-primary mt-2">
                KSh {dashboard.currentBalance.toLocaleString()}
              </p>
            </div>
            <div className="p-3 bg-blue-100 rounded-lg">
              <FaMoneyBillWave className="text-blue-600 text-2xl" />
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow-sm border border-primary/10 p-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-primary/60 tracking-wide">Next Payment</p>
              <p className="text-2xl font-medium text-primary mt-2">
                {dashboard.daysUntilDue !== undefined && dashboard.daysUntilDue >= 0
                  ? `${dashboard.daysUntilDue} days`
                  : 'Overdue'}
              </p>
              {dashboard.nextPaymentAmount && (
                <p className="text-xs text-primary/60 mt-1">
                  KSh {dashboard.nextPaymentAmount.toLocaleString()}
                </p>
              )}
            </div>
            <div className={`p-3 rounded-lg ${dashboard.daysUntilDue && dashboard.daysUntilDue < 0 ? 'bg-red-100' : 'bg-yellow-100'}`}>
              <FaCalendarAlt className={`text-2xl ${dashboard.daysUntilDue && dashboard.daysUntilDue < 0 ? 'text-red-600' : 'text-yellow-600'}`} />
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow-sm border border-primary/10 p-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-primary/60 tracking-wide">Total Paid</p>
              <p className="text-2xl font-medium text-primary mt-2">
                KSh {dashboard.totalAmountPaid.toLocaleString()}
              </p>
              <p className="text-xs text-primary/60 mt-1">
                {dashboard.totalPaymentsMade} payments
              </p>
            </div>
            <div className="p-3 bg-green-100 rounded-lg">
              <FaCheckCircle className="text-green-600 text-2xl" />
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow-sm border border-primary/10 p-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-primary/60 tracking-wide">Documents</p>
              <p className="text-2xl font-medium text-primary mt-2">{dashboard.documentCount}</p>
              {dashboard.daysUntilLeaseExpiry !== undefined && (
                <p className="text-xs text-primary/60 mt-1">
                  Lease: {dashboard.daysUntilLeaseExpiry} days
                </p>
              )}
            </div>
            <div className="p-3 bg-purple-100 rounded-lg">
              <FaFileAlt className="text-purple-600 text-2xl" />
            </div>
          </div>
        </div>
      </div>

      {/* Quick Actions */}
      <div className="bg-white rounded-lg shadow-sm border border-primary/10 p-6">
        <h2 className="text-xl font-serif font-normal text-primary tracking-wide mb-4">
          Quick Actions
        </h2>
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <Link
            href="/tenant-portal/pay-now"
            className="flex items-center gap-3 p-4 border-2 border-green-500 bg-green-50 rounded-lg hover:bg-green-100 transition-colors"
          >
            <FaMobileAlt className="text-green-600 text-xl" />
            <div>
              <p className="font-medium text-green-700">Pay with M-Pesa</p>
              <p className="text-sm text-green-600">STK Push</p>
            </div>
          </Link>

          <Link
            href="/tenant-portal/record-payment"
            className="flex items-center gap-3 p-4 border border-primary/20 rounded-lg hover:bg-primary/5 transition-colors"
          >
            <FaReceipt className="text-blue-600 text-xl" />
            <div>
              <p className="font-medium text-primary">Record Payment</p>
              <p className="text-sm text-primary/60">Already paid?</p>
            </div>
          </Link>

          <Link
            href="/tenant-portal/payment-instructions"
            className="flex items-center gap-3 p-4 border border-primary/20 rounded-lg hover:bg-primary/5 transition-colors"
          >
            <FaMoneyBillWave className="text-purple-600 text-xl" />
            <div>
              <p className="font-medium text-primary">Payment Info</p>
              <p className="text-sm text-primary/60">Instructions</p>
            </div>
          </Link>

          <Link
            href="/tenant-portal/documents"
            className="flex items-center gap-3 p-4 border border-primary/20 rounded-lg hover:bg-primary/5 transition-colors"
          >
            <FaFileAlt className="text-purple-600 text-xl" />
            <div>
              <p className="font-medium text-primary">Documents</p>
              <p className="text-sm text-primary/60">Upload & view</p>
            </div>
          </Link>

          <Link
            href="/tenant-portal/lease-info"
            className="flex items-center gap-3 p-4 border border-primary/20 rounded-lg hover:bg-primary/5 transition-colors"
          >
            <FaInfoCircle className="text-orange-600 text-xl" />
            <div>
              <p className="font-medium text-primary">Lease Info</p>
              <p className="text-sm text-primary/60">View details</p>
            </div>
          </Link>
        </div>
      </div>

      {/* Pending Payments */}
      {dashboard.pendingPayments.length > 0 && (
        <div className="bg-white rounded-lg shadow-sm border border-primary/10 p-6">
          <h2 className="text-xl font-serif font-normal text-primary tracking-wide mb-4">
            Pending Payments
          </h2>
          <div className="space-y-3">
            {dashboard.pendingPayments.map((payment) => (
              <div
                key={payment.id}
                className="flex items-center justify-between p-4 border border-primary/10 rounded-lg"
              >
                <div className="flex items-center gap-3">
                  {getStatusIcon(payment.status)}
                  <div>
                    <p className="font-medium text-primary">
                      KSh {payment.totalAmount.toLocaleString()}
                      {payment.lateFeeAmount > 0 && (
                        <span className="text-sm text-red-600 ml-2">
                          (includes KSh {payment.lateFeeAmount.toLocaleString()} late fee)
                        </span>
                      )}
                    </p>
                    <p className="text-sm text-primary/60">
                      Due: {new Date(payment.dueDate).toLocaleDateString()}
                      {payment.isLate && payment.daysOverdue > 0 && (
                        <span className="text-red-600 ml-2">
                          ({payment.daysOverdue} days overdue)
                        </span>
                      )}
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
        </div>
      )}

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

        {dashboard.recentPayments.length === 0 ? (
          <p className="text-primary/60 text-center py-8">No payments recorded yet</p>
        ) : (
          <div className="space-y-3">
            {dashboard.recentPayments.map((payment) => (
              <div
                key={payment.id}
                className="flex items-center justify-between p-4 border border-primary/10 rounded-lg"
              >
                <div className="flex items-center gap-3">
                  {getStatusIcon(payment.status)}
                  <div>
                    <p className="font-medium text-primary">
                      KSh {payment.totalAmount.toLocaleString()}
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
