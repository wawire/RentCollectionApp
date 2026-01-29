'use client'

import { useGetPayments } from '@/lib/hooks'
import { Card, Badge, LoadingSpinner } from '@/components/common'
import Link from 'next/link'
import { Eye } from 'lucide-react'
import { PaymentStatus } from '@/lib/types'

export default function RecentPayments() {
  const { data: payments, loading } = useGetPayments()

  const recentPayments = payments.slice(0, 5)

  const getStatusVariant = (status: PaymentStatus): 'success' | 'warning' | 'danger' | 'default' => {
    switch (status) {
      case PaymentStatus.Completed:
        return 'success'
      case PaymentStatus.Pending:
        return 'warning'
      case PaymentStatus.Failed:
        return 'danger'
      default:
        return 'default'
    }
  }

  return (
    <Card padding="md">
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-lg font-semibold text-text-primary">Recent Payments</h2>
        <Link href="/payments" className="text-sm text-brand-secondary hover:text-brand-primary font-medium">
          View all
        </Link>
      </div>

      {loading ? (
        <div className="py-8">
          <LoadingSpinner size="md" text="Loading payments..." />
        </div>
      ) : recentPayments.length === 0 ? (
        <div className="text-center py-8 text-text-muted">
          <p>No payments recorded yet</p>
        </div>
      ) : (
        <div className="space-y-3">
          {recentPayments.map((payment) => (
            <div
              key={payment.id}
              className="flex items-center justify-between p-3 bg-brand-bg/60 rounded-lg hover:bg-brand-bg transition-colors"
            >
              <div className="flex-1">
                <p className="font-medium text-text-primary">{payment.tenantName}</p>
                <p className="text-sm text-text-muted">
                  {payment.propertyName} - {payment.unitNumber}
                </p>
                <p className="text-xs text-text-muted mt-1">
                  {new Date(payment.paymentDate).toLocaleDateString()}
                </p>
              </div>
              <div className="flex items-center space-x-3">
                <div className="text-right">
                  <p className="font-semibold text-text-primary">KSh {payment.amount.toLocaleString()}</p>
                  <Badge variant={getStatusVariant(payment.status)} size="sm">
                    {payment.statusName}
                  </Badge>
                </div>
                <Link
                  href={`/payments/${payment.id}`}
                  className="text-text-muted hover:text-brand-secondary transition-colors"
                >
                  <Eye className="w-4 h-4" />
                </Link>
              </div>
            </div>
          ))}
        </div>
      )}
    </Card>
  )
}
