'use client'

import { useState } from 'react'
import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { tenantPaymentService } from '@/lib/services/tenantPaymentService'
import { RecordPaymentDto } from '@/lib/types/tenantPayment.types'
import { FaArrowLeft, FaCheckCircle, FaExclamationCircle } from 'react-icons/fa'

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
      // Convert date strings to ISO format with time
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
          <div className="inline-flex items-center justify-center w-16 h-16 bg-green-100 rounded-full mb-4">
            <FaCheckCircle className="text-green-600 text-3xl" />
          </div>
          <h2 className="text-2xl font-serif font-normal text-primary tracking-wide mb-2">
            Payment Recorded Successfully!
          </h2>
          <p className="text-primary/60 mb-4">
            Your payment has been submitted and is awaiting landlord confirmation.
          </p>
          <p className="text-sm text-primary/60">Redirecting to dashboard...</p>
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-6 max-w-2xl mx-auto">
      {/* Header */}
      <div>
        <Link
          href="/tenant-portal"
          className="inline-flex items-center gap-2 text-primary/60 hover:text-primary transition-colors mb-4"
        >
          <FaArrowLeft />
          Back to Dashboard
        </Link>
        <h1 className="text-3xl font-serif font-normal text-primary tracking-wide">
          Record Payment
        </h1>
        <p className="text-primary/60 mt-2 tracking-wide">
          Submit your payment details for landlord verification
        </p>
      </div>

      {error && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4 flex items-start gap-3">
          <FaExclamationCircle className="text-red-600 text-xl flex-shrink-0 mt-0.5" />
          <div>
            <p className="font-medium text-red-800">Error</p>
            <p className="text-red-700 text-sm">{error}</p>
          </div>
        </div>
      )}

      {/* Form */}
      <form onSubmit={handleSubmit} className="bg-white rounded-lg shadow-sm border border-primary/10 p-6 space-y-6">
        {/* Payment Details */}
        <div>
          <h2 className="text-lg font-serif font-normal text-primary tracking-wide mb-4">
            Payment Details
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label htmlFor="amount" className="block text-sm font-medium text-primary/80 mb-1">
                Amount (KSh) <span className="text-red-500">*</span>
              </label>
              <input
                type="number"
                id="amount"
                name="amount"
                value={formData.amount || ''}
                onChange={handleChange}
                required
                min="0"
                step="0.01"
                className="w-full px-4 py-2 border border-primary/20 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/50"
                placeholder="12000"
              />
            </div>

            <div>
              <label htmlFor="paymentDate" className="block text-sm font-medium text-primary/80 mb-1">
                Payment Date <span className="text-red-500">*</span>
              </label>
              <input
                type="date"
                id="paymentDate"
                name="paymentDate"
                value={formData.paymentDate}
                onChange={handleChange}
                required
                max={new Date().toISOString().split('T')[0]}
                className="w-full px-4 py-2 border border-primary/20 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/50"
              />
            </div>

            <div>
              <label htmlFor="paymentMethod" className="block text-sm font-medium text-primary/80 mb-1">
                Payment Method <span className="text-red-500">*</span>
              </label>
              <select
                id="paymentMethod"
                name="paymentMethod"
                value={formData.paymentMethod}
                onChange={handleChange}
                required
                className="w-full px-4 py-2 border border-primary/20 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/50"
              >
                <option value="MPesa">M-Pesa</option>
                <option value="BankTransfer">Bank Transfer</option>
                <option value="Cash">Cash</option>
                <option value="Cheque">Cheque</option>
              </select>
            </div>

            <div>
              <label
                htmlFor="transactionReference"
                className="block text-sm font-medium text-primary/80 mb-1"
              >
                Transaction Reference <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                id="transactionReference"
                name="transactionReference"
                value={formData.transactionReference}
                onChange={handleChange}
                required
                className="w-full px-4 py-2 border border-primary/20 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/50"
                placeholder="SKG8N9Q2RT"
              />
              <p className="text-xs text-primary/60 mt-1">
                M-Pesa code from SMS or bank transaction ID
              </p>
            </div>

            {formData.paymentMethod === 'MPesa' && (
              <div className="md:col-span-2">
                <label
                  htmlFor="mPesaPhoneNumber"
                  className="block text-sm font-medium text-primary/80 mb-1"
                >
                  M-Pesa Phone Number
                </label>
                <input
                  type="tel"
                  id="mPesaPhoneNumber"
                  name="mPesaPhoneNumber"
                  value={formData.mPesaPhoneNumber}
                  onChange={handleChange}
                  className="w-full px-4 py-2 border border-primary/20 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/50"
                  placeholder="0722123456 or +254722123456"
                />
                <p className="text-xs text-primary/60 mt-1">
                  Phone number used for M-Pesa payment
                </p>
              </div>
            )}
          </div>
        </div>

        {/* Payment Period */}
        <div>
          <h2 className="text-lg font-serif font-normal text-primary tracking-wide mb-4">
            Payment Period
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label htmlFor="periodStart" className="block text-sm font-medium text-primary/80 mb-1">
                Period Start <span className="text-red-500">*</span>
              </label>
              <input
                type="date"
                id="periodStart"
                name="periodStart"
                value={formData.periodStart}
                onChange={handleChange}
                required
                className="w-full px-4 py-2 border border-primary/20 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/50"
              />
            </div>

            <div>
              <label htmlFor="periodEnd" className="block text-sm font-medium text-primary/80 mb-1">
                Period End <span className="text-red-500">*</span>
              </label>
              <input
                type="date"
                id="periodEnd"
                name="periodEnd"
                value={formData.periodEnd}
                onChange={handleChange}
                required
                className="w-full px-4 py-2 border border-primary/20 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/50"
              />
            </div>
          </div>
          <p className="text-xs text-primary/60 mt-2">
            The rental period this payment covers (e.g., December 1-31, 2025)
          </p>
        </div>

        {/* Notes */}
        <div>
          <label htmlFor="notes" className="block text-sm font-medium text-primary/80 mb-1">
            Notes (Optional)
          </label>
          <textarea
            id="notes"
            name="notes"
            value={formData.notes}
            onChange={handleChange}
            rows={3}
            className="w-full px-4 py-2 border border-primary/20 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/50"
            placeholder="Any additional information about this payment..."
          />
        </div>

        {/* Submit Button */}
        <div className="flex items-center gap-4">
          <button
            type="submit"
            disabled={loading}
            className="px-6 py-3 bg-primary text-white rounded-lg hover:bg-primary/90 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {loading ? 'Submitting...' : 'Submit Payment'}
          </button>
          <Link
            href="/tenant-portal"
            className="px-6 py-3 border border-primary/20 text-primary rounded-lg hover:bg-primary/5 transition-colors"
          >
            Cancel
          </Link>
        </div>

        {/* Info Box */}
        <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
          <p className="text-sm font-medium text-primary mb-2">What happens next?</p>
          <ul className="text-sm text-primary/80 space-y-1">
            <li>• Your payment will be marked as "Pending"</li>
            <li>• Landlord will verify the transaction with their payment records</li>
            <li>• Once verified, your payment status will change to "Completed"</li>
            <li>• You'll receive a notification when your payment is confirmed</li>
          </ul>
        </div>
      </form>
    </div>
  )
}
