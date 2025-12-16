'use client'

import { useState, useEffect } from 'react'
import Link from 'next/link'
import { FaArrowLeft, FaBell, FaEnvelope, FaSms, FaPaperPlane, FaExclamationTriangle, FaUsers, FaSpinner, FaCheckCircle } from 'react-icons/fa'
import { notificationService } from '@/lib/services/notificationService'

interface Tenant {
  id: number
  fullName: string
  email: string
  phoneNumber: string
  unitNumber: string
  rentAmount: number
}

export default function NotificationsPage() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)

  const [activeTab, setActiveTab] = useState<'single' | 'bulk'>('single')

  // Single tenant notification
  const [selectedTenantId, setSelectedTenantId] = useState<number | ''>('')
  const [notificationType, setNotificationType] = useState<'reminder' | 'overdue'>('reminder')

  // Bulk notification
  const [daysUntilDue, setDaysUntilDue] = useState(3)
  const [includeEmail, setIncludeEmail] = useState(true)
  const [includeSms, setIncludeSms] = useState(true)

  // Mock tenants list - In a real app, fetch this from the backend
  const [tenants, setTenants] = useState<Tenant[]>([])

  useEffect(() => {
    // TODO: Fetch tenants from backend
    // For now, using mock data
    setTenants([])
  }, [])

  const handleSendSingleNotification = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!selectedTenantId) return

    try {
      setLoading(true)
      setError(null)
      setSuccessMessage(null)

      if (notificationType === 'reminder') {
        await notificationService.sendPaymentReminder(Number(selectedTenantId))
        setSuccessMessage('Payment reminder sent successfully!')
      } else {
        await notificationService.sendOverdueNotice(Number(selectedTenantId))
        setSuccessMessage('Overdue notice sent successfully!')
      }

      setSelectedTenantId('')
    } catch (err: any) {
      setError(err.message || 'Failed to send notification')
    } finally {
      setLoading(false)
    }
  }

  const handleSendBulkReminders = async (e: React.FormEvent) => {
    e.preventDefault()

    if (!includeEmail && !includeSms) {
      setError('Please select at least one notification method (Email or SMS)')
      return
    }

    if (!confirm(`Are you sure you want to send payment reminders to all tenants with payments due in ${daysUntilDue} days?`)) {
      return
    }

    try {
      setLoading(true)
      setError(null)
      setSuccessMessage(null)

      const result = await notificationService.sendBulkPaymentReminders(daysUntilDue)
      setSuccessMessage(result.message || 'Successfully sent reminders to tenants!')
    } catch (err: any) {
      setError(err.message || 'Failed to send bulk reminders')
    } finally {
      setLoading(false)
    }
  }

  const handleSendBulkOverdueNotices = async () => {
    if (!includeEmail && !includeSms) {
      setError('Please select at least one notification method (Email or SMS)')
      return
    }

    if (!confirm('Are you sure you want to send overdue notices to all tenants with overdue payments?')) {
      return
    }

    try {
      setLoading(true)
      setError(null)
      setSuccessMessage(null)

      const result = await notificationService.sendBulkOverdueNotices()
      setSuccessMessage(result.message || 'Successfully sent overdue notices!')
    } catch (err: any) {
      setError(err.message || 'Failed to send overdue notices')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Header */}
        <div className="mb-8">
          <Link
            href="/dashboard"
            className="inline-flex items-center text-blue-600 hover:text-blue-800 mb-4"
          >
            <FaArrowLeft className="mr-2" />
            Back to Dashboard
          </Link>
          <h1 className="text-3xl font-bold text-gray-900">Notifications & Reminders</h1>
          <p className="mt-2 text-gray-600">
            Send payment reminders and overdue notices to your tenants via Email and SMS
          </p>
        </div>

        {/* Error/Success Messages */}
        {error && (
          <div className="mb-6 bg-red-50 border border-red-200 text-red-800 px-4 py-3 rounded-lg flex items-start">
            <FaExclamationTriangle className="mr-2 mt-0.5 flex-shrink-0" />
            <span>{error}</span>
          </div>
        )}
        {successMessage && (
          <div className="mb-6 bg-green-50 border border-green-200 text-green-800 px-4 py-3 rounded-lg flex items-start">
            <FaCheckCircle className="mr-2 mt-0.5 flex-shrink-0" />
            <span>{successMessage}</span>
          </div>
        )}

        {/* Info Banner */}
        <div className="mb-6 bg-blue-50 border border-blue-200 rounded-lg p-4">
          <h3 className="font-semibold text-blue-900 mb-2 flex items-center">
            <FaBell className="mr-2" />
            How Notifications Work
          </h3>
          <ul className="list-disc list-inside space-y-1 text-sm text-blue-800">
            <li>Payment reminders are sent to tenants before their rent is due</li>
            <li>Overdue notices are sent to tenants with past-due payments</li>
            <li>Notifications are sent via both Email and SMS (if configured)</li>
            <li>Each notification includes payment amount, due date, and payment instructions</li>
          </ul>
        </div>

        {/* Tabs */}
        <div className="mb-6 border-b border-gray-200">
          <nav className="-mb-px flex space-x-8">
            <button
              onClick={() => setActiveTab('single')}
              className={`py-4 px-1 border-b-2 font-medium text-sm ${
                activeTab === 'single'
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              }`}
            >
              Single Tenant
            </button>
            <button
              onClick={() => setActiveTab('bulk')}
              className={`py-4 px-1 border-b-2 font-medium text-sm flex items-center ${
                activeTab === 'bulk'
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              }`}
            >
              <FaUsers className="mr-2" />
              Bulk Operations
            </button>
          </nav>
        </div>

        {/* Single Tenant Notification */}
        {activeTab === 'single' && (
          <div className="bg-white rounded-lg shadow-md p-6">
            <h2 className="text-xl font-semibold text-gray-900 mb-4 flex items-center">
              <FaPaperPlane className="mr-2 text-blue-600" />
              Send Notification to Single Tenant
            </h2>

            <form onSubmit={handleSendSingleNotification} className="space-y-6">
              <div>
                <label htmlFor="tenant" className="block text-sm font-medium text-gray-700 mb-1">
                  Select Tenant
                </label>
                <select
                  id="tenant"
                  value={selectedTenantId}
                  onChange={(e) => setSelectedTenantId(e.target.value ? Number(e.target.value) : '')}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  required
                >
                  <option value="">-- Select a tenant --</option>
                  {tenants.map((tenant) => (
                    <option key={tenant.id} value={tenant.id}>
                      {tenant.fullName} - Unit {tenant.unitNumber} (KES {tenant.rentAmount.toLocaleString()})
                    </option>
                  ))}
                </select>
                {tenants.length === 0 && (
                  <p className="mt-2 text-sm text-gray-500">
                    No tenants found. Please add tenants to your properties first.
                  </p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Notification Type
                </label>
                <div className="space-y-2">
                  <label className="flex items-center">
                    <input
                      type="radio"
                      name="notificationType"
                      value="reminder"
                      checked={notificationType === 'reminder'}
                      onChange={(e) => setNotificationType('reminder')}
                      className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300"
                    />
                    <span className="ml-2 text-gray-700">
                      Payment Reminder (upcoming rent due)
                    </span>
                  </label>
                  <label className="flex items-center">
                    <input
                      type="radio"
                      name="notificationType"
                      value="overdue"
                      checked={notificationType === 'overdue'}
                      onChange={(e) => setNotificationType('overdue')}
                      className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300"
                    />
                    <span className="ml-2 text-gray-700">
                      Overdue Notice (past-due rent)
                    </span>
                  </label>
                </div>
              </div>

              <button
                type="submit"
                disabled={loading || !selectedTenantId}
                className="w-full bg-blue-600 text-white py-3 px-4 rounded-lg hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed flex items-center justify-center"
              >
                {loading ? (
                  <>
                    <FaSpinner className="animate-spin mr-2" />
                    Sending...
                  </>
                ) : (
                  <>
                    <FaPaperPlane className="mr-2" />
                    Send Notification
                  </>
                )}
              </button>
            </form>
          </div>
        )}

        {/* Bulk Operations */}
        {activeTab === 'bulk' && (
          <div className="space-y-6">
            {/* Bulk Payment Reminders */}
            <div className="bg-white rounded-lg shadow-md p-6">
              <h2 className="text-xl font-semibold text-gray-900 mb-4 flex items-center">
                <FaEnvelope className="mr-2 text-blue-600" />
                Send Bulk Payment Reminders
              </h2>
              <p className="text-gray-600 mb-4">
                Send payment reminders to all tenants with upcoming rent due dates
              </p>

              <form onSubmit={handleSendBulkReminders} className="space-y-4">
                <div>
                  <label htmlFor="daysUntilDue" className="block text-sm font-medium text-gray-700 mb-1">
                    Notify tenants with rent due within
                  </label>
                  <div className="flex items-center gap-2">
                    <input
                      id="daysUntilDue"
                      type="number"
                      value={daysUntilDue}
                      onChange={(e) => setDaysUntilDue(Number(e.target.value))}
                      min="1"
                      max="30"
                      className="w-24 px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    />
                    <span className="text-gray-700">days</span>
                  </div>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Notification Methods
                  </label>
                  <div className="space-y-2">
                    <label className="flex items-center">
                      <input
                        type="checkbox"
                        checked={includeEmail}
                        onChange={(e) => setIncludeEmail(e.target.checked)}
                        className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                      />
                      <FaEnvelope className="ml-2 mr-1 text-gray-600" />
                      <span className="text-gray-700">Email</span>
                    </label>
                    <label className="flex items-center">
                      <input
                        type="checkbox"
                        checked={includeSms}
                        onChange={(e) => setIncludeSms(e.target.checked)}
                        className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                      />
                      <FaSms className="ml-2 mr-1 text-gray-600" />
                      <span className="text-gray-700">SMS</span>
                    </label>
                  </div>
                </div>

                <button
                  type="submit"
                  disabled={loading}
                  className="w-full bg-blue-600 text-white py-3 px-4 rounded-lg hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed flex items-center justify-center"
                >
                  {loading ? (
                    <>
                      <FaSpinner className="animate-spin mr-2" />
                      Sending Reminders...
                    </>
                  ) : (
                    <>
                      <FaPaperPlane className="mr-2" />
                      Send Bulk Reminders
                    </>
                  )}
                </button>
              </form>
            </div>

            {/* Bulk Overdue Notices */}
            <div className="bg-white rounded-lg shadow-md p-6">
              <h2 className="text-xl font-semibold text-gray-900 mb-4 flex items-center">
                <FaExclamationTriangle className="mr-2 text-orange-600" />
                Send Bulk Overdue Notices
              </h2>
              <p className="text-gray-600 mb-4">
                Send overdue notices to all tenants with past-due payments
              </p>

              <div className="bg-orange-50 border border-orange-200 rounded-lg p-4 mb-4">
                <p className="text-sm text-orange-800">
                  <strong>Note:</strong> This will send notifications to all tenants who have payments past their due date. The notice will include late fee information if applicable.
                </p>
              </div>

              <button
                onClick={handleSendBulkOverdueNotices}
                disabled={loading}
                className="w-full bg-orange-600 text-white py-3 px-4 rounded-lg hover:bg-orange-700 disabled:bg-gray-400 disabled:cursor-not-allowed flex items-center justify-center"
              >
                {loading ? (
                  <>
                    <FaSpinner className="animate-spin mr-2" />
                    Sending Overdue Notices...
                  </>
                ) : (
                  <>
                    <FaExclamationTriangle className="mr-2" />
                    Send Bulk Overdue Notices
                  </>
                )}
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}
