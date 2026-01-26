'use client'

import { Suspense } from 'react'
import { useRouter, useSearchParams } from 'next/navigation'
import { Card } from '@/components/common'
import TenantForm from '@/components/tenants/TenantForm'
import { useCreateTenant } from '@/lib/hooks'
import { CreateTenantDto } from '@/lib/types'
import Link from 'next/link'
import { ArrowLeft } from 'lucide-react'

function NewTenantContent() {
  const router = useRouter()
  const searchParams = useSearchParams()
  const preselectedUnitId = searchParams.get('unitId') ? Number(searchParams.get('unitId')) : undefined

  const { createTenant, loading, error } = useCreateTenant()

  const handleSubmit = async (data: CreateTenantDto) => {
    const tenant = await createTenant(data)
    if (tenant) {
      router.push('/tenants')
    }
  }

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      {/* Header */}
      <div>
        <Link
          href="/tenants"
          className="inline-flex items-center text-sm text-primary/60 hover:text-primary mb-4 tracking-wide"
        >
          <ArrowLeft className="mr-2 w-4 h-4" />
          Back to Tenants
        </Link>
        <h1 className="text-3xl font-serif font-normal text-primary tracking-wide">Add New Tenant</h1>
        <p className="text-primary/60 mt-2 tracking-wide">Create a new tenant profile</p>
      </div>

      {/* Form */}
      <Card padding="lg">
        {error && (
          <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm">
            {error.message}
          </div>
        )}
        <TenantForm onSubmit={handleSubmit} loading={loading} preselectedUnitId={preselectedUnitId} />
      </Card>
    </div>
  )
}

export default function NewTenantPage() {
  return (
    <Suspense fallback={<div>Loading...</div>}>
      <NewTenantContent />
    </Suspense>
  )
}
