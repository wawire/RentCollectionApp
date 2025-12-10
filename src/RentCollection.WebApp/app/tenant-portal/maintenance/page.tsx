'use client'

import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import Link from 'next/link'
import { maintenanceRequestService } from '@/lib/services/maintenanceRequestService'
import { MaintenanceRequest, MaintenancePriority, MaintenanceRequestStatus } from '@/lib/types/maintenanceRequest.types'

export default function TenantMaintenanceRequestsPage() {
  const router = useRouter()
  const [requests, setRequests] = useState<MaintenanceRequest[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [filter, setFilter] = useState<MaintenanceRequestStatus | 'all'>('all')

  useEffect(() => {
    fetchRequests()
  }, [])

  const fetchRequests = async () => {
    try {
      setLoading(true)
      const data = await maintenanceRequestService.getMyRequests()
      setRequests(data)
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch maintenance requests')
    } finally {
      setLoading(false)
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

  const filteredRequests = filter === 'all'
    ? requests
    : requests.filter((r) => r.status === filter)

  return (
    <div className="max-w-7xl mx-auto">
      <div className="mb-6 flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">My Maintenance Requests</h1>
          <p className="mt-2 text-gray-600">Track and manage your maintenance requests</p>
        </div>
        <Link
          href="/tenant-portal/maintenance/new"
          className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
        >
          New Request
        </Link>
      </div>

      {error && (
        <div className="mb-6 bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
          {error}
        </div>
      )}

      <div className="mb-6 bg-white shadow rounded-lg p-4">
        <div className="flex space-x-2">
          <button
            onClick={() => setFilter('all')}
            className={`px-4 py-2 rounded-md ${
              filter === 'all'
                ? 'bg-blue-600 text-white'
                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
            }`}
          >
            All ({requests.length})
          </button>
          <button
            onClick={() => setFilter(MaintenanceRequestStatus.Pending)}
            className={`px-4 py-2 rounded-md ${
              filter === MaintenanceRequestStatus.Pending
                ? 'bg-blue-600 text-white'
                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
            }`}
          >
            Pending ({requests.filter((r) => r.status === MaintenanceRequestStatus.Pending).length})
          </button>
          <button
            onClick={() => setFilter(MaintenanceRequestStatus.InProgress)}
            className={`px-4 py-2 rounded-md ${
              filter === MaintenanceRequestStatus.InProgress
                ? 'bg-blue-600 text-white'
                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
            }`}
          >
            In Progress ({requests.filter((r) => r.status === MaintenanceRequestStatus.InProgress).length})
          </button>
          <button
            onClick={() => setFilter(MaintenanceRequestStatus.Completed)}
            className={`px-4 py-2 rounded-md ${
              filter === MaintenanceRequestStatus.Completed
                ? 'bg-blue-600 text-white'
                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
            }`}
          >
            Completed ({requests.filter((r) => r.status === MaintenanceRequestStatus.Completed).length})
          </button>
        </div>
      </div>

      {loading ? (
        <div className="text-center py-12">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">Loading requests...</p>
        </div>
      ) : filteredRequests.length === 0 ? (
        <div className="bg-white shadow rounded-lg p-12 text-center">
          <p className="text-gray-600">No maintenance requests found</p>
          <Link
            href="/tenant-portal/maintenance/new"
            className="mt-4 inline-block px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
          >
            Create Your First Request
          </Link>
        </div>
      ) : (
        <div className="space-y-4">
          {filteredRequests.map((request) => (
            <div
              key={request.id}
              className="bg-white shadow rounded-lg p-6 hover:shadow-lg transition-shadow"
            >
              <div className="flex justify-between items-start">
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
                      <span className="px-2 py-1 rounded-full text-xs font-medium bg-red-100 text-red-800">
                        Overdue
                      </span>
                    )}
                  </div>
                  <p className="text-gray-600 mb-3">{request.description}</p>
                  <div className="grid grid-cols-2 gap-4 text-sm">
                    <div>
                      <span className="text-gray-500">Unit:</span>{' '}
                      <span className="font-medium">{request.unitNumber}</span>
                    </div>
                    <div>
                      <span className="text-gray-500">Created:</span>{' '}
                      <span className="font-medium">
                        {new Date(request.createdAt).toLocaleDateString()} ({request.daysSinceCreated} days ago)
                      </span>
                    </div>
                    {request.assignedToName && (
                      <div>
                        <span className="text-gray-500">Assigned to:</span>{' '}
                        <span className="font-medium">{request.assignedToName}</span>
                      </div>
                    )}
                    {request.estimatedCost && (
                      <div>
                        <span className="text-gray-500">Estimated Cost:</span>{' '}
                        <span className="font-medium">KES {request.estimatedCost.toLocaleString()}</span>
                      </div>
                    )}
                    {request.actualCost && (
                      <div>
                        <span className="text-gray-500">Actual Cost:</span>{' '}
                        <span className="font-medium">KES {request.actualCost.toLocaleString()}</span>
                      </div>
                    )}
                    {request.completedAt && (
                      <div>
                        <span className="text-gray-500">Completed:</span>{' '}
                        <span className="font-medium">{new Date(request.completedAt).toLocaleDateString()}</span>
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
                  {request.notes && (
                    <div className="mt-3 p-3 bg-gray-50 rounded-md">
                      <p className="text-sm text-gray-500 mb-1">Notes:</p>
                      <p className="text-sm text-gray-700">{request.notes}</p>
                    </div>
                  )}
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
