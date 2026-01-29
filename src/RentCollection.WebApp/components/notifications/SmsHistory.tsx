'use client'

import { useState, useMemo } from 'react'
import { Card, Badge, SearchBar, Select } from '@/components/common'
import { SmsHistoryItem } from '@/lib/types'
import { CheckCircle2, Clock, History, XCircle } from 'lucide-react'

interface SmsHistoryProps {
  history: SmsHistoryItem[]
}

export default function SmsHistory({ history }: SmsHistoryProps) {
  const [searchTerm, setSearchTerm] = useState('')
  const [statusFilter, setStatusFilter] = useState<'all' | 'sent' | 'failed' | 'pending'>('all')

  // Filter history
  const filteredHistory = useMemo(() => {
    return history.filter(item => {
      // Search filter
      const searchLower = searchTerm.toLowerCase()
      const matchesSearch =
        item.recipientName.toLowerCase().includes(searchLower) ||
        item.phoneNumber.includes(searchLower) ||
        item.message.toLowerCase().includes(searchLower)

      // Status filter
      const matchesStatus = statusFilter === 'all' || item.status === statusFilter

      return matchesSearch && matchesStatus
    })
  }, [history, searchTerm, statusFilter])

  const getStatusIcon = (status: 'sent' | 'failed' | 'pending') => {
    switch (status) {
      case 'sent':
        return <CheckCircle2 className="text-green-600 w-4 h-4" />
      case 'failed':
        return <XCircle className="text-red-600 w-4 h-4" />
      case 'pending':
        return <Clock className="text-yellow-600 w-4 h-4" />
    }
  }

  const getStatusBadge = (status: 'sent' | 'failed' | 'pending') => {
    switch (status) {
      case 'sent':
        return <Badge variant="success">Sent</Badge>
      case 'failed':
        return <Badge variant="danger">Failed</Badge>
      case 'pending':
        return <Badge variant="warning">Pending</Badge>
    }
  }

  const statusOptions = [
    { value: 'all', label: 'All Status' },
    { value: 'sent', label: 'Sent' },
    { value: 'pending', label: 'Pending' },
    { value: 'failed', label: 'Failed' },
  ]

  // Calculate stats
  const stats = {
    total: history.length,
    sent: history.filter(h => h.status === 'sent').length,
    failed: history.filter(h => h.status === 'failed').length,
    pending: history.filter(h => h.status === 'pending').length,
  }

  return (
    <div className="space-y-4">
      <Card padding="md">
        <div className="flex items-center gap-2 mb-4">
          <History className="text-primary-600 w-5 h-5" />
          <h2 className="text-lg font-semibold text-gray-900">SMS History</h2>
        </div>

        {/* Stats Summary */}
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3 mb-4">
          <div className="p-3 bg-gray-50 rounded-lg">
            <p className="text-xs text-gray-600 mb-1">Total</p>
            <p className="text-xl font-bold text-gray-900">{stats.total}</p>
          </div>
          <div className="p-3 bg-green-50 rounded-lg">
            <p className="text-xs text-green-600 mb-1">Sent</p>
            <p className="text-xl font-bold text-green-900">{stats.sent}</p>
          </div>
          <div className="p-3 bg-yellow-50 rounded-lg">
            <p className="text-xs text-yellow-600 mb-1">Pending</p>
            <p className="text-xl font-bold text-yellow-900">{stats.pending}</p>
          </div>
          <div className="p-3 bg-red-50 rounded-lg">
            <p className="text-xs text-red-600 mb-1">Failed</p>
            <p className="text-xl font-bold text-red-900">{stats.failed}</p>
          </div>
        </div>

        {/* Filters */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
          <SearchBar
            value={searchTerm}
            onChange={setSearchTerm}
            placeholder="Search by name, phone, or message..."
            className="w-full"
          />
          <Select
            label=""
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value as 'all' | 'sent' | 'failed' | 'pending')}
            options={statusOptions}
            fullWidth
          />
        </div>

        {(searchTerm || statusFilter !== 'all') && (
          <div className="flex justify-end mb-3">
            <button
              onClick={() => {
                setSearchTerm('')
                setStatusFilter('all')
              }}
              className="text-sm text-primary-600 hover:text-primary-700 font-medium"
            >
              Clear Filters
            </button>
          </div>
        )}
      </Card>

      {/* History Table */}
      <Card padding="none">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Recipient</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Phone</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Message</th>
                <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase">Status</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Sent At</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {filteredHistory.length === 0 ? (
                <tr>
                  <td colSpan={5} className="px-6 py-12 text-center text-gray-500">
                    {history.length === 0
                      ? 'No messages sent yet. Use the form above to send your first SMS.'
                      : 'No messages match your filters.'
                    }
                  </td>
                </tr>
              ) : (
                filteredHistory.map((item) => (
                  <tr key={item.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <p className="text-sm font-medium text-gray-900">{item.recipientName}</p>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <p className="text-sm text-gray-600">{item.phoneNumber}</p>
                    </td>
                    <td className="px-6 py-4">
                      <p className="text-sm text-gray-700 line-clamp-2" title={item.message}>
                        {item.message}
                      </p>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-center">
                      <div className="flex items-center justify-center gap-2">
                        {getStatusIcon(item.status)}
                        {getStatusBadge(item.status)}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <p className="text-sm text-gray-600">
                        {new Date(item.sentAt).toLocaleString()}
                      </p>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </Card>
    </div>
  )
}
