'use client'

import { useState } from 'react'
import Link from 'next/link'
import { Unit } from '@/lib/types'
import { Card, Badge, Button, Modal } from '@/components/common'
import { Bath, BedDouble, DoorOpen, Eye, Pencil, Ruler, Trash2, User } from 'lucide-react'
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
          <div className="flex items-start justify-between">
            <div className="flex items-center space-x-2">
              <div className={`w-10 h-10 rounded-lg flex items-center justify-center ${
                unit.isOccupied ? 'bg-state-success/15' : 'bg-brand-bg'
              }`}>
                <DoorOpen className={`w-5 h-5 ${unit.isOccupied ? 'text-state-success' : 'text-text-muted'}`} />
              </div>
              <div>
                <h3 className="font-semibold text-text-primary">{unit.unitNumber}</h3>
                <p className="text-xs text-text-muted">{unit.propertyName}</p>
              </div>
            </div>
            <div className="flex flex-col gap-1 items-end">
              <Badge variant={unit.isOccupied ? 'success' : 'default'}>
                {unit.isOccupied ? 'Occupied' : 'Vacant'}
              </Badge>
              {unit.isOccupied && unit.paymentStatus && (
                <Badge
                  variant={
                    unit.paymentStatus === 'Paid' ? 'success' :
                    unit.paymentStatus === 'Pending' ? 'warning' :
                    unit.paymentStatus === 'Overdue' ? 'danger' :
                    'default'
                  }
                >
                  {unit.paymentStatus === 'Paid' && 'Paid'}
                  {unit.paymentStatus === 'Pending' && 'Pending'}
                  {unit.paymentStatus === 'Overdue' && `${unit.daysOverdue}d Overdue`}
                  {unit.paymentStatus === 'NoTenant' && 'No Tenant'}
                </Badge>
              )}
            </div>
          </div>

          <div className="py-3 border-t border-b border-border-muted">
            <p className="text-2xl font-semibold text-text-primary">
              KSh {unit.monthlyRent.toLocaleString()}
            </p>
            <p className="text-xs text-text-muted">per month</p>
          </div>

          <div className="grid grid-cols-3 gap-2 text-sm">
            <div className="flex items-center space-x-1 text-text-secondary">
              <BedDouble className="text-text-muted w-4 h-4" />
              <span>{unit.bedrooms} bed</span>
            </div>
            <div className="flex items-center space-x-1 text-text-secondary">
              <Bath className="text-text-muted w-4 h-4" />
              <span>{unit.bathrooms} bath</span>
            </div>
            {unit.squareFeet && (
              <div className="flex items-center space-x-1 text-text-secondary">
                <Ruler className="text-text-muted w-4 h-4" />
                <span>{unit.squareFeet} ft2</span>
              </div>
            )}
          </div>

          {unit.currentTenantName && (
            <div className="space-y-2">
              <div className="flex items-center space-x-2 text-sm p-2 bg-state-success/10 rounded-lg">
                <User className="text-state-success w-4 h-4" />
                <span className="text-text-secondary">{unit.currentTenantName}</span>
              </div>
              {unit.lastPaymentDate && unit.lastPaymentAmount && (
                <div className="text-xs text-text-muted px-2">
                  Last payment: <span className="font-semibold">KSh {unit.lastPaymentAmount.toLocaleString()}</span>
                  <span className="text-text-muted"> - {new Date(unit.lastPaymentDate).toLocaleDateString()}</span>
                </div>
              )}
            </div>
          )}

          <div className="flex gap-2">
            <Link href={`/units/${unit.id}`} className="flex-1">
              <Button variant="secondary" size="sm" fullWidth>
                <Eye className="mr-2 w-4 h-4" />
                View
              </Button>
            </Link>
            <Link href={`/units/${unit.id}/edit`}>
              <Button variant="secondary" size="sm">
                <Pencil className="w-4 h-4" />
              </Button>
            </Link>
            <Button variant="destructive" size="sm" onClick={() => setShowDeleteModal(true)}>
              <Trash2 className="w-4 h-4" />
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
            <Button variant="destructive" onClick={handleDelete} loading={deleting}>
              Delete
            </Button>
          </>
        }
      >
        <p className="text-text-secondary">
          Are you sure you want to delete unit <strong>{unit.unitNumber}</strong>? This action cannot be undone.
        </p>
      </Modal>
    </>
  )
}
