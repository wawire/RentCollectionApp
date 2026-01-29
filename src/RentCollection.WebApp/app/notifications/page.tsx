'use client'

import { useState, useEffect } from 'react'
import SmsForm from '@/components/notifications/SmsForm'
import SmsHistory from '@/components/notifications/SmsHistory'
import BulkSms from '@/components/notifications/BulkSms'
import { SmsHistoryItem } from '@/lib/types'
import { Bell, MessageSquare, Users } from 'lucide-react'

type TabType = 'send' | 'bulk' | 'history'

const STORAGE_KEY = 'sms_history'

export default function NotificationsPage() {
  const [activeTab, setActiveTab] = useState<TabType>('send')
  const [smsHistory, setSmsHistory] = useState<SmsHistoryItem[]>([])

  // Load history from localStorage on mount
  useEffect(() => {
    const stored = localStorage.getItem(STORAGE_KEY)
    if (stored) {
      try {
        setSmsHistory(JSON.parse(stored))
      } catch (e) {
        console.error('Failed to load SMS history', e)
      }
    }
  }, [])

  // Save history to localStorage whenever it changes
  useEffect(() => {
    if (smsHistory.length > 0) {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(smsHistory))
    }
  }, [smsHistory])

  const handleSmsSent = () => {
    // Add a new entry to history (in a real app, this would come from the backend)
    const newEntry: SmsHistoryItem = {
      id: Date.now().toString(),
      recipientName: 'Recipient', // This would come from the actual send response
      phoneNumber: '0700000000',
      message: 'Message sent',
      status: 'sent',
      sentAt: new Date().toISOString(),
    }
    setSmsHistory(prev => [newEntry, ...prev])
  }

  const tabs = [
    { id: 'send' as TabType, label: 'Send SMS', icon: MessageSquare },
    { id: 'bulk' as TabType, label: 'Bulk SMS', icon: Users },
    { id: 'history' as TabType, label: 'SMS History', icon: Bell },
  ]

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-serif font-normal text-primary tracking-wide">Notifications & SMS</h1>
        <p className="text-primary/60 mt-2 tracking-wide">Send messages and track communication with tenants</p>
      </div>

      {/* Tabs */}
      <div className="border-b border-gray-200">
        <nav className="flex -mb-px">
          {tabs.map((tab) => {
            const Icon = tab.icon
            const isActive = activeTab === tab.id
            return (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id)}
                className={`
                  flex items-center px-6 py-4 text-sm font-medium border-b-2 transition-colors
                  ${
                    isActive
                      ? 'border-primary-500 text-primary-600'
                      : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                  }
                `}
              >
                <Icon className="mr-2" />
                {tab.label}
                {tab.id === 'history' && smsHistory.length > 0 && (
                  <span className="ml-2 px-2 py-0.5 text-xs font-semibold bg-primary-100 text-primary-800 rounded-full">
                    {smsHistory.length}
                  </span>
                )}
              </button>
            )
          })}
        </nav>
      </div>

      {/* Tab Content */}
      <div className="mt-6">
        {activeTab === 'send' && (
          <div className="max-w-3xl">
            <SmsForm onSmsSent={handleSmsSent} />
          </div>
        )}
        {activeTab === 'bulk' && (
          <div className="max-w-4xl">
            <BulkSms onSmsSent={handleSmsSent} />
          </div>
        )}
        {activeTab === 'history' && <SmsHistory history={smsHistory} />}
      </div>
    </div>
  )
}
