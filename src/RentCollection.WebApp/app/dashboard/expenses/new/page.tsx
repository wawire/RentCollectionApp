'use client'

import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import Link from 'next/link'
import { FaArrowLeft, FaSave, FaSpinner } from 'react-icons/fa'
import { expenseService, CreateExpense, ExpenseCategory, PaymentMethod } from '@/lib/services/expenseService'
import { propertyService } from '@/lib/services/propertyService'
import { Property } from '@/lib/types/property.types'

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

    // Validation
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
    <div className="p-6 max-w-4xl mx-auto">
      {/* Header */}
      <div className="mb-6">
        <Link href="/dashboard/expenses" className="text-blue-600 hover:text-blue-800 flex items-center gap-2 mb-4">
          <FaArrowLeft /> Back to Expenses
        </Link>
        <h1 className="text-3xl font-bold text-gray-800">Add New Expense</h1>
        <p className="text-gray-600 mt-1">Record a new property expense</p>
      </div>

      {/* Error Message */}
      {error && (
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded mb-6">
          {error}
        </div>
      )}

      {/* Form */}
      <form onSubmit={handleSubmit} className="bg-white rounded-lg shadow p-6">
        <div className="space-y-6">
          {/* Property Selection */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Property <span className="text-red-500">*</span>
            </label>
            <select
              value={formData.propertyId}
              onChange={(e) => handleInputChange('propertyId', parseInt(e.target.value))}
              className="w-full border border-gray-300 rounded-lg px-4 py-2 focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              required
            >
              <option value={0}>Select a property</option>
              {properties.map(property => (
                <option key={property.id} value={property.id}>
                  {property.name}
                </option>
              ))}
            </select>
          </div>

          {/* Category */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Category <span className="text-red-500">*</span>
            </label>
            <select
              value={formData.category}
              onChange={(e) => handleInputChange('category', e.target.value as ExpenseCategory)}
              className="w-full border border-gray-300 rounded-lg px-4 py-2 focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              required
            >
              <option value="Maintenance">Maintenance</option>
              <option value="Utilities">Utilities</option>
              <option value="Insurance">Insurance</option>
              <option value="PropertyTax">Property Tax</option>
              <option value="Management">Management</option>
              <option value="Marketing">Marketing</option>
              <option value="Legal">Legal</option>
              <option value="Cleaning">Cleaning</option>
              <option value="Landscaping">Landscaping</option>
              <option value="Security">Security</option>
              <option value="Mortgage">Mortgage</option>
              <option value="Supplies">Supplies</option>
              <option value="Other">Other</option>
            </select>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {/* Vendor */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Vendor <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                value={formData.vendor}
                onChange={(e) => handleInputChange('vendor', e.target.value)}
                className="w-full border border-gray-300 rounded-lg px-4 py-2 focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="e.g., ABC Plumbing"
                required
              />
            </div>

            {/* Amount */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Amount (KES) <span className="text-red-500">*</span>
              </label>
              <input
                type="number"
                step="0.01"
                min="0"
                value={formData.amount}
                onChange={(e) => handleInputChange('amount', parseFloat(e.target.value))}
                className="w-full border border-gray-300 rounded-lg px-4 py-2 focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="0.00"
                required
              />
            </div>
          </div>

          {/* Description */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Description <span className="text-red-500">*</span>
            </label>
            <textarea
              value={formData.description}
              onChange={(e) => handleInputChange('description', e.target.value)}
              rows={3}
              className="w-full border border-gray-300 rounded-lg px-4 py-2 focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="Describe the expense..."
              required
            />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {/* Expense Date */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Expense Date <span className="text-red-500">*</span>
              </label>
              <input
                type="date"
                value={formData.expenseDate}
                onChange={(e) => handleInputChange('expenseDate', e.target.value)}
                className="w-full border border-gray-300 rounded-lg px-4 py-2 focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                required
              />
            </div>

            {/* Payment Method */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Payment Method
              </label>
              <select
                value={formData.paymentMethod || ''}
                onChange={(e) => handleInputChange('paymentMethod', e.target.value ? e.target.value as PaymentMethod : undefined)}
                className="w-full border border-gray-300 rounded-lg px-4 py-2 focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              >
                <option value="">Select payment method</option>
                <option value="Cash">Cash</option>
                <option value="Check">Check</option>
                <option value="BankTransfer">Bank Transfer</option>
                <option value="CreditCard">Credit Card</option>
                <option value="DebitCard">Debit Card</option>
                <option value="MPesa">M-Pesa</option>
                <option value="Other">Other</option>
              </select>
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {/* Reference Number */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Reference Number
              </label>
              <input
                type="text"
                value={formData.referenceNumber || ''}
                onChange={(e) => handleInputChange('referenceNumber', e.target.value)}
                className="w-full border border-gray-300 rounded-lg px-4 py-2 focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="Invoice/Receipt number"
              />
            </div>

            {/* Receipt URL */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Receipt URL
              </label>
              <input
                type="url"
                value={formData.receiptUrl || ''}
                onChange={(e) => handleInputChange('receiptUrl', e.target.value)}
                className="w-full border border-gray-300 rounded-lg px-4 py-2 focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="https://..."
              />
            </div>
          </div>

          {/* Notes */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Notes
            </label>
            <textarea
              value={formData.notes || ''}
              onChange={(e) => handleInputChange('notes', e.target.value)}
              rows={2}
              className="w-full border border-gray-300 rounded-lg px-4 py-2 focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="Additional notes..."
            />
          </div>

          {/* Tags */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Tags
            </label>
            <input
              type="text"
              value={formData.tags || ''}
              onChange={(e) => handleInputChange('tags', e.target.value)}
              className="w-full border border-gray-300 rounded-lg px-4 py-2 focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="Comma-separated tags (e.g., urgent, emergency)"
            />
          </div>

          {/* Checkboxes */}
          <div className="space-y-4">
            {/* Tax Deductible */}
            <div className="flex items-center">
              <input
                type="checkbox"
                id="taxDeductible"
                checked={formData.isTaxDeductible}
                onChange={(e) => handleInputChange('isTaxDeductible', e.target.checked)}
                className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
              />
              <label htmlFor="taxDeductible" className="ml-2 block text-sm text-gray-700">
                Tax Deductible
              </label>
            </div>

            {/* Recurring */}
            <div className="flex items-center">
              <input
                type="checkbox"
                id="recurring"
                checked={formData.isRecurring}
                onChange={(e) => handleInputChange('isRecurring', e.target.checked)}
                className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
              />
              <label htmlFor="recurring" className="ml-2 block text-sm text-gray-700">
                Recurring Expense
              </label>
            </div>

            {/* Recurrence Interval */}
            {formData.isRecurring && (
              <div className="ml-6">
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Recurrence Interval (months) <span className="text-red-500">*</span>
                </label>
                <select
                  value={formData.recurrenceMonths || ''}
                  onChange={(e) => handleInputChange('recurrenceMonths', parseInt(e.target.value))}
                  className="w-full md:w-64 border border-gray-300 rounded-lg px-4 py-2 focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  required={formData.isRecurring}
                >
                  <option value="">Select interval</option>
                  <option value="1">Monthly (1 month)</option>
                  <option value="2">Bi-monthly (2 months)</option>
                  <option value="3">Quarterly (3 months)</option>
                  <option value="6">Semi-annually (6 months)</option>
                  <option value="12">Annually (12 months)</option>
                </select>
              </div>
            )}
          </div>

          {/* Action Buttons */}
          <div className="flex gap-4 pt-4 border-t border-gray-200">
            <button
              type="submit"
              disabled={loading}
              className="flex-1 bg-blue-600 text-white px-6 py-3 rounded-lg hover:bg-blue-700 transition disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
            >
              {loading ? (
                <>
                  <FaSpinner className="animate-spin" /> Saving...
                </>
              ) : (
                <>
                  <FaSave /> Save Expense
                </>
              )}
            </button>
            <Link
              href="/dashboard/expenses"
              className="flex-1 bg-gray-200 text-gray-700 px-6 py-3 rounded-lg hover:bg-gray-300 transition text-center"
            >
              Cancel
            </Link>
          </div>
        </div>
      </form>
    </div>
  )
}
