'use client'

import { useState, useEffect } from 'react'
import { useParams, useRouter } from 'next/navigation'
import Link from 'next/link'
import { moveOutInspectionService } from '@/lib/services/moveOutInspectionService'
import type {
  MoveOutInspection,
  RecordInspectionDto,
  RecordInspectionItemDto,
  SettleInspectionDto,
  ProcessRefundDto,
} from '@/lib/types'
import { MoveOutInspectionStatus, InspectionCategory, PhotoType } from '@/lib/types'
import PhotoUpload from '@/components/inspections/PhotoUpload'
import PhotoGallery from '@/components/inspections/PhotoGallery'
import ProtectedRoute from '@/components/auth/ProtectedRoute'
import {
  ArrowLeft,
  Banknote,
  Camera,
  CheckCircle2,
  Pencil,
  Plus,
  Save,
  Trash2,
} from 'lucide-react'

export default function InspectionDetailPage() {
  const params = useParams()
  const router = useRouter()
  const inspectionId = parseInt(params.id as string)

  const [inspection, setInspection] = useState<MoveOutInspection | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)

  // Recording state
  const [recordingMode, setRecordingMode] = useState(false)
  const [recordForm, setRecordForm] = useState<RecordInspectionDto>({
    overallCondition: '',
    generalNotes: '',
    unpaidRent: 0,
    unpaidUtilities: 0,
    otherCharges: 0,
    inspectionItems: [],
  })

  // Settlement state
  const [settlementMode, setSettlementMode] = useState(false)
  const [settlementNotes, setSettlementNotes] = useState('')

  // Refund state
  const [refundMode, setRefundMode] = useState(false)
  const [refundForm, setRefundForm] = useState<ProcessRefundDto>({
    refundMethod: 'MPesa',
    refundReference: '',
    notes: '',
  })

  // Photo management state
  const [photoUploadMode, setPhotoUploadMode] = useState(false)
  const [selectedItemForPhoto, setSelectedItemForPhoto] = useState<number | undefined>(undefined)

  useEffect(() => {
    loadInspection()
  }, [inspectionId])

  const loadInspection = async () => {
    try {
      setLoading(true)
      const data = await moveOutInspectionService.getInspectionById(inspectionId)
      setInspection(data)

      // Initialize record form if inspection exists
      if (data && data.status < MoveOutInspectionStatus.Completed) {
        setRecordForm({
          overallCondition: data.overallCondition || '',
          generalNotes: data.generalNotes || '',
          unpaidRent: data.unpaidRent || 0,
          unpaidUtilities: data.unpaidUtilities || 0,
          otherCharges: data.otherCharges || 0,
          inspectionItems: data.inspectionItems.map(item => ({
            category: item.category.toString(),
            itemName: item.itemName,
            moveInCondition: item.moveInCondition,
            moveOutCondition: item.moveOutCondition,
            isDamaged: item.isDamaged,
            damageDescription: item.damageDescription,
            estimatedRepairCost: item.estimatedRepairCost,
            notes: item.notes,
          })),
        })
      }
    } catch (error: any) {
      console.error('Failed to load inspection:', error)
      setError('Failed to load inspection details.')
    } finally {
      setLoading(false)
    }
  }

  const handleRecordInspection = async () => {
    if (!inspection) return

    try {
      setSaving(true)
      const updated = await moveOutInspectionService.recordInspection(inspectionId, recordForm)
      setInspection(updated)
      setRecordingMode(false)
      setError('')
    } catch (error: any) {
      console.error('Failed to record inspection:', error)
      setError(error.response?.data?.error || 'Failed to record inspection.')
    } finally {
      setSaving(false)
    }
  }

  const handleSettleInspection = async () => {
    if (!inspection) return

    try {
      setSaving(true)
      const updated = await moveOutInspectionService.settleInspection(inspectionId, {
        settlementNotes,
      })
      setInspection(updated)
      setSettlementMode(false)
      setError('')
    } catch (error: any) {
      console.error('Failed to settle inspection:', error)
      setError(error.response?.data?.error || 'Failed to settle inspection.')
    } finally {
      setSaving(false)
    }
  }

  const handleProcessRefund = async () => {
    if (!inspection) return

    try {
      setSaving(true)
      const updated = await moveOutInspectionService.processRefund(inspectionId, refundForm)
      setInspection(updated)
      setRefundMode(false)
      setError('')
    } catch (error: any) {
      console.error('Failed to process refund:', error)
      setError(error.response?.data?.error || 'Failed to process refund.')
    } finally {
      setSaving(false)
    }
  }

  const addInspectionItem = () => {
    setRecordForm({
      ...recordForm,
      inspectionItems: [
        ...recordForm.inspectionItems,
        {
          category: InspectionCategory.Walls.toString(),
          itemName: '',
          moveInCondition: '',
          moveOutCondition: '',
          isDamaged: false,
          damageDescription: '',
          estimatedRepairCost: 0,
          notes: '',
        },
      ],
    })
  }

  const removeInspectionItem = (index: number) => {
    setRecordForm({
      ...recordForm,
      inspectionItems: recordForm.inspectionItems.filter((_, i) => i !== index),
    })
  }

  const updateInspectionItem = (index: number, field: keyof RecordInspectionItemDto, value: any) => {
    const updated = [...recordForm.inspectionItems]
    updated[index] = { ...updated[index], [field]: value }
    setRecordForm({ ...recordForm, inspectionItems: updated })
  }

  const handlePhotoUploadSuccess = (photoData: any) => {
    // Reload inspection to get updated photos
    loadInspection()
    setPhotoUploadMode(false)
    setSelectedItemForPhoto(undefined)
  }

  const handleDeletePhoto = async (photoId: number) => {
    try {
      await moveOutInspectionService.deletePhoto(photoId)
      // Reload inspection to refresh photos
      loadInspection()
    } catch (error: any) {
      console.error('Failed to delete photo:', error)
      setError('Failed to delete photo.')
    }
  }

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-KE', {
      style: 'currency',
      currency: 'KES',
      minimumFractionDigits: 0,
    }).format(amount)
  }

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-KE', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    })
  }

  const getStatusBadge = (status: MoveOutInspectionStatus) => {
    const badges: Record<MoveOutInspectionStatus, string> = {
      [MoveOutInspectionStatus.Scheduled]: 'bg-blue-100 text-blue-800',
      [MoveOutInspectionStatus.InProgress]: 'bg-yellow-100 text-yellow-800',
      [MoveOutInspectionStatus.Completed]: 'bg-purple-100 text-purple-800',
      [MoveOutInspectionStatus.Reviewed]: 'bg-indigo-100 text-indigo-800',
      [MoveOutInspectionStatus.DisputeRaised]: 'bg-red-100 text-red-800',
      [MoveOutInspectionStatus.Settled]: 'bg-green-100 text-green-800',
      [MoveOutInspectionStatus.RefundProcessed]: 'bg-teal-100 text-teal-800',
    }
    return badges[status] || 'bg-gray-100 text-gray-800'
  }

  const categoryOptions = [
    { value: InspectionCategory.Walls, label: 'Walls' },
    { value: InspectionCategory.Floors, label: 'Floors' },
    { value: InspectionCategory.Ceiling, label: 'Ceiling' },
    { value: InspectionCategory.Doors, label: 'Doors' },
    { value: InspectionCategory.Windows, label: 'Windows' },
    { value: InspectionCategory.Kitchen, label: 'Kitchen' },
    { value: InspectionCategory.Bathroom, label: 'Bathroom' },
    { value: InspectionCategory.Fixtures, label: 'Fixtures' },
    { value: InspectionCategory.Appliances, label: 'Appliances' },
    { value: InspectionCategory.Plumbing, label: 'Plumbing' },
    { value: InspectionCategory.Electrical, label: 'Electrical' },
    { value: InspectionCategory.Locks, label: 'Locks' },
    { value: InspectionCategory.Keys, label: 'Keys' },
    { value: InspectionCategory.Yard, label: 'Yard' },
    { value: InspectionCategory.Other, label: 'Other' },
  ]

  const content = loading ? (
    <div className="p-6">
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    </div>
  ) : !inspection ? (
    <div className="p-6">
      <div className="text-center text-red-600">Inspection not found</div>
    </div>
  ) : (
    <div className="p-6 max-w-7xl mx-auto">
      {/* Header */}
      <div className="mb-6">
        <Link href="/dashboard/move-out-inspections">
          <button className="flex items-center gap-2 text-blue-600 hover:text-blue-800 transition mb-4">
            <ArrowLeft className="w-4 h-4" />
            Back to Inspections
          </button>
        </Link>
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">
              Move-Out Inspection #{inspection.id}
            </h1>
            <p className="text-gray-600 mt-1">
              {inspection.tenantName} - {inspection.propertyName} {inspection.unitNumber}
            </p>
          </div>
          <span
            className={`px-4 py-2 rounded-full text-sm font-medium ${getStatusBadge(
              inspection.status
            )}`}
          >
            {inspection.statusDisplay}
          </span>
        </div>
      </div>

      {error && (
        <div className="mb-6 bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg">
          {error}
        </div>
      )}

      {/* Main Content */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Left Column - Details */}
        <div className="lg:col-span-2 space-y-6">
          {/* Basic Info Card */}
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
            <h2 className="text-lg font-semibold text-gray-900 mb-4">Inspection Details</h2>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <div className="text-sm text-gray-600">Move-Out Date</div>
                <div className="font-semibold">{formatDate(inspection.moveOutDate)}</div>
              </div>
              <div>
                <div className="text-sm text-gray-600">Inspection Date</div>
                <div className="font-semibold">{formatDate(inspection.inspectionDate)}</div>
              </div>
              <div>
                <div className="text-sm text-gray-600">Inspected By</div>
                <div className="font-semibold">{inspection.inspectedByUserName}</div>
              </div>
              <div>
                <div className="text-sm text-gray-600">Overall Condition</div>
                <div className="font-semibold">{inspection.overallCondition || 'Not recorded'}</div>
              </div>
            </div>
          </div>

          {/* Recording Interface */}
          {(inspection.status === MoveOutInspectionStatus.Scheduled ||
            inspection.status === MoveOutInspectionStatus.InProgress ||
            recordingMode) && (
            <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
              <div className="flex items-center justify-between mb-4">
                <h2 className="text-lg font-semibold text-gray-900">Record Inspection</h2>
                {!recordingMode && (
                  <button
                    onClick={() => setRecordingMode(true)}
                    className="flex items-center gap-2 text-blue-600 hover:text-blue-800"
                  >
                    <Pencil className="w-4 h-4" />
                    Start Recording
                  </button>
                )}
              </div>

              {recordingMode && (
                <div className="space-y-6">
                  {/* Overall Assessment */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Overall Condition
                    </label>
                    <select
                      value={recordForm.overallCondition}
                      onChange={(e) =>
                        setRecordForm({ ...recordForm, overallCondition: e.target.value })
                      }
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                    >
                      <option value="">Select condition...</option>
                      <option value="Excellent">Excellent</option>
                      <option value="Good">Good</option>
                      <option value="Fair">Fair</option>
                      <option value="Poor">Poor</option>
                    </select>
                  </div>

                  {/* Financial Charges */}
                  <div className="grid grid-cols-3 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Unpaid Rent (KSh)
                      </label>
                      <input
                        type="number"
                        value={recordForm.unpaidRent}
                        onChange={(e) =>
                          setRecordForm({ ...recordForm, unpaidRent: Number(e.target.value) })
                        }
                        className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Unpaid Utilities (KSh)
                      </label>
                      <input
                        type="number"
                        value={recordForm.unpaidUtilities}
                        onChange={(e) =>
                          setRecordForm({
                            ...recordForm,
                            unpaidUtilities: Number(e.target.value),
                          })
                        }
                        className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Other Charges (KSh)
                      </label>
                      <input
                        type="number"
                        value={recordForm.otherCharges}
                        onChange={(e) =>
                          setRecordForm({ ...recordForm, otherCharges: Number(e.target.value) })
                        }
                        className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                      />
                    </div>
                  </div>

                  {/* Inspection Items */}
                  <div>
                    <div className="flex items-center justify-between mb-4">
                      <h3 className="font-semibold text-gray-900">Inspection Items</h3>
                      <button
                        onClick={addInspectionItem}
                        className="flex items-center gap-2 text-blue-600 hover:text-blue-800"
                      >
                        <Plus className="w-4 h-4" />
                        Add Item
                      </button>
                    </div>

                    <div className="space-y-4">
                      {recordForm.inspectionItems.map((item, index) => (
                        <div key={index} className="border border-gray-200 rounded-lg p-4">
                          <div className="flex items-start justify-between mb-3">
                            <h4 className="font-medium">Item {index + 1}</h4>
                            <button
                              onClick={() => removeInspectionItem(index)}
                              className="text-red-600 hover:text-red-800"
                            >
                              <Trash2 className="w-4 h-4" />
                            </button>
                          </div>

                          <div className="grid grid-cols-2 gap-3">
                            <div>
                              <label className="block text-sm text-gray-600 mb-1">Category</label>
                              <select
                                value={item.category}
                                onChange={(e) =>
                                  updateInspectionItem(index, 'category', e.target.value)
                                }
                                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm"
                              >
                                {categoryOptions.map((opt) => (
                                  <option key={opt.value} value={opt.value}>
                                    {opt.label}
                                  </option>
                                ))}
                              </select>
                            </div>
                            <div>
                              <label className="block text-sm text-gray-600 mb-1">Item Name</label>
                              <input
                                type="text"
                                value={item.itemName}
                                onChange={(e) =>
                                  updateInspectionItem(index, 'itemName', e.target.value)
                                }
                                placeholder="e.g., Living Room Wall"
                                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm"
                              />
                            </div>
                            <div>
                              <label className="block text-sm text-gray-600 mb-1">
                                Move-In Condition
                              </label>
                              <input
                                type="text"
                                value={item.moveInCondition}
                                onChange={(e) =>
                                  updateInspectionItem(index, 'moveInCondition', e.target.value)
                                }
                                placeholder="e.g., Freshly painted"
                                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm"
                              />
                            </div>
                            <div>
                              <label className="block text-sm text-gray-600 mb-1">
                                Move-Out Condition
                              </label>
                              <input
                                type="text"
                                value={item.moveOutCondition}
                                onChange={(e) =>
                                  updateInspectionItem(index, 'moveOutCondition', e.target.value)
                                }
                                placeholder="e.g., Scuff marks visible"
                                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm"
                              />
                            </div>
                            <div className="col-span-2">
                              <label className="flex items-center gap-2">
                                <input
                                  type="checkbox"
                                  checked={item.isDamaged}
                                  onChange={(e) =>
                                    updateInspectionItem(index, 'isDamaged', e.target.checked)
                                  }
                                  className="rounded"
                                />
                                <span className="text-sm text-gray-700">Item is damaged</span>
                              </label>
                            </div>
                            {item.isDamaged && (
                              <>
                                <div className="col-span-2">
                                  <label className="block text-sm text-gray-600 mb-1">
                                    Damage Description
                                  </label>
                                  <textarea
                                    value={item.damageDescription || ''}
                                    onChange={(e) =>
                                      updateInspectionItem(
                                        index,
                                        'damageDescription',
                                        e.target.value
                                      )
                                    }
                                    rows={2}
                                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm"
                                  />
                                </div>
                                <div>
                                  <label className="block text-sm text-gray-600 mb-1">
                                    Estimated Repair Cost (KSh)
                                  </label>
                                  <input
                                    type="number"
                                    value={item.estimatedRepairCost}
                                    onChange={(e) =>
                                      updateInspectionItem(
                                        index,
                                        'estimatedRepairCost',
                                        Number(e.target.value)
                                      )
                                    }
                                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm"
                                  />
                                </div>
                              </>
                            )}
                          </div>
                        </div>
                      ))}
                    </div>
                  </div>

                  {/* General Notes */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      General Notes
                    </label>
                    <textarea
                      value={recordForm.generalNotes}
                      onChange={(e) =>
                        setRecordForm({ ...recordForm, generalNotes: e.target.value })
                      }
                      rows={4}
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                    />
                  </div>

                  {/* Action Buttons */}
                  <div className="flex gap-3">
                        <button
                          onClick={handleRecordInspection}
                          disabled={saving}
                          className="flex-1 bg-blue-600 text-white px-6 py-3 rounded-lg hover:bg-blue-700 transition disabled:bg-gray-400 font-medium flex items-center justify-center gap-2"
                        >
                          {saving ? (
                            <>
                              <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-white"></div>
                              Saving...
                            </>
                          ) : (
                            <>
                              <Save className="w-4 h-4" />
                              Save Inspection
                            </>
                          )}
                        </button>
                    <button
                      onClick={() => setRecordingMode(false)}
                      className="px-6 py-3 border border-gray-300 rounded-lg hover:bg-gray-50 transition font-medium"
                    >
                      Cancel
                    </button>
                  </div>
                </div>
              )}
            </div>
          )}

          {/* Settlement Section */}
          {inspection.status === MoveOutInspectionStatus.Completed && (
            <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
              <h2 className="text-lg font-semibold text-gray-900 mb-4">Settle Inspection</h2>
              {!settlementMode ? (
                <button
                  onClick={() => setSettlementMode(true)}
                  className="w-full bg-green-600 text-white px-6 py-3 rounded-lg hover:bg-green-700 transition font-medium flex items-center justify-center gap-2"
                >
                  <CheckCircle2 className="w-4 h-4" />
                  Settle & Finalize Deductions
                </button>
              ) : (
                <div className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Settlement Notes
                    </label>
                    <textarea
                      value={settlementNotes}
                      onChange={(e) => setSettlementNotes(e.target.value)}
                      rows={3}
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500"
                      placeholder="Any additional notes about the settlement..."
                    />
                  </div>
                  <div className="flex gap-3">
                    <button
                      onClick={handleSettleInspection}
                      disabled={saving}
                      className="flex-1 bg-green-600 text-white px-6 py-3 rounded-lg hover:bg-green-700 transition disabled:bg-gray-400 font-medium"
                    >
                      {saving ? 'Processing...' : 'Confirm Settlement'}
                    </button>
                    <button
                      onClick={() => setSettlementMode(false)}
                      className="px-6 py-3 border border-gray-300 rounded-lg hover:bg-gray-50 transition font-medium"
                    >
                      Cancel
                    </button>
                  </div>
                </div>
              )}
            </div>
          )}

          {/* Refund Section */}
          {inspection.status === MoveOutInspectionStatus.Settled &&
            !inspection.refundProcessed &&
            inspection.refundAmount > 0 && (
              <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                <h2 className="text-lg font-semibold text-gray-900 mb-4">Process Refund</h2>
                {!refundMode ? (
                  <button
                    onClick={() => setRefundMode(true)}
                    className="w-full bg-teal-600 text-white px-6 py-3 rounded-lg hover:bg-teal-700 transition font-medium flex items-center justify-center gap-2"
                  >
                    <Banknote className="w-4 h-4" />
                    Process Refund ({formatCurrency(inspection.refundAmount)})
                  </button>
                ) : (
                  <div className="space-y-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Refund Method
                      </label>
                      <select
                        value={refundForm.refundMethod}
                        onChange={(e) =>
                          setRefundForm({ ...refundForm, refundMethod: e.target.value })
                        }
                        className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-teal-500"
                      >
                        <option value="MPesa">M-Pesa</option>
                        <option value="BankTransfer">Bank Transfer</option>
                        <option value="Cash">Cash</option>
                      </select>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Transaction Reference
                      </label>
                      <input
                        type="text"
                        value={refundForm.refundReference}
                        onChange={(e) =>
                          setRefundForm({ ...refundForm, refundReference: e.target.value })
                        }
                        placeholder="e.g., M-Pesa transaction ID"
                        className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-teal-500"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Notes
                      </label>
                      <textarea
                        value={refundForm.notes}
                        onChange={(e) => setRefundForm({ ...refundForm, notes: e.target.value })}
                        rows={2}
                        className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-teal-500"
                      />
                    </div>
                    <div className="flex gap-3">
                      <button
                        onClick={handleProcessRefund}
                        disabled={saving}
                        className="flex-1 bg-teal-600 text-white px-6 py-3 rounded-lg hover:bg-teal-700 transition disabled:bg-gray-400 font-medium"
                      >
                        {saving ? 'Processing...' : 'Confirm Refund'}
                      </button>
                      <button
                        onClick={() => setRefundMode(false)}
                        className="px-6 py-3 border border-gray-300 rounded-lg hover:bg-gray-50 transition font-medium"
                      >
                        Cancel
                      </button>
                    </div>
                  </div>
                )}
              </div>
            )}

          {/* Photos Section */}
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-lg font-semibold text-gray-900 flex items-center gap-2">
                <Camera className="w-4 h-4" />
                Inspection Photos ({inspection.photos.length})
              </h2>
              {!photoUploadMode && (
                <button
                  onClick={() => setPhotoUploadMode(true)}
                  className="flex items-center gap-2 text-blue-600 hover:text-blue-800 font-medium"
                >
                  <Plus className="w-4 h-4" />
                  Upload Photo
                </button>
              )}
            </div>

            {photoUploadMode ? (
              <div>
                <div className="mb-4">
                  <button
                    onClick={() => {
                      setPhotoUploadMode(false)
                      setSelectedItemForPhoto(undefined)
                    }}
                    className="text-sm text-gray-600 hover:text-gray-900"
                  >
                    Cancel Upload
                  </button>
                </div>
                <PhotoUpload
                  inspectionId={inspection.id}
                  onUploadSuccess={handlePhotoUploadSuccess}
                  onUploadError={(err) => setError(err)}
                  inspectionItemId={selectedItemForPhoto}
                />
              </div>
            ) : (
              <PhotoGallery
                photos={inspection.photos}
                onDeletePhoto={handleDeletePhoto}
                canDelete={true}
              />
            )}
          </div>
        </div>

        {/* Right Column - Financial Summary */}
        <div className="space-y-6">
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 sticky top-6">
            <h2 className="text-lg font-semibold text-gray-900 mb-4">Financial Summary</h2>

            <div className="space-y-3">
              <div className="flex justify-between">
                <span className="text-gray-600">Security Deposit:</span>
                <span className="font-semibold">
                  {formatCurrency(inspection.securityDepositHeld)}
                </span>
              </div>

              {inspection.status >= MoveOutInspectionStatus.Completed && (
                <>
                  <div className="border-t border-gray-200 pt-3">
                    <div className="text-sm font-medium text-gray-700 mb-2">Deductions:</div>
                    {inspection.cleaningCharges > 0 && (
                      <div className="flex justify-between text-sm">
                        <span className="text-gray-600">Cleaning:</span>
                        <span className="text-red-600">
                          -{formatCurrency(inspection.cleaningCharges)}
                        </span>
                      </div>
                    )}
                    {inspection.repairCharges > 0 && (
                      <div className="flex justify-between text-sm">
                        <span className="text-gray-600">Repairs:</span>
                        <span className="text-red-600">
                          -{formatCurrency(inspection.repairCharges)}
                        </span>
                      </div>
                    )}
                    {inspection.unpaidRent > 0 && (
                      <div className="flex justify-between text-sm">
                        <span className="text-gray-600">Unpaid Rent:</span>
                        <span className="text-red-600">
                          -{formatCurrency(inspection.unpaidRent)}
                        </span>
                      </div>
                    )}
                    {inspection.unpaidUtilities > 0 && (
                      <div className="flex justify-between text-sm">
                        <span className="text-gray-600">Unpaid Utilities:</span>
                        <span className="text-red-600">
                          -{formatCurrency(inspection.unpaidUtilities)}
                        </span>
                      </div>
                    )}
                    {inspection.otherCharges > 0 && (
                      <div className="flex justify-between text-sm">
                        <span className="text-gray-600">Other:</span>
                        <span className="text-red-600">
                          -{formatCurrency(inspection.otherCharges)}
                        </span>
                      </div>
                    )}
                  </div>

                  <div className="border-t border-gray-200 pt-3">
                    <div className="flex justify-between font-semibold">
                      <span>Total Deductions:</span>
                      <span className="text-red-600">
                        -{formatCurrency(inspection.totalDeductions)}
                      </span>
                    </div>
                  </div>

                  <div className="border-t border-gray-200 pt-3">
                    <div className="flex justify-between text-lg font-bold">
                      <span>Refund Amount:</span>
                      <span className="text-green-600">
                        {formatCurrency(inspection.refundAmount)}
                      </span>
                    </div>
                  </div>

                  {inspection.tenantOwes > 0 && (
                    <div className="bg-red-50 border border-red-200 rounded-lg p-3">
                      <div className="flex justify-between font-semibold">
                        <span className="text-red-900">Tenant Owes:</span>
                        <span className="text-red-600">
                          {formatCurrency(inspection.tenantOwes)}
                        </span>
                      </div>
                    </div>
                  )}
                </>
              )}

              {inspection.refundProcessed && (
                <div className="bg-teal-50 border border-teal-200 rounded-lg p-3 mt-3">
                  <div className="text-sm font-medium text-teal-900 mb-1">Refund Processed</div>
                  <div className="text-sm text-teal-700">
                    <div>Method: {inspection.refundMethod}</div>
                    {inspection.refundReference && (
                      <div>Ref: {inspection.refundReference}</div>
                    )}
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  )

  return (
    <ProtectedRoute allowedRoles={['Landlord', 'Manager', 'Caretaker']}>
      {content}
    </ProtectedRoute>
  )
}
