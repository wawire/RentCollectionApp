import { apiClient } from './api'

export interface TenantDashboardDto {
  tenantInfo: {
    id: number
    fullName: string
    email: string
    phoneNumber: string
    propertyName: string
    unitNumber: string
    monthlyRent: number
    leaseStartDate: string
    leaseEndDate?: string
  }
  currentBalance: number
  nextPaymentDueDate?: string
  nextPaymentAmount?: number
  daysUntilDue?: number
  hasOverduePayments: boolean
  overdueAmount: number
  daysOverdue: number
  totalPaymentsMade: number
  totalAmountPaid: number
  recentPayments: RecentPayment[]
  pendingPayments: RecentPayment[]
  documentCount: number
  leaseExpiryDate?: string
  daysUntilLeaseExpiry?: number
  lateFeePolicy: string
}

export interface RecentPayment {
  id: number
  amount: number
  lateFeeAmount: number
  totalAmount: number
  paymentDate: string
  dueDate: string
  paymentMethod: string
  status: string
  transactionReference?: string
  isLate: boolean
  daysOverdue: number
}

export interface TenantLeaseInfoDto {
  tenant: {
    fullName: string
    email: string
    phoneNumber: string
    idNumber?: string
  }
  property: {
    name: string
    address?: string
    city?: string
    description?: string
    amenities: string[]
  }
  unit: {
    unitNumber: string
    type: string
    bedrooms: number
    bathrooms: number
    squareFeet: number
    description?: string
  }
  lease: {
    startDate: string
    endDate?: string
    monthlyRent: number
    securityDeposit?: number
    rentDueDay: number
    lateFeeGracePeriodDays: number
    lateFeePercentage: number
    lateFeeFixedAmount?: number
    lateFeePolicy: string
    isActive: boolean
    daysUntilExpiry?: number
  }
  paymentAccount: {
    accountType: string
    accountName: string
    accountNumber?: string
    bankName?: string
    mpesaPaybill?: string
    paymentAccountNumber?: string
    instructions: string
  }
}

export const tenantPortalService = {
  // Get tenant dashboard
  getDashboard: async (): Promise<TenantDashboardDto> => {
    const response = await apiClient.get<{ data: TenantDashboardDto }>('/tenantportal/dashboard')
    return response.data.data
  },

  // Get lease information
  getLeaseInfo: async (): Promise<TenantLeaseInfoDto> => {
    const response = await apiClient.get<{ data: TenantLeaseInfoDto }>('/tenantportal/lease-info')
    return response.data.data
  }
}
