'use client'

import { useState, useEffect } from 'react'
import Link from 'next/link'
import {
  Badge,
  Button,
  Card,
  Input,
  Modal,
  PageHeader,
  Alert,
} from '@/components/common'
import {
  CheckCircle,
  Shield,
  QrCode,
  KeyRound,
  XCircle,
  Loader2,
  Lock,
  Wallet,
  User,
  Settings,
} from 'lucide-react'
import { twoFactorAuthService, Setup2FAResponse } from '@/lib/services/twoFactorAuthService'
import { useAuth } from '@/contexts/AuthContext'

export default function SettingsPage() {
  const { user } = useAuth()
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)

  const [is2FAEnabled, setIs2FAEnabled] = useState(false)
  const [showSetupModal, setShowSetupModal] = useState(false)
  const [setupData, setSetupData] = useState<Setup2FAResponse | null>(null)
  const [verificationCode, setVerificationCode] = useState('')
  const [disablePassword, setDisablePassword] = useState('')

  useEffect(() => {
    setIs2FAEnabled(false)
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
      setSuccessMessage('Two-Factor Authentication enabled successfully.')
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
      setSuccessMessage('Two-Factor Authentication disabled successfully.')
      setIs2FAEnabled(false)
      setDisablePassword('')
    } catch (err: any) {
      setError(err.message || 'Failed to disable 2FA. Please check your password.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Account Settings"
        subtitle="Manage your account security and preferences."
        breadcrumbs={[
          { label: 'Dashboard', href: '/dashboard' },
          { label: 'Settings' },
        ]}
      />

      {error && (
        <Alert type="error" message={error} />
      )}
      {successMessage && (
        <Alert type="success" message={successMessage} />
      )}

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Link href="/dashboard/payment-accounts" className="block">
          <Card className="hover:shadow-subtle transition-shadow">
            <div className="flex items-center gap-4">
              <div className="w-12 h-12 rounded-full bg-state-success/10 flex items-center justify-center">
                <Wallet className="text-state-success" size={20} />
              </div>
              <div>
                <h3 className="font-semibold text-text-primary">Payment Accounts</h3>
                <p className="text-sm text-text-secondary">Manage how you receive payments</p>
              </div>
            </div>
          </Card>
        </Link>

        <Card className="opacity-50 cursor-not-allowed">
          <div className="flex items-center gap-4">
            <div className="w-12 h-12 rounded-full bg-brand-bg flex items-center justify-center">
              <User className="text-brand-secondary" size={20} />
            </div>
            <div>
              <h3 className="font-semibold text-text-primary">Profile Settings</h3>
              <p className="text-sm text-text-secondary">Coming soon</p>
            </div>
          </div>
        </Card>

        <Card className="opacity-50 cursor-not-allowed">
          <div className="flex items-center gap-4">
            <div className="w-12 h-12 rounded-full bg-brand-bg flex items-center justify-center">
              <Settings className="text-brand-secondary" size={20} />
            </div>
            <div>
              <h3 className="font-semibold text-text-primary">Preferences</h3>
              <p className="text-sm text-text-secondary">Coming soon</p>
            </div>
          </div>
        </Card>
      </div>

      <Card>
        <div className="flex items-start justify-between mb-6">
          <div>
            <h2 className="text-xl font-semibold text-text-primary flex items-center">
              <Shield className="mr-2 text-brand-secondary" size={20} />
              Two-Factor Authentication
            </h2>
            <p className="mt-2 text-text-secondary">
              Add an extra layer of security to your account by enabling two-factor authentication.
            </p>
          </div>
          <div>
            {is2FAEnabled ? (
              <Badge variant="success">Enabled</Badge>
            ) : (
              <Badge variant="default">Disabled</Badge>
            )}
          </div>
        </div>

        {!is2FAEnabled ? (
          <div className="space-y-4">
            <Alert
              type="info"
              title="Why enable 2FA?"
              message="Protects your account even if your password is compromised. Uses TOTP with common authenticator apps and adds verification during login."
            />

            <Button onClick={handleSetup2FA} disabled={loading}>
              {loading ? (
                <>
                  <Loader2 className="mr-2 animate-spin" size={16} />
                  Setting up...
                </>
              ) : (
                <>
                  <Shield className="mr-2" size={16} />
                  Enable Two-Factor Authentication
                </>
              )}
            </Button>
          </div>
        ) : (
          <div className="space-y-4">
            <Alert
              type="success"
              message="Your account is protected with two-factor authentication. You will need a code from your authenticator app when you sign in."
            />

            <form onSubmit={handleDisable2FA} className="space-y-4">
              <Input
                id="disablePassword"
                type="password"
                label="Enter your password to disable 2FA"
                value={disablePassword}
                onChange={(e) => setDisablePassword(e.target.value)}
                placeholder="Enter your password"
                required
                fullWidth
              />

              <Button variant="destructive" type="submit" disabled={loading || !disablePassword}>
                {loading ? (
                  <>
                    <Loader2 className="mr-2 animate-spin" size={16} />
                    Disabling...
                  </>
                ) : (
                  <>
                    <XCircle className="mr-2" size={16} />
                    Disable Two-Factor Authentication
                  </>
                )}
              </Button>
            </form>
          </div>
        )}
      </Card>

      <Card>
        <h2 className="text-xl font-semibold text-text-primary flex items-center mb-4">
          <Lock className="mr-2 text-brand-secondary" size={20} />
          Password & Security
        </h2>
        <p className="text-text-secondary mb-4">
          Additional security settings coming soon.
        </p>
      </Card>

      <Modal
        isOpen={showSetupModal && !!setupData}
        onClose={() => {
          setShowSetupModal(false)
          setSetupData(null)
          setVerificationCode('')
          setError(null)
        }}
        title="Setup Two-Factor Authentication"
        footer={
          <div className="flex gap-3 w-full">
            <Button
              variant="secondary"
              onClick={() => {
                setShowSetupModal(false)
                setSetupData(null)
                setVerificationCode('')
                setError(null)
              }}
              fullWidth
            >
              Cancel
            </Button>
            <Button
              onClick={handleEnable2FA}
              disabled={loading || verificationCode.length !== 6}
              fullWidth
            >
              {loading ? (
                <>
                  <Loader2 className="mr-2 animate-spin" size={16} />
                  Verifying...
                </>
              ) : (
                <>
                  <KeyRound className="mr-2" size={16} />
                  Verify & Enable
                </>
              )}
            </Button>
          </div>
        }
      >
        {setupData && (
          <div className="space-y-4">
            <div>
              <p className="text-sm text-text-secondary mb-3">
                Scan this QR code with your authenticator app:
              </p>
              <div className="bg-surface border border-border-muted rounded-lg p-4 flex items-center justify-center">
                <img
                  src={setupData.qrCodeUri}
                  alt="2FA QR Code"
                  className="w-64 h-64"
                />
              </div>
            </div>

            <div className="bg-brand-bg/60 border border-border-muted rounded-lg p-3">
              <p className="text-xs text-text-muted mb-1">Or enter this code manually:</p>
              <code className="text-sm font-mono text-text-primary break-all">
                {setupData.secretKey}
              </code>
            </div>

            <form onSubmit={handleEnable2FA} className="space-y-4">
              <div>
                <label htmlFor="verificationCode" className="block text-sm font-medium text-text-secondary mb-1">
                  Enter the 6-digit code from your app
                </label>
                <input
                  id="verificationCode"
                  type="text"
                  value={verificationCode}
                  onChange={(e) => setVerificationCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
                  className="w-full px-3 py-2 border border-border-muted rounded-lg focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-secondary/60 text-center text-xl tracking-widest font-mono"
                  placeholder="000000"
                  maxLength={6}
                  required
                />
              </div>
            </form>

            <Alert
              type="warning"
              message="Important: Save your secret code in a safe place. You will need it if you lose access to your authenticator app."
            />
          </div>
        )}
      </Modal>
    </div>
  )
}
