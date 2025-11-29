'use client'

import { useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { useAuth } from '@/contexts/AuthContext'
import { UserRole } from '@/lib/types/auth.types'

interface ProtectedRouteProps {
  children: React.ReactNode
  allowedRoles?: UserRole[]
}

export default function ProtectedRoute({ children, allowedRoles }: ProtectedRouteProps) {
  const { user, isAuthenticated, isLoading } = useAuth()
  const router = useRouter()

  useEffect(() => {
    if (!isLoading) {
      if (!isAuthenticated) {
        router.push('/login')
        return
      }

      // Check if user has required role
      if (allowedRoles && allowedRoles.length > 0 && user) {
        if (!allowedRoles.includes(user.role)) {
          router.push('/unauthorized')
        }
      }
    }
  }, [isAuthenticated, isLoading, user, allowedRoles, router])

  // Show loading state
  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="text-center">
          <div className="animate-spin rounded-full h-16 w-16 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">Loading...</p>
        </div>
      </div>
    )
  }

  // Show nothing while redirecting
  if (!isAuthenticated) {
    return null
  }

  // Check role authorization
  if (allowedRoles && allowedRoles.length > 0 && user) {
    if (!allowedRoles.includes(user.role)) {
      return null
    }
  }

  return <>{children}</>
}
