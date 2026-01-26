// Enums
export enum MoveOutInspectionStatus {
  Scheduled = 1,
  InProgress = 2,
  Completed = 3,
  Reviewed = 4,
  DisputeRaised = 5,
  Settled = 6,
  RefundProcessed = 7,
}

export enum InspectionCategory {
  Walls = 1,
  Floors = 2,
  Ceiling = 3,
  Doors = 4,
  Windows = 5,
  Kitchen = 6,
  Bathroom = 7,
  Fixtures = 8,
  Appliances = 9,
  Plumbing = 10,
  Electrical = 11,
  Locks = 12,
  Keys = 13,
  Yard = 14,
  Other = 99,
}

export enum PhotoType {
  MoveIn = 1,
  MoveOut = 2,
  Damage = 3,
  Before = 4,
  After = 5,
}

// Main Inspection Type
export interface MoveOutInspection {
  id: number;
  tenantId: number;
  tenantName: string;
  unitId: number;
  unitNumber: string;
  propertyId: number;
  propertyName: string;
  moveOutDate: string;
  inspectionDate: string;
  inspectedByUserId: number;
  inspectedByUserName: string;
  status: MoveOutInspectionStatus;
  statusDisplay: string;
  overallCondition: string;
  generalNotes: string;

  // Calculated amounts
  cleaningCharges: number;
  repairCharges: number;
  unpaidRent: number;
  unpaidUtilities: number;
  otherCharges: number;
  totalDeductions: number;

  securityDepositHeld: number;
  refundAmount: number;
  tenantOwes: number;

  // Settlement
  isSettled: boolean;
  settlementDate?: string;
  settlementNotes?: string;

  // Refund tracking
  refundProcessed: boolean;
  refundDate?: string;
  refundMethod?: string;
  refundReference?: string;

  inspectionItems: InspectionItem[];
  photos: InspectionPhoto[];
  createdAt: string;
}

export interface InspectionItem {
  id: number;
  moveOutInspectionId: number;
  category: InspectionCategory;
  categoryDisplay: string;
  itemName: string;
  moveInCondition: string;
  moveOutCondition: string;
  isDamaged: boolean;
  damageDescription?: string;
  estimatedRepairCost: number;
  notes?: string;
  photos: InspectionPhoto[];
}

export interface InspectionPhoto {
  id: number;
  moveOutInspectionId: number;
  inspectionItemId?: number;
  photoUrl: string;
  caption?: string;
  photoType: PhotoType;
  photoTypeDisplay: string;
  takenAt: string;
}

// DTOs
export interface CreateMoveOutInspectionDto {
  tenantId: number;
  moveOutDate: string;
  inspectionDate: string;
  notes?: string;
}

export interface RecordInspectionItemDto {
  category: string;
  itemName: string;
  moveInCondition: string;
  moveOutCondition: string;
  isDamaged: boolean;
  damageDescription?: string;
  estimatedRepairCost: number;
  notes?: string;
}

export interface RecordInspectionDto {
  overallCondition: string;
  generalNotes: string;
  unpaidRent: number;
  unpaidUtilities: number;
  otherCharges: number;
  inspectionItems: RecordInspectionItemDto[];
}

export interface SettleInspectionDto {
  settlementNotes: string;
}

export interface ProcessRefundDto {
  refundMethod: string;
  refundReference?: string;
  notes?: string;
}

export interface UploadPhotoDto {
  photoUrl: string;
  caption?: string;
  photoType: PhotoType;
  inspectionItemId?: number;
}
