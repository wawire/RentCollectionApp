import { apiClient } from './api'
import { UnmatchedPayment, UnmatchedPaymentStatus } from '../types/unmatchedPayment.types'

export interface ResolveUnmatchedPaymentPayload {
  tenantId: number
  landlordAccountId?: number
  periodStart: string
  periodEnd: string
  paymentDate?: string
  notes?: string
}

export const unmatchedPaymentService = {
  async getAll(status?: UnmatchedPaymentStatus): Promise<UnmatchedPayment[]> {
    const response = await apiClient.get('/unmatchedpayments', {
      params: { status },
    })
    return response.data.data
  },

  async updateStatus(id: number, status: UnmatchedPaymentStatus): Promise<UnmatchedPayment> {
    const response = await apiClient.patch(`/unmatchedpayments/${id}/status`, { status })
    return response.data.data
  },

  async resolve(id: number, payload: ResolveUnmatchedPaymentPayload): Promise<UnmatchedPayment> {
    const response = await apiClient.post(`/unmatchedpayments/${id}/resolve`, payload)
    return response.data.data
  },
}
