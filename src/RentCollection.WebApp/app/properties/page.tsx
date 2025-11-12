'use client'

import { useState } from 'react'
import { useGetProperties } from '@/lib/hooks'
import PropertyList from '@/components/properties/PropertyList'
import { Button, SearchBar, LoadingSpinner } from '@/components/common'
import Link from 'next/link'
import { FaPlus } from 'react-icons/fa'

export default function PropertiesPage() {
  const { data: properties, loading, error, refetch } = useGetProperties()
  const [searchTerm, setSearchTerm] = useState('')
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid')

  const filteredProperties = properties.filter((property) =>
    property.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    property.location.toLowerCase().includes(searchTerm.toLowerCase())
  )

  if (loading) {
    return (
      <div className="flex items-center justify-center h-96">
        <LoadingSpinner size="lg" text="Loading properties..." />
      </div>
    )
  }

  if (error) {
    return (
      <div className="text-center py-12">
        <p className="text-red-600">Error loading properties: {error.message}</p>
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
          <h1 className="text-3xl font-bold text-gray-900">Properties</h1>
          <p className="text-gray-600 mt-1">Manage your rental properties</p>
        </div>
        <Link href="/properties/new">
          <Button variant="primary">
            <FaPlus className="mr-2" />
            Add Property
          </Button>
        </Link>
      </div>

      {/* Search and Filters */}
      <div className="flex flex-col sm:flex-row gap-4">
        <div className="flex-1">
          <SearchBar
            value={searchTerm}
            onChange={setSearchTerm}
            placeholder="Search properties by name or location..."
          />
        </div>
        <div className="flex gap-2">
          <button
            onClick={() => setViewMode('grid')}
            className={`px-4 py-2 rounded-lg border ${
              viewMode === 'grid'
                ? 'bg-primary-600 text-white border-primary-600'
                : 'bg-white text-gray-700 border-gray-300 hover:bg-gray-50'
            }`}
          >
            Grid
          </button>
          <button
            onClick={() => setViewMode('list')}
            className={`px-4 py-2 rounded-lg border ${
              viewMode === 'list'
                ? 'bg-primary-600 text-white border-primary-600'
                : 'bg-white text-gray-700 border-gray-300 hover:bg-gray-50'
            }`}
          >
            List
          </button>
        </div>
      </div>

      {/* Properties List/Grid */}
      <PropertyList properties={filteredProperties} viewMode={viewMode} onUpdate={refetch} />

      {/* Stats */}
      {properties.length > 0 && (
        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-center">
            <div>
              <p className="text-2xl font-bold text-gray-900">{properties.length}</p>
              <p className="text-sm text-gray-600">Total Properties</p>
            </div>
            <div>
              <p className="text-2xl font-bold text-gray-900">
                {properties.reduce((sum, p) => sum + p.totalUnits, 0)}
              </p>
              <p className="text-sm text-gray-600">Total Units</p>
            </div>
            <div>
              <p className="text-2xl font-bold text-gray-900">
                {properties.reduce((sum, p) => sum + p.occupiedUnits, 0)}
              </p>
              <p className="text-sm text-gray-600">Occupied Units</p>
            </div>
            <div>
              <p className="text-2xl font-bold text-gray-900">
                {properties.reduce((sum, p) => sum + p.vacantUnits, 0)}
              </p>
              <p className="text-sm text-gray-600">Vacant Units</p>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
