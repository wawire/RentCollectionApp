export enum MaintenanceRequestStatus {
  Pending = 0,
  Assigned = 1,
  InProgress = 2,
  Completed = 3,
  Cancelled = 4,
}

export enum MaintenancePriority {
  Low = 0,
  Medium = 1,
  High = 2,
  Emergency = 3,
}

export interface MaintenanceRequest {
  id: number
  tenantId: number
  tenantName: string
  tenantPhone: string
  unitId: number
  unitNumber: string
  propertyId: number
  propertyName: string
  title: string
  description: string
  priority: MaintenancePriority
  priorityName: string
  status: MaintenanceRequestStatus
  statusName: string
  photoUrls: string[]
  assignedToUserId?: number
  assignedToName?: string
  estimatedCost?: number
  actualCost?: number
  createdAt: string
  completedAt?: string
  notes?: string
  daysSinceCreated: number
  isOverdue: boolean
}

export interface CreateMaintenanceRequestDto {
  title: string
  description: string
  priority: MaintenancePriority
  photos?: File[]
}

export interface UpdateMaintenanceRequestDto {
  status?: MaintenanceRequestStatus
  priority?: MaintenancePriority
  assignedToUserId?: number
  estimatedCost?: number
  actualCost?: number
  notes?: string
}

export interface AssignMaintenanceRequestDto {
  caretakerId: number
}

export interface CompleteMaintenanceRequestDto {
  actualCost: number
  notes?: string
}
