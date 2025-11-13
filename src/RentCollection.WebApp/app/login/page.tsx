'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import Link from 'next/link'
import { useAuth } from '@/lib/contexts'
import { useToast } from '@/components/common'
import { FaUser, FaLock, FaSpinner } from 'react-icons/fa'

export default function LoginPage() {
  const router = useRouter()
  const { login } = useAuth()
  const { success, error: showError } = useToast()
  const [formData, setFormData] = useState({
    username: '',
    password: '',
  })
  const [isSubmitting, setIsSubmitting] = useState(false)

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    })
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    if (!formData.username || !formData.password) {
      showError('Please enter both username and password')
      return
    }

    setIsSubmitting(true)

    try {
      const result = await login(formData)

      if (result) {
        success('Login successful! Redirecting...')
        router.push('/dashboard')
      } else {
        showError('Invalid username or password')
      }
    } catch (err) {
      showError('Login failed. Please try again.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100 px-4">
      <div className="max-w-md w-full">
        <div className="bg-white shadow-2xl rounded-2xl p-8">
          {/* Header */}
          <div className="text-center mb-8">
            <div className="inline-flex items-center justify-center w-16 h-16 bg-blue-600 rounded-full mb-4">
              <FaUser className="text-white text-2xl" />
            </div>
            <h1 className="text-3xl font-bold text-gray-900">Welcome Back</h1>
            <p className="text-gray-600 mt-2">Sign in to your RentPro account</p>
          </div>

          {/* Form */}
          <form onSubmit={handleSubmit} className="space-y-6">
            {/* Username */}
            <div>
              <label htmlFor="username" className="block text-sm font-medium text-gray-700 mb-2">
                Username
              </label>
              <div className="relative">
                <FaUser className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" />
                <input
                  type="text"
                  id="username"
                  name="username"
                  value={formData.username}
                  onChange={handleChange}
                  className="pl-10 w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                  placeholder="Enter your username"
                  required
                  disabled={isSubmitting}
                />
              </div>
            </div>

            {/* Password */}
            <div>
              <label htmlFor="password" className="block text-sm font-medium text-gray-700 mb-2">
                Password
              </label>
              <div className="relative">
                <FaLock className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" />
                <input
                  type="password"
                  id="password"
                  name="password"
                  value={formData.password}
                  onChange={handleChange}
                  className="pl-10 w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                  placeholder="Enter your password"
                  required
                  disabled={isSubmitting}
                />
              </div>
            </div>

            {/* Submit Button */}
            <button
              type="submit"
              disabled={isSubmitting}
              className="w-full bg-blue-600 text-white py-3 px-4 rounded-lg font-semibold hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed transition-all flex items-center justify-center"
            >
              {isSubmitting ? (
                <>
                  <FaSpinner className="animate-spin mr-2" />
                  Signing in...
                </>
              ) : (
                'Sign In'
              )}
            </button>
          </form>

          {/* Footer */}
          <div className="mt-6 text-center">
            <p className="text-sm text-gray-600">
              Don't have an account?{' '}
              <Link href="/register" className="text-blue-600 hover:text-blue-700 font-semibold">
                Sign up
              </Link>
            </p>
          </div>
        </div>

        {/* Info Box */}
        <div className="mt-6 bg-white rounded-lg p-4 shadow-md">
          <p className="text-sm text-gray-600 text-center">
            <strong>Demo Accounts:</strong>
            <br />
            Admin: admin / password
            <br />
            Manager: manager / password
          </p>
        </div>
      </div>
    </div>
  )
}
