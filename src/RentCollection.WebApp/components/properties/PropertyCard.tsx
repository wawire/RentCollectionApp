'use client'

import { useState } from 'react'
import Link from 'next/link'
import { Property } from '@/lib/types'
import { Card, Badge, Button, Modal } from '@/components/common'
import { Building2, Eye, Pencil, Trash2 } from 'lucide-react'
import { useDeleteProperty } from '@/lib/hooks'

interface PropertyCardProps {
  property: Property
  viewMode?: 'grid' | 'list'
  onUpdate: () => void
}

export default function PropertyCard({ property, viewMode = 'grid', onUpdate }: PropertyCardProps) {
  const [showDeleteModal, setShowDeleteModal] = useState(false)
  const { deleteProperty, loading: deleting } = useDeleteProperty()

  const handleDelete = async () => {
    const success = await deleteProperty(property.id)
    if (success) {
      setShowDeleteModal(false)
      onUpdate()
    }
  }

  const occupancyRate = property.totalUnits > 0
    ? ((property.occupiedUnits / property.totalUnits) * 100).toFixed(0)
    : 0

  if (viewMode === 'list') {
    return (
      <>
        <Card padding="md" hover>
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-4 flex-1">
              <div className="w-12 h-12 bg-primary-100 rounded-lg flex items-center justify-center">
                <Building2 className="text-primary-600 text-xl" />
              </div>
              <div className="flex-1">
                <Link href={`/properties/${property.id}`}>
                  <h3 className="text-lg font-semibold text-gray-900 hover:text-primary-600">
                    {property.name}
                  </h3>
                </Link>
                <p className="text-sm text-gray-600">{property.location}</p>
              </div>
            </div>

            <div className="flex items-center gap-6">
              <div className="text-center">
                <p className="text-2xl font-bold text-gray-900">{property.totalUnits}</p>
                <p className="text-xs text-gray-500">Total Units</p>
              </div>
              <div className="text-center">
                <p className="text-2xl font-bold text-green-600">{property.occupiedUnits}</p>
                <p className="text-xs text-gray-500">Occupied</p>
              </div>
              <div className="text-center">
                <p className="text-2xl font-bold text-gray-400">{property.vacantUnits}</p>
                <p className="text-xs text-gray-500">Vacant</p>
              </div>
              <div className="text-center">
                <p className="text-2xl font-bold text-primary-600">{occupancyRate}%</p>
                <p className="text-xs text-gray-500">Occupancy</p>
              </div>
              <div className="w-px h-12 bg-gray-200"></div>
              <div className="text-center">
                <p className="text-2xl font-bold text-green-600">{property.unitsPaid}</p>
                <p className="text-xs text-gray-500">Paid</p>
              </div>
              <div className="text-center">
                <p className="text-2xl font-bold text-red-600">{property.unitsOverdue}</p>
                <p className="text-xs text-gray-500">Overdue</p>
              </div>
              <div className="text-center">
                <p className="text-2xl font-bold text-yellow-600">{property.unitsPending}</p>
                <p className="text-xs text-gray-500">Pending</p>
              </div>

              <div className="flex gap-2">
                <Link href={`/properties/${property.id}`}>
                  <Button variant="secondary" size="sm">
                    <Eye />
                  </Button>
                </Link>
                <Link href={`/properties/${property.id}/edit`}>
                  <Button variant="secondary" size="sm">
                    <Pencil />
                  </Button>
                </Link>
                <Button variant="destructive" size="sm" onClick={() => setShowDeleteModal(true)}>
                  <Trash2 />
                </Button>
              </div>
            </div>
          </div>
        </Card>

        <Modal
          isOpen={showDeleteModal}
          onClose={() => setShowDeleteModal(false)}
          title="Delete Property"
          footer={
            <>
              <Button variant="secondary" onClick={() => setShowDeleteModal(false)} disabled={deleting}>
                Cancel
              </Button>
              <Button variant="destructive" onClick={handleDelete} loading={deleting}>
                Delete
              </Button>
            </>
          }
        >
          <p className="text-gray-700">
            Are you sure you want to delete <strong>{property.name}</strong>? This action cannot be undone.
          </p>
        </Modal>
      </>
    )
  }

  return (
    <>
      <Card padding="md" hover>
        <div className="space-y-4">
          {/* Header */}
          <div className="flex items-start justify-between">
            <div className="w-12 h-12 bg-primary-100 rounded-lg flex items-center justify-center">
              <Building2 className="text-primary-600 text-xl" />
            </div>
            <Badge variant={property.isActive ? 'success' : 'default'}>
              {property.isActive ? 'Active' : 'Inactive'}
            </Badge>
          </div>

          {/* Title */}
          <div>
            <Link href={`/properties/${property.id}`}>
              <h3 className="text-lg font-semibold text-gray-900 hover:text-primary-600">
                {property.name}
              </h3>
            </Link>
            <p className="text-sm text-gray-600 mt-1">{property.location}</p>
          </div>

          {/* Stats */}
          <div className="grid grid-cols-3 gap-3 py-3 border-t border-b border-gray-200">
            <div className="text-center">
              <p className="text-lg font-bold text-gray-900">{property.totalUnits}</p>
              <p className="text-xs text-gray-500">Units</p>
            </div>
            <div className="text-center">
              <p className="text-lg font-bold text-green-600">{property.occupiedUnits}</p>
              <p className="text-xs text-gray-500">Occupied</p>
            </div>
            <div className="text-center">
              <p className="text-lg font-bold text-gray-400">{property.vacantUnits}</p>
              <p className="text-xs text-gray-500">Vacant</p>
            </div>
          </div>

          {/* Payment Status */}
          {property.occupiedUnits > 0 && (
            <div className="space-y-2">
              <div className="flex items-center justify-between text-xs">
                <span className="text-gray-600">Payment Status</span>
                <span className="font-semibold text-gray-900">
                  {property.collectionRate ? property.collectionRate.toFixed(0) : 0}% collected
                </span>
              </div>
              <div className="grid grid-cols-3 gap-2">
                <div className="text-center p-2 bg-green-50 rounded">
                  <p className="text-sm font-bold text-green-600">{property.unitsPaid}</p>
                  <p className="text-[10px] text-gray-600">Paid</p>
                </div>
                <div className="text-center p-2 bg-yellow-50 rounded">
                  <p className="text-sm font-bold text-yellow-600">{property.unitsPending}</p>
                  <p className="text-[10px] text-gray-600">Pending</p>
                </div>
                <div className="text-center p-2 bg-red-50 rounded">
                  <p className="text-sm font-bold text-red-600">{property.unitsOverdue}</p>
                  <p className="text-[10px] text-gray-600">Overdue</p>
                </div>
              </div>
              <div className="text-xs text-center text-gray-600 pt-1">
                KES {property.totalCollectedRent.toLocaleString()} / {property.totalExpectedRent.toLocaleString()}
              </div>
            </div>
          )}

          {/* Occupancy Bar */}
          <div>
            <div className="flex items-center justify-between text-sm mb-1">
              <span className="text-gray-600">Occupancy</span>
              <span className="font-semibold text-gray-900">{occupancyRate}%</span>
            </div>
            <div className="w-full bg-gray-200 rounded-full h-2">
              <div
                className="bg-green-500 h-2 rounded-full transition-all"
                style={{ width: `${occupancyRate}%` }}
              ></div>
            </div>
          </div>

          {/* Actions */}
          <div className="flex gap-2">
            <Link href={`/properties/${property.id}`} className="flex-1">
              <Button variant="secondary" size="sm" fullWidth>
                <Eye className="mr-2" />
                View
              </Button>
            </Link>
            <Link href={`/properties/${property.id}/edit`}>
              <Button variant="secondary" size="sm">
                <Pencil />
              </Button>
            </Link>
            <Button variant="destructive" size="sm" onClick={() => setShowDeleteModal(true)}>
              <Trash2 />
            </Button>
          </div>
        </div>
      </Card>

      <Modal
        isOpen={showDeleteModal}
        onClose={() => setShowDeleteModal(false)}
        title="Delete Property"
        footer={
          <>
            <Button variant="secondary" onClick={() => setShowDeleteModal(false)} disabled={deleting}>
              Cancel
            </Button>
              <Button variant="destructive" onClick={handleDelete} loading={deleting}>
                Delete
              </Button>
          </>
        }
      >
        <p className="text-gray-700">
          Are you sure you want to delete <strong>{property.name}</strong>? This action cannot be undone.
        </p>
      </Modal>
    </>
  )
}
