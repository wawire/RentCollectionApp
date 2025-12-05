'use client'

import { useState } from 'react'
import { useAuth } from '@/contexts/AuthContext'
import Link from 'next/link'
import { FiMail, FiLock, FiEye, FiEyeOff, FiAlertCircle } from 'react-icons/fi'
import Image from 'next/image'

export default function LoginPage() {
  const { login } = useAuth()
  const [emailOrPhone, setEmailOrPhone] = useState('')
  const [password, setPassword] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState('')

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    setIsLoading(true)

    try {
      await login({ emailOrPhone, password })
    } catch (err: any) {
      setError(err.response?.data?.message || err.message || 'Invalid credentials. Please try again.')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="min-h-screen flex">
      {/* Left side - Full width image */}
      <div className="hidden lg:flex lg:w-1/2 relative bg-gradient-to-br from-bg-dark via-primary-800 to-primary-900">
        <div className="absolute inset-0 bg-black/20" />
        <div className="relative z-10 flex flex-col justify-between p-12 text-white">
          <div>
            <h1 className="text-4xl font-serif font-normal mb-3 tracking-wide">RentCollection</h1>
            <p className="text-white/70 text-lg tracking-wide">Simplifying property management for modern landlords</p>
          </div>

          <div className="space-y-8">
            <div>
              <div className="flex items-start space-x-4">
                <div className="flex-shrink-0 w-12 h-12 rounded-full bg-white/10 backdrop-blur-sm flex items-center justify-center">
                  <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
                  </svg>
                </div>
                <div>
                  <h3 className="text-lg font-serif font-normal mb-1 tracking-wide">Centralized Dashboard</h3>
                  <p className="text-sm text-white/70 tracking-wide leading-relaxed">Monitor all properties, tenants, and payments from one intuitive dashboard</p>
                </div>
              </div>
            </div>

            <div>
              <div className="flex items-start space-x-4">
                <div className="flex-shrink-0 w-12 h-12 rounded-full bg-accent/20 backdrop-blur-sm flex items-center justify-center border border-accent/30">
                  <svg className="w-6 h-6 text-accent" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                </div>
                <div>
                  <h3 className="text-lg font-serif font-normal mb-1 tracking-wide">Instant Payment Tracking</h3>
                  <p className="text-sm text-white/70 tracking-wide leading-relaxed">Real-time M-Pesa integration with automatic receipt generation</p>
                </div>
              </div>
            </div>

            <div>
              <div className="flex items-start space-x-4">
                <div className="flex-shrink-0 w-12 h-12 rounded-full bg-white/10 backdrop-blur-sm flex items-center justify-center">
                  <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
                  </svg>
                </div>
                <div>
                  <h3 className="text-lg font-serif font-normal mb-1 tracking-wide">Smart Analytics</h3>
                  <p className="text-sm text-white/70 tracking-wide leading-relaxed">Generate financial reports and track occupancy rates effortlessly</p>
                </div>
              </div>
            </div>
          </div>

          <div className="text-sm text-secondary/60">
            © 2024 RentCollection. Designed for Kenyan property managers.
          </div>
        </div>
      </div>

      {/* Right side - Login form */}
      <div className="flex-1 flex items-center justify-center px-4 sm:px-6 lg:px-8 bg-gray-50">
        <div className="w-full max-w-md space-y-8">
          {/* Logo for mobile */}
          <div className="lg:hidden text-center mb-8">
            <h1 className="text-3xl font-serif font-normal text-primary tracking-wide">RentCollection</h1>
            <p className="text-primary/60 mt-2 tracking-wide">Property Management Platform</p>
          </div>

          <div>
            <h2 className="text-3xl font-serif font-normal text-primary mb-3 tracking-wide">Welcome Back</h2>
            <p className="text-base text-primary/70 tracking-wide leading-relaxed">
              Sign in to manage your properties, track payments, and connect with tenants.
            </p>
          </div>

          <form className="mt-8 space-y-6" onSubmit={handleSubmit}>
            {error && (
              <div className="rounded-lg bg-red-50 border border-red-200 p-4 flex items-start space-x-3">
                <FiAlertCircle className="h-5 w-5 text-red-500 mt-0.5" />
                <div>
                  <h3 className="text-sm font-medium text-red-800">Error</h3>
                  <p className="text-sm text-red-700 mt-1">{error}</p>
                </div>
              </div>
            )}

            <div className="space-y-4">
              <div>
                <label htmlFor="emailOrPhone" className="block text-sm font-medium text-gray-700 mb-1">
                  Email or Phone Number
                </label>
                <div className="relative">
                  <div className="absolute left-0 bottom-3 pointer-events-none">
                    <FiMail className="h-5 w-5 text-primary/40" />
                  </div>
                  <input
                    id="emailOrPhone"
                    name="emailOrPhone"
                    type="text"
                    required
                    className="w-full pl-7 pr-2 py-3 border-b-2 border-secondary/30 focus:border-accent outline-none transition-colors bg-transparent text-primary placeholder:text-primary/40"
                    placeholder="admin@rentcollection.com or 0712345678"
                    value={emailOrPhone}
                    onChange={(e) => setEmailOrPhone(e.target.value)}
                  />
                </div>
              </div>

              <div>
                <label htmlFor="password" className="block text-sm font-medium text-gray-700 mb-1">
                  Password
                </label>
                <div className="relative">
                  <div className="absolute left-0 bottom-3 pointer-events-none">
                    <FiLock className="h-5 w-5 text-primary/40" />
                  </div>
                  <input
                    id="password"
                    name="password"
                    type={showPassword ? 'text' : 'password'}
                    required
                    className="w-full pl-7 pr-10 py-3 border-b-2 border-secondary/30 focus:border-accent outline-none transition-colors bg-transparent text-primary placeholder:text-primary/40"
                    placeholder="Enter your password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                  />
                  <button
                    type="button"
                    className="absolute right-0 bottom-3 flex items-center"
                    onClick={() => setShowPassword(!showPassword)}
                  >
                    {showPassword ? (
                      <FiEyeOff className="h-5 w-5 text-primary/40 hover:text-primary/60 transition-colors" />
                    ) : (
                      <FiEye className="h-5 w-5 text-primary/40 hover:text-primary/60 transition-colors" />
                    )}
                  </button>
                </div>
              </div>
            </div>

            <div className="flex items-center justify-between">
              <div className="flex items-center">
                <input
                  id="remember-me"
                  name="remember-me"
                  type="checkbox"
                  className="h-4 w-4 text-accent focus:ring-accent border-muted rounded"
                />
                <label htmlFor="remember-me" className="ml-2 block text-sm text-primary">
                  Remember me
                </label>
              </div>

              <div className="text-sm">
                <a href="#" className="font-medium text-accent hover:text-accent-600 transition-colors">
                  Forgot password?
                </a>
              </div>
            </div>

            <button
              type="submit"
              disabled={isLoading}
              className="group relative w-full flex justify-center py-3 px-4 border border-transparent text-sm font-semibold rounded-lg text-primary bg-accent hover:bg-accent-600 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-accent transition-all disabled:bg-accent/50 disabled:cursor-not-allowed shadow-sm"
            >
              {isLoading ? (
                <span className="flex items-center">
                  <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                  </svg>
                  Signing in...
                </span>
              ) : (
                'Sign In'
              )}
            </button>

            <div className="text-center">
              <p className="text-sm text-primary/70 tracking-wide">
                Don't have an account?{' '}
                <Link href="/register" className="font-medium text-accent hover:text-accent-600 transition-colors">
                  Create account
                </Link>
              </p>
            </div>
          </form>
        </div>
      </div>
    </div>
  )
}
