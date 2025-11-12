'use client'

import { useState } from 'react'
import { useParams } from 'next/navigation'
import { useGetTenant, useGetTenantPayments, useSendRentReminder } from '@/lib/hooks'
import { Card, Button, Badge, LoadingSpinner, Alert } from '@/components/common'
import Link from 'next/link'
import { FaArrowLeft, FaEdit, FaEnvelope, FaPhone, FaHome, FaSms, FaMoneyBillWave } from 'react-icons/fa'

export default function TenantDetailPage() {
  const params = useParams()
  const tenantId = Number(params.id)

  const { data: tenant, loading, error } = useGetTenant(tenantId)
  const { data: payments, loading: paymentsLoading } = useGetTenantPayments(tenantId)
  const { sendReminder, loading: sendingReminder } = useSendRentReminder()

  const [reminderSent, setReminderSent] = useState(false)

  const handleSendReminder = async () => {
    const response = await sendReminder(tenantId)
    if (response) {
      setReminderSent(true)
      setTimeout(() => setReminderSent(false), 3000)
    }
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center h-96">
        <LoadingSpinner size="lg" text="Loading tenant..." />
      </div>
    )
  }

  if (error || !tenant) {
    return (
      <div className="text-center py-12">
        <p className="text-red-600">Error loading tenant</p>
        <Link href="/tenants">
          <Button className="mt-4">Back to Tenants</Button>
        </Link>
      </div>
    )
  }

  const totalPaid = payments.reduce((sum, p) => sum + p.amount, 0)
  const lastPayment = payments[0]

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <Link
          href="/tenants"
          className="inline-flex items-center text-sm text-gray-600 hover:text-gray-900 mb-4"
        >
          <FaArrowLeft className="mr-2" />
          Back to Tenants
        </Link>
        <div className="flex items-start justify-between">
          <div>
            <div className="flex items-center gap-3">
              <h1 className="text-3xl font-bold text-gray-900">{tenant.fullName}</h1>
              <Badge variant={tenant.isActive ? 'success' : 'default'}>
                {tenant.isActive ? 'Active' : 'Inactive'}
              </Badge>
            </div>
            <p className="text-gray-600 mt-1">{tenant.idNumber || 'No ID Number'}</p>
          </div>
          <div className="flex gap-2">
            <Button variant="secondary" onClick={handleSendReminder} loading={sendingReminder}>
              <FaSms className="mr-2" />
              Send Reminder
            </Button>
            <Link href={`/tenants/${tenant.id}/edit`}>
              <Button variant="secondary">
                <FaEdit className="mr-2" />
                Edit
              </Button>
            </Link>
          </div>
        </div>
      </div>

      {/* Reminder Sent Alert */}
      {reminderSent && (
        <Alert type="success" message="Rent reminder sent successfully!" onClose={() => setReminderSent(false)} />
      )}

      {/* Contact & Unit Info */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {/* Contact Information */}
        <Card padding="md">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Contact Information</h2>
          <div className="space-y-3">
            <div className="flex items-center space-x-3">
              <FaEnvelope className="text-gray-400" />
              <div>
                <p className="text-sm text-gray-500">Email</p>
                <p className="text-gray-900">{tenant.email}</p>
              </div>
            </div>
            <div className="flex items-center space-x-3">
              <FaPhone className="text-gray-400" />
              <div>
                <p className="text-sm text-gray-500">Phone</p>
                <p className="text-gray-900">{tenant.phoneNumber}</p>
              </div>
            </div>
          </div>
        </Card>

        {/* Unit Information */}
        <Card padding="md">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Unit Information</h2>
          <div className="flex items-center space-x-3 mb-4">
            <FaHome className="text-primary-600 text-xl" />
            <div>
              <p className="font-semibold text-gray-900">{tenant.unitNumber}</p>
              <p className="text-sm text-gray-600">{tenant.propertyName}</p>
            </div>
          </div>
          <Link href={`/units/${tenant.unitId}`}>
            <Button variant="secondary" size="sm">
              View Unit Details →
            </Button>
          </Link>
        </Card>
      </div>

      {/* Lease & Payment Info */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Card padding="md">
          <p className="text-sm text-gray-600 mb-1">Monthly Rent</p>
          <p className="text-2xl font-bold text-gray-900">KSh {tenant.monthlyRent.toLocaleString()}</p>
        </Card>

        {tenant.securityDeposit && (
          <Card padding="md">
            <p className="text-sm text-gray-600 mb-1">Security Deposit</p>
            <p className="text-2xl font-bold text-gray-900">KSh {tenant.securityDeposit.toLocaleString()}</p>
          </Card>
        )}

        <Card padding="md">
          <p className="text-sm text-gray-600 mb-1">Lease Period</p>
          <p className="text-sm font-semibold text-gray-900">
            {new Date(tenant.leaseStartDate).toLocaleDateString()}
          </p>
          {tenant.leaseEndDate && (
            <p className="text-xs text-gray-500">
              Until {new Date(tenant.leaseEndDate).toLocaleDateString()}
            </p>
          )}
        </Card>
      </div>

      {/* Notes */}
      {tenant.notes && (
        <Card padding="md">
          <h2 className="text-lg font-semibold text-gray-900 mb-2">Notes</h2>
          <p className="text-gray-700">{tenant.notes}</p>
        </Card>
      )}

      {/* Payment History */}
      <Card padding="md">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-semibold text-gray-900">Payment History</h2>
          <Link href={`/payments/new?tenantId=${tenant.id}`}>
            <Button variant="primary" size="sm">
              <FaMoneyBillWave className="mr-2" />
              Record Payment
            </Button>
          </Link>
        </div>

        {paymentsLoading ? (
          <div className="py-8">
            <LoadingSpinner size="md" text="Loading payments..." />
          </div>
        ) : payments && payments.length > 0 ? (
          <>
            <div className="mb-4 p-4 bg-primary-50 rounded-lg">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <p className="text-sm text-gray-600">Total Paid</p>
                  <p className="text-xl font-bold text-primary-600">KSh {totalPaid.toLocaleString()}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-600">Last Payment</p>
                  <p className="text-xl font-bold text-gray-900">
                    {lastPayment ? new Date(lastPayment.paymentDate).toLocaleDateString() : 'N/A'}
                  </p>
                </div>
              </div>
            </div>

            <div className="space-y-3">
              {payments.slice(0, 10).map((payment) => (
                <div key={payment.id} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                  <div>
                    <p className="font-medium text-gray-900">KSh {payment.amount.toLocaleString()}</p>
                    <p className="text-sm text-gray-500">
                      {new Date(payment.paymentDate).toLocaleDateString()} • {payment.paymentMethodName}
                    </p>
                  </div>
                  <Badge variant={payment.status === 2 ? 'success' : 'warning'}>
                    {payment.statusName}
                  </Badge>
                </div>
              ))}
            </div>

            {payments.length > 10 && (
              <div className="mt-4 text-center">
                <Link href="/payments" className="text-sm text-primary-600 hover:text-primary-700">
                  View all {payments.length} payments →
                </Link>
              </div>
            )}
          </>
        ) : (
          <div className="text-center py-8 text-gray-500">
            <p>No payments recorded yet</p>
            <Link href={`/payments/new?tenantId=${tenant.id}`}>
              <Button variant="primary" size="sm" className="mt-3">
                Record First Payment
              </Button>
            </Link>
          </div>
        )}
      </Card>
    </div>
  )
}
