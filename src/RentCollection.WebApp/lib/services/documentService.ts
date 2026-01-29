import { apiClient } from './api'

export interface DocumentDto {
  id: number
  documentType: string
  documentTypeName: string
  tenantId?: number
  tenantName?: string
  propertyId?: number
  propertyName?: string
  unitId?: number
  unitNumber?: string
  fileName: string
  fileUrl: string
  fileSize: number
  fileSizeFormatted: string
  contentType: string
  uploadedByUserId: number
  uploadedByName: string
  uploadedAt: string
  description?: string
  isVerified: boolean
  verifiedByUserId?: number
  verifiedByName?: string
  verifiedAt?: string
}

export const documentService = {
  // Upload document
  uploadDocument: async (file: File, documentType: string, tenantId?: number, description?: string) => {
    const formData = new FormData()
    formData.append('file', file)
    formData.append('documentType', documentType)
    if (tenantId) formData.append('tenantId', tenantId.toString())
    if (description) formData.append('description', description)

    const response = await apiClient.post<{ data: DocumentDto }>('/documents/upload', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    return response.data.data
  },

  // Get my documents (for tenants)
  getMyDocuments: async () => {
    const response = await apiClient.get<{ data: DocumentDto[] }>('/documents/me')
    return response.data.data
  },

  // Get all documents
  getAllDocuments: async () => {
    const response = await apiClient.get<{ data: DocumentDto[] }>('/documents')
    return response.data.data
  },

  // Get documents by type
  getDocumentsByType: async (documentType: string) => {
    const response = await apiClient.get<{ data: DocumentDto[] }>(`/documents/type/${documentType}`)
    return response.data.data
  },

  // Get documents by tenant
  getDocumentsByTenant: async (tenantId: number) => {
    const response = await apiClient.get<{ data: DocumentDto[] }>(`/documents/tenant/${tenantId}`)
    return response.data.data
  },

  // Download document file
  downloadDocument: async (documentId: number) => {
    const response = await apiClient.get(`/documents/${documentId}/download`, {
      responseType: 'blob'
    })
    return response.data
  },

  // Verify document
  verifyDocument: async (documentId: number, isVerified: boolean) => {
    const response = await apiClient.patch<{ data: DocumentDto }>(
      `/documents/${documentId}/verify`,
      { isVerified }
    )
    return response.data.data
  },

  // Delete document
  deleteDocument: async (documentId: number) => {
    await apiClient.delete(`/documents/${documentId}`)
  }
}
