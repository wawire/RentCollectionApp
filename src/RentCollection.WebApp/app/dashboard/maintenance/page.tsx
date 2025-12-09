'use client'

import { useState, useEffect } from 'react'
import { maintenanceRequestService } from '@/lib/services/maintenanceRequestService'
import {
  MaintenanceRequest,
  MaintenancePriority,
  MaintenanceRequestStatus,
  UpdateMaintenanceRequestDto,
  AssignMaintenanceRequestDto,
  CompleteMaintenanceRequestDto,
} from '@/lib/types/maintenanceRequest.types'

export default function MaintenanceRequestsDashboardPage() {
  const [requests, setRequests] = useState<MaintenanceRequest[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [filter, setFilter] = useState<MaintenanceRequestStatus | 'all'>('all')
  const [selectedRequest, setSelectedRequest] = useState<MaintenanceRequest | null>(null)
  const [showAssignModal, setShowAssignModal] = useState(false)
  const [showCompleteModal, setShowCompleteModal] = useState(false)
  const [assignCaretakerId, setAssignCaretakerId] = useState('')
  const [completionData, setCompletionData] = useState({ actualCost: '', notes: '' })

  useEffect(() => {
    fetchRequests()
  }, [])

  const fetchRequests = async () => {
    try {
      setLoading(true)
      const data = await maintenanceRequestService.getAll()
      setRequests(data)
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch maintenance requests')
    } finally {
      setLoading(false)
    }
  }

  const handleUpdateStatus = async (requestId: number, status: MaintenanceRequestStatus) => {
    try {
      const updateData: UpdateMaintenanceRequestDto = { status }
      await maintenanceRequestService.update(requestId, updateData)
      await fetchRequests()
    } catch (err: any) {
      alert(err.response?.data?.message || 'Failed to update status')
    }
  }

  const handleAssign = async () => {
    if (!selectedRequest || !assignCaretakerId) return

    try {
      const assignData: AssignMaintenanceRequestDto = {
        caretakerId: parseInt(assignCaretakerId),
      }
      await maintenanceRequestService.assign(selectedRequest.id, assignData)
      setShowAssignModal(false)
      setSelectedRequest(null)
      setAssignCaretakerId('')
      await fetchRequests()
    } catch (err: any) {
      alert(err.response?.data?.message || 'Failed to assign request')
    }
  }

  const handleComplete = async () => {
    if (!selectedRequest || !completionData.actualCost) return

    try {
      const completeData: CompleteMaintenanceRequestDto = {
        actualCost: parseFloat(completionData.actualCost),
        notes: completionData.notes || undefined,
      }
      await maintenanceRequestService.complete(selectedRequest.id, completeData)
      setShowCompleteModal(false)
      setSelectedRequest(null)
      setCompletionData({ actualCost: '', notes: '' })
      await fetchRequests()
    } catch (err: any) {
      alert(err.response?.data?.message || 'Failed to complete request')
    }
  }

  const handleDelete = async (requestId: number) => {
    if (!confirm('Are you sure you want to delete this maintenance request?')) return

    try {
      await maintenanceRequestService.delete(requestId)
      await fetchRequests()
    } catch (err: any) {
      alert(err.response?.data?.message || 'Failed to delete request')
    }
  }

  const getStatusColor = (status: MaintenanceRequestStatus) => {
    switch (status) {
      case MaintenanceRequestStatus.Pending:
        return 'bg-yellow-100 text-yellow-800'
      case MaintenanceRequestStatus.Assigned:
        return 'bg-blue-100 text-blue-800'
      case MaintenanceRequestStatus.InProgress:
        return 'bg-purple-100 text-purple-800'
      case MaintenanceRequestStatus.Completed:
        return 'bg-green-100 text-green-800'
      case MaintenanceRequestStatus.Cancelled:
        return 'bg-gray-100 text-gray-800'
      default:
        return 'bg-gray-100 text-gray-800'
    }
  }

  const getPriorityColor = (priority: MaintenancePriority) => {
    switch (priority) {
      case MaintenancePriority.Emergency:
        return 'bg-red-100 text-red-800'
      case MaintenancePriority.High:
        return 'bg-orange-100 text-orange-800'
      case MaintenancePriority.Medium:
        return 'bg-yellow-100 text-yellow-800'
      case MaintenancePriority.Low:
        return 'bg-green-100 text-green-800'
      default:
        return 'bg-gray-100 text-gray-800'
    }
  }

  const filteredRequests = filter === 'all' ? requests : requests.filter((r) => r.status === filter)

  const stats = {
    total: requests.length,
    pending: requests.filter((r) => r.status === MaintenanceRequestStatus.Pending).length,
    inProgress: requests.filter((r) => r.status === MaintenanceRequestStatus.InProgress).length,
    completed: requests.filter((r) => r.status === MaintenanceRequestStatus.Completed).length,
    overdue: requests.filter((r) => r.isOverdue).length,
  }

  return (
    <div className="max-w-7xl mx-auto">
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-gray-900">Maintenance Requests</h1>
        <p className="mt-2 text-gray-600">Manage and track all maintenance requests</p>
      </div>

      {error && (
        <div className="mb-6 bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
          {error}
        </div>
      )}

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-5 gap-4 mb-6">
        <div className="bg-white rounded-lg shadow p-4">
          <p className="text-sm text-gray-600">Total</p>
          <p className="text-2xl font-bold text-gray-900">{stats.total}</p>
        </div>
        <div className="bg-yellow-50 rounded-lg shadow p-4">
          <p className="text-sm text-yellow-600">Pending</p>
          <p className="text-2xl font-bold text-yellow-900">{stats.pending}</p>
        </div>
        <div className="bg-purple-50 rounded-lg shadow p-4">
          <p className="text-sm text-purple-600">In Progress</p>
          <p className="text-2xl font-bold text-purple-900">{stats.inProgress}</p>
        </div>
        <div className="bg-green-50 rounded-lg shadow p-4">
          <p className="text-sm text-green-600">Completed</p>
          <p className="text-2xl font-bold text-green-900">{stats.completed}</p>
        </div>
        <div className="bg-red-50 rounded-lg shadow p-4">
          <p className="text-sm text-red-600">Overdue</p>
          <p className="text-2xl font-bold text-red-900">{stats.overdue}</p>
        </div>
      </div>

      {/* Filters */}
      <div className="mb-6 bg-white shadow rounded-lg p-4">
        <div className="flex flex-wrap gap-2">
          <button
            onClick={() => setFilter('all')}
            className={`px-4 py-2 rounded-md ${
              filter === 'all' ? 'bg-blue-600 text-white' : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
            }`}
          >
            All
          </button>
          <button
            onClick={() => setFilter(MaintenanceRequestStatus.Pending)}
            className={`px-4 py-2 rounded-md ${
              filter === MaintenanceRequestStatus.Pending
                ? 'bg-blue-600 text-white'
                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
            }`}
          >
            Pending
          </button>
          <button
            onClick={() => setFilter(MaintenanceRequestStatus.Assigned)}
            className={`px-4 py-2 rounded-md ${
              filter === MaintenanceRequestStatus.Assigned
                ? 'bg-blue-600 text-white'
                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
            }`}
          >
            Assigned
          </button>
          <button
            onClick={() => setFilter(MaintenanceRequestStatus.InProgress)}
            className={`px-4 py-2 rounded-md ${
              filter === MaintenanceRequestStatus.InProgress
                ? 'bg-blue-600 text-white'
                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
            }`}
          >
            In Progress
          </button>
          <button
            onClick={() => setFilter(MaintenanceRequestStatus.Completed)}
            className={`px-4 py-2 rounded-md ${
              filter === MaintenanceRequestStatus.Completed
                ? 'bg-blue-600 text-white'
                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
            }`}
          >
            Completed
          </button>
        </div>
      </div>

      {/* Requests List */}
      {loading ? (
        <div className="text-center py-12">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">Loading requests...</p>
        </div>
      ) : filteredRequests.length === 0 ? (
        <div className="bg-white shadow rounded-lg p-12 text-center">
          <p className="text-gray-600">No maintenance requests found</p>
        </div>
      ) : (
        <div className="space-y-4">
          {filteredRequests.map((request) => (
            <div key={request.id} className="bg-white shadow rounded-lg p-6 hover:shadow-lg transition-shadow">
              <div className="flex justify-between items-start mb-4">
                <div className="flex-1">
                  <div className="flex items-center space-x-3 mb-2">
                    <h3 className="text-lg font-semibold text-gray-900">{request.title}</h3>
                    <span className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(request.status)}`}>
                      {request.statusName}
                    </span>
                    <span className={`px-2 py-1 rounded-full text-xs font-medium ${getPriorityColor(request.priority)}`}>
                      {request.priorityName}
                    </span>
                    {request.isOverdue && (
                      <span className="px-2 py-1 rounded-full text-xs font-medium bg-red-100 text-red-800">Overdue</span>
                    )}
                  </div>
                  <p className="text-gray-600 mb-3">{request.description}</p>
                  <div className="grid grid-cols-2 md:grid-cols-3 gap-4 text-sm">
                    <div>
                      <span className="text-gray-500">Tenant:</span>{' '}
                      <span className="font-medium">{request.tenantName}</span>
                    </div>
                    <div>
                      <span className="text-gray-500">Phone:</span>{' '}
                      <span className="font-medium">{request.tenantPhone}</span>
                    </div>
                    <div>
                      <span className="text-gray-500">Unit:</span>{' '}
                      <span className="font-medium">{request.unitNumber}</span>
                    </div>
                    <div>
                      <span className="text-gray-500">Property:</span>{' '}
                      <span className="font-medium">{request.propertyName}</span>
                    </div>
                    <div>
                      <span className="text-gray-500">Created:</span>{' '}
                      <span className="font-medium">
                        {new Date(request.createdAt).toLocaleDateString()} ({request.daysSinceCreated}d ago)
                      </span>
                    </div>
                    {request.assignedToName && (
                      <div>
                        <span className="text-gray-500">Assigned to:</span>{' '}
                        <span className="font-medium">{request.assignedToName}</span>
                      </div>
                    )}
                  </div>

                  {request.photoUrls.length > 0 && (
                    <div className="mt-4">
                      <p className="text-sm text-gray-500 mb-2">Photos:</p>
                      <div className="flex space-x-2">
                        {request.photoUrls.map((url, index) => (
                          <img
                            key={index}
                            src={url}
                            alt={`Photo ${index + 1}`}
                            className="h-20 w-20 object-cover rounded-md cursor-pointer hover:opacity-75"
                            onClick={() => window.open(url, '_blank')}
                          />
                        ))}
                      </div>
                    </div>
                  )}
                </div>
              </div>

              {/* Action Buttons */}
              <div className="flex flex-wrap gap-2 mt-4">
                {request.status === MaintenanceRequestStatus.Pending && (
                  <>
                    <button
                      onClick={() => {
                        setSelectedRequest(request)
                        setShowAssignModal(true)
                      }}
                      className="px-3 py-1 bg-blue-600 text-white rounded-md hover:bg-blue-700 text-sm"
                    >
                      Assign
                    </button>
                    <button
                      onClick={() => handleUpdateStatus(request.id, MaintenanceRequestStatus.InProgress)}
                      className="px-3 py-1 bg-purple-600 text-white rounded-md hover:bg-purple-700 text-sm"
                    >
                      Start Work
                    </button>
                  </>
                )}
                {request.status === MaintenanceRequestStatus.Assigned && (
                  <button
                    onClick={() => handleUpdateStatus(request.id, MaintenanceRequestStatus.InProgress)}
                    className="px-3 py-1 bg-purple-600 text-white rounded-md hover:bg-purple-700 text-sm"
                  >
                    Start Work
                  </button>
                )}
                {(request.status === MaintenanceRequestStatus.InProgress ||
                  request.status === MaintenanceRequestStatus.Assigned) && (
                  <button
                    onClick={() => {
                      setSelectedRequest(request)
                      setShowCompleteModal(true)
                    }}
                    className="px-3 py-1 bg-green-600 text-white rounded-md hover:bg-green-700 text-sm"
                  >
                    Mark Complete
                  </button>
                )}
                <button
                  onClick={() => handleDelete(request.id)}
                  className="px-3 py-1 bg-red-600 text-white rounded-md hover:bg-red-700 text-sm"
                >
                  Delete
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Assign Modal */}
      {showAssignModal && selectedRequest && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 max-w-md w-full">
            <h3 className="text-lg font-semibold mb-4">Assign Maintenance Request</h3>
            <p className="text-gray-600 mb-4">Assign "{selectedRequest.title}" to a caretaker</p>
            <input
              type="number"
              placeholder="Caretaker User ID"
              value={assignCaretakerId}
              onChange={(e) => setAssignCaretakerId(e.target.value)}
              className="w-full px-3 py-2 border rounded-md mb-4"
            />
            <div className="flex justify-end space-x-2">
              <button
                onClick={() => {
                  setShowAssignModal(false)
                  setSelectedRequest(null)
                  setAssignCaretakerId('')
                }}
                className="px-4 py-2 bg-gray-200 rounded-md hover:bg-gray-300"
              >
                Cancel
              </button>
              <button
                onClick={handleAssign}
                disabled={!assignCaretakerId}
                className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:bg-gray-400"
              >
                Assign
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Complete Modal */}
      {showCompleteModal && selectedRequest && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 max-w-md w-full">
            <h3 className="text-lg font-semibold mb-4">Complete Maintenance Request</h3>
            <p className="text-gray-600 mb-4">Complete "{selectedRequest.title}"</p>
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Actual Cost (KES) *</label>
                <input
                  type="number"
                  step="0.01"
                  placeholder="0.00"
                  value={completionData.actualCost}
                  onChange={(e) => setCompletionData({ ...completionData, actualCost: e.target.value })}
                  className="w-full px-3 py-2 border rounded-md"
                  required
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Notes (Optional)</label>
                <textarea
                  placeholder="Completion notes..."
                  value={completionData.notes}
                  onChange={(e) => setCompletionData({ ...completionData, notes: e.target.value })}
                  className="w-full px-3 py-2 border rounded-md"
                  rows={3}
                />
              </div>
            </div>
            <div className="flex justify-end space-x-2 mt-4">
              <button
                onClick={() => {
                  setShowCompleteModal(false)
                  setSelectedRequest(null)
                  setCompletionData({ actualCost: '', notes: '' })
                }}
                className="px-4 py-2 bg-gray-200 rounded-md hover:bg-gray-300"
              >
                Cancel
              </button>
              <button
                onClick={handleComplete}
                disabled={!completionData.actualCost}
                className="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 disabled:bg-gray-400"
              >
                Complete
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
