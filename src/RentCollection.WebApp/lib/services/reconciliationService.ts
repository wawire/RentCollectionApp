import { apiClient } from './api'

export const reconciliationService = {
  async allocatePayment(paymentId: number, invoiceId?: number, amount?: number) {
    const response = await apiClient.post(`/reconciliation/payments/${paymentId}/allocate`, {
      invoiceId,
      amount,
    })
    return response.data
  },
}
