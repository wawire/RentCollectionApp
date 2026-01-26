import { apiClient } from './api'
import { MeterReading, CreateMeterReadingDto } from '../types/utility.types'

export const meterReadingService = {
  async getAll(
    propertyId?: number,
    unitId?: number,
    utilityConfigId?: number,
    startDate?: string,
    endDate?: string
  ): Promise<MeterReading[]> {
    const response = await apiClient.get('/meterreadings', {
      params: { propertyId, unitId, utilityConfigId, startDate, endDate },
    })
    return response.data.data
  },

  async create(data: CreateMeterReadingDto): Promise<MeterReading> {
    const response = await apiClient.post('/meterreadings', data)
    return response.data.data
  },
}
