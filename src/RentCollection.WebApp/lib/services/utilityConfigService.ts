import { apiClient } from './api'
import { UtilityConfig, CreateUtilityConfigDto, UpdateUtilityConfigDto } from '../types/utility.types'

export const utilityConfigService = {
  async getAll(propertyId?: number, unitId?: number, includeInactive: boolean = false): Promise<UtilityConfig[]> {
    const response = await apiClient.get('/utilityconfigs', {
      params: { propertyId, unitId, includeInactive },
    })
    return response.data.data
  },

  async create(data: CreateUtilityConfigDto): Promise<UtilityConfig> {
    const response = await apiClient.post('/utilityconfigs', data)
    return response.data.data
  },

  async update(id: number, data: UpdateUtilityConfigDto): Promise<UtilityConfig> {
    const response = await apiClient.put(`/utilityconfigs/${id}`, data)
    return response.data.data
  },
}
