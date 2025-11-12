import { useState, useEffect, useCallback } from 'react'
import { dashboardService } from '../services'
import { DashboardStats, MonthlyReport, ApiError } from '../types'

export function useGetDashboardStats() {
  const [data, setData] = useState<DashboardStats | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<ApiError | null>(null)

  const fetchStats = useCallback(async () => {
    try {
      setLoading(true)
      setError(null)
      const stats = await dashboardService.getStats()
      setData(stats)
    } catch (err) {
      setError(err as ApiError)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    fetchStats()
  }, [fetchStats])

  return { data, loading, error, refetch: fetchStats }
}

export function useGetMonthlyReport(year: number | null, month: number | null) {
  const [data, setData] = useState<MonthlyReport | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<ApiError | null>(null)

  const fetchReport = useCallback(async () => {
    if (!year || !month) return

    try {
      setLoading(true)
      setError(null)
      const report = await dashboardService.getMonthlyReport(year, month)
      setData(report)
    } catch (err) {
      setError(err as ApiError)
    } finally {
      setLoading(false)
    }
  }, [year, month])

  useEffect(() => {
    fetchReport()
  }, [fetchReport])

  return { data, loading, error, refetch: fetchReport }
}

export function useGetYearlyReports(year: number | null) {
  const [data, setData] = useState<MonthlyReport[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<ApiError | null>(null)

  const fetchReports = useCallback(async () => {
    if (!year) return

    try {
      setLoading(true)
      setError(null)
      const reports = await dashboardService.getYearlyReports(year)
      setData(reports)
    } catch (err) {
      setError(err as ApiError)
    } finally {
      setLoading(false)
    }
  }, [year])

  useEffect(() => {
    fetchReports()
  }, [fetchReports])

  return { data, loading, error, refetch: fetchReports }
}
