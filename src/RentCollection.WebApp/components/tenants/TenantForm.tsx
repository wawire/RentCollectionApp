'use client'

import { useState } from 'react'
import { CreateTenantDto } from '@/lib/types'
import { Button, Input, Select } from '@/components/common'
import { useGetVacantUnits } from '@/lib/hooks'

interface TenantFormProps {
  initialData?: CreateTenantDto & { id?: number }
  onSubmit: (data: CreateTenantDto) => Promise<void>
  loading?: boolean
  preselectedUnitId?: number
}

export default function TenantForm({ initialData, onSubmit, loading, preselectedUnitId }: TenantFormProps) {
  const { data: vacantUnits } = useGetVacantUnits()

  const [formData, setFormData] = useState<CreateTenantDto>({
    firstName: initialData?.firstName || '',
    lastName: initialData?.lastName || '',
    email: initialData?.email || '',
    phoneNumber: initialData?.phoneNumber || '',
    idNumber: initialData?.idNumber || '',
    unitId: initialData?.unitId || preselectedUnitId || 0,
    leaseStartDate: initialData?.leaseStartDate || '',
    leaseEndDate: initialData?.leaseEndDate || '',
    monthlyRent: initialData?.monthlyRent || 0,
    securityDeposit: initialData?.securityDeposit || 0,
    notes: initialData?.notes || '',
  })

  const [errors, setErrors] = useState<Record<string, string>>({})

  const validate = () => {
    const newErrors: Record<string, string> = {}

    if (!formData.firstName.trim()) newErrors.firstName = 'First name is required'
    if (!formData.lastName.trim()) newErrors.lastName = 'Last name is required'
    if (!formData.email.trim()) newErrors.email = 'Email is required'
    if (!formData.phoneNumber.trim()) newErrors.phoneNumber = 'Phone number is required'
    if (!formData.unitId || formData.unitId === 0) newErrors.unitId = 'Unit is required'
    if (!formData.leaseStartDate) newErrors.leaseStartDate = 'Lease start date is required'
    if (formData.monthlyRent <= 0) newErrors.monthlyRent = 'Monthly rent must be greater than 0'

    setErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (validate()) {
      await onSubmit(formData)
    }
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <Input
          label="First Name"
          value={formData.firstName}
          onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
          error={errors.firstName}
          placeholder="e.g., John"
          required
          fullWidth
        />

        <Input
          label="Last Name"
          value={formData.lastName}
          onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
          error={errors.lastName}
          placeholder="e.g., Doe"
          required
          fullWidth
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <Input
          label="Email"
          type="email"
          value={formData.email}
          onChange={(e) => setFormData({ ...formData, email: e.target.value })}
          error={errors.email}
          placeholder="e.g., john@example.com"
          required
          fullWidth
        />

        <Input
          label="Phone Number"
          type="tel"
          value={formData.phoneNumber}
          onChange={(e) => setFormData({ ...formData, phoneNumber: e.target.value })}
          error={errors.phoneNumber}
          placeholder="e.g., +254712345678"
          required
          fullWidth
        />
      </div>

      <Input
        label="ID Number (Optional)"
        value={formData.idNumber}
        onChange={(e) => setFormData({ ...formData, idNumber: e.target.value })}
        placeholder="e.g., 12345678"
        fullWidth
      />

      <Select
        label="Unit"
        value={formData.unitId}
        onChange={(e) => setFormData({ ...formData, unitId: Number(e.target.value) })}
        options={vacantUnits.map(u => ({
          value: u.id,
          label: `${u.propertyName} - ${u.unitNumber} (KSh ${u.monthlyRent.toLocaleString()})`
        }))}
        placeholder="Select a vacant unit"
        error={errors.unitId}
        required
        fullWidth
      />

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <Input
          label="Lease Start Date"
          type="date"
          value={formData.leaseStartDate}
          onChange={(e) => setFormData({ ...formData, leaseStartDate: e.target.value })}
          error={errors.leaseStartDate}
          required
          fullWidth
        />

        <Input
          label="Lease End Date (Optional)"
          type="date"
          value={formData.leaseEndDate}
          onChange={(e) => setFormData({ ...formData, leaseEndDate: e.target.value })}
          fullWidth
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <Input
          label="Monthly Rent (KSh)"
          type="number"
          value={formData.monthlyRent}
          onChange={(e) => setFormData({ ...formData, monthlyRent: Number(e.target.value) })}
          error={errors.monthlyRent}
          placeholder="e.g., 15000"
          required
          fullWidth
        />

        <Input
          label="Security Deposit (KSh)"
          type="number"
          value={formData.securityDeposit}
          onChange={(e) => setFormData({ ...formData, securityDeposit: Number(e.target.value) })}
          placeholder="e.g., 30000"
          fullWidth
        />
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Notes
        </label>
        <textarea
          value={formData.notes}
          onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
          rows={3}
          className="w-full px-2 py-3 border-b-2 border-secondary/30 focus:border-accent outline-none transition-colors bg-transparent text-primary placeholder:text-primary/40 resize-none"
          placeholder="Additional notes about the tenant..."
        />
      </div>

      <Button type="submit" variant="primary" loading={loading} fullWidth>
        {initialData ? 'Update Tenant' : 'Create Tenant'}
      </Button>
    </form>
  )
}
