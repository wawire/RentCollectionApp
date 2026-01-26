import { apiClient } from './api'

export const ownerStatementService = {
  async getStatement(params: { year: number; month: number; propertyId?: number; landlordId?: number }) {
    const response = await apiClient.get('/ownerstatements', {
      params,
      responseType: 'blob',
    })
    return response.data as Blob
  },
}
