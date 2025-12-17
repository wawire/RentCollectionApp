'use client'

import { useEffect, useState } from 'react'
import Link from 'next/link'
import { LoadingSpinner } from '@/components/common'
import { tenantPortalService, TenantDashboardDto } from '@/lib/services/tenantPortalService'
import { FaMoneyBillWave, FaClock, FaCheckCircle, FaExclamationTriangle, FaReceipt, FaFileAlt, FaCalendarAlt, FaInfoCircle, FaMobileAlt, FaArrowRight, FaHome } from 'react-icons/fa'

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
      {/* Welcome Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">
            Welcome back, {dashboard.tenantInfo.fullName.split(' ')[0]}!
          </h1>
          <p className="text-gray-600 mt-1">Here's your rental summary</p>
        </div>
        <Link
          href="/tenant-portal/pay-now"
          className="hidden md:flex items-center gap-2 px-6 py-3 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors font-medium shadow-sm"
        >
          <FaMobileAlt />
          Pay Rent
        </Link>
      </div>

      {/* Overdue Alert */}
      {dashboard.hasOverduePayments && (
        <div className="bg-gradient-to-r from-red-50 to-orange-50 border-l-4 border-red-500 rounded-lg p-6 shadow-sm">
          <div className="flex items-start gap-4">
            <div className="flex-shrink-0">
              <FaExclamationTriangle className="text-red-600 text-2xl" />
            </div>
            <div className="flex-1">
              <h3 className="text-lg font-semibold text-red-900">Payment Overdue</h3>
              <p className="text-red-700 mt-1">
                You have an overdue balance of <strong>KSh {dashboard.overdueAmount.toLocaleString()}</strong>.
                {dashboard.daysOverdue > 0 && ` Your payment is ${dashboard.daysOverdue} day(s) overdue.`}
              </p>
              <p className="text-sm text-red-600 mt-2">{dashboard.lateFeePolicy}</p>
              <Link
                href="/tenant-portal/pay-now"
                className="inline-flex items-center gap-2 mt-4 px-5 py-2.5 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors font-medium shadow-sm"
              >
                <FaMobileAlt />
                Pay Now
              </Link>
            </div>
          </div>
        </div>
      )}

      {/* Property Info Card */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <div className="flex items-center gap-3 mb-4">
          <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
            <FaHome className="text-blue-600 text-lg" />
          </div>
          <h2 className="text-lg font-semibold text-gray-900">Your Rental</h2>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div>
            <p className="text-sm text-gray-600">Unit Number</p>
            <p className="text-xl font-semibold text-gray-900 mt-1">{dashboard.tenantInfo.unitNumber}</p>
          </div>
          <div>
            <p className="text-sm text-gray-600">Property</p>
            <p className="text-xl font-semibold text-gray-900 mt-1">{dashboard.tenantInfo.propertyName}</p>
          </div>
          <div>
            <p className="text-sm text-gray-600">Monthly Rent</p>
            <p className="text-xl font-semibold text-gray-900 mt-1">
              KSh {dashboard.tenantInfo.monthlyRent.toLocaleString()}
            </p>
          </div>
        </div>
      </div>

      {/* Key Stats Grid */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {/* Current Balance */}
        <div className="bg-white rounded-lg shadow-sm border border-gray-100 p-5 hover:shadow-md transition-shadow">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-xs font-medium text-gray-600 uppercase tracking-wide">Balance</p>
              <p className="text-2xl font-bold text-gray-900 mt-1">
                KSh {dashboard.currentBalance.toLocaleString()}
              </p>
            </div>
            <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
              <FaMoneyBillWave className="text-blue-600 text-lg" />
            </div>
          </div>
        </div>

        {/* Next Payment Due */}
        <div className="bg-white rounded-lg shadow-sm border border-gray-100 p-5 hover:shadow-md transition-shadow">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-xs font-medium text-gray-600 uppercase tracking-wide">Next Due</p>
              <p className="text-2xl font-bold text-gray-900 mt-1">
                {dashboard.daysUntilDue !== undefined && dashboard.daysUntilDue >= 0
                  ? `${dashboard.daysUntilDue} days`
                  : 'Overdue'}
              </p>
              {dashboard.nextPaymentAmount && (
                <p className="text-xs text-gray-500 mt-0.5">
                  KSh {dashboard.nextPaymentAmount.toLocaleString()}
                </p>
              )}
            </div>
            <div className={`w-10 h-10 rounded-lg flex items-center justify-center ${dashboard.daysUntilDue && dashboard.daysUntilDue < 0 ? 'bg-red-100' : 'bg-yellow-100'}`}>
              <FaCalendarAlt className={`text-lg ${dashboard.daysUntilDue && dashboard.daysUntilDue < 0 ? 'text-red-600' : 'text-yellow-600'}`} />
            </div>
          </div>
        </div>

        {/* Total Paid */}
        <div className="bg-white rounded-lg shadow-sm border border-gray-100 p-5 hover:shadow-md transition-shadow">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-xs font-medium text-gray-600 uppercase tracking-wide">Total Paid</p>
              <p className="text-2xl font-bold text-gray-900 mt-1">
                KSh {dashboard.totalAmountPaid.toLocaleString()}
              </p>
              <p className="text-xs text-gray-500 mt-0.5">
                {dashboard.totalPaymentsMade} payment{dashboard.totalPaymentsMade !== 1 ? 's' : ''}
              </p>
            </div>
            <div className="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center">
              <FaCheckCircle className="text-green-600 text-lg" />
            </div>
          </div>
        </div>

        {/* Documents */}
        <div className="bg-white rounded-lg shadow-sm border border-gray-100 p-5 hover:shadow-md transition-shadow">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-xs font-medium text-gray-600 uppercase tracking-wide">Documents</p>
              <p className="text-2xl font-bold text-gray-900 mt-1">{dashboard.documentCount}</p>
              {dashboard.daysUntilLeaseExpiry !== undefined && (
                <p className="text-xs text-gray-500 mt-0.5">
                  Lease: {dashboard.daysUntilLeaseExpiry} days
                </p>
              )}
            </div>
            <div className="w-10 h-10 bg-purple-100 rounded-lg flex items-center justify-center">
              <FaFileAlt className="text-purple-600 text-lg" />
            </div>
          </div>
        </div>
      </div>

      {/* Quick Actions */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h2 className="text-xl font-semibold text-gray-900 mb-6">Quick Actions</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <Link
            href="/tenant-portal/pay-now"
            className="flex items-center gap-4 p-5 bg-gradient-to-r from-green-50 to-emerald-50 border-2 border-green-500 rounded-lg hover:shadow-md transition-all group"
          >
            <div className="w-12 h-12 bg-green-600 rounded-lg flex items-center justify-center group-hover:scale-110 transition-transform">
              <FaMobileAlt className="text-white text-xl" />
            </div>
            <div>
              <p className="font-semibold text-green-900">Pay Rent</p>
              <p className="text-sm text-green-700">Instant M-Pesa payment</p>
            </div>
          </Link>

          <Link
            href="/tenant-portal/record-payment"
            className="flex items-center gap-4 p-5 border-2 border-gray-200 rounded-lg hover:border-blue-500 hover:bg-blue-50 transition-all group"
          >
            <div className="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center group-hover:bg-blue-600 transition-colors">
              <FaReceipt className="text-blue-600 group-hover:text-white text-xl transition-colors" />
            </div>
            <div>
              <p className="font-semibold text-gray-900">Record Payment</p>
              <p className="text-sm text-gray-600">Already paid elsewhere?</p>
            </div>
          </Link>

          <Link
            href="/tenant-portal/documents"
            className="flex items-center gap-4 p-5 border-2 border-gray-200 rounded-lg hover:border-purple-500 hover:bg-purple-50 transition-all group"
          >
            <div className="w-12 h-12 bg-purple-100 rounded-lg flex items-center justify-center group-hover:bg-purple-600 transition-colors">
              <FaFileAlt className="text-purple-600 group-hover:text-white text-xl transition-colors" />
            </div>
            <div>
              <p className="font-semibold text-gray-900">Documents</p>
              <p className="text-sm text-gray-600">Upload & view files</p>
            </div>
          </Link>
        </div>

        {/* Additional Links */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mt-4">
          <Link
            href="/tenant-portal/payment-instructions"
            className="flex items-center justify-between p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors"
          >
            <div className="flex items-center gap-3">
              <FaInfoCircle className="text-gray-400 text-lg" />
              <span className="text-sm font-medium text-gray-700">Payment Instructions</span>
            </div>
            <FaArrowRight className="text-gray-400 text-sm" />
          </Link>

          <Link
            href="/tenant-portal/lease-info"
            className="flex items-center justify-between p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors"
          >
            <div className="flex items-center gap-3">
              <FaFileAlt className="text-gray-400 text-lg" />
              <span className="text-sm font-medium text-gray-700">Lease Information</span>
            </div>
            <FaArrowRight className="text-gray-400 text-sm" />
          </Link>

          <Link
            href="/tenant-portal/history"
            className="flex items-center justify-between p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors"
          >
            <div className="flex items-center gap-3">
              <FaClock className="text-gray-400 text-lg" />
              <span className="text-sm font-medium text-gray-700">Payment History</span>
            </div>
            <FaArrowRight className="text-gray-400 text-sm" />
          </Link>
        </div>
      </div>

      {/* Pending Payments */}
      {dashboard.pendingPayments.length > 0 && (
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
          <h2 className="text-xl font-semibold text-gray-900 mb-4">
            Pending Payments
          </h2>
          <div className="space-y-3">
            {dashboard.pendingPayments.map((payment) => (
              <div
                key={payment.id}
                className="flex items-center justify-between p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors"
              >
                <div className="flex items-center gap-4">
                  <div className="flex-shrink-0">
                    {getStatusIcon(payment.status)}
                  </div>
                  <div>
                    <p className="font-semibold text-gray-900">
                      KSh {payment.totalAmount.toLocaleString()}
                      {payment.lateFeeAmount > 0 && (
                        <span className="text-sm text-red-600 ml-2 font-normal">
                          (includes KSh {payment.lateFeeAmount.toLocaleString()} late fee)
                        </span>
                      )}
                    </p>
                    <p className="text-sm text-gray-600">
                      Due: {new Date(payment.dueDate).toLocaleDateString()}
                      {payment.isLate && payment.daysOverdue > 0 && (
                        <span className="text-red-600 ml-2">
                          ({payment.daysOverdue} days overdue)
                        </span>
                      )}
                    </p>
                    {payment.transactionReference && (
                      <p className="text-xs text-gray-500 mt-1">
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
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-xl font-semibold text-gray-900">Recent Payments</h2>
          <Link
            href="/tenant-portal/history"
            className="text-sm text-blue-600 hover:text-blue-700 font-medium flex items-center gap-1"
          >
            View all
            <FaArrowRight className="text-xs" />
          </Link>
        </div>

        {dashboard.recentPayments.length === 0 ? (
          <div className="text-center py-12">
            <div className="inline-flex items-center justify-center w-16 h-16 bg-gray-100 rounded-full mb-4">
              <FaReceipt className="text-gray-400 text-2xl" />
            </div>
            <p className="text-gray-600">No payments recorded yet</p>
            <Link
              href="/tenant-portal/pay-now"
              className="inline-flex items-center gap-2 mt-4 px-5 py-2.5 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors font-medium"
            >
              <FaMobileAlt />
              Make Your First Payment
            </Link>
          </div>
        ) : (
          <div className="space-y-3">
            {dashboard.recentPayments.map((payment) => (
              <div
                key={payment.id}
                className="flex items-center justify-between p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors"
              >
                <div className="flex items-center gap-4">
                  <div className="flex-shrink-0">
                    {getStatusIcon(payment.status)}
                  </div>
                  <div>
                    <p className="font-semibold text-gray-900">
                      KSh {payment.totalAmount.toLocaleString()}
                    </p>
                    <p className="text-sm text-gray-600">
                      {new Date(payment.paymentDate).toLocaleDateString()} •{' '}
                      {payment.paymentMethod}
                    </p>
                    {payment.transactionReference && (
                      <p className="text-xs text-gray-500 mt-1">
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
