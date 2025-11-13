'use client'

import { useParams, useRouter } from 'next/navigation'
import { useGetUnit } from '@/lib/hooks'
import { Card, Button, Badge, LoadingSpinner } from '@/components/common'
import Link from 'next/link'
import { FaArrowLeft, FaEdit, FaBed, FaBath, FaRuler, FaBuilding, FaDoorOpen, FaUser } from 'react-icons/fa'

export default function UnitDetailPage() {
  const params = useParams()
  const router = useRouter()
  const unitId = Number(params.id)

  const { data: unit, loading, error } = useGetUnit(unitId)

  if (loading) {
    return (
      <div className="flex items-center justify-center h-96">
        <LoadingSpinner size="lg" text="Loading unit..." />
      </div>
    )
  }

  if (error || !unit) {
    return (
      <div className="text-center py-12">
        <p className="text-red-600">Error loading unit</p>
        <Link href="/units">
          <Button className="mt-4">Back to Units</Button>
        </Link>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <Link
          href="/units"
          className="inline-flex items-center text-sm text-gray-600 hover:text-gray-900 mb-4"
        >
          <FaArrowLeft className="mr-2" />
          Back to Units
        </Link>
        <div className="flex items-start justify-between">
          <div>
            <div className="flex items-center gap-3">
              <h1 className="text-3xl font-bold text-gray-900">{unit.unitNumber}</h1>
              <Badge variant={unit.isOccupied ? 'success' : 'default'}>
                {unit.isOccupied ? 'Occupied' : 'Vacant'}
              </Badge>
              <Badge variant={unit.isActive ? 'success' : 'default'}>
                {unit.isActive ? 'Active' : 'Inactive'}
              </Badge>
            </div>
            <p className="text-gray-600 mt-1">{unit.propertyName}</p>
          </div>
          <Link href={`/units/${unit.id}/edit`}>
            <Button variant="secondary">
              <FaEdit className="mr-2" />
              Edit Unit
            </Button>
          </Link>
        </div>
      </div>

      {/* Details Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {/* Basic Info */}
        <Card padding="md">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Basic Information</h2>
          <div className="space-y-3">
            <div className="flex items-center justify-between">
              <span className="text-gray-600">Monthly Rent</span>
              <span className="text-xl font-bold text-gray-900">KSh {unit.monthlyRent.toLocaleString()}</span>
            </div>
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-2 text-gray-600">
                <FaBed />
                <span>Bedrooms</span>
              </div>
              <span className="font-semibold text-gray-900">{unit.bedrooms}</span>
            </div>
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-2 text-gray-600">
                <FaBath />
                <span>Bathrooms</span>
              </div>
              <span className="font-semibold text-gray-900">{unit.bathrooms}</span>
            </div>
            {unit.squareFeet && (
              <div className="flex items-center justify-between">
                <div className="flex items-center space-x-2 text-gray-600">
                  <FaRuler />
                  <span>Square Feet</span>
                </div>
                <span className="font-semibold text-gray-900">{unit.squareFeet}</span>
              </div>
            )}
          </div>
        </Card>

        {/* Property Info */}
        <Card padding="md">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Property Details</h2>
          <div className="space-y-3">
            <div className="flex items-center space-x-3">
              <div className="w-10 h-10 bg-primary-100 rounded-lg flex items-center justify-center">
                <FaBuilding className="text-primary-600" />
              </div>
              <div>
                <p className="font-semibold text-gray-900">{unit.propertyName}</p>
                <Link href={`/properties/${unit.propertyId}`} className="text-sm text-primary-600 hover:text-primary-700">
                  View Property →
                </Link>
              </div>
            </div>
          </div>
        </Card>
      </div>

      {/* Tenant Info */}
      {unit.isOccupied && unit.currentTenantName && (
        <Card padding="md">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Current Tenant</h2>
          <div className="flex items-center space-x-3 p-4 bg-green-50 rounded-lg">
            <div className="w-12 h-12 bg-green-100 rounded-full flex items-center justify-center">
              <FaUser className="text-green-600 text-xl" />
            </div>
            <div>
              <p className="font-semibold text-gray-900">{unit.currentTenantName}</p>
              <p className="text-sm text-gray-600">Tenant Details</p>
            </div>
          </div>
        </Card>
      )}

      {/* Description */}
      {unit.description && (
        <Card padding="md">
          <h2 className="text-lg font-semibold text-gray-900 mb-2">Description</h2>
          <p className="text-gray-700">{unit.description}</p>
        </Card>
      )}

      {/* Quick Actions */}
      <Card padding="md">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Quick Actions</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
          {!unit.isOccupied && (
            <Link href={`/tenants/new?unitId=${unit.id}`}>
              <Button variant="primary" fullWidth>
                Add Tenant
              </Button>
            </Link>
          )}
          <Link href={`/properties/${unit.propertyId}`}>
            <Button variant="secondary" fullWidth>
              View Property
            </Button>
          </Link>
          <Link href={`/units/${unit.id}/edit`}>
            <Button variant="secondary" fullWidth>
              Edit Unit
            </Button>
          </Link>
        </div>
      </Card>
    </div>
  )
}
