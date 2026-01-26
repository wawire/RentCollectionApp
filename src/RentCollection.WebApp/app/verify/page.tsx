'use client'

import { useEffect, useState } from 'react'
import { useRouter } from 'next/navigation'
import { Alert, Button, Card, Input, PageHeader, Select } from '@/components/common'
import { authService } from '@/lib/services/authService'
import { useAuth } from '@/contexts/AuthContext'
import { UserRole } from '@/lib/types/auth.types'

export default function VerifyAccountPage() {
  const router = useRouter()
  const { user, refreshUser } = useAuth()
  const [channel, setChannel] = useState<'Email' | 'Phone'>('Email')
  const [code, setCode] = useState('')
  const [isSending, setIsSending] = useState(false)
  const [isVerifying, setIsVerifying] = useState(false)
  const [message, setMessage] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!user) return
    if (user.isVerified) {
      router.push(user.role === UserRole.Tenant ? '/tenant-portal' : '/dashboard')
    }
  }, [user, router])

  const handleSend = async () => {
    setError(null)
    setMessage(null)
    setIsSending(true)
    try {
      await authService.sendVerificationOtp(channel)
      setMessage(`Verification code sent via ${channel.toLowerCase()}.`)
    } catch (err: any) {
      setError(err.response?.data?.message || err.message || 'Failed to send verification code.')
    } finally {
      setIsSending(false)
    }
  }

  const handleVerify = async () => {
    setError(null)
    setMessage(null)
    setIsVerifying(true)
    try {
      const authData = await authService.verifyOtp(code)
      refreshUser()
      setMessage('Verification successful.')
      if (authData.role === UserRole.Tenant) {
        router.push('/tenant-portal')
      } else {
        router.push('/dashboard')
      }
    } catch (err: any) {
      setError(err.response?.data?.message || err.message || 'Invalid verification code.')
    } finally {
      setIsVerifying(false)
    }
  }

  return (
    <div className="min-h-screen bg-brand-bg flex items-center justify-center px-4 py-12">
      <div className="w-full max-w-lg space-y-6">
        <PageHeader
          title="Verify your account"
          subtitle="Enter the one-time code sent to your email or phone."
        />

        {error && <Alert type="error" message={error} />}
        {message && <Alert type="success" message={message} />}

        <Card className="space-y-4">
          <Select
            label="Delivery channel"
            value={channel}
            onChange={(event) => setChannel(event.target.value as 'Email' | 'Phone')}
            options={[
              { value: 'Email', label: 'Email' },
              { value: 'Phone', label: 'Phone (SMS)' },
            ]}
            fullWidth
          />

          <Button variant="secondary" onClick={handleSend} loading={isSending} fullWidth>
            Send verification code
          </Button>

          <Input
            label="Verification code"
            value={code}
            onChange={(event) => setCode(event.target.value)}
            placeholder="Enter 6-digit code"
            fullWidth
          />

          <Button onClick={handleVerify} loading={isVerifying} fullWidth>
            Verify account
          </Button>
        </Card>
      </div>
    </div>
  )
}
