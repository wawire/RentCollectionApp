import { apiClient } from './api'

export const reportService = {
  async downloadMonthlyReport(year: number, month: number): Promise<Blob> {
    const response = await apiClient.get(`/reports/monthly-report/${year}/${month}`, {
      responseType: 'blob',
    })
    return response.data
  },

  async downloadTenantList(): Promise<Blob> {
    const response = await apiClient.get('/reports/tenant-list', {
      responseType: 'blob',
    })
    return response.data
  },

  async previewMonthlyReport(year: number, month: number): Promise<Blob> {
    const response = await apiClient.get(`/reports/monthly-report/${year}/${month}/preview`, {
      responseType: 'blob',
    })
    return response.data
  },

  async previewTenantList(): Promise<Blob> {
    const response = await apiClient.get('/reports/tenant-list/preview', {
      responseType: 'blob',
    })
    return response.data
  },
}
