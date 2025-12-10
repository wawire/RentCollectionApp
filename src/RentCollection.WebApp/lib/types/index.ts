// Properties
export type { Property, CreatePropertyDto, UpdatePropertyDto } from './property.types'

// Tenants
export type { Tenant, CreateTenantDto } from './tenant.types'

// Units
export type { Unit, CreateUnitDto, UpdateUnitDto } from './unit.types'

// Payments
export type { Payment, CreatePaymentDto } from './payment.types'
export { PaymentMethod, PaymentStatus } from './payment.types'

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

// Common
export type {
  ApiResponse,
  PaginatedResponse,
  PaginationParams,
  ApiError,
  SelectOption,
} from './common.types'
