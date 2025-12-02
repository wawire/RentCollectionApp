// Public listing types (no authentication required)

export interface PublicUnitListing {
  id: number
  unitNumber: string
  bedrooms: number
  bathrooms: number
  squareFeet: number | null
  monthlyRent: number
  description: string | null
  isOccupied: boolean
  propertyId: number
  propertyName: string | null
  propertyLocation: string | null
}

export interface PublicPropertyListing {
  id: number
  name: string
  description: string | null
  location: string | null
  totalUnits: number
  vacantUnits: number
  availableUnits: PublicUnitListing[]
}

export interface TenantApplication {
  firstName: string
  lastName: string
  email: string
  phoneNumber: string
  idNumber?: string
  unitId: number
  leaseStartDate: string
  leaseEndDate?: string
  securityDeposit?: number
  applicationNotes?: string
}

export interface TenantApplicationResponse {
  id: number
  firstName: string
  lastName: string
  email: string
  phoneNumber: string
  idNumber: string | null
  unitId: number
  unitNumber: string | null
  propertyName: string | null
  leaseStartDate: string
  leaseEndDate: string | null
  monthlyRent: number
  securityDeposit: number | null
  status: TenantStatus
  applicationDate: string | null
  approvalDate: string | null
  applicationNotes: string | null
}

export enum TenantStatus {
  Prospective = 0,
  Active = 1,
  Inactive = 2
}
