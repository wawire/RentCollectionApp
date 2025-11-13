import { useState, useEffect, useCallback } from 'react'
import { paymentService } from '../services'
import { Payment, CreatePaymentDto, ApiError } from '../types'

export function useGetPayments() {
  const [data, setData] = useState<Payment[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<ApiError | null>(null)

  const fetchPayments = useCallback(async () => {
    try {
      setLoading(true)
      setError(null)
      const payments = await paymentService.getAll()
      setData(payments)
    } catch (err) {
      setError(err as ApiError)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    fetchPayments()
  }, [fetchPayments])

  return { data, loading, error, refetch: fetchPayments }
}

export function useGetPayment(id: number | null) {
  const [data, setData] = useState<Payment | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<ApiError | null>(null)

  const fetchPayment = useCallback(async () => {
    if (!id) return

    try {
      setLoading(true)
      setError(null)
      const payment = await paymentService.getById(id)
      setData(payment)
    } catch (err) {
      setError(err as ApiError)
    } finally {
      setLoading(false)
    }
  }, [id])

  useEffect(() => {
    fetchPayment()
  }, [fetchPayment])

  return { data, loading, error, refetch: fetchPayment }
}

export function useGetTenantPayments(tenantId: number | null) {
  const [data, setData] = useState<Payment[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<ApiError | null>(null)

  const fetchPayments = useCallback(async () => {
    if (!tenantId) return

    try {
      setLoading(true)
      setError(null)
      const payments = await paymentService.getByTenant(tenantId)
      setData(payments)
    } catch (err) {
      setError(err as ApiError)
    } finally {
      setLoading(false)
    }
  }, [tenantId])

  useEffect(() => {
    fetchPayments()
  }, [fetchPayments])

  return { data, loading, error, refetch: fetchPayments }
}

export function useGetPropertyPayments(propertyId: number | null) {
  const [data, setData] = useState<Payment[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<ApiError | null>(null)

  const fetchPayments = useCallback(async () => {
    if (!propertyId) return

    try {
      setLoading(true)
      setError(null)
      const payments = await paymentService.getByProperty(propertyId)
      setData(payments)
    } catch (err) {
      setError(err as ApiError)
    } finally {
      setLoading(false)
    }
  }, [propertyId])

  useEffect(() => {
    fetchPayments()
  }, [fetchPayments])

  return { data, loading, error, refetch: fetchPayments }
}

export function useGetPaymentsByDateRange() {
  const [data, setData] = useState<Payment[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<ApiError | null>(null)

  const fetchPayments = useCallback(async (startDate: string, endDate: string) => {
    try {
      setLoading(true)
      setError(null)
      const payments = await paymentService.getByDateRange(startDate, endDate)
      setData(payments)
    } catch (err) {
      setError(err as ApiError)
    } finally {
      setLoading(false)
    }
  }, [])

  return { data, loading, error, fetchPayments }
}

export function useCreatePayment() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<ApiError | null>(null)

  const createPayment = useCallback(async (data: CreatePaymentDto): Promise<Payment | null> => {
    try {
      setLoading(true)
      setError(null)
      const payment = await paymentService.create(data)
      return payment
    } catch (err) {
      setError(err as ApiError)
      return null
    } finally {
      setLoading(false)
    }
  }, [])

  return { createPayment, loading, error }
}

export function useDeletePayment() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<ApiError | null>(null)

  const deletePayment = useCallback(async (id: number): Promise<boolean> => {
    try {
      setLoading(true)
      setError(null)
      await paymentService.delete(id)
      return true
    } catch (err) {
      setError(err as ApiError)
      return false
    } finally {
      setLoading(false)
    }
  }, [])

  return { deletePayment, loading, error }
}

export function useDownloadPaymentReceipt() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<ApiError | null>(null)

  const downloadReceipt = useCallback(async (paymentId: number, fileName?: string): Promise<boolean> => {
    try {
      setLoading(true)
      setError(null)
      const blob = await paymentService.downloadReceipt(paymentId)

      // Create download link
      const url = window.URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = fileName || `Payment-Receipt-${paymentId}.pdf`
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)
      window.URL.revokeObjectURL(url)

      return true
    } catch (err) {
      setError(err as ApiError)
      return false
    } finally {
      setLoading(false)
    }
  }, [])

  return { downloadReceipt, loading, error }
}
