import { useState, useEffect } from 'react'
import { PublicPropertyListing, PublicUnitListing, TenantApplication } from '../types/public.types'
import { publicListingService } from '../services/publicListingService'

export function usePublicProperties() {
  const [properties, setProperties] = useState<PublicPropertyListing[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    fetchProperties()
  }, [])

  const fetchProperties = async () => {
    try {
      setLoading(true)
      setError(null)
      const data = await publicListingService.getPublicProperties()
      setProperties(data)
    } catch (err: any) {
      setError(err.message || 'Failed to fetch properties')
      console.error('Error fetching properties:', err)
    } finally {
      setLoading(false)
    }
  }

  return { properties, loading, error, refetch: fetchProperties }
}

export function useVacantUnits() {
  const [units, setUnits] = useState<PublicUnitListing[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    fetchUnits()
  }, [])

  const fetchUnits = async () => {
    try {
      setLoading(true)
      setError(null)
      const data = await publicListingService.getVacantUnits()
      setUnits(data)
    } catch (err: any) {
      setError(err.message || 'Failed to fetch units')
      console.error('Error fetching units:', err)
    } finally {
      setLoading(false)
    }
  }

  return { units, loading, error, refetch: fetchUnits }
}

export function useTenantApplication() {
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState(false)

  const submitApplication = async (application: TenantApplication) => {
    try {
      setSubmitting(true)
      setError(null)
      setSuccess(false)
      await publicListingService.submitApplication(application)
      setSuccess(true)
      return true
    } catch (err: any) {
      setError(err.message || 'Failed to submit application')
      console.error('Error submitting application:', err)
      return false
    } finally {
      setSubmitting(false)
    }
  }

  const reset = () => {
    setError(null)
    setSuccess(false)
  }

  return { submitApplication, submitting, error, success, reset }
}
