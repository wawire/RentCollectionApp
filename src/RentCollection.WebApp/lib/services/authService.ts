import { apiClient } from './api'
import { LoginDto, RegisterDto, AuthResponse, RefreshTokenDto, User } from '../types/auth.types'

export const authService = {
  async login(credentials: LoginDto): Promise<AuthResponse> {
    const response = await apiClient.post('/auth/login', credentials)
    const authData = response.data.data

    // Store tokens in localStorage
    if (typeof window !== 'undefined') {
      localStorage.setItem('authToken', authData.accessToken)
      localStorage.setItem('refreshToken', authData.refreshToken)
      localStorage.setItem('user', JSON.stringify({
        id: authData.id,
        username: authData.username,
        email: authData.email,
        firstName: authData.firstName,
        lastName: authData.lastName,
        role: authData.role,
      }))
    }

    return authData
  },

  async register(data: RegisterDto): Promise<AuthResponse> {
    const response = await apiClient.post('/auth/register', data)
    const authData = response.data.data

    // Store tokens in localStorage
    if (typeof window !== 'undefined') {
      localStorage.setItem('authToken', authData.accessToken)
      localStorage.setItem('refreshToken', authData.refreshToken)
      localStorage.setItem('user', JSON.stringify({
        id: authData.id,
        username: authData.username,
        email: authData.email,
        firstName: authData.firstName,
        lastName: authData.lastName,
        role: authData.role,
      }))
    }

    return authData
  },

  async logout(): Promise<void> {
    try {
      await apiClient.post('/auth/logout')
    } catch (error) {
      console.error('Logout API call failed:', error)
    } finally {
      // Clear tokens from localStorage regardless of API call result
      if (typeof window !== 'undefined') {
        localStorage.removeItem('authToken')
        localStorage.removeItem('refreshToken')
        localStorage.removeItem('user')
      }
    }
  },

  async refreshToken(): Promise<AuthResponse> {
    if (typeof window === 'undefined') {
      throw new Error('Cannot refresh token on server side')
    }

    const refreshToken = localStorage.getItem('refreshToken')
    if (!refreshToken) {
      throw new Error('No refresh token available')
    }

    const response = await apiClient.post('/auth/refresh-token', {
      refreshToken,
    } as RefreshTokenDto)
    const authData = response.data.data

    // Update tokens in localStorage
    localStorage.setItem('authToken', authData.accessToken)
    localStorage.setItem('refreshToken', authData.refreshToken)
    localStorage.setItem('user', JSON.stringify({
      id: authData.id,
      username: authData.username,
      email: authData.email,
      firstName: authData.firstName,
      lastName: authData.lastName,
      role: authData.role,
    }))

    return authData
  },

  async getCurrentUser(): Promise<User> {
    const response = await apiClient.get('/auth/me')
    return response.data.data
  },

  async getAllUsers(): Promise<User[]> {
    const response = await apiClient.get('/auth/users')
    return response.data.data
  },

  // Helper methods for token management
  getStoredToken(): string | null {
    if (typeof window === 'undefined') return null
    return localStorage.getItem('authToken')
  },

  getStoredUser(): User | null {
    if (typeof window === 'undefined') return null
    const userStr = localStorage.getItem('user')
    if (!userStr) return null

    try {
      return JSON.parse(userStr)
    } catch {
      return null
    }
  },

  isAuthenticated(): boolean {
    return !!this.getStoredToken()
  },
}
