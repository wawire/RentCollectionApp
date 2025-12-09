import { apiClient } from './api'
import {
  MaintenanceRequest,
  CreateMaintenanceRequestDto,
  UpdateMaintenanceRequestDto,
  AssignMaintenanceRequestDto,
  CompleteMaintenanceRequestDto,
  MaintenanceRequestStatus,
} from '../types/maintenanceRequest.types'

export const maintenanceRequestService = {
  async getAll(): Promise<MaintenanceRequest[]> {
    const response = await apiClient.get('/maintenancerequests')
    return response.data.data
  },

  async getById(id: number): Promise<MaintenanceRequest> {
    const response = await apiClient.get(`/maintenancerequests/${id}`)
    return response.data.data
  },

  async getMyRequests(): Promise<MaintenanceRequest[]> {
    const response = await apiClient.get('/maintenancerequests/my-requests')
    return response.data.data
  },

  async getByProperty(propertyId: number): Promise<MaintenanceRequest[]> {
    const response = await apiClient.get(`/maintenancerequests/property/${propertyId}`)
    return response.data.data
  },

  async getByStatus(status: MaintenanceRequestStatus): Promise<MaintenanceRequest[]> {
    const response = await apiClient.get(`/maintenancerequests/status/${status}`)
    return response.data.data
  },

  async getAssignedToMe(): Promise<MaintenanceRequest[]> {
    const response = await apiClient.get('/maintenancerequests/assigned-to-me')
    return response.data.data
  },

  async create(data: CreateMaintenanceRequestDto): Promise<MaintenanceRequest> {
    const formData = new FormData()
    formData.append('title', data.title)
    formData.append('description', data.description)
    formData.append('priority', data.priority.toString())

    if (data.photos && data.photos.length > 0) {
      data.photos.forEach((photo) => {
        formData.append('photos', photo)
      })
    }

    const response = await apiClient.post('/maintenancerequests', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    })
    return response.data.data
  },

  async update(id: number, data: UpdateMaintenanceRequestDto): Promise<MaintenanceRequest> {
    const response = await apiClient.put(`/maintenancerequests/${id}`, data)
    return response.data.data
  },

  async assign(id: number, data: AssignMaintenanceRequestDto): Promise<MaintenanceRequest> {
    const response = await apiClient.post(`/maintenancerequests/${id}/assign`, data)
    return response.data.data
  },

  async complete(id: number, data: CompleteMaintenanceRequestDto): Promise<MaintenanceRequest> {
    const response = await apiClient.post(`/maintenancerequests/${id}/complete`, data)
    return response.data.data
  },

  async delete(id: number): Promise<void> {
    await apiClient.delete(`/maintenancerequests/${id}`)
  },
}
