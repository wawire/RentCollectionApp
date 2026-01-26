'use client'

import { useState } from 'react'
import { useAuth } from '@/contexts/AuthContext'
import Link from 'next/link'
import { AlertCircle, Eye, EyeOff, Lock, Mail } from 'lucide-react'

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
      <div className="hidden lg:flex lg:w-1/2 relative bg-gradient-to-br from-brand-primary via-brand-secondary to-brand-primary">
        <div className="absolute inset-0 bg-brand-primary/20" />
        <div className="relative z-10 flex flex-col justify-between p-12 text-white">
          <div>
            <h1 className="text-4xl font-serif font-normal mb-3 tracking-wide">Hisa Rentals</h1>
            <p className="text-white/80 text-lg tracking-wide">
              Smart Rent & Property Management for Modern Landlords and Agencies
            </p>
            <p className="text-white/70 mt-3 tracking-wide">
              Collect rent, manage properties, and stay in control - without spreadsheets or chaos.
            </p>
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
                  <h3 className="text-lg font-serif font-normal mb-1 tracking-wide">Centralized Portfolio</h3>
                  <p className="text-sm text-white/70 tracking-wide leading-relaxed">Monitor properties, tenants, and revenue in one unified workspace.</p>
                </div>
              </div>
            </div>

            <div>
              <div className="flex items-start space-x-4">
                <div className="flex-shrink-0 w-12 h-12 rounded-full bg-white/10 backdrop-blur-sm flex items-center justify-center border border-white/20">
                  <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                </div>
                <div>
                  <h3 className="text-lg font-serif font-normal mb-1 tracking-wide">Instant Payment Tracking</h3>
                  <p className="text-sm text-white/70 tracking-wide leading-relaxed">Real-time M-Pesa updates with receipts and reconciliation.</p>
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
                  <h3 className="text-lg font-serif font-normal mb-1 tracking-wide">Operational Insights</h3>
                  <p className="text-sm text-white/70 tracking-wide leading-relaxed">Generate reports, track occupancy, and plan growth confidently.</p>
                </div>
              </div>
            </div>
          </div>

          <div className="text-sm text-white/60">
            (c) 2024 Hisa Rentals. Built for Kenyan property teams.
          </div>
        </div>
      </div>

      <div className="flex-1 flex items-center justify-center px-4 sm:px-6 lg:px-8 bg-brand-bg">
        <div className="w-full max-w-md space-y-8">
          <div className="lg:hidden text-center mb-8">
            <h1 className="text-3xl font-serif font-normal text-brand-primary tracking-wide">Hisa Rentals</h1>
            <p className="text-text-secondary mt-2 tracking-wide">Property Management Platform</p>
          </div>

          <div>
            <h2 className="text-3xl font-serif font-normal text-text-primary mb-3 tracking-wide">Welcome Back</h2>
            <p className="text-base text-text-secondary tracking-wide leading-relaxed">
              Sign in to manage properties, track payments, and support tenants.
            </p>
          </div>

          <form className="mt-8 space-y-6" onSubmit={handleSubmit}>
            {error && (
              <div className="rounded-lg bg-state-error/10 border border-state-error/30 p-4 flex items-start space-x-3">
                <AlertCircle className="h-5 w-5 text-state-error mt-0.5" />
                <div>
                  <h3 className="text-sm font-medium text-text-primary">Error</h3>
                  <p className="text-sm text-text-secondary mt-1">{error}</p>
                </div>
              </div>
            )}

            <div className="space-y-4">
              <div>
                <label htmlFor="emailOrPhone" className="block text-sm font-medium text-text-secondary mb-1">
                  Email or Phone Number
                </label>
                <div className="relative">
                  <div className="absolute left-3 top-1/2 -translate-y-1/2 pointer-events-none">
                    <Mail className="h-5 w-5 text-text-muted" />
                  </div>
                  <input
                    id="emailOrPhone"
                    name="emailOrPhone"
                    type="text"
                    required
                    className="w-full pl-10 pr-3 py-2.5 rounded-lg border border-border-muted bg-surface text-text-primary placeholder:text-text-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-secondary/60 focus-visible:ring-offset-2 focus-visible:ring-offset-brand-bg"
                    placeholder="admin@hisa.co.ke or 0712345678"
                    value={emailOrPhone}
                    onChange={(e) => setEmailOrPhone(e.target.value)}
                  />
                </div>
              </div>

              <div>
                <label htmlFor="password" className="block text-sm font-medium text-text-secondary mb-1">
                  Password
                </label>
                <div className="relative">
                  <div className="absolute left-3 top-1/2 -translate-y-1/2 pointer-events-none">
                    <Lock className="h-5 w-5 text-text-muted" />
                  </div>
                  <input
                    id="password"
                    name="password"
                    type={showPassword ? 'text' : 'password'}
                    required
                    className="w-full pl-10 pr-10 py-2.5 rounded-lg border border-border-muted bg-surface text-text-primary placeholder:text-text-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-secondary/60 focus-visible:ring-offset-2 focus-visible:ring-offset-brand-bg"
                    placeholder="Enter your password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                  />
                  <button
                    type="button"
                    className="absolute right-3 top-1/2 -translate-y-1/2 flex items-center"
                    onClick={() => setShowPassword(!showPassword)}
                  >
                    {showPassword ? (
                      <EyeOff className="h-5 w-5 text-text-muted hover:text-text-secondary transition-colors" />
                    ) : (
                      <Eye className="h-5 w-5 text-text-muted hover:text-text-secondary transition-colors" />
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
                  className="h-4 w-4 text-brand-secondary focus:ring-brand-secondary border-border-muted rounded"
                />
                <label htmlFor="remember-me" className="ml-2 block text-sm text-text-secondary">
                  Remember me
                </label>
              </div>

              <div className="text-sm">
                <a href="#" className="font-medium text-brand-secondary hover:text-brand-primary transition-colors">
                  Forgot password?
                </a>
              </div>
            </div>

            <button
              type="submit"
              disabled={isLoading}
              className="group relative w-full flex justify-center py-3 px-4 text-sm font-semibold rounded-lg text-white bg-brand-primary hover:bg-brand-secondary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:ring-brand-secondary/60 transition-all disabled:opacity-60 disabled:cursor-not-allowed shadow-subtle"
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
              <p className="text-sm text-text-secondary tracking-wide">
                Don't have an account?{' '}
                <Link href="/register" className="font-medium text-brand-secondary hover:text-brand-primary transition-colors">
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
