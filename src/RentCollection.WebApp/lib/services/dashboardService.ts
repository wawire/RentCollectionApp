import { apiClient } from './api'
import { DashboardStats, MonthlyReport } from '../types/dashboard.types'

export const dashboardService = {
  async getStats(): Promise<DashboardStats> {
    const response = await apiClient.get('/dashboard/stats')
    return response.data.data
  },

  async getMonthlyReport(year: number, month: number): Promise<MonthlyReport> {
    const response = await apiClient.get('/dashboard/monthly-report', {
      params: { year, month },
    })
    return response.data.data
  },

  async getYearlyReports(year: number): Promise<MonthlyReport[]> {
    const response = await apiClient.get('/dashboard/yearly-reports', {
      params: { year },
    })
    return response.data.data
  },
}
