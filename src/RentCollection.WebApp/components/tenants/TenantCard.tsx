'use client'

import { useState } from 'react'
import Link from 'next/link'
import { Tenant } from '@/lib/types'
import { Card, Badge, Button, Modal } from '@/components/common'
import { Eye, Home, Mail, Pencil, Phone, Trash2, User } from 'lucide-react'
import { useDeleteTenant } from '@/lib/hooks'

interface TenantCardProps {
  tenant: Tenant
  onUpdate: () => void
}

export default function TenantCard({ tenant, onUpdate }: TenantCardProps) {
  const [showDeleteModal, setShowDeleteModal] = useState(false)
  const { deleteTenant, loading: deleting } = useDeleteTenant()

  const handleDelete = async () => {
    const success = await deleteTenant(tenant.id)
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
            <div className="flex items-center space-x-3">
              <div className={`w-12 h-12 rounded-full flex items-center justify-center ${
                tenant.isActive ? 'bg-state-success/15' : 'bg-brand-bg'
              }`}>
                <User className={`w-5 h-5 ${tenant.isActive ? 'text-state-success' : 'text-text-muted'}`} />
              </div>
              <div>
                <h3 className="font-semibold text-text-primary">{tenant.fullName}</h3>
                <p className="text-xs text-text-muted">{tenant.idNumber || 'No ID'}</p>
              </div>
            </div>
            <div className="flex flex-col gap-1 items-end">
              <Badge variant={tenant.isActive ? 'success' : 'default'}>
                {tenant.isActive ? 'Active' : 'Inactive'}
              </Badge>
              {tenant.isActive && tenant.paymentStatus && (
                <Badge
                  variant={
                    tenant.paymentStatus === 'Paid' ? 'success' :
                    tenant.paymentStatus === 'Pending' ? 'warning' :
                    tenant.paymentStatus === 'Overdue' ? 'danger' :
                    'default'
                  }
                >
                  {tenant.paymentStatus === 'Paid' && 'Paid'}
                  {tenant.paymentStatus === 'Pending' && 'Pending'}
                  {tenant.paymentStatus === 'Overdue' && `${tenant.daysOverdue}d Overdue`}
                  {tenant.paymentStatus === 'NoPayment' && 'No Payment'}
                </Badge>
              )}
            </div>
          </div>

          <div className="space-y-2 text-sm">
            <div className="flex items-center space-x-2 text-text-secondary">
              <Mail className="text-text-muted w-4 h-4" />
              <span className="truncate">{tenant.email}</span>
            </div>
            <div className="flex items-center space-x-2 text-text-secondary">
              <Phone className="text-text-muted w-4 h-4" />
              <span>{tenant.phoneNumber}</span>
            </div>
          </div>

          <div className="py-3 border-t border-b border-border-muted">
            <div className="flex items-center space-x-2 text-sm">
              <Home className="text-brand-secondary w-4 h-4" />
              <div>
                <p className="font-medium text-text-primary">{tenant.unitNumber}</p>
                <p className="text-xs text-text-muted">{tenant.propertyName}</p>
              </div>
            </div>
          </div>

          <div>
            <div className="flex items-center justify-between mb-1">
              <span className="text-sm text-text-muted">Monthly Rent</span>
              <span className="font-semibold text-text-primary">KSh {tenant.monthlyRent.toLocaleString()}</span>
            </div>
            {tenant.securityDeposit && (
              <div className="flex items-center justify-between">
                <span className="text-xs text-text-muted">Security Deposit</span>
                <span className="text-xs text-text-secondary">KSh {tenant.securityDeposit.toLocaleString()}</span>
              </div>
            )}
            {tenant.lastPaymentDate && tenant.lastPaymentAmount && (
              <div className="flex items-center justify-between mt-2 pt-2 border-t border-border-muted">
                <span className="text-xs text-text-muted">Last Payment</span>
                <span className="text-xs text-text-secondary">
                  KSh {tenant.lastPaymentAmount.toLocaleString()} - {new Date(tenant.lastPaymentDate).toLocaleDateString()}
                </span>
              </div>
            )}
          </div>

          <div className="text-xs text-text-muted">
            <p>Lease: {new Date(tenant.leaseStartDate).toLocaleDateString()}</p>
            {tenant.leaseEndDate && (
              <p>Ends: {new Date(tenant.leaseEndDate).toLocaleDateString()}</p>
            )}
          </div>

          <div className="flex gap-2">
            <Link href={`/tenants/${tenant.id}`} className="flex-1">
              <Button variant="secondary" size="sm" fullWidth>
                <Eye className="mr-2 w-4 h-4" />
                View
              </Button>
            </Link>
            <Link href={`/tenants/${tenant.id}/edit`}>
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
        title="Delete Tenant"
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
          Are you sure you want to delete tenant <strong>{tenant.fullName}</strong>? This action cannot be undone.
        </p>
      </Modal>
    </>
  )
}
