'use client'

import { useParams, useRouter } from 'next/navigation'
import { Card, LoadingSpinner } from '@/components/common'
import TenantForm from '@/components/tenants/TenantForm'
import { useGetTenant, useUpdateTenant } from '@/lib/hooks'
import { CreateTenantDto } from '@/lib/types'
import Link from 'next/link'
import { ArrowLeft } from 'lucide-react'

export default function EditTenantPage() {
  const params = useParams()
  const router = useRouter()
  const tenantId = Number(params.id)

  const { data: tenant, loading: loadingTenant, error: loadError } = useGetTenant(tenantId)
  const { updateTenant, loading: updating, error: updateError } = useUpdateTenant()

  const handleSubmit = async (data: CreateTenantDto | any) => {
    const updatedTenant = await updateTenant(tenantId, data as CreateTenantDto)
    if (updatedTenant) {
      router.push(`/tenants/${tenantId}`)
    }
  }

  if (loadingTenant) {
    return (
      <div className="flex items-center justify-center h-96">
        <LoadingSpinner size="lg" text="Loading tenant..." />
      </div>
    )
  }

  if (loadError || !tenant) {
    return (
      <div className="max-w-3xl mx-auto space-y-6">
        <div className="text-center py-12">
          <p className="text-red-600">Error loading tenant: {loadError?.message || 'Tenant not found'}</p>
          <Link href="/tenants">
            <button className="mt-4 px-4 py-2 bg-accent text-primary rounded-lg hover:bg-accent-600 transition-colors">
              Back to Tenants
            </button>
          </Link>
        </div>
      </div>
    )
  }

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      {/* Header */}
      <div>
        <Link
          href={`/tenants/${tenantId}`}
          className="inline-flex items-center text-sm text-primary/60 hover:text-primary mb-4 tracking-wide"
        >
          <ArrowLeft className="mr-2 w-4 h-4" />
          Back to Tenant Details
        </Link>
        <h1 className="text-3xl font-serif font-normal text-primary tracking-wide">Edit Tenant</h1>
        <p className="text-primary/60 mt-2 tracking-wide">Update {tenant.fullName}'s information</p>
      </div>

      {/* Form */}
      <Card padding="lg">
        {updateError && (
          <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm">
            {updateError.message}
          </div>
        )}
        <TenantForm
          initialData={{
            firstName: tenant.firstName,
            lastName: tenant.lastName,
            email: tenant.email,
            phoneNumber: tenant.phoneNumber,
            idNumber: tenant.idNumber || '',
            unitId: tenant.unitId,
            leaseStartDate: tenant.leaseStartDate ? new Date(tenant.leaseStartDate).toISOString().split('T')[0] : '',
            leaseEndDate: tenant.leaseEndDate ? new Date(tenant.leaseEndDate).toISOString().split('T')[0] : '',
            monthlyRent: tenant.monthlyRent,
            securityDeposit: tenant.securityDeposit || 0,
            notes: tenant.notes || '',
          }}
          onSubmit={handleSubmit}
          loading={updating}
        />
      </Card>
    </div>
  )
}
