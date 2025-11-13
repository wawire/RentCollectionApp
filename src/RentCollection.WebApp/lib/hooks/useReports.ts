import { useState, useCallback } from 'react'
import { reportService } from '../services/reportService'
import { ApiError } from '../types'

export function useDownloadMonthlyReport() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<ApiError | null>(null)

  const downloadReport = useCallback(async (year: number, month: number): Promise<boolean> => {
    try {
      setLoading(true)
      setError(null)
      const blob = await reportService.downloadMonthlyReport(year, month)

      // Create download link
      const url = window.URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = `Monthly-Report-${year}-${String(month).padStart(2, '0')}.pdf`
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

  return { downloadReport, loading, error }
}

export function useDownloadTenantList() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<ApiError | null>(null)

  const downloadList = useCallback(async (): Promise<boolean> => {
    try {
      setLoading(true)
      setError(null)
      const blob = await reportService.downloadTenantList()

      // Create download link
      const url = window.URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = `Tenant-List-${new Date().toISOString().split('T')[0]}.pdf`
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

  return { downloadList, loading, error }
}
