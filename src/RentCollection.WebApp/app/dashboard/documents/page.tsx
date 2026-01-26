'use client'

import { useState, useEffect } from 'react'
import { CheckCircle2, Clock, Download, FileText, Filter, XCircle } from 'lucide-react'
import { documentService, DocumentDto } from '@/lib/services/documentService'
import ProtectedRoute from '@/components/auth/ProtectedRoute'
import {
  Alert,
  Badge,
  Button,
  Card,
  EmptyState,
  PageHeader,
  SearchBar,
  Select,
  SkeletonTable,
  Table,
} from '@/components/common'

type FilterType = 'all' | 'verified' | 'pending'
type DocumentTypeFilter = 'all' | 'LeaseAgreement' | 'IDCopy' | 'ProofOfIncome' | 'ReferenceLetter' | 'Other'

const documentTypes = [
  { value: 'all', label: 'All Types' },
  { value: 'LeaseAgreement', label: 'Lease Agreement' },
  { value: 'IDCopy', label: 'ID Copy' },
  { value: 'ProofOfIncome', label: 'Proof of Income' },
  { value: 'ReferenceLetter', label: 'Reference Letter' },
  { value: 'Other', label: 'Other' },
]

export default function DocumentVerificationPage() {
  const [documents, setDocuments] = useState<DocumentDto[]>([])
  const [filteredDocuments, setFilteredDocuments] = useState<DocumentDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)

  const [statusFilter, setStatusFilter] = useState<FilterType>('all')
  const [typeFilter, setTypeFilter] = useState<DocumentTypeFilter>('all')
  const [searchTerm, setSearchTerm] = useState('')

  useEffect(() => {
    fetchAllDocuments()
  }, [])

  useEffect(() => {
    applyFilters()
  }, [documents, statusFilter, typeFilter, searchTerm])

  const fetchAllDocuments = async () => {
    try {
      setLoading(true)
      const data = await documentService.getAllDocuments()
      setDocuments(data)
      setError(null)
    } catch (err: any) {
      setError(err.message || 'Failed to load documents')
    } finally {
      setLoading(false)
    }
  }

  const handleDownload = async (documentId: number, fileName: string) => {
    try {
      const blob = await documentService.downloadDocument(documentId)
      const url = URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = fileName
      document.body.appendChild(link)
      link.click()
      link.remove()
      URL.revokeObjectURL(url)
    } catch (err: any) {
      setError(err.message || 'Failed to download document')
    }
  }

  const applyFilters = () => {
    let filtered = [...documents]

    if (statusFilter === 'verified') {
      filtered = filtered.filter(doc => doc.isVerified)
    } else if (statusFilter === 'pending') {
      filtered = filtered.filter(doc => !doc.isVerified)
    }

    if (typeFilter !== 'all') {
      filtered = filtered.filter(doc => doc.documentType === typeFilter)
    }

    if (searchTerm) {
      const search = searchTerm.toLowerCase()
      filtered = filtered.filter(doc =>
        doc.fileName.toLowerCase().includes(search) ||
        doc.tenantName?.toLowerCase().includes(search) ||
        doc.uploadedByName.toLowerCase().includes(search)
      )
    }

    setFilteredDocuments(filtered)
  }

  const handleVerifyDocument = async (documentId: number, isVerified: boolean) => {
    try {
      setError(null)
      setSuccessMessage(null)

      await documentService.verifyDocument(documentId, isVerified)
      setSuccessMessage(`Document ${isVerified ? 'verified' : 'rejected'} successfully`)

      await fetchAllDocuments()
    } catch (err: any) {
      setError(err.message || 'Failed to update document status')
    }
  }

  const getDocumentTypeLabel = (type: string) => {
    return documentTypes.find(dt => dt.value === type)?.label || type
  }

  const getVerificationBadge = (isVerified: boolean) => {
    if (isVerified) {
      return (
        <Badge variant="success" size="sm">
          <CheckCircle2 className="mr-1 w-3 h-3" />
          Verified
        </Badge>
      )
    }
    return (
      <Badge variant="warning" size="sm">
        <Clock className="mr-1 w-3 h-3" />
        Pending Verification
      </Badge>
    )
  }

  const stats = {
    total: documents.length,
    verified: documents.filter(d => d.isVerified).length,
    pending: documents.filter(d => !d.isVerified).length,
  }

  return (
    <ProtectedRoute allowedRoles={['PlatformAdmin', 'Landlord', 'Manager', 'Caretaker', 'Accountant']}>
      <div className="space-y-6">
        <PageHeader
          title="Document Verification"
          subtitle="Review and verify tenant documents."
          breadcrumbs={[
            { label: 'Dashboard', href: '/dashboard' },
            { label: 'Documents' },
          ]}
        />

        {error && <Alert type="error" message={error} />}
        {successMessage && <Alert type="success" message={successMessage} />}

        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <Card>
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-text-muted">Total Documents</p>
                <p className="text-3xl font-semibold text-text-primary">{stats.total}</p>
              </div>
              <FileText className="w-8 h-8 text-brand-secondary" />
            </div>
          </Card>
          <Card>
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-text-muted">Verified</p>
                <p className="text-3xl font-semibold text-state-success">{stats.verified}</p>
              </div>
              <CheckCircle2 className="w-8 h-8 text-state-success" />
            </div>
          </Card>
          <Card>
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-text-muted">Pending</p>
                <p className="text-3xl font-semibold text-state-warning">{stats.pending}</p>
              </div>
              <Clock className="w-8 h-8 text-state-warning" />
            </div>
          </Card>
        </div>

        <Card>
          <div className="flex items-center gap-2 mb-4">
            <Filter className="text-text-muted w-4 h-4" />
            <h2 className="text-lg font-semibold text-text-primary">Filters</h2>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <Select
              label="Verification Status"
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value as FilterType)}
              options={[
                { value: 'all', label: 'All Documents' },
                { value: 'pending', label: 'Pending Verification' },
                { value: 'verified', label: 'Verified' },
              ]}
              fullWidth
            />

            <Select
              label="Document Type"
              value={typeFilter}
              onChange={(e) => setTypeFilter(e.target.value as DocumentTypeFilter)}
              options={documentTypes}
              fullWidth
            />

            <div>
              <label className="block text-sm font-medium text-text-secondary mb-1">Search</label>
              <SearchBar
                value={searchTerm}
                onChange={setSearchTerm}
                placeholder="Search by file name or tenant..."
              />
            </div>
          </div>
        </Card>

        <Card padding="md">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-lg font-semibold text-text-primary">
              Documents ({filteredDocuments.length})
            </h2>
          </div>

          {loading ? (
            <SkeletonTable rows={5} columns={5} />
          ) : filteredDocuments.length === 0 ? (
            <EmptyState
              title="No documents found"
              description={documents.length === 0
                ? 'No documents have been uploaded yet.'
                : 'Try adjusting your filters to see results.'}
            />
          ) : (
            <Table
              data={filteredDocuments}
              columns={[
                {
                  key: 'fileName',
                  header: 'File',
                  render: (doc) => (
                    <div>
                      <div className="font-semibold text-text-primary">{doc.fileName}</div>
                      <div className="text-xs text-text-muted">
                        {getDocumentTypeLabel(doc.documentType)} - {doc.fileSizeFormatted}
                      </div>
                      {doc.description && (
                        <div className="text-xs text-text-muted mt-1">{doc.description}</div>
                      )}
                    </div>
                  ),
                },
                {
                  key: 'tenantName',
                  header: 'Tenant',
                  render: (doc) => (
                    <span className="text-sm text-text-secondary">{doc.tenantName || '-'}</span>
                  ),
                },
                {
                  key: 'isVerified',
                  header: 'Status',
                  render: (doc) => (
                    <div className="flex items-center gap-2">
                      {getVerificationBadge(doc.isVerified)}
                      {doc.isVerified && doc.verifiedByName && (
                        <span className="text-xs text-text-muted">by {doc.verifiedByName}</span>
                      )}
                    </div>
                  ),
                },
                {
                  key: 'uploadedAt',
                  header: 'Uploaded',
                  render: (doc) => (
                    <div className="text-sm text-text-secondary">
                      {new Date(doc.uploadedAt).toLocaleDateString()} by {doc.uploadedByName}
                    </div>
                  ),
                },
                {
                  key: 'actions',
                  header: 'Actions',
                  render: (doc) => (
                    <div className="flex items-center gap-2">
                      <Button
                        variant="secondary"
                        size="sm"
                        onClick={() => handleDownload(doc.id, doc.fileName)}
                      >
                        <Download className="w-4 h-4 mr-2" />
                        Download
                      </Button>
                      {!doc.isVerified && (
                        <>
                          <Button
                            size="sm"
                            onClick={() => handleVerifyDocument(doc.id, true)}
                          >
                            <CheckCircle2 className="w-4 h-4 mr-2" />
                            Verify
                          </Button>
                          <Button
                            variant="destructive"
                            size="sm"
                            onClick={() => handleVerifyDocument(doc.id, false)}
                          >
                            <XCircle className="w-4 h-4 mr-2" />
                            Reject
                          </Button>
                        </>
                      )}
                    </div>
                  ),
                },
              ]}
            />
          )}
        </Card>
      </div>
    </ProtectedRoute>
  )
}

