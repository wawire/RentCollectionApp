// Tenant Payment Portal Types

export interface PaymentInstructions {
  tenantName: string
  unitNumber: string
  propertyName: string
  monthlyRent: number
  landlordName: string
  accountType: string
  paybillNumber?: string
  paybillName?: string
  accountNumber?: string
  bankName?: string
  bankAccountNumber?: string
  bankAccountName?: string
  bankBranch?: string
  swiftCode?: string
  paymentInstructions: string
}

export interface RecordPaymentDto {
  amount: number
  paymentDate: string
  paymentMethod: 'MPesa' | 'BankTransfer' | 'Cash' | 'Cheque'
  transactionReference: string
  mPesaPhoneNumber?: string
  notes?: string
  periodStart: string
  periodEnd: string
}

export interface StkPushRequest {
  amount: number
  phoneNumber: string
  accountReference: string
  transactionDesc?: string
}

export interface StkPushResponse {
  merchantRequestId: string
  checkoutRequestId: string
  responseCode: string
  responseDescription: string
  customerMessage: string
}

export interface TenantPaymentHistory {
  id: number
  amount: number
  paymentDate: string
  paymentMethod: string
  status: 'Pending' | 'Completed' | 'Rejected' | 'Failed'
  transactionReference?: string
  confirmedAt?: string
  confirmedByUserName?: string
  notes?: string
  periodStart?: string
  periodEnd?: string
}

export interface TenantPaymentStats {
  totalPaid: number
  pendingAmount: number
  lastPaymentDate?: string
  lastPaymentAmount?: number
  upcomingDueDate?: string
}
