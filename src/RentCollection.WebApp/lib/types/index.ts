// Properties
export type { Property, CreatePropertyDto, UpdatePropertyDto } from './property.types'

// Tenants
export type { Tenant, CreateTenantDto } from './tenant.types'

// Units
export type { Unit, CreateUnitDto, UpdateUnitDto } from './unit.types'

// Payments
export type { Payment, CreatePaymentDto, StkPushStatusResponse } from './payment.types'
export { PaymentMethod, PaymentStatus } from './payment.types'

// Invoices
export type { Invoice } from './invoice.types'
export { InvoiceStatus } from './invoice.types'

// Unmatched Payments
export type { UnmatchedPayment } from './unmatchedPayment.types'
export { UnmatchedPaymentStatus } from './unmatchedPayment.types'

// Dashboard
export type { DashboardStats, MonthlyReport } from './dashboard.types'

// SMS
export type { SendSmsDto, SmsResponse, SmsTemplate, SmsHistoryItem, BulkSmsDto } from './sms.types'

// Maintenance Requests
export type {
  MaintenanceRequest,
  CreateMaintenanceRequestDto,
  UpdateMaintenanceRequestDto,
  AssignMaintenanceRequestDto,
  CompleteMaintenanceRequestDto,
} from './maintenanceRequest.types'
export { MaintenanceRequestStatus, MaintenancePriority } from './maintenanceRequest.types'

// Lease Renewals
export type {
  LeaseRenewal,
  CreateLeaseRenewalDto,
  UpdateLeaseRenewalDto,
  TenantResponseDto,
  RejectLeaseRenewalDto,
} from './leaseRenewal.types'
export { LeaseRenewalStatus } from './leaseRenewal.types'

// Security Deposits
export type {
  SecurityDepositTransaction,
  SecurityDepositBalance,
  RecordSecurityDepositDto,
  DeductSecurityDepositDto,
  RefundSecurityDepositDto,
} from './securityDeposit.types'
export { SecurityDepositTransactionType } from './securityDeposit.types'

// Move-Out Inspections
export type {
  MoveOutInspection,
  InspectionItem,
  InspectionPhoto,
  CreateMoveOutInspectionDto,
  RecordInspectionDto,
  RecordInspectionItemDto,
  SettleInspectionDto,
  ProcessRefundDto,
  UploadPhotoDto,
} from './moveOutInspection.types'
export { MoveOutInspectionStatus, InspectionCategory, PhotoType } from './moveOutInspection.types'

// Utilities
export type {
  UtilityType,
  UtilityConfig,
  CreateUtilityTypeDto,
  CreateUtilityConfigDto,
  UpdateUtilityConfigDto,
  MeterReading,
  CreateMeterReadingDto,
} from './utility.types'

// Common
export type {
  ApiResponse,
  PaginatedResponse,
  PaginationParams,
  ApiError,
  SelectOption,
} from './common.types'

// Organizations
export type {
  Organization,
  CreateOrganizationRequest,
  AssignUserToPropertyRequest,
} from './organization.types'
