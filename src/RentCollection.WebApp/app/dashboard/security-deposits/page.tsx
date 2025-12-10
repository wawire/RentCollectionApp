'use client'

import { useState, useEffect } from 'react'
import { securityDepositService } from '@/lib/services/securityDepositService'
import type {
  SecurityDepositBalance,
  SecurityDepositTransaction,
  RecordSecurityDepositDto,
  DeductSecurityDepositDto,
  RefundSecurityDepositDto,
  SecurityDepositTransactionType
} from '@/lib/types/securityDeposit.types'
import { FaMoneyBillWave, FaArrowDown, FaArrowUp, FaSearch } from 'react-icons/fa'

export default function SecurityDepositsPage() {
  const [deposits, setDeposits] = useState<SecurityDepositBalance[]>([])
  const [loading, setLoading] = useState(true)
  const [selectedDeposit, setSelectedDeposit] = useState<SecurityDepositBalance | null>(null)
  const [searchQuery, setSearchQuery] = useState('')
  const [modalType, setModalType] = useState<'record' | 'deduct' | 'refund' | 'view' | null>(null)

  // Form states
  const [recordForm, setRecordForm] = useState<RecordSecurityDepositDto>({
    amount: 0,
    transactionDate: new Date().toISOString().split('T')[0],
    notes: ''
  })

  const [deductForm, setDeductForm] = useState<DeductSecurityDepositDto>({
    amount: 0,
    reason: '',
    notes: ''
  })

  const [refundForm, setRefundForm] = useState<RefundSecurityDepositDto>({
    amount: 0,
    refundDate: new Date().toISOString().split('T')[0],
    refundMethod: 'M-Pesa',
    transactionReference: '',
    notes: ''
  })

  const [transactionHistory, setTransactionHistory] = useState<SecurityDepositTransaction[]>([])

  useEffect(() => {
    loadDeposits()
  }, [])

  const loadDeposits = async () => {
    try {
      setLoading(true)
      const data = await securityDepositService.getAllDeposits()
      setDeposits(data)
    } catch (error) {
      console.error('Failed to load deposits:', error)
    } finally {
      setLoading(false)
    }
  }

  const loadTransactionHistory = async (tenantId: number) => {
    try {
      const history = await securityDepositService.getTransactionHistory(tenantId)
      setTransactionHistory(history)
    } catch (error) {
      console.error('Failed to load transaction history:', error)
    }
  }

  const handleRecordDeposit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!selectedDeposit) return

    try {
      await securityDepositService.recordDeposit(selectedDeposit.tenantId, recordForm)
      alert('Security deposit recorded successfully')
      setModalType(null)
      loadDeposits()
      setRecordForm({ amount: 0, transactionDate: new Date().toISOString().split('T')[0], notes: '' })
    } catch (error: any) {
      alert(error.response?.data?.error || 'Failed to record deposit')
    }
  }

  const handleDeduct = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!selectedDeposit) return

    try {
      await securityDepositService.deductFromDeposit(selectedDeposit.tenantId, deductForm)
      alert('Deduction recorded successfully')
      setModalType(null)
      loadDeposits()
      setDeductForm({ amount: 0, reason: '', notes: '' })
    } catch (error: any) {
      alert(error.response?.data?.error || 'Failed to record deduction')
    }
  }

  const handleRefund = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!selectedDeposit) return

    try {
      await securityDepositService.refundDeposit(selectedDeposit.tenantId, refundForm)
      alert('Refund processed successfully')
      setModalType(null)
      loadDeposits()
      setRefundForm({
        amount: 0,
        refundDate: new Date().toISOString().split('T')[0],
        refundMethod: 'M-Pesa',
        transactionReference: '',
        notes: ''
      })
    } catch (error: any) {
      alert(error.response?.data?.error || 'Failed to process refund')
    }
  }

  const openModal = (type: 'record' | 'deduct' | 'refund' | 'view', deposit: SecurityDepositBalance) => {
    setSelectedDeposit(deposit)
    setModalType(type)
    if (type === 'view') {
      loadTransactionHistory(deposit.tenantId)
    }
  }

  const filteredDeposits = deposits.filter(
    (d) =>
      d.tenantName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      d.unitNumber.toLowerCase().includes(searchQuery.toLowerCase())
  )

  const totalDepositsHeld = deposits.reduce((sum, d) => sum + d.currentBalance, 0)
  const totalInitialDeposits = deposits.reduce((sum, d) => sum + d.initialDeposit, 0)

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-gray-500">Loading security deposits...</div>
      </div>
    )
  }

  return (
    <div className="container mx-auto p-6">
      <h1 className="text-3xl font-bold mb-6">Security Deposits</h1>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-6">
        <div className="bg-white p-6 rounded-lg shadow">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-gray-600 text-sm">Total Deposits Held</p>
              <p className="text-2xl font-bold text-green-600">
                KES {totalDepositsHeld.toLocaleString()}
              </p>
            </div>
            <FaMoneyBillWave className="text-green-500 text-3xl" />
          </div>
        </div>

        <div className="bg-white p-6 rounded-lg shadow">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-gray-600 text-sm">Total Initial Deposits</p>
              <p className="text-2xl font-bold text-blue-600">
                KES {totalInitialDeposits.toLocaleString()}
              </p>
            </div>
            <FaMoneyBillWave className="text-blue-500 text-3xl" />
          </div>
        </div>

        <div className="bg-white p-6 rounded-lg shadow">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-gray-600 text-sm">Active Tenants</p>
              <p className="text-2xl font-bold text-purple-600">{deposits.length}</p>
            </div>
            <FaMoneyBillWave className="text-purple-500 text-3xl" />
          </div>
        </div>
      </div>

      {/* Search Bar */}
      <div className="bg-white p-4 rounded-lg shadow mb-6">
        <div className="relative">
          <FaSearch className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" />
          <input
            type="text"
            placeholder="Search by tenant name or unit number..."
            className="w-full pl-10 pr-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
        </div>
      </div>

      {/* Deposits Table */}
      <div className="bg-white rounded-lg shadow overflow-hidden">
        <table className="min-w-full">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                Tenant
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                Unit
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                Initial Deposit
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                Deductions
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                Refunds
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                Current Balance
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                Actions
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200">
            {filteredDeposits.map((deposit) => (
              <tr key={deposit.tenantId} className="hover:bg-gray-50">
                <td className="px-6 py-4 whitespace-nowrap">{deposit.tenantName}</td>
                <td className="px-6 py-4 whitespace-nowrap">{deposit.unitNumber}</td>
                <td className="px-6 py-4 whitespace-nowrap">
                  KES {deposit.initialDeposit.toLocaleString()}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-red-600">
                  KES {deposit.totalDeductions.toLocaleString()}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-orange-600">
                  KES {deposit.totalRefunds.toLocaleString()}
                </td>
                <td className="px-6 py-4 whitespace-nowrap font-semibold text-green-600">
                  KES {deposit.currentBalance.toLocaleString()}
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <div className="flex gap-2">
                    <button
                      onClick={() => openModal('record', deposit)}
                      className="text-sm text-blue-600 hover:text-blue-800"
                    >
                      Record
                    </button>
                    <button
                      onClick={() => openModal('deduct', deposit)}
                      className="text-sm text-red-600 hover:text-red-800"
                    >
                      Deduct
                    </button>
                    <button
                      onClick={() => openModal('refund', deposit)}
                      className="text-sm text-green-600 hover:text-green-800"
                    >
                      Refund
                    </button>
                    <button
                      onClick={() => openModal('view', deposit)}
                      className="text-sm text-purple-600 hover:text-purple-800"
                    >
                      View
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Modals */}
      {modalType === 'record' && selectedDeposit && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 w-full max-w-md">
            <h2 className="text-2xl font-bold mb-4">
              Record Security Deposit - {selectedDeposit.tenantName}
            </h2>
            <form onSubmit={handleRecordDeposit}>
              <div className="mb-4">
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Amount (KES)
                </label>
                <input
                  type="number"
                  required
                  min="0"
                  step="0.01"
                  className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  value={recordForm.amount}
                  onChange={(e) =>
                    setRecordForm({ ...recordForm, amount: parseFloat(e.target.value) })
                  }
                />
              </div>
              <div className="mb-4">
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Transaction Date
                </label>
                <input
                  type="date"
                  required
                  className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  value={recordForm.transactionDate}
                  onChange={(e) =>
                    setRecordForm({ ...recordForm, transactionDate: e.target.value })
                  }
                />
              </div>
              <div className="mb-4">
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Notes (Optional)
                </label>
                <textarea
                  className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  rows={3}
                  value={recordForm.notes}
                  onChange={(e) => setRecordForm({ ...recordForm, notes: e.target.value })}
                />
              </div>
              <div className="flex gap-3">
                <button
                  type="submit"
                  className="flex-1 bg-blue-500 text-white py-2 px-4 rounded-lg hover:bg-blue-600"
                >
                  Record Deposit
                </button>
                <button
                  type="button"
                  onClick={() => setModalType(null)}
                  className="flex-1 bg-gray-300 text-gray-700 py-2 px-4 rounded-lg hover:bg-gray-400"
                >
                  Cancel
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {modalType === 'deduct' && selectedDeposit && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 w-full max-w-md">
            <h2 className="text-2xl font-bold mb-4">
              Deduct from Deposit - {selectedDeposit.tenantName}
            </h2>
            <p className="text-sm text-gray-600 mb-4">
              Current Balance: <span className="font-semibold">KES {selectedDeposit.currentBalance.toLocaleString()}</span>
            </p>
            <form onSubmit={handleDeduct}>
              <div className="mb-4">
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Amount to Deduct (KES)
                </label>
                <input
                  type="number"
                  required
                  min="0"
                  max={selectedDeposit.currentBalance}
                  step="0.01"
                  className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  value={deductForm.amount}
                  onChange={(e) =>
                    setDeductForm({ ...deductForm, amount: parseFloat(e.target.value) })
                  }
                />
              </div>
              <div className="mb-4">
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Reason for Deduction
                </label>
                <textarea
                  required
                  className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  rows={3}
                  value={deductForm.reason}
                  onChange={(e) => setDeductForm({ ...deductForm, reason: e.target.value })}
                  placeholder="e.g., Damages to unit, unpaid rent, cleaning fees..."
                />
              </div>
              <div className="mb-4">
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Additional Notes (Optional)
                </label>
                <textarea
                  className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  rows={2}
                  value={deductForm.notes}
                  onChange={(e) => setDeductForm({ ...deductForm, notes: e.target.value })}
                />
              </div>
              <div className="flex gap-3">
                <button
                  type="submit"
                  className="flex-1 bg-red-500 text-white py-2 px-4 rounded-lg hover:bg-red-600"
                >
                  Record Deduction
                </button>
                <button
                  type="button"
                  onClick={() => setModalType(null)}
                  className="flex-1 bg-gray-300 text-gray-700 py-2 px-4 rounded-lg hover:bg-gray-400"
                >
                  Cancel
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {modalType === 'refund' && selectedDeposit && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 w-full max-w-md">
            <h2 className="text-2xl font-bold mb-4">
              Refund Deposit - {selectedDeposit.tenantName}
            </h2>
            <p className="text-sm text-gray-600 mb-4">
              Available Balance: <span className="font-semibold">KES {selectedDeposit.currentBalance.toLocaleString()}</span>
            </p>
            <form onSubmit={handleRefund}>
              <div className="mb-4">
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Refund Amount (KES)
                </label>
                <input
                  type="number"
                  required
                  min="0"
                  max={selectedDeposit.currentBalance}
                  step="0.01"
                  className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  value={refundForm.amount}
                  onChange={(e) =>
                    setRefundForm({ ...refundForm, amount: parseFloat(e.target.value) })
                  }
                />
              </div>
              <div className="mb-4">
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Refund Method
                </label>
                <select
                  required
                  className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  value={refundForm.refundMethod}
                  onChange={(e) =>
                    setRefundForm({ ...refundForm, refundMethod: e.target.value })
                  }
                >
                  <option value="M-Pesa">M-Pesa</option>
                  <option value="Bank Transfer">Bank Transfer</option>
                  <option value="Cash">Cash</option>
                  <option value="Cheque">Cheque</option>
                </select>
              </div>
              <div className="mb-4">
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Refund Date
                </label>
                <input
                  type="date"
                  required
                  className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  value={refundForm.refundDate}
                  onChange={(e) =>
                    setRefundForm({ ...refundForm, refundDate: e.target.value })
                  }
                />
              </div>
              <div className="mb-4">
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Transaction Reference (Optional)
                </label>
                <input
                  type="text"
                  className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  value={refundForm.transactionReference}
                  onChange={(e) =>
                    setRefundForm({ ...refundForm, transactionReference: e.target.value })
                  }
                  placeholder="M-Pesa code, check number, etc."
                />
              </div>
              <div className="mb-4">
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Notes (Optional)
                </label>
                <textarea
                  className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  rows={2}
                  value={refundForm.notes}
                  onChange={(e) => setRefundForm({ ...refundForm, notes: e.target.value })}
                />
              </div>
              <div className="flex gap-3">
                <button
                  type="submit"
                  className="flex-1 bg-green-500 text-white py-2 px-4 rounded-lg hover:bg-green-600"
                >
                  Process Refund
                </button>
                <button
                  type="button"
                  onClick={() => setModalType(null)}
                  className="flex-1 bg-gray-300 text-gray-700 py-2 px-4 rounded-lg hover:bg-gray-400"
                >
                  Cancel
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {modalType === 'view' && selectedDeposit && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 overflow-y-auto">
          <div className="bg-white rounded-lg p-6 w-full max-w-4xl m-4">
            <h2 className="text-2xl font-bold mb-4">
              Transaction History - {selectedDeposit.tenantName}
            </h2>
            <div className="mb-4 grid grid-cols-2 gap-4">
              <div className="bg-gray-50 p-4 rounded">
                <p className="text-sm text-gray-600">Initial Deposit</p>
                <p className="text-xl font-bold">KES {selectedDeposit.initialDeposit.toLocaleString()}</p>
              </div>
              <div className="bg-gray-50 p-4 rounded">
                <p className="text-sm text-gray-600">Current Balance</p>
                <p className="text-xl font-bold text-green-600">
                  KES {selectedDeposit.currentBalance.toLocaleString()}
                </p>
              </div>
            </div>
            <div className="overflow-x-auto max-h-96">
              <table className="min-w-full">
                <thead className="bg-gray-50 sticky top-0">
                  <tr>
                    <th className="px-4 py-2 text-left text-xs font-medium text-gray-500">Date</th>
                    <th className="px-4 py-2 text-left text-xs font-medium text-gray-500">Type</th>
                    <th className="px-4 py-2 text-left text-xs font-medium text-gray-500">Amount</th>
                    <th className="px-4 py-2 text-left text-xs font-medium text-gray-500">Reason</th>
                    <th className="px-4 py-2 text-left text-xs font-medium text-gray-500">By</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200">
                  {transactionHistory.map((transaction) => (
                    <tr key={transaction.id}>
                      <td className="px-4 py-2 text-sm">
                        {new Date(transaction.transactionDate).toLocaleDateString()}
                      </td>
                      <td className="px-4 py-2 text-sm">
                        <span
                          className={`px-2 py-1 rounded text-xs font-semibold ${
                            transaction.transactionType === 1
                              ? 'bg-blue-100 text-blue-800'
                              : transaction.transactionType === 2
                              ? 'bg-red-100 text-red-800'
                              : transaction.transactionType === 3
                              ? 'bg-green-100 text-green-800'
                              : 'bg-gray-100 text-gray-800'
                          }`}
                        >
                          {transaction.transactionTypeDisplay}
                        </span>
                      </td>
                      <td className="px-4 py-2 text-sm font-semibold">
                        KES {transaction.amount.toLocaleString()}
                      </td>
                      <td className="px-4 py-2 text-sm">{transaction.reason || '-'}</td>
                      <td className="px-4 py-2 text-sm">{transaction.createdByUserName}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            <div className="mt-4">
              <button
                onClick={() => setModalType(null)}
                className="w-full bg-gray-300 text-gray-700 py-2 px-4 rounded-lg hover:bg-gray-400"
              >
                Close
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
