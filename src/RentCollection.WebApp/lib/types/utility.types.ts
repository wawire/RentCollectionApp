export type UtilityBillingMode = 'Fixed' | 'Metered' | 'Shared'

export interface UtilityType {
  id: number
  name: string
  billingMode: UtilityBillingMode
  unitOfMeasure?: string
  isActive: boolean
  description?: string
}

export interface UtilityConfig {
  id: number
  utilityTypeId: number
  utilityTypeName: string
  propertyId: number
  propertyName: string
  unitId?: number
  unitNumber?: string
  billingMode: UtilityBillingMode
  fixedAmount?: number
  rate?: number
  sharedAmount?: number
  isActive: boolean
  effectiveFrom: string
  effectiveTo?: string
  notes?: string
}

export interface CreateUtilityTypeDto {
  name: string
  billingMode: UtilityBillingMode
  unitOfMeasure?: string
  description?: string
}

export interface CreateUtilityConfigDto {
  utilityTypeId: number
  propertyId: number
  unitId?: number
  billingMode: UtilityBillingMode
  fixedAmount?: number
  rate?: number
  sharedAmount?: number
  effectiveFrom: string
  effectiveTo?: string
  notes?: string
}

export interface UpdateUtilityConfigDto {
  billingMode: UtilityBillingMode
  fixedAmount?: number
  rate?: number
  sharedAmount?: number
  isActive: boolean
  effectiveTo?: string
  notes?: string
}

export interface MeterReading {
  id: number
  utilityConfigId: number
  unitId: number
  unitNumber: string
  utilityName: string
  readingDate: string
  readingValue: number
  photoUrl?: string
  notes?: string
}

export interface CreateMeterReadingDto {
  utilityConfigId: number
  unitId: number
  readingDate: string
  readingValue: number
  photoUrl?: string
  notes?: string
}
