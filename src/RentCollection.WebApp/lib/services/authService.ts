import { apiClient } from './api'
import {
  LoginRequest,
  RegisterRequest,
  AuthResponse,
  User,
  ChangePasswordRequest,
} from '../types/auth.types'

const AUTH_TOKEN_KEY = 'authToken'
const AUTH_USER_KEY = 'authUser'

export const authService = {
  // Login
  async login(credentials: LoginRequest): Promise<AuthResponse> {
    const response = await apiClient.post<AuthResponse>('/auth/login', credentials)
    const authData = response.data

    // Store token and user data
    if (typeof window !== 'undefined') {
      localStorage.setItem(AUTH_TOKEN_KEY, authData.token)
      localStorage.setItem(AUTH_USER_KEY, JSON.stringify(authData))
    }

    return authData
  },

  // Register
  async register(userData: RegisterRequest): Promise<AuthResponse> {
    const response = await apiClient.post<AuthResponse>('/auth/register', userData)
    const authData = response.data

    // Store token and user data
    if (typeof window !== 'undefined') {
      localStorage.setItem(AUTH_TOKEN_KEY, authData.token)
      localStorage.setItem(AUTH_USER_KEY, JSON.stringify(authData))
    }

    return authData
  },

  // Logout
  logout(): void {
    if (typeof window !== 'undefined') {
      localStorage.removeItem(AUTH_TOKEN_KEY)
      localStorage.removeItem(AUTH_USER_KEY)
    }
  },

  // Get current user from localStorage
  getCurrentUser(): AuthResponse | null {
    if (typeof window !== 'undefined') {
      const userStr = localStorage.getItem(AUTH_USER_KEY)
      if (userStr) {
        try {
          return JSON.parse(userStr)
        } catch {
          return null
        }
      }
    }
    return null
  },

  // Get current user from API
  async getMe(): Promise<User> {
    const response = await apiClient.get<User>('/auth/me')
    return response.data
  },

  // Get token
  getToken(): string | null {
    if (typeof window !== 'undefined') {
      return localStorage.getItem(AUTH_TOKEN_KEY)
    }
    return null
  },

  // Check if user is authenticated
  isAuthenticated(): boolean {
    return !!this.getToken()
  },

  // Change password
  async changePassword(passwords: ChangePasswordRequest): Promise<void> {
    await apiClient.post('/auth/change-password', passwords)
  },

  async completePasswordChange(newPassword: string, confirmPassword: string): Promise<AuthResponse> {
    const response = await apiClient.post<AuthResponse>('/auth/complete-password-change', {
      newPassword,
      confirmPassword,
    })
    const authData = response.data

    if (typeof window !== 'undefined') {
      localStorage.setItem(AUTH_TOKEN_KEY, authData.token)
      localStorage.setItem(AUTH_USER_KEY, JSON.stringify(authData))
    }

    return authData
  },

  async sendVerificationOtp(channel: 'Email' | 'Phone'): Promise<void> {
    await apiClient.post('/auth/verification/send', { channel })
  },

  async verifyOtp(code: string): Promise<AuthResponse> {
    const response = await apiClient.post<AuthResponse>('/auth/verification/verify', { code })
    const authData = response.data

    if (typeof window !== 'undefined') {
      localStorage.setItem(AUTH_TOKEN_KEY, authData.token)
      localStorage.setItem(AUTH_USER_KEY, JSON.stringify(authData))
    }

    return authData
  },

  // Check if token is expired
  isTokenExpired(): boolean {
    const user = this.getCurrentUser()
    if (!user || !user.expiresAt) return true

    const expiryTime = new Date(user.expiresAt).getTime()
    const currentTime = new Date().getTime()

    return currentTime >= expiryTime
  },
}
