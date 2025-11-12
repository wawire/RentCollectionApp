import { apiClient } from './api'
import { SendSmsDto, SmsResponse } from '../types/sms.types'

export const smsService = {
  async sendSms(data: SendSmsDto): Promise<SmsResponse> {
    const response = await apiClient.post('/sms/send', data)
    return response.data.data
  },

  async sendRentReminder(tenantId: number): Promise<SmsResponse> {
    const response = await apiClient.post(`/sms/rent-reminder/${tenantId}`)
    return response.data.data
  },

  async sendPaymentReceipt(paymentId: number): Promise<SmsResponse> {
    const response = await apiClient.post(`/sms/payment-receipt/${paymentId}`)
    return response.data.data
  },
}
