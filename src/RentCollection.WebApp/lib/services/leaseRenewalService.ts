import { apiClient } from './api'
import {
  LeaseRenewal,
  CreateLeaseRenewalDto,
  UpdateLeaseRenewalDto,
  TenantResponseDto,
  RejectLeaseRenewalDto,
  LeaseRenewalStatus,
} from '../types/leaseRenewal.types'

export const leaseRenewalService = {
  async getAll(): Promise<LeaseRenewal[]> {
    const response = await apiClient.get('/leaserenewals')
    return response.data.data
  },

  async getById(id: number): Promise<LeaseRenewal> {
    const response = await apiClient.get(`/leaserenewals/${id}`)
    return response.data.data
  },

  async getMyRenewals(): Promise<LeaseRenewal[]> {
    const response = await apiClient.get('/leaserenewals/my-renewals')
    return response.data.data
  },

  async getByProperty(propertyId: number): Promise<LeaseRenewal[]> {
    const response = await apiClient.get(`/leaserenewals/property/${propertyId}`)
    return response.data.data
  },

  async getByStatus(status: LeaseRenewalStatus): Promise<LeaseRenewal[]> {
    const response = await apiClient.get(`/leaserenewals/status/${status}`)
    return response.data.data
  },

  async getExpiringSoon(daysUntilExpiry: number = 60): Promise<LeaseRenewal[]> {
    const response = await apiClient.get(`/leaserenewals/expiring-soon?daysUntilExpiry=${daysUntilExpiry}`)
    return response.data.data
  },

  async create(data: CreateLeaseRenewalDto): Promise<LeaseRenewal> {
    const response = await apiClient.post('/leaserenewals', data)
    return response.data.data
  },

  async update(id: number, data: UpdateLeaseRenewalDto): Promise<LeaseRenewal> {
    const response = await apiClient.put(`/leaserenewals/${id}`, data)
    return response.data.data
  },

  async tenantRespond(id: number, data: TenantResponseDto): Promise<LeaseRenewal> {
    const response = await apiClient.post(`/leaserenewals/${id}/respond`, data)
    return response.data.data
  },

  async landlordApprove(id: number): Promise<LeaseRenewal> {
    const response = await apiClient.post(`/leaserenewals/${id}/approve`)
    return response.data.data
  },

  async landlordReject(id: number, data: RejectLeaseRenewalDto): Promise<LeaseRenewal> {
    const response = await apiClient.post(`/leaserenewals/${id}/reject`, data)
    return response.data.data
  },

  async complete(id: number): Promise<LeaseRenewal> {
    const response = await apiClient.post(`/leaserenewals/${id}/complete`)
    return response.data.data
  },

  async delete(id: number): Promise<void> {
    await apiClient.delete(`/leaserenewals/${id}`)
  },
}
