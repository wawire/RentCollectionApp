'use client'

import { useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { useAuth } from '@/contexts/AuthContext'
import { UserRole } from '@/lib/types/auth.types'
import { useGetDashboardStats } from '@/lib/hooks'
import StatsCard from '@/components/dashboard/StatsCard'
import RecentPayments from '@/components/dashboard/RecentPayments'
import OccupancyChart from '@/components/dashboard/OccupancyChart'
import RevenueChart from '@/components/dashboard/RevenueChart'
import QuickActions from '@/components/dashboard/QuickActions'
import PaymentStatusWidget from '@/components/dashboard/PaymentStatusWidget'
import { LoadingSpinner } from '@/components/common'
import { Banknote, Building2, DoorOpen, Users } from 'lucide-react'

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

  // Show loading while checking authentication
  if (authLoading) {
    return (
      <div className="flex items-center justify-center h-96">
        <LoadingSpinner size="lg" text="Loading..." />
      </div>
    )
  }

  // Don't render dashboard for tenants (they'll be redirected)
  if (user?.role === UserRole.Tenant) {
    return null
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center h-96">
        <LoadingSpinner size="lg" text="Loading dashboard..." />
      </div>
    )
  }

  if (error) {
    return (
      <div className="text-center py-12">
        <p className="text-red-600">Error loading dashboard: {error.message}</p>
      </div>
    )
  }

  if (!stats) return null

  const occupancyRate = stats.totalUnits > 0 ? (stats.occupiedUnits / stats.totalUnits) * 100 : 0

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-serif font-normal text-primary tracking-wide">Dashboard</h1>
        <p className="text-primary/60 mt-2 tracking-wide">Overview of your rental properties and finances</p>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <StatsCard
          title="Total Properties"
          value={stats.totalProperties}
          icon={Building2}
          color="blue"
        />
        <StatsCard
          title="Total Units"
          value={stats.totalUnits}
          subtitle={`${stats.occupiedUnits} occupied, ${stats.vacantUnits} vacant`}
          icon={DoorOpen}
          color="purple"
        />
        <StatsCard
          title="Active Tenants"
          value={stats.activeTenants}
          icon={Users}
          color="green"
        />
        <StatsCard
          title="Collection Rate"
          value={`${stats.collectionRate.toFixed(1)}%`}
          subtitle={`KSh ${stats.totalRentCollected.toLocaleString()} / ${stats.totalRentExpected.toLocaleString()}`}
          icon={Banknote}
          color="yellow"
        />
      </div>

      {/* Charts and Recent Activity */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Occupancy Chart */}
        <OccupancyChart
          occupied={stats.occupiedUnits}
          vacant={stats.vacantUnits}
          occupancyRate={occupancyRate}
        />

        {/* Payment Status Widget */}
        <PaymentStatusWidget
          unitsPaid={stats.unitsPaid}
          unitsOverdue={stats.unitsOverdue}
          unitsPending={stats.unitsPending}
          totalOccupied={stats.occupiedUnits}
        />
      </div>

      {/* Quick Actions */}
      <QuickActions />

      {/* Revenue Chart */}
      <RevenueChart />

      {/* Recent Payments */}
      <RecentPayments />
    </div>
  )
}
