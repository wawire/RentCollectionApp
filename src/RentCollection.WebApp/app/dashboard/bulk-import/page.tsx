'use client'

import { useState } from 'react'
import { apiClient } from '@/lib/services/api'

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
    <div className="max-w-4xl mx-auto">
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-gray-900">Bulk Import</h1>
        <p className="mt-2 text-gray-600">Import tenants and payments from CSV files</p>
      </div>

      {/* Tabs */}
      <div className="mb-6 border-b border-gray-200">
        <div className="flex space-x-8">
          <button
            onClick={() => setActiveTab('tenants')}
            className={`pb-4 px-1 border-b-2 font-medium text-sm ${
              activeTab === 'tenants'
                ? 'border-blue-500 text-blue-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            Import Tenants
          </button>
          <button
            onClick={() => setActiveTab('payments')}
            className={`pb-4 px-1 border-b-2 font-medium text-sm ${
              activeTab === 'payments'
                ? 'border-blue-500 text-blue-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            Import Payments
          </button>
        </div>
      </div>

      {/* Template Download */}
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-6">
        <p className="text-sm text-blue-800 mb-2">
          <strong>Download Template:</strong> Use the CSV template to format your data correctly
        </p>
        <button
          onClick={() => downloadTemplate(activeTab)}
          className="text-sm text-blue-600 hover:text-blue-800 underline"
        >
          Download {activeTab} CSV template
        </button>
      </div>

      {error && (
        <div className="mb-6 bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">{error}</div>
      )}

      {/* File Upload Form */}
      <div className="bg-white shadow rounded-lg p-6 mb-6">
        <div className="space-y-4">
          {activeTab === 'tenants' && (
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Property ID *</label>
              <input
                type="number"
                value={propertyId}
                onChange={(e) => setPropertyId(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                placeholder="Enter property ID"
              />
            </div>
          )}

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">CSV File *</label>
            <input
              type="file"
              accept=".csv"
              onChange={(e) => setFile(e.target.files?.[0] || null)}
              className="w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-sm file:font-semibold file:bg-blue-50 file:text-blue-700 hover:file:bg-blue-100"
            />
            {file && (
              <p className="mt-2 text-sm text-gray-600">
                Selected: {file.name} ({(file.size / 1024).toFixed(2)} KB)
              </p>
            )}
          </div>

          <div className="flex space-x-3">
            <button
              onClick={handleValidate}
              disabled={loading || !file || (activeTab === 'tenants' && !propertyId)}
              className="px-4 py-2 bg-gray-600 text-white rounded-md hover:bg-gray-700 disabled:bg-gray-400"
            >
              {loading ? 'Validating...' : 'Validate'}
            </button>
            <button
              onClick={activeTab === 'tenants' ? handleImportTenants : handleImportPayments}
              disabled={loading || !file || (activeTab === 'tenants' && !propertyId)}
              className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:bg-gray-400"
            >
              {loading ? 'Importing...' : 'Import'}
            </button>
          </div>
        </div>
      </div>

      {/* Results */}
      {result && (
        <div className="bg-white shadow rounded-lg p-6">
          <h3 className="text-lg font-semibold mb-4">Import Results</h3>

          <div className="grid grid-cols-3 gap-4 mb-6">
            <div className="bg-gray-50 p-4 rounded-md">
              <p className="text-sm text-gray-600">Total</p>
              <p className="text-2xl font-bold text-gray-900">{result.totalCount}</p>
            </div>
            <div className="bg-green-50 p-4 rounded-md">
              <p className="text-sm text-green-600">Success</p>
              <p className="text-2xl font-bold text-green-900">{result.successCount}</p>
            </div>
            <div className="bg-red-50 p-4 rounded-md">
              <p className="text-sm text-red-600">Failed</p>
              <p className="text-2xl font-bold text-red-900">{result.failureCount}</p>
            </div>
          </div>

          <p className="text-sm font-medium text-gray-900 mb-4">{result.summary}</p>

          {result.errors.length > 0 && (
            <div className="mb-4">
              <h4 className="text-sm font-semibold text-red-700 mb-2">Errors:</h4>
              <div className="bg-red-50 border border-red-200 rounded-md p-3 max-h-60 overflow-y-auto">
                {result.errors.map((error, index) => (
                  <p key={index} className="text-sm text-red-700 mb-1">
                    {error}
                  </p>
                ))}
              </div>
            </div>
          )}

          {result.successMessages.length > 0 && (
            <div>
              <h4 className="text-sm font-semibold text-green-700 mb-2">Successfully Imported:</h4>
              <div className="bg-green-50 border border-green-200 rounded-md p-3 max-h-60 overflow-y-auto">
                {result.successMessages.map((msg, index) => (
                  <p key={index} className="text-sm text-green-700 mb-1">
                    ✓ {msg}
                  </p>
                ))}
              </div>
            </div>
          )}
        </div>
      )}

      {/* Instructions */}
      <div className="mt-6 bg-gray-50 rounded-lg p-4">
        <h4 className="text-sm font-semibold text-gray-900 mb-2">Instructions:</h4>
        <ul className="text-sm text-gray-600 space-y-1 list-disc list-inside">
          <li>Download the CSV template and fill in your data</li>
          <li>Ensure all required fields are filled correctly</li>
          <li>Dates should be in YYYY-MM-DD format (e.g., 2024-01-15)</li>
          <li>Use "Validate" to check for errors before importing</li>
          <li>Review the results carefully after import</li>
        </ul>
      </div>
    </div>
  )
}
