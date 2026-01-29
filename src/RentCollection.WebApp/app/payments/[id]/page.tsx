'use client'

import { useState } from 'react'
import { useParams, useRouter } from 'next/navigation'
import { useGetPayment, useDeletePayment, useSendPaymentReceipt, useDownloadPaymentReceipt } from '@/lib/hooks'
import { Card, Button, Badge, LoadingSpinner, Alert, Modal, PageHeader } from '@/components/common'
import ProtectedRoute from '@/components/auth/ProtectedRoute'
import { UserRole } from '@/lib/types/auth.types'
import Link from 'next/link'
import { ArrowLeft, Banknote, Calendar, CreditCard, Download, FileText, Home, MessageSquare, Trash2, User } from 'lucide-react'
import { PaymentStatus } from '@/lib/types'

export default function PaymentDetailPage() {
  const params = useParams()
  const router = useRouter()
  const paymentId = Number(params.id)

  const { data: payment, loading, error } = useGetPayment(paymentId)
  const { deletePayment, loading: deleting } = useDeletePayment()
  const { sendReceipt, loading: sendingReceipt } = useSendPaymentReceipt()
  const { downloadReceipt, loading: downloading } = useDownloadPaymentReceipt()

  const [showDeleteModal, setShowDeleteModal] = useState(false)
  const [alert, setAlert] = useState<{ type: 'success' | 'error', message: string } | null>(null)

  const handleDownloadReceipt = async () => {
    const success = await downloadReceipt(paymentId)
    if (success) {
      setAlert({ type: 'success', message: 'Receipt downloaded successfully!' })
      setTimeout(() => setAlert(null), 3000)
    } else {
      setAlert({ type: 'error', message: 'Failed to download receipt' })
      setTimeout(() => setAlert(null), 3000)
    }
  }

  const handleSendSmsReceipt = async () => {
    const success = await sendReceipt(paymentId)
    if (success) {
      setAlert({ type: 'success', message: 'Receipt sent via SMS successfully!' })
      setTimeout(() => setAlert(null), 3000)
    } else {
      setAlert({ type: 'error', message: 'Failed to send SMS receipt' })
      setTimeout(() => setAlert(null), 3000)
    }
  }

  const handleDelete = async () => {
    const success = await deletePayment(paymentId)
    if (success) {
      router.push('/payments')
    }
  }

  const getStatusBadge = (status: PaymentStatus) => {
    switch (status) {
      case PaymentStatus.Completed:
        return <Badge variant="success">Completed</Badge>
      case PaymentStatus.Pending:
        return <Badge variant="warning">Pending</Badge>
      case PaymentStatus.Failed:
        return <Badge variant="danger">Failed</Badge>
      case PaymentStatus.Refunded:
        return <Badge variant="default">Refunded</Badge>
      case PaymentStatus.PartiallyPaid:
        return <Badge variant="warning">Partially Paid</Badge>
      default:
        return <Badge variant="default">Unknown</Badge>
    }
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center h-96">
        <LoadingSpinner size="lg" text="Loading payment..." />
      </div>
    )
  }

  if (error || !payment) {
    return (
      <div className="text-center py-12">
        <p className="text-state-error">Error loading payment</p>
        <Link href="/payments">
          <Button className="mt-4">Back to Payments</Button>
        </Link>
      </div>
    )
  }

  return (
    <ProtectedRoute allowedRoles={[UserRole.PlatformAdmin, UserRole.Landlord, UserRole.Manager, UserRole.Accountant]}>
      <div className="max-w-5xl mx-auto space-y-6">
        <Link
          href="/payments"
          className="inline-flex items-center text-sm text-text-muted hover:text-brand-secondary"
        >
          <ArrowLeft className="mr-2 w-4 h-4" />
          Back to Payments
        </Link>

        <PageHeader
          title="Payment Details"
          subtitle={`Payment ID: #${payment.id}`}
          breadcrumbs={[
            { label: 'Dashboard', href: '/dashboard' },
            { label: 'Payments', href: '/payments' },
            { label: 'Payment Details' },
          ]}
          actions={
            <div className="flex gap-2">
              <Button variant="secondary" onClick={handleDownloadReceipt} loading={downloading}>
                <Download className="mr-2 w-4 h-4" />
                Download PDF
              </Button>
              <Button variant="secondary" onClick={handleSendSmsReceipt} loading={sendingReceipt}>
                <MessageSquare className="mr-2 w-4 h-4" />
                Send SMS
              </Button>
              <Button variant="destructive" onClick={() => setShowDeleteModal(true)}>
                <Trash2 className="mr-2 w-4 h-4" />
                Delete
              </Button>
            </div>
          }
        />

        <div className="flex items-center gap-3">
          {getStatusBadge(payment.status)}
        </div>

        {alert && (
          <Alert
            type={alert.type}
            message={alert.message}
            onClose={() => setAlert(null)}
          />
        )}

        <Card padding="lg">
          <div className="text-center py-6">
            <Banknote className="w-12 h-12 text-brand-secondary mx-auto mb-4" />
            <p className="text-sm text-text-muted mb-2">Amount Paid</p>
            <p className="text-5xl font-semibold text-text-primary">KSh {payment.amount.toLocaleString()}</p>
          </div>
        </Card>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <Card padding="md">
            <h2 className="text-lg font-semibold text-text-primary mb-4 flex items-center">
              <User className="mr-2 text-brand-secondary w-5 h-5" />
              Tenant Information
            </h2>
            <div className="space-y-3">
              <div>
                <p className="text-sm text-text-muted">Name</p>
                <Link href={`/tenants/${payment.tenantId}`}>
                  <p className="text-text-primary font-medium hover:text-brand-secondary">{payment.tenantName}</p>
                </Link>
              </div>
            </div>
          </Card>

          <Card padding="md">
            <h2 className="text-lg font-semibold text-text-primary mb-4 flex items-center">
              <Home className="mr-2 text-brand-secondary w-5 h-5" />
              Unit Information
            </h2>
            <div className="space-y-3">
              <div>
                <p className="text-sm text-text-muted">Unit</p>
                <p className="text-text-primary font-medium">{payment.unitNumber}</p>
              </div>
              <div>
                <p className="text-sm text-text-muted">Property</p>
                <p className="text-text-primary font-medium">{payment.propertyName}</p>
              </div>
            </div>
          </Card>
        </div>

        <Card padding="md">
          <h2 className="text-lg font-semibold text-text-primary mb-4 flex items-center">
            <FileText className="mr-2 text-brand-secondary w-5 h-5" />
            Payment Details
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div className="space-y-4">
              <div className="flex items-center space-x-3">
                <Calendar className="text-text-muted w-4 h-4" />
                <div>
                  <p className="text-sm text-text-muted">Payment Date</p>
                  <p className="text-text-primary font-medium">{new Date(payment.paymentDate).toLocaleDateString()}</p>
                </div>
              </div>
              <div className="flex items-center space-x-3">
                <CreditCard className="text-text-muted w-4 h-4" />
                <div>
                  <p className="text-sm text-text-muted">Payment Method</p>
                  <p className="text-text-primary font-medium">{payment.paymentMethodName}</p>
                </div>
              </div>
              {payment.transactionReference && (
                <div className="flex items-center space-x-3">
                  <FileText className="text-text-muted w-4 h-4" />
                  <div>
                    <p className="text-sm text-text-muted">Transaction Reference</p>
                    <p className="text-text-primary font-medium font-mono">{payment.transactionReference}</p>
                  </div>
                </div>
              )}
            </div>

            <div className="space-y-4">
              <div>
                <p className="text-sm text-text-muted mb-1">Payment Period</p>
                <div className="p-3 bg-brand-bg/60 rounded-lg">
                  <p className="text-sm text-text-secondary">
                    <span className="font-medium">From:</span> {new Date(payment.periodStart).toLocaleDateString()}
                  </p>
                  <p className="text-sm text-text-secondary">
                    <span className="font-medium">To:</span> {new Date(payment.periodEnd).toLocaleDateString()}
                  </p>
                </div>
              </div>
              <div>
                <p className="text-sm text-text-muted mb-1">Recorded On</p>
                <p className="text-text-primary text-sm">{new Date(payment.createdAt).toLocaleString()}</p>
              </div>
            </div>
          </div>
        </Card>

        {payment.notes && (
          <Card padding="md">
            <h2 className="text-lg font-semibold text-text-primary mb-2">Notes</h2>
            <p className="text-text-secondary">{payment.notes}</p>
          </Card>
        )}

        <Modal
          isOpen={showDeleteModal}
          onClose={() => setShowDeleteModal(false)}
          title="Delete Payment"
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
            Are you sure you want to delete this payment of <strong>KSh {payment.amount.toLocaleString()}</strong>? This action cannot be undone.
          </p>
        </Modal>
      </div>
    </ProtectedRoute>
  )
}
