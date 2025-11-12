import { useState, useEffect, useCallback } from 'react'
import { propertyService } from '../services'
import { Property, CreatePropertyDto, UpdatePropertyDto, ApiError } from '../types'

export function useGetProperties() {
  const [data, setData] = useState<Property[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<ApiError | null>(null)

  const fetchProperties = useCallback(async () => {
    try {
      setLoading(true)
      setError(null)
      const properties = await propertyService.getAll()
      setData(properties)
    } catch (err) {
      setError(err as ApiError)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    fetchProperties()
  }, [fetchProperties])

  return { data, loading, error, refetch: fetchProperties }
}

export function useGetProperty(id: number | null) {
  const [data, setData] = useState<Property | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<ApiError | null>(null)

  const fetchProperty = useCallback(async () => {
    if (!id) return

    try {
      setLoading(true)
      setError(null)
      const property = await propertyService.getById(id)
      setData(property)
    } catch (err) {
      setError(err as ApiError)
    } finally {
      setLoading(false)
    }
  }, [id])

  useEffect(() => {
    fetchProperty()
  }, [fetchProperty])

  return { data, loading, error, refetch: fetchProperty }
}

export function useCreateProperty() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<ApiError | null>(null)

  const createProperty = useCallback(async (data: CreatePropertyDto): Promise<Property | null> => {
    try {
      setLoading(true)
      setError(null)
      const property = await propertyService.create(data)
      return property
    } catch (err) {
      setError(err as ApiError)
      return null
    } finally {
      setLoading(false)
    }
  }, [])

  return { createProperty, loading, error }
}

export function useUpdateProperty() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<ApiError | null>(null)

  const updateProperty = useCallback(
    async (id: number, data: UpdatePropertyDto): Promise<Property | null> => {
      try {
        setLoading(true)
        setError(null)
        const property = await propertyService.update(id, data)
        return property
      } catch (err) {
        setError(err as ApiError)
        return null
      } finally {
        setLoading(false)
      }
    },
    []
  )

  return { updateProperty, loading, error }
}

export function useDeleteProperty() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<ApiError | null>(null)

  const deleteProperty = useCallback(async (id: number): Promise<boolean> => {
    try {
      setLoading(true)
      setError(null)
      await propertyService.delete(id)
      return true
    } catch (err) {
      setError(err as ApiError)
      return false
    } finally {
      setLoading(false)
    }
  }, [])

  return { deleteProperty, loading, error }
}
