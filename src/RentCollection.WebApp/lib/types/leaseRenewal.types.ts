export enum LeaseRenewalStatus {
  Pending = 0,
  TenantAccepted = 1,
  TenantRejected = 2,
  LandlordApproved = 3,
  LandlordRejected = 4,
  Completed = 5,
  Cancelled = 6,
}

export interface LeaseRenewal {
  id: number
  tenantId: number
  tenantName: string
  tenantEmail: string
  tenantPhone: string
  unitId: number
  unitNumber: string
  propertyId: number
  propertyName: string
  currentLeaseEndDate: string
  proposedLeaseEndDate: string
  currentRentAmount: number
  proposedRentAmount: number
  rentIncreasePercentage?: number
  rentIncrease: number
  status: LeaseRenewalStatus
  statusName: string
  landlordTerms?: string
  rejectionReason?: string
  tenantResponseDate?: string
  landlordResponseDate?: string
  completedDate?: string
  notes?: string
  createdAt: string
  daysUntilExpiry: number
  isExpiringSoon: boolean
}

export interface CreateLeaseRenewalDto {
  tenantId: number
  proposedLeaseEndDate: string
  proposedRentAmount: number
  landlordTerms?: string
  notes?: string
}

export interface UpdateLeaseRenewalDto {
  proposedLeaseEndDate?: string
  proposedRentAmount?: number
  landlordTerms?: string
  notes?: string
}

export interface TenantResponseDto {
  accept: boolean
  rejectionReason?: string
}

export interface RejectLeaseRenewalDto {
  reason: string
}
