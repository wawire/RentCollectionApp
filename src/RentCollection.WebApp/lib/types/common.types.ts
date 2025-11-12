export interface ApiResponse<T = any> {
  data: T
  message?: string
  success: boolean
}

export interface PaginatedResponse<T> {
  items: T[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

export interface PaginationParams {
  pageNumber?: number
  pageSize?: number
  searchTerm?: string
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

export interface ApiError {
  message: string
  statusCode: number
  errors?: Record<string, string[]>
}

export interface SelectOption {
  value: string | number
  label: string
}
