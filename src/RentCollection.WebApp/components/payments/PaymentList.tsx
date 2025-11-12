'use client'

import { Payment, PaymentStatus } from '@/lib/types'
import { Table, Badge } from '@/components/common'
import { FaEye, FaDownload, FaEdit, FaTrash } from 'react-icons/fa'
import Link from 'next/link'

interface PaymentListProps {
  payments: Payment[]
  loading?: boolean
  onDownloadReceipt?: (paymentId: number) => void
  onDelete?: (paymentId: number) => void
}

export default function PaymentList({ payments, loading, onDownloadReceipt, onDelete }: PaymentListProps) {
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

  const columns = [
    {
      key: 'paymentDate',
      header: 'Date',
      sortable: true,
      render: (payment: Payment) => new Date(payment.paymentDate).toLocaleDateString(),
    },
    {
      key: 'tenantName',
      header: 'Tenant',
      sortable: true,
      render: (payment: Payment) => (
        <div>
          <p className="font-medium text-gray-900">{payment.tenantName}</p>
          <p className="text-xs text-gray-500">{payment.unitNumber} - {payment.propertyName}</p>
        </div>
      ),
    },
    {
      key: 'amount',
      header: 'Amount',
      sortable: true,
      render: (payment: Payment) => (
        <span className="font-semibold text-gray-900">KSh {payment.amount.toLocaleString()}</span>
      ),
    },
    {
      key: 'paymentMethodName',
      header: 'Method',
      sortable: true,
    },
    {
      key: 'status',
      header: 'Status',
      sortable: true,
      render: (payment: Payment) => getStatusBadge(payment.status),
    },
    {
      key: 'periodStart',
      header: 'Period',
      sortable: false,
      render: (payment: Payment) => (
        <span className="text-sm text-gray-600">
          {new Date(payment.periodStart).toLocaleDateString()} - {new Date(payment.periodEnd).toLocaleDateString()}
        </span>
      ),
    },
    {
      key: 'actions',
      header: 'Actions',
      sortable: false,
      render: (payment: Payment) => (
        <div className="flex gap-2">
          <Link href={`/payments/${payment.id}`}>
            <button
              className="p-2 text-gray-600 hover:text-primary-600 hover:bg-gray-100 rounded transition-colors"
              title="View Details"
            >
              <FaEye />
            </button>
          </Link>
          {onDownloadReceipt && (
            <button
              onClick={() => onDownloadReceipt(payment.id)}
              className="p-2 text-gray-600 hover:text-blue-600 hover:bg-blue-50 rounded transition-colors"
              title="Download Receipt"
            >
              <FaDownload />
            </button>
          )}
          {onDelete && (
            <button
              onClick={() => onDelete(payment.id)}
              className="p-2 text-gray-600 hover:text-red-600 hover:bg-red-50 rounded transition-colors"
              title="Delete"
            >
              <FaTrash />
            </button>
          )}
        </div>
      ),
    },
  ]

  return (
    <Table<Payment>
      data={payments}
      columns={columns}
      loading={loading}
      emptyMessage="No payments found"
    />
  )
}
