import { useState, useEffect, useCallback } from 'react'
import { unitService } from '../services'
import { Unit, CreateUnitDto, UpdateUnitDto, ApiError } from '../types'

export function useGetUnits() {
  const [data, setData] = useState<Unit[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<ApiError | null>(null)

  const fetchUnits = useCallback(async () => {
    try {
      setLoading(true)
      setError(null)
      const units = await unitService.getAll()
      setData(units)
    } catch (err) {
      setError(err as ApiError)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    fetchUnits()
  }, [fetchUnits])

  return { data, loading, error, refetch: fetchUnits }
}

export function useGetUnit(id: number | null) {
  const [data, setData] = useState<Unit | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<ApiError | null>(null)

  const fetchUnit = useCallback(async () => {
    if (!id) return

    try {
      setLoading(true)
      setError(null)
      const unit = await unitService.getById(id)
      setData(unit)
    } catch (err) {
      setError(err as ApiError)
    } finally {
      setLoading(false)
    }
  }, [id])

  useEffect(() => {
    fetchUnit()
  }, [fetchUnit])

  return { data, loading, error, refetch: fetchUnit }
}

export function useGetUnitsByProperty(propertyId: number | null) {
  const [data, setData] = useState<Unit[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<ApiError | null>(null)

  const fetchUnits = useCallback(async () => {
    if (!propertyId) return

    try {
      setLoading(true)
      setError(null)
      const units = await unitService.getByProperty(propertyId)
      setData(units)
    } catch (err) {
      setError(err as ApiError)
    } finally {
      setLoading(false)
    }
  }, [propertyId])

  useEffect(() => {
    fetchUnits()
  }, [fetchUnits])

  return { data, loading, error, refetch: fetchUnits }
}

export function useGetVacantUnits() {
  const [data, setData] = useState<Unit[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<ApiError | null>(null)

  const fetchVacantUnits = useCallback(async () => {
    try {
      setLoading(true)
      setError(null)
      const units = await unitService.getVacantUnits()
      setData(units)
    } catch (err) {
      setError(err as ApiError)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    fetchVacantUnits()
  }, [fetchVacantUnits])

  return { data, loading, error, refetch: fetchVacantUnits }
}

export function useCreateUnit() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<ApiError | null>(null)

  const createUnit = useCallback(async (data: CreateUnitDto): Promise<Unit | null> => {
    try {
      setLoading(true)
      setError(null)
      const unit = await unitService.create(data)
      return unit
    } catch (err) {
      setError(err as ApiError)
      return null
    } finally {
      setLoading(false)
    }
  }, [])

  return { createUnit, loading, error }
}

export function useUpdateUnit() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<ApiError | null>(null)

  const updateUnit = useCallback(
    async (id: number, data: UpdateUnitDto): Promise<Unit | null> => {
      try {
        setLoading(true)
        setError(null)
        const unit = await unitService.update(id, data)
        return unit
      } catch (err) {
        setError(err as ApiError)
        return null
      } finally {
        setLoading(false)
      }
    },
    []
  )

  return { updateUnit, loading, error }
}

export function useDeleteUnit() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<ApiError | null>(null)

  const deleteUnit = useCallback(async (id: number): Promise<boolean> => {
    try {
      setLoading(true)
      setError(null)
      await unitService.delete(id)
      return true
    } catch (err) {
      setError(err as ApiError)
      return false
    } finally {
      setLoading(false)
    }
  }, [])

  return { deleteUnit, loading, error }
}
