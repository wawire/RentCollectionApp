'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import Link from 'next/link'
import { AlertCircle, CheckCircle2, Eye, EyeOff, Lock, Mail, Phone, User } from 'lucide-react'
import { authService } from '@/lib/services/authService'
import { UserRole } from '@/lib/types/auth.types'

export default function RegisterPage() {
  const router = useRouter()
  const inviteOnly = true
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    phoneNumber: '',
    password: '',
    confirmPassword: '',
    role: UserRole.Tenant,
  })
  const [showPassword, setShowPassword] = useState(false)
  const [showConfirmPassword, setShowConfirmPassword] = useState(false)
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState(false)

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    setFormData({ ...formData, [e.target.name]: e.target.value })
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    setSuccess(false)

    if (inviteOnly) {
      setError('Hisa Rentals accounts are created by your organization owner. Please contact support or your admin.')
      return
    }

    if (formData.password !== formData.confirmPassword) {
      setError('Passwords do not match')
      return
    }

    setIsLoading(true)

    try {
      await authService.register(formData)
      setSuccess(true)
      setTimeout(() => {
        router.push('/dashboard')
      }, 1500)
    } catch (err: any) {
      setError(
        err.response?.data?.message ||
          err.message ||
          'Registration failed. Please try again.'
      )
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
            <h1 className="text-4xl font-serif font-normal mb-3 tracking-wide">Join Hisa Rentals</h1>
            <p className="text-white/80 text-lg tracking-wide">A smarter tenant experience for Kenya's rental market.</p>
          </div>

          <div className="space-y-6">
            <div>
              <div className="flex items-start space-x-4">
                <div className="flex-shrink-0 w-12 h-12 rounded-full bg-white/10 backdrop-blur-sm flex items-center justify-center">
                  <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                  </svg>
                </div>
                <div>
                  <h3 className="text-lg font-serif font-normal mb-1 tracking-wide">Verified Listings</h3>
                  <p className="text-sm text-white/70 tracking-wide leading-relaxed">Search trusted properties with transparent rent details.</p>
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
                  <h3 className="text-lg font-serif font-normal mb-1 tracking-wide">Secure M-Pesa Payments</h3>
                  <p className="text-sm text-white/70 tracking-wide leading-relaxed">Pay rent safely with instant receipts and history.</p>
                </div>
              </div>
            </div>

            <div>
              <div className="flex items-start space-x-4">
                <div className="flex-shrink-0 w-12 h-12 rounded-full bg-white/10 backdrop-blur-sm flex items-center justify-center">
                  <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-5 5v-5z" />
                  </svg>
                </div>
                <div>
                  <h3 className="text-lg font-serif font-normal mb-1 tracking-wide">Direct Support</h3>
                  <p className="text-sm text-white/70 tracking-wide leading-relaxed">Message landlords and submit maintenance requests.</p>
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
        <div className="w-full max-w-md space-y-8 py-12">
          <div className="lg:hidden text-center mb-8">
            <h1 className="text-3xl font-serif font-normal text-brand-primary tracking-wide">Hisa Rentals</h1>
            <p className="text-text-secondary mt-2 tracking-wide">Property Management Platform</p>
          </div>

          <div>
            <h2 className="text-3xl font-serif font-normal text-text-primary mb-3 tracking-wide">Find Your Next Home</h2>
            <p className="text-base text-text-secondary tracking-wide leading-relaxed mb-4">
              Create your account to browse properties, request viewings, and manage rent payments.
            </p>
            <p className="text-sm text-state-warning tracking-wide mb-2">
              Invite-only: your organization owner will create your account.
            </p>
            <p className="text-sm text-text-secondary tracking-wide">
              Already have an account?{' '}
              <Link href="/login" className="font-medium text-brand-secondary hover:text-brand-primary transition-colors">
                Sign in
              </Link>
            </p>
          </div>

          <form className="mt-8 space-y-6" onSubmit={handleSubmit}>
            {error && (
              <div className="rounded-lg bg-state-error/10 border border-state-error/30 p-4 flex items-start space-x-3">
                <AlertCircle className="h-5 w-5 text-state-error mt-0.5 flex-shrink-0" />
                <div>
                  <h3 className="text-sm font-medium text-text-primary">Error</h3>
                  <p className="text-sm text-text-secondary mt-1">{error}</p>
                </div>
              </div>
            )}

            {success && (
              <div className="rounded-lg bg-state-success/10 border border-state-success/30 p-4 flex items-start space-x-3">
                <CheckCircle2 className="h-5 w-5 text-state-success mt-0.5 flex-shrink-0" />
                <div>
                  <h3 className="text-sm font-medium text-text-primary">Success!</h3>
                  <p className="text-sm text-text-secondary mt-1">Account created successfully. Redirecting...</p>
                </div>
              </div>
            )}

            <div className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label htmlFor="firstName" className="block text-sm font-medium text-text-secondary mb-1">
                    First Name
                  </label>
                  <div className="relative">
                    <div className="absolute left-3 top-1/2 -translate-y-1/2 pointer-events-none">
                      <User className="h-5 w-5 text-text-muted" />
                    </div>
                    <input
                      id="firstName"
                      name="firstName"
                      type="text"
                      required
                      className="w-full pl-10 pr-3 py-2.5 rounded-lg border border-border-muted bg-surface text-text-primary placeholder:text-text-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-secondary/60 focus-visible:ring-offset-2 focus-visible:ring-offset-brand-bg text-sm"
                      placeholder="John"
                      value={formData.firstName}
                      onChange={handleChange}
                    />
                  </div>
                </div>

                <div>
                  <label htmlFor="lastName" className="block text-sm font-medium text-text-secondary mb-1">
                    Last Name
                  </label>
                  <div className="relative">
                    <div className="absolute left-3 top-1/2 -translate-y-1/2 pointer-events-none">
                      <User className="h-5 w-5 text-text-muted" />
                    </div>
                    <input
                      id="lastName"
                      name="lastName"
                      type="text"
                      required
                      className="w-full pl-10 pr-3 py-2.5 rounded-lg border border-border-muted bg-surface text-text-primary placeholder:text-text-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-secondary/60 focus-visible:ring-offset-2 focus-visible:ring-offset-brand-bg text-sm"
                      placeholder="Doe"
                      value={formData.lastName}
                      onChange={handleChange}
                    />
                  </div>
                </div>
              </div>

              <div>
                <label htmlFor="email" className="block text-sm font-medium text-text-secondary mb-1">
                  Email Address
                </label>
                <div className="relative">
                  <div className="absolute left-3 top-1/2 -translate-y-1/2 pointer-events-none">
                    <Mail className="h-5 w-5 text-text-muted" />
                  </div>
                  <input
                    id="email"
                    name="email"
                    type="email"
                    required
                    className="w-full pl-10 pr-3 py-2.5 rounded-lg border border-border-muted bg-surface text-text-primary placeholder:text-text-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-secondary/60 focus-visible:ring-offset-2 focus-visible:ring-offset-brand-bg text-sm"
                    placeholder="john@example.com"
                    value={formData.email}
                    onChange={handleChange}
                  />
                </div>
              </div>

              <div>
                <label htmlFor="phoneNumber" className="block text-sm font-medium text-text-secondary mb-1">
                  Phone Number
                </label>
                <div className="relative">
                  <div className="absolute left-3 top-1/2 -translate-y-1/2 pointer-events-none">
                    <Phone className="h-5 w-5 text-text-muted" />
                  </div>
                  <input
                    id="phoneNumber"
                    name="phoneNumber"
                    type="tel"
                    required
                    className="w-full pl-10 pr-3 py-2.5 rounded-lg border border-border-muted bg-surface text-text-primary placeholder:text-text-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-secondary/60 focus-visible:ring-offset-2 focus-visible:ring-offset-brand-bg text-sm"
                    placeholder="+254712345678"
                    value={formData.phoneNumber}
                    onChange={handleChange}
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
                    className="w-full pl-10 pr-10 py-2.5 rounded-lg border border-border-muted bg-surface text-text-primary placeholder:text-text-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-secondary/60 focus-visible:ring-offset-2 focus-visible:ring-offset-brand-bg text-sm"
                    placeholder="Enter password"
                    value={formData.password}
                    onChange={handleChange}
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

              <div>
                <label htmlFor="confirmPassword" className="block text-sm font-medium text-text-secondary mb-1">
                  Confirm Password
                </label>
                <div className="relative">
                  <div className="absolute left-3 top-1/2 -translate-y-1/2 pointer-events-none">
                    <Lock className="h-5 w-5 text-text-muted" />
                  </div>
                  <input
                    id="confirmPassword"
                    name="confirmPassword"
                    type={showConfirmPassword ? 'text' : 'password'}
                    required
                    className="w-full pl-10 pr-10 py-2.5 rounded-lg border border-border-muted bg-surface text-text-primary placeholder:text-text-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-secondary/60 focus-visible:ring-offset-2 focus-visible:ring-offset-brand-bg text-sm"
                    placeholder="Confirm password"
                    value={formData.confirmPassword}
                    onChange={handleChange}
                  />
                  <button
                    type="button"
                    className="absolute right-3 top-1/2 -translate-y-1/2 flex items-center"
                    onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                  >
                    {showConfirmPassword ? (
                      <EyeOff className="h-5 w-5 text-text-muted hover:text-text-secondary transition-colors" />
                    ) : (
                      <Eye className="h-5 w-5 text-text-muted hover:text-text-secondary transition-colors" />
                    )}
                  </button>
                </div>
              </div>
            </div>

            <button
              type="submit"
              disabled={isLoading || success || inviteOnly}
              className="group relative w-full flex justify-center py-3 px-4 text-sm font-semibold rounded-lg text-white bg-brand-primary hover:bg-brand-secondary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:ring-brand-secondary/60 transition-all disabled:opacity-60 disabled:cursor-not-allowed shadow-subtle"
            >
              {isLoading ? (
                <span className="flex items-center">
                  <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                  </svg>
                  Creating your account...
                </span>
              ) : (
                'Get Started'
              )}
            </button>

            <p className="text-xs text-center text-text-muted">
              By creating an account, you agree to our Terms of Service and Privacy Policy
            </p>
          </form>
        </div>
      </div>
    </div>
  )
}
