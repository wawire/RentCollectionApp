import { apiClient } from './api'
import { Invoice } from '../types/invoice.types'

export const invoiceService = {
  async getAll(propertyId?: number, startDate?: string, endDate?: string): Promise<Invoice[]> {
    const response = await apiClient.get('/invoices', {
      params: { propertyId, startDate, endDate },
    })
    return response.data.data
  },

  async getById(id: number): Promise<Invoice> {
    const response = await apiClient.get(`/invoices/${id}`)
    return response.data.data
  },

  async getByTenant(tenantId: number): Promise<Invoice[]> {
    const response = await apiClient.get(`/invoices/tenant/${tenantId}`)
    return response.data.data
  },

  async getMine(): Promise<Invoice[]> {
    const response = await apiClient.get('/invoices/me')
    return response.data.data
  },

  async generateMonthly(year: number, month: number) {
    const response = await apiClient.post('/invoices/generate', null, {
      params: { year, month },
    })
    return response.data
  },
}
