'use client'

import { useState, useEffect } from 'react'
import { Card, Button, Select, Alert } from '@/components/common'
import { useGetActiveTenants, useSendSms } from '@/lib/hooks'
import { SmsTemplate } from '@/lib/types'
import { Mail, Send } from 'lucide-react'

interface SmsFormProps {
  onSmsSent?: () => void
}

// Predefined message templates
const SMS_TEMPLATES: SmsTemplate[] = [
  {
    id: 'rent_reminder',
    name: 'Rent Reminder',
    content: 'Dear {tenantName}, this is a friendly reminder that your rent of KSh {amount} for {unitNumber} is due. Please make payment at your earliest convenience. Thank you.',
    variables: ['tenantName', 'unitNumber', 'amount'],
  },
  {
    id: 'payment_confirmation',
    name: 'Payment Confirmation',
    content: 'Dear {tenantName}, we have received your rent payment of KSh {amount} for {unitNumber}. Thank you for your prompt payment.',
    variables: ['tenantName', 'unitNumber', 'amount'],
  },
  {
    id: 'lease_expiry',
    name: 'Lease Expiry Notice',
    content: 'Dear {tenantName}, your lease for {unitNumber} will expire on {date}. Please contact us to discuss renewal options.',
    variables: ['tenantName', 'unitNumber', 'date'],
  },
  {
    id: 'maintenance_notice',
    name: 'Maintenance Notice',
    content: 'Dear {tenantName}, scheduled maintenance will be conducted at {unitNumber} on {date}. We apologize for any inconvenience.',
    variables: ['tenantName', 'unitNumber', 'date'],
  },
  {
    id: 'custom',
    name: 'Custom Message',
    content: '',
    variables: [],
  },
]

export default function SmsForm({ onSmsSent }: SmsFormProps) {
  const { data: tenants } = useGetActiveTenants()
  const { sendSms, loading, error } = useSendSms()

  const [selectedTenantId, setSelectedTenantId] = useState<number>(0)
  const [selectedTemplateId, setSelectedTemplateId] = useState<string>('custom')
  const [message, setMessage] = useState('')
  const [charCount, setCharCount] = useState(0)
  const [alert, setAlert] = useState<{ type: 'success' | 'error', message: string } | null>(null)

  const selectedTenant = tenants.find(t => t.id === selectedTenantId)
  const selectedTemplate = SMS_TEMPLATES.find(t => t.id === selectedTemplateId)

  // Update message when template changes
  useEffect(() => {
    if (selectedTemplate && selectedTemplate.id !== 'custom' && selectedTenant) {
      let templateMessage = selectedTemplate.content

      // Replace variables
      templateMessage = templateMessage.replace('{tenantName}', selectedTenant.fullName)
      templateMessage = templateMessage.replace('{unitNumber}', selectedTenant.unitNumber)
      templateMessage = templateMessage.replace('{amount}', selectedTenant.monthlyRent.toLocaleString())
      templateMessage = templateMessage.replace('{date}', new Date().toLocaleDateString())

      setMessage(templateMessage)
    } else if (selectedTemplateId === 'custom') {
      setMessage('')
    }
  }, [selectedTemplateId, selectedTenant])

  // Update character count
  useEffect(() => {
    setCharCount(message.length)
  }, [message])

  const handleSend = async () => {
    if (!selectedTenantId) {
      setAlert({ type: 'error', message: 'Please select a tenant' })
      setTimeout(() => setAlert(null), 3000)
      return
    }

    if (!message.trim()) {
      setAlert({ type: 'error', message: 'Please enter a message' })
      setTimeout(() => setAlert(null), 3000)
      return
    }

    if (!selectedTenant) return

    const response = await sendSms({
      phoneNumber: selectedTenant.phoneNumber,
      message: message.trim(),
      tenantId: selectedTenantId,
    })

    if (response) {
      setAlert({ type: 'success', message: 'SMS sent successfully!' })
      setMessage('')
      setSelectedTenantId(0)
      setSelectedTemplateId('custom')
      setTimeout(() => setAlert(null), 3000)

      if (onSmsSent) {
        onSmsSent()
      }
    } else {
      setAlert({ type: 'error', message: error?.message || 'Failed to send SMS' })
      setTimeout(() => setAlert(null), 3000)
    }
  }

  const tenantOptions = [
    { value: 0, label: 'Select a tenant...' },
    ...tenants.map(t => ({
      value: t.id,
      label: `${t.fullName} - ${t.unitNumber} (${t.phoneNumber})`
    }))
  ]

  const templateOptions = SMS_TEMPLATES.map(t => ({
    value: t.id,
    label: t.name
  }))

  return (
    <Card padding="md">
      <div className="space-y-4">
        <div className="flex items-center gap-2 mb-4">
          <Mail className="text-primary-600 w-5 h-5" />
          <h2 className="text-lg font-semibold text-gray-900">Send SMS</h2>
        </div>

        {alert && (
          <Alert
            type={alert.type}
            message={alert.message}
            onClose={() => setAlert(null)}
          />
        )}

        <Select
          label="Recipient"
          value={selectedTenantId}
          onChange={(e) => setSelectedTenantId(Number(e.target.value))}
          options={tenantOptions}
          fullWidth
          required
        />

        {selectedTenant && (
          <div className="p-3 bg-blue-50 border border-blue-200 rounded-lg">
            <p className="text-sm text-blue-900">
              <strong>Phone:</strong> {selectedTenant.phoneNumber}
            </p>
            <p className="text-sm text-blue-900">
              <strong>Unit:</strong> {selectedTenant.unitNumber} - {selectedTenant.propertyName}
            </p>
            <p className="text-sm text-blue-900">
              <strong>Rent:</strong> KSh {selectedTenant.monthlyRent.toLocaleString()}/month
            </p>
          </div>
        )}

        <Select
          label="Message Template"
          value={selectedTemplateId}
          onChange={(e) => setSelectedTemplateId(e.target.value)}
          options={templateOptions}
          fullWidth
        />

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Message <span className="text-red-500">*</span>
          </label>
          <textarea
            value={message}
            onChange={(e) => setMessage(e.target.value)}
            rows={6}
            maxLength={500}
            placeholder="Type your message here..."
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
            disabled={!selectedTenant}
          />
          <div className="flex justify-between mt-1">
            <p className="text-xs text-gray-500">
              {selectedTemplate && selectedTemplate.id !== 'custom' && (
                <span className="text-blue-600">Using template: {selectedTemplate.name}</span>
              )}
            </p>
            <p className={`text-xs ${charCount > 450 ? 'text-red-600' : 'text-gray-500'}`}>
              {charCount}/500 characters
            </p>
          </div>
        </div>

        <div className="flex gap-3">
          <Button
            variant="primary"
            onClick={handleSend}
            loading={loading}
            disabled={!selectedTenantId || !message.trim()}
            fullWidth
          >
            <Send className="mr-2 w-4 h-4" />
            Send SMS
          </Button>
          <Button
            variant="secondary"
            onClick={() => {
              setMessage('')
              setSelectedTenantId(0)
              setSelectedTemplateId('custom')
            }}
          >
            Clear
          </Button>
        </div>
      </div>
    </Card>
  )
}
