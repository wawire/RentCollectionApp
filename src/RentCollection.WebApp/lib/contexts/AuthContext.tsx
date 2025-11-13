'use client'

import React, { createContext, useContext, useState, useEffect, useCallback } from 'react'
import { useRouter } from 'next/navigation'
import { authService } from '../services'
import { User, LoginDto, RegisterDto, AuthContextType } from '../types'

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const router = useRouter()

  // Check if user is authenticated on mount
  useEffect(() => {
    const checkAuth = () => {
      const storedUser = authService.getStoredUser()
      const token = authService.getStoredToken()

      if (storedUser && token) {
        setUser(storedUser)
      }

      setIsLoading(false)
    }

    checkAuth()
  }, [])

  const login = useCallback(
    async (credentials: LoginDto): Promise<boolean> => {
      try {
        setIsLoading(true)
        const authResponse = await authService.login(credentials)

        const userData: User = {
          id: authResponse.id,
          username: authResponse.username,
          email: authResponse.email,
          firstName: authResponse.firstName,
          lastName: authResponse.lastName,
          phoneNumber: '',
          role: authResponse.role as 'Admin' | 'PropertyManager' | 'Viewer',
          isActive: true,
          createdAt: new Date().toISOString(),
        }

        setUser(userData)
        return true
      } catch (error) {
        console.error('Login failed:', error)
        return false
      } finally {
        setIsLoading(false)
      }
    },
    []
  )

  const register = useCallback(
    async (data: RegisterDto): Promise<boolean> => {
      try {
        setIsLoading(true)
        const authResponse = await authService.register(data)

        const userData: User = {
          id: authResponse.id,
          username: authResponse.username,
          email: authResponse.email,
          firstName: authResponse.firstName,
          lastName: authResponse.lastName,
          phoneNumber: data.phoneNumber,
          role: authResponse.role as 'Admin' | 'PropertyManager' | 'Viewer',
          isActive: true,
          createdAt: new Date().toISOString(),
        }

        setUser(userData)
        return true
      } catch (error) {
        console.error('Registration failed:', error)
        return false
      } finally {
        setIsLoading(false)
      }
    },
    []
  )

  const logout = useCallback(async () => {
    try {
      await authService.logout()
    } catch (error) {
      console.error('Logout failed:', error)
    } finally {
      setUser(null)
      router.push('/login')
    }
  }, [router])

  const refreshToken = useCallback(async (): Promise<boolean> => {
    try {
      const authResponse = await authService.refreshToken()

      const userData: User = {
        id: authResponse.id,
        username: authResponse.username,
        email: authResponse.email,
        firstName: authResponse.firstName,
        lastName: authResponse.lastName,
        phoneNumber: '',
        role: authResponse.role as 'Admin' | 'PropertyManager' | 'Viewer',
        isActive: true,
        createdAt: new Date().toISOString(),
      }

      setUser(userData)
      return true
    } catch (error) {
      console.error('Token refresh failed:', error)
      await logout()
      return false
    }
  }, [logout])

  const value: AuthContextType = {
    user,
    isAuthenticated: !!user,
    isLoading,
    login,
    register,
    logout,
    refreshToken,
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}
