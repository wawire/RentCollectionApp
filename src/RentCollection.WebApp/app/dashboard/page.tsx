'use client'

import { useEffect } from 'react'
import { useRouter } from 'next/navigation'
import Link from 'next/link'
import { useAuth } from '@/contexts/AuthContext'
import { UserRole } from '@/lib/types/auth.types'
import { useGetDashboardStats } from '@/lib/hooks'
import { LoadingSpinner } from '@/components/common'
import {
  FaBuilding,
  FaDoorOpen,
  FaUsers,
  FaMoneyBillWave,
  FaChartLine,
  FaExclamationCircle,
  FaCheckCircle,
  FaClock,
  FaPlus,
  FaArrowRight
} from 'react-icons/fa'

export default function DashboardPage() {
  const router = useRouter()
  const { user, isLoading: authLoading } = useAuth()
  const { data: stats, loading, error } = useGetDashboardStats()

  // Redirect tenants to their own portal
  useEffect(() => {
    if (!authLoading && user?.role === UserRole.Tenant) {
      router.replace('/tenant-portal')
    }
  }, [user, authLoading, router])

  if (authLoading || loading) {
    return (
      <div className="flex items-center justify-center h-96">
        <LoadingSpinner size="lg" text="Loading your dashboard..." />
      </div>
    )
  }

  if (user?.role === UserRole.Tenant) return null

  if (error || !stats) {
    return (
      <div className="text-center py-12">
        <p className="text-red-600">Unable to load dashboard data</p>
      </div>
    )
  }

  const occupancyRate = stats.totalUnits > 0 ? (stats.occupiedUnits / stats.totalUnits) * 100 : 0
  const hasOverduePayments = stats.unitsOverdue > 0

  return (
    <div className="space-y-8">
      {/* Welcome Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Welcome back!</h1>
          <p className="text-gray-600 mt-1">Here's what's happening with your properties today</p>
        </div>
        <Link
          href="/properties/new"
          className="hidden md:flex items-center gap-2 px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors font-medium shadow-sm"
        >
          <FaPlus />
          Add Property
        </Link>
      </div>

      {/* Alert Banner - Overdue Payments */}
      {hasOverduePayments && (
        <div className="bg-gradient-to-r from-red-50 to-orange-50 border-l-4 border-red-500 rounded-lg p-6 shadow-sm">
          <div className="flex items-start gap-4">
            <div className="flex-shrink-0">
              <FaExclamationCircle className="text-red-600 text-2xl" />
            </div>
            <div className="flex-1">
              <h3 className="text-lg font-semibold text-red-900">Action Required</h3>
              <p className="text-red-700 mt-1">
                You have <strong>{stats.unitsOverdue} unit{stats.unitsOverdue > 1 ? 's' : ''}</strong> with overdue payments
              </p>
              <Link
                href="/payments?filter=overdue"
                className="inline-flex items-center gap-2 mt-3 text-red-700 font-medium hover:text-red-800"
              >
                Review overdue payments
                <FaArrowRight className="text-sm" />
              </Link>
            </div>
          </div>
        </div>
      )}

      {/* Key Metrics - Clean & Modern */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {/* Properties */}
        <div className="bg-white rounded-lg shadow-sm border border-gray-100 p-5 hover:shadow-md transition-shadow">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-xs font-medium text-gray-600 uppercase tracking-wide">Properties</p>
              <p className="text-2xl font-bold text-gray-900 mt-1">{stats.totalProperties}</p>
            </div>
            <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
              <FaBuilding className="text-blue-600 text-lg" />
            </div>
          </div>
          <Link
            href="/properties"
            className="inline-flex items-center gap-1 mt-4 text-sm text-blue-600 hover:text-blue-700 font-medium"
          >
            View all
            <FaArrowRight className="text-xs" />
          </Link>
        </div>

        {/* Units Occupancy */}
        <div className="bg-white rounded-lg shadow-sm border border-gray-100 p-5 hover:shadow-md transition-shadow">
          <div className="flex items-center justify-between mb-3">
            <div>
              <p className="text-xs font-medium text-gray-600 uppercase tracking-wide">Occupancy</p>
              <p className="text-2xl font-bold text-gray-900 mt-1">{occupancyRate.toFixed(0)}%</p>
              <p className="text-xs text-gray-500 mt-0.5">{stats.occupiedUnits}/{stats.totalUnits} units</p>
            </div>
            <div className="w-10 h-10 bg-purple-100 rounded-lg flex items-center justify-center">
              <FaDoorOpen className="text-purple-600 text-lg" />
            </div>
          </div>
          <div className="mt-4 w-full bg-gray-200 rounded-full h-2">
            <div
              className="bg-purple-600 h-2 rounded-full transition-all"
              style={{ width: `${occupancyRate}%` }}
            ></div>
          </div>
        </div>

        {/* Active Tenants */}
        <div className="bg-white rounded-lg shadow-sm border border-gray-100 p-5 hover:shadow-md transition-shadow">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-xs font-medium text-gray-600 uppercase tracking-wide">Tenants</p>
              <p className="text-2xl font-bold text-gray-900 mt-1">{stats.activeTenants}</p>
            </div>
            <div className="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center">
              <FaUsers className="text-green-600 text-lg" />
            </div>
          </div>
          <Link
            href="/tenants"
            className="inline-flex items-center gap-1 mt-4 text-sm text-green-600 hover:text-green-700 font-medium"
          >
            Manage tenants
            <FaArrowRight className="text-xs" />
          </Link>
        </div>

        {/* Revenue */}
        <div className="bg-white rounded-lg shadow-sm border border-gray-100 p-5 hover:shadow-md transition-shadow">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-xs font-medium text-gray-600 uppercase tracking-wide">Revenue</p>
              <p className="text-2xl font-bold text-gray-900 mt-1">
                KSh {(stats.totalRentCollected / 1000).toFixed(0)}K
              </p>
              <p className="text-xs text-gray-500 mt-0.5">
                {stats.collectionRate.toFixed(0)}% collected
              </p>
            </div>
            <div className="w-10 h-10 bg-yellow-100 rounded-lg flex items-center justify-center">
              <FaMoneyBillWave className="text-yellow-600 text-lg" />
            </div>
          </div>
          <Link
            href="/payments"
            className="inline-flex items-center gap-1 mt-4 text-sm text-yellow-600 hover:text-yellow-700 font-medium"
          >
            View payments
            <FaArrowRight className="text-xs" />
          </Link>
        </div>
      </div>

      {/* Payment Status Overview */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-xl font-semibold text-gray-900">Payment Status</h2>
          <Link
            href="/payments"
            className="text-sm text-blue-600 hover:text-blue-700 font-medium"
          >
            View all
          </Link>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          {/* Paid */}
          <div className="flex items-center gap-4 p-4 bg-green-50 rounded-lg border border-green-200">
            <div className="flex-shrink-0">
              <div className="w-10 h-10 bg-green-600 rounded-full flex items-center justify-center">
                <FaCheckCircle className="text-white" />
              </div>
            </div>
            <div>
              <p className="text-2xl font-bold text-green-900">{stats.unitsPaid}</p>
              <p className="text-sm text-green-700">Units Paid</p>
            </div>
          </div>

          {/* Pending */}
          <div className="flex items-center gap-4 p-4 bg-yellow-50 rounded-lg border border-yellow-200">
            <div className="flex-shrink-0">
              <div className="w-10 h-10 bg-yellow-600 rounded-full flex items-center justify-center">
                <FaClock className="text-white" />
              </div>
            </div>
            <div>
              <p className="text-2xl font-bold text-yellow-900">{stats.unitsPending}</p>
              <p className="text-sm text-yellow-700">Awaiting Confirmation</p>
            </div>
          </div>

          {/* Overdue */}
          <div className="flex items-center gap-4 p-4 bg-red-50 rounded-lg border border-red-200">
            <div className="flex-shrink-0">
              <div className="w-10 h-10 bg-red-600 rounded-full flex items-center justify-center">
                <FaExclamationCircle className="text-white" />
              </div>
            </div>
            <div>
              <p className="text-2xl font-bold text-red-900">{stats.unitsOverdue}</p>
              <p className="text-sm text-red-700">Overdue Payments</p>
            </div>
          </div>
        </div>
      </div>

      {/* Quick Actions */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h2 className="text-xl font-semibold text-gray-900 mb-6">Quick Actions</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          <Link
            href="/properties/new"
            className="flex items-center gap-3 p-4 border-2 border-gray-200 rounded-lg hover:border-blue-500 hover:bg-blue-50 transition-all group"
          >
            <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center group-hover:bg-blue-600 transition-colors">
              <FaBuilding className="text-blue-600 group-hover:text-white transition-colors" />
            </div>
            <div>
              <p className="font-medium text-gray-900">Add Property</p>
              <p className="text-sm text-gray-500">Create new listing</p>
            </div>
          </Link>

          <Link
            href="/units/new"
            className="flex items-center gap-3 p-4 border-2 border-gray-200 rounded-lg hover:border-purple-500 hover:bg-purple-50 transition-all group"
          >
            <div className="w-10 h-10 bg-purple-100 rounded-lg flex items-center justify-center group-hover:bg-purple-600 transition-colors">
              <FaDoorOpen className="text-purple-600 group-hover:text-white transition-colors" />
            </div>
            <div>
              <p className="font-medium text-gray-900">Add Unit</p>
              <p className="text-sm text-gray-500">Create rental unit</p>
            </div>
          </Link>

          <Link
            href="/tenants/new"
            className="flex items-center gap-3 p-4 border-2 border-gray-200 rounded-lg hover:border-green-500 hover:bg-green-50 transition-all group"
          >
            <div className="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center group-hover:bg-green-600 transition-colors">
              <FaUsers className="text-green-600 group-hover:text-white transition-colors" />
            </div>
            <div>
              <p className="font-medium text-gray-900">Add Tenant</p>
              <p className="text-sm text-gray-500">Onboard new tenant</p>
            </div>
          </Link>

          <Link
            href="/payments/new"
            className="flex items-center gap-3 p-4 border-2 border-gray-200 rounded-lg hover:border-yellow-500 hover:bg-yellow-50 transition-all group"
          >
            <div className="w-10 h-10 bg-yellow-100 rounded-lg flex items-center justify-center group-hover:bg-yellow-600 transition-colors">
              <FaMoneyBillWave className="text-yellow-600 group-hover:text-white transition-colors" />
            </div>
            <div>
              <p className="font-medium text-gray-900">Record Payment</p>
              <p className="text-sm text-gray-500">Log transaction</p>
            </div>
          </Link>
        </div>
      </div>
    </div>
  )
}
