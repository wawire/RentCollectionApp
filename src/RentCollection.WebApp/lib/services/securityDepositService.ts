import apiClient from './apiClient';
import type {
  SecurityDepositBalance,
  SecurityDepositTransaction,
  RecordSecurityDepositDto,
  DeductSecurityDepositDto,
  RefundSecurityDepositDto,
} from '../types/securityDeposit.types';

export const securityDepositService = {
  /**
   * Get all security deposits (Landlord/Admin only)
   */
  async getAllDeposits(): Promise<SecurityDepositBalance[]> {
    const response = await apiClient.get<SecurityDepositBalance[]>(
      '/securitydeposits'
    );
    return response.data;
  },

  /**
   * Get deposit balance for a specific tenant
   */
  async getDepositBalance(tenantId: number): Promise<SecurityDepositBalance> {
    const response = await apiClient.get<SecurityDepositBalance>(
      `/securitydeposits/tenant/${tenantId}/balance`
    );
    return response.data;
  },

  /**
   * Get transaction history for a tenant
   */
  async getTransactionHistory(
    tenantId: number,
    startDate?: string,
    endDate?: string
  ): Promise<SecurityDepositTransaction[]> {
    const params = new URLSearchParams();
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);

    const url = `/securitydeposits/tenant/${tenantId}/transactions${
      params.toString() ? `?${params.toString()}` : ''
    }`;
    const response = await apiClient.get<SecurityDepositTransaction[]>(url);
    return response.data;
  },

  /**
   * Record initial security deposit payment
   */
  async recordDeposit(
    tenantId: number,
    data: RecordSecurityDepositDto
  ): Promise<SecurityDepositTransaction> {
    const response = await apiClient.post<SecurityDepositTransaction>(
      `/securitydeposits/tenant/${tenantId}/record`,
      data
    );
    return response.data;
  },

  /**
   * Deduct from security deposit
   */
  async deductFromDeposit(
    tenantId: number,
    data: DeductSecurityDepositDto
  ): Promise<SecurityDepositTransaction> {
    const response = await apiClient.post<SecurityDepositTransaction>(
      `/securitydeposits/tenant/${tenantId}/deduct`,
      data
    );
    return response.data;
  },

  /**
   * Refund security deposit
   */
  async refundDeposit(
    tenantId: number,
    data: RefundSecurityDepositDto
  ): Promise<SecurityDepositTransaction> {
    const response = await apiClient.post<SecurityDepositTransaction>(
      `/securitydeposits/tenant/${tenantId}/refund`,
      data
    );
    return response.data;
  },
};
