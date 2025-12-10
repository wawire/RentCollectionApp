export enum SecurityDepositTransactionType {
  Initial = 1,
  Deduction = 2,
  Refund = 3,
  Adjustment = 4
}

export interface SecurityDepositTransaction {
  id: number
  tenantId: number
  tenantName: string
  unitNumber: string
  amount: number
  transactionType: SecurityDepositTransactionType
  transactionTypeDisplay: string
  reason?: string
  transactionDate: string
  relatedPaymentId?: number
  relatedMaintenanceRequestId?: number
  receiptUrl?: string
  notes?: string
  createdByUserId: number
  createdByUserName: string
  createdAt: string
}

export interface SecurityDepositBalance {
  tenantId: number
  tenantName: string
  unitNumber: string
  initialDeposit: number
  totalDeductions: number
  totalRefunds: number
  currentBalance: number
  lastTransactionDate?: string
  totalTransactions: number
  recentTransactions: SecurityDepositTransaction[]
}

export interface RecordSecurityDepositDto {
  amount: number
  transactionDate: string
  relatedPaymentId?: number
  notes?: string
}

export interface DeductSecurityDepositDto {
  amount: number
  reason: string
  relatedMaintenanceRequestId?: number
  notes?: string
}

export interface RefundSecurityDepositDto {
  amount: number
  refundDate: string
  refundMethod: string
  transactionReference?: string
  notes?: string
}
