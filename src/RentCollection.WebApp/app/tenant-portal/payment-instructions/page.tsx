'use client'

import { useEffect, useState } from 'react'
import Link from 'next/link'
import { LoadingSpinner } from '@/components/common'
import { tenantPaymentService } from '@/lib/services/tenantPaymentService'
import { PaymentInstructions } from '@/lib/types/tenantPayment.types'
import { FaArrowLeft, FaMobileAlt, FaUniversity, FaCopy, FaCheckCircle } from 'react-icons/fa'

export default function PaymentInstructionsPage() {
  const [instructions, setInstructions] = useState<PaymentInstructions | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [copiedField, setCopiedField] = useState<string | null>(null)

  useEffect(() => {
    const fetchInstructions = async () => {
      try {
        setLoading(true)
        const data = await tenantPaymentService.getPaymentInstructions()
        setInstructions(data)
        setError(null)
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to load payment instructions')
      } finally {
        setLoading(false)
      }
    }

    fetchInstructions()
  }, [])

  const copyToClipboard = (text: string, fieldName: string) => {
    navigator.clipboard.writeText(text)
    setCopiedField(fieldName)
    setTimeout(() => setCopiedField(null), 2000)
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center h-96">
        <LoadingSpinner size="lg" text="Loading payment instructions..." />
      </div>
    )
  }

  if (error) {
    return (
      <div className="text-center py-12">
        <p className="text-red-600">{error}</p>
        <Link href="/tenant-portal" className="text-primary hover:underline mt-4 inline-block">
          ← Back to Dashboard
        </Link>
      </div>
    )
  }

  if (!instructions) return null

  const isMPesa = instructions.accountType === 'MPesaPaybill'
  const isBankAccount = instructions.accountType === 'BankAccount'

  return (
    <div className="space-y-6 max-w-3xl mx-auto">
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
          Payment Instructions
        </h1>
        <p className="text-primary/60 mt-2 tracking-wide">
          How to pay rent for {instructions.unitNumber} - {instructions.propertyName}
        </p>
      </div>

      {/* Rent Details */}
      <div className="bg-white rounded-lg shadow-sm border border-primary/10 p-6">
        <h2 className="text-xl font-serif font-normal text-primary tracking-wide mb-4">
          Rent Details
        </h2>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <p className="text-sm text-primary/60">Tenant Name</p>
            <p className="text-lg font-medium text-primary mt-1">{instructions.tenantName}</p>
          </div>
          <div>
            <p className="text-sm text-primary/60">Unit Number</p>
            <p className="text-lg font-medium text-primary mt-1">{instructions.unitNumber}</p>
          </div>
          <div>
            <p className="text-sm text-primary/60">Property</p>
            <p className="text-lg font-medium text-primary mt-1">{instructions.propertyName}</p>
          </div>
          <div>
            <p className="text-sm text-primary/60">Monthly Rent</p>
            <p className="text-lg font-medium text-primary mt-1">
              KSh {instructions.monthlyRent.toLocaleString()}
            </p>
          </div>
          <div className="md:col-span-2">
            <p className="text-sm text-primary/60">Landlord</p>
            <p className="text-lg font-medium text-primary mt-1">{instructions.landlordName}</p>
          </div>
        </div>
      </div>

      {/* M-Pesa Instructions */}
      {isMPesa && (
        <div className="bg-gradient-to-br from-green-50 to-green-100 rounded-lg shadow-sm border border-green-200 p-6">
          <div className="flex items-center gap-3 mb-4">
            <div className="p-3 bg-green-600 rounded-lg">
              <FaMobileAlt className="text-white text-2xl" />
            </div>
            <div>
              <h2 className="text-xl font-serif font-normal text-primary tracking-wide">
                M-Pesa Paybill
              </h2>
              <p className="text-sm text-primary/60">Pay via M-Pesa on your phone</p>
            </div>
          </div>

          <div className="bg-white rounded-lg p-4 space-y-4">
            <div>
              <div className="flex items-center justify-between mb-1">
                <p className="text-sm font-medium text-primary/60">Paybill Number</p>
                <button
                  onClick={() =>
                    copyToClipboard(instructions.paybillNumber!, 'paybillNumber')
                  }
                  className="flex items-center gap-1 text-sm text-primary/60 hover:text-primary transition-colors"
                >
                  {copiedField === 'paybillNumber' ? (
                    <>
                      <FaCheckCircle className="text-green-600" /> Copied
                    </>
                  ) : (
                    <>
                      <FaCopy /> Copy
                    </>
                  )}
                </button>
              </div>
              <p className="text-2xl font-bold text-primary">{instructions.paybillNumber}</p>
              <p className="text-sm text-primary/60 mt-1">{instructions.paybillName}</p>
            </div>

            <div>
              <div className="flex items-center justify-between mb-1">
                <p className="text-sm font-medium text-primary/60">Account Number</p>
                <button
                  onClick={() =>
                    copyToClipboard(instructions.accountNumber!, 'accountNumber')
                  }
                  className="flex items-center gap-1 text-sm text-primary/60 hover:text-primary transition-colors"
                >
                  {copiedField === 'accountNumber' ? (
                    <>
                      <FaCheckCircle className="text-green-600" /> Copied
                    </>
                  ) : (
                    <>
                      <FaCopy /> Copy
                    </>
                  )}
                </button>
              </div>
              <p className="text-2xl font-bold text-primary">{instructions.accountNumber}</p>
            </div>

            <div className="bg-green-50 rounded-lg p-4 mt-4">
              <p className="text-sm font-medium text-primary mb-2">Steps to Pay:</p>
              <ol className="list-decimal list-inside space-y-2 text-sm text-primary/80">
                <li>Go to M-Pesa on your phone</li>
                <li>Select Lipa na M-Pesa → Paybill</li>
                <li>Enter Business No: <strong>{instructions.paybillNumber}</strong></li>
                <li>Enter Account No: <strong>{instructions.accountNumber}</strong></li>
                <li>Enter Amount: <strong>KSh {instructions.monthlyRent.toLocaleString()}</strong></li>
                <li>Enter your M-Pesa PIN and confirm</li>
                <li>You will receive a confirmation SMS with a transaction code</li>
              </ol>
            </div>

            {instructions.paymentInstructions && (
              <div className="bg-blue-50 rounded-lg p-4 mt-4">
                <p className="text-sm font-medium text-primary mb-2">Additional Instructions:</p>
                <p className="text-sm text-primary/80">{instructions.paymentInstructions}</p>
              </div>
            )}
          </div>
        </div>
      )}

      {/* Bank Account Instructions */}
      {isBankAccount && (
        <div className="bg-gradient-to-br from-blue-50 to-blue-100 rounded-lg shadow-sm border border-blue-200 p-6">
          <div className="flex items-center gap-3 mb-4">
            <div className="p-3 bg-blue-600 rounded-lg">
              <FaUniversity className="text-white text-2xl" />
            </div>
            <div>
              <h2 className="text-xl font-serif font-normal text-primary tracking-wide">
                Bank Transfer
              </h2>
              <p className="text-sm text-primary/60">Pay via bank transfer</p>
            </div>
          </div>

          <div className="bg-white rounded-lg p-4 space-y-4">
            <div>
              <p className="text-sm font-medium text-primary/60 mb-1">Bank Name</p>
              <p className="text-lg font-bold text-primary">{instructions.bankName}</p>
            </div>

            <div>
              <div className="flex items-center justify-between mb-1">
                <p className="text-sm font-medium text-primary/60">Account Number</p>
                <button
                  onClick={() =>
                    copyToClipboard(instructions.bankAccountNumber!, 'bankAccountNumber')
                  }
                  className="flex items-center gap-1 text-sm text-primary/60 hover:text-primary transition-colors"
                >
                  {copiedField === 'bankAccountNumber' ? (
                    <>
                      <FaCheckCircle className="text-green-600" /> Copied
                    </>
                  ) : (
                    <>
                      <FaCopy /> Copy
                    </>
                  )}
                </button>
              </div>
              <p className="text-2xl font-bold text-primary">{instructions.bankAccountNumber}</p>
            </div>

            <div>
              <p className="text-sm font-medium text-primary/60 mb-1">Account Name</p>
              <p className="text-lg font-medium text-primary">{instructions.bankAccountName}</p>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-sm font-medium text-primary/60 mb-1">Branch</p>
                <p className="text-lg font-medium text-primary">{instructions.bankBranch}</p>
              </div>
              {instructions.swiftCode && (
                <div>
                  <p className="text-sm font-medium text-primary/60 mb-1">SWIFT Code</p>
                  <p className="text-lg font-medium text-primary">{instructions.swiftCode}</p>
                </div>
              )}
            </div>

            {instructions.paymentInstructions && (
              <div className="bg-blue-50 rounded-lg p-4 mt-4">
                <p className="text-sm font-medium text-primary mb-2">Additional Instructions:</p>
                <p className="text-sm text-primary/80">{instructions.paymentInstructions}</p>
              </div>
            )}
          </div>
        </div>
      )}

      {/* Next Steps */}
      <div className="bg-white rounded-lg shadow-sm border border-primary/10 p-6">
        <h2 className="text-xl font-serif font-normal text-primary tracking-wide mb-4">
          After Making Payment
        </h2>
        <div className="space-y-3">
          <div className="flex items-start gap-3">
            <div className="flex-shrink-0 w-6 h-6 bg-primary text-white rounded-full flex items-center justify-center text-sm font-medium">
              1
            </div>
            <p className="text-primary/80">
              Save the transaction reference/code from your M-Pesa SMS or bank receipt
            </p>
          </div>
          <div className="flex items-start gap-3">
            <div className="flex-shrink-0 w-6 h-6 bg-primary text-white rounded-full flex items-center justify-center text-sm font-medium">
              2
            </div>
            <p className="text-primary/80">
              Record your payment by clicking the button below
            </p>
          </div>
          <div className="flex items-start gap-3">
            <div className="flex-shrink-0 w-6 h-6 bg-primary text-white rounded-full flex items-center justify-center text-sm font-medium">
              3
            </div>
            <p className="text-primary/80">
              Your landlord will verify and confirm your payment within 24-48 hours
            </p>
          </div>
        </div>

        <Link
          href="/tenant-portal/record-payment"
          className="inline-flex items-center justify-center px-6 py-3 bg-primary text-white rounded-lg hover:bg-primary/90 transition-colors mt-6 w-full md:w-auto"
        >
          Record Payment
        </Link>
      </div>
    </div>
  )
}
