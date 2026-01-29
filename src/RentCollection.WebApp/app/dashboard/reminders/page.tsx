'use client'

import { useState, useEffect } from 'react'
import Link from 'next/link'
import { ArrowLeft, Bell, CheckCircle2, Clock, History, Loader2, Mail, MessageSquare, Save } from 'lucide-react'
import { rentReminderService, ReminderSettings, UpdateReminderSettings } from '@/lib/services/rentReminderService'
import ProtectedRoute from '@/components/auth/ProtectedRoute'

export default function RentRemindersPage() {
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)
  const [settings, setSettings] = useState<ReminderSettings | null>(null)
  const [activeTab, setActiveTab] = useState<'settings' | 'history'>('settings')

  useEffect(() => {
    loadSettings()
  }, [])

  const loadSettings = async () => {
    try {
      setLoading(true)
      setError(null)
      const response = await rentReminderService.getSettings()
      if (response.success) {
        setSettings(response.data)
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load reminder settings')
    } finally {
      setLoading(false)
    }
  }

  const handleToggleSetting = (field: keyof ReminderSettings) => {
    if (!settings) return
    setSettings({ ...settings, [field]: !settings[field] })
  }

  const handleChannelChange = (channel: 'SMS' | 'Email' | 'Both') => {
    if (!settings) return
    setSettings({ ...settings, defaultChannel: channel })
  }

  const handleSaveSettings = async () => {
    if (!settings) return

    try {
      setSaving(true)
      setError(null)
      setSuccessMessage(null)

      const updateDto: UpdateReminderSettings = {
        isEnabled: settings.isEnabled,
        defaultChannel: settings.defaultChannel,
        sevenDaysBeforeEnabled: settings.sevenDaysBeforeEnabled,
        threeDaysBeforeEnabled: settings.threeDaysBeforeEnabled,
        oneDayBeforeEnabled: settings.oneDayBeforeEnabled,
        onDueDateEnabled: settings.onDueDateEnabled,
        oneDayOverdueEnabled: settings.oneDayOverdueEnabled,
        threeDaysOverdueEnabled: settings.threeDaysOverdueEnabled,
        sevenDaysOverdueEnabled: settings.sevenDaysOverdueEnabled,
        sevenDaysBeforeTemplate: settings.sevenDaysBeforeTemplate,
        threeDaysBeforeTemplate: settings.threeDaysBeforeTemplate,
        oneDayBeforeTemplate: settings.oneDayBeforeTemplate,
        onDueDateTemplate: settings.onDueDateTemplate,
        oneDayOverdueTemplate: settings.oneDayOverdueTemplate,
        threeDaysOverdueTemplate: settings.threeDaysOverdueTemplate,
        sevenDaysOverdueTemplate: settings.sevenDaysOverdueTemplate,
        quietHoursStart: settings.quietHoursStart,
        quietHoursEnd: settings.quietHoursEnd
      }

      const response = await rentReminderService.updateSettings(updateDto)
      if (response.success) {
        setSuccessMessage(response.message)
        setSettings(response.data)
      }
    } catch (err: any) {
      setError(err.message || 'Failed to save reminder settings')
    } finally {
      setSaving(false)
    }
  }

  const content = loading ? (
    <div className="flex justify-center items-center min-h-screen">
      <Loader2 className="animate-spin w-10 h-10 text-blue-600" />
    </div>
  ) : (
    <div className="p-6 max-w-7xl mx-auto">
      {/* Header */}
      <div className="mb-6">
        <Link href="/dashboard" className="text-blue-600 hover:text-blue-800 flex items-center gap-2 mb-4">
          <ArrowLeft className="w-4 h-4" /> Back to Dashboard
        </Link>
        <div className="flex justify-between items-center">
          <div>
            <h1 className="text-3xl font-bold text-gray-800">Automated Rent Reminders</h1>
            <p className="text-gray-600 mt-1">Configure automated reminder notifications for your tenants</p>
          </div>
          <Link
            href="/dashboard/reminders/history"
            className="flex items-center gap-2 px-4 py-2 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-lg transition"
          >
            <History className="w-4 h-4" /> View History
          </Link>
        </div>
      </div>

      {/* Error/Success Messages */}
      {error && (
        <div className="mb-4 p-4 bg-red-50 border border-red-200 text-red-800 rounded-lg">
          {error}
        </div>
      )}
      {successMessage && (
        <div className="mb-4 p-4 bg-green-50 border border-green-200 text-green-800 rounded-lg flex items-center gap-2">
          <CheckCircle2 className="w-4 h-4" /> {successMessage}
        </div>
      )}

      {settings && (
        <div className="bg-white rounded-lg shadow-sm border border-gray-200">
          {/* Master Toggle */}
          <div className="p-6 border-b border-gray-200">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-3">
                <Bell className="w-6 h-6 text-blue-600" />
                <div>
                  <h2 className="text-xl font-semibold text-gray-800">Enable Automated Reminders</h2>
                  <p className="text-sm text-gray-600">Turn on/off automated rent reminder system</p>
                </div>
              </div>
              <label className="relative inline-flex items-center cursor-pointer">
                <input
                  type="checkbox"
                  checked={settings.isEnabled}
                  onChange={() => handleToggleSetting('isEnabled')}
                  className="sr-only peer"
                />
                <div className="w-14 h-7 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-blue-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-0.5 after:left-[4px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-6 after:w-6 after:transition-all peer-checked:bg-blue-600"></div>
              </label>
            </div>
          </div>

          {/* Default Channel */}
          <div className="p-6 border-b border-gray-200">
            <h3 className="text-lg font-semibold text-gray-800 mb-4">Default Notification Channel</h3>
            <div className="flex gap-4">
                <button
                  onClick={() => handleChannelChange('SMS')}
                  className={`flex-1 p-4 border-2 rounded-lg transition ${
                    settings.defaultChannel === 'SMS'
                      ? 'border-blue-600 bg-blue-50'
                      : 'border-gray-200 hover:border-gray-300'
                  }`}
                >
                  <MessageSquare className="w-6 h-6 mx-auto mb-2 text-blue-600" />
                  <div className="font-medium">SMS Only</div>
                </button>
                <button
                  onClick={() => handleChannelChange('Email')}
                  className={`flex-1 p-4 border-2 rounded-lg transition ${
                    settings.defaultChannel === 'Email'
                      ? 'border-blue-600 bg-blue-50'
                      : 'border-gray-200 hover:border-gray-300'
                  }`}
                >
                  <Mail className="w-6 h-6 mx-auto mb-2 text-blue-600" />
                  <div className="font-medium">Email Only</div>
                </button>
                <button
                  onClick={() => handleChannelChange('Both')}
                  className={`flex-1 p-4 border-2 rounded-lg transition ${
                    settings.defaultChannel === 'Both'
                      ? 'border-blue-600 bg-blue-50'
                      : 'border-gray-200 hover:border-gray-300'
                  }`}
                >
                  <div className="flex justify-center gap-2 text-2xl mb-2 text-blue-600">
                    <MessageSquare className="w-6 h-6" />
                    <Mail className="w-6 h-6" />
                  </div>
                  <div className="font-medium">Both</div>
                </button>
              </div>
            </div>

          {/* Reminder Schedule */}
          <div className="p-6 border-b border-gray-200">
            <h3 className="text-lg font-semibold text-gray-800 mb-4">Reminder Schedule</h3>
            <div className="space-y-3">
              {[
                { key: 'sevenDaysBeforeEnabled', label: '7 Days Before Due Date', type: 'before' },
                { key: 'threeDaysBeforeEnabled', label: '3 Days Before Due Date', type: 'before' },
                { key: 'oneDayBeforeEnabled', label: '1 Day Before Due Date', type: 'before' },
                { key: 'onDueDateEnabled', label: 'On Due Date', type: 'due' },
                { key: 'oneDayOverdueEnabled', label: '1 Day Overdue', type: 'overdue' },
                { key: 'threeDaysOverdueEnabled', label: '3 Days Overdue', type: 'overdue' },
                { key: 'sevenDaysOverdueEnabled', label: '7 Days Overdue', type: 'overdue' }
              ].map(({ key, label, type }) => (
                <div key={key} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                  <div className="flex items-center gap-3">
                    <Clock className={`${
                      type === 'before' ? 'text-blue-600' :
                      type === 'due' ? 'text-yellow-600' : 'text-red-600'
                    }`} />
                    <span className="font-medium text-gray-700">{label}</span>
                  </div>
                  <label className="relative inline-flex items-center cursor-pointer">
                    <input
                      type="checkbox"
                      checked={settings[key as keyof ReminderSettings] as boolean}
                      onChange={() => handleToggleSetting(key as keyof ReminderSettings)}
                      className="sr-only peer"
                      disabled={!settings.isEnabled}
                    />
                    <div className="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-blue-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-blue-600 peer-disabled:opacity-50"></div>
                  </label>
                </div>
              ))}
            </div>
          </div>

          {/* Quiet Hours */}
          <div className="p-6 border-b border-gray-200">
            <h3 className="text-lg font-semibold text-gray-800 mb-4">Quiet Hours</h3>
            <p className="text-sm text-gray-600 mb-4">No reminders will be sent during these hours (currently set to 10 PM - 8 AM)</p>
            <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 text-sm text-blue-800">
              <Clock className="inline mr-2 w-4 h-4" />
              Advanced quiet hours configuration coming soon. Default: 10:00 PM - 8:00 AM
            </div>
          </div>

          {/* Save Button */}
          <div className="p-6">
            <button
              onClick={handleSaveSettings}
              disabled={saving || !settings.isEnabled}
              className="w-full flex items-center justify-center gap-2 px-6 py-3 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium transition disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {saving ? (
                <>
                  <Loader2 className="animate-spin w-4 h-4" />
                  Saving Settings...
                </>
              ) : (
                <>
                  <Save className="w-4 h-4" />
                  Save Reminder Settings
                </>
              )}
            </button>
          </div>
        </div>
      )}
    </div>
  )

  return (
    <ProtectedRoute allowedRoles={['PlatformAdmin', 'Landlord']}>
      {content}
    </ProtectedRoute>
  )
}

