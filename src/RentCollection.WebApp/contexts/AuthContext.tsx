'use client'

import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react'
import { useRouter } from 'next/navigation'
import { authService } from '@/lib/services/authService'
import { AuthResponse, LoginRequest, UserRole } from '@/lib/types/auth.types'

interface AuthContextType {
  user: AuthResponse | null
  login: (credentials: LoginRequest) => Promise<void>
  logout: () => void
  refreshUser: () => void
  isAuthenticated: boolean
  isLoading: boolean
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthResponse | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const router = useRouter()

  const refreshUser = () => {
    const currentUser = authService.getCurrentUser()

    if (currentUser && !authService.isTokenExpired()) {
      setUser(currentUser)
    } else {
      authService.logout()
      setUser(null)
    }
  }

  useEffect(() => {
    refreshUser()

    setIsLoading(false)
  }, [])

  const login = async (credentials: LoginRequest) => {
    const authData = await authService.login(credentials)
    setUser(authData)

    // Redirect based on role
    if (!authData.isVerified) {
      router.push('/verify')
      return
    }

    if (authData.mustChangePassword && authData.role === UserRole.Tenant) {
      router.push('/tenant-portal/setup-password')
      return
    }

    if (authData.role === UserRole.Tenant) {
      router.push('/tenant-portal')
    } else {
      router.push('/dashboard')
    }
  }

  const logout = () => {
    authService.logout()
    setUser(null)
    router.push('/login')
  }

  return (
    <AuthContext.Provider
      value={{
        user,
        login,
        logout,
        refreshUser,
        isAuthenticated: !!user && !authService.isTokenExpired(),
        isLoading,
      }}
    >
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}
