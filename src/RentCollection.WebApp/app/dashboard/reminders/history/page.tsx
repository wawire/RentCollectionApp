'use client'

import { useState, useEffect } from 'react'
import Link from 'next/link'
import { AlertTriangle, ArrowLeft, BarChart3, CheckCircle2, Clock, Loader2, Send, Settings, X } from 'lucide-react'
import { rentReminderService, RentReminder, ReminderStatistics } from '@/lib/services/rentReminderService'
import ProtectedRoute from '@/components/auth/ProtectedRoute'

export default function ReminderHistoryPage() {
  const [loading, setLoading] = useState(true)
  const [reminders, setReminders] = useState<RentReminder[]>([])
  const [statistics, setStatistics] = useState<ReminderStatistics | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [filter, setFilter] = useState<'all' | 'sent' | 'scheduled' | 'failed'>('all')

  useEffect(() => {
    loadData()
  }, [])

  const loadData = async () => {
    try {
      setLoading(true)
      setError(null)

      // Load both history and statistics
      const [historyResponse, statsResponse] = await Promise.all([
        rentReminderService.getHistory(),
        rentReminderService.getStatistics()
      ])

      if (historyResponse.success) {
        setReminders(historyResponse.data)
      }
      if (statsResponse.success) {
        setStatistics(statsResponse.data)
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load reminder history')
    } finally {
      setLoading(false)
    }
  }

  const getStatusIcon = (status: string) => {
    switch (status.toLowerCase()) {
      case 'sent':
        return <CheckCircle2 className="text-green-600 w-4 h-4" />
      case 'scheduled':
        return <Clock className="text-blue-600 w-4 h-4" />
      case 'failed':
        return <AlertTriangle className="text-red-600 w-4 h-4" />
      case 'cancelled':
        return <X className="text-gray-600 w-4 h-4" />
      default:
        return <Clock className="text-gray-600 w-4 h-4" />
    }
  }

  const getStatusBadge = (status: string) => {
    const baseClasses = "px-2 py-1 rounded-full text-xs font-medium"
    switch (status.toLowerCase()) {
      case 'sent':
        return `${baseClasses} bg-green-100 text-green-800`
      case 'scheduled':
        return `${baseClasses} bg-blue-100 text-blue-800`
      case 'failed':
        return `${baseClasses} bg-red-100 text-red-800`
      case 'cancelled':
        return `${baseClasses} bg-gray-100 text-gray-800`
      default:
        return `${baseClasses} bg-gray-100 text-gray-800`
    }
  }

  const filteredReminders = reminders.filter(r => {
    if (filter === 'all') return true
    return r.status.toLowerCase() === filter
  })

  const content = loading ? (
    <div className="flex justify-center items-center min-h-screen">
      <Loader2 className="animate-spin w-10 h-10 text-blue-600" />
    </div>
  ) : (
    <div className="p-6 max-w-7xl mx-auto">
      {/* Header */}
      <div className="mb-6">
        <Link href="/dashboard/reminders" className="text-blue-600 hover:text-blue-800 flex items-center gap-2 mb-4">
          <ArrowLeft className="w-4 h-4" /> Back to Settings
        </Link>
        <div className="flex justify-between items-center">
          <div>
            <h1 className="text-3xl font-bold text-gray-800">Reminder History</h1>
            <p className="text-gray-600 mt-1">View past and scheduled rent reminders</p>
          </div>
          <Link
            href="/dashboard/reminders"
            className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg transition"
          >
            <Settings className="w-4 h-4" /> Settings
          </Link>
        </div>
      </div>

      {/* Error Message */}
      {error && (
        <div className="mb-4 p-4 bg-red-50 border border-red-200 text-red-800 rounded-lg">
          {error}
        </div>
      )}

      {/* Statistics Cards */}
      {statistics && (
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
          <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-200">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">Total Reminders</p>
                <p className="text-3xl font-bold text-gray-800">{statistics.totalReminders}</p>
              </div>
              <BarChart3 className="text-3xl text-blue-600" />
            </div>
          </div>
          <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-200">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">Sent Successfully</p>
                <p className="text-3xl font-bold text-green-600">{statistics.sentReminders}</p>
              </div>
              <CheckCircle2 className="text-3xl text-green-600" />
            </div>
          </div>
          <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-200">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">Scheduled</p>
                <p className="text-3xl font-bold text-blue-600">{statistics.scheduledReminders}</p>
              </div>
              <Clock className="text-3xl text-blue-600" />
            </div>
          </div>
          <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-200">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">Success Rate</p>
                <p className="text-3xl font-bold text-blue-600">{statistics.successRate.toFixed(1)}%</p>
              </div>
              <Send className="text-3xl text-blue-600" />
            </div>
          </div>
        </div>
      )}

      {/* Filter Tabs */}
      <div className="bg-white rounded-lg shadow-sm border border-gray-200 mb-6">
        <div className="flex border-b border-gray-200">
          {[
            { key: 'all', label: 'All' },
            { key: 'sent', label: 'Sent' },
            { key: 'scheduled', label: 'Scheduled' },
            { key: 'failed', label: 'Failed' }
          ].map(tab => (
            <button
              key={tab.key}
              onClick={() => setFilter(tab.key as typeof filter)}
              className={`px-6 py-3 font-medium transition ${
                filter === tab.key
                  ? 'text-blue-600 border-b-2 border-blue-600'
                  : 'text-gray-600 hover:text-gray-800'
              }`}
            >
              {tab.label}
            </button>
          ))}
        </div>

        {/* Reminders Table */}
        <div className="overflow-x-auto">
          {filteredReminders.length === 0 ? (
            <div className="p-12 text-center text-gray-500">
              <Clock className="text-5xl mx-auto mb-4 text-gray-300" />
              <p className="text-lg">No reminders found</p>
              <p className="text-sm mt-2">Reminders will appear here once they are scheduled or sent</p>
            </div>
          ) : (
            <table className="w-full">
              <thead className="bg-gray-50 border-b border-gray-200">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Tenant</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Property / Unit</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Type</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Due Date</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Scheduled</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Amount</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredReminders.map(reminder => (
                  <tr key={reminder.id} className="hover:bg-gray-50 transition">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center gap-2">
                        {getStatusIcon(reminder.status)}
                        <span className={getStatusBadge(reminder.status)}>{reminder.statusDisplay}</span>
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="text-sm font-medium text-gray-900">{reminder.tenantName}</div>
                      <div className="text-xs text-gray-500">{reminder.tenantPhone}</div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="text-sm text-gray-900">{reminder.propertyName}</div>
                      <div className="text-xs text-gray-500">Unit {reminder.unitNumber}</div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className="text-sm text-gray-900">{reminder.reminderTypeDisplay}</span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className="text-sm text-gray-900">
                        {new Date(reminder.rentDueDate).toLocaleDateString()}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className="text-sm text-gray-900">
                        {new Date(reminder.scheduledDate).toLocaleDateString()}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className="text-sm font-medium text-gray-900">
                        KSh {reminder.rentAmount.toLocaleString()}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </div>
    </div>
  )

  return (
    <ProtectedRoute allowedRoles={['PlatformAdmin', 'Landlord']}>
      {content}
    </ProtectedRoute>
  )
}

