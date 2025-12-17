import { apiClient } from './api'
import {
  LandlordPaymentAccount,
  CreateLandlordPaymentAccountDto,
  UpdateLandlordPaymentAccountDto,
} from '../types/landlordPaymentAccount.types'

export const landlordPaymentAccountService = {
  /**
   * Get all payment accounts for the current landlord
   */
  async getMyAccounts(): Promise<LandlordPaymentAccount[]> {
    const response = await apiClient.get('/LandlordPaymentAccounts')
    return response.data.data
  },

  /**
   * Get payment accounts for a specific property
   */
  async getPropertyAccounts(propertyId: number): Promise<LandlordPaymentAccount[]> {
    const response = await apiClient.get(`/LandlordPaymentAccounts/property/${propertyId}`)
    return response.data.data
  },

  /**
   * Get payment account by ID
   */
  async getById(id: number): Promise<LandlordPaymentAccount> {
    const response = await apiClient.get(`/LandlordPaymentAccounts/${id}`)
    return response.data.data
  },

  /**
   * Get default payment account
   */
  async getDefaultAccount(propertyId?: number): Promise<LandlordPaymentAccount> {
    const params = propertyId ? { propertyId } : {}
    const response = await apiClient.get('/LandlordPaymentAccounts/default', { params })
    return response.data.data
  },

  /**
   * Create a new payment account
   */
  async create(data: CreateLandlordPaymentAccountDto): Promise<LandlordPaymentAccount> {
    const response = await apiClient.post('/LandlordPaymentAccounts', data)
    return response.data.data
  },

  /**
   * Update an existing payment account
   */
  async update(id: number, data: UpdateLandlordPaymentAccountDto): Promise<LandlordPaymentAccount> {
    const response = await apiClient.put(`/LandlordPaymentAccounts/${id}`, data)
    return response.data.data
  },

  /**
   * Delete a payment account
   */
  async delete(id: number): Promise<void> {
    await apiClient.delete(`/LandlordPaymentAccounts/${id}`)
  },

  /**
   * Set an account as default
   */
  async setDefault(id: number): Promise<void> {
    await apiClient.post(`/LandlordPaymentAccounts/${id}/set-default`)
  },
}
