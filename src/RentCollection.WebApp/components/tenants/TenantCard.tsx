'use client'

import { useState } from 'react'
import Link from 'next/link'
import { Tenant } from '@/lib/types'
import { Card, Badge, Button, Modal } from '@/components/common'
import { FaUser, FaEnvelope, FaPhone, FaHome, FaEdit, FaTrash, FaEye, FaSms } from 'react-icons/fa'
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
          {/* Header */}
          <div className="flex items-start justify-between">
            <div className="flex items-center space-x-3">
              <div className={`w-12 h-12 rounded-full flex items-center justify-center ${
                tenant.isActive ? 'bg-green-100' : 'bg-gray-100'
              }`}>
                <FaUser className={tenant.isActive ? 'text-green-600' : 'text-gray-400'} />
              </div>
              <div>
                <h3 className="font-semibold text-gray-900">{tenant.fullName}</h3>
                <p className="text-xs text-gray-500">{tenant.idNumber || 'No ID'}</p>
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
                  {tenant.paymentStatus === 'Paid' && '✓ Paid'}
                  {tenant.paymentStatus === 'Pending' && '⏳ Pending'}
                  {tenant.paymentStatus === 'Overdue' && `⚠ ${tenant.daysOverdue}d Overdue`}
                  {tenant.paymentStatus === 'NoPayment' && 'No Payment'}
                </Badge>
              )}
            </div>
          </div>

          {/* Contact Info */}
          <div className="space-y-2 text-sm">
            <div className="flex items-center space-x-2 text-gray-600">
              <FaEnvelope className="text-gray-400" />
              <span className="truncate">{tenant.email}</span>
            </div>
            <div className="flex items-center space-x-2 text-gray-600">
              <FaPhone className="text-gray-400" />
              <span>{tenant.phoneNumber}</span>
            </div>
          </div>

          {/* Unit Info */}
          <div className="py-3 border-t border-b border-gray-200">
            <div className="flex items-center space-x-2 text-sm">
              <FaHome className="text-primary-600" />
              <div>
                <p className="font-medium text-gray-900">{tenant.unitNumber}</p>
                <p className="text-xs text-gray-500">{tenant.propertyName}</p>
              </div>
            </div>
          </div>

          {/* Rent Info */}
          <div>
            <div className="flex items-center justify-between mb-1">
              <span className="text-sm text-gray-600">Monthly Rent</span>
              <span className="font-semibold text-gray-900">KSh {tenant.monthlyRent.toLocaleString()}</span>
            </div>
            {tenant.securityDeposit && (
              <div className="flex items-center justify-between">
                <span className="text-xs text-gray-500">Security Deposit</span>
                <span className="text-xs text-gray-700">KSh {tenant.securityDeposit.toLocaleString()}</span>
              </div>
            )}
            {tenant.lastPaymentDate && tenant.lastPaymentAmount && (
              <div className="flex items-center justify-between mt-2 pt-2 border-t border-gray-100">
                <span className="text-xs text-gray-500">Last Payment</span>
                <span className="text-xs text-gray-700">
                  KSh {tenant.lastPaymentAmount.toLocaleString()} • {new Date(tenant.lastPaymentDate).toLocaleDateString()}
                </span>
              </div>
            )}
          </div>

          {/* Lease Dates */}
          <div className="text-xs text-gray-500">
            <p>Lease: {new Date(tenant.leaseStartDate).toLocaleDateString()}</p>
            {tenant.leaseEndDate && (
              <p>Ends: {new Date(tenant.leaseEndDate).toLocaleDateString()}</p>
            )}
          </div>

          {/* Actions */}
          <div className="flex gap-2">
            <Link href={`/tenants/${tenant.id}`} className="flex-1">
              <Button variant="secondary" size="sm" fullWidth>
                <FaEye className="mr-2" />
                View
              </Button>
            </Link>
            <Link href={`/tenants/${tenant.id}/edit`}>
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
        title="Delete Tenant"
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
          Are you sure you want to delete tenant <strong>{tenant.fullName}</strong>? This action cannot be undone.
        </p>
      </Modal>
    </>
  )
}
