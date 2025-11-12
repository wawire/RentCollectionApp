export interface SendSmsDto {
  phoneNumber: string
  message: string
  tenantId?: number
}

export interface SmsResponse {
  success: boolean
  message: string
  messageId?: string
}
