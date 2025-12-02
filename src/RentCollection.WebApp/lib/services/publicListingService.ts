import axios from 'axios'
import { PublicPropertyListing, PublicUnitListing, TenantApplication, TenantApplicationResponse } from '../types/public.types'

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'https://localhost:7000/api'

// Create a separate axios instance for public endpoints (no auth interceptor)
const publicApiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 30000,
})

export const publicListingService = {
  /**
   * Get all properties with vacant units
   */
  async getPublicProperties(): Promise<PublicPropertyListing[]> {
    const response = await publicApiClient.get<PublicPropertyListing[]>('/public/properties')
    return response.data
  },

  /**
   * Get specific property details with available units
   */
  async getPublicPropertyById(propertyId: number): Promise<PublicPropertyListing> {
    const response = await publicApiClient.get<PublicPropertyListing>(`/public/properties/${propertyId}`)
    return response.data
  },

  /**
   * Get all vacant units across all properties
   */
  async getVacantUnits(): Promise<PublicUnitListing[]> {
    const response = await publicApiClient.get<PublicUnitListing[]>('/public/units/vacant')
    return response.data
  },

  /**
   * Get vacant units for a specific property
   */
  async getVacantUnitsByProperty(propertyId: number): Promise<PublicUnitListing[]> {
    const response = await publicApiClient.get<PublicUnitListing[]>(`/public/properties/${propertyId}/units/vacant`)
    return response.data
  },

  /**
   * Get details of a specific unit
   */
  async getUnitDetails(unitId: number): Promise<PublicUnitListing> {
    const response = await publicApiClient.get<PublicUnitListing>(`/public/units/${unitId}`)
    return response.data
  },

  /**
   * Submit a tenant application (no authentication required)
   */
  async submitApplication(application: TenantApplication): Promise<TenantApplicationResponse> {
    const response = await publicApiClient.post<TenantApplicationResponse>('/public/applications', application)
    return response.data
  },
}

export default publicListingService
