import { apiClient } from './api'

// Types/Interfaces
export type ExpenseCategory =
  | 'Maintenance'
  | 'Utilities'
  | 'Insurance'
  | 'PropertyTax'
  | 'Management'
  | 'Marketing'
  | 'Legal'
  | 'Cleaning'
  | 'Landscaping'
  | 'Security'
  | 'Mortgage'
  | 'Supplies'
  | 'Other'

export type PaymentMethod =
  | 'Cash'
  | 'Check'
  | 'BankTransfer'
  | 'CreditCard'
  | 'DebitCard'
  | 'MPesa'
  | 'Other'

export interface Expense {
  id: number
  propertyId: number
  propertyName: string
  landlordId: number
  unitId?: number
  unitNumber?: string
  category: ExpenseCategory
  categoryDisplay: string
  amount: number
  expenseDate: string
  vendor: string
  description: string
  paymentMethod?: PaymentMethod
  paymentMethodDisplay?: string
  referenceNumber?: string
  isRecurring: boolean
  recurrenceMonths?: number
  nextRecurrenceDate?: string
  receiptUrl?: string
  notes?: string
  isTaxDeductible: boolean
  tags?: string
  createdAt: string
  updatedAt?: string
}

export interface CreateExpense {
  propertyId: number
  unitId?: number
  category: ExpenseCategory
  amount: number
  expenseDate: string
  vendor: string
  description: string
  paymentMethod?: PaymentMethod
  referenceNumber?: string
  isRecurring?: boolean
  recurrenceMonths?: number
  receiptUrl?: string
  notes?: string
  isTaxDeductible?: boolean
  tags?: string
}

export interface UpdateExpense {
  propertyId: number
  unitId?: number
  category: ExpenseCategory
  amount: number
  expenseDate: string
  vendor: string
  description: string
  paymentMethod?: PaymentMethod
  referenceNumber?: string
  isRecurring: boolean
  recurrenceMonths?: number
  nextRecurrenceDate?: string
  receiptUrl?: string
  notes?: string
  isTaxDeductible: boolean
  tags?: string
}

export interface ExpenseSummary {
  totalExpenses: number
  totalAmount: number
  averageExpense: number
  expensesByCategory: Record<string, number>
  expenseCountByCategory: Record<string, number>
  taxDeductibleAmount: number
  recurringExpensesTotal: number
}

export const expenseService = {
  // Get all expenses for current landlord
  getAllExpenses: async (startDate?: string, endDate?: string) => {
    const response = await apiClient.get<{ success: boolean; data: Expense[]; count: number }>(
      '/expenses',
      { params: { startDate, endDate } }
    )
    return response.data
  },

  // Get expense by ID
  getExpenseById: async (id: number) => {
    const response = await apiClient.get<{ success: boolean; data: Expense }>(
      `/expenses/${id}`
    )
    return response.data
  },

  // Get expenses for a property
  getExpensesByProperty: async (propertyId: number, startDate?: string, endDate?: string) => {
    const response = await apiClient.get<{ success: boolean; data: Expense[]; count: number }>(
      `/expenses/property/${propertyId}`,
      { params: { startDate, endDate } }
    )
    return response.data
  },

  // Get expenses by category
  getExpensesByCategory: async (category: ExpenseCategory, startDate?: string, endDate?: string) => {
    const response = await apiClient.get<{ success: boolean; data: Expense[]; count: number }>(
      `/expenses/category/${category}`,
      { params: { startDate, endDate } }
    )
    return response.data
  },

  // Get recurring expenses
  getRecurringExpenses: async () => {
    const response = await apiClient.get<{ success: boolean; data: Expense[]; count: number }>(
      '/expenses/recurring'
    )
    return response.data
  },

  // Get expense summary
  getSummary: async (startDate?: string, endDate?: string) => {
    const response = await apiClient.get<{ success: boolean; data: ExpenseSummary }>(
      '/expenses/summary',
      { params: { startDate, endDate } }
    )
    return response.data
  },

  // Get total expenses
  getTotalExpenses: async (startDate?: string, endDate?: string) => {
    const response = await apiClient.get<{ success: boolean; data: { total: number } }>(
      '/expenses/total',
      { params: { startDate, endDate } }
    )
    return response.data
  },

  // Get total expenses for a property
  getTotalExpensesByProperty: async (propertyId: number, startDate?: string, endDate?: string) => {
    const response = await apiClient.get<{ success: boolean; data: { total: number } }>(
      `/expenses/total/property/${propertyId}`,
      { params: { startDate, endDate } }
    )
    return response.data
  },

  // Create new expense
  createExpense: async (expense: CreateExpense) => {
    const response = await apiClient.post<{ success: boolean; data: Expense; message: string }>(
      '/expenses',
      expense
    )
    return response.data
  },

  // Update expense
  updateExpense: async (id: number, expense: UpdateExpense) => {
    const response = await apiClient.put<{ success: boolean; data: Expense; message: string }>(
      `/expenses/${id}`,
      expense
    )
    return response.data
  },

  // Delete expense
  deleteExpense: async (id: number) => {
    const response = await apiClient.delete<{ success: boolean; message: string }>(
      `/expenses/${id}`
    )
    return response.data
  },

  // Process recurring expenses (admin only)
  processRecurringExpenses: async () => {
    const response = await apiClient.post<{ success: boolean; message: string }>(
      '/expenses/process-recurring'
    )
    return response.data
  }
}
