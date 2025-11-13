import { apiClient } from './api'
import { Payment, CreatePaymentDto } from '../types/payment.types'

export const paymentService = {
  async getAll(): Promise<Payment[]> {
    const response = await apiClient.get('/payments')
    return response.data.data
  },

  async getById(id: number): Promise<Payment> {
    const response = await apiClient.get(`/payments/${id}`)
    return response.data.data
  },

  async getByTenant(tenantId: number): Promise<Payment[]> {
    const response = await apiClient.get(`/payments/tenant/${tenantId}`)
    return response.data.data
  },

  async getByProperty(propertyId: number): Promise<Payment[]> {
    const response = await apiClient.get(`/payments/property/${propertyId}`)
    return response.data.data
  },

  async getByDateRange(startDate: string, endDate: string): Promise<Payment[]> {
    const response = await apiClient.get('/payments/date-range', {
      params: { startDate, endDate },
    })
    return response.data.data
  },

  async create(data: CreatePaymentDto): Promise<Payment> {
    const response = await apiClient.post('/payments', data)
    return response.data.data
  },

  async delete(id: number): Promise<void> {
    await apiClient.delete(`/payments/${id}`)
  },

  async downloadReceipt(id: number): Promise<Blob> {
    const response = await apiClient.get(`/reports/payment-receipt/${id}`, {
      responseType: 'blob',
    })
    return response.data
  },

  async previewReceipt(id: number): Promise<Blob> {
    const response = await apiClient.get(`/reports/payment-receipt/${id}/preview`, {
      responseType: 'blob',
    })
    return response.data
  },
}
