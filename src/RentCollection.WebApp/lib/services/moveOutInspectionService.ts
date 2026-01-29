import { apiClient } from './api';
import type {
  MoveOutInspection,
  CreateMoveOutInspectionDto,
  RecordInspectionDto,
  SettleInspectionDto,
  ProcessRefundDto,
  UploadPhotoDto,
  InspectionPhoto,
  MoveOutInspectionStatus,
} from '../types/moveOutInspection.types';

export const moveOutInspectionService = {
  /**
   * Get all pending inspections (Landlord/Admin only)
   */
  async getPendingInspections(): Promise<MoveOutInspection[]> {
    const response = await apiClient.get<MoveOutInspection[]>(
      '/moveoutinspections/pending'
    );
    return response.data;
  },

  /**
   * Get inspections by status
   */
  async getInspectionsByStatus(
    status: MoveOutInspectionStatus
  ): Promise<MoveOutInspection[]> {
    const response = await apiClient.get<MoveOutInspection[]>(
      `/moveoutinspections/status/${status}`
    );
    return response.data;
  },

  /**
   * Get inspection by ID
   */
  async getInspectionById(id: number): Promise<MoveOutInspection> {
    const response = await apiClient.get<MoveOutInspection>(
      `/moveoutinspections/${id}`
    );
    return response.data;
  },

  /**
   * Get all inspections for a tenant
   */
  async getTenantInspections(tenantId: number): Promise<MoveOutInspection[]> {
    const response = await apiClient.get<MoveOutInspection[]>(
      `/moveoutinspections/tenant/${tenantId}`
    );
    return response.data;
  },

  /**
   * Get all inspections for the authenticated tenant
   */
  async getMyInspections(): Promise<MoveOutInspection[]> {
    const response = await apiClient.get<MoveOutInspection[]>(
      '/moveoutinspections/me'
    );
    return response.data;
  },

  /**
   * Get all inspections for a property
   */
  async getPropertyInspections(
    propertyId: number
  ): Promise<MoveOutInspection[]> {
    const response = await apiClient.get<MoveOutInspection[]>(
      `/moveoutinspections/property/${propertyId}`
    );
    return response.data;
  },

  /**
   * Schedule a new move-out inspection
   */
  async scheduleInspection(
    data: CreateMoveOutInspectionDto
  ): Promise<MoveOutInspection> {
    const response = await apiClient.post<MoveOutInspection>(
      '/moveoutinspections',
      data
    );
    return response.data;
  },

  /**
   * Record inspection results and calculate deductions
   */
  async recordInspection(
    id: number,
    data: RecordInspectionDto
  ): Promise<MoveOutInspection> {
    const response = await apiClient.post<MoveOutInspection>(
      `/moveoutinspections/${id}/record`,
      data
    );
    return response.data;
  },

  /**
   * Settle the inspection and finalize deductions
   */
  async settleInspection(
    id: number,
    data: SettleInspectionDto
  ): Promise<MoveOutInspection> {
    const response = await apiClient.post<MoveOutInspection>(
      `/moveoutinspections/${id}/settle`,
      data
    );
    return response.data;
  },

  /**
   * Process refund to tenant
   */
  async processRefund(
    id: number,
    data: ProcessRefundDto
  ): Promise<MoveOutInspection> {
    const response = await apiClient.post<MoveOutInspection>(
      `/moveoutinspections/${id}/refund`,
      data
    );
    return response.data;
  },

  /**
   * Upload photo for inspection
   */
  async uploadPhoto(
    id: number,
    data: UploadPhotoDto
  ): Promise<InspectionPhoto> {
    const response = await apiClient.post<InspectionPhoto>(
      `/moveoutinspections/${id}/photos`,
      data
    );
    return response.data;
  },

  /**
   * Delete inspection photo
   */
  async deletePhoto(photoId: number): Promise<void> {
    await apiClient.delete(`/moveoutinspections/photos/${photoId}`);
  },
};
