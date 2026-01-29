'use client'

import { useParams, useRouter } from 'next/navigation'
import { useGetProperty, useGetUnitsByProperty } from '@/lib/hooks'
import { Card, Button, Badge, LoadingSpinner } from '@/components/common'
import Link from 'next/link'
import { ArrowLeft, Building2, DoorOpen, Pencil, Plus } from 'lucide-react'

export default function PropertyDetailPage() {
  const params = useParams()
  const router = useRouter()
  const propertyId = Number(params.id)

  const { data: property, loading, error } = useGetProperty(propertyId)
  const { data: units, loading: unitsLoading } = useGetUnitsByProperty(propertyId)

  if (loading) {
    return (
      <div className="flex items-center justify-center h-96">
        <LoadingSpinner size="lg" text="Loading property..." />
      </div>
    )
  }

  if (error || !property) {
    return (
      <div className="text-center py-12">
        <p className="text-red-600">Error loading property</p>
        <Link href="/properties">
          <Button className="mt-4">Back to Properties</Button>
        </Link>
      </div>
    )
  }

  const occupancyRate = property.totalUnits > 0
    ? ((property.occupiedUnits / property.totalUnits) * 100).toFixed(0)
    : 0

  return (
    <div className="max-w-5xl mx-auto space-y-6">
      {/* Header */}
      <div>
        <Link
          href="/properties"
          className="inline-flex items-center text-sm text-gray-600 hover:text-gray-900 mb-4"
        >
          <ArrowLeft className="mr-2" />
          Back to Properties
        </Link>
        <div className="flex items-start justify-between">
          <div>
            <div className="flex items-center gap-3">
              <h1 className="text-3xl font-bold text-gray-900">{property.name}</h1>
              <Badge variant={property.isActive ? 'success' : 'default'}>
                {property.isActive ? 'Active' : 'Inactive'}
              </Badge>
            </div>
            <p className="text-gray-600 mt-1">{property.location}</p>
          </div>
          <Link href={`/properties/${property.id}/edit`}>
            <Button variant="secondary">
              <Pencil className="mr-2" />
              Edit Property
            </Button>
          </Link>
        </div>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        <Card padding="md">
          <div className="flex items-center space-x-3">
            <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
              <Building2 className="text-blue-600" />
            </div>
            <div>
              <p className="text-2xl font-bold text-gray-900">{property.totalUnits}</p>
              <p className="text-sm text-gray-600">Total Units</p>
            </div>
          </div>
        </Card>

        <Card padding="md">
          <div className="flex items-center space-x-3">
            <div className="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center">
              <DoorOpen className="text-green-600" />
            </div>
            <div>
              <p className="text-2xl font-bold text-green-600">{property.occupiedUnits}</p>
              <p className="text-sm text-gray-600">Occupied</p>
            </div>
          </div>
        </Card>

        <Card padding="md">
          <div className="flex items-center space-x-3">
            <div className="w-10 h-10 bg-gray-100 rounded-lg flex items-center justify-center">
              <DoorOpen className="text-gray-600" />
            </div>
            <div>
              <p className="text-2xl font-bold text-gray-600">{property.vacantUnits}</p>
              <p className="text-sm text-gray-600">Vacant</p>
            </div>
          </div>
        </Card>

        <Card padding="md">
          <div>
            <p className="text-sm text-gray-600 mb-2">Occupancy Rate</p>
            <p className="text-2xl font-bold text-primary-600 mb-2">{occupancyRate}%</p>
            <div className="w-full bg-gray-200 rounded-full h-2">
              <div
                className="bg-primary-600 h-2 rounded-full"
                style={{ width: `${occupancyRate}%` }}
              ></div>
            </div>
          </div>
        </Card>
      </div>

      {/* Description */}
      {property.description && (
        <Card padding="md">
          <h2 className="text-lg font-semibold text-gray-900 mb-2">Description</h2>
          <p className="text-gray-700">{property.description}</p>
        </Card>
      )}

      {/* Units Section */}
      <Card padding="md">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-semibold text-gray-900">Units</h2>
          <Link href={`/units/new?propertyId=${property.id}`}>
            <Button variant="primary" size="sm">
              <Plus className="mr-2" />
              Add Unit
            </Button>
          </Link>
        </div>

        {unitsLoading ? (
          <div className="py-8">
            <LoadingSpinner size="md" text="Loading units..." />
          </div>
        ) : units && units.length > 0 ? (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {units.map((unit) => (
              <Link
                key={unit.id}
                href={`/units/${unit.id}`}
                className="p-4 border border-gray-200 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-all"
              >
                <div className="flex items-center justify-between mb-2">
                  <span className="font-semibold text-gray-900">{unit.unitNumber}</span>
                  <Badge variant={unit.isOccupied ? 'success' : 'default'}>
                    {unit.isOccupied ? 'Occupied' : 'Vacant'}
                  </Badge>
                </div>
                <p className="text-sm text-gray-600">KSh {unit.monthlyRent.toLocaleString()}/month</p>
                <p className="text-xs text-gray-500 mt-1">
                  {unit.bedrooms} bed • {unit.bathrooms} bath
                </p>
                {unit.currentTenantName && (
                  <p className="text-xs text-gray-500 mt-1">Tenant: {unit.currentTenantName}</p>
                )}
              </Link>
            ))}
          </div>
        ) : (
          <div className="text-center py-8 text-gray-500">
            <p>No units added yet</p>
            <Link href={`/units/new?propertyId=${property.id}`}>
              <Button variant="primary" size="sm" className="mt-3">
                <Plus className="mr-2" />
                Add First Unit
              </Button>
            </Link>
          </div>
        )}
      </Card>
    </div>
  )
}
