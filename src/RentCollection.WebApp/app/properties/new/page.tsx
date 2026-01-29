'use client'

import { useRouter } from 'next/navigation'
import { Card } from '@/components/common'
import PropertyForm from '@/components/properties/PropertyForm'
import { useCreateProperty } from '@/lib/hooks'
import { CreatePropertyDto } from '@/lib/types'
import Link from 'next/link'
import { ArrowLeft } from 'lucide-react'

export default function NewPropertyPage() {
  const router = useRouter()
  const { createProperty, loading, error } = useCreateProperty()

  const handleSubmit = async (data: CreatePropertyDto) => {
    const property = await createProperty(data)
    if (property) {
      router.push('/properties')
    }
  }

  return (
    <div className="max-w-2xl mx-auto space-y-6">
      {/* Header */}
      <div>
        <Link
          href="/properties"
          className="inline-flex items-center text-sm text-gray-600 hover:text-gray-900 mb-4"
        >
          <ArrowLeft className="mr-2" />
          Back to Properties
        </Link>
        <h1 className="text-3xl font-bold text-gray-900">Add New Property</h1>
        <p className="text-gray-600 mt-1">Create a new rental property</p>
      </div>

      {/* Form */}
      <Card padding="lg">
        {error && (
          <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm">
            {error.message}
          </div>
        )}
        <PropertyForm onSubmit={handleSubmit} loading={loading} />
      </Card>
    </div>
  )
}
