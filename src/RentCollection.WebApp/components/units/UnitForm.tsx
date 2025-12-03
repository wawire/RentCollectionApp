'use client'

import { useState, useEffect } from 'react'
import { CreateUnitDto, UpdateUnitDto } from '@/lib/types'
import { Button, Input, Select } from '@/components/common'
import { useGetProperties } from '@/lib/hooks'

interface UnitFormProps {
  initialData?: UpdateUnitDto & { id?: number; propertyId?: number }
  onSubmit: (data: CreateUnitDto | UpdateUnitDto) => Promise<void>
  loading?: boolean
  preselectedPropertyId?: number
}

export default function UnitForm({ initialData, onSubmit, loading, preselectedPropertyId }: UnitFormProps) {
  const { data: properties } = useGetProperties()

  const [formData, setFormData] = useState({
    unitNumber: initialData?.unitNumber || '',
    propertyId: initialData?.propertyId || preselectedPropertyId || 0,
    monthlyRent: initialData?.monthlyRent || 0,
    bedrooms: initialData?.bedrooms || 1,
    bathrooms: initialData?.bathrooms || 1,
    squareFeet: initialData?.squareFeet || undefined,
    description: initialData?.description || '',
    rentalType: initialData?.rentalType || 1,
    isOccupied: initialData?.isOccupied || false,
    isActive: initialData?.isActive !== undefined ? initialData.isActive : true,
  })

  const [errors, setErrors] = useState<Record<string, string>>({})

  const validate = () => {
    const newErrors: Record<string, string> = {}

    if (!formData.unitNumber.trim()) {
      newErrors.unitNumber = 'Unit number is required'
    }
    if (!formData.propertyId || formData.propertyId === 0) {
      newErrors.propertyId = 'Property is required'
    }
    if (formData.monthlyRent <= 0) {
      newErrors.monthlyRent = 'Monthly rent must be greater than 0'
    }
    if (formData.bedrooms < 0) {
      newErrors.bedrooms = 'Bedrooms cannot be negative'
    }
    if (formData.bathrooms < 0) {
      newErrors.bathrooms = 'Bathrooms cannot be negative'
    }

    setErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (validate()) {
      const submitData = { ...formData }
      if (!submitData.squareFeet || submitData.squareFeet === 0) {
        delete submitData.squareFeet
      }
      await onSubmit(submitData)
    }
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <Select
        label="Property"
        value={formData.propertyId}
        onChange={(e) => setFormData({ ...formData, propertyId: Number(e.target.value) })}
        options={properties.map(p => ({ value: p.id, label: p.name }))}
        placeholder="Select a property"
        error={errors.propertyId}
        required
        fullWidth
      />

      <Input
        label="Unit Number"
        value={formData.unitNumber}
        onChange={(e) => setFormData({ ...formData, unitNumber: e.target.value })}
        error={errors.unitNumber}
        placeholder="e.g., A101, 12B"
        required
        fullWidth
      />

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
          label="Square Feet (Optional)"
          type="number"
          value={formData.squareFeet || ''}
          onChange={(e) => setFormData({ ...formData, squareFeet: e.target.value ? Number(e.target.value) : undefined })}
          placeholder="e.g., 850"
          fullWidth
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <Input
          label="Bedrooms"
          type="number"
          value={formData.bedrooms}
          onChange={(e) => setFormData({ ...formData, bedrooms: Number(e.target.value) })}
          error={errors.bedrooms}
          required
          fullWidth
        />

        <Input
          label="Bathrooms"
          type="number"
          value={formData.bathrooms}
          onChange={(e) => setFormData({ ...formData, bathrooms: Number(e.target.value) })}
          error={errors.bathrooms}
          required
          fullWidth
        />
      </div>

      <Select
        label="Rental Type"
        value={formData.rentalType}
        onChange={(e) => setFormData({ ...formData, rentalType: Number(e.target.value) })}
        options={[
          { value: 1, label: 'For Rent Only' },
          { value: 2, label: 'For Lease Only' },
          { value: 3, label: 'For Rent or Lease' }
        ]}
        required
        fullWidth
      />

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Description
        </label>
        <textarea
          value={formData.description}
          onChange={(e) => setFormData({ ...formData, description: e.target.value })}
          rows={3}
          className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
          placeholder="Enter unit description..."
        />
      </div>

      {initialData && (
        <div className="flex items-center space-x-4">
          <label className="flex items-center space-x-2">
            <input
              type="checkbox"
              checked={formData.isOccupied}
              onChange={(e) => setFormData({ ...formData, isOccupied: e.target.checked })}
              className="rounded border-gray-300 text-primary-600 focus:ring-primary-500"
            />
            <span className="text-sm text-gray-700">Unit is occupied</span>
          </label>

          <label className="flex items-center space-x-2">
            <input
              type="checkbox"
              checked={formData.isActive}
              onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
              className="rounded border-gray-300 text-primary-600 focus:ring-primary-500"
            />
            <span className="text-sm text-gray-700">Unit is active</span>
          </label>
        </div>
      )}

      <Button type="submit" variant="primary" loading={loading} fullWidth>
        {initialData ? 'Update Unit' : 'Create Unit'}
      </Button>
    </form>
  )
}
