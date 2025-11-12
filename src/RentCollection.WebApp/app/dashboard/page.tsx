'use client'

import { useGetDashboardStats } from '@/lib/hooks'
import StatsCard from '@/components/dashboard/StatsCard'
import RecentPayments from '@/components/dashboard/RecentPayments'
import OccupancyChart from '@/components/dashboard/OccupancyChart'
import QuickActions from '@/components/dashboard/QuickActions'
import { LoadingSpinner } from '@/components/common'
import { FaBuilding, FaDoorOpen, FaUsers, FaMoneyBillWave } from 'react-icons/fa'

export default function DashboardPage() {
  const { data: stats, loading, error } = useGetDashboardStats()

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
        <h1 className="text-3xl font-bold text-gray-900">Dashboard</h1>
        <p className="text-gray-600 mt-1">Overview of your rental properties and finances</p>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <StatsCard
          title="Total Properties"
          value={stats.totalProperties}
          icon={FaBuilding}
          color="blue"
        />
        <StatsCard
          title="Total Units"
          value={stats.totalUnits}
          subtitle={`${stats.occupiedUnits} occupied, ${stats.vacantUnits} vacant`}
          icon={FaDoorOpen}
          color="purple"
        />
        <StatsCard
          title="Active Tenants"
          value={stats.activeTenants}
          icon={FaUsers}
          color="green"
        />
        <StatsCard
          title="Collection Rate"
          value={`${stats.collectionRate.toFixed(1)}%`}
          subtitle={`KSh ${stats.totalRentCollected.toLocaleString()} / ${stats.totalRentExpected.toLocaleString()}`}
          icon={FaMoneyBillWave}
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

        {/* Quick Actions */}
        <QuickActions />
      </div>

      {/* Recent Payments */}
      <RecentPayments />
    </div>
  )
}
