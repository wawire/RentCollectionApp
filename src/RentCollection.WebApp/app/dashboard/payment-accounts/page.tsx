'use client'

import { useState, useEffect } from 'react'
import Link from 'next/link'
import { Banknote, CheckCircle2, Landmark, Loader2, Pencil, Plus, Smartphone, Star, StarOff, Trash2 } from 'lucide-react'
import { landlordPaymentAccountService } from '@/lib/services/landlordPaymentAccountService'
import {
  LandlordPaymentAccount,
  CreateLandlordPaymentAccountDto,
  UpdateLandlordPaymentAccountDto,
  PaymentAccountType,
} from '@/lib/types/landlordPaymentAccount.types'
import LoadingSpinner from '@/components/common/LoadingSpinner'
import ProtectedRoute from '@/components/auth/ProtectedRoute'

export default function PaymentAccountsPage() {
  const [accounts, setAccounts] = useState<LandlordPaymentAccount[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)
  const [showCreateModal, setShowCreateModal] = useState(false)
  const [showEditModal, setShowEditModal] = useState(false)
  const [selectedAccount, setSelectedAccount] = useState<LandlordPaymentAccount | null>(null)
  const [actionLoading, setActionLoading] = useState(false)

  // Form state
  const [formData, setFormData] = useState<CreateLandlordPaymentAccountDto>({
    accountName: '',
    accountType: PaymentAccountType.MPesaPaybill,
    isDefault: false,
    isActive: true,
    autoReconciliation: false,
  })

  useEffect(() => {
    fetchAccounts()
  }, [])

  const fetchAccounts = async () => {
    try {
      setLoading(true)
      const data = await landlordPaymentAccountService.getMyAccounts()
      setAccounts(data)
      setError(null)
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load payment accounts')
    } finally {
      setLoading(false)
    }
  }

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      setActionLoading(true)
      await landlordPaymentAccountService.create(formData)
      setSuccessMessage('Payment account created successfully!')
      setShowCreateModal(false)
      resetForm()
      await fetchAccounts()
      setTimeout(() => setSuccessMessage(null), 3000)
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to create payment account')
      setTimeout(() => setError(null), 3000)
    } finally {
      setActionLoading(false)
    }
  }

  const handleUpdate = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!selectedAccount) return

    try {
      setActionLoading(true)
      const updateDto: UpdateLandlordPaymentAccountDto = {
        accountName: formData.accountName,
        accountType: formData.accountType,
        paybillNumber: formData.paybillNumber,
        paybillName: formData.paybillName,
        tillNumber: formData.tillNumber,
        mPesaPhoneNumber: formData.mPesaPhoneNumber,
        bankName: formData.bankName,
        bankAccountNumber: formData.bankAccountNumber,
        bankAccountName: formData.bankAccountName,
        bankBranch: formData.bankBranch,
        swiftCode: formData.swiftCode,
        mPesaConsumerKey: formData.mPesaConsumerKey,
        mPesaConsumerSecret: formData.mPesaConsumerSecret,
        mPesaShortCode: formData.mPesaShortCode,
        mPesaPasskey: formData.mPesaPasskey,
        isDefault: formData.isDefault || false,
        isActive: formData.isActive || true,
        autoReconciliation: formData.autoReconciliation || false,
        paymentInstructions: formData.paymentInstructions,
      }

      await landlordPaymentAccountService.update(selectedAccount.id, updateDto)
      setSuccessMessage('Payment account updated successfully!')
      setShowEditModal(false)
      setSelectedAccount(null)
      resetForm()
      await fetchAccounts()
      setTimeout(() => setSuccessMessage(null), 3000)
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to update payment account')
      setTimeout(() => setError(null), 3000)
    } finally {
      setActionLoading(false)
    }
  }

  const handleDelete = async (id: number, accountName: string) => {
    if (!confirm(`Are you sure you want to delete the payment account "${accountName}"?`)) {
      return
    }

    try {
      await landlordPaymentAccountService.delete(id)
      setSuccessMessage('Payment account deleted successfully')
      await fetchAccounts()
      setTimeout(() => setSuccessMessage(null), 3000)
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete payment account')
      setTimeout(() => setError(null), 3000)
    }
  }

  const handleSetDefault = async (id: number) => {
    try {
      await landlordPaymentAccountService.setDefault(id)
      setSuccessMessage('Default payment account updated')
      await fetchAccounts()
      setTimeout(() => setSuccessMessage(null), 3000)
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to set default account')
      setTimeout(() => setError(null), 3000)
    }
  }

  const handleEditClick = (account: LandlordPaymentAccount) => {
    setSelectedAccount(account)
    setFormData({
      accountName: account.accountName,
      accountType: account.accountType,
      paybillNumber: account.paybillNumber,
      paybillName: account.paybillName,
      tillNumber: account.tillNumber,
      mPesaPhoneNumber: account.mPesaPhoneNumber,
      bankName: account.bankName,
      bankAccountNumber: account.bankAccountNumber,
      bankAccountName: account.bankAccountName,
      bankBranch: account.bankBranch,
      swiftCode: account.swiftCode,
      isDefault: account.isDefault,
      isActive: account.isActive,
      autoReconciliation: account.autoReconciliation,
      paymentInstructions: account.paymentInstructions,
    })
    setShowEditModal(true)
  }

  const resetForm = () => {
    setFormData({
      accountName: '',
      accountType: PaymentAccountType.MPesaPaybill,
      isDefault: false,
      isActive: true,
      autoReconciliation: false,
    })
  }

  const getAccountTypeIcon = (type: PaymentAccountType) => {
    switch (type) {
      case PaymentAccountType.MPesaPaybill:
      case PaymentAccountType.MPesaTillNumber:
      case PaymentAccountType.MPesaPhone:
        return <Smartphone className="text-green-600 w-5 h-5" />
      case PaymentAccountType.BankAccount:
        return <Landmark className="text-blue-600 w-5 h-5" />
      case PaymentAccountType.Cash:
        return <Banknote className="text-yellow-600 w-5 h-5" />
      default:
        return <Smartphone className="text-gray-600 w-5 h-5" />
    }
  }

  const getAccountTypeLabel = (type: PaymentAccountType) => {
    switch (type) {
      case PaymentAccountType.MPesaPaybill:
        return 'M-Pesa Paybill'
      case PaymentAccountType.MPesaTillNumber:
        return 'M-Pesa Till Number'
      case PaymentAccountType.MPesaPhone:
        return 'M-Pesa Phone'
      case PaymentAccountType.BankAccount:
        return 'Bank Account'
      case PaymentAccountType.Cash:
        return 'Cash'
      default:
        return type
    }
  }

  const content = loading ? (
    <div className="flex items-center justify-center h-96">
      <LoadingSpinner size="lg" text="Loading payment accounts..." />
    </div>
  ) : (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-serif font-normal text-primary tracking-wide">
            Payment Accounts
          </h1>
          <p className="text-primary/60 mt-2 tracking-wide">
            Manage how you receive rent payments from your tenants
          </p>
        </div>
        <button
          onClick={() => {
            resetForm()
            setShowCreateModal(true)
          }}
          className="flex items-center gap-2 px-6 py-3 bg-accent hover:bg-accent-600 text-primary font-medium rounded-lg transition-all shadow-sm tracking-wide"
        >
          <Plus className="w-4 h-4" />
          Add Payment Account
        </button>
      </div>

      {/* Alerts */}
      {error && (
        <div className="bg-red-50 border border-red-200 text-red-800 px-4 py-3 rounded-lg">
          {error}
        </div>
      )}
      {successMessage && (
        <div className="bg-green-50 border border-green-200 text-green-800 px-4 py-3 rounded-lg">
          {successMessage}
        </div>
      )}

      {/* Info Box */}
      {accounts.length === 0 && (
        <div className="bg-blue-50 border border-blue-200 rounded-lg p-6">
          <h3 className="text-lg font-semibold text-blue-900 mb-2">Get Started with Payment Accounts</h3>
          <p className="text-blue-800 mb-4">
            Set up payment accounts to receive rent from your tenants. You can add M-Pesa paybills, bank accounts, or cash collection points.
          </p>
          <ul className="list-disc list-inside space-y-1 text-sm text-blue-800">
            <li><strong>M-Pesa Paybill:</strong> Automated payments with unit-specific account numbers</li>
            <li><strong>M-Pesa Till/Phone:</strong> Quick payments for small landlords</li>
            <li><strong>Bank Account:</strong> Traditional bank transfers</li>
            <li><strong>Cash:</strong> Manual cash collection tracking</li>
          </ul>
        </div>
      )}

      {/* Accounts List */}
      {accounts.length > 0 && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {accounts.map((account) => (
            <div
              key={account.id}
              className={`bg-white rounded-lg shadow-sm border-2 transition-all ${
                account.isDefault ? 'border-accent' : 'border-primary/10'
              }`}
            >
              <div className="p-6">
                {/* Header */}
                <div className="flex items-start justify-between mb-4">
                  <div className="flex items-center gap-3">
                    <div className="text-2xl">{getAccountTypeIcon(account.accountType)}</div>
                    <div>
                      <h3 className="font-semibold text-primary">{account.accountName}</h3>
                      <p className="text-sm text-primary/60">{getAccountTypeLabel(account.accountType)}</p>
                    </div>
                  </div>
                  <button
                    onClick={() => handleSetDefault(account.id)}
                    className="text-xl"
                    title={account.isDefault ? 'Default Account' : 'Set as Default'}
                  >
                    {account.isDefault ? (
                      <Star className="text-accent w-5 h-5" />
                    ) : (
                      <StarOff className="text-primary/30 hover:text-accent transition-colors w-5 h-5" />
                    )}
                  </button>
                </div>

                {/* Account Details */}
                <div className="space-y-2 mb-4">
                  {account.accountType === PaymentAccountType.MPesaPaybill && account.paybillNumber && (
                    <div>
                      <p className="text-xs text-primary/60">Paybill Number</p>
                      <p className="text-sm font-mono font-medium text-primary">{account.paybillNumber}</p>
                    </div>
                  )}
                  {account.accountType === PaymentAccountType.MPesaTillNumber && account.tillNumber && (
                    <div>
                      <p className="text-xs text-primary/60">Till Number</p>
                      <p className="text-sm font-mono font-medium text-primary">{account.tillNumber}</p>
                    </div>
                  )}
                  {account.accountType === PaymentAccountType.MPesaPhone && account.mPesaPhoneNumber && (
                    <div>
                      <p className="text-xs text-primary/60">M-Pesa Phone</p>
                      <p className="text-sm font-mono font-medium text-primary">{account.mPesaPhoneNumber}</p>
                    </div>
                  )}
                  {account.accountType === PaymentAccountType.BankAccount && (
                    <>
                      {account.bankName && (
                        <div>
                          <p className="text-xs text-primary/60">Bank</p>
                          <p className="text-sm font-medium text-primary">{account.bankName}</p>
                        </div>
                      )}
                      {account.bankAccountNumber && (
                        <div>
                          <p className="text-xs text-primary/60">Account Number</p>
                          <p className="text-sm font-mono font-medium text-primary">{account.bankAccountNumber}</p>
                        </div>
                      )}
                    </>
                  )}
                  {account.propertyName && (
                    <div>
                      <p className="text-xs text-primary/60">Property</p>
                      <p className="text-sm font-medium text-primary">{account.propertyName}</p>
                    </div>
                  )}
                </div>

                {/* Status Badges */}
                <div className="flex gap-2 mb-4">
                  {account.isDefault && (
                    <span className="px-2 py-1 rounded-full text-xs font-medium bg-accent/20 text-accent border border-accent/30">
                      Default
                    </span>
                  )}
                  <span
                    className={`px-2 py-1 rounded-full text-xs font-medium ${
                      account.isActive
                        ? 'bg-green-100 text-green-800 border border-green-200'
                        : 'bg-gray-100 text-gray-800 border border-gray-200'
                    }`}
                  >
                    {account.isActive ? 'Active' : 'Inactive'}
                  </span>
                  {account.autoReconciliation && (
                    <span className="px-2 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800 border border-blue-200">
                      Auto Sync
                    </span>
                  )}
                </div>

                {/* Actions */}
                <div className="flex gap-2">
                  <button
                    onClick={() => handleEditClick(account)}
                    className="flex-1 flex items-center justify-center gap-2 px-4 py-2 border border-primary/20 text-primary rounded-lg hover:bg-primary/5 transition-colors"
                  >
                    <Pencil className="w-4 h-4" />
                    Edit
                  </button>
                  <button
                    onClick={() => handleDelete(account.id, account.accountName)}
                    className="flex items-center justify-center gap-2 px-4 py-2 border border-red-200 text-red-600 rounded-lg hover:bg-red-50 transition-colors"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Create Modal */}
      {showCreateModal && <AccountFormModal
        title="Add Payment Account"
        formData={formData}
        setFormData={setFormData}
        onSubmit={handleCreate}
        onCancel={() => {
          setShowCreateModal(false)
          resetForm()
        }}
        loading={actionLoading}
      />}

      {/* Edit Modal */}
      {showEditModal && selectedAccount && <AccountFormModal
        title="Edit Payment Account"
        formData={formData}
        setFormData={setFormData}
        onSubmit={handleUpdate}
        onCancel={() => {
          setShowEditModal(false)
          setSelectedAccount(null)
          resetForm()
        }}
        loading={actionLoading}
      />}
    </div>
  )

  return (
    <ProtectedRoute allowedRoles={['PlatformAdmin', 'Landlord']}>
      {content}
    </ProtectedRoute>
  )
}

// Modal Component
interface AccountFormModalProps {
  title: string
  formData: CreateLandlordPaymentAccountDto
  setFormData: React.Dispatch<React.SetStateAction<CreateLandlordPaymentAccountDto>>
  onSubmit: (e: React.FormEvent) => void
  onCancel: () => void
  loading: boolean
}

function AccountFormModal({ title, formData, setFormData, onSubmit, onCancel, loading }: AccountFormModalProps) {
  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4 overflow-y-auto">
      <div className="bg-white rounded-lg shadow-xl max-w-2xl w-full my-8">
        <form onSubmit={onSubmit}>
          {/* Header */}
          <div className="px-6 py-4 border-b border-primary/10">
            <h2 className="text-2xl font-serif font-normal text-primary tracking-wide">{title}</h2>
          </div>

          {/* Body */}
          <div className="px-6 py-4 space-y-4 max-h-[calc(100vh-16rem)] overflow-y-auto">
            {/* Account Name */}
            <div>
              <label className="block text-sm font-medium text-primary/80 mb-1">
                Account Name <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                required
                value={formData.accountName}
                onChange={(e) => setFormData({ ...formData, accountName: e.target.value })}
                className="w-full px-4 py-2 border border-primary/20 rounded-lg focus:outline-none focus:ring-2 focus:ring-accent"
                placeholder="e.g., Main M-Pesa Account"
              />
            </div>

            {/* Account Type */}
            <div>
              <label className="block text-sm font-medium text-primary/80 mb-1">
                Account Type <span className="text-red-500">*</span>
              </label>
              <select
                required
                value={formData.accountType}
                onChange={(e) => setFormData({ ...formData, accountType: e.target.value as PaymentAccountType })}
                className="w-full px-4 py-2 border border-primary/20 rounded-lg focus:outline-none focus:ring-2 focus:ring-accent"
              >
                <option value={PaymentAccountType.MPesaPaybill}>M-Pesa Paybill</option>
                <option value={PaymentAccountType.MPesaTillNumber}>M-Pesa Till Number</option>
                <option value={PaymentAccountType.MPesaPhone}>M-Pesa Phone</option>
                <option value={PaymentAccountType.BankAccount}>Bank Account</option>
                <option value={PaymentAccountType.Cash}>Cash</option>
              </select>
            </div>

            {/* M-Pesa Paybill Fields */}
            {formData.accountType === PaymentAccountType.MPesaPaybill && (
              <>
                <div>
                  <label className="block text-sm font-medium text-primary/80 mb-1">
                    Paybill Number <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="text"
                    required
                    value={formData.paybillNumber || ''}
                    onChange={(e) => setFormData({ ...formData, paybillNumber: e.target.value })}
                    className="w-full px-4 py-2 border border-primary/20 rounded-lg focus:outline-none focus:ring-2 focus:ring-accent"
                    placeholder="e.g., 400200"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-primary/80 mb-1">
                    Paybill Name
                  </label>
                  <input
                    type="text"
                    value={formData.paybillName || ''}
                    onChange={(e) => setFormData({ ...formData, paybillName: e.target.value })}
                    className="w-full px-4 py-2 border border-primary/20 rounded-lg focus:outline-none focus:ring-2 focus:ring-accent"
                    placeholder="e.g., My Properties Ltd"
                  />
                </div>
              </>
            )}

            {/* M-Pesa Till Fields */}
            {formData.accountType === PaymentAccountType.MPesaTillNumber && (
              <div>
                <label className="block text-sm font-medium text-primary/80 mb-1">
                  Till Number <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  required
                  value={formData.tillNumber || ''}
                  onChange={(e) => setFormData({ ...formData, tillNumber: e.target.value })}
                  className="w-full px-4 py-2 border border-primary/20 rounded-lg focus:outline-none focus:ring-2 focus:ring-accent"
                  placeholder="e.g., 123456"
                />
              </div>
            )}

            {/* M-Pesa Phone Fields */}
            {formData.accountType === PaymentAccountType.MPesaPhone && (
              <div>
                <label className="block text-sm font-medium text-primary/80 mb-1">
                  M-Pesa Phone Number <span className="text-red-500">*</span>
                </label>
                <input
                  type="tel"
                  required
                  value={formData.mPesaPhoneNumber || ''}
                  onChange={(e) => setFormData({ ...formData, mPesaPhoneNumber: e.target.value })}
                  className="w-full px-4 py-2 border border-primary/20 rounded-lg focus:outline-none focus:ring-2 focus:ring-accent"
                  placeholder="e.g., 0712345678"
                />
              </div>
            )}

            {/* Bank Account Fields */}
            {formData.accountType === PaymentAccountType.BankAccount && (
              <>
                <div>
                  <label className="block text-sm font-medium text-primary/80 mb-1">
                    Bank Name <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="text"
                    required
                    value={formData.bankName || ''}
                    onChange={(e) => setFormData({ ...formData, bankName: e.target.value })}
                    className="w-full px-4 py-2 border border-primary/20 rounded-lg focus:outline-none focus:ring-2 focus:ring-accent"
                    placeholder="e.g., Equity Bank"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-primary/80 mb-1">
                    Account Number <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="text"
                    required
                    value={formData.bankAccountNumber || ''}
                    onChange={(e) => setFormData({ ...formData, bankAccountNumber: e.target.value })}
                    className="w-full px-4 py-2 border border-primary/20 rounded-lg focus:outline-none focus:ring-2 focus:ring-accent"
                    placeholder="e.g., 1234567890"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-primary/80 mb-1">
                    Account Name <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="text"
                    required
                    value={formData.bankAccountName || ''}
                    onChange={(e) => setFormData({ ...formData, bankAccountName: e.target.value })}
                    className="w-full px-4 py-2 border border-primary/20 rounded-lg focus:outline-none focus:ring-2 focus:ring-accent"
                    placeholder="e.g., John Doe"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-primary/80 mb-1">
                    Branch
                  </label>
                  <input
                    type="text"
                    value={formData.bankBranch || ''}
                    onChange={(e) => setFormData({ ...formData, bankBranch: e.target.value })}
                    className="w-full px-4 py-2 border border-primary/20 rounded-lg focus:outline-none focus:ring-2 focus:ring-accent"
                    placeholder="e.g., Westlands Branch"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-primary/80 mb-1">
                    SWIFT Code
                  </label>
                  <input
                    type="text"
                    value={formData.swiftCode || ''}
                    onChange={(e) => setFormData({ ...formData, swiftCode: e.target.value })}
                    className="w-full px-4 py-2 border border-primary/20 rounded-lg focus:outline-none focus:ring-2 focus:ring-accent"
                    placeholder="e.g., EQBLKENA"
                  />
                </div>
              </>
            )}

            {/* Payment Instructions */}
            <div>
              <label className="block text-sm font-medium text-primary/80 mb-1">
                Payment Instructions
              </label>
              <textarea
                rows={3}
                value={formData.paymentInstructions || ''}
                onChange={(e) => setFormData({ ...formData, paymentInstructions: e.target.value })}
                className="w-full px-4 py-2 border border-primary/20 rounded-lg focus:outline-none focus:ring-2 focus:ring-accent"
                placeholder="Instructions for tenants on how to pay (e.g., 'Send payment to paybill 400200, use your unit number as account number')"
              />
            </div>

            {/* Settings */}
            <div className="space-y-2">
              <label className="flex items-center gap-2">
                <input
                  type="checkbox"
                  checked={formData.isDefault || false}
                  onChange={(e) => setFormData({ ...formData, isDefault: e.target.checked })}
                  className="w-4 h-4 text-accent focus:ring-accent border-primary/20 rounded"
                />
                <span className="text-sm text-primary/80">Set as default payment account</span>
              </label>
              <label className="flex items-center gap-2">
                <input
                  type="checkbox"
                  checked={formData.isActive !== undefined ? formData.isActive : true}
                  onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                  className="w-4 h-4 text-accent focus:ring-accent border-primary/20 rounded"
                />
                <span className="text-sm text-primary/80">Active (accepting payments)</span>
              </label>
            </div>
          </div>

          {/* Footer */}
          <div className="px-6 py-4 border-t border-primary/10 flex justify-end gap-3">
            <button
              type="button"
              onClick={onCancel}
              disabled={loading}
              className="px-6 py-2 border border-primary/20 text-primary rounded-lg hover:bg-primary/5 transition-colors disabled:opacity-50"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={loading}
              className="px-6 py-2 bg-accent hover:bg-accent-600 text-primary font-medium rounded-lg transition-colors disabled:opacity-50 flex items-center gap-2"
            >
              {loading ? (
                <>
                  <Loader2 className="animate-spin w-4 h-4" />
                  Saving...
                </>
              ) : (
                <>
                  <CheckCircle2 className="w-4 h-4" />
                  Save Account
                </>
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

