'use client'

import { Payment, PaymentStatus } from '@/lib/types'
import { Table, Badge } from '@/components/common'
import { Download, Eye, Trash2 } from 'lucide-react'
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
          <p className="font-medium text-text-primary">{payment.tenantName}</p>
          <p className="text-xs text-text-muted">{payment.unitNumber} - {payment.propertyName}</p>
        </div>
      ),
    },
    {
      key: 'amount',
      header: 'Amount',
      sortable: true,
      render: (payment: Payment) => (
        <span className="font-semibold text-text-primary">KSh {payment.amount.toLocaleString()}</span>
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
        <span className="text-sm text-text-secondary">
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
              className="p-2 text-text-secondary hover:text-brand-secondary hover:bg-brand-bg/60 rounded transition-colors"
              title="View Details"
            >
              <Eye className="w-4 h-4" />
            </button>
          </Link>
          {onDownloadReceipt && (
            <button
              onClick={() => onDownloadReceipt(payment.id)}
              className="p-2 text-text-secondary hover:text-brand-secondary hover:bg-brand-bg/60 rounded transition-colors"
              title="Download Receipt"
            >
              <Download className="w-4 h-4" />
            </button>
          )}
          {onDelete && (
            <button
              onClick={() => onDelete(payment.id)}
              className="p-2 text-text-secondary hover:text-state-error hover:bg-state-error/10 rounded transition-colors"
              title="Delete"
            >
              <Trash2 className="w-4 h-4" />
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
