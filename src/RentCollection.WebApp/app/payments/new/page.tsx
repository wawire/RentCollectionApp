'use client'

import { Suspense } from 'react'
import { useRouter, useSearchParams } from 'next/navigation'
import { Card } from '@/components/common'
import PaymentForm from '@/components/payments/PaymentForm'
import { useCreatePayment } from '@/lib/hooks'
import { CreatePaymentDto } from '@/lib/types'
import Link from 'next/link'
import { FaArrowLeft } from 'react-icons/fa'

function NewPaymentContent() {
  const router = useRouter()
  const searchParams = useSearchParams()
  const preselectedTenantId = searchParams.get('tenantId') ? Number(searchParams.get('tenantId')) : undefined

  const { createPayment, loading, error } = useCreatePayment()

  const handleSubmit = async (data: CreatePaymentDto) => {
    const payment = await createPayment(data)
    if (payment) {
      router.push('/payments')
    }
  }

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      {/* Header */}
      <div>
        <Link
          href="/payments"
          className="inline-flex items-center text-sm text-gray-600 hover:text-gray-900 mb-4"
        >
          <FaArrowLeft className="mr-2" />
          Back to Payments
        </Link>
        <h1 className="text-3xl font-bold text-gray-900">Record Payment</h1>
        <p className="text-gray-600 mt-1">Record a new rent payment</p>
      </div>

      {/* Form */}
      <Card padding="lg">
        {error && (
          <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm">
            {error.message}
          </div>
        )}
        <PaymentForm onSubmit={handleSubmit} loading={loading} preselectedTenantId={preselectedTenantId} />
      </Card>
    </div>
  )
}

export default function NewPaymentPage() {
  return (
    <Suspense fallback={<div>Loading...</div>}>
      <NewPaymentContent />
    </Suspense>
  )
}
