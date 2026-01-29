'use client'

import { useState } from 'react'
import ProtectedRoute from '@/components/auth/ProtectedRoute'
import { Card, Button, Alert, Select } from '@/components/common'
import { exportService } from '@/lib/services/exportService'
import { useGetProperties } from '@/lib/hooks/useProperties'

export default function ExportsPage() {
  const { data: properties } = useGetProperties()
  const [propertyId, setPropertyId] = useState<number | 'all'>('all')
  const [startDate, setStartDate] = useState('')
  const [endDate, setEndDate] = useState('')
  const [alert, setAlert] = useState<{ type: 'success' | 'error'; message: string } | null>(null)

  const propertyOptions = [
    { value: 'all', label: 'All Properties' },
    ...properties.map((p) => ({ value: p.id, label: p.name })),
  ]

  const downloadFile = (blob: Blob, name: string) => {
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = name
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)
  }

  const handleExport = async (type: 'payments' | 'invoices' | 'expenses' | 'arrears') => {
    try {
      const params = {
        propertyId: propertyId === 'all' ? undefined : propertyId,
        startDate: startDate || undefined,
        endDate: endDate || undefined,
      }

      let blob: Blob
      if (type === 'payments') {
        blob = await exportService.exportPayments(params)
      } else if (type === 'invoices') {
        blob = await exportService.exportInvoices(params)
      } else if (type === 'expenses') {
        blob = await exportService.exportExpenses(params)
      } else {
        blob = await exportService.exportArrears({ propertyId: params.propertyId })
      }

      downloadFile(blob, `${type}-${new Date().toISOString().slice(0, 10)}.csv`)
      setAlert({ type: 'success', message: `${type} export downloaded` })
    } catch (err: any) {
      setAlert({ type: 'error', message: err.message || 'Export failed' })
    }
  }

  return (
    <ProtectedRoute allowedRoles={['PlatformAdmin', 'Landlord', 'Manager', 'Accountant']}>
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-serif font-normal text-primary tracking-wide">CSV Exports</h1>
          <p className="text-primary/60 mt-2 tracking-wide">Download payments, invoices, expenses, and arrears</p>
        </div>

        {alert && <Alert type={alert.type} message={alert.message} onClose={() => setAlert(null)} />}

        <Card padding="md">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <Select
              label="Property"
              value={propertyId}
              onChange={(event) => {
                const value = event.target.value
                setPropertyId(value === 'all' ? 'all' : Number(value))
              }}
              options={propertyOptions}
            />
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Start Date</label>
              <input
                type="date"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
                className="w-full rounded-lg border border-gray-300 px-3 py-2"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">End Date</label>
              <input
                type="date"
                value={endDate}
                onChange={(e) => setEndDate(e.target.value)}
                className="w-full rounded-lg border border-gray-300 px-3 py-2"
              />
            </div>
          </div>
        </Card>

        <Card padding="md">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <Button variant="primary" onClick={() => handleExport('payments')}>
              Export Payments
            </Button>
            <Button variant="primary" onClick={() => handleExport('invoices')}>
              Export Invoices
            </Button>
            <Button variant="primary" onClick={() => handleExport('expenses')}>
              Export Expenses
            </Button>
            <Button variant="primary" onClick={() => handleExport('arrears')}>
              Export Arrears
            </Button>
          </div>
        </Card>
      </div>
    </ProtectedRoute>
  )
}

