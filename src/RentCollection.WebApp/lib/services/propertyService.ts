import { apiClient } from './api'
import { Property, CreatePropertyDto, UpdatePropertyDto } from '../types/property.types'

export const propertyService = {
  async getAll(): Promise<Property[]> {
    const response = await apiClient.get('/properties')
    return response.data.data
  },

  async getById(id: number): Promise<Property> {
    const response = await apiClient.get(`/properties/${id}`)
    return response.data.data
  },

  async create(data: CreatePropertyDto): Promise<Property> {
    const response = await apiClient.post('/properties', data)
    return response.data.data
  },

  async update(id: number, data: UpdatePropertyDto): Promise<Property> {
    const response = await apiClient.put(`/properties/${id}`, data)
    return response.data.data
  },

  async delete(id: number): Promise<void> {
    await apiClient.delete(`/properties/${id}`)
  },
}
