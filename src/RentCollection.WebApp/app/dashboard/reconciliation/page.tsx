'use client'

import { useEffect, useState } from 'react'
import ProtectedRoute from '@/components/auth/ProtectedRoute'
import { Card, Button, LoadingSpinner, Alert, Select } from '@/components/common'
import { reconciliationService } from '@/lib/services/reconciliationService'
import { paymentService } from '@/lib/services/paymentService'
import { unmatchedPaymentService } from '@/lib/services/unmatchedPaymentService'
import { UnmatchedPayment, UnmatchedPaymentStatus } from '@/lib/types/unmatchedPayment.types'
import { useGetActiveTenants } from '@/lib/hooks'
import { useAuth } from '@/contexts/AuthContext'
import { UserRole } from '@/lib/types/auth.types'

export default function ReconciliationPage() {
  const { user } = useAuth()
  const [paymentId, setPaymentId] = useState('')
  const [invoiceId, setInvoiceId] = useState('')
  const [amount, setAmount] = useState('')
  const [allocating, setAllocating] = useState(false)
  const [resolving, setResolving] = useState(false)
  const [alert, setAlert] = useState<{ type: 'success' | 'error'; message: string } | null>(null)
  const [unmatched, setUnmatched] = useState<UnmatchedPayment[]>([])
  const [loadingUnmatched, setLoadingUnmatched] = useState(false)
  const [statusFilter, setStatusFilter] = useState<number | 'all'>('all')
  const [resolveId, setResolveId] = useState('')
  const [resolveTenantId, setResolveTenantId] = useState('')
  const [resolvePeriodStart, setResolvePeriodStart] = useState('')
  const [resolvePeriodEnd, setResolvePeriodEnd] = useState('')
  const [resolvePaymentDate, setResolvePaymentDate] = useState('')
  const [resolveNotes, setResolveNotes] = useState('')
  const [tenantSearch, setTenantSearch] = useState('')
  const [debouncedTenantSearch, setDebouncedTenantSearch] = useState('')
  const [checkoutRequestId, setCheckoutRequestId] = useState('')
  const [statusLookup, setStatusLookup] = useState<string | null>(null)
  const [checkingStatus, setCheckingStatus] = useState(false)
  const { data: activeTenants, loading: loadingTenants } = useGetActiveTenants()

  const canViewUnmatched =
    user?.role === UserRole.PlatformAdmin || user?.role === UserRole.Landlord || user?.role === UserRole.Manager

  const formatDate = (date: Date) => date.toISOString().split('T')[0]
  const clampDay = (year: number, month: number, day: number) => {
    const daysInMonth = new Date(year, month + 1, 0).getDate()
    return Math.min(day, daysInMonth)
  }

  const getPeriodForRentDueDay = (rentDueDay: number) => {
    const today = new Date()
    const year = today.getFullYear()
    const month = today.getMonth()
    const dueDate = new Date(year, month, clampDay(year, month, rentDueDay))
    const isPastDue = today > dueDate
    const targetMonth = isPastDue ? month + 1 : month
    const targetYear = isPastDue && month === 11 ? year + 1 : year
    const periodStart = new Date(targetYear, targetMonth, 1)
    const periodEnd = new Date(targetYear, targetMonth + 1, 0)
    return { periodStart, periodEnd }
  }

  const loadUnmatched = async () => {
    if (!canViewUnmatched) return
    try {
      setLoadingUnmatched(true)
      const data = await unmatchedPaymentService.getAll(
        statusFilter === 'all' ? undefined : (statusFilter as UnmatchedPaymentStatus)
      )
      setUnmatched(data)
    } catch (err: any) {
      setAlert({ type: 'error', message: err.message || 'Failed to load unmatched payments' })
    } finally {
      setLoadingUnmatched(false)
    }
  }

  useEffect(() => {
    const today = new Date()
    const periodStart = new Date(today.getFullYear(), today.getMonth(), 1)
    const periodEnd = new Date(today.getFullYear(), today.getMonth() + 1, 0)
    setResolvePaymentDate(formatDate(today))
    setResolvePeriodStart(formatDate(periodStart))
    setResolvePeriodEnd(formatDate(periodEnd))
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  useEffect(() => {
    if (!resolveTenantId) return
    const tenant = activeTenants.find((t) => t.id === Number(resolveTenantId))
    if (!tenant) return
    const { periodStart, periodEnd } = getPeriodForRentDueDay(tenant.rentDueDay)
    setResolvePeriodStart(formatDate(periodStart))
    setResolvePeriodEnd(formatDate(periodEnd))
  }, [resolveTenantId, activeTenants])

  useEffect(() => {
    const handle = setTimeout(() => {
      setDebouncedTenantSearch(tenantSearch)
    }, 200)

    return () => clearTimeout(handle)
  }, [tenantSearch])

  useEffect(() => {
    loadUnmatched()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [statusFilter, canViewUnmatched])

  const handleAllocate = async () => {
    if (!paymentId) {
      setAlert({ type: 'error', message: 'Payment ID is required' })
      return
    }

    try {
      setAllocating(true)
      const parsedPaymentId = Number(paymentId)
      const parsedInvoiceId = invoiceId ? Number(invoiceId) : undefined
      const parsedAmount = amount ? Number(amount) : undefined

      await reconciliationService.allocatePayment(parsedPaymentId, parsedInvoiceId, parsedAmount)
      setAlert({ type: 'success', message: 'Allocation completed' })
      setPaymentId('')
      setInvoiceId('')
      setAmount('')
    } catch (err: any) {
      setAlert({ type: 'error', message: err.message || 'Allocation failed' })
    } finally {
      setAllocating(false)
    }
  }

  const handleUpdateStatus = async (id: number, status: UnmatchedPaymentStatus) => {
    try {
      await unmatchedPaymentService.updateStatus(id, status)
      await loadUnmatched()
    } catch (err: any) {
      setAlert({ type: 'error', message: err.message || 'Failed to update status' })
    }
  }

  const handleResolve = async () => {
    if (!resolveId || !resolveTenantId) {
      setAlert({ type: 'error', message: 'Unmatched Payment ID and Tenant ID are required' })
      return
    }

    if (!resolvePeriodStart || !resolvePeriodEnd) {
      setAlert({ type: 'error', message: 'Period start and end dates are required' })
      return
    }

    try {
      setResolving(true)
      await unmatchedPaymentService.resolve(Number(resolveId), {
        tenantId: Number(resolveTenantId),
        periodStart: resolvePeriodStart,
        periodEnd: resolvePeriodEnd,
        paymentDate: resolvePaymentDate || undefined,
        notes: resolveNotes || undefined,
      })

      setAlert({ type: 'success', message: 'Unmatched payment resolved and payment recorded' })
      setResolveId('')
      setResolveTenantId('')
      setResolveNotes('')
      await loadUnmatched()
    } catch (err: any) {
      setAlert({ type: 'error', message: err.message || 'Failed to resolve unmatched payment' })
    } finally {
      setResolving(false)
    }
  }

  const handleCheckStatus = async () => {
    if (!checkoutRequestId.trim()) {
      setAlert({ type: 'error', message: 'CheckoutRequestID is required' })
      return
    }

    try {
      setCheckingStatus(true)
      const result = await paymentService.getStkStatus(checkoutRequestId.trim())
      const description = result.resultDesc || result.responseDescription || 'Status received'
      setStatusLookup(description)
    } catch (err: any) {
      setStatusLookup(err.message || 'Failed to fetch status')
    } finally {
      setCheckingStatus(false)
    }
  }

  const statusOptions = [
    { value: 'all', label: 'All' },
    { value: UnmatchedPaymentStatus.Pending, label: 'Pending' },
    { value: UnmatchedPaymentStatus.Resolved, label: 'Resolved' },
    { value: UnmatchedPaymentStatus.Ignored, label: 'Ignored' },
  ]

  return (
    <ProtectedRoute allowedRoles={['PlatformAdmin', 'Landlord', 'Manager', 'Accountant']}>
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-serif font-normal text-primary tracking-wide">Reconciliation</h1>
          <p className="text-primary/60 mt-2 tracking-wide">Allocate payments and review unmatched transactions</p>
        </div>

        {alert && <Alert type={alert.type} message={alert.message} onClose={() => setAlert(null)} />}

        <Card padding="md">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Manual Allocation</h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Payment ID</label>
              <input
                type="number"
                value={paymentId}
                onChange={(e) => setPaymentId(e.target.value)}
                className="w-full rounded-lg border border-gray-300 px-3 py-2"
                placeholder="Payment ID"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Invoice ID (optional)</label>
              <input
                type="number"
                value={invoiceId}
                onChange={(e) => setInvoiceId(e.target.value)}
                className="w-full rounded-lg border border-gray-300 px-3 py-2"
                placeholder="Invoice ID"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Amount (optional)</label>
              <input
                type="number"
                value={amount}
                onChange={(e) => setAmount(e.target.value)}
                className="w-full rounded-lg border border-gray-300 px-3 py-2"
                placeholder="Amount"
              />
            </div>
          </div>
          <div className="mt-4">
            <Button variant="primary" onClick={handleAllocate} disabled={allocating}>
              {allocating ? 'Allocating...' : 'Allocate (FIFO default)'}
            </Button>
          </div>
        </Card>

        <Card padding="md">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">STK Status Lookup</h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 items-end">
            <div className="md:col-span-2">
              <label className="block text-sm font-medium text-gray-700 mb-1">Checkout Request ID</label>
              <input
                type="text"
                value={checkoutRequestId}
                onChange={(e) => setCheckoutRequestId(e.target.value)}
                className="w-full rounded-lg border border-gray-300 px-3 py-2"
                placeholder="ws_CO_..."
              />
            </div>
            <div>
              <Button variant="secondary" onClick={handleCheckStatus} disabled={checkingStatus}>
                {checkingStatus ? 'Checking...' : 'Check Status'}
              </Button>
            </div>
          </div>
          {statusLookup && (
            <div className="mt-3 text-sm text-gray-700">
              Status: {statusLookup}
            </div>
          )}
        </Card>

        {canViewUnmatched && (
          <Card padding="md">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-semibold text-gray-900">Unmatched Payments</h2>
              <div className="w-40">
                <Select
                  value={statusFilter}
                  onChange={(event) => {
                    const value = event.target.value
                    setStatusFilter(value === 'all' ? 'all' : Number(value))
                  }}
                  options={statusOptions}
                />
              </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-6 gap-4 mb-6">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Unmatched Payment ID</label>
                <input
                  type="number"
                  value={resolveId}
                  onChange={(e) => setResolveId(e.target.value)}
                  className="w-full rounded-lg border border-gray-300 px-3 py-2"
                  placeholder="Unmatched ID"
                />
              </div>
              <div>
                <Select
                  label="Tenant"
                  value={resolveTenantId}
                  onChange={(e) => setResolveTenantId(e.target.value)}
                  options={activeTenants
                    .filter((tenant) => {
                      if (!debouncedTenantSearch) return true
                      const query = debouncedTenantSearch.toLowerCase()
                      return (
                        tenant.fullName.toLowerCase().includes(query) ||
                        tenant.unitNumber.toLowerCase().includes(query) ||
                        tenant.email.toLowerCase().includes(query)
                      )
                    })
                    .map((tenant) => ({
                    value: tenant.id,
                    label: `${tenant.fullName} • ${tenant.unitNumber} • ${tenant.propertyName}`,
                  }))}
                  placeholder={loadingTenants ? 'Loading tenants...' : 'Select tenant'}
                  disabled={loadingTenants}
                />
              </div>
              <div className="md:col-span-6">
                <label className="block text-sm font-medium text-gray-700 mb-1">Search Tenant</label>
                <input
                  type="text"
                  value={tenantSearch}
                  onChange={(e) => setTenantSearch(e.target.value)}
                  className="w-full rounded-lg border border-gray-300 px-3 py-2"
                  placeholder="Search by name, unit, or email"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Period Start</label>
                <input
                  type="date"
                  value={resolvePeriodStart}
                  onChange={(e) => setResolvePeriodStart(e.target.value)}
                  className="w-full rounded-lg border border-gray-300 px-3 py-2"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Period End</label>
                <input
                  type="date"
                  value={resolvePeriodEnd}
                  onChange={(e) => setResolvePeriodEnd(e.target.value)}
                  className="w-full rounded-lg border border-gray-300 px-3 py-2"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Payment Date</label>
                <input
                  type="date"
                  value={resolvePaymentDate}
                  onChange={(e) => setResolvePaymentDate(e.target.value)}
                  className="w-full rounded-lg border border-gray-300 px-3 py-2"
                />
              </div>
              <div className="flex items-end">
                <Button variant="secondary" onClick={handleResolve} disabled={resolving}>
                  {resolving ? 'Resolving...' : 'Resolve Payment'}
                </Button>
              </div>
              <div className="md:col-span-6">
                <label className="block text-sm font-medium text-gray-700 mb-1">Notes (optional)</label>
                <input
                  type="text"
                  value={resolveNotes}
                  onChange={(e) => setResolveNotes(e.target.value)}
                  className="w-full rounded-lg border border-gray-300 px-3 py-2"
                  placeholder="Resolution notes"
                />
              </div>
            </div>

            {loadingUnmatched ? (
              <div className="flex items-center justify-center py-8">
                <LoadingSpinner size="md" text="Loading unmatched payments..." />
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="text-left text-gray-500 border-b">
                      <th className="py-2">Transaction</th>
                      <th className="py-2">Account Ref</th>
                      <th className="py-2">Amount</th>
                      <th className="py-2">Reason</th>
                      <th className="py-2">Status</th>
                      <th className="py-2">Action</th>
                    </tr>
                  </thead>
                  <tbody>
                    {unmatched.map((item) => (
                      <tr key={item.id} className="border-b last:border-b-0">
                        <td className="py-3">{item.transactionReference}</td>
                        <td className="py-3">{item.accountReference}</td>
                        <td className="py-3">KSh {item.amount.toLocaleString()}</td>
                        <td className="py-3">{item.reason}</td>
                        <td className="py-3">{UnmatchedPaymentStatus[item.status]}</td>
                        <td className="py-3 space-x-2">
                          {item.status === UnmatchedPaymentStatus.Pending && (
                            <>
                              <Button
                                variant="secondary"
                                onClick={() => handleUpdateStatus(item.id, UnmatchedPaymentStatus.Resolved)}
                              >
                                Resolve
                              </Button>
                              <Button
                                variant="secondary"
                                onClick={() => handleUpdateStatus(item.id, UnmatchedPaymentStatus.Ignored)}
                              >
                                Ignore
                              </Button>
                            </>
                          )}
                        </td>
                      </tr>
                    ))}
                    {unmatched.length === 0 && (
                      <tr>
                        <td colSpan={6} className="py-6 text-center text-gray-500">
                          No unmatched payments
                        </td>
                      </tr>
                    )}
                  </tbody>
                </table>
              </div>
            )}
          </Card>
        )}
      </div>
    </ProtectedRoute>
  )
}

