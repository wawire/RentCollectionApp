export enum UnmatchedPaymentStatus {
  Pending = 1,
  Resolved = 2,
  Ignored = 3,
}

export interface UnmatchedPayment {
  id: number
  transactionReference: string
  amount: number
  accountReference: string
  phoneNumber?: string
  businessShortCode?: string
  correlationId?: string
  reason: string
  landlordId?: number
  propertyId?: number
  resolvedPaymentId?: number
  resolvedByUserId?: number
  resolvedAt?: string
  resolutionNotes?: string
  status: UnmatchedPaymentStatus
  createdAt: string
}
