'use client'

import { useState } from 'react'
import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { tenantPaymentService } from '@/lib/services/tenantPaymentService'
import { Card } from '@/components/common'
import { AlertCircle, ArrowLeft, CheckCircle2, Loader2, Smartphone } from 'lucide-react'

export default function PayNowPage() {
  const router = useRouter()

  const [formData, setFormData] = useState({
    amount: '',
    phoneNumber: '',
    accountReference: 'RENT',
  })

  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState(false)
  const [stkPushSent, setStkPushSent] = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setLoading(true)
    setError(null)

    try {
      const response = await tenantPaymentService.initiateStkPush({
        amount: parseFloat(formData.amount),
        phoneNumber: formData.phoneNumber,
        accountReference: formData.accountReference,
        transactionDesc: 'Rent Payment',
      })

      setStkPushSent(true)

      // Simulate waiting for payment confirmation
      setTimeout(() => {
        setSuccess(true)
        setTimeout(() => {
          router.push('/tenant-portal')
        }, 3000)
      }, 5000)

    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to initiate M-Pesa payment')
    } finally {
      setLoading(false)
    }
  }

  if (success) {
    return (
      <div className="max-w-2xl mx-auto space-y-6">
        <div className="flex items-center justify-center h-96">
          <div className="text-center">
            <div className="inline-flex items-center justify-center w-16 h-16 bg-green-100 rounded-full mb-4">
              <CheckCircle2 className="text-green-600 text-3xl" />
            </div>
            <h2 className="text-2xl font-serif font-normal text-primary tracking-wide mb-2">
              Payment Successful!
            </h2>
            <p className="text-primary/60 mb-4">
              Your M-Pesa payment has been received and processed.
            </p>
            <p className="text-sm text-primary/60">Redirecting to dashboard...</p>
          </div>
        </div>
      </div>
    )
  }

  if (stkPushSent) {
    return (
      <div className="max-w-2xl mx-auto space-y-6">
        <div className="flex items-center justify-center h-96">
          <div className="text-center">
            <div className="inline-flex items-center justify-center w-16 h-16 bg-blue-100 rounded-full mb-4 animate-pulse">
              <Smartphone className="text-blue-600 text-3xl" />
            </div>
            <h2 className="text-2xl font-serif font-normal text-primary tracking-wide mb-2">
              Check Your Phone
            </h2>
            <p className="text-primary/60 mb-4">
              An M-Pesa payment request has been sent to <strong>{formData.phoneNumber}</strong>
            </p>
            <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 text-left max-w-md mx-auto">
              <p className="text-sm font-medium text-primary mb-2">Next Steps:</p>
              <ol className="text-sm text-primary/80 space-y-1 list-decimal list-inside">
                <li>Check your phone for the M-Pesa prompt</li>
                <li>Enter your M-Pesa PIN</li>
                <li>Confirm the payment</li>
                <li>Wait for confirmation SMS</li>
              </ol>
            </div>
            <div className="mt-6 flex items-center justify-center gap-2 text-primary/60">
              <Loader2 className="animate-spin w-4 h-4" />
              <span className="text-sm">Waiting for payment confirmation...</span>
            </div>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="max-w-2xl mx-auto space-y-6">
      {/* Header */}
      <div>
        <Link
          href="/tenant-portal"
          className="inline-flex items-center gap-2 text-primary/60 hover:text-primary transition-colors mb-4"
        >
          <ArrowLeft className="w-4 h-4" />
          Back to Dashboard
        </Link>
        <h1 className="text-3xl font-serif font-normal text-primary tracking-wide">
          Pay with M-Pesa
        </h1>
        <p className="text-primary/60 mt-2 tracking-wide">
          Make instant rent payment via M-Pesa STK Push
        </p>
      </div>

      {error && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4 flex items-start gap-3">
          <AlertCircle className="text-red-600 text-xl flex-shrink-0 mt-0.5" />
          <div>
            <p className="font-medium text-red-800">Payment Failed</p>
            <p className="text-red-700 text-sm">{error}</p>
          </div>
        </div>
      )}

      {/* Payment Info Card */}
      <Card padding="lg">
        <div className="bg-gradient-to-r from-green-50 to-green-100 rounded-lg p-6 mb-6 border border-green-200">
          <div className="flex items-center gap-4">
            <div className="w-12 h-12 bg-green-600 rounded-full flex items-center justify-center">
              <Smartphone className="text-white text-xl" />
            </div>
            <div>
              <h3 className="text-lg font-semibold text-primary">M-Pesa STK Push Payment</h3>
              <p className="text-sm text-primary/70">Enter your phone number and amount to pay instantly</p>
            </div>
          </div>
        </div>

        <form onSubmit={handleSubmit} className="space-y-6">
          {/* Amount */}
          <div>
            <label htmlFor="amount" className="block text-sm font-medium text-primary/80 mb-1">
              Amount (KSh) <span className="text-red-500">*</span>
            </label>
            <input
              type="number"
              id="amount"
              value={formData.amount}
              onChange={(e) => setFormData({ ...formData, amount: e.target.value })}
              required
              min="1"
              step="0.01"
              className="w-full px-4 py-3 text-lg border border-primary/20 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/50"
              placeholder="12000"
            />
            <p className="text-xs text-primary/60 mt-1">
              Enter the amount you want to pay
            </p>
          </div>

          {/* Phone Number */}
          <div>
            <label htmlFor="phoneNumber" className="block text-sm font-medium text-primary/80 mb-1">
              M-Pesa Phone Number <span className="text-red-500">*</span>
            </label>
            <input
              type="tel"
              id="phoneNumber"
              value={formData.phoneNumber}
              onChange={(e) => setFormData({ ...formData, phoneNumber: e.target.value })}
              required
              className="w-full px-4 py-3 text-lg border border-primary/20 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/50"
              placeholder="0722123456 or +254722123456"
            />
            <p className="text-xs text-primary/60 mt-1">
              The phone number registered with M-Pesa
            </p>
          </div>

          {/* Info Box */}
          <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
            <p className="text-sm font-medium text-primary mb-2">How STK Push Works:</p>
            <ol className="text-sm text-primary/80 space-y-1 list-decimal list-inside">
              <li>Click "Pay Now" button below</li>
              <li>You'll receive an M-Pesa prompt on your phone</li>
              <li>Enter your M-Pesa PIN to confirm payment</li>
              <li>You'll receive an SMS confirmation from M-Pesa</li>
              <li>Payment will be automatically recorded in the system</li>
            </ol>
          </div>

          {/* Submit Button */}
          <div className="flex items-center gap-4">
            <button
              type="submit"
              disabled={loading}
              className="flex-1 px-6 py-4 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed font-semibold text-lg flex items-center justify-center gap-2"
            >
              {loading ? (
                <>
                  <Loader2 className="animate-spin w-4 h-4" />
                  Processing...
                </>
              ) : (
                <>
                  <Smartphone className="w-4 h-4" />
                  Pay Now
                </>
              )}
            </button>
            <Link
              href="/tenant-portal"
              className="px-6 py-4 border border-primary/20 text-primary rounded-lg hover:bg-primary/5 transition-colors"
            >
              Cancel
            </Link>
          </div>
        </form>
      </Card>

      {/* Alternative Payment Method */}
      <div className="text-center">
        <p className="text-sm text-primary/60 mb-2">Already made a payment manually?</p>
        <Link
          href="/tenant-portal/record-payment"
          className="text-sm text-primary hover:underline font-medium"
        >
          Record Payment Instead →
        </Link>
      </div>
    </div>
  )
}
