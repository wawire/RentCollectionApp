'use client'

import { useState, useEffect } from 'react'
import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { useAuth } from '@/contexts/AuthContext'
import { tenantPaymentService } from '@/lib/services/tenantPaymentService'
import { securityDepositService } from '@/lib/services/securityDepositService'
import { FaArrowLeft, FaMobileAlt, FaCheckCircle, FaExclamationCircle, FaSpinner, FaInfoCircle, FaShieldAlt } from 'react-icons/fa'

export default function PaySecurityDepositPage() {
  const router = useRouter()
  const { user } = useAuth()

  const [depositAmount, setDepositAmount] = useState(0)
  const [amountPaid, setAmountPaid] = useState(0)
  const [amountOwed, setAmountOwed] = useState(0)
  const [loadingDepositInfo, setLoadingDepositInfo] = useState(true)

  const [formData, setFormData] = useState({
    amount: '',
    phoneNumber: '',
    accountReference: 'SECURITY_DEPOSIT',
  })

  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState(false)
  const [stkPushSent, setStkPushSent] = useState(false)

  useEffect(() => {
    const loadDepositInfo = async () => {
      if (!user?.tenantId) return

      try {
        const balance = await securityDepositService.getDepositBalance(user.tenantId)
        setDepositAmount(balance.initialDeposit)
        setAmountPaid(balance.currentBalance)
        const owed = balance.initialDeposit - balance.currentBalance
        setAmountOwed(owed)
        setFormData(prev => ({ ...prev, amount: owed.toString() }))
      } catch (err) {
        console.error('Failed to load deposit info:', err)
      } finally {
        setLoadingDepositInfo(false)
      }
    }

    loadDepositInfo()
  }, [user])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setLoading(true)
    setError(null)

    try {
      const response = await tenantPaymentService.initiateStkPush({
        amount: parseFloat(formData.amount),
        phoneNumber: formData.phoneNumber,
        accountReference: formData.accountReference,
        transactionDesc: 'Security Deposit Payment',
      })

      setStkPushSent(true)

      // Simulate waiting for payment confirmation
      setTimeout(() => {
        setSuccess(true)
        setTimeout(() => {
          router.push('/tenant-portal/security-deposit')
        }, 3000)
      }, 5000)

    } catch (err: any) {
      setError(err.response?.data?.message || 'Payment failed. Please try again or contact support.')
    } finally {
      setLoading(false)
    }
  }

  if (loadingDepositInfo) {
    return (
      <div className="flex items-center justify-center h-96">
        <FaSpinner className="animate-spin text-4xl text-blue-600" />
      </div>
    )
  }

  if (success) {
    return (
      <div className="max-w-2xl mx-auto min-h-screen flex items-center justify-center -mt-20">
        <div className="text-center">
          <div className="inline-flex items-center justify-center w-20 h-20 bg-green-100 rounded-full mb-6">
            <FaCheckCircle className="text-green-600 text-4xl" />
          </div>
          <h2 className="text-3xl font-bold text-gray-900 mb-3">
            Payment Successful!
          </h2>
          <p className="text-gray-600 text-lg mb-2">
            Your security deposit payment has been received
          </p>
          <p className="text-sm text-gray-500">Redirecting you back...</p>
        </div>
      </div>
    )
  }

  if (stkPushSent) {
    return (
      <div className="max-w-2xl mx-auto min-h-screen flex items-center justify-center -mt-20">
        <div className="text-center px-6">
          <div className="inline-flex items-center justify-center w-20 h-20 bg-blue-100 rounded-full mb-6 animate-pulse">
            <FaMobileAlt className="text-blue-600 text-4xl" />
          </div>
          <h2 className="text-3xl font-bold text-gray-900 mb-3">
            Check Your Phone
          </h2>
          <p className="text-gray-600 text-lg mb-6">
            We sent a payment request to <strong>{formData.phoneNumber}</strong>
          </p>
          <div className="bg-blue-50 border border-blue-200 rounded-xl p-6 text-left max-w-md mx-auto">
            <p className="font-semibold text-gray-900 mb-3">Follow these steps:</p>
            <ol className="space-y-2 text-gray-700">
              <li className="flex items-start gap-3">
                <span className="flex-shrink-0 w-6 h-6 bg-blue-600 text-white rounded-full flex items-center justify-center text-sm font-medium">1</span>
                <span>Check your phone for an M-Pesa notification</span>
              </li>
              <li className="flex items-start gap-3">
                <span className="flex-shrink-0 w-6 h-6 bg-blue-600 text-white rounded-full flex items-center justify-center text-sm font-medium">2</span>
                <span>Enter your M-Pesa PIN when prompted</span>
              </li>
              <li className="flex items-start gap-3">
                <span className="flex-shrink-0 w-6 h-6 bg-blue-600 text-white rounded-full flex items-center justify-center text-sm font-medium">3</span>
                <span>Confirm the payment amount</span>
              </li>
              <li className="flex items-start gap-3">
                <span className="flex-shrink-0 w-6 h-6 bg-blue-600 text-white rounded-full flex items-center justify-center text-sm font-medium">4</span>
                <span>You'll receive an SMS confirmation</span>
              </li>
            </ol>
          </div>
          <div className="mt-8 flex items-center justify-center gap-2 text-gray-500">
            <FaSpinner className="animate-spin text-blue-600" />
            <span>Waiting for confirmation...</span>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4 mb-8">
        <Link
          href="/tenant-portal/security-deposit"
          className="flex items-center justify-center w-10 h-10 rounded-lg border border-gray-300 hover:bg-gray-50 transition-colors"
        >
          <FaArrowLeft className="text-gray-600" />
        </Link>
        <div>
          <h1 className="text-3xl font-bold text-gray-900">
            Pay Security Deposit
          </h1>
          <p className="text-gray-600 mt-1">
            Quick and secure payment via M-Pesa
          </p>
        </div>
      </div>

      {error && (
        <div className="bg-red-50 border border-red-200 rounded-xl p-6 flex items-start gap-4">
          <FaExclamationCircle className="text-red-600 text-2xl flex-shrink-0 mt-0.5" />
          <div>
            <p className="font-semibold text-red-900 text-lg">Payment Failed</p>
            <p className="text-red-700 mt-1">{error}</p>
            <button
              onClick={() => setError(null)}
              className="mt-3 text-sm font-medium text-red-700 hover:text-red-800 underline"
            >
              Try again
            </button>
          </div>
        </div>
      )}

      {/* Deposit Information */}
      <div className="bg-gradient-to-r from-blue-50 to-indigo-50 border border-blue-200 rounded-xl p-6">
        <div className="flex items-start gap-4">
          <div className="flex-shrink-0 w-12 h-12 bg-blue-600 rounded-xl flex items-center justify-center">
            <FaShieldAlt className="text-white text-xl" />
          </div>
          <div>
            <h3 className="text-lg font-semibold text-gray-900 mb-2">Your Security Deposit Details</h3>
            <div className="grid grid-cols-3 gap-4 text-sm">
              <div>
                <p className="text-gray-600">Required Deposit</p>
                <p className="text-lg font-bold text-gray-900">KSh {depositAmount.toLocaleString()}</p>
              </div>
              <div>
                <p className="text-gray-600">Already Paid</p>
                <p className="text-lg font-bold text-green-600">KSh {amountPaid.toLocaleString()}</p>
              </div>
              <div>
                <p className="text-gray-600">Amount Owed</p>
                <p className="text-lg font-bold text-yellow-600">KSh {amountOwed.toLocaleString()}</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* What is M-Pesa Payment */}
      <div className="bg-gradient-to-r from-green-50 to-emerald-50 border border-green-200 rounded-xl p-6">
        <div className="flex items-start gap-4">
          <div className="flex-shrink-0 w-12 h-12 bg-green-600 rounded-xl flex items-center justify-center">
            <FaInfoCircle className="text-white text-xl" />
          </div>
          <div>
            <h3 className="text-lg font-semibold text-gray-900 mb-2">How M-Pesa Payment Works</h3>
            <p className="text-gray-700 mb-3">
              M-Pesa payment lets you pay your security deposit instantly from your phone - no need to visit a shop or bank!
            </p>
            <ul className="space-y-1 text-sm text-gray-700">
              <li>✓ Payment happens in seconds</li>
              <li>✓ You'll get instant confirmation SMS</li>
              <li>✓ Payment is recorded automatically</li>
              <li>✓ Safe and secure transaction</li>
            </ul>
          </div>
        </div>
      </div>

      {/* Payment Form */}
      <div className="bg-white border border-gray-200 rounded-xl shadow-sm p-8">
        <form onSubmit={handleSubmit} className="space-y-6">
          {/* Amount */}
          <div>
            <label htmlFor="amount" className="block text-sm font-semibold text-gray-900 mb-2">
              How much do you want to pay? <span className="text-red-500">*</span>
            </label>
            <div className="relative">
              <span className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-500 font-medium">
                KSh
              </span>
              <input
                type="number"
                id="amount"
                value={formData.amount}
                onChange={(e) => setFormData({ ...formData, amount: e.target.value })}
                required
                min="1"
                max={amountOwed}
                step="0.01"
                className="w-full pl-16 pr-4 py-4 text-2xl font-semibold border-2 border-gray-300 rounded-xl focus:outline-none focus:border-blue-500 transition-colors"
                placeholder="0"
              />
            </div>
            <p className="text-sm text-gray-500 mt-2">
              Maximum: KSh {amountOwed.toLocaleString()} (remaining balance)
            </p>
          </div>

          {/* Phone Number */}
          <div>
            <label htmlFor="phoneNumber" className="block text-sm font-semibold text-gray-900 mb-2">
              Your M-Pesa Phone Number <span className="text-red-500">*</span>
            </label>
            <input
              type="tel"
              id="phoneNumber"
              value={formData.phoneNumber}
              onChange={(e) => setFormData({ ...formData, phoneNumber: e.target.value })}
              required
              className="w-full px-4 py-4 text-lg border-2 border-gray-300 rounded-xl focus:outline-none focus:border-blue-500 transition-colors"
              placeholder="0722123456 or +254722123456"
            />
            <p className="text-sm text-gray-500 mt-2">
              The phone number you use for M-Pesa transactions
            </p>
          </div>

          {/* Submit Button */}
          <button
            type="submit"
            disabled={loading}
            className="w-full py-5 bg-green-600 text-white rounded-xl hover:bg-green-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed font-semibold text-lg flex items-center justify-center gap-3 shadow-lg shadow-green-600/20"
          >
            {loading ? (
              <>
                <FaSpinner className="animate-spin text-xl" />
                Processing...
              </>
            ) : (
              <>
                <FaMobileAlt className="text-xl" />
                Pay KSh {formData.amount || '0'} Now
              </>
            )}
          </button>
        </form>
      </div>

      {/* Alternative Option */}
      <div className="text-center py-6">
        <p className="text-gray-600 mb-3">Need help with payment?</p>
        <Link
          href="/tenant-portal/payment-instructions"
          className="text-blue-600 hover:text-blue-700 font-medium hover:underline"
        >
          View payment instructions →
        </Link>
      </div>
    </div>
  )
}
