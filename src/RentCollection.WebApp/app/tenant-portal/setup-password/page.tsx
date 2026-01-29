'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { Alert, Button, Card, Input, PageHeader } from '@/components/common'
import { authService } from '@/lib/services/authService'
import { useAuth } from '@/contexts/AuthContext'

export default function TenantSetupPasswordPage() {
  const router = useRouter()
  const { refreshUser } = useAuth()
  const [newPassword, setNewPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  const handleSubmit = async () => {
    setError(null)
    setSuccess(null)
    setIsSubmitting(true)
    try {
      await authService.completePasswordChange(newPassword, confirmPassword)
      refreshUser()
      setSuccess('Password updated successfully.')
      router.push('/tenant-portal')
    } catch (err: any) {
      setError(err.response?.data?.message || err.message || 'Failed to update password.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="min-h-screen bg-brand-bg flex items-center justify-center px-4 py-12">
      <div className="w-full max-w-lg space-y-6">
        <PageHeader
          title="Set your password"
          subtitle="Create a new password to activate your tenant portal."
        />

        {error && <Alert type="error" message={error} />}
        {success && <Alert type="success" message={success} />}

        <Card className="space-y-4">
          <Input
            label="New password"
            type="password"
            value={newPassword}
            onChange={(event) => setNewPassword(event.target.value)}
            helperText="Minimum 12 characters."
            fullWidth
          />

          <Input
            label="Confirm password"
            type="password"
            value={confirmPassword}
            onChange={(event) => setConfirmPassword(event.target.value)}
            fullWidth
          />

          <Button onClick={handleSubmit} loading={isSubmitting} fullWidth>
            Save password
          </Button>
        </Card>
      </div>
    </div>
  )
}
