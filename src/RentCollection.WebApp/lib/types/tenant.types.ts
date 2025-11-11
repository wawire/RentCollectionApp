export interface Tenant {
  id: number
  firstName: string
  lastName: string
  fullName: string
  email: string
  phoneNumber: string
  idNumber?: string
  unitId: number
  unitNumber: string
  propertyName: string
  leaseStartDate: string
  leaseEndDate?: string
  monthlyRent: number
  securityDeposit?: number
  isActive: boolean
  notes?: string
  createdAt: string
}

export interface CreateTenantDto {
  firstName: string
  lastName: string
  email: string
  phoneNumber: string
  idNumber?: string
  unitId: number
  leaseStartDate: string
  leaseEndDate?: string
  monthlyRent: number
  securityDeposit?: number
  notes?: string
}
