'use client'

import { useState, useMemo } from 'react'
import { Card, Button, SearchBar, Select, Alert } from '@/components/common'
import { useGetActiveTenants, useSendSms } from '@/lib/hooks'
import { CheckSquare, Send, Square, Users } from 'lucide-react'

interface BulkSmsProps {
  onSmsSent?: () => void
}

export default function BulkSms({ onSmsSent }: BulkSmsProps) {
  const { data: tenants } = useGetActiveTenants()
  const { sendSms, loading } = useSendSms()

  const [selectedTenantIds, setSelectedTenantIds] = useState<Set<number>>(new Set())
  const [message, setMessage] = useState('')
  const [searchTerm, setSearchTerm] = useState('')
  const [propertyFilter, setPropertyFilter] = useState<string>('all')
  const [alert, setAlert] = useState<{ type: 'success' | 'error', message: string } | null>(null)
  const [sendingProgress, setSendingProgress] = useState<{ current: number; total: number } | null>(null)

  // Get unique properties
  const properties = useMemo(() => {
    const uniqueProps = Array.from(new Set(tenants.map(t => t.propertyName)))
    return uniqueProps.map(name => ({ value: name, label: name }))
  }, [tenants])

  const propertyOptions = [
    { value: 'all', label: 'All Properties' },
    ...properties
  ]

  // Filter tenants
  const filteredTenants = useMemo(() => {
    return tenants.filter(tenant => {
      const searchLower = searchTerm.toLowerCase()
      const matchesSearch =
        tenant.fullName.toLowerCase().includes(searchLower) ||
        tenant.phoneNumber.includes(searchLower) ||
        tenant.unitNumber.toLowerCase().includes(searchLower)

      const matchesProperty =
        propertyFilter === 'all' || tenant.propertyName === propertyFilter

      return matchesSearch && matchesProperty
    })
  }, [tenants, searchTerm, propertyFilter])

  const handleSelectAll = () => {
    if (selectedTenantIds.size === filteredTenants.length) {
      setSelectedTenantIds(new Set())
    } else {
      setSelectedTenantIds(new Set(filteredTenants.map(t => t.id)))
    }
  }

  const handleToggleTenant = (tenantId: number) => {
    const newSet = new Set(selectedTenantIds)
    if (newSet.has(tenantId)) {
      newSet.delete(tenantId)
    } else {
      newSet.add(tenantId)
    }
    setSelectedTenantIds(newSet)
  }

  const handleSendBulk = async () => {
    if (selectedTenantIds.size === 0) {
      setAlert({ type: 'error', message: 'Please select at least one tenant' })
      setTimeout(() => setAlert(null), 3000)
      return
    }

    if (!message.trim()) {
      setAlert({ type: 'error', message: 'Please enter a message' })
      setTimeout(() => setAlert(null), 3000)
      return
    }

    const selectedTenants = tenants.filter(t => selectedTenantIds.has(t.id))
    setSendingProgress({ current: 0, total: selectedTenants.length })

    let successCount = 0
    let failCount = 0

    // Send SMS to each tenant sequentially
    for (let i = 0; i < selectedTenants.length; i++) {
      const tenant = selectedTenants[i]
      setSendingProgress({ current: i + 1, total: selectedTenants.length })

      const response = await sendSms({
        phoneNumber: tenant.phoneNumber,
        message: message.trim(),
        tenantId: tenant.id,
      })

      if (response) {
        successCount++
      } else {
        failCount++
      }

      // Small delay to avoid overwhelming the system
      await new Promise(resolve => setTimeout(resolve, 500))
    }

    setSendingProgress(null)

    if (failCount === 0) {
      setAlert({
        type: 'success',
        message: `Successfully sent ${successCount} messages!`
      })
      setMessage('')
      setSelectedTenantIds(new Set())
      if (onSmsSent) onSmsSent()
    } else {
      setAlert({
        type: 'error',
        message: `Sent ${successCount} messages, ${failCount} failed`
      })
    }

    setTimeout(() => setAlert(null), 5000)
  }

  const allSelected = filteredTenants.length > 0 && selectedTenantIds.size === filteredTenants.length

  return (
    <Card padding="md">
      <div className="space-y-4">
        <div className="flex items-center gap-2 mb-4">
          <Users className="text-primary-600 w-5 h-5" />
          <h2 className="text-lg font-semibold text-gray-900">Bulk SMS</h2>
        </div>

        {alert && (
          <Alert
            type={alert.type}
            message={alert.message}
            onClose={() => setAlert(null)}
          />
        )}

        {/* Message Input */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Message <span className="text-red-500">*</span>
          </label>
          <textarea
            value={message}
            onChange={(e) => setMessage(e.target.value)}
            rows={5}
            maxLength={500}
            placeholder="Type your message here... This will be sent to all selected tenants."
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
          />
          <p className={`text-xs text-right mt-1 ${message.length > 450 ? 'text-red-600' : 'text-gray-500'}`}>
            {message.length}/500 characters
          </p>
        </div>

        {/* Filters */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <SearchBar
            value={searchTerm}
            onChange={setSearchTerm}
            placeholder="Search tenants..."
            className="w-full"
          />
          <Select
            label=""
            value={propertyFilter}
            onChange={(e) => setPropertyFilter(e.target.value)}
            options={propertyOptions}
            fullWidth
          />
        </div>

        {/* Tenant Selection */}
        <div>
          <div className="flex items-center justify-between mb-3">
            <h3 className="text-sm font-semibold text-gray-700">
              Select Recipients ({selectedTenantIds.size} selected)
            </h3>
            <button
              onClick={handleSelectAll}
              className="flex items-center gap-2 text-sm text-primary-600 hover:text-primary-700 font-medium"
            >
              {allSelected ? <CheckSquare className="w-4 h-4" /> : <Square className="w-4 h-4" />}
              {allSelected ? 'Deselect All' : 'Select All'}
            </button>
          </div>

          <div className="border border-gray-200 rounded-lg max-h-96 overflow-y-auto">
            {filteredTenants.length === 0 ? (
              <div className="p-8 text-center text-gray-500">
                No tenants found
              </div>
            ) : (
              <div className="divide-y divide-gray-200">
                {filteredTenants.map((tenant) => {
                  const isSelected = selectedTenantIds.has(tenant.id)
                  return (
                    <label
                      key={tenant.id}
                      className={`flex items-center gap-3 p-4 cursor-pointer hover:bg-gray-50 transition-colors ${
                        isSelected ? 'bg-blue-50' : ''
                      }`}
                    >
                      <input
                        type="checkbox"
                        checked={isSelected}
                        onChange={() => handleToggleTenant(tenant.id)}
                        className="w-5 h-5 text-primary-600 rounded focus:ring-primary-500"
                      />
                      <div className="flex-1">
                        <p className="text-sm font-medium text-gray-900">{tenant.fullName}</p>
                        <p className="text-xs text-gray-500">
                          {tenant.unitNumber} - {tenant.propertyName} | {tenant.phoneNumber}
                        </p>
                      </div>
                    </label>
                  )
                })}
              </div>
            )}
          </div>
        </div>

        {/* Send Button */}
        <div>
          {sendingProgress && (
            <div className="mb-3">
              <div className="flex justify-between text-sm text-gray-600 mb-1">
                <span>Sending messages...</span>
                <span>{sendingProgress.current} / {sendingProgress.total}</span>
              </div>
              <div className="w-full bg-gray-200 rounded-full h-2">
                <div
                  className="bg-primary-600 h-2 rounded-full transition-all duration-300"
                  style={{ width: `${(sendingProgress.current / sendingProgress.total) * 100}%` }}
                />
              </div>
            </div>
          )}

          <Button
            variant="primary"
            onClick={handleSendBulk}
            loading={loading || !!sendingProgress}
            disabled={selectedTenantIds.size === 0 || !message.trim()}
            fullWidth
          >
            <Send className="mr-2 w-4 h-4" />
            Send to {selectedTenantIds.size} Tenant{selectedTenantIds.size !== 1 ? 's' : ''}
          </Button>
        </div>
      </div>
    </Card>
  )
}
