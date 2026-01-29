'use client'

import { useState } from 'react'
import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { ArrowLeft, CheckCircle, AlertCircle } from 'lucide-react'
import { tenantPaymentService } from '@/lib/services/tenantPaymentService'
import { RecordPaymentDto } from '@/lib/types/tenantPayment.types'
import { Alert, Button, Card, Input, PageHeader, Select, TextArea } from '@/components/common'

export default function RecordPaymentPage() {
  const router = useRouter()
  const [formData, setFormData] = useState<RecordPaymentDto>({
    amount: 0,
    paymentDate: new Date().toISOString().split('T')[0],
    paymentMethod: 'MPesa',
    transactionReference: '',
    mPesaPhoneNumber: '',
    notes: '',
    periodStart: new Date(new Date().getFullYear(), new Date().getMonth(), 1)
      .toISOString()
      .split('T')[0],
    periodEnd: new Date(new Date().getFullYear(), new Date().getMonth() + 1, 0)
      .toISOString()
      .split('T')[0],
  })
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState(false)

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>
  ) => {
    const { name, value } = e.target
    setFormData((prev) => ({
      ...prev,
      [name]: name === 'amount' ? parseFloat(value) || 0 : value,
    }))
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setLoading(true)
    setError(null)

    try {
      const paymentData: RecordPaymentDto = {
        ...formData,
        paymentDate: new Date(formData.paymentDate).toISOString(),
        periodStart: new Date(formData.periodStart).toISOString(),
        periodEnd: new Date(formData.periodEnd).toISOString(),
      }

      await tenantPaymentService.recordPayment(paymentData)
      setSuccess(true)
      setTimeout(() => {
        router.push('/tenant-portal')
      }, 2000)
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to record payment')
    } finally {
      setLoading(false)
    }
  }

  if (success) {
    return (
      <div className="flex items-center justify-center h-96">
        <div className="text-center">
          <div className="inline-flex items-center justify-center w-16 h-16 bg-state-success/10 rounded-full mb-4">
            <CheckCircle className="text-state-success" size={32} />
          </div>
          <h2 className="text-2xl font-semibold text-text-primary mb-2">
            Payment Recorded Successfully
          </h2>
          <p className="text-text-secondary mb-4">
            Your payment has been submitted and is awaiting landlord confirmation.
          </p>
          <p className="text-sm text-text-muted">Redirecting to dashboard...</p>
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-6 max-w-2xl mx-auto">
      <PageHeader
        title="Record Payment"
        subtitle="Submit your payment details for landlord verification."
        breadcrumbs={[
          { label: 'Tenant Portal', href: '/tenant-portal' },
          { label: 'Record Payment' },
        ]}
        actions={
          <Link href="/tenant-portal">
            <Button variant="ghost">
              <ArrowLeft size={16} className="mr-2" />
              Back to Dashboard
            </Button>
          </Link>
        }
      />

      {error && (
        <Alert
          type="error"
          title="Error"
          message={error}
        />
      )}

      <Card padding="md">
        <form onSubmit={handleSubmit} className="space-y-6">
          <div>
            <h2 className="text-lg font-semibold text-text-primary mb-4">
              Payment Details
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <Input
                type="number"
                id="amount"
                name="amount"
                label="Amount (KSh)"
                value={formData.amount || ''}
                onChange={handleChange}
                required
                min="0"
                step="0.01"
                placeholder="12000"
              />

              <Input
                type="date"
                id="paymentDate"
                name="paymentDate"
                label="Payment Date"
                value={formData.paymentDate}
                onChange={handleChange}
                required
                max={new Date().toISOString().split('T')[0]}
              />

              <Select
                id="paymentMethod"
                name="paymentMethod"
                label="Payment Method"
                value={formData.paymentMethod}
                onChange={handleChange}
                required
                options={[
                  { value: 'MPesa', label: 'M-Pesa' },
                  { value: 'BankTransfer', label: 'Bank Transfer' },
                  { value: 'Cash', label: 'Cash' },
                  { value: 'Cheque', label: 'Cheque' },
                ]}
              />

              <Input
                type="text"
                id="transactionReference"
                name="transactionReference"
                label="Transaction Reference"
                value={formData.transactionReference}
                onChange={handleChange}
                required
                placeholder="SKG8N9Q2RT"
                helperText="M-Pesa code from SMS or bank transaction ID"
              />

              {formData.paymentMethod === 'MPesa' && (
                <div className="md:col-span-2">
                  <Input
                    type="tel"
                    id="mPesaPhoneNumber"
                    name="mPesaPhoneNumber"
                    label="M-Pesa Phone Number"
                    value={formData.mPesaPhoneNumber}
                    onChange={handleChange}
                    placeholder="0722123456 or +254722123456"
                    helperText="Phone number used for M-Pesa payment"
                    fullWidth
                  />
                </div>
              )}
            </div>
          </div>

          <div>
            <h2 className="text-lg font-semibold text-text-primary mb-4">
              Payment Period
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <Input
                type="date"
                id="periodStart"
                name="periodStart"
                label="Period Start"
                value={formData.periodStart}
                onChange={handleChange}
                required
              />
              <Input
                type="date"
                id="periodEnd"
                name="periodEnd"
                label="Period End"
                value={formData.periodEnd}
                onChange={handleChange}
                required
              />
            </div>
            <p className="text-xs text-text-muted mt-2">
              The rental period this payment covers (e.g., December 1-31, 2025).
            </p>
          </div>

          <TextArea
            id="notes"
            name="notes"
            label="Notes (Optional)"
            value={formData.notes}
            onChange={handleChange}
            rows={3}
            placeholder="Any additional information about this payment..."
          />

          <div className="flex items-center gap-4">
            <Button type="submit" disabled={loading}>
              {loading ? 'Submitting...' : 'Submit Payment'}
            </Button>
            <Link href="/tenant-portal">
              <Button variant="secondary">Cancel</Button>
            </Link>
          </div>

          <Alert
            type="info"
            title="What happens next"
            message="Your payment will be marked as Pending. The landlord will verify the transaction, then your status will update to Completed and you will be notified."
          />
        </form>
      </Card>
    </div>
  )
}
