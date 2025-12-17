'use client'

import { useState } from 'react'
import { CreatePropertyDto, UpdatePropertyDto } from '@/lib/types'
import { Button, Input } from '@/components/common'

interface PropertyFormProps {
  initialData?: UpdatePropertyDto & { id?: number }
  onSubmit: (data: CreatePropertyDto | UpdatePropertyDto) => Promise<void>
  loading?: boolean
}

export default function PropertyForm({ initialData, onSubmit, loading }: PropertyFormProps) {
  const [formData, setFormData] = useState({
    name: initialData?.name || '',
    location: initialData?.location || '',
    description: initialData?.description || '',
    imageUrl: initialData?.imageUrl || '',
    totalUnits: initialData?.totalUnits || 0,
  })

  const [errors, setErrors] = useState<Record<string, string>>({})

  const validate = () => {
    const newErrors: Record<string, string> = {}

    if (!formData.name.trim()) {
      newErrors.name = 'Property name is required'
    }
    if (!formData.location.trim()) {
      newErrors.location = 'Location is required'
    }
    if (formData.totalUnits < 1) {
      newErrors.totalUnits = 'Must have at least 1 unit'
    }

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
      <Input
        label="Property Name"
        value={formData.name}
        onChange={(e) => setFormData({ ...formData, name: e.target.value })}
        error={errors.name}
        placeholder="e.g., Sunset Apartments"
        required
        fullWidth
      />

      <Input
        label="Location"
        value={formData.location}
        onChange={(e) => setFormData({ ...formData, location: e.target.value })}
        error={errors.location}
        placeholder="e.g., Nairobi, Kenya"
        required
        fullWidth
      />

      <Input
        label="Total Units"
        type="number"
        value={formData.totalUnits}
        onChange={(e) => setFormData({ ...formData, totalUnits: Number(e.target.value) })}
        error={errors.totalUnits}
        placeholder="e.g., 10"
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
          rows={4}
          className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
          placeholder="Enter property description..."
        />
      </div>

      <Input
        label="Image URL (Optional)"
        value={formData.imageUrl}
        onChange={(e) => setFormData({ ...formData, imageUrl: e.target.value })}
        placeholder="https://example.com/property-image.jpg"
        fullWidth
      />

      <div className="flex gap-3">
        <Button type="submit" variant="primary" loading={loading} fullWidth>
          {initialData ? 'Update Property' : 'Create Property'}
        </Button>
      </div>
    </form>
  )
}
