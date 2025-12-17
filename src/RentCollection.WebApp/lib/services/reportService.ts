import { apiClient } from './api'

export interface PropertyProfitLoss {
  propertyId: number
  propertyName: string
  rentCollected: number
  rentExpected: number
  collectionRate: number
  lateFees: number
  totalIncome: number
  expenses: number
  netProfit: number
  totalUnits: number
  occupiedUnits: number
  occupancyRate: number
}

export interface ProfitLossReport {
  startDate: string
  endDate: string
  period: string
  totalRentCollected: number
  totalRentExpected: number
  collectionRate: number
  securityDepositsReceived: number
  lateFees: number
  totalIncome: number
  maintenanceExpenses: number
  utilitiesExpenses: number
  propertyManagementFees: number
  taxesAndInsurance: number
  otherExpenses: number
  totalExpenses: number
  expensesByCategory: Record<string, number>
  netProfit: number
  profitMargin: number
  propertiesBreakdown: PropertyProfitLoss[]
}

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

  async getProfitLossReport(startDate: string, endDate: string): Promise<ProfitLossReport> {
    const response = await apiClient.get<{ data: ProfitLossReport }>(
      '/reports/profit-loss',
      { params: { startDate, endDate } }
    )
    return response.data.data
  },
}
