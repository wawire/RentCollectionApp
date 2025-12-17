'use client'

import { useState } from 'react'
import { useParams, useRouter } from 'next/navigation'
import { useGetPayment, useDeletePayment, useSendPaymentReceipt, useDownloadPaymentReceipt } from '@/lib/hooks'
import { Card, Button, Badge, LoadingSpinner, Alert, Modal } from '@/components/common'
import Link from 'next/link'
import { FaArrowLeft, FaDownload, FaTrash, FaEnvelope, FaUser, FaHome, FaMoneyBillWave, FaCalendar, FaCreditCard, FaFileInvoice, FaSms } from 'react-icons/fa'
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
        <p className="text-red-600">Error loading payment</p>
        <Link href="/payments">
          <Button className="mt-4">Back to Payments</Button>
        </Link>
      </div>
    )
  }

  return (
    <div className="max-w-5xl mx-auto space-y-6">
      {/* Header */}
      <div>
        <Link
          href="/payments"
          className="inline-flex items-center text-sm text-gray-600 hover:text-gray-900 mb-4"
        >
          <FaArrowLeft className="mr-2" />
          Back to Payments
        </Link>
        <div className="flex items-start justify-between">
          <div>
            <div className="flex items-center gap-3">
              <h1 className="text-3xl font-bold text-gray-900">Payment Details</h1>
              {getStatusBadge(payment.status)}
            </div>
            <p className="text-gray-600 mt-1">Payment ID: #{payment.id}</p>
          </div>
          <div className="flex gap-2">
            <Button variant="secondary" onClick={handleDownloadReceipt} loading={downloading}>
              <FaDownload className="mr-2" />
              Download PDF
            </Button>
            <Button variant="secondary" onClick={handleSendSmsReceipt} loading={sendingReceipt}>
              <FaSms className="mr-2" />
              Send SMS
            </Button>
            <Button variant="danger" onClick={() => setShowDeleteModal(true)}>
              <FaTrash className="mr-2" />
              Delete
            </Button>
          </div>
        </div>
      </div>

      {/* Alert */}
      {alert && (
        <Alert
          type={alert.type}
          message={alert.message}
          onClose={() => setAlert(null)}
        />
      )}

      {/* Payment Amount */}
      <Card padding="lg">
        <div className="text-center py-6">
          <FaMoneyBillWave className="text-5xl text-green-600 mx-auto mb-4" />
          <p className="text-sm text-gray-600 mb-2">Amount Paid</p>
          <p className="text-5xl font-bold text-gray-900">KSh {payment.amount.toLocaleString()}</p>
        </div>
      </Card>

      {/* Tenant & Unit Information */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {/* Tenant Information */}
        <Card padding="md">
          <h2 className="text-lg font-semibold text-gray-900 mb-4 flex items-center">
            <FaUser className="mr-2 text-primary-600" />
            Tenant Information
          </h2>
          <div className="space-y-3">
            <div>
              <p className="text-sm text-gray-500">Name</p>
              <Link href={`/tenants/${payment.tenantId}`}>
                <p className="text-gray-900 font-medium hover:text-primary-600">{payment.tenantName}</p>
              </Link>
            </div>
          </div>
        </Card>

        {/* Unit Information */}
        <Card padding="md">
          <h2 className="text-lg font-semibold text-gray-900 mb-4 flex items-center">
            <FaHome className="mr-2 text-primary-600" />
            Unit Information
          </h2>
          <div className="space-y-3">
            <div>
              <p className="text-sm text-gray-500">Unit</p>
              <p className="text-gray-900 font-medium">{payment.unitNumber}</p>
            </div>
            <div>
              <p className="text-sm text-gray-500">Property</p>
              <p className="text-gray-900 font-medium">{payment.propertyName}</p>
            </div>
          </div>
        </Card>
      </div>

      {/* Payment Details */}
      <Card padding="md">
        <h2 className="text-lg font-semibold text-gray-900 mb-4 flex items-center">
          <FaFileInvoice className="mr-2 text-primary-600" />
          Payment Details
        </h2>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div className="space-y-4">
            <div className="flex items-center space-x-3">
              <FaCalendar className="text-gray-400" />
              <div>
                <p className="text-sm text-gray-500">Payment Date</p>
                <p className="text-gray-900 font-medium">{new Date(payment.paymentDate).toLocaleDateString()}</p>
              </div>
            </div>
            <div className="flex items-center space-x-3">
              <FaCreditCard className="text-gray-400" />
              <div>
                <p className="text-sm text-gray-500">Payment Method</p>
                <p className="text-gray-900 font-medium">{payment.paymentMethodName}</p>
              </div>
            </div>
            {payment.transactionReference && (
              <div className="flex items-center space-x-3">
                <FaFileInvoice className="text-gray-400" />
                <div>
                  <p className="text-sm text-gray-500">Transaction Reference</p>
                  <p className="text-gray-900 font-medium font-mono">{payment.transactionReference}</p>
                </div>
              </div>
            )}
          </div>

          <div className="space-y-4">
            <div>
              <p className="text-sm text-gray-500 mb-1">Payment Period</p>
              <div className="p-3 bg-gray-50 rounded-lg">
                <p className="text-sm text-gray-700">
                  <span className="font-medium">From:</span> {new Date(payment.periodStart).toLocaleDateString()}
                </p>
                <p className="text-sm text-gray-700">
                  <span className="font-medium">To:</span> {new Date(payment.periodEnd).toLocaleDateString()}
                </p>
              </div>
            </div>
            <div>
              <p className="text-sm text-gray-500 mb-1">Recorded On</p>
              <p className="text-gray-900 text-sm">{new Date(payment.createdAt).toLocaleString()}</p>
            </div>
          </div>
        </div>
      </Card>

      {/* Notes */}
      {payment.notes && (
        <Card padding="md">
          <h2 className="text-lg font-semibold text-gray-900 mb-2">Notes</h2>
          <p className="text-gray-700">{payment.notes}</p>
        </Card>
      )}

      {/* Delete Confirmation Modal */}
      <Modal
        isOpen={showDeleteModal}
        onClose={() => setShowDeleteModal(false)}
        title="Delete Payment"
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
          Are you sure you want to delete this payment of <strong>KSh {payment.amount.toLocaleString()}</strong>? This action cannot be undone.
        </p>
      </Modal>
    </div>
  )
}
