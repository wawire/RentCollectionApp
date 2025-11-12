import { apiClient } from './api'
import { Tenant, CreateTenantDto } from '../types/tenant.types'

export const tenantService = {
  async getAll(): Promise<Tenant[]> {
    const response = await apiClient.get('/tenants')
    return response.data.data
  },

  async getById(id: number): Promise<Tenant> {
    const response = await apiClient.get(`/tenants/${id}`)
    return response.data.data
  },

  async getActive(): Promise<Tenant[]> {
    const response = await apiClient.get('/tenants/active')
    return response.data.data
  },

  async create(data: CreateTenantDto): Promise<Tenant> {
    const response = await apiClient.post('/tenants', data)
    return response.data.data
  },

  async update(id: number, data: CreateTenantDto): Promise<Tenant> {
    const response = await apiClient.put(`/tenants/${id}`, data)
    return response.data.data
  },

  async delete(id: number): Promise<void> {
    await apiClient.delete(`/tenants/${id}`)
  },

  async deactivate(id: number): Promise<void> {
    await apiClient.post(`/tenants/${id}/deactivate`)
  },

  async activate(id: number): Promise<void> {
    await apiClient.post(`/tenants/${id}/activate`)
  },
}
