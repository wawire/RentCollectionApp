import { apiClient } from './api'
import {
  PaymentInstructions,
  RecordPaymentDto,
  StkPushRequest,
  StkPushResponse,
  TenantPaymentHistory,
} from '../types/tenantPayment.types'

export const tenantPaymentService = {
  /**
   * Get payment instructions for the authenticated tenant
   */
  async getPaymentInstructions(): Promise<PaymentInstructions> {
    const response = await apiClient.get('/TenantPayments/instructions')
    return response.data.data
  },

  /**
   * Record a payment made by the tenant
   */
  async recordPayment(data: RecordPaymentDto): Promise<TenantPaymentHistory> {
    const response = await apiClient.post('/TenantPayments/record', data)
    return response.data.data
  },

  /**
   * Get payment history for the authenticated tenant
   */
  async getPaymentHistory(): Promise<TenantPaymentHistory[]> {
    const response = await apiClient.get('/TenantPayments/history')
    return response.data.data
  },

  /**
   * Initiate M-Pesa STK Push for rent payment
   */
  async initiateStkPush(data: StkPushRequest): Promise<StkPushResponse> {
    const response = await apiClient.post('/TenantPayments/stk-push', data)
    return response.data.data
  },
}
