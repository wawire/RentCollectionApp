'use client'

import { useState } from 'react'
import { apiClient } from '@/lib/services/api'
import ProtectedRoute from '@/components/auth/ProtectedRoute'
import { Alert, Button, Card, Input, PageHeader } from '@/components/common'

interface ImportResult {
  successCount: number
  failureCount: number
  totalCount: number
  errors: string[]
  successMessages: string[]
  isSuccess: boolean
  summary: string
}

export default function BulkImportPage() {
  const [activeTab, setActiveTab] = useState<'tenants' | 'payments'>('tenants')
  const [file, setFile] = useState<File | null>(null)
  const [propertyId, setPropertyId] = useState('')
  const [loading, setLoading] = useState(false)
  const [result, setResult] = useState<ImportResult | null>(null)
  const [error, setError] = useState('')

  const handleImportTenants = async () => {
    if (!file || !propertyId) {
      setError('Please select a file and enter property ID')
      return
    }

    setLoading(true)
    setError('')
    setResult(null)

    try {
      const formData = new FormData()
      formData.append('file', file)
      formData.append('propertyId', propertyId)

      const response = await apiClient.post('/bulkimport/tenants', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })

      setResult(response.data.data)
    } catch (err: any) {
      setError(err.response?.data?.message || 'Import failed')
    } finally {
      setLoading(false)
    }
  }

  const handleImportPayments = async () => {
    if (!file) {
      setError('Please select a file')
      return
    }

    setLoading(true)
    setError('')
    setResult(null)

    try {
      const formData = new FormData()
      formData.append('file', file)

      const response = await apiClient.post('/bulkimport/payments', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })

      setResult(response.data.data)
    } catch (err: any) {
      setError(err.response?.data?.message || 'Import failed')
    } finally {
      setLoading(false)
    }
  }

  const handleValidate = async () => {
    if (!file) {
      setError('Please select a file')
      return
    }

    setLoading(true)
    setError('')
    setResult(null)

    try {
      const formData = new FormData()
      formData.append('file', file)
      if (activeTab === 'tenants') {
        formData.append('propertyId', propertyId)
      }

      const endpoint = activeTab === 'tenants' ? '/bulkimport/tenants/validate' : '/bulkimport/payments/validate'
      const response = await apiClient.post(endpoint, formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })

      setResult(response.data)
    } catch (err: any) {
      setError(err.response?.data?.message || 'Validation failed')
    } finally {
      setLoading(false)
    }
  }

  const downloadTemplate = (type: 'tenants' | 'payments') => {
    let csvContent = ''
    if (type === 'tenants') {
      csvContent =
        'FirstName,LastName,Email,PhoneNumber,UnitNumber,LeaseStartDate,LeaseEndDate,MonthlyRent,SecurityDeposit,IdNumber\n' +
        'John,Doe,john@example.com,+254712345678,A101,2024-01-01,2025-01-01,30000,60000,12345678\n'
    } else {
      csvContent =
        'TenantEmail,Amount,PaymentDate,PaymentMethod,ReferenceNumber,Notes\n' +
        'john@example.com,30000,2024-01-15,M-Pesa,ABC123,January rent\n'
    }

    const blob = new Blob([csvContent], { type: 'text/csv' })
    const url = window.URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `${type}_template.csv`
    a.click()
  }

  return (
    <ProtectedRoute allowedRoles={['PlatformAdmin', 'Landlord']}>
      <div className="max-w-4xl mx-auto space-y-6">
        <PageHeader
          title="Bulk Import"
          subtitle="Import tenants and payments from CSV files."
          breadcrumbs={[
            { label: 'Dashboard', href: '/dashboard' },
            { label: 'Bulk Import' },
          ]}
        />

        <div className="border-b border-border-muted">
          <div className="flex space-x-8">
            <button
              onClick={() => setActiveTab('tenants')}
              className={`pb-4 px-1 border-b-2 font-medium text-sm transition-colors ${
                activeTab === 'tenants'
                  ? 'border-brand-secondary text-brand-primary'
                  : 'border-transparent text-text-muted hover:text-text-secondary hover:border-border-muted'
              }`}
            >
              Import Tenants
            </button>
            <button
              onClick={() => setActiveTab('payments')}
              className={`pb-4 px-1 border-b-2 font-medium text-sm transition-colors ${
                activeTab === 'payments'
                  ? 'border-brand-secondary text-brand-primary'
                  : 'border-transparent text-text-muted hover:text-text-secondary hover:border-border-muted'
              }`}
            >
              Import Payments
            </button>
          </div>
        </div>

        <Alert
          type="info"
          title="Download Template"
          message="Use the CSV template to format your data correctly."
        />
        <div>
          <Button variant="secondary" size="sm" onClick={() => downloadTemplate(activeTab)}>
            Download {activeTab} CSV template
          </Button>
        </div>

        {error && (
          <Alert type="error" message={error} />
        )}

        <Card padding="md">
          <div className="space-y-4">
            {activeTab === 'tenants' && (
              <Input
                type="number"
                label="Property ID"
                required
                value={propertyId}
                onChange={(e) => setPropertyId(e.target.value)}
                placeholder="Enter property ID"
                fullWidth
              />
            )}

            <div>
              <label className="block text-sm font-medium text-text-secondary mb-1">CSV File *</label>
              <input
                type="file"
                accept=".csv"
                onChange={(e) => setFile(e.target.files?.[0] || null)}
                className="w-full text-sm text-text-muted file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-sm file:font-semibold file:bg-brand-bg file:text-brand-primary hover:file:bg-brand-bg/80"
              />
              {file && (
                <p className="mt-2 text-sm text-text-muted">
                  Selected: {file.name} ({(file.size / 1024).toFixed(2)} KB)
                </p>
              )}
            </div>

            <div className="flex space-x-3">
              <Button
                variant="secondary"
                onClick={handleValidate}
                disabled={loading || !file || (activeTab === 'tenants' && !propertyId)}
              >
                {loading ? 'Validating...' : 'Validate'}
              </Button>
              <Button
                onClick={activeTab === 'tenants' ? handleImportTenants : handleImportPayments}
                disabled={loading || !file || (activeTab === 'tenants' && !propertyId)}
              >
                {loading ? 'Importing...' : 'Import'}
              </Button>
            </div>
          </div>
        </Card>

        {result && (
          <Card padding="md">
            <h3 className="text-lg font-semibold text-text-primary mb-4">Import Results</h3>

            <div className="grid grid-cols-3 gap-4 mb-6">
              <div className="bg-brand-bg/60 p-4 rounded-md">
                <p className="text-sm text-text-muted">Total</p>
                <p className="text-2xl font-semibold text-text-primary">{result.totalCount}</p>
              </div>
              <div className="bg-state-success/10 p-4 rounded-md">
                <p className="text-sm text-state-success">Success</p>
                <p className="text-2xl font-semibold text-text-primary">{result.successCount}</p>
              </div>
              <div className="bg-state-error/10 p-4 rounded-md">
                <p className="text-sm text-state-error">Failed</p>
                <p className="text-2xl font-semibold text-text-primary">{result.failureCount}</p>
              </div>
            </div>

            <p className="text-sm font-medium text-text-primary mb-4">{result.summary}</p>

            {result.errors.length > 0 && (
              <div className="mb-4">
                <h4 className="text-sm font-semibold text-state-error mb-2">Errors</h4>
                <div className="bg-state-error/10 border border-state-error/30 rounded-md p-3 max-h-60 overflow-y-auto">
                  {result.errors.map((issue, index) => (
                    <p key={index} className="text-sm text-text-secondary mb-1">
                      {issue}
                    </p>
                  ))}
                </div>
              </div>
            )}

            {result.successMessages.length > 0 && (
              <div>
                <h4 className="text-sm font-semibold text-state-success mb-2">Successfully Imported</h4>
                <div className="bg-state-success/10 border border-state-success/30 rounded-md p-3 max-h-60 overflow-y-auto">
                  {result.successMessages.map((msg, index) => (
                    <p key={index} className="text-sm text-text-secondary mb-1">
                      OK: {msg}
                    </p>
                  ))}
                </div>
              </div>
            )}
          </Card>
        )}

        <Card padding="md">
          <h4 className="text-sm font-semibold text-text-primary mb-2">Instructions</h4>
          <ul className="text-sm text-text-secondary space-y-1 list-disc list-inside">
            <li>Download the CSV template and fill in your data</li>
            <li>Ensure all required fields are filled correctly</li>
            <li>Dates should be in YYYY-MM-DD format (e.g., 2024-01-15)</li>
            <li>Use "Validate" to check for errors before importing</li>
            <li>Review the results carefully after import</li>
          </ul>
        </Card>
      </div>
    </ProtectedRoute>
  )
}

