'use client'

import { useState, useEffect } from 'react'
import Link from 'next/link'
import { useAuth } from '@/contexts/AuthContext'
import { securityDepositService } from '@/lib/services/securityDepositService'
import type {
  SecurityDepositBalance,
  SecurityDepositTransaction
} from '@/lib/types/securityDeposit.types'
import { FaMoneyBillWave, FaArrowDown, FaArrowUp, FaInfoCircle, FaMobileAlt } from 'react-icons/fa'

export default function TenantSecurityDepositPage() {
  const { user } = useAuth()
  const [depositBalance, setDepositBalance] = useState<SecurityDepositBalance | null>(null)
  const [transactions, setTransactions] = useState<SecurityDepositTransaction[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (user?.tenantId) {
      loadDepositData()
    }
  }, [user])

  const loadDepositData = async () => {
    if (!user?.tenantId) return

    try {
      setLoading(true)
      const [balance, history] = await Promise.all([
        securityDepositService.getDepositBalance(user.tenantId),
        securityDepositService.getTransactionHistory(user.tenantId)
      ])
      setDepositBalance(balance)
      setTransactions(history)
    } catch (error) {
      console.error('Failed to load deposit data:', error)
    } finally {
      setLoading(false)
    }
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-gray-500">Loading security deposit information...</div>
      </div>
    )
  }

  if (!depositBalance) {
    return (
      <div className="container mx-auto p-6">
        <div className="bg-yellow-50 border-l-4 border-yellow-400 p-4 rounded">
          <div className="flex items-start">
            <FaInfoCircle className="text-yellow-400 text-xl mr-3 mt-1" />
            <div>
              <h3 className="text-lg font-semibold text-yellow-800 mb-2">
                No Security Deposit Record
              </h3>
              <p className="text-yellow-700">
                You don't have a security deposit on record. Please contact your landlord if you believe this is an error.
              </p>
            </div>
          </div>
        </div>
      </div>
    )
  }

  const getTransactionIcon = (type: number) => {
    if (type === 1) return <FaMoneyBillWave className="text-blue-500" />
    if (type === 2) return <FaArrowDown className="text-red-500" />
    if (type === 3) return <FaArrowUp className="text-green-500" />
    return <FaMoneyBillWave className="text-gray-500" />
  }

  const getTransactionColor = (type: number) => {
    if (type === 1) return 'text-blue-600'
    if (type === 2) return 'text-red-600'
    if (type === 3) return 'text-green-600'
    return 'text-gray-600'
  }

  const amountOwed = depositBalance.initialDeposit - depositBalance.currentBalance
  const needsPayment = amountOwed > 0

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold text-gray-900">Security Deposit</h1>
        {needsPayment && (
          <Link
            href="/tenant-portal/pay-security-deposit"
            className="flex items-center gap-2 px-6 py-3 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors font-medium shadow-sm"
          >
            <FaMobileAlt />
            Pay Deposit
          </Link>
        )}
      </div>

      {/* Alert for unpaid deposit */}
      {needsPayment && (
        <div className="bg-gradient-to-r from-yellow-50 to-orange-50 border-l-4 border-yellow-500 rounded-lg p-6 shadow-sm">
          <div className="flex items-start gap-4">
            <div className="flex-shrink-0">
              <FaInfoCircle className="text-yellow-600 text-2xl" />
            </div>
            <div className="flex-1">
              <h3 className="text-lg font-semibold text-yellow-900">Security Deposit Payment Required</h3>
              <p className="text-yellow-700 mt-1">
                You need to pay <strong>KSh {amountOwed.toLocaleString()}</strong> to complete your security deposit.
              </p>
              <Link
                href="/tenant-portal/pay-security-deposit"
                className="inline-flex items-center gap-2 mt-4 px-5 py-2.5 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors font-medium shadow-sm"
              >
                <FaMobileAlt />
                Pay Now with M-Pesa
              </Link>
            </div>
          </div>
        </div>
      )}

      {/* Info Box */}
      <div className="bg-blue-50 border border-blue-200 rounded-xl p-6">
        <div className="flex items-start gap-4">
          <FaInfoCircle className="text-blue-600 text-xl flex-shrink-0 mt-1" />
          <div>
            <h3 className="text-lg font-semibold text-blue-900 mb-2">
              About Your Security Deposit
            </h3>
            <p className="text-blue-700">
              Your security deposit is held by the landlord and will be refunded when you move out, minus any deductions for damages or unpaid rent.
            </p>
          </div>
        </div>
      </div>

      {/* Balance Summary Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <div className="bg-white rounded-lg shadow-sm border border-gray-100 p-5">
          <p className="text-xs font-medium text-gray-600 uppercase tracking-wide">Required Deposit</p>
          <p className="text-2xl font-bold text-blue-600 mt-1">
            KSh {depositBalance.initialDeposit.toLocaleString()}
          </p>
        </div>

        <div className="bg-white rounded-lg shadow-sm border border-gray-100 p-5">
          <p className="text-xs font-medium text-gray-600 uppercase tracking-wide">Amount Paid</p>
          <p className="text-2xl font-bold text-green-600 mt-1">
            KSh {depositBalance.currentBalance.toLocaleString()}
          </p>
        </div>

        <div className="bg-white rounded-lg shadow-sm border border-gray-100 p-5">
          <p className="text-xs font-medium text-gray-600 uppercase tracking-wide">Deductions</p>
          <p className="text-2xl font-bold text-red-600 mt-1">
            KSh {depositBalance.totalDeductions.toLocaleString()}
          </p>
        </div>

        <div className={`bg-white rounded-lg shadow-sm p-5 ${needsPayment ? 'border-2 border-yellow-500' : 'border-2 border-green-500'}`}>
          <p className="text-xs font-medium text-gray-600 uppercase tracking-wide">
            {needsPayment ? 'Amount Owed' : 'Refunds'}
          </p>
          <p className={`text-2xl font-bold mt-1 ${needsPayment ? 'text-yellow-600' : 'text-orange-600'}`}>
            KSh {needsPayment ? amountOwed.toLocaleString() : depositBalance.totalRefunds.toLocaleString()}
          </p>
        </div>
      </div>

      {/* Transaction History */}
      <div className="bg-white rounded-lg shadow p-6">
        <h2 className="text-xl font-bold mb-4">Transaction History</h2>

        {transactions.length === 0 ? (
          <div className="text-center py-8 text-gray-500">
            <p>No transactions yet</p>
          </div>
        ) : (
          <div className="space-y-4">
            {transactions.map((transaction) => (
              <div
                key={transaction.id}
                className="flex items-start justify-between p-4 border rounded-lg hover:bg-gray-50"
              >
                <div className="flex items-start gap-4">
                  <div className="mt-1">
                    {getTransactionIcon(transaction.transactionType)}
                  </div>
                  <div>
                    <div className="flex items-center gap-2 mb-1">
                      <span className="font-semibold text-gray-800">
                        {transaction.transactionTypeDisplay}
                      </span>
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
                        {transaction.transactionType === 1
                          ? 'Deposit'
                          : transaction.transactionType === 2
                          ? 'Deduction'
                          : transaction.transactionType === 3
                          ? 'Refund'
                          : 'Adjustment'}
                      </span>
                    </div>
                    <p className="text-sm text-gray-600">
                      {new Date(transaction.transactionDate).toLocaleDateString('en-GB', {
                        year: 'numeric',
                        month: 'long',
                        day: 'numeric'
                      })}
                    </p>
                    {transaction.reason && (
                      <p className="text-sm text-gray-700 mt-2">
                        <span className="font-medium">Reason:</span> {transaction.reason}
                      </p>
                    )}
                    {transaction.notes && (
                      <p className="text-sm text-gray-600 mt-1">
                        <span className="font-medium">Notes:</span> {transaction.notes}
                      </p>
                    )}
                    <p className="text-xs text-gray-500 mt-2">
                      Processed by {transaction.createdByUserName}
                    </p>
                  </div>
                </div>
                <div className="text-right">
                  <p
                    className={`text-xl font-bold ${getTransactionColor(
                      transaction.transactionType
                    )}`}
                  >
                    {transaction.transactionType === 1 ? '+' : '-'}KES{' '}
                    {transaction.amount.toLocaleString()}
                  </p>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Helpful Tips */}
      <div className="mt-6 bg-gray-50 p-6 rounded-lg">
        <h3 className="font-semibold text-gray-800 mb-3">Important Information</h3>
        <ul className="space-y-2 text-sm text-gray-700">
          <li className="flex items-start">
            <span className="text-blue-500 mr-2">•</span>
            <span>
              Your security deposit will be refunded within 30 days after you move out, subject to deductions for damages or unpaid rent.
            </span>
          </li>
          <li className="flex items-start">
            <span className="text-blue-500 mr-2">•</span>
            <span>
              Any deductions from your deposit will be documented with reasons and evidence.
            </span>
          </li>
          <li className="flex items-start">
            <span className="text-blue-500 mr-2">•</span>
            <span>
              If you have questions about any transaction, please contact your landlord or property manager.
            </span>
          </li>
          <li className="flex items-start">
            <span className="text-blue-500 mr-2">•</span>
            <span>
              Keep your unit in good condition to ensure a full refund of your deposit.
            </span>
          </li>
        </ul>
      </div>
    </div>
  )
}
