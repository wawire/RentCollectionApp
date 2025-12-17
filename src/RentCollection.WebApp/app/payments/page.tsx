'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { useGetPayments, useDeletePayment, useGetProperties, useSendPaymentReceipt, useDownloadPaymentReceipt } from '@/lib/hooks'
import { Card, Button, LoadingSpinner, SearchBar, Select, Modal, Alert } from '@/components/common'
import PaymentList from '@/components/payments/PaymentList'
import PaymentCalendar from '@/components/payments/PaymentCalendar'
import { FaPlus, FaFileDownload, FaFilter, FaList, FaCalendarAlt } from 'react-icons/fa'
import { PaymentStatus } from '@/lib/types'

export default function PaymentsPage() {
  const router = useRouter()
  const { data: payments, loading, error, refetch } = useGetPayments()
  const { data: properties } = useGetProperties()
  const { deletePayment, loading: deleting } = useDeletePayment()
  const { sendReceipt, loading: sendingReceipt } = useSendPaymentReceipt()
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
    // Search filter
    const searchLower = searchTerm.toLowerCase()
    const matchesSearch =
      payment.tenantName.toLowerCase().includes(searchLower) ||
      payment.unitNumber.toLowerCase().includes(searchLower) ||
      payment.propertyName.toLowerCase().includes(searchLower) ||
      payment.transactionReference?.toLowerCase().includes(searchLower)

    // Status filter
    const matchesStatus = statusFilter === 'all' || payment.status === statusFilter

    // Property filter
    const matchesProperty = propertyFilter === 'all' || payment.propertyName === properties.find(p => p.id === propertyFilter)?.name

    // Date range filter
    const paymentDate = new Date(payment.paymentDate)
    const matchesDateFrom = !dateFrom || paymentDate >= new Date(dateFrom)
    const matchesDateTo = !dateTo || paymentDate <= new Date(dateTo)

    return matchesSearch && matchesStatus && matchesProperty && matchesDateFrom && matchesDateTo
  })

  // Calculate total amounts
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
    ...properties.map(p => ({ value: p.id, label: p.name }))
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
        <p className="text-red-600">Error loading payments</p>
        <Button onClick={() => refetch()} className="mt-4">Try Again</Button>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-3xl font-serif font-normal text-primary tracking-wide">Payments</h1>
          <p className="text-primary/60 mt-2 tracking-wide">Track and manage rent payments</p>
        </div>
        <div className="flex items-center gap-3">
          {/* View Mode Toggle */}
          <div className="flex gap-2">
            <button
              onClick={() => setViewMode('list')}
              className={`px-4 py-2 rounded-full text-sm font-medium tracking-wide transition-all ${
                viewMode === 'list'
                  ? 'bg-accent text-primary shadow-sm'
                  : 'bg-white text-primary/70 border border-secondary/50 hover:border-accent/50'
              }`}
            >
              <FaList className="inline mr-2" />
              List
            </button>
            <button
              onClick={() => setViewMode('calendar')}
              className={`px-4 py-2 rounded-full text-sm font-medium tracking-wide transition-all ${
                viewMode === 'calendar'
                  ? 'bg-accent text-primary shadow-sm'
                  : 'bg-white text-primary/70 border border-secondary/50 hover:border-accent/50'
              }`}
            >
              <FaCalendarAlt className="inline mr-2" />
              Calendar
            </button>
          </div>
          <Button variant="primary" onClick={() => router.push('/payments/new')}>
            <FaPlus className="mr-2" />
            Record Payment
          </Button>
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

      {/* Stats Summary */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card padding="md">
          <p className="text-sm text-gray-600 mb-1">Total Payments</p>
          <p className="text-2xl font-bold text-gray-900">{filteredPayments.length}</p>
        </Card>
        <Card padding="md">
          <p className="text-sm text-gray-600 mb-1">Total Amount</p>
          <p className="text-2xl font-bold text-primary-600">KSh {totalAmount.toLocaleString()}</p>
        </Card>
        <Card padding="md">
          <p className="text-sm text-gray-600 mb-1">Completed</p>
          <p className="text-2xl font-bold text-green-600">{completedPayments.length}</p>
        </Card>
        <Card padding="md">
          <p className="text-sm text-gray-600 mb-1">Completed Amount</p>
          <p className="text-2xl font-bold text-green-600">KSh {totalCompleted.toLocaleString()}</p>
        </Card>
      </div>

      {/* Filters */}
      <Card padding="md">
        <div className="space-y-4">
          <div className="flex items-center gap-2 mb-3">
            <FaFilter className="text-gray-500" />
            <h2 className="text-lg font-semibold text-gray-900">Filters</h2>
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
                className="flex-1 px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 text-sm"
              />
              <input
                type="date"
                value={dateTo}
                onChange={(e) => setDateTo(e.target.value)}
                placeholder="To date"
                className="flex-1 px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 text-sm"
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

      {/* Payments View */}
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
            <Button variant="danger" onClick={handleDeleteConfirm} loading={deleting}>
              Delete
            </Button>
          </>
        }
      >
        <p className="text-gray-700">
          Are you sure you want to delete this payment? This action cannot be undone.
        </p>
      </Modal>
    </div>
  )
}
