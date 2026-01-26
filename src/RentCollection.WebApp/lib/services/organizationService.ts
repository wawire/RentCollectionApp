import { apiClient } from './api'
import {
  Organization,
  CreateOrganizationRequest,
  AssignUserToPropertyRequest,
} from '../types/organization.types'
import { AuthResponse, RegisterRequest } from '../types/auth.types'

export const organizationService = {
  async list(): Promise<Organization[]> {
    const response = await apiClient.get('/organizations')
    return response.data
  },

  async getById(id: number): Promise<Organization> {
    const response = await apiClient.get(`/organizations/${id}`)
    return response.data
  },

  async create(data: CreateOrganizationRequest): Promise<Organization> {
    const response = await apiClient.post('/organizations', data)
    return response.data
  },

  async createUser(organizationId: number, data: RegisterRequest): Promise<AuthResponse> {
    const response = await apiClient.post(`/organizations/${organizationId}/users`, data)
    return response.data
  },

  async assignUserToProperty(
    organizationId: number,
    propertyId: number,
    data: AssignUserToPropertyRequest
  ): Promise<void> {
    await apiClient.post(`/organizations/${organizationId}/properties/${propertyId}/assign-user`, data)
  },
}
