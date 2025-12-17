export enum PaymentAccountType {
  MPesaPaybill = 'MPesaPaybill',
  MPesaTillNumber = 'MPesaTillNumber',
  MPesaPhone = 'MPesaPhone',
  BankAccount = 'BankAccount',
  Cash = 'Cash',
}

export interface LandlordPaymentAccount {
  id: number
  landlordId: number
  propertyId?: number
  propertyName?: string
  accountName: string
  accountType: PaymentAccountType

  // M-Pesa Paybill
  paybillNumber?: string
  paybillName?: string

  // M-Pesa Till Number
  tillNumber?: string

  // M-Pesa Phone
  mPesaPhoneNumber?: string

  // Bank Details
  bankName?: string
  bankAccountNumber?: string
  bankAccountName?: string
  bankBranch?: string
  swiftCode?: string

  // Settings
  isDefault: boolean
  isActive: boolean
  autoReconciliation: boolean

  paymentInstructions?: string

  createdAt: string
  updatedAt?: string
}

export interface CreateLandlordPaymentAccountDto {
  propertyId?: number
  accountName: string
  accountType: PaymentAccountType

  // M-Pesa Paybill Details
  paybillNumber?: string
  paybillName?: string

  // M-Pesa Till Number
  tillNumber?: string

  // M-Pesa Phone Number
  mPesaPhoneNumber?: string

  // Bank Details
  bankName?: string
  bankAccountNumber?: string
  bankAccountName?: string
  bankBranch?: string
  swiftCode?: string

  // M-Pesa API Credentials (for STK Push)
  mPesaConsumerKey?: string
  mPesaConsumerSecret?: string
  mPesaShortCode?: string
  mPesaPasskey?: string

  // Settings
  isDefault?: boolean
  isActive?: boolean
  autoReconciliation?: boolean

  paymentInstructions?: string
}

export interface UpdateLandlordPaymentAccountDto {
  accountName: string
  accountType: PaymentAccountType

  // M-Pesa Paybill Details
  paybillNumber?: string
  paybillName?: string

  // M-Pesa Till Number
  tillNumber?: string

  // M-Pesa Phone Number
  mPesaPhoneNumber?: string

  // Bank Details
  bankName?: string
  bankAccountNumber?: string
  bankAccountName?: string
  bankBranch?: string
  swiftCode?: string

  // M-Pesa API Credentials
  mPesaConsumerKey?: string
  mPesaConsumerSecret?: string
  mPesaShortCode?: string
  mPesaPasskey?: string

  // Settings
  isDefault: boolean
  isActive: boolean
  autoReconciliation: boolean

  paymentInstructions?: string
}
