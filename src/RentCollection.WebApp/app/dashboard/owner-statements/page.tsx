'use client'

import { useEffect, useState } from 'react'
import ProtectedRoute from '@/components/auth/ProtectedRoute'
import { Card, Button, LoadingSpinner, Alert, Select } from '@/components/common'
import { documentService } from '@/lib/services/documentService'
import { ownerStatementService } from '@/lib/services/ownerStatementService'
import { useGetProperties } from '@/lib/hooks/useProperties'
import { useAuth } from '@/contexts/AuthContext'
import { UserRole } from '@/lib/types/auth.types'

interface OwnerStatementDocument {
  id: number
  fileName: string
  fileUrl: string
  uploadedAt: string
  description?: string
}

export default function OwnerStatementsPage() {
  const { data: properties } = useGetProperties()
  const { user } = useAuth()
  const [documents, setDocuments] = useState<OwnerStatementDocument[]>([])
  const [loading, setLoading] = useState(true)
  const [alert, setAlert] = useState<{ type: 'success' | 'error'; message: string } | null>(null)
  const [year, setYear] = useState(new Date().getFullYear())
  const [month, setMonth] = useState(new Date().getMonth() + 1)
  const [propertyId, setPropertyId] = useState<number | 'all'>('all')
  const [landlordId, setLandlordId] = useState('')

  const canSetLandlord = user?.role === UserRole.PlatformAdmin

  const loadDocuments = async () => {
    try {
      setLoading(true)
      const data = await documentService.getDocumentsByType('OwnerStatement')
      setDocuments(
        data.map((doc) => ({
          id: doc.id,
          fileName: doc.fileName,
          fileUrl: doc.fileUrl,
          uploadedAt: doc.uploadedAt,
          description: doc.description,
        }))
      )
    } catch (err: any) {
      setAlert({ type: 'error', message: err.message || 'Failed to load statements' })
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadDocuments()
  }, [])

  const handleGenerate = async () => {
    try {
      const blob = await ownerStatementService.getStatement({
        year,
        month,
        propertyId: propertyId === 'all' ? undefined : propertyId,
        landlordId: canSetLandlord && landlordId ? Number(landlordId) : undefined,
      })

      const fileName = `OwnerStatement_${year}_${String(month).padStart(2, '0')}.pdf`
      const url = window.URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = fileName
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)
      window.URL.revokeObjectURL(url)

      setAlert({ type: 'success', message: 'Owner statement generated' })
      await loadDocuments()
    } catch (err: any) {
      setAlert({ type: 'error', message: err.message || 'Failed to generate statement' })
    }
  }

  const handleDownload = async (doc: OwnerStatementDocument) => {
    try {
      const blob = await documentService.downloadDocument(doc.id)
      const url = window.URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = doc.fileName
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)
      window.URL.revokeObjectURL(url)
    } catch (err: any) {
      setAlert({ type: 'error', message: err.message || 'Failed to download statement' })
    }
  }

  const propertyOptions = [
    { value: 'all', label: 'All Properties' },
    ...properties.map((p) => ({ value: p.id, label: p.name })),
  ]

  return (
    <ProtectedRoute allowedRoles={['PlatformAdmin', 'Landlord', 'Accountant']}>
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-serif font-normal text-primary tracking-wide">Owner Statements</h1>
          <p className="text-primary/60 mt-2 tracking-wide">Generate monthly statements and download PDFs</p>
        </div>

        {alert && <Alert type={alert.type} message={alert.message} onClose={() => setAlert(null)} />}

        <Card padding="md">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Generate Statement</h2>
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Year</label>
              <input
                type="number"
                value={year}
                onChange={(e) => setYear(Number(e.target.value))}
                className="w-full rounded-lg border border-gray-300 px-3 py-2"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Month</label>
              <input
                type="number"
                min={1}
                max={12}
                value={month}
                onChange={(e) => setMonth(Number(e.target.value))}
                className="w-full rounded-lg border border-gray-300 px-3 py-2"
              />
            </div>
            <Select
              label="Property"
              value={propertyId}
              onChange={(event) => {
                const value = event.target.value
                setPropertyId(value === 'all' ? 'all' : Number(value))
              }}
              options={propertyOptions}
            />
            {canSetLandlord && (
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Landlord ID</label>
                <input
                  type="number"
                  value={landlordId}
                  onChange={(e) => setLandlordId(e.target.value)}
                  className="w-full rounded-lg border border-gray-300 px-3 py-2"
                />
              </div>
            )}
          </div>
          <div className="mt-4">
            <Button variant="primary" onClick={handleGenerate}>
              Generate PDF
            </Button>
          </div>
        </Card>

        <Card padding="md">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Saved Statements</h2>
          {loading ? (
            <div className="flex items-center justify-center py-10">
              <LoadingSpinner size="md" text="Loading statements..." />
            </div>
          ) : (
            <div className="space-y-3">
              {documents.map((doc) => (
                <div
                  key={doc.id}
                  className="flex flex-col md:flex-row md:items-center md:justify-between gap-2 border border-gray-200 rounded-lg p-4"
                >
                  <div>
                    <p className="font-medium text-gray-900">{doc.fileName}</p>
                    <p className="text-sm text-gray-500">{doc.description}</p>
                    <p className="text-xs text-gray-400">
                      {new Date(doc.uploadedAt).toLocaleString()}
                    </p>
                  </div>
                  <div>
                    <Button
                      variant="secondary"
                      onClick={() => handleDownload(doc)}
                    >
                      Download
                    </Button>
                  </div>
                </div>
              ))}
              {documents.length === 0 && (
                <div className="text-center text-gray-500 py-6">No statements saved yet.</div>
              )}
            </div>
          )}
        </Card>
      </div>
    </ProtectedRoute>
  )
}

