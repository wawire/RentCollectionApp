export enum PaymentMethod {
  Cash = 1,
  MPesa = 2,
  BankTransfer = 3,
  Cheque = 4,
  Other = 5,
}

export enum PaymentStatus {
  Pending = 1,
  Completed = 2,
  Failed = 3,
  Refunded = 4,
  PartiallyPaid = 5,
}

export interface Payment {
  id: number
  tenantId: number
  tenantName: string
  unitNumber: string
  propertyName: string
  amount: number
  paymentDate: string
  paymentMethod: PaymentMethod
  paymentMethodName: string
  status: PaymentStatus
  statusName: string
  transactionReference?: string
  notes?: string
  periodStart: string
  periodEnd: string
  createdAt: string
}

export interface CreatePaymentDto {
  tenantId: number
  amount: number
  paymentDate: string
  paymentMethod: PaymentMethod
  transactionReference?: string
  notes?: string
  periodStart: string
  periodEnd: string
}
