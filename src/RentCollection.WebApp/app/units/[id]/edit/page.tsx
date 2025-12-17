'use client'

import { useParams, useRouter } from 'next/navigation'
import { Card, LoadingSpinner } from '@/components/common'
import UnitForm from '@/components/units/UnitForm'
import { useGetUnit, useUpdateUnit } from '@/lib/hooks'
import { UpdateUnitDto } from '@/lib/types'
import Link from 'next/link'
import { FaArrowLeft } from 'react-icons/fa'

export default function EditUnitPage() {
  const params = useParams()
  const router = useRouter()
  const unitId = Number(params.id)

  const { data: unit, loading: loadingUnit, error: loadError } = useGetUnit(unitId)
  const { updateUnit, loading: updating, error: updateError } = useUpdateUnit()

  const handleSubmit = async (data: UpdateUnitDto | any) => {
    const updatedUnit = await updateUnit(unitId, data as UpdateUnitDto)
    if (updatedUnit) {
      router.push(`/units/${unitId}`)
    }
  }

  if (loadingUnit) {
    return (
      <div className="flex items-center justify-center h-96">
        <LoadingSpinner size="lg" text="Loading unit..." />
      </div>
    )
  }

  if (loadError || !unit) {
    return (
      <div className="max-w-2xl mx-auto space-y-6">
        <div className="text-center py-12">
          <p className="text-red-600">Error loading unit: {loadError?.message || 'Unit not found'}</p>
          <Link href="/units">
            <button className="mt-4 px-4 py-2 bg-accent text-primary rounded-lg hover:bg-accent-600 transition-colors">
              Back to Units
            </button>
          </Link>
        </div>
      </div>
    )
  }

  return (
    <div className="max-w-2xl mx-auto space-y-6">
      {/* Header */}
      <div>
        <Link
          href={`/units/${unitId}`}
          className="inline-flex items-center text-sm text-primary/60 hover:text-primary mb-4 tracking-wide"
        >
          <FaArrowLeft className="mr-2" />
          Back to Unit Details
        </Link>
        <h1 className="text-3xl font-serif font-normal text-primary tracking-wide">Edit Unit</h1>
        <p className="text-primary/60 mt-2 tracking-wide">Update unit {unit.unitNumber} details</p>
      </div>

      {/* Form */}
      <Card padding="lg">
        {updateError && (
          <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm">
            {updateError.message}
          </div>
        )}
        <UnitForm
          initialData={{
            unitNumber: unit.unitNumber,
            propertyId: unit.propertyId,
            monthlyRent: unit.monthlyRent,
            bedrooms: unit.bedrooms,
            bathrooms: unit.bathrooms,
            squareFeet: unit.squareFeet || undefined,
            description: unit.description || '',
            imageUrl: unit.imageUrl || '',
            rentalType: unit.rentalType,
            isOccupied: unit.isOccupied,
            isActive: unit.isActive,
          }}
          onSubmit={handleSubmit}
          loading={updating}
        />
      </Card>
    </div>
  )
}
