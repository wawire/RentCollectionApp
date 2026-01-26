'use client'

import { useEffect, useState } from 'react'
import Link from 'next/link'
import ProtectedRoute from '@/components/auth/ProtectedRoute'
import { LoadingSpinner } from '@/components/common'
import { invoiceService } from '@/lib/services/invoiceService'
import { Invoice, InvoiceStatus } from '@/lib/types/invoice.types'
import { useAuth } from '@/contexts/AuthContext'
import { ArrowLeft, Banknote, FileText } from 'lucide-react'

export default function TenantInvoicesPage() {
  const { user } = useAuth()
  const [invoices, setInvoices] = useState<Invoice[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const loadInvoices = async () => {
      try {
        setLoading(true)
        const data = await invoiceService.getMine()
        setInvoices(data)
      } catch (err: any) {
        setError(err.message || 'Failed to load invoices')
      } finally {
        setLoading(false)
      }
    }

    if (user) {
      loadInvoices()
    }
  }, [user])

  if (loading) {
    return (
      <ProtectedRoute allowedRoles={['Tenant']}>
        <div className="flex items-center justify-center h-96">
          <LoadingSpinner size="lg" text="Loading invoices..." />
        </div>
      </ProtectedRoute>
    )
  }

  if (error) {
    return (
      <ProtectedRoute allowedRoles={['Tenant']}>
        <div className="text-center py-12">
          <p className="text-red-600">{error}</p>
        </div>
      </ProtectedRoute>
    )
  }

  return (
    <ProtectedRoute allowedRoles={['Tenant']}>
      <div className="space-y-6">
        <div>
          <Link
            href="/tenant-portal"
            className="inline-flex items-center gap-2 text-primary/60 hover:text-primary transition-colors mb-4"
          >
            <ArrowLeft className="w-4 h-4" />
            Back to Dashboard
          </Link>
          <h1 className="text-3xl font-serif font-normal text-primary tracking-wide">My Invoices</h1>
          <p className="text-primary/60 mt-2 tracking-wide">Track monthly rent invoices and balances</p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {invoices.map((invoice) => (
            <Link
              key={invoice.id}
              href={`/tenant-portal/invoices/${invoice.id}`}
              className="bg-white rounded-lg shadow-sm border border-primary/10 p-5 hover:border-primary/30 transition-colors"
            >
              <div className="flex items-start justify-between">
                <div>
                  <p className="text-sm text-primary/60">Period</p>
                  <p className="text-lg font-medium text-primary">
                    {new Date(invoice.periodStart).toLocaleDateString()} -{' '}
                    {new Date(invoice.periodEnd).toLocaleDateString()}
                  </p>
                </div>
                <span className="inline-flex items-center gap-2 text-sm text-primary/60">
                  <FileText className="w-4 h-4" />
                  {InvoiceStatus[invoice.status]}
                </span>
              </div>
              <div className="mt-4 flex items-center justify-between">
                <div>
                  <p className="text-sm text-primary/60">Balance</p>
                  <p className="text-xl font-semibold text-primary">
                    KSh {invoice.balance.toLocaleString()}
                  </p>
                </div>
                <div className="text-right">
                  <p className="text-sm text-primary/60">Due</p>
                  <p className="text-sm font-medium text-primary">
                    {new Date(invoice.dueDate).toLocaleDateString()}
                  </p>
                </div>
              </div>
              <div className="mt-4 inline-flex items-center gap-2 text-primary">
                <Banknote className="w-4 h-4" />
                View & Pay
              </div>
            </Link>
          ))}
          {invoices.length === 0 && (
            <div className="bg-white rounded-lg shadow-sm border border-primary/10 p-6 text-center text-primary/60">
              No invoices available yet.
            </div>
          )}
        </div>
      </div>
    </ProtectedRoute>
  )
}
