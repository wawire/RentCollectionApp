export interface Unit {
  id: number
  unitNumber: string
  propertyId: number
  propertyName: string
  monthlyRent: number
  bedrooms: number
  bathrooms: number
  squareFeet?: number
  description?: string
  isOccupied: boolean
  isActive: boolean
  currentTenantName?: string
  createdAt: string
}

export interface CreateUnitDto {
  unitNumber: string
  propertyId: number
  monthlyRent: number
  bedrooms: number
  bathrooms: number
  squareFeet?: number
  description?: string
}

export interface UpdateUnitDto {
  unitNumber: string
  monthlyRent: number
  bedrooms: number
  bathrooms: number
  squareFeet?: number
  description?: string
  isOccupied: boolean
  isActive: boolean
}
