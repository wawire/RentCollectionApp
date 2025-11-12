import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios'
import { ApiError } from '../types/common.types'

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'https://localhost:7000/api'

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 30000, // 30 seconds timeout
})

// Request interceptor for adding auth token
apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    // Add auth token here if using authentication
    if (typeof window !== 'undefined') {
      const token = localStorage.getItem('authToken')
      if (token && config.headers) {
        config.headers.Authorization = `Bearer ${token}`
      }
    }
    return config
  },
  (error: AxiosError) => {
    console.error('Request error:', error)
    return Promise.reject(error)
  }
)

// Response interceptor for error handling
apiClient.interceptors.response.use(
  (response) => {
    // Return successful response
    return response
  },
  (error: AxiosError<any>) => {
    // Create structured error object
    const apiError: ApiError = {
      message: 'An unexpected error occurred',
      statusCode: 500,
    }

    if (error.response) {
      // Server responded with error status
      apiError.statusCode = error.response.status

      // Handle different error response formats
      if (error.response.data) {
        if (typeof error.response.data === 'string') {
          apiError.message = error.response.data
        } else if (error.response.data.message) {
          apiError.message = error.response.data.message
        } else if (error.response.data.title) {
          apiError.message = error.response.data.title
        }

        // Handle validation errors
        if (error.response.data.errors) {
          apiError.errors = error.response.data.errors
        }
      }

      // Handle specific HTTP status codes
      switch (error.response.status) {
        case 400:
          apiError.message = apiError.message || 'Bad request. Please check your input.'
          break
        case 401:
          apiError.message = 'Unauthorized. Please log in again.'
          // Optionally redirect to login
          if (typeof window !== 'undefined') {
            // window.location.href = '/login'
          }
          break
        case 403:
          apiError.message = 'Forbidden. You do not have permission to access this resource.'
          break
        case 404:
          apiError.message = apiError.message || 'Resource not found.'
          break
        case 500:
          apiError.message = 'Internal server error. Please try again later.'
          break
        case 503:
          apiError.message = 'Service unavailable. Please try again later.'
          break
      }
    } else if (error.request) {
      // Request made but no response received
      apiError.message = 'Network error. Please check your connection and try again.'
      apiError.statusCode = 0
    } else {
      // Something happened in setting up the request
      apiError.message = error.message || 'An unexpected error occurred'
    }

    // Log error for debugging (in development only)
    if (process.env.NODE_ENV === 'development') {
      console.error('API Error:', {
        message: apiError.message,
        status: apiError.statusCode,
        errors: apiError.errors,
        originalError: error,
      })
    }

    return Promise.reject(apiError)
  }
)

export default apiClient
