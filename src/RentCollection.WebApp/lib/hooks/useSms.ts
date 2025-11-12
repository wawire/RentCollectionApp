import { useState, useCallback } from 'react'
import { smsService } from '../services'
import { SendSmsDto, SmsResponse, ApiError } from '../types'

export function useSendSms() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<ApiError | null>(null)

  const sendSms = useCallback(async (data: SendSmsDto): Promise<SmsResponse | null> => {
    try {
      setLoading(true)
      setError(null)
      const response = await smsService.sendSms(data)
      return response
    } catch (err) {
      setError(err as ApiError)
      return null
    } finally {
      setLoading(false)
    }
  }, [])

  return { sendSms, loading, error }
}

export function useSendRentReminder() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<ApiError | null>(null)

  const sendReminder = useCallback(async (tenantId: number): Promise<SmsResponse | null> => {
    try {
      setLoading(true)
      setError(null)
      const response = await smsService.sendRentReminder(tenantId)
      return response
    } catch (err) {
      setError(err as ApiError)
      return null
    } finally {
      setLoading(false)
    }
  }, [])

  return { sendReminder, loading, error }
}

export function useSendPaymentReceipt() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<ApiError | null>(null)

  const sendReceipt = useCallback(async (paymentId: number): Promise<SmsResponse | null> => {
    try {
      setLoading(true)
      setError(null)
      const response = await smsService.sendPaymentReceipt(paymentId)
      return response
    } catch (err) {
      setError(err as ApiError)
      return null
    } finally {
      setLoading(false)
    }
  }, [])

  return { sendReceipt, loading, error }
}
