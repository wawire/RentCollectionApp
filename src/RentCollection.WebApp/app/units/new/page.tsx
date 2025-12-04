'use client'

import { Suspense } from 'react'
import { useRouter, useSearchParams } from 'next/navigation'
import { Card } from '@/components/common'
import UnitForm from '@/components/units/UnitForm'
import { useCreateUnit } from '@/lib/hooks'
import { CreateUnitDto } from '@/lib/types'
import Link from 'next/link'
import { FaArrowLeft } from 'react-icons/fa'

function NewUnitContent() {
  const router = useRouter()
  const searchParams = useSearchParams()
  const preselectedPropertyId = searchParams.get('propertyId') ? Number(searchParams.get('propertyId')) : undefined

  const { createUnit, loading, error } = useCreateUnit()

  const handleSubmit = async (data: CreateUnitDto | any) => {
    const unit = await createUnit(data as CreateUnitDto)
    if (unit) {
      router.push('/units')
    }
  }

  return (
    <div className="max-w-2xl mx-auto space-y-6">
      {/* Header */}
      <div>
        <Link
          href="/units"
          className="inline-flex items-center text-sm text-primary/60 hover:text-primary mb-4 tracking-wide"
        >
          <FaArrowLeft className="mr-2" />
          Back to Units
        </Link>
        <h1 className="text-3xl font-serif font-normal text-primary tracking-wide">Add New Unit</h1>
        <p className="text-primary/60 mt-2 tracking-wide">Create a new rental unit</p>
      </div>

      {/* Form */}
      <Card padding="lg">
        {error && (
          <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm">
            {error.message}
          </div>
        )}
        <UnitForm onSubmit={handleSubmit} loading={loading} preselectedPropertyId={preselectedPropertyId} />
      </Card>
    </div>
  )
}

export default function NewUnitPage() {
  return (
    <Suspense fallback={<div>Loading...</div>}>
      <NewUnitContent />
    </Suspense>
  )
}
