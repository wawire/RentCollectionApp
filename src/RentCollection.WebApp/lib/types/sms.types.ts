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

export interface SmsTemplate {
  id: string
  name: string
  content: string
  variables: string[] // e.g., ['tenantName', 'unitNumber', 'amount']
}

export interface SmsHistoryItem {
  id: string
  recipientName: string
  phoneNumber: string
  message: string
  status: 'sent' | 'failed' | 'pending'
  sentAt: string
  tenantId?: number
}

export interface BulkSmsDto {
  tenantIds: number[]
  message: string
}
