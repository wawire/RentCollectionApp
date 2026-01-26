'use client'

import { useState, useEffect } from 'react'
import Link from 'next/link'
import { moveOutInspectionService } from '@/lib/services/moveOutInspectionService'
import type { MoveOutInspection } from '@/lib/types'
import { MoveOutInspectionStatus } from '@/lib/types'
import ProtectedRoute from '@/components/auth/ProtectedRoute'
import {
  Calendar,
  CheckCircle2,
  ClipboardCheck,
  Clock,
  Eye,
  Filter,
  Plus,
  Search,
} from 'lucide-react'

export default function MoveOutInspectionsPage() {
  const [inspections, setInspections] = useState<MoveOutInspection[]>([])
  const [filteredInspections, setFilteredInspections] = useState<MoveOutInspection[]>([])
  const [loading, setLoading] = useState(true)
  const [searchQuery, setSearchQuery] = useState('')
  const [statusFilter, setStatusFilter] = useState<MoveOutInspectionStatus | 'all'>('all')

  useEffect(() => {
    loadPendingInspections()
  }, [])

  useEffect(() => {
    filterInspections()
  }, [inspections, searchQuery, statusFilter])

  const loadPendingInspections = async () => {
    try {
      setLoading(true)
      const data = await moveOutInspectionService.getPendingInspections()
      setInspections(data)
    } catch (error) {
      console.error('Failed to load inspections:', error)
    } finally {
      setLoading(false)
    }
  }

  const loadInspectionsByStatus = async (status: MoveOutInspectionStatus) => {
    try {
      setLoading(true)
      const data = await moveOutInspectionService.getInspectionsByStatus(status)
      setInspections(data)
    } catch (error) {
      console.error('Failed to load inspections:', error)
    } finally {
      setLoading(false)
    }
  }

  const filterInspections = () => {
    let filtered = inspections

    if (searchQuery) {
      const query = searchQuery.toLowerCase()
      filtered = filtered.filter(
        (inspection) =>
          inspection.tenantName.toLowerCase().includes(query) ||
          inspection.unitNumber.toLowerCase().includes(query) ||
          inspection.propertyName.toLowerCase().includes(query)
      )
    }

    if (statusFilter !== 'all') {
      filtered = filtered.filter((inspection) => inspection.status === statusFilter)
    }

    setFilteredInspections(filtered)
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

  const getStatusIcon = (status: MoveOutInspectionStatus) => {
    if (status === MoveOutInspectionStatus.Scheduled) return <Calendar className="inline mr-1 w-3 h-3" />
    if (status === MoveOutInspectionStatus.InProgress) return <Clock className="inline mr-1 w-3 h-3" />
    if (status >= MoveOutInspectionStatus.Settled) return <CheckCircle2 className="inline mr-1 w-3 h-3" />
    return null
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
      month: 'short',
      day: 'numeric',
    })
  }

  const content = loading ? (
    <div className="p-6">
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    </div>
  ) : (
    <div className="p-6 max-w-7xl mx-auto">
      {/* Header */}
      <div className="mb-6">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold text-gray-900 flex items-center gap-2">
              <ClipboardCheck className="text-blue-600 w-6 h-6" />
              Move-Out Inspections
            </h1>
            <p className="text-gray-600 mt-1">
              Manage tenant move-out inspections and security deposit settlements
            </p>
          </div>
          <Link href="/dashboard/move-out-inspections/new">
            <button className="flex items-center gap-2 bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition">
              <Plus className="w-4 h-4" />
              Schedule Inspection
            </button>
          </Link>
        </div>
      </div>

      {/* Filters */}
      <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-4 mb-6">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {/* Search */}
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-4 h-4" />
            <input
              type="text"
              placeholder="Search by tenant, unit, or property..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
          </div>

          {/* Status Filter */}
          <div className="relative">
            <Filter className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-4 h-4" />
            <select
              value={statusFilter}
              onChange={(e) => {
                const value = e.target.value
                if (value === 'all') {
                  setStatusFilter('all')
                  loadPendingInspections()
                } else {
                  const status = parseInt(value) as MoveOutInspectionStatus
                  setStatusFilter(status)
                  loadInspectionsByStatus(status)
                }
              }}
              className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent appearance-none"
            >
              <option value="all">All Statuses</option>
              <option value={MoveOutInspectionStatus.Scheduled}>Scheduled</option>
              <option value={MoveOutInspectionStatus.InProgress}>In Progress</option>
              <option value={MoveOutInspectionStatus.Completed}>Completed</option>
              <option value={MoveOutInspectionStatus.Reviewed}>Reviewed</option>
              <option value={MoveOutInspectionStatus.Settled}>Settled</option>
              <option value={MoveOutInspectionStatus.RefundProcessed}>Refund Processed</option>
            </select>
          </div>
        </div>
      </div>

      {/* Inspections List */}
      {filteredInspections.length === 0 ? (
        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-12 text-center">
          <ClipboardCheck className="mx-auto w-12 h-12 text-gray-300 mb-4" />
          <h3 className="text-xl font-semibold text-gray-700 mb-2">No Inspections Found</h3>
          <p className="text-gray-500 mb-6">
            {searchQuery || statusFilter !== 'all'
              ? 'No inspections match your search criteria.'
              : 'Schedule your first move-out inspection to get started.'}
          </p>
          <Link href="/dashboard/move-out-inspections/new">
            <button className="bg-blue-600 text-white px-6 py-2 rounded-lg hover:bg-blue-700 transition">
              Schedule Inspection
            </button>
          </Link>
        </div>
      ) : (
        <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead className="bg-gray-50 border-b border-gray-200">
                <tr>
                  <th className="text-left px-6 py-3 text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Tenant
                  </th>
                  <th className="text-left px-6 py-3 text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Unit
                  </th>
                  <th className="text-left px-6 py-3 text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Inspection Date
                  </th>
                  <th className="text-left px-6 py-3 text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Status
                  </th>
                  <th className="text-right px-6 py-3 text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Deposit Held
                  </th>
                  <th className="text-right px-6 py-3 text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Deductions
                  </th>
                  <th className="text-right px-6 py-3 text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Refund Amount
                  </th>
                  <th className="text-center px-6 py-3 text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200">
                {filteredInspections.map((inspection) => (
                  <tr key={inspection.id} className="hover:bg-gray-50 transition">
                    <td className="px-6 py-4">
                      <div className="font-medium text-gray-900">{inspection.tenantName}</div>
                      <div className="text-sm text-gray-500">{inspection.propertyName}</div>
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-900">{inspection.unitNumber}</td>
                    <td className="px-6 py-4 text-sm text-gray-900">
                      {formatDate(inspection.inspectionDate)}
                    </td>
                    <td className="px-6 py-4">
                      <span
                        className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getStatusBadge(
                          inspection.status
                        )}`}
                      >
                        {getStatusIcon(inspection.status)}
                        {inspection.statusDisplay}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-right text-sm font-medium text-gray-900">
                      {formatCurrency(inspection.securityDepositHeld)}
                    </td>
                    <td className="px-6 py-4 text-right text-sm text-red-600 font-medium">
                      {formatCurrency(inspection.totalDeductions)}
                    </td>
                    <td className="px-6 py-4 text-right text-sm text-green-600 font-medium">
                      {formatCurrency(inspection.refundAmount)}
                    </td>
                    <td className="px-6 py-4 text-center">
                      <Link href={`/dashboard/move-out-inspections/${inspection.id}`}>
                        <button className="text-blue-600 hover:text-blue-800 transition">
                          <Eye className="inline mr-1 w-4 h-4" />
                          View
                        </button>
                      </Link>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Summary Stats */}
      {filteredInspections.length > 0 && (
        <div className="mt-6 grid grid-cols-1 md:grid-cols-3 gap-4">
          <div className="bg-blue-50 rounded-lg p-4 border border-blue-200">
            <div className="text-sm text-blue-600 font-medium mb-1">Total Deposit Held</div>
            <div className="text-2xl font-bold text-blue-900">
              {formatCurrency(
                filteredInspections.reduce((sum, i) => sum + i.securityDepositHeld, 0)
              )}
            </div>
          </div>
          <div className="bg-red-50 rounded-lg p-4 border border-red-200">
            <div className="text-sm text-red-600 font-medium mb-1">Total Deductions</div>
            <div className="text-2xl font-bold text-red-900">
              {formatCurrency(
                filteredInspections.reduce((sum, i) => sum + i.totalDeductions, 0)
              )}
            </div>
          </div>
          <div className="bg-green-50 rounded-lg p-4 border border-green-200">
            <div className="text-sm text-green-600 font-medium mb-1">Total Refunds</div>
            <div className="text-2xl font-bold text-green-900">
              {formatCurrency(
                filteredInspections.reduce((sum, i) => sum + i.refundAmount, 0)
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  )

  return (
    <ProtectedRoute allowedRoles={['Landlord', 'Manager', 'Caretaker']}>
      {content}
    </ProtectedRoute>
  )
}
