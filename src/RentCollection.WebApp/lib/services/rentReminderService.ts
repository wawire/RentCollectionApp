import { apiClient } from './api'

// Types/Interfaces
export interface ReminderSettings {
  id: number
  landlordId: number
  isEnabled: boolean
  defaultChannel: 'SMS' | 'Email' | 'Both'
  sevenDaysBeforeEnabled: boolean
  threeDaysBeforeEnabled: boolean
  oneDayBeforeEnabled: boolean
  onDueDateEnabled: boolean
  oneDayOverdueEnabled: boolean
  threeDaysOverdueEnabled: boolean
  sevenDaysOverdueEnabled: boolean
  sevenDaysBeforeTemplate?: string
  threeDaysBeforeTemplate?: string
  oneDayBeforeTemplate?: string
  onDueDateTemplate?: string
  oneDayOverdueTemplate?: string
  threeDaysOverdueTemplate?: string
  sevenDaysOverdueTemplate?: string
  quietHoursStart?: string  // TimeSpan as string
  quietHoursEnd?: string    // TimeSpan as string
}

export interface UpdateReminderSettings {
  isEnabled: boolean
  defaultChannel: 'SMS' | 'Email' | 'Both'
  sevenDaysBeforeEnabled: boolean
  threeDaysBeforeEnabled: boolean
  oneDayBeforeEnabled: boolean
  onDueDateEnabled: boolean
  oneDayOverdueEnabled: boolean
  threeDaysOverdueEnabled: boolean
  sevenDaysOverdueEnabled: boolean
  sevenDaysBeforeTemplate?: string
  threeDaysBeforeTemplate?: string
  oneDayBeforeTemplate?: string
  onDueDateTemplate?: string
  oneDayOverdueTemplate?: string
  threeDaysOverdueTemplate?: string
  sevenDaysOverdueTemplate?: string
  quietHoursStart?: string
  quietHoursEnd?: string
}

export interface RentReminder {
  id: number
  tenantId: number
  tenantName: string
  tenantPhone: string
  tenantEmail: string
  propertyId: number
  propertyName: string
  unitId: number
  unitNumber: string
  reminderType: string
  reminderTypeDisplay: string
  channel: string
  channelDisplay: string
  status: string
  statusDisplay: string
  scheduledDate: string
  sentDate?: string
  rentDueDate: string
  rentAmount: number
  messageSent: string
  failureReason?: string
  retryCount: number
  createdAt: string
}

export interface ReminderStatistics {
  totalReminders: number
  scheduledReminders: number
  sentReminders: number
  failedReminders: number
  successRate: number
  remindersByType: Record<string, number>
  remindersByStatus: Record<string, number>
}

export const rentReminderService = {
  // Get reminder settings for current landlord
  getSettings: async () => {
    const response = await apiClient.get<{ success: boolean; data: ReminderSettings }>('/rentreminders/settings')
    return response.data
  },

  // Update reminder settings
  updateSettings: async (settings: UpdateReminderSettings) => {
    const response = await apiClient.put<{ success: boolean; data: ReminderSettings; message: string }>(
      '/rentreminders/settings',
      settings
    )
    return response.data
  },

  // Get reminder history
  getHistory: async (startDate?: string, endDate?: string) => {
    const response = await apiClient.get<{ success: boolean; data: RentReminder[]; count: number }>(
      '/rentreminders/history',
      { params: { startDate, endDate } }
    )
    return response.data
  },

  // Get reminder statistics
  getStatistics: async (startDate?: string, endDate?: string) => {
    const response = await apiClient.get<{ success: boolean; data: ReminderStatistics }>(
      '/rentreminders/statistics',
      { params: { startDate, endDate } }
    )
    return response.data
  },

  // Get reminders for specific tenant
  getTenantReminders: async (tenantId: number) => {
    const response = await apiClient.get<{ success: boolean; data: RentReminder[]; count: number }>(
      `/rentreminders/tenant/${tenantId}`
    )
    return response.data
  },

  // Send reminder manually
  sendReminder: async (reminderId: number) => {
    const response = await apiClient.post<{ success: boolean; message: string }>(
      `/rentreminders/${reminderId}/send`
    )
    return response.data
  },

  // Cancel reminder
  cancelReminder: async (reminderId: number) => {
    const response = await apiClient.delete<{ success: boolean; message: string }>(
      `/rentreminders/${reminderId}`
    )
    return response.data
  },

  // Schedule reminders for specific tenant
  scheduleTenantReminders: async (tenantId: number) => {
    const response = await apiClient.post<{ success: boolean; message: string }>(
      `/rentreminders/schedule/tenant/${tenantId}`
    )
    return response.data
  },

  // Schedule reminders for all tenants (admin only)
  scheduleAllReminders: async () => {
    const response = await apiClient.post<{ success: boolean; message: string }>(
      '/rentreminders/schedule/all'
    )
    return response.data
  },

  // Update tenant preferences
  updateTenantPreferences: async (
    tenantId: number,
    remindersEnabled: boolean,
    preferredChannel: 'SMS' | 'Email' | 'Both'
  ) => {
    const response = await apiClient.put<{ success: boolean; message: string }>(
      `/rentreminders/preferences/tenant/${tenantId}`,
      null,
      { params: { remindersEnabled, preferredChannel } }
    )
    return response.data
  }
}
