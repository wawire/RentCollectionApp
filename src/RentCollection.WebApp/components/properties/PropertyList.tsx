'use client'

import { useState } from 'react'
import PropertyCard from './PropertyCard'
import { Property } from '@/lib/types'
import { Alert } from '@/components/common'

interface PropertyListProps {
  properties: Property[]
  viewMode: 'grid' | 'list'
  onUpdate: () => void
}

export default function PropertyList({ properties, viewMode, onUpdate }: PropertyListProps) {
  if (properties.length === 0) {
    return (
      <div className="text-center py-12">
        <Alert type="info" message="No properties found. Add your first property to get started!" />
      </div>
    )
  }

  if (viewMode === 'grid') {
    return (
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {properties.map((property) => (
          <PropertyCard key={property.id} property={property} onUpdate={onUpdate} />
        ))}
      </div>
    )
  }

  return (
    <div className="space-y-4">
      {properties.map((property) => (
        <PropertyCard key={property.id} property={property} viewMode="list" onUpdate={onUpdate} />
      ))}
    </div>
  )
}
