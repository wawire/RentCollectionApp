'use client'

import { useState, useEffect } from 'react'
import Link from 'next/link'
import {
  FaPlus,
  FaSpinner,
  FaChartBar,
  FaDollarSign,
  FaReceipt,
  FaEdit,
  FaTrash,
  FaFilter,
  FaCalendar,
  FaBuilding,
  FaTags
} from 'react-icons/fa'
import { expenseService, Expense, ExpenseSummary } from '@/lib/services/expenseService'

export default function ExpensesPage() {
  const [loading, setLoading] = useState(true)
  const [expenses, setExpenses] = useState<Expense[]>([])
  const [summary, setSummary] = useState<ExpenseSummary | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [filter, setFilter] = useState<'all' | 'recent' | 'recurring' | 'tax-deductible'>('all')
  const [categoryFilter, setCategoryFilter] = useState<string>('all')
  const [deleteId, setDeleteId] = useState<number | null>(null)

  useEffect(() => {
    loadData()
  }, [])

  const loadData = async () => {
    try {
      setLoading(true)
      setError(null)

      // Get current month start and end for filtering
      const now = new Date()
      const startOfMonth = new Date(now.getFullYear(), now.getMonth(), 1).toISOString()
      const endOfMonth = new Date(now.getFullYear(), now.getMonth() + 1, 0).toISOString()

      // Load expenses and summary
      const [expensesResponse, summaryResponse] = await Promise.all([
        expenseService.getAllExpenses(),
        expenseService.getSummary()
      ])

      if (expensesResponse.success) {
        setExpenses(expensesResponse.data)
      }
      if (summaryResponse.success) {
        setSummary(summaryResponse.data)
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load expenses')
    } finally {
      setLoading(false)
    }
  }

  const handleDelete = async (id: number) => {
    if (!confirm('Are you sure you want to delete this expense?')) {
      return
    }

    try {
      setDeleteId(id)
      await expenseService.deleteExpense(id)
      await loadData()
    } catch (err: any) {
      setError(err.message || 'Failed to delete expense')
    } finally {
      setDeleteId(null)
    }
  }

  const getCategoryColor = (category: string): string => {
    const colors: Record<string, string> = {
      Maintenance: 'bg-red-100 text-red-800',
      Utilities: 'bg-blue-100 text-blue-800',
      Insurance: 'bg-purple-100 text-purple-800',
      PropertyTax: 'bg-yellow-100 text-yellow-800',
      Management: 'bg-green-100 text-green-800',
      Marketing: 'bg-pink-100 text-pink-800',
      Legal: 'bg-indigo-100 text-indigo-800',
      Cleaning: 'bg-teal-100 text-teal-800',
      Landscaping: 'bg-lime-100 text-lime-800',
      Security: 'bg-orange-100 text-orange-800',
      Mortgage: 'bg-red-100 text-red-800',
      Supplies: 'bg-cyan-100 text-cyan-800',
      Other: 'bg-gray-100 text-gray-800'
    }
    return colors[category] || 'bg-gray-100 text-gray-800'
  }

  const filteredExpenses = expenses.filter(expense => {
    // Apply status filter
    if (filter === 'recent') {
      const thirtyDaysAgo = new Date()
      thirtyDaysAgo.setDate(thirtyDaysAgo.getDate() - 30)
      if (new Date(expense.expenseDate) < thirtyDaysAgo) return false
    } else if (filter === 'recurring') {
      if (!expense.isRecurring) return false
    } else if (filter === 'tax-deductible') {
      if (!expense.isTaxDeductible) return false
    }

    // Apply category filter
    if (categoryFilter !== 'all' && expense.category !== categoryFilter) {
      return false
    }

    return true
  })

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-KE', {
      style: 'currency',
      currency: 'KES'
    }).format(amount)
  }

  const formatDate = (date: string) => {
    return new Date(date).toLocaleDateString('en-KE', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    })
  }

  if (loading) {
    return (
      <div className="flex justify-center items-center min-h-screen">
        <FaSpinner className="animate-spin text-4xl text-blue-600" />
      </div>
    )
  }

  return (
    <div className="p-6 max-w-7xl mx-auto">
      {/* Header */}
      <div className="mb-6">
        <div className="flex justify-between items-center">
          <div>
            <h1 className="text-3xl font-bold text-gray-800">Expense Tracking</h1>
            <p className="text-gray-600 mt-1">Track and manage property expenses</p>
          </div>
          <Link
            href="/dashboard/expenses/new"
            className="bg-blue-600 text-white px-6 py-3 rounded-lg hover:bg-blue-700 transition flex items-center gap-2"
          >
            <FaPlus /> Add Expense
          </Link>
        </div>
      </div>

      {/* Error Message */}
      {error && (
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded mb-6">
          {error}
        </div>
      )}

      {/* Statistics Cards */}
      {summary && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-6">
          {/* Total Expenses */}
          <div className="bg-white rounded-lg shadow p-6 border-l-4 border-blue-600">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-600 text-sm font-medium">Total Expenses</p>
                <p className="text-2xl font-bold text-gray-800 mt-1">
                  {formatCurrency(summary.totalAmount)}
                </p>
                <p className="text-xs text-gray-500 mt-1">{summary.totalExpenses} transactions</p>
              </div>
              <FaDollarSign className="text-4xl text-blue-600 opacity-20" />
            </div>
          </div>

          {/* Average Expense */}
          <div className="bg-white rounded-lg shadow p-6 border-l-4 border-green-600">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-600 text-sm font-medium">Average Expense</p>
                <p className="text-2xl font-bold text-gray-800 mt-1">
                  {formatCurrency(summary.averageExpense)}
                </p>
                <p className="text-xs text-gray-500 mt-1">Per transaction</p>
              </div>
              <FaChartBar className="text-4xl text-green-600 opacity-20" />
            </div>
          </div>

          {/* Tax Deductible */}
          <div className="bg-white rounded-lg shadow p-6 border-l-4 border-purple-600">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-600 text-sm font-medium">Tax Deductible</p>
                <p className="text-2xl font-bold text-gray-800 mt-1">
                  {formatCurrency(summary.taxDeductibleAmount)}
                </p>
                <p className="text-xs text-gray-500 mt-1">Eligible for deduction</p>
              </div>
              <FaReceipt className="text-4xl text-purple-600 opacity-20" />
            </div>
          </div>

          {/* Recurring Expenses */}
          <div className="bg-white rounded-lg shadow p-6 border-l-4 border-orange-600">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-600 text-sm font-medium">Recurring Total</p>
                <p className="text-2xl font-bold text-gray-800 mt-1">
                  {formatCurrency(summary.recurringExpensesTotal)}
                </p>
                <p className="text-xs text-gray-500 mt-1">Recurring expenses</p>
              </div>
              <FaCalendar className="text-4xl text-orange-600 opacity-20" />
            </div>
          </div>
        </div>
      )}

      {/* Filters */}
      <div className="bg-white rounded-lg shadow mb-6 p-4">
        <div className="flex items-center gap-4 mb-4">
          <FaFilter className="text-gray-600" />
          <span className="font-medium text-gray-700">Filters</span>
        </div>

        {/* Status Filter */}
        <div className="flex gap-2 mb-4">
          <button
            onClick={() => setFilter('all')}
            className={`px-4 py-2 rounded-lg transition ${
              filter === 'all'
                ? 'bg-blue-600 text-white'
                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
            }`}
          >
            All
          </button>
          <button
            onClick={() => setFilter('recent')}
            className={`px-4 py-2 rounded-lg transition ${
              filter === 'recent'
                ? 'bg-blue-600 text-white'
                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
            }`}
          >
            Recent (30 days)
          </button>
          <button
            onClick={() => setFilter('recurring')}
            className={`px-4 py-2 rounded-lg transition ${
              filter === 'recurring'
                ? 'bg-blue-600 text-white'
                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
            }`}
          >
            Recurring
          </button>
          <button
            onClick={() => setFilter('tax-deductible')}
            className={`px-4 py-2 rounded-lg transition ${
              filter === 'tax-deductible'
                ? 'bg-blue-600 text-white'
                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
            }`}
          >
            Tax Deductible
          </button>
        </div>

        {/* Category Filter */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">Category</label>
          <select
            value={categoryFilter}
            onChange={(e) => setCategoryFilter(e.target.value)}
            className="border border-gray-300 rounded-lg px-4 py-2 w-full md:w-64"
          >
            <option value="all">All Categories</option>
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
      </div>

      {/* Expenses Table */}
      <div className="bg-white rounded-lg shadow overflow-hidden">
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Date
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Property/Unit
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Category
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Vendor/Description
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Amount
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Status
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {filteredExpenses.length === 0 ? (
                <tr>
                  <td colSpan={7} className="px-6 py-8 text-center text-gray-500">
                    No expenses found. <Link href="/dashboard/expenses/new" className="text-blue-600 hover:underline">Add your first expense</Link>
                  </td>
                </tr>
              ) : (
                filteredExpenses.map((expense) => (
                  <tr key={expense.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {formatDate(expense.expenseDate)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center">
                        <FaBuilding className="text-gray-400 mr-2" />
                        <div>
                          <div className="text-sm font-medium text-gray-900">{expense.propertyName}</div>
                          {expense.unitNumber && (
                            <div className="text-xs text-gray-500">Unit {expense.unitNumber}</div>
                          )}
                        </div>
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className={`px-2 py-1 rounded-full text-xs font-medium ${getCategoryColor(expense.category)}`}>
                        {expense.categoryDisplay}
                      </span>
                    </td>
                    <td className="px-6 py-4">
                      <div>
                        <div className="text-sm font-medium text-gray-900">{expense.vendor}</div>
                        <div className="text-xs text-gray-500 truncate max-w-xs">{expense.description}</div>
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm font-semibold text-gray-900">
                        {formatCurrency(expense.amount)}
                      </div>
                      {expense.paymentMethodDisplay && (
                        <div className="text-xs text-gray-500">{expense.paymentMethodDisplay}</div>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex gap-1">
                        {expense.isRecurring && (
                          <span className="px-2 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                            Recurring
                          </span>
                        )}
                        {expense.isTaxDeductible && (
                          <span className="px-2 py-1 rounded-full text-xs font-medium bg-green-100 text-green-800">
                            Tax
                          </span>
                        )}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                      <div className="flex gap-2">
                        <Link
                          href={`/dashboard/expenses/${expense.id}/edit`}
                          className="text-blue-600 hover:text-blue-900"
                        >
                          <FaEdit />
                        </Link>
                        <button
                          onClick={() => handleDelete(expense.id)}
                          disabled={deleteId === expense.id}
                          className="text-red-600 hover:text-red-900 disabled:opacity-50"
                        >
                          {deleteId === expense.id ? (
                            <FaSpinner className="animate-spin" />
                          ) : (
                            <FaTrash />
                          )}
                        </button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Summary by Category */}
      {summary && summary.expensesByCategory && Object.keys(summary.expensesByCategory).length > 0 && (
        <div className="bg-white rounded-lg shadow p-6 mt-6">
          <h2 className="text-xl font-bold text-gray-800 mb-4 flex items-center gap-2">
            <FaTags /> Expenses by Category
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {Object.entries(summary.expensesByCategory).map(([category, amount]) => (
              <div key={category} className="flex justify-between items-center p-3 bg-gray-50 rounded-lg">
                <span className={`px-2 py-1 rounded-full text-xs font-medium ${getCategoryColor(category)}`}>
                  {category}
                </span>
                <div className="text-right">
                  <div className="font-semibold text-gray-900">{formatCurrency(amount)}</div>
                  <div className="text-xs text-gray-500">
                    {summary.expenseCountByCategory[category]} transactions
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  )
}
