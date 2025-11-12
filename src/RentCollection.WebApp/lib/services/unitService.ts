import { apiClient } from './api'
import { Unit, CreateUnitDto, UpdateUnitDto } from '../types/unit.types'

export const unitService = {
  async getAll(): Promise<Unit[]> {
    const response = await apiClient.get('/units')
    return response.data.data
  },

  async getById(id: number): Promise<Unit> {
    const response = await apiClient.get(`/units/${id}`)
    return response.data.data
  },

  async getByProperty(propertyId: number): Promise<Unit[]> {
    const response = await apiClient.get(`/units/property/${propertyId}`)
    return response.data.data
  },

  async getVacantUnits(): Promise<Unit[]> {
    const response = await apiClient.get('/units/vacant')
    return response.data.data
  },

  async create(data: CreateUnitDto): Promise<Unit> {
    const response = await apiClient.post('/units', data)
    return response.data.data
  },

  async update(id: number, data: UpdateUnitDto): Promise<Unit> {
    const response = await apiClient.put(`/units/${id}`, data)
    return response.data.data
  },

  async delete(id: number): Promise<void> {
    await apiClient.delete(`/units/${id}`)
  },
}
