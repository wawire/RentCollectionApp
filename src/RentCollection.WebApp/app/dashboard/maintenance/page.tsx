'use client'

import { useState, useEffect } from 'react'
import { maintenanceRequestService } from '@/lib/services/maintenanceRequestService'
import ProtectedRoute from '@/components/auth/ProtectedRoute'
import {
  MaintenanceRequest,
  MaintenancePriority,
  MaintenanceRequestStatus,
  UpdateMaintenanceRequestDto,
  AssignMaintenanceRequestDto,
  CompleteMaintenanceRequestDto,
} from '@/lib/types/maintenanceRequest.types'
import {
  Alert,
  Badge,
  Button,
  Card,
  EmptyState,
  Input,
  Modal,
  PageHeader,
  SkeletonCard,
  TextArea,
} from '@/components/common'

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
      setError('')
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

  const statusVariant = (status: MaintenanceRequestStatus) => {
    switch (status) {
      case MaintenanceRequestStatus.Pending:
        return 'warning'
      case MaintenanceRequestStatus.Assigned:
        return 'info'
      case MaintenanceRequestStatus.InProgress:
        return 'default'
      case MaintenanceRequestStatus.Completed:
        return 'success'
      case MaintenanceRequestStatus.Cancelled:
        return 'default'
      default:
        return 'default'
    }
  }

  const priorityVariant = (priority: MaintenancePriority) => {
    switch (priority) {
      case MaintenancePriority.Emergency:
        return 'danger'
      case MaintenancePriority.High:
        return 'warning'
      case MaintenancePriority.Medium:
        return 'default'
      case MaintenancePriority.Low:
        return 'success'
      default:
        return 'default'
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
    <ProtectedRoute allowedRoles={['PlatformAdmin', 'Landlord', 'Manager', 'Caretaker']}>
      <div className="max-w-7xl mx-auto space-y-6">
        <PageHeader
          title="Maintenance"
          subtitle="Manage and track all maintenance requests."
          breadcrumbs={[
            { label: 'Dashboard', href: '/dashboard' },
            { label: 'Maintenance' },
          ]}
        />

        {error && (
          <Alert type="error" message={error} />
        )}

        <div className="grid grid-cols-1 md:grid-cols-5 gap-4">
          <Card padding="md">
            <p className="text-sm text-text-muted">Total</p>
            <p className="text-2xl font-semibold text-text-primary">{stats.total}</p>
          </Card>
          <Card padding="md" className="bg-state-warning/10 border-state-warning/30">
            <p className="text-sm text-state-warning">Pending</p>
            <p className="text-2xl font-semibold text-text-primary">{stats.pending}</p>
          </Card>
          <Card padding="md" className="bg-brand-bg/60">
            <p className="text-sm text-text-secondary">In Progress</p>
            <p className="text-2xl font-semibold text-text-primary">{stats.inProgress}</p>
          </Card>
          <Card padding="md" className="bg-state-success/10 border-state-success/30">
            <p className="text-sm text-state-success">Completed</p>
            <p className="text-2xl font-semibold text-text-primary">{stats.completed}</p>
          </Card>
          <Card padding="md" className="bg-state-error/10 border-state-error/30">
            <p className="text-sm text-state-error">Overdue</p>
            <p className="text-2xl font-semibold text-text-primary">{stats.overdue}</p>
          </Card>
        </div>

        <Card padding="md">
          <div className="flex flex-wrap gap-2">
            <Button
              variant={filter === 'all' ? 'primary' : 'secondary'}
              size="sm"
              onClick={() => setFilter('all')}
            >
              All
            </Button>
            <Button
              variant={filter === MaintenanceRequestStatus.Pending ? 'primary' : 'secondary'}
              size="sm"
              onClick={() => setFilter(MaintenanceRequestStatus.Pending)}
            >
              Pending
            </Button>
            <Button
              variant={filter === MaintenanceRequestStatus.Assigned ? 'primary' : 'secondary'}
              size="sm"
              onClick={() => setFilter(MaintenanceRequestStatus.Assigned)}
            >
              Assigned
            </Button>
            <Button
              variant={filter === MaintenanceRequestStatus.InProgress ? 'primary' : 'secondary'}
              size="sm"
              onClick={() => setFilter(MaintenanceRequestStatus.InProgress)}
            >
              In Progress
            </Button>
            <Button
              variant={filter === MaintenanceRequestStatus.Completed ? 'primary' : 'secondary'}
              size="sm"
              onClick={() => setFilter(MaintenanceRequestStatus.Completed)}
            >
              Completed
            </Button>
          </div>
        </Card>

        {loading ? (
          <div className="space-y-4">
            <SkeletonCard />
            <SkeletonCard />
            <SkeletonCard />
          </div>
        ) : filteredRequests.length === 0 ? (
          <Card padding="md">
            <EmptyState title="No maintenance requests found" />
          </Card>
        ) : (
          <div className="space-y-4">
            {filteredRequests.map((request) => (
              <Card key={request.id} padding="md" hover>
                <div className="flex justify-between items-start mb-4">
                  <div className="flex-1">
                    <div className="flex items-center flex-wrap gap-2 mb-2">
                      <h3 className="text-lg font-semibold text-text-primary">{request.title}</h3>
                      <Badge variant={statusVariant(request.status)}>{request.statusName}</Badge>
                      <Badge variant={priorityVariant(request.priority)}>{request.priorityName}</Badge>
                      {request.isOverdue && (
                        <Badge variant="danger">Overdue</Badge>
                      )}
                    </div>
                    <p className="text-text-secondary mb-3">{request.description}</p>
                    <div className="grid grid-cols-2 md:grid-cols-3 gap-4 text-sm">
                      <div>
                        <span className="text-text-muted">Tenant:</span>{' '}
                        <span className="font-medium text-text-primary">{request.tenantName}</span>
                      </div>
                      <div>
                        <span className="text-text-muted">Phone:</span>{' '}
                        <span className="font-medium text-text-primary">{request.tenantPhone}</span>
                      </div>
                      <div>
                        <span className="text-text-muted">Unit:</span>{' '}
                        <span className="font-medium text-text-primary">{request.unitNumber}</span>
                      </div>
                      <div>
                        <span className="text-text-muted">Property:</span>{' '}
                        <span className="font-medium text-text-primary">{request.propertyName}</span>
                      </div>
                      <div>
                        <span className="text-text-muted">Created:</span>{' '}
                        <span className="font-medium text-text-primary">
                          {new Date(request.createdAt).toLocaleDateString()} ({request.daysSinceCreated}d ago)
                        </span>
                      </div>
                      {request.assignedToName && (
                        <div>
                          <span className="text-text-muted">Assigned to:</span>{' '}
                          <span className="font-medium text-text-primary">{request.assignedToName}</span>
                        </div>
                      )}
                    </div>

                    {request.photoUrls.length > 0 && (
                      <div className="mt-4">
                        <p className="text-sm text-text-muted mb-2">Photos:</p>
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

                <div className="flex flex-wrap gap-2 mt-4">
                  {request.status === MaintenanceRequestStatus.Pending && (
                    <>
                      <Button
                        size="sm"
                        onClick={() => {
                          setSelectedRequest(request)
                          setShowAssignModal(true)
                        }}
                      >
                        Assign
                      </Button>
                      <Button
                        size="sm"
                        variant="secondary"
                        onClick={() => handleUpdateStatus(request.id, MaintenanceRequestStatus.InProgress)}
                      >
                        Start Work
                      </Button>
                    </>
                  )}
                  {request.status === MaintenanceRequestStatus.Assigned && (
                    <Button
                      size="sm"
                      variant="secondary"
                      onClick={() => handleUpdateStatus(request.id, MaintenanceRequestStatus.InProgress)}
                    >
                      Start Work
                    </Button>
                  )}
                  {(request.status === MaintenanceRequestStatus.InProgress ||
                    request.status === MaintenanceRequestStatus.Assigned) && (
                    <Button
                      size="sm"
                      variant="secondary"
                      onClick={() => {
                        setSelectedRequest(request)
                        setShowCompleteModal(true)
                      }}
                    >
                      Mark Complete
                    </Button>
                  )}
                  <Button
                    size="sm"
                    variant="destructive"
                    onClick={() => handleDelete(request.id)}
                  >
                    Delete
                  </Button>
                </div>
              </Card>
            ))}
          </div>
        )}

        <Modal
          isOpen={showAssignModal && !!selectedRequest}
          onClose={() => {
            setShowAssignModal(false)
            setSelectedRequest(null)
            setAssignCaretakerId('')
          }}
          title="Assign Maintenance Request"
          footer={
            <>
              <Button variant="secondary" onClick={() => {
                setShowAssignModal(false)
                setSelectedRequest(null)
                setAssignCaretakerId('')
              }}>
                Cancel
              </Button>
              <Button onClick={handleAssign} disabled={!assignCaretakerId}>
                Assign
              </Button>
            </>
          }
        >
          <p className="text-text-secondary mb-4">
            Assign "{selectedRequest?.title}" to a caretaker.
          </p>
          <Input
            type="number"
            label="Caretaker User ID"
            value={assignCaretakerId}
            onChange={(e) => setAssignCaretakerId(e.target.value)}
            placeholder="Caretaker User ID"
            fullWidth
          />
        </Modal>

        <Modal
          isOpen={showCompleteModal && !!selectedRequest}
          onClose={() => {
            setShowCompleteModal(false)
            setSelectedRequest(null)
            setCompletionData({ actualCost: '', notes: '' })
          }}
          title="Complete Maintenance Request"
          footer={
            <>
              <Button variant="secondary" onClick={() => {
                setShowCompleteModal(false)
                setSelectedRequest(null)
                setCompletionData({ actualCost: '', notes: '' })
              }}>
                Cancel
              </Button>
              <Button onClick={handleComplete} disabled={!completionData.actualCost}>
                Complete
              </Button>
            </>
          }
        >
          <p className="text-text-secondary mb-4">
            Complete "{selectedRequest?.title}".
          </p>
          <div className="space-y-4">
            <Input
              type="number"
              step="0.01"
              label="Actual Cost (KES)"
              value={completionData.actualCost}
              onChange={(e) => setCompletionData({ ...completionData, actualCost: e.target.value })}
              placeholder="0.00"
              required
              fullWidth
            />
            <TextArea
              label="Notes (Optional)"
              value={completionData.notes}
              onChange={(e) => setCompletionData({ ...completionData, notes: e.target.value })}
              rows={3}
              placeholder="Completion notes..."
            />
          </div>
        </Modal>
      </div>
    </ProtectedRoute>
  )
}

