'use client'

import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import Link from 'next/link'
import { ArrowLeft, Loader2, Save } from 'lucide-react'
import { expenseService, CreateExpense, ExpenseCategory, PaymentMethod } from '@/lib/services/expenseService'
import { propertyService } from '@/lib/services/propertyService'
import { Property } from '@/lib/types/property.types'
import ProtectedRoute from '@/components/auth/ProtectedRoute'
import { Alert, Button, Card, Input, PageHeader, Select, TextArea } from '@/components/common'

export default function NewExpensePage() {
  const router = useRouter()
  const [loading, setLoading] = useState(false)
  const [properties, setProperties] = useState<Property[]>([])
  const [error, setError] = useState<string | null>(null)
  const [formData, setFormData] = useState<CreateExpense>({
    propertyId: 0,
    category: 'Maintenance' as ExpenseCategory,
    amount: 0,
    expenseDate: new Date().toISOString().split('T')[0],
    vendor: '',
    description: '',
    isTaxDeductible: true,
    isRecurring: false
  })

  useEffect(() => {
    loadProperties()
  }, [])

  const loadProperties = async () => {
    try {
      const properties = await propertyService.getAll()
      setProperties(properties)
    } catch (err: any) {
      console.error('Failed to load properties:', err)
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    if (!formData.propertyId) {
      setError('Please select a property')
      return
    }
    if (!formData.vendor.trim()) {
      setError('Please enter vendor name')
      return
    }
    if (!formData.description.trim()) {
      setError('Please enter description')
      return
    }
    if (formData.amount <= 0) {
      setError('Please enter a valid amount')
      return
    }
    if (formData.isRecurring && (!formData.recurrenceMonths || formData.recurrenceMonths <= 0)) {
      setError('Please enter recurrence interval for recurring expenses')
      return
    }

    try {
      setLoading(true)
      setError(null)
      await expenseService.createExpense(formData)
      router.push('/dashboard/expenses')
    } catch (err: any) {
      setError(err.message || 'Failed to create expense')
      setLoading(false)
    }
  }

  const handleInputChange = (field: keyof CreateExpense, value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }))
  }

  return (
    <ProtectedRoute allowedRoles={['Landlord', 'Manager']}>
      <div className="max-w-4xl mx-auto space-y-6">
        <PageHeader
          title="Add New Expense"
          subtitle="Record a new property expense."
          breadcrumbs={[
            { label: 'Dashboard', href: '/dashboard' },
            { label: 'Expenses', href: '/dashboard/expenses' },
            { label: 'New Expense' },
          ]}
          actions={
            <Link href="/dashboard/expenses">
              <Button variant="ghost">
                <ArrowLeft size={16} className="mr-2" />
                Back to Expenses
              </Button>
            </Link>
          }
        />

        {error && (
          <Alert type="error" message={error} />
        )}

        <Card padding="md">
          <form onSubmit={handleSubmit} className="space-y-6">
            <Select
              label="Property"
              value={formData.propertyId}
              onChange={(e) => handleInputChange('propertyId', parseInt(e.target.value))}
              options={[
                { value: 0, label: 'Select a property' },
                ...properties.map(property => ({ value: property.id, label: property.name })),
              ]}
              required
            />

            <Select
              label="Category"
              value={formData.category}
              onChange={(e) => handleInputChange('category', e.target.value as ExpenseCategory)}
              options={[
                { value: 'Maintenance', label: 'Maintenance' },
                { value: 'Utilities', label: 'Utilities' },
                { value: 'Insurance', label: 'Insurance' },
                { value: 'PropertyTax', label: 'Property Tax' },
                { value: 'Management', label: 'Management' },
                { value: 'Marketing', label: 'Marketing' },
                { value: 'Legal', label: 'Legal' },
                { value: 'Cleaning', label: 'Cleaning' },
                { value: 'Landscaping', label: 'Landscaping' },
                { value: 'Security', label: 'Security' },
                { value: 'Mortgage', label: 'Mortgage' },
                { value: 'Supplies', label: 'Supplies' },
                { value: 'Other', label: 'Other' },
              ]}
              required
            />

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <Input
                label="Vendor"
                value={formData.vendor}
                onChange={(e) => handleInputChange('vendor', e.target.value)}
                placeholder="e.g., ABC Plumbing"
                required
              />

              <Input
                label="Amount (KES)"
                type="number"
                step="0.01"
                min="0"
                value={formData.amount}
                onChange={(e) => handleInputChange('amount', parseFloat(e.target.value))}
                placeholder="0.00"
                required
              />
            </div>

            <TextArea
              label="Description"
              value={formData.description}
              onChange={(e) => handleInputChange('description', e.target.value)}
              rows={3}
              placeholder="Describe the expense..."
              required
            />

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <Input
                label="Expense Date"
                type="date"
                value={formData.expenseDate}
                onChange={(e) => handleInputChange('expenseDate', e.target.value)}
                required
              />

              <Select
                label="Payment Method"
                value={formData.paymentMethod || ''}
                onChange={(e) => handleInputChange('paymentMethod', e.target.value ? e.target.value as PaymentMethod : undefined)}
                options={[
                  { value: '', label: 'Select payment method' },
                  { value: 'Cash', label: 'Cash' },
                  { value: 'Check', label: 'Check' },
                  { value: 'BankTransfer', label: 'Bank Transfer' },
                  { value: 'CreditCard', label: 'Credit Card' },
                  { value: 'DebitCard', label: 'Debit Card' },
                  { value: 'MPesa', label: 'M-Pesa' },
                  { value: 'Other', label: 'Other' },
                ]}
              />
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <Input
                label="Reference Number"
                value={formData.referenceNumber || ''}
                onChange={(e) => handleInputChange('referenceNumber', e.target.value)}
                placeholder="Invoice/Receipt number"
              />

              <Input
                label="Receipt URL"
                type="url"
                value={formData.receiptUrl || ''}
                onChange={(e) => handleInputChange('receiptUrl', e.target.value)}
                placeholder="https://..."
              />
            </div>

            <TextArea
              label="Notes"
              value={formData.notes || ''}
              onChange={(e) => handleInputChange('notes', e.target.value)}
              rows={2}
              placeholder="Additional notes..."
            />

            <Input
              label="Tags"
              value={formData.tags || ''}
              onChange={(e) => handleInputChange('tags', e.target.value)}
              placeholder="Comma-separated tags (e.g., urgent, emergency)"
            />

            <div className="space-y-4">
              <div className="flex items-center">
                <input
                  type="checkbox"
                  id="taxDeductible"
                  checked={formData.isTaxDeductible}
                  onChange={(e) => handleInputChange('isTaxDeductible', e.target.checked)}
                  className="h-4 w-4 text-brand-secondary focus-visible:ring-brand-secondary border-border-muted rounded"
                />
                <label htmlFor="taxDeductible" className="ml-2 block text-sm text-text-secondary">
                  Tax Deductible
                </label>
              </div>

              <div className="flex items-center">
                <input
                  type="checkbox"
                  id="recurring"
                  checked={formData.isRecurring}
                  onChange={(e) => handleInputChange('isRecurring', e.target.checked)}
                  className="h-4 w-4 text-brand-secondary focus-visible:ring-brand-secondary border-border-muted rounded"
                />
                <label htmlFor="recurring" className="ml-2 block text-sm text-text-secondary">
                  Recurring Expense
                </label>
              </div>

              {formData.isRecurring && (
                <div className="ml-6">
                  <Select
                    label="Recurrence Interval (months)"
                    value={formData.recurrenceMonths || ''}
                    onChange={(e) => handleInputChange('recurrenceMonths', parseInt(e.target.value))}
                    options={[
                      { value: '', label: 'Select interval' },
                      { value: 1, label: 'Monthly (1 month)' },
                      { value: 2, label: 'Bi-monthly (2 months)' },
                      { value: 3, label: 'Quarterly (3 months)' },
                      { value: 6, label: 'Semi-annually (6 months)' },
                      { value: 12, label: 'Annually (12 months)' },
                    ]}
                    required
                  />
                </div>
              )}
            </div>

            <div className="flex gap-4 pt-4 border-t border-border-muted">
              <Button type="submit" disabled={loading}>
                {loading ? (
                  <>
                    <Loader2 size={16} className="mr-2 animate-spin" />
                    Saving...
                  </>
                ) : (
                  <>
                    <Save size={16} className="mr-2" />
                    Save Expense
                  </>
                )}
              </Button>
              <Link href="/dashboard/expenses" className="flex-1">
                <Button variant="secondary" fullWidth>
                  Cancel
                </Button>
              </Link>
            </div>
          </form>
        </Card>
      </div>
    </ProtectedRoute>
  )
}
