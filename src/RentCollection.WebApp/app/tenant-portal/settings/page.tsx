'use client'

import { useState, useEffect } from 'react'
import Link from 'next/link'
import { FaArrowLeft, FaShieldAlt, FaQrcode, FaKey, FaCheckCircle, FaTimesCircle, FaSpinner, FaLock } from 'react-icons/fa'
import { twoFactorAuthService, Setup2FAResponse } from '@/lib/services/twoFactorAuthService'
import { useAuth } from '@/contexts/AuthContext'

export default function SettingsPage() {
  const { user } = useAuth()
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)

  // 2FA Setup State
  const [is2FAEnabled, setIs2FAEnabled] = useState(false)
  const [showSetupModal, setShowSetupModal] = useState(false)
  const [setupData, setSetupData] = useState<Setup2FAResponse | null>(null)
  const [verificationCode, setVerificationCode] = useState('')
  const [disablePassword, setDisablePassword] = useState('')

  useEffect(() => {
    // Check 2FA status from user object if available
    // In a real app, you'd fetch this from the backend
    setIs2FAEnabled(false) // Default, update based on your user object structure
  }, [user])

  const handleSetup2FA = async () => {
    try {
      setLoading(true)
      setError(null)
      const data = await twoFactorAuthService.setup()
      setSetupData(data)
      setShowSetupModal(true)
    } catch (err: any) {
      setError(err.message || 'Failed to setup 2FA')
    } finally {
      setLoading(false)
    }
  }

  const handleEnable2FA = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!verificationCode || !setupData) return

    try {
      setLoading(true)
      setError(null)
      await twoFactorAuthService.enable(verificationCode)
      setSuccessMessage('Two-Factor Authentication enabled successfully!')
      setIs2FAEnabled(true)
      setShowSetupModal(false)
      setVerificationCode('')
      setSetupData(null)
    } catch (err: any) {
      setError(err.message || 'Failed to enable 2FA. Please check your code.')
    } finally {
      setLoading(false)
    }
  }

  const handleDisable2FA = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!disablePassword) return

    if (!confirm('Are you sure you want to disable Two-Factor Authentication? This will make your account less secure.')) {
      return
    }

    try {
      setLoading(true)
      setError(null)
      await twoFactorAuthService.disable(disablePassword)
      setSuccessMessage('Two-Factor Authentication disabled successfully')
      setIs2FAEnabled(false)
      setDisablePassword('')
    } catch (err: any) {
      setError(err.message || 'Failed to disable 2FA. Please check your password.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Header */}
        <div className="mb-8">
          <Link
            href="/tenant-portal"
            className="inline-flex items-center text-blue-600 hover:text-blue-800 mb-4"
          >
            <FaArrowLeft className="mr-2" />
            Back to Dashboard
          </Link>
          <h1 className="text-3xl font-bold text-gray-900">Account Settings</h1>
          <p className="mt-2 text-gray-600">
            Manage your account security and preferences
          </p>
        </div>

        {/* Error/Success Messages */}
        {error && (
          <div className="mb-6 bg-red-50 border border-red-200 text-red-800 px-4 py-3 rounded-lg">
            {error}
          </div>
        )}
        {successMessage && (
          <div className="mb-6 bg-green-50 border border-green-200 text-green-800 px-4 py-3 rounded-lg">
            {successMessage}
          </div>
        )}

        {/* Two-Factor Authentication Section */}
        <div className="bg-white rounded-lg shadow-md p-6 mb-6">
          <div className="flex items-start justify-between mb-6">
            <div>
              <h2 className="text-xl font-semibold text-gray-900 flex items-center">
                <FaShieldAlt className="mr-2 text-blue-600" />
                Two-Factor Authentication
              </h2>
              <p className="mt-2 text-gray-600">
                Add an extra layer of security to your account by enabling two-factor authentication
              </p>
            </div>
            <div>
              {is2FAEnabled ? (
                <span className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-green-100 text-green-800">
                  <FaCheckCircle className="mr-1" />
                  Enabled
                </span>
              ) : (
                <span className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-gray-100 text-gray-800">
                  <FaTimesCircle className="mr-1" />
                  Disabled
                </span>
              )}
            </div>
          </div>

          {!is2FAEnabled ? (
            <div className="space-y-4">
              <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                <h3 className="font-semibold text-blue-900 mb-2">Why enable 2FA?</h3>
                <ul className="list-disc list-inside space-y-1 text-sm text-blue-800">
                  <li>Protects your account even if your password is compromised</li>
                  <li>Uses industry-standard TOTP (Time-based One-Time Password)</li>
                  <li>Works with popular authenticator apps like Google Authenticator, Authy, or Microsoft Authenticator</li>
                  <li>Provides an additional verification step during login</li>
                </ul>
              </div>

              <button
                onClick={handleSetup2FA}
                disabled={loading}
                className="bg-blue-600 text-white py-2 px-6 rounded-lg hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed flex items-center"
              >
                {loading ? (
                  <>
                    <FaSpinner className="animate-spin mr-2" />
                    Setting up...
                  </>
                ) : (
                  <>
                    <FaShieldAlt className="mr-2" />
                    Enable Two-Factor Authentication
                  </>
                )}
              </button>
            </div>
          ) : (
            <div className="space-y-4">
              <div className="bg-green-50 border border-green-200 rounded-lg p-4">
                <p className="text-sm text-green-800">
                  Your account is protected with two-factor authentication. You'll need to enter a code from your authenticator app when you sign in.
                </p>
              </div>

              <form onSubmit={handleDisable2FA} className="space-y-4">
                <div>
                  <label htmlFor="disablePassword" className="block text-sm font-medium text-gray-700 mb-1">
                    Enter your password to disable 2FA
                  </label>
                  <input
                    id="disablePassword"
                    type="password"
                    value={disablePassword}
                    onChange={(e) => setDisablePassword(e.target.value)}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-red-500 focus:border-transparent"
                    placeholder="Enter your password"
                    required
                  />
                </div>

                <button
                  type="submit"
                  disabled={loading || !disablePassword}
                  className="bg-red-600 text-white py-2 px-6 rounded-lg hover:bg-red-700 disabled:bg-gray-400 disabled:cursor-not-allowed flex items-center"
                >
                  {loading ? (
                    <>
                      <FaSpinner className="animate-spin mr-2" />
                      Disabling...
                    </>
                  ) : (
                    <>
                      <FaTimesCircle className="mr-2" />
                      Disable Two-Factor Authentication
                    </>
                  )}
                </button>
              </form>
            </div>
          )}
        </div>

        {/* Other Settings Sections Can Be Added Here */}
        <div className="bg-white rounded-lg shadow-md p-6">
          <h2 className="text-xl font-semibold text-gray-900 flex items-center mb-4">
            <FaLock className="mr-2 text-blue-600" />
            Password & Security
          </h2>
          <p className="text-gray-600 mb-4">
            Additional security settings coming soon...
          </p>
        </div>
      </div>

      {/* 2FA Setup Modal */}
      {showSetupModal && setupData && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg shadow-xl max-w-md w-full p-6">
            <h3 className="text-2xl font-bold text-gray-900 mb-4 flex items-center">
              <FaQrcode className="mr-2 text-blue-600" />
              Setup Two-Factor Authentication
            </h3>

            <div className="space-y-4">
              <div>
                <p className="text-sm text-gray-600 mb-3">
                  Scan this QR code with your authenticator app:
                </p>
                <div className="bg-white border-4 border-gray-200 rounded-lg p-4 flex items-center justify-center">
                  <img
                    src={setupData.qrCodeUri}
                    alt="2FA QR Code"
                    className="w-64 h-64"
                  />
                </div>
              </div>

              <div className="bg-gray-50 border border-gray-200 rounded-lg p-3">
                <p className="text-xs text-gray-600 mb-1">Or enter this code manually:</p>
                <code className="text-sm font-mono text-gray-900 break-all">
                  {setupData.secretKey}
                </code>
              </div>

              <form onSubmit={handleEnable2FA} className="space-y-4">
                <div>
                  <label htmlFor="verificationCode" className="block text-sm font-medium text-gray-700 mb-1">
                    Enter the 6-digit code from your app
                  </label>
                  <input
                    id="verificationCode"
                    type="text"
                    value={verificationCode}
                    onChange={(e) => setVerificationCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent text-center text-2xl tracking-widest font-mono"
                    placeholder="000000"
                    maxLength={6}
                    required
                  />
                </div>

                <div className="flex gap-3">
                  <button
                    type="button"
                    onClick={() => {
                      setShowSetupModal(false)
                      setSetupData(null)
                      setVerificationCode('')
                      setError(null)
                    }}
                    className="flex-1 bg-gray-200 text-gray-800 py-2 px-4 rounded-lg hover:bg-gray-300"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    disabled={loading || verificationCode.length !== 6}
                    className="flex-1 bg-blue-600 text-white py-2 px-4 rounded-lg hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed flex items-center justify-center"
                  >
                    {loading ? (
                      <>
                        <FaSpinner className="animate-spin mr-2" />
                        Verifying...
                      </>
                    ) : (
                      <>
                        <FaKey className="mr-2" />
                        Verify & Enable
                      </>
                    )}
                  </button>
                </div>
              </form>

              <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-3">
                <p className="text-xs text-yellow-800">
                  <strong>Important:</strong> Make sure to save your secret code in a safe place. You'll need it if you lose access to your authenticator app.
                </p>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
