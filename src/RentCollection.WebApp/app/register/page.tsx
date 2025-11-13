'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import Link from 'next/link'
import { useAuth } from '@/lib/contexts'
import { useToast } from '@/components/common'
import { FaUser, FaEnvelope, FaLock, FaPhone, FaSpinner, FaUserTie } from 'react-icons/fa'

export default function RegisterPage() {
  const router = useRouter()
  const { register } = useAuth()
  const { success, error: showError } = useToast()
  const [formData, setFormData] = useState({
    username: '',
    email: '',
    password: '',
    confirmPassword: '',
    firstName: '',
    lastName: '',
    phoneNumber: '',
    role: 'Viewer' as 'Admin' | 'PropertyManager' | 'Viewer',
  })
  const [isSubmitting, setIsSubmitting] = useState(false)

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    })
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    // Validation
    if (formData.password !== formData.confirmPassword) {
      showError('Passwords do not match')
      return
    }

    if (formData.password.length < 6) {
      showError('Password must be at least 6 characters long')
      return
    }

    setIsSubmitting(true)

    try {
      const { confirmPassword, ...registerData } = formData
      const result = await register(registerData)

      if (result) {
        success('Registration successful! Redirecting...')
        router.push('/dashboard')
      } else {
        showError('Registration failed. Please try again.')
      }
    } catch (err: any) {
      showError(err.message || 'Registration failed. Please try again.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100 px-4 py-8">
      <div className="max-w-2xl w-full">
        <div className="bg-white shadow-2xl rounded-2xl p-8">
          {/* Header */}
          <div className="text-center mb-8">
            <div className="inline-flex items-center justify-center w-16 h-16 bg-blue-600 rounded-full mb-4">
              <FaUserTie className="text-white text-2xl" />
            </div>
            <h1 className="text-3xl font-bold text-gray-900">Create Account</h1>
            <p className="text-gray-600 mt-2">Join RentPro to manage your properties</p>
          </div>

          {/* Form */}
          <form onSubmit={handleSubmit} className="space-y-5">
            {/* Name Row */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {/* First Name */}
              <div>
                <label htmlFor="firstName" className="block text-sm font-medium text-gray-700 mb-2">
                  First Name
                </label>
                <div className="relative">
                  <FaUser className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" />
                  <input
                    type="text"
                    id="firstName"
                    name="firstName"
                    value={formData.firstName}
                    onChange={handleChange}
                    className="pl-10 w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                    placeholder="John"
                    required
                    disabled={isSubmitting}
                  />
                </div>
              </div>

              {/* Last Name */}
              <div>
                <label htmlFor="lastName" className="block text-sm font-medium text-gray-700 mb-2">
                  Last Name
                </label>
                <div className="relative">
                  <FaUser className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" />
                  <input
                    type="text"
                    id="lastName"
                    name="lastName"
                    value={formData.lastName}
                    onChange={handleChange}
                    className="pl-10 w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                    placeholder="Doe"
                    required
                    disabled={isSubmitting}
                  />
                </div>
              </div>
            </div>

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
                  className="pl-10 w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                  placeholder="johndoe"
                  required
                  disabled={isSubmitting}
                />
              </div>
            </div>

            {/* Email and Phone Row */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {/* Email */}
              <div>
                <label htmlFor="email" className="block text-sm font-medium text-gray-700 mb-2">
                  Email
                </label>
                <div className="relative">
                  <FaEnvelope className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" />
                  <input
                    type="email"
                    id="email"
                    name="email"
                    value={formData.email}
                    onChange={handleChange}
                    className="pl-10 w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                    placeholder="john@example.com"
                    required
                    disabled={isSubmitting}
                  />
                </div>
              </div>

              {/* Phone */}
              <div>
                <label htmlFor="phoneNumber" className="block text-sm font-medium text-gray-700 mb-2">
                  Phone Number
                </label>
                <div className="relative">
                  <FaPhone className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" />
                  <input
                    type="tel"
                    id="phoneNumber"
                    name="phoneNumber"
                    value={formData.phoneNumber}
                    onChange={handleChange}
                    className="pl-10 w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                    placeholder="+254712345678"
                    required
                    disabled={isSubmitting}
                  />
                </div>
              </div>
            </div>

            {/* Role */}
            <div>
              <label htmlFor="role" className="block text-sm font-medium text-gray-700 mb-2">
                Role
              </label>
              <select
                id="role"
                name="role"
                value={formData.role}
                onChange={handleChange}
                className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                disabled={isSubmitting}
              >
                <option value="Viewer">Viewer</option>
                <option value="PropertyManager">Property Manager</option>
                <option value="Admin">Admin</option>
              </select>
            </div>

            {/* Password Row */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
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
                    className="pl-10 w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                    placeholder="••••••••"
                    required
                    disabled={isSubmitting}
                    minLength={6}
                  />
                </div>
              </div>

              {/* Confirm Password */}
              <div>
                <label htmlFor="confirmPassword" className="block text-sm font-medium text-gray-700 mb-2">
                  Confirm Password
                </label>
                <div className="relative">
                  <FaLock className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" />
                  <input
                    type="password"
                    id="confirmPassword"
                    name="confirmPassword"
                    value={formData.confirmPassword}
                    onChange={handleChange}
                    className="pl-10 w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                    placeholder="••••••••"
                    required
                    disabled={isSubmitting}
                    minLength={6}
                  />
                </div>
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
                  Creating account...
                </>
              ) : (
                'Create Account'
              )}
            </button>
          </form>

          {/* Footer */}
          <div className="mt-6 text-center">
            <p className="text-sm text-gray-600">
              Already have an account?{' '}
              <Link href="/login" className="text-blue-600 hover:text-blue-700 font-semibold">
                Sign in
              </Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  )
}
