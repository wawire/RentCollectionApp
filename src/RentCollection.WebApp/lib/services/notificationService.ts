import { apiClient } from './api'

export const notificationService = {
  // Send payment reminder to specific tenant
  sendPaymentReminder: async (tenantId: number, daysBeforeDue: number = 3) => {
    const response = await apiClient.post<{ message: string }>(
      `/notifications/payment-reminder/tenant/${tenantId}`,
      null,
      { params: { daysBeforeDue } }
    )
    return response.data
  },

  // Send bulk payment reminders
  sendBulkPaymentReminders: async (daysBeforeDue: number = 3, landlordId?: number) => {
    const response = await apiClient.post<{ data: number; message: string }>(
      '/notifications/payment-reminder/bulk',
      null,
      { params: { daysBeforeDue, landlordId } }
    )
    return response.data
  },

  // Send overdue notice to specific tenant
  sendOverdueNotice: async (tenantId: number) => {
    const response = await apiClient.post<{ message: string }>(
      `/notifications/overdue-notice/tenant/${tenantId}`
    )
    return response.data
  },

  // Send bulk overdue notices
  sendBulkOverdueNotices: async (landlordId?: number) => {
    const response = await apiClient.post<{ data: number; message: string }>(
      '/notifications/overdue-notice/bulk',
      null,
      { params: { landlordId } }
    )
    return response.data
  },

  // Send payment receipt
  sendPaymentReceipt: async (paymentId: number) => {
    const response = await apiClient.post<{ message: string }>(
      `/notifications/payment-receipt/${paymentId}`
    )
    return response.data
  }
}
