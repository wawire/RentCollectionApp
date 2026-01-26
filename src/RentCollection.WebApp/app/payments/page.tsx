'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import {
  useGetPayments,
  useDeletePayment,
  useGetProperties,
  useDownloadPaymentReceipt,
} from '@/lib/hooks'
import { Card, Button, LoadingSpinner, SearchBar, Select, Modal, Alert, PageHeader } from '@/components/common'
import PaymentList from '@/components/payments/PaymentList'
import PaymentCalendar from '@/components/payments/PaymentCalendar'
import ProtectedRoute from '@/components/auth/ProtectedRoute'
import { UserRole } from '@/lib/types/auth.types'
import { Calendar, Filter, List, Plus } from 'lucide-react'
import { PaymentStatus } from '@/lib/types'

export default function PaymentsPage() {
  const router = useRouter()
  const { data: payments, loading, error, refetch } = useGetPayments()
  const { data: properties } = useGetProperties()
  const { deletePayment, loading: deleting } = useDeletePayment()
  const { downloadReceipt, loading: downloading } = useDownloadPaymentReceipt()

  const [searchTerm, setSearchTerm] = useState('')
  const [statusFilter, setStatusFilter] = useState<number | 'all'>('all')
  const [propertyFilter, setPropertyFilter] = useState<number | 'all'>('all')
  const [dateFrom, setDateFrom] = useState('')
  const [dateTo, setDateTo] = useState('')
  const [showDeleteModal, setShowDeleteModal] = useState(false)
  const [selectedPaymentId, setSelectedPaymentId] = useState<number | null>(null)
  const [alert, setAlert] = useState<{ type: 'success' | 'error', message: string } | null>(null)
  const [viewMode, setViewMode] = useState<'list' | 'calendar'>('list')

  // Filter payments
  const filteredPayments = payments.filter(payment => {
    const searchLower = searchTerm.toLowerCase()
    const matchesSearch =
      payment.tenantName.toLowerCase().includes(searchLower) ||
      payment.unitNumber.toLowerCase().includes(searchLower) ||
      payment.propertyName.toLowerCase().includes(searchLower) ||
      payment.transactionReference?.toLowerCase().includes(searchLower)

    const matchesStatus = statusFilter === 'all' || payment.status === statusFilter
    const matchesProperty = propertyFilter === 'all' || payment.propertyName === properties.find(p => p.id === propertyFilter)?.name

    const paymentDate = new Date(payment.paymentDate)
    const matchesDateFrom = !dateFrom || paymentDate >= new Date(dateFrom)
    const matchesDateTo = !dateTo || paymentDate <= new Date(dateTo)

    return matchesSearch && matchesStatus && matchesProperty && matchesDateFrom && matchesDateTo
  })

  const totalAmount = filteredPayments.reduce((sum, p) => sum + p.amount, 0)
  const completedPayments = filteredPayments.filter(p => p.status === PaymentStatus.Completed)
  const totalCompleted = completedPayments.reduce((sum, p) => sum + p.amount, 0)

  const handleDownloadReceipt = async (paymentId: number) => {
    const success = await downloadReceipt(paymentId)
    if (success) {
      setAlert({ type: 'success', message: 'Receipt downloaded successfully!' })
      setTimeout(() => setAlert(null), 3000)
    } else {
      setAlert({ type: 'error', message: 'Failed to download receipt' })
      setTimeout(() => setAlert(null), 3000)
    }
  }

  const handleDeleteClick = (paymentId: number) => {
    setSelectedPaymentId(paymentId)
    setShowDeleteModal(true)
  }

  const handleDeleteConfirm = async () => {
    if (selectedPaymentId) {
      const success = await deletePayment(selectedPaymentId)
      if (success) {
        setShowDeleteModal(false)
        setSelectedPaymentId(null)
        refetch()
        setAlert({ type: 'success', message: 'Payment deleted successfully' })
        setTimeout(() => setAlert(null), 3000)
      }
    }
  }

  const statusOptions = [
    { value: 'all', label: 'All Statuses' },
    { value: PaymentStatus.Completed, label: 'Completed' },
    { value: PaymentStatus.Pending, label: 'Pending' },
    { value: PaymentStatus.Failed, label: 'Failed' },
    { value: PaymentStatus.Refunded, label: 'Refunded' },
    { value: PaymentStatus.PartiallyPaid, label: 'Partially Paid' },
  ]

  const propertyOptions = [
    { value: 'all', label: 'All Properties' },
    ...properties.map(p => ({ value: p.id, label: p.name })),
  ]

  if (loading) {
    return (
      <div className="flex items-center justify-center h-96">
        <LoadingSpinner size="lg" text="Loading payments..." />
      </div>
    )
  }

  if (error) {
    return (
      <div className="text-center py-12">
        <p className="text-state-error">Error loading payments</p>
        <Button onClick={() => refetch()} className="mt-4">Try Again</Button>
      </div>
    )
  }

  return (
    <ProtectedRoute allowedRoles={[UserRole.PlatformAdmin, UserRole.Landlord, UserRole.Manager, UserRole.Accountant]}>
      <div className="space-y-6">
        <PageHeader
          title="Payments / Rent"
          subtitle="Track, reconcile, and confirm rent payments."
          breadcrumbs={[
            { label: 'Dashboard', href: '/dashboard' },
            { label: 'Payments' },
          ]}
          actions={
            <div className="flex items-center gap-3">
              <div className="flex gap-2">
                <button
                  onClick={() => setViewMode('list')}
                  className={`px-4 py-2 rounded-full text-sm font-medium transition-all ${
                    viewMode === 'list'
                      ? 'bg-brand-primary text-white shadow-subtle'
                      : 'bg-surface text-text-secondary border border-border-muted hover:border-brand-secondary'
                  }`}
                >
                  <List className="inline mr-2 w-4 h-4" />
                  List
                </button>
                <button
                  onClick={() => setViewMode('calendar')}
                  className={`px-4 py-2 rounded-full text-sm font-medium transition-all ${
                    viewMode === 'calendar'
                      ? 'bg-brand-primary text-white shadow-subtle'
                      : 'bg-surface text-text-secondary border border-border-muted hover:border-brand-secondary'
                  }`}
                >
                  <Calendar className="inline mr-2 w-4 h-4" />
                  Calendar
                </button>
              </div>
              <Button onClick={() => router.push('/payments/new')}>
                <Plus className="mr-2 w-4 h-4" />
                Record Payment
              </Button>
            </div>
          }
        />

        {alert && (
          <Alert
            type={alert.type}
            message={alert.message}
            onClose={() => setAlert(null)}
          />
        )}

        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <Card padding="md">
            <p className="text-sm text-text-muted mb-1">Total Payments</p>
            <p className="text-2xl font-semibold text-text-primary">{filteredPayments.length}</p>
          </Card>
          <Card padding="md">
            <p className="text-sm text-text-muted mb-1">Total Amount</p>
            <p className="text-2xl font-semibold text-brand-primary">KSh {totalAmount.toLocaleString()}</p>
          </Card>
          <Card padding="md">
            <p className="text-sm text-text-muted mb-1">Completed</p>
            <p className="text-2xl font-semibold text-state-success">{completedPayments.length}</p>
          </Card>
          <Card padding="md">
            <p className="text-sm text-text-muted mb-1">Completed Amount</p>
            <p className="text-2xl font-semibold text-state-success">KSh {totalCompleted.toLocaleString()}</p>
          </Card>
        </div>

        <Card padding="md">
          <div className="space-y-4">
            <div className="flex items-center gap-2 mb-3">
              <Filter className="text-text-muted w-4 h-4" />
              <h2 className="text-lg font-semibold text-text-primary">Filters</h2>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
              <div>
                <SearchBar
                  value={searchTerm}
                  onChange={setSearchTerm}
                  placeholder="Search by tenant, unit, property..."
                  className="w-full"
                />
              </div>

              <Select
                label=""
                value={statusFilter}
                onChange={(e) => setStatusFilter(e.target.value === 'all' ? 'all' : Number(e.target.value))}
                options={statusOptions}
                fullWidth
              />

              <Select
                label=""
                value={propertyFilter}
                onChange={(e) => setPropertyFilter(e.target.value === 'all' ? 'all' : Number(e.target.value))}
                options={propertyOptions}
                fullWidth
              />

              <div className="flex gap-2">
                <input
                  type="date"
                  value={dateFrom}
                  onChange={(e) => setDateFrom(e.target.value)}
                  placeholder="From date"
                  className="flex-1 px-3 py-2 rounded-lg border border-border-muted bg-surface text-text-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-secondary/60 focus-visible:ring-offset-2 focus-visible:ring-offset-brand-bg text-sm"
                />
                <input
                  type="date"
                  value={dateTo}
                  onChange={(e) => setDateTo(e.target.value)}
                  placeholder="To date"
                  className="flex-1 px-3 py-2 rounded-lg border border-border-muted bg-surface text-text-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-secondary/60 focus-visible:ring-offset-2 focus-visible:ring-offset-brand-bg text-sm"
                />
              </div>
            </div>

            {(searchTerm || statusFilter !== 'all' || propertyFilter !== 'all' || dateFrom || dateTo) && (
              <div className="flex justify-end">
                <Button
                  variant="secondary"
                  size="sm"
                  onClick={() => {
                    setSearchTerm('')
                    setStatusFilter('all')
                    setPropertyFilter('all')
                    setDateFrom('')
                    setDateTo('')
                  }}
                >
                  Clear Filters
                </Button>
              </div>
            )}
          </div>
        </Card>

        {viewMode === 'list' ? (
          <Card padding="none">
            <PaymentList
              payments={filteredPayments}
              loading={loading}
              onDownloadReceipt={handleDownloadReceipt}
              onDelete={handleDeleteClick}
            />
          </Card>
        ) : (
          <PaymentCalendar payments={filteredPayments} />
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
              <Button variant="destructive" onClick={handleDeleteConfirm} loading={deleting}>
                Delete
              </Button>
            </>
          }
        >
          <p className="text-text-secondary">
            Are you sure you want to delete this payment? This action cannot be undone.
          </p>
        </Modal>
      </div>
    </ProtectedRoute>
  )
}

