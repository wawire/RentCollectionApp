'use client'

import { useState } from 'react'
import Link from 'next/link'
import { Property } from '@/lib/types'
import { Card, Badge, Button, Modal } from '@/components/common'
import { FaBuilding, FaDoorOpen, FaEdit, FaTrash, FaEye } from 'react-icons/fa'
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
                <FaBuilding className="text-primary-600 text-xl" />
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

              <div className="flex gap-2">
                <Link href={`/properties/${property.id}`}>
                  <Button variant="secondary" size="sm">
                    <FaEye />
                  </Button>
                </Link>
                <Link href={`/properties/${property.id}/edit`}>
                  <Button variant="secondary" size="sm">
                    <FaEdit />
                  </Button>
                </Link>
                <Button variant="danger" size="sm" onClick={() => setShowDeleteModal(true)}>
                  <FaTrash />
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
              <Button variant="danger" onClick={handleDelete} loading={deleting}>
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
              <FaBuilding className="text-primary-600 text-xl" />
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
                <FaEye className="mr-2" />
                View
              </Button>
            </Link>
            <Link href={`/properties/${property.id}/edit`}>
              <Button variant="secondary" size="sm">
                <FaEdit />
              </Button>
            </Link>
            <Button variant="danger" size="sm" onClick={() => setShowDeleteModal(true)}>
              <FaTrash />
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
            <Button variant="danger" onClick={handleDelete} loading={deleting}>
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
