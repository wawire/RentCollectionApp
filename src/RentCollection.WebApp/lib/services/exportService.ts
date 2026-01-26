import { apiClient } from './api'

export const exportService = {
  async exportPayments(params: { propertyId?: number; startDate?: string; endDate?: string }) {
    const response = await apiClient.get('/exports/payments', {
      params,
      responseType: 'blob',
    })
    return response.data as Blob
  },

  async exportInvoices(params: { propertyId?: number; startDate?: string; endDate?: string }) {
    const response = await apiClient.get('/exports/invoices', {
      params,
      responseType: 'blob',
    })
    return response.data as Blob
  },

  async exportExpenses(params: { propertyId?: number; startDate?: string; endDate?: string }) {
    const response = await apiClient.get('/exports/expenses', {
      params,
      responseType: 'blob',
    })
    return response.data as Blob
  },

  async exportArrears(params: { propertyId?: number }) {
    const response = await apiClient.get('/exports/arrears', {
      params,
      responseType: 'blob',
    })
    return response.data as Blob
  },
}
