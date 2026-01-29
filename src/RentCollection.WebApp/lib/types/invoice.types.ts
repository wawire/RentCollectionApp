export enum InvoiceStatus {
  Draft = 1,
  Issued = 2,
  PartiallyPaid = 3,
  Paid = 4,
  Overdue = 5,
  Void = 6,
}

export interface InvoiceLineItem {
  id: number
  lineItemType: string
  description: string
  quantity: number
  rate: number
  amount: number
  unitOfMeasure?: string
  utilityTypeId?: number
  utilityName?: string
}

export interface Invoice {
  id: number
  tenantId: number
  tenantName: string
  unitId: number
  unitNumber: string
  propertyId: number
  propertyName: string
  landlordId: number
  periodStart: string
  periodEnd: string
  dueDate: string
  amount: number
  openingBalance: number
  balance: number
  status: InvoiceStatus
  lineItems?: InvoiceLineItem[]
}
