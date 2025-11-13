'use client'

import { useState } from 'react'
import Link from 'next/link'
import { Unit } from '@/lib/types'
import { Card, Badge, Button, Modal } from '@/components/common'
import { FaDoorOpen, FaBed, FaBath, FaRuler, FaEdit, FaTrash, FaEye, FaUser } from 'react-icons/fa'
import { useDeleteUnit } from '@/lib/hooks'

interface UnitCardProps {
  unit: Unit
  onUpdate: () => void
}

export default function UnitCard({ unit, onUpdate }: UnitCardProps) {
  const [showDeleteModal, setShowDeleteModal] = useState(false)
  const { deleteUnit, loading: deleting } = useDeleteUnit()

  const handleDelete = async () => {
    const success = await deleteUnit(unit.id)
    if (success) {
      setShowDeleteModal(false)
      onUpdate()
    }
  }

  return (
    <>
      <Card padding="md" hover>
        <div className="space-y-4">
          {/* Header */}
          <div className="flex items-start justify-between">
            <div className="flex items-center space-x-2">
              <div className={`w-10 h-10 rounded-lg flex items-center justify-center ${
                unit.isOccupied ? 'bg-green-100' : 'bg-gray-100'
              }`}>
                <FaDoorOpen className={unit.isOccupied ? 'text-green-600' : 'text-gray-400'} />
              </div>
              <div>
                <h3 className="font-semibold text-gray-900">{unit.unitNumber}</h3>
                <p className="text-xs text-gray-500">{unit.propertyName}</p>
              </div>
            </div>
            <Badge variant={unit.isOccupied ? 'success' : 'default'}>
              {unit.isOccupied ? 'Occupied' : 'Vacant'}
            </Badge>
          </div>

          {/* Rent */}
          <div className="py-3 border-t border-b border-gray-200">
            <p className="text-2xl font-bold text-gray-900">
              KSh {unit.monthlyRent.toLocaleString()}
            </p>
            <p className="text-xs text-gray-500">per month</p>
          </div>

          {/* Details */}
          <div className="grid grid-cols-3 gap-2 text-sm">
            <div className="flex items-center space-x-1 text-gray-600">
              <FaBed className="text-gray-400" />
              <span>{unit.bedrooms} bed</span>
            </div>
            <div className="flex items-center space-x-1 text-gray-600">
              <FaBath className="text-gray-400" />
              <span>{unit.bathrooms} bath</span>
            </div>
            {unit.squareFeet && (
              <div className="flex items-center space-x-1 text-gray-600">
                <FaRuler className="text-gray-400" />
                <span>{unit.squareFeet} ft²</span>
              </div>
            )}
          </div>

          {/* Tenant Info */}
          {unit.currentTenantName && (
            <div className="flex items-center space-x-2 text-sm p-2 bg-green-50 rounded-lg">
              <FaUser className="text-green-600" />
              <span className="text-gray-700">{unit.currentTenantName}</span>
            </div>
          )}

          {/* Actions */}
          <div className="flex gap-2">
            <Link href={`/units/${unit.id}`} className="flex-1">
              <Button variant="secondary" size="sm" fullWidth>
                <FaEye className="mr-2" />
                View
              </Button>
            </Link>
            <Link href={`/units/${unit.id}/edit`}>
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
        title="Delete Unit"
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
          Are you sure you want to delete unit <strong>{unit.unitNumber}</strong>? This action cannot be undone.
        </p>
      </Modal>
    </>
  )
}
