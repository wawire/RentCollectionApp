import { apiClient } from './api'
import { UtilityType, CreateUtilityTypeDto } from '../types/utility.types'

export const utilityTypeService = {
  async getAll(includeInactive: boolean = false): Promise<UtilityType[]> {
    const response = await apiClient.get('/utilitytypes', {
      params: { includeInactive },
    })
    return response.data.data
  },

  async create(data: CreateUtilityTypeDto): Promise<UtilityType> {
    const response = await apiClient.post('/utilitytypes', data)
    return response.data.data
  },
}
