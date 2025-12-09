import { apiClient } from './api'

export interface Setup2FAResponse {
  secretKey: string
  qrCodeUri: string
  issuer: string
  accountName: string
}

export const twoFactorAuthService = {
  // Setup 2FA
  setup: async (): Promise<Setup2FAResponse> => {
    const response = await apiClient.post<Setup2FAResponse>('/twofactorauth/setup')
    return response.data
  },

  // Enable 2FA
  enable: async (verificationCode: string): Promise<{ message: string }> => {
    const response = await apiClient.post<{ message: string }>(
      '/twofactorauth/enable',
      { verificationCode }
    )
    return response.data
  },

  // Disable 2FA
  disable: async (password: string): Promise<{ message: string }> => {
    const response = await apiClient.post<{ message: string }>(
      '/twofactorauth/disable',
      { password }
    )
    return response.data
  },

  // Verify 2FA code during login
  verify: async (emailOrPhone: string, code: string): Promise<{ token: string; user: any }> => {
    const response = await apiClient.post<{ token: string; user: any }>(
      '/twofactorauth/verify',
      { emailOrPhone, code }
    )
    return response.data
  }
}
