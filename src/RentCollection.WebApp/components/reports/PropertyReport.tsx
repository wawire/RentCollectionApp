'use client'

import { useState, useMemo } from 'react'
import { Card, Select, LoadingSpinner, Button, Alert } from '@/components/common'
import { useGetProperties, useGetPayments, useDownloadRentRoll } from '@/lib/hooks'
import { Banknote, DoorClosed, DoorOpen, Home, Percent } from 'lucide-react'
import { PaymentStatus } from '@/lib/types'

export default function PropertyReport() {
  const { data: properties, loading: loadingProperties } = useGetProperties()
  const { data: payments, loading: loadingPayments } = useGetPayments()
  const { downloadRentRoll, loading: downloading } = useDownloadRentRoll()

  const [selectedPropertyId, setSelectedPropertyId] = useState<number | 'all'>('all')
  const [alert, setAlert] = useState<{ type: 'success' | 'error'; message: string } | null>(null)

  const propertyOptions = [
    { value: 'all', label: 'All Properties' },
    ...properties.map(p => ({ value: p.id, label: p.name }))
  ]

  // Calculate property statistics
  const propertyStats = useMemo(() => {
    if (selectedPropertyId === 'all') {
      // Aggregate stats for all properties
      return properties.map(property => {
        const propertyPayments = payments.filter(
          p => p.propertyName === property.name && p.status === PaymentStatus.Completed
        )
        const totalRevenue = propertyPayments.reduce((sum, p) => sum + p.amount, 0)
        const occupancyRate = property.totalUnits > 0
          ? (property.occupiedUnits / property.totalUnits) * 100
          : 0

        return {
          id: property.id,
          name: property.name,
          location: property.location,
          totalUnits: property.totalUnits,
          occupiedUnits: property.occupiedUnits,
          vacantUnits: property.vacantUnits,
          occupancyRate,
          totalRevenue,
          averageRentPerUnit: property.totalUnits > 0 ? totalRevenue / property.totalUnits : 0,
        }
      })
    } else {
      // Stats for selected property
      const property = properties.find(p => p.id === selectedPropertyId)
      if (!property) return []

      const propertyPayments = payments.filter(
        p => p.propertyName === property.name && p.status === PaymentStatus.Completed
      )
      const totalRevenue = propertyPayments.reduce((sum, p) => sum + p.amount, 0)
      const occupancyRate = property.totalUnits > 0
        ? (property.occupiedUnits / property.totalUnits) * 100
        : 0

      return [{
        id: property.id,
        name: property.name,
        location: property.location,
        totalUnits: property.totalUnits,
        occupiedUnits: property.occupiedUnits,
        vacantUnits: property.vacantUnits,
        occupancyRate,
        totalRevenue,
        averageRentPerUnit: property.totalUnits > 0 ? totalRevenue / property.totalUnits : 0,
      }]
    }
  }, [properties, payments, selectedPropertyId])

  // Calculate totals
  const totals = useMemo(() => {
    const totalUnits = propertyStats.reduce((sum, p) => sum + p.totalUnits, 0)
    const occupiedUnits = propertyStats.reduce((sum, p) => sum + p.occupiedUnits, 0)
    const vacantUnits = propertyStats.reduce((sum, p) => sum + p.vacantUnits, 0)
    const totalRevenue = propertyStats.reduce((sum, p) => sum + p.totalRevenue, 0)
    const overallOccupancyRate = totalUnits > 0 ? (occupiedUnits / totalUnits) * 100 : 0

    return {
      totalUnits,
      occupiedUnits,
      vacantUnits,
      totalRevenue,
      overallOccupancyRate,
    }
  }, [propertyStats])

  const loading = loadingProperties || loadingPayments

  const handleDownloadRentRoll = async () => {
    const propertyId = selectedPropertyId === 'all' ? undefined : selectedPropertyId
    const success = await downloadRentRoll(propertyId)
    if (success) {
      setAlert({ type: 'success', message: 'Rent roll downloaded successfully!' })
    } else {
      setAlert({ type: 'error', message: 'Failed to download rent roll' })
    }
    setTimeout(() => setAlert(null), 3000)
  }

  if (loading) {
    return (
      <div className="flex justify-center py-12">
        <LoadingSpinner size="lg" text="Loading property data..." />
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold text-gray-900">Property Performance Report</h2>
        <Button variant="primary" onClick={handleDownloadRentRoll} loading={downloading}>
          Download Rent Roll
        </Button>
      </div>

      {alert && (
        <Alert
          type={alert.type}
          message={alert.message}
          onClose={() => setAlert(null)}
        />
      )}

      {/* Property Selector */}
      <Card padding="md">
        <Select
          label="Select Property"
          value={selectedPropertyId}
          onChange={(e) => setSelectedPropertyId(e.target.value === 'all' ? 'all' : Number(e.target.value))}
          options={propertyOptions}
          fullWidth
        />
      </Card>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-4">
        <Card padding="md" className="bg-gradient-to-br from-blue-50 to-blue-100">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-blue-600 font-medium mb-1">Total Units</p>
              <p className="text-2xl font-bold text-blue-900">{totals.totalUnits}</p>
            </div>
            <Home className="text-3xl text-blue-600" />
          </div>
        </Card>

        <Card padding="md" className="bg-gradient-to-br from-green-50 to-green-100">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-green-600 font-medium mb-1">Occupied</p>
              <p className="text-2xl font-bold text-green-900">{totals.occupiedUnits}</p>
            </div>
            <DoorClosed className="text-3xl text-green-600" />
          </div>
        </Card>

        <Card padding="md" className="bg-gradient-to-br from-orange-50 to-orange-100">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-orange-600 font-medium mb-1">Vacant</p>
              <p className="text-2xl font-bold text-orange-900">{totals.vacantUnits}</p>
            </div>
            <DoorOpen className="text-3xl text-orange-600" />
          </div>
        </Card>

        <Card padding="md" className="bg-gradient-to-br from-purple-50 to-purple-100">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-purple-600 font-medium mb-1">Occupancy Rate</p>
              <p className="text-2xl font-bold text-purple-900">{totals.overallOccupancyRate.toFixed(1)}%</p>
            </div>
            <Percent className="text-3xl text-purple-600" />
          </div>
        </Card>

        <Card padding="md" className="bg-gradient-to-br from-indigo-50 to-indigo-100">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-indigo-600 font-medium mb-1">Total Revenue</p>
              <p className="text-2xl font-bold text-indigo-900">
                KSh {(totals.totalRevenue / 1000).toFixed(0)}K
              </p>
            </div>
            <Banknote className="text-3xl text-indigo-600" />
          </div>
        </Card>
      </div>

      {/* Property Details Table */}
      <Card padding="none">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Property</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Location</th>
                <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase">Total Units</th>
                <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase">Occupied</th>
                <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase">Vacant</th>
                <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase">Occupancy Rate</th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">Total Revenue</th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">Avg/Unit</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {propertyStats.map((property) => (
                <tr key={property.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 whitespace-nowrap">
                    <p className="text-sm font-medium text-gray-900">{property.name}</p>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <p className="text-sm text-gray-600">{property.location}</p>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-center">
                    <p className="text-sm text-gray-900">{property.totalUnits}</p>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-center">
                    <p className="text-sm text-green-600 font-medium">{property.occupiedUnits}</p>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-center">
                    <p className="text-sm text-orange-600 font-medium">{property.vacantUnits}</p>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-center">
                    <div className="flex items-center justify-center">
                      <div className="w-24">
                        <div className="flex items-center">
                          <div className="flex-1 bg-gray-200 rounded-full h-2">
                            <div
                              className={`h-2 rounded-full ${
                                property.occupancyRate >= 90
                                  ? 'bg-green-500'
                                  : property.occupancyRate >= 70
                                  ? 'bg-yellow-500'
                                  : 'bg-red-500'
                              }`}
                              style={{ width: `${property.occupancyRate}%` }}
                            />
                          </div>
                          <span className="ml-2 text-xs font-medium text-gray-700">
                            {property.occupancyRate.toFixed(0)}%
                          </span>
                        </div>
                      </div>
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-right">
                    <p className="text-sm font-semibold text-gray-900">
                      KSh {property.totalRevenue.toLocaleString()}
                    </p>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-right">
                    <p className="text-sm text-gray-600">
                      KSh {property.averageRentPerUnit.toLocaleString(undefined, { maximumFractionDigits: 0 })}
                    </p>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Card>
    </div>
  )
}
