export interface Property {
  id: number
  name: string
  location: string
  description?: string
  imageUrl?: string
  totalUnits: number
  occupiedUnits: number
  vacantUnits: number
  isActive: boolean
  createdAt: string
}

export interface CreatePropertyDto {
  name: string
  location: string
  description?: string
  imageUrl?: string
  totalUnits: number
}

export interface UpdatePropertyDto {
  name: string
  location: string
  description?: string
  imageUrl?: string
  totalUnits: number
  isActive: boolean
}
