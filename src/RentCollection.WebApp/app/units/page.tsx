'use client'

import { useState } from 'react'
import { useGetUnits, useGetProperties } from '@/lib/hooks'
import UnitList from '@/components/units/UnitList'
import { Button, SearchBar, Select, LoadingSpinner } from '@/components/common'
import Link from 'next/link'
import { FaPlus } from 'react-icons/fa'

export default function UnitsPage() {
  const { data: units, loading, error, refetch } = useGetUnits()
  const { data: properties } = useGetProperties()
  const [searchTerm, setSearchTerm] = useState('')
  const [filterProperty, setFilterProperty] = useState<string>('')

  const filteredUnits = units.filter((unit) => {
    const matchesSearch = unit.unitNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
      unit.propertyName.toLowerCase().includes(searchTerm.toLowerCase())
    const matchesProperty = !filterProperty || unit.propertyId.toString() === filterProperty
    return matchesSearch && matchesProperty
  })

  const vacantUnits = filteredUnits.filter(u => !u.isOccupied).length
  const occupiedUnits = filteredUnits.filter(u => u.isOccupied).length
  const paidUnits = filteredUnits.filter(u => u.paymentStatus === 'Paid').length
  const overdueUnits = filteredUnits.filter(u => u.paymentStatus === 'Overdue').length
  const pendingUnits = filteredUnits.filter(u => u.paymentStatus === 'Pending').length

  if (loading) {
    return (
      <div className="flex items-center justify-center h-96">
        <LoadingSpinner size="lg" text="Loading units..." />
      </div>
    )
  }

  if (error) {
    return (
      <div className="text-center py-12">
        <p className="text-red-600">Error loading units: {error.message}</p>
        <Button onClick={refetch} className="mt-4">
          Retry
        </Button>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-3xl font-serif font-normal text-primary tracking-wide">Units</h1>
          <p className="text-primary/60 mt-2 tracking-wide">Manage rental units across all properties</p>
        </div>
        <Link href="/units/new">
          <Button variant="primary">
            <FaPlus className="mr-2" />
            Add Unit
          </Button>
        </Link>
      </div>

      {/* Search and Filters */}
      <div className="flex flex-col sm:flex-row gap-4">
        <div className="flex-1">
          <SearchBar
            value={searchTerm}
            onChange={setSearchTerm}
            placeholder="Search units by number or property..."
          />
        </div>
        <div className="w-full sm:w-64">
          <Select
            value={filterProperty}
            onChange={(e) => setFilterProperty(e.target.value)}
            options={[
              { value: '', label: 'All Properties' },
              ...properties.map(p => ({ value: p.id.toString(), label: p.name }))
            ]}
          />
        </div>
      </div>

      {/* Units Grid */}
      <UnitList units={filteredUnits} onUpdate={refetch} />

      {/* Stats */}
      {units.length > 0 && (
        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <div className="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-7 gap-4 text-center">
            <div>
              <p className="text-2xl font-bold text-gray-900">{filteredUnits.length}</p>
              <p className="text-sm text-gray-600">Total Units</p>
            </div>
            <div>
              <p className="text-2xl font-bold text-green-600">{occupiedUnits}</p>
              <p className="text-sm text-gray-600">Occupied</p>
            </div>
            <div>
              <p className="text-2xl font-bold text-gray-400">{vacantUnits}</p>
              <p className="text-sm text-gray-600">Vacant</p>
            </div>
            <div>
              <p className="text-2xl font-bold text-primary-600">
                {filteredUnits.length > 0 ? ((occupiedUnits / filteredUnits.length) * 100).toFixed(0) : 0}%
              </p>
              <p className="text-sm text-gray-600">Occupancy</p>
            </div>
            <div className="border-l border-gray-200 pl-4">
              <p className="text-2xl font-bold text-green-600">{paidUnits}</p>
              <p className="text-sm text-gray-600">Paid</p>
            </div>
            <div>
              <p className="text-2xl font-bold text-red-600">{overdueUnits}</p>
              <p className="text-sm text-gray-600">Overdue</p>
            </div>
            <div>
              <p className="text-2xl font-bold text-yellow-600">{pendingUnits}</p>
              <p className="text-sm text-gray-600">Pending</p>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
