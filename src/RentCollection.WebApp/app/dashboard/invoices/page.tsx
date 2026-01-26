'use client'

import { useEffect, useMemo, useState } from 'react'
import Link from 'next/link'
import ProtectedRoute from '@/components/auth/ProtectedRoute'
import { Card, LoadingSpinner, Button, Select, Alert, PageHeader } from '@/components/common'
import { invoiceService } from '@/lib/services/invoiceService'
import { useGetProperties } from '@/lib/hooks/useProperties'
import { Invoice, InvoiceStatus } from '@/lib/types/invoice.types'
import { useAuth } from '@/contexts/AuthContext'
import { UserRole } from '@/lib/types/auth.types'
import { Calendar, FileText, Filter, Plus } from 'lucide-react'

export default function InvoicesPage() {
  const { data: properties } = useGetProperties()
  const { user } = useAuth()
  const [invoices, setInvoices] = useState<Invoice[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [alert, setAlert] = useState<{ type: 'success' | 'error'; message: string } | null>(null)
  const [propertyId, setPropertyId] = useState<number | 'all'>('all')
  const [status, setStatus] = useState<number | 'all'>('all')
  const [startDate, setStartDate] = useState('')
  const [endDate, setEndDate] = useState('')

  const canGenerate = user?.role === UserRole.PlatformAdmin || user?.role === UserRole.Landlord

  const loadInvoices = async () => {
    try {
      setLoading(true)
      setError(null)
      const data = await invoiceService.getAll(
        propertyId === 'all' ? undefined : propertyId,
        startDate || undefined,
        endDate || undefined
      )
      setInvoices(data)
    } catch (err: any) {
      setError(err.message || 'Failed to load invoices')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadInvoices()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const filteredInvoices = useMemo(() => {
    return invoices.filter((invoice) => {
      if (status !== 'all' && invoice.status !== status) {
        return false
      }
      return true
    })
  }, [invoices, status])

  const handleGenerate = async () => {
    try {
      const now = new Date()
      await invoiceService.generateMonthly(now.getFullYear(), now.getMonth() + 1)
      setAlert({ type: 'success', message: 'Invoice generation started' })
      await loadInvoices()
    } catch (err: any) {
      setAlert({ type: 'error', message: err.message || 'Failed to generate invoices' })
    }
  }

  const statusOptions = [
    { value: 'all', label: 'All Statuses' },
    { value: InvoiceStatus.Draft, label: 'Draft' },
    { value: InvoiceStatus.Issued, label: 'Issued' },
    { value: InvoiceStatus.PartiallyPaid, label: 'Partially Paid' },
    { value: InvoiceStatus.Paid, label: 'Paid' },
    { value: InvoiceStatus.Overdue, label: 'Overdue' },
    { value: InvoiceStatus.Void, label: 'Void' },
  ]

  const propertyOptions = [
    { value: 'all', label: 'All Properties' },
    ...properties.map((p) => ({ value: p.id, label: p.name })),
  ]

  return (
    <ProtectedRoute allowedRoles={['PlatformAdmin', 'Landlord', 'Manager', 'Caretaker', 'Accountant']}>
      <div className="space-y-6">
        <PageHeader
          title="Invoices & Statements"
          subtitle="Track monthly rent invoices, balances, and payment status."
          breadcrumbs={[
            { label: 'Dashboard', href: '/dashboard' },
            { label: 'Invoices & Statements' },
          ]}
          actions={
            canGenerate ? (
              <Button onClick={handleGenerate}>
                <Plus className="mr-2 w-4 h-4" />
                Generate This Month
              </Button>
            ) : null
          }
        />

        {alert && <Alert type={alert.type} message={alert.message} onClose={() => setAlert(null)} />}

        <Card padding="md">
          <div className="flex items-center gap-2 mb-4">
            <Filter className="text-text-muted w-4 h-4" />
            <h2 className="text-lg font-semibold text-text-primary">Filters</h2>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <Select
              label="Property"
              value={propertyId}
              onChange={(event) => {
                const value = event.target.value
                setPropertyId(value === 'all' ? 'all' : Number(value))
              }}
              options={propertyOptions}
            />
            <Select
              label="Status"
              value={status}
              onChange={(event) => {
                const value = event.target.value
                setStatus(value === 'all' ? 'all' : Number(value))
              }}
              options={statusOptions}
            />
            <div>
              <label className="block text-sm font-medium text-text-secondary mb-1">Start Date</label>
              <input
                type="date"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
                className="w-full rounded-lg border border-border-muted px-3 py-2 bg-surface text-text-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-secondary/60 focus-visible:ring-offset-2 focus-visible:ring-offset-brand-bg"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-text-secondary mb-1">End Date</label>
              <input
                type="date"
                value={endDate}
                onChange={(e) => setEndDate(e.target.value)}
                className="w-full rounded-lg border border-border-muted px-3 py-2 bg-surface text-text-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-secondary/60 focus-visible:ring-offset-2 focus-visible:ring-offset-brand-bg"
              />
            </div>
          </div>
          <div className="mt-4">
            <Button variant="secondary" onClick={loadInvoices}>
              <Calendar className="mr-2 w-4 h-4" />
              Apply Filters
            </Button>
          </div>
        </Card>

        {loading ? (
          <div className="flex items-center justify-center h-96">
            <LoadingSpinner size="lg" text="Loading invoices..." />
          </div>
        ) : error ? (
          <div className="text-center py-12">
            <p className="text-state-error">{error}</p>
            <Button onClick={loadInvoices} className="mt-4">
              Try Again
            </Button>
          </div>
        ) : (
          <Card padding="md">
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="text-left text-text-muted border-b border-border-muted">
                    <th className="py-2">Tenant</th>
                    <th className="py-2">Property / Unit</th>
                    <th className="py-2">Period</th>
                    <th className="py-2">Due</th>
                    <th className="py-2">Amount</th>
                    <th className="py-2">Balance</th>
                    <th className="py-2">Status</th>
                    <th className="py-2">Action</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredInvoices.map((invoice) => (
                    <tr key={invoice.id} className="border-b border-border-muted last:border-b-0">
                      <td className="py-3 text-text-primary">{invoice.tenantName}</td>
                      <td className="py-3 text-text-primary">
                        {invoice.propertyName} - {invoice.unitNumber}
                      </td>
                      <td className="py-3 text-text-secondary">
                        {new Date(invoice.periodStart).toLocaleDateString()} -{' '}
                        {new Date(invoice.periodEnd).toLocaleDateString()}
                      </td>
                      <td className="py-3 text-text-secondary">{new Date(invoice.dueDate).toLocaleDateString()}</td>
                      <td className="py-3 text-text-primary">KSh {invoice.amount.toLocaleString()}</td>
                      <td className="py-3 text-text-primary">KSh {invoice.balance.toLocaleString()}</td>
                      <td className="py-3">
                        <span className="inline-flex items-center gap-2 text-text-secondary">
                          <FileText className="text-text-muted w-4 h-4" />
                          {InvoiceStatus[invoice.status]}
                        </span>
                      </td>
                      <td className="py-3">
                        <Link href={`/dashboard/invoices/${invoice.id}`} className="text-brand-secondary hover:text-brand-primary">
                          View
                        </Link>
                      </td>
                    </tr>
                  ))}
                  {filteredInvoices.length === 0 && (
                    <tr>
                      <td colSpan={8} className="py-6 text-center text-text-muted">
                        No invoices found
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </Card>
        )}
      </div>
    </ProtectedRoute>
  )
}

