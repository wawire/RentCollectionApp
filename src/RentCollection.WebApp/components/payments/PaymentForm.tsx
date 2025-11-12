'use client'

import { useState, useEffect } from 'react'
import { CreatePaymentDto, PaymentMethod } from '@/lib/types'
import { Button, Input, Select } from '@/components/common'
import { useGetActiveTenants } from '@/lib/hooks'

interface PaymentFormProps {
  initialData?: CreatePaymentDto & { id?: number }
  onSubmit: (data: CreatePaymentDto) => Promise<void>
  loading?: boolean
  preselectedTenantId?: number
}

export default function PaymentForm({ initialData, onSubmit, loading, preselectedTenantId }: PaymentFormProps) {
  const { data: tenants } = useGetActiveTenants()

  // Get today's date in YYYY-MM-DD format
  const today = new Date().toISOString().split('T')[0]

  // Calculate default period (current month)
  const currentDate = new Date()
  const periodStart = new Date(currentDate.getFullYear(), currentDate.getMonth(), 1)
  const periodEnd = new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, 0)

  const [formData, setFormData] = useState<CreatePaymentDto>({
    tenantId: initialData?.tenantId || preselectedTenantId || 0,
    amount: initialData?.amount || 0,
    paymentDate: initialData?.paymentDate || today,
    paymentMethod: initialData?.paymentMethod || PaymentMethod.MPesa,
    transactionReference: initialData?.transactionReference || '',
    notes: initialData?.notes || '',
    periodStart: initialData?.periodStart || periodStart.toISOString().split('T')[0],
    periodEnd: initialData?.periodEnd || periodEnd.toISOString().split('T')[0],
  })

  const [errors, setErrors] = useState<Record<string, string>>({})

  // Auto-fill amount when tenant is selected
  useEffect(() => {
    if (formData.tenantId && tenants.length > 0) {
      const selectedTenant = tenants.find(t => t.id === formData.tenantId)
      if (selectedTenant && formData.amount === 0) {
        setFormData(prev => ({ ...prev, amount: selectedTenant.monthlyRent }))
      }
    }
  }, [formData.tenantId, tenants, formData.amount])

  const validate = () => {
    const newErrors: Record<string, string> = {}

    if (!formData.tenantId || formData.tenantId === 0) newErrors.tenantId = 'Tenant is required'
    if (formData.amount <= 0) newErrors.amount = 'Amount must be greater than 0'
    if (!formData.paymentDate) newErrors.paymentDate = 'Payment date is required'
    if (!formData.periodStart) newErrors.periodStart = 'Period start date is required'
    if (!formData.periodEnd) newErrors.periodEnd = 'Period end date is required'

    // Validate period dates
    if (formData.periodStart && formData.periodEnd) {
      const start = new Date(formData.periodStart)
      const end = new Date(formData.periodEnd)
      if (start > end) {
        newErrors.periodEnd = 'Period end date must be after start date'
      }
    }

    setErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (validate()) {
      await onSubmit(formData)
    }
  }

  const paymentMethodOptions = [
    { value: PaymentMethod.MPesa, label: 'M-Pesa' },
    { value: PaymentMethod.Cash, label: 'Cash' },
    { value: PaymentMethod.BankTransfer, label: 'Bank Transfer' },
    { value: PaymentMethod.Cheque, label: 'Cheque' },
    { value: PaymentMethod.Other, label: 'Other' },
  ]

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <Select
        label="Tenant"
        value={formData.tenantId}
        onChange={(e) => setFormData({ ...formData, tenantId: Number(e.target.value) })}
        options={tenants.map(t => ({
          value: t.id,
          label: `${t.fullName} - ${t.unitNumber} (KSh ${t.monthlyRent.toLocaleString()}/month)`
        }))}
        placeholder="Select a tenant"
        error={errors.tenantId}
        required
        fullWidth
      />

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <Input
          label="Amount (KSh)"
          type="number"
          value={formData.amount}
          onChange={(e) => setFormData({ ...formData, amount: Number(e.target.value) })}
          error={errors.amount}
          placeholder="e.g., 15000"
          required
          fullWidth
        />

        <Input
          label="Payment Date"
          type="date"
          value={formData.paymentDate}
          onChange={(e) => setFormData({ ...formData, paymentDate: e.target.value })}
          error={errors.paymentDate}
          required
          fullWidth
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <Select
          label="Payment Method"
          value={formData.paymentMethod}
          onChange={(e) => setFormData({ ...formData, paymentMethod: Number(e.target.value) as PaymentMethod })}
          options={paymentMethodOptions}
          required
          fullWidth
        />

        <Input
          label="Transaction Reference (Optional)"
          value={formData.transactionReference}
          onChange={(e) => setFormData({ ...formData, transactionReference: e.target.value })}
          placeholder="e.g., RCE123456789"
          fullWidth
        />
      </div>

      <div className="border-t border-gray-200 pt-4">
        <h3 className="text-sm font-medium text-gray-700 mb-3">Payment Period</h3>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Input
            label="Period Start Date"
            type="date"
            value={formData.periodStart}
            onChange={(e) => setFormData({ ...formData, periodStart: e.target.value })}
            error={errors.periodStart}
            required
            fullWidth
          />

          <Input
            label="Period End Date"
            type="date"
            value={formData.periodEnd}
            onChange={(e) => setFormData({ ...formData, periodEnd: e.target.value })}
            error={errors.periodEnd}
            required
            fullWidth
          />
        </div>
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Notes
        </label>
        <textarea
          value={formData.notes}
          onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
          rows={3}
          className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
          placeholder="Additional notes about the payment..."
        />
      </div>

      <Button type="submit" variant="primary" loading={loading} fullWidth>
        {initialData ? 'Update Payment' : 'Record Payment'}
      </Button>
    </form>
  )
}
