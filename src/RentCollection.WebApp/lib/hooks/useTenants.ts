import { useState, useEffect, useCallback } from 'react'
import { tenantService } from '../services'
import { Tenant, CreateTenantDto, ApiError } from '../types'

export function useGetTenants() {
  const [data, setData] = useState<Tenant[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<ApiError | null>(null)

  const fetchTenants = useCallback(async () => {
    try {
      setLoading(true)
      setError(null)
      const tenants = await tenantService.getAll()
      setData(tenants)
    } catch (err) {
      setError(err as ApiError)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    fetchTenants()
  }, [fetchTenants])

  return { data, loading, error, refetch: fetchTenants }
}

export function useGetTenant(id: number | null) {
  const [data, setData] = useState<Tenant | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<ApiError | null>(null)

  const fetchTenant = useCallback(async () => {
    if (!id) return

    try {
      setLoading(true)
      setError(null)
      const tenant = await tenantService.getById(id)
      setData(tenant)
    } catch (err) {
      setError(err as ApiError)
    } finally {
      setLoading(false)
    }
  }, [id])

  useEffect(() => {
    fetchTenant()
  }, [fetchTenant])

  return { data, loading, error, refetch: fetchTenant }
}

export function useGetActiveTenants() {
  const [data, setData] = useState<Tenant[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<ApiError | null>(null)

  const fetchActiveTenants = useCallback(async () => {
    try {
      setLoading(true)
      setError(null)
      const tenants = await tenantService.getActive()
      setData(tenants)
    } catch (err) {
      setError(err as ApiError)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    fetchActiveTenants()
  }, [fetchActiveTenants])

  return { data, loading, error, refetch: fetchActiveTenants }
}

export function useCreateTenant() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<ApiError | null>(null)

  const createTenant = useCallback(async (data: CreateTenantDto): Promise<Tenant | null> => {
    try {
      setLoading(true)
      setError(null)
      const tenant = await tenantService.create(data)
      return tenant
    } catch (err) {
      setError(err as ApiError)
      return null
    } finally {
      setLoading(false)
    }
  }, [])

  return { createTenant, loading, error }
}

export function useUpdateTenant() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<ApiError | null>(null)

  const updateTenant = useCallback(
    async (id: number, data: CreateTenantDto): Promise<Tenant | null> => {
      try {
        setLoading(true)
        setError(null)
        const tenant = await tenantService.update(id, data)
        return tenant
      } catch (err) {
        setError(err as ApiError)
        return null
      } finally {
        setLoading(false)
      }
    },
    []
  )

  return { updateTenant, loading, error }
}

export function useDeleteTenant() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<ApiError | null>(null)

  const deleteTenant = useCallback(async (id: number): Promise<boolean> => {
    try {
      setLoading(true)
      setError(null)
      await tenantService.delete(id)
      return true
    } catch (err) {
      setError(err as ApiError)
      return false
    } finally {
      setLoading(false)
    }
  }, [])

  return { deleteTenant, loading, error }
}

export function useActivateTenant() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<ApiError | null>(null)

  const activateTenant = useCallback(async (id: number): Promise<boolean> => {
    try {
      setLoading(true)
      setError(null)
      await tenantService.activate(id)
      return true
    } catch (err) {
      setError(err as ApiError)
      return false
    } finally {
      setLoading(false)
    }
  }, [])

  return { activateTenant, loading, error }
}

export function useDeactivateTenant() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<ApiError | null>(null)

  const deactivateTenant = useCallback(async (id: number): Promise<boolean> => {
    try {
      setLoading(true)
      setError(null)
      await tenantService.deactivate(id)
      return true
    } catch (err) {
      setError(err as ApiError)
      return false
    } finally {
      setLoading(false)
    }
  }, [])

  return { deactivateTenant, loading, error }
}
