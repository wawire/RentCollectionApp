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
  imageUrl?: string
  isOccupied: boolean
  isActive: boolean
  rentalType: number
  currentTenantName?: string
  createdAt: string

  // Payment Status Information
  lastPaymentDate?: string
  lastPaymentAmount?: number
  paymentStatus?: 'Paid' | 'Overdue' | 'Pending' | 'NoTenant'
  daysOverdue?: number
  currentTenantId?: number
}

export interface CreateUnitDto {
  unitNumber: string
  propertyId: number
  monthlyRent: number
  bedrooms: number
  bathrooms: number
  squareFeet?: number
  description?: string
  imageUrl?: string
  rentalType: number
}

export interface UpdateUnitDto {
  unitNumber: string
  monthlyRent: number
  bedrooms: number
  bathrooms: number
  squareFeet?: number
  description?: string
  imageUrl?: string
  isOccupied: boolean
  isActive: boolean
  rentalType: number
}
