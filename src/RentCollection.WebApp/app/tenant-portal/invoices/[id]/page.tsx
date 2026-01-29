'use client'

import { useEffect, useState } from 'react'
import { useParams, useRouter } from 'next/navigation'
import ProtectedRoute from '@/components/auth/ProtectedRoute'
import { Card, Button, LoadingSpinner, Alert } from '@/components/common'
import { invoiceService } from '@/lib/services/invoiceService'
import { tenantPaymentService } from '@/lib/services/tenantPaymentService'
import { Invoice, InvoiceStatus } from '@/lib/types/invoice.types'

export default function TenantInvoiceDetailPage() {
  const params = useParams()
  const router = useRouter()
  const [invoice, setInvoice] = useState<Invoice | null>(null)
  const [phoneNumber, setPhoneNumber] = useState('')
  const [loading, setLoading] = useState(true)
  const [paying, setPaying] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [alert, setAlert] = useState<{ type: 'success' | 'error'; message: string } | null>(null)
  const [checkoutRequestId, setCheckoutRequestId] = useState<string | null>(null)
  const [statusMessage, setStatusMessage] = useState<string | null>(null)
  const [statusLoading, setStatusLoading] = useState(false)

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

  const handlePayNow = async () => {
    if (!invoice) return
    if (!phoneNumber.trim()) {
      setAlert({ type: 'error', message: 'Phone number is required' })
      return
    }

    try {
      setPaying(true)
      const response = await tenantPaymentService.initiateStkPush({
        amount: invoice.balance,
        phoneNumber,
        accountReference: invoice.unitNumber,
      })
      setAlert({ type: 'success', message: 'STK Push sent. Check your phone to complete payment.' })
      setCheckoutRequestId(response.checkoutRequestId)
      setStatusMessage('Waiting for payment confirmation...')
    } catch (err: any) {
      setAlert({ type: 'error', message: err.message || 'Failed to initiate STK Push' })
    } finally {
      setPaying(false)
    }
  }

  const handleCheckStatus = async () => {
    if (!checkoutRequestId) return
    setStatusLoading(true)
    try {
      const result = await tenantPaymentService.getStkStatus(checkoutRequestId)
      const description = result.resultDesc || result.responseDescription || 'Status received'
      setStatusMessage(description)
    } catch (err: any) {
      setStatusMessage(err.message || 'Failed to fetch status')
    } finally {
      setStatusLoading(false)
    }
  }

  return (
    <ProtectedRoute allowedRoles={['Tenant']}>
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-serif font-normal text-primary tracking-wide">Invoice Details</h1>
            <p className="text-primary/60 mt-2 tracking-wide">Invoice #{invoiceId}</p>
          </div>
          <Button variant="secondary" onClick={() => router.back()}>
            Back
          </Button>
        </div>

        {alert && <Alert type={alert.type} message={alert.message} onClose={() => setAlert(null)} />}

        {loading ? (
          <div className="flex items-center justify-center h-96">
            <LoadingSpinner size="lg" text="Loading invoice..." />
          </div>
        ) : error ? (
          <div className="text-center py-12">
            <p className="text-red-600">{error}</p>
          </div>
        ) : invoice ? (
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            <Card padding="md" className="lg:col-span-2">
              <div className="space-y-4">
                <div>
                  <p className="text-sm text-primary/60">Property / Unit</p>
                  <p className="text-lg font-semibold text-primary">
                    {invoice.propertyName} - {invoice.unitNumber}
                  </p>
                </div>
                <div>
                  <p className="text-sm text-primary/60">Period</p>
                  <p className="text-lg font-semibold text-primary">
                    {new Date(invoice.periodStart).toLocaleDateString()} -{' '}
                    {new Date(invoice.periodEnd).toLocaleDateString()}
                  </p>
                </div>
                <div>
                  <p className="text-sm text-primary/60">Due Date</p>
                  <p className="text-lg font-semibold text-primary">
                    {new Date(invoice.dueDate).toLocaleDateString()}
                  </p>
                </div>
                <div>
                  <p className="text-sm text-primary/60">Status</p>
                  <p className="text-lg font-semibold text-primary">{InvoiceStatus[invoice.status]}</p>
                </div>
                <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                  <div>
                    <p className="text-sm text-primary/60">Amount</p>
                    <p className="text-lg font-semibold text-primary">KSh {invoice.amount.toLocaleString()}</p>
                  </div>
                  <div>
                    <p className="text-sm text-primary/60">Opening Balance</p>
                    <p className="text-lg font-semibold text-primary">
                      KSh {invoice.openingBalance.toLocaleString()}
                    </p>
                  </div>
                  <div>
                    <p className="text-sm text-primary/60">Balance</p>
                    <p className="text-lg font-semibold text-primary">KSh {invoice.balance.toLocaleString()}</p>
                  </div>
                </div>
              </div>
            </Card>

            {invoice.lineItems && invoice.lineItems.length > 0 && (
              <Card padding="md" className="lg:col-span-2">
                <h2 className="text-lg font-semibold text-primary mb-4">Invoice Line Items</h2>
                <div className="overflow-x-auto">
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="text-left text-primary/60 border-b border-primary/10">
                        <th className="py-2">Item</th>
                        <th className="py-2">Quantity</th>
                        <th className="py-2">Rate</th>
                        <th className="py-2 text-right">Amount</th>
                      </tr>
                    </thead>
                    <tbody>
                      {invoice.lineItems.map((item) => (
                        <tr key={item.id} className="border-b border-primary/10 last:border-b-0">
                          <td className="py-2">
                            <div className="text-primary">{item.description}</div>
                            {item.utilityName && (
                              <div className="text-xs text-primary/60">{item.utilityName}</div>
                            )}
                          </td>
                          <td className="py-2">
                            {item.quantity.toLocaleString()} {item.unitOfMeasure || ''}
                          </td>
                          <td className="py-2">KSh {item.rate.toLocaleString()}</td>
                          <td className="py-2 text-right">KSh {item.amount.toLocaleString()}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </Card>
            )}

            <Card padding="md">
              <h2 className="text-lg font-semibold text-primary mb-4">Pay via M-Pesa STK</h2>
              <p className="text-sm text-primary/60 mb-4">
                Enter your phone number to receive the payment prompt.
              </p>
              <div className="space-y-3">
                <div>
                  <label className="block text-sm font-medium text-primary/80 mb-1">Phone Number</label>
                  <input
                    type="tel"
                    value={phoneNumber}
                    onChange={(e) => setPhoneNumber(e.target.value)}
                    className="w-full rounded-lg border border-primary/20 px-3 py-2"
                    placeholder="07XXXXXXXX"
                  />
                </div>
                <Button variant="primary" onClick={handlePayNow} disabled={paying || invoice.balance <= 0}>
                  {paying ? 'Sending STK...' : `Pay KSh ${invoice.balance.toLocaleString()}`}
                </Button>
                {checkoutRequestId && (
                  <div className="pt-2 space-y-2 text-sm text-primary/70">
                    <button
                      onClick={handleCheckStatus}
                      disabled={statusLoading}
                      className="inline-flex items-center gap-2 text-primary hover:text-primary/80"
                    >
                      {statusLoading ? 'Checking...' : 'Check Payment Status'}
                    </button>
                    <div className="text-xs text-primary/60">Checkout ID: {checkoutRequestId}</div>
                    {statusMessage && <div>{statusMessage}</div>}
                  </div>
                )}
              </div>
            </Card>
          </div>
        ) : null}
      </div>
    </ProtectedRoute>
  )
}
