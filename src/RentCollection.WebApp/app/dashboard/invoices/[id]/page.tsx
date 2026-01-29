'use client'

import { useEffect, useState } from 'react'
import { useParams, useRouter } from 'next/navigation'
import ProtectedRoute from '@/components/auth/ProtectedRoute'
import { Alert, Badge, Button, Card, EmptyState, PageHeader, Skeleton, SkeletonTable, Table } from '@/components/common'
import { invoiceService } from '@/lib/services/invoiceService'
import { Invoice, InvoiceStatus } from '@/lib/types/invoice.types'

export default function InvoiceDetailPage() {
  const params = useParams()
  const router = useRouter()
  const [invoice, setInvoice] = useState<Invoice | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const invoiceId = Number(params?.id)

  useEffect(() => {
    const loadInvoice = async () => {
      try {
        setLoading(true)
        const data = await invoiceService.getById(invoiceId)
        setInvoice(data)
      } catch (err: any) {
        setError(err.message || 'Failed to load invoice')
      } finally {
        setLoading(false)
      }
    }

    if (!Number.isNaN(invoiceId)) {
      loadInvoice()
    }
  }, [invoiceId])

  const getStatusVariant = (status: InvoiceStatus): 'success' | 'warning' | 'danger' | 'info' => {
    switch (status) {
      case InvoiceStatus.Paid:
        return 'success'
      case InvoiceStatus.Overdue:
        return 'danger'
      case InvoiceStatus.PartiallyPaid:
      case InvoiceStatus.Issued:
        return 'warning'
      default:
        return 'info'
    }
  }

  return (
    <ProtectedRoute allowedRoles={['PlatformAdmin', 'Landlord', 'Manager', 'Caretaker', 'Accountant']}>
      <div className="space-y-6">
        <PageHeader
          title="Invoice Details"
          subtitle={`Invoice #${invoiceId}`}
          breadcrumbs={[
            { label: 'Dashboard', href: '/dashboard' },
            { label: 'Invoices', href: '/dashboard/invoices' },
            { label: `Invoice #${invoiceId}` },
          ]}
          actions={
            <Button variant="secondary" onClick={() => router.back()}>
              Back
            </Button>
          }
        />

        {loading && (
          <div className="space-y-6">
            <Card>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                {Array.from({ length: 8 }).map((_, index) => (
                  <div key={`invoice-skeleton-${index}`} className="space-y-2">
                    <Skeleton height="14px" width="40%" />
                    <Skeleton height="20px" width="70%" />
                  </div>
                ))}
              </div>
            </Card>
            <Card>
              <SkeletonTable rows={4} columns={4} />
            </Card>
          </div>
        )}

        {!loading && error && <Alert type="error" message={error} />}

        {!loading && !error && !invoice && (
          <EmptyState
            title="Invoice not found"
            description="This invoice may have been deleted or you may not have access."
          />
        )}

        {!loading && invoice && (
          <div className="space-y-6">
            <Card padding="md">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                  <p className="text-sm text-text-muted">Tenant</p>
                  <p className="text-lg font-semibold text-text-primary">{invoice.tenantName}</p>
                </div>
                <div>
                  <p className="text-sm text-text-muted">Property / Unit</p>
                  <p className="text-lg font-semibold text-text-primary">
                    {invoice.propertyName} - {invoice.unitNumber}
                  </p>
                </div>
                <div>
                  <p className="text-sm text-text-muted">Period</p>
                  <p className="text-lg font-semibold text-text-primary">
                    {new Date(invoice.periodStart).toLocaleDateString()} -{' '}
                    {new Date(invoice.periodEnd).toLocaleDateString()}
                  </p>
                </div>
                <div>
                  <p className="text-sm text-text-muted">Due Date</p>
                  <p className="text-lg font-semibold text-text-primary">
                    {new Date(invoice.dueDate).toLocaleDateString()}
                  </p>
                </div>
                <div>
                  <p className="text-sm text-text-muted">Amount</p>
                  <p className="text-lg font-semibold text-text-primary">KSh {invoice.amount.toLocaleString()}</p>
                </div>
                <div>
                  <p className="text-sm text-text-muted">Opening Balance</p>
                  <p className="text-lg font-semibold text-text-primary">KSh {invoice.openingBalance.toLocaleString()}</p>
                </div>
                <div>
                  <p className="text-sm text-text-muted">Balance</p>
                  <p className="text-lg font-semibold text-text-primary">KSh {invoice.balance.toLocaleString()}</p>
                </div>
                <div>
                  <p className="text-sm text-text-muted">Status</p>
                  <Badge variant={getStatusVariant(invoice.status)} size="sm">
                    {InvoiceStatus[invoice.status]}
                  </Badge>
                </div>
              </div>
            </Card>

            <Card padding="md">
              <h2 className="text-lg font-semibold text-text-primary mb-4">Invoice Line Items</h2>
              {invoice.lineItems && invoice.lineItems.length > 0 ? (
                <Table
                  data={invoice.lineItems}
                  columns={[
                    {
                      key: 'description',
                      header: 'Item',
                      render: (item) => (
                        <div>
                          <div className="text-text-primary">{item.description}</div>
                          {item.utilityName && (
                            <div className="text-xs text-text-muted">{item.utilityName}</div>
                          )}
                        </div>
                      ),
                    },
                    {
                      key: 'quantity',
                      header: 'Quantity',
                      render: (item) => (
                        <span className="text-text-secondary">
                          {item.quantity.toLocaleString()} {item.unitOfMeasure || ''}
                        </span>
                      ),
                    },
                    {
                      key: 'rate',
                      header: 'Rate',
                      render: (item) => `KSh ${item.rate.toLocaleString()}`,
                    },
                    {
                      key: 'amount',
                      header: 'Amount',
                      render: (item) => (
                        <span className="font-semibold text-text-primary">KSh {item.amount.toLocaleString()}</span>
                      ),
                    },
                  ]}
                />
              ) : (
                <EmptyState
                  title="No line items"
                  description="This invoice does not include any line items."
                />
              )}
            </Card>
          </div>
        )}
      </div>
    </ProtectedRoute>
  )
}
