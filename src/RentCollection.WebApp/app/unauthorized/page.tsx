'use client'

import Link from 'next/link'
import { useAuth } from '@/contexts/AuthContext'
import { UserRole } from '@/lib/types/auth.types'

export default function UnauthorizedPage() {
  const { user } = useAuth()
  const homePath = user?.role === UserRole.Tenant ? '/tenant-portal' : '/dashboard'

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 px-6">
      <div className="max-w-lg text-center bg-white border border-gray-200 rounded-xl p-8 shadow-sm">
        <h1 className="text-3xl font-semibold text-gray-900 mb-3">Access denied</h1>
        <p className="text-gray-600 mb-6">
          You do not have permission to view this page. If you believe this is an error, contact your administrator.
        </p>
        <Link
          href={homePath}
          className="inline-flex items-center justify-center px-5 py-2.5 rounded-lg bg-blue-600 text-white font-medium hover:bg-blue-700 transition-colors"
        >
          Return to dashboard
        </Link>
      </div>
    </div>
  )
}
