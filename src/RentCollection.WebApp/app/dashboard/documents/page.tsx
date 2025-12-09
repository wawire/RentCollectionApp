'use client'

import { useState, useEffect } from 'react'
import Link from 'next/link'
import { FaArrowLeft, FaFileAlt, FaCheckCircle, FaClock, FaDownload, FaSearch, FaFilter, FaSpinner, FaTimesCircle } from 'react-icons/fa'
import { documentService, DocumentDto } from '@/lib/services/documentService'

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
      // TODO: Create a getAllDocuments endpoint in the backend for landlords
      // For now, using getMyDocuments which only returns current user's documents
      const data = await documentService.getMyDocuments()
      setDocuments(data)
      setError(null)
    } catch (err: any) {
      setError(err.message || 'Failed to load documents')
    } finally {
      setLoading(false)
    }
  }

  const applyFilters = () => {
    let filtered = [...documents]

    // Status filter
    if (statusFilter === 'verified') {
      filtered = filtered.filter(doc => doc.isVerified)
    } else if (statusFilter === 'pending') {
      filtered = filtered.filter(doc => !doc.isVerified)
    }

    // Type filter
    if (typeFilter !== 'all') {
      filtered = filtered.filter(doc => doc.documentType === typeFilter)
    }

    // Search filter
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

      // Refresh documents list
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
        <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
          <FaCheckCircle className="mr-1" />
          Verified
        </span>
      )
    }
    return (
      <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800">
        <FaClock className="mr-1" />
        Pending Verification
      </span>
    )
  }

  const stats = {
    total: documents.length,
    verified: documents.filter(d => d.isVerified).length,
    pending: documents.filter(d => !d.isVerified).length,
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Header */}
        <div className="mb-8">
          <Link
            href="/dashboard"
            className="inline-flex items-center text-blue-600 hover:text-blue-800 mb-4"
          >
            <FaArrowLeft className="mr-2" />
            Back to Dashboard
          </Link>
          <h1 className="text-3xl font-bold text-gray-900">Document Verification</h1>
          <p className="mt-2 text-gray-600">
            Review and verify tenant documents
          </p>
        </div>

        {/* Error/Success Messages */}
        {error && (
          <div className="mb-6 bg-red-50 border border-red-200 text-red-800 px-4 py-3 rounded-lg">
            {error}
          </div>
        )}
        {successMessage && (
          <div className="mb-6 bg-green-50 border border-green-200 text-green-800 px-4 py-3 rounded-lg">
            {successMessage}
          </div>
        )}

        {/* Stats Cards */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-6">
          <div className="bg-white rounded-lg shadow-md p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">Total Documents</p>
                <p className="text-3xl font-bold text-gray-900">{stats.total}</p>
              </div>
              <FaFileAlt className="text-4xl text-blue-600" />
            </div>
          </div>
          <div className="bg-white rounded-lg shadow-md p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">Verified</p>
                <p className="text-3xl font-bold text-green-600">{stats.verified}</p>
              </div>
              <FaCheckCircle className="text-4xl text-green-600" />
            </div>
          </div>
          <div className="bg-white rounded-lg shadow-md p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">Pending</p>
                <p className="text-3xl font-bold text-yellow-600">{stats.pending}</p>
              </div>
              <FaClock className="text-4xl text-yellow-600" />
            </div>
          </div>
        </div>

        {/* Filters */}
        <div className="bg-white rounded-lg shadow-md p-6 mb-6">
          <div className="flex items-center gap-2 mb-4">
            <FaFilter className="text-gray-600" />
            <h2 className="text-lg font-semibold text-gray-900">Filters</h2>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            {/* Status Filter */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Verification Status
              </label>
              <select
                value={statusFilter}
                onChange={(e) => setStatusFilter(e.target.value as FilterType)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              >
                <option value="all">All Documents</option>
                <option value="pending">Pending Verification</option>
                <option value="verified">Verified</option>
              </select>
            </div>

            {/* Type Filter */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Document Type
              </label>
              <select
                value={typeFilter}
                onChange={(e) => setTypeFilter(e.target.value as DocumentTypeFilter)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              >
                {documentTypes.map(type => (
                  <option key={type.value} value={type.value}>
                    {type.label}
                  </option>
                ))}
              </select>
            </div>

            {/* Search */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Search
              </label>
              <div className="relative">
                <FaSearch className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" />
                <input
                  type="text"
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  placeholder="Search by file name or tenant..."
                  className="w-full pl-10 pr-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
            </div>
          </div>
        </div>

        {/* Documents List */}
        <div className="bg-white rounded-lg shadow-md p-6">
          <h2 className="text-xl font-semibold text-gray-900 mb-4">
            Documents ({filteredDocuments.length})
          </h2>

          {loading ? (
            <div className="flex items-center justify-center py-12">
              <FaSpinner className="animate-spin text-4xl text-blue-600" />
            </div>
          ) : filteredDocuments.length === 0 ? (
            <div className="text-center py-12">
              <FaFileAlt className="mx-auto text-6xl text-gray-300 mb-4" />
              <p className="text-gray-500 text-lg">No documents found</p>
              <p className="text-gray-400 text-sm mt-2">
                {documents.length === 0
                  ? 'No documents have been uploaded yet'
                  : 'Try adjusting your filters'}
              </p>
            </div>
          ) : (
            <div className="space-y-4">
              {filteredDocuments.map((doc) => (
                <div
                  key={doc.id}
                  className="border border-gray-200 rounded-lg p-4 hover:border-blue-300 transition-colors"
                >
                  <div className="flex items-start justify-between">
                    <div className="flex-1">
                      <div className="flex items-center gap-3 mb-2">
                        <FaFileAlt className="text-blue-600 text-xl flex-shrink-0" />
                        <div>
                          <h3 className="font-semibold text-gray-900">{doc.fileName}</h3>
                          <p className="text-sm text-gray-600">
                            {getDocumentTypeLabel(doc.documentType)} • {doc.fileSizeFormatted}
                          </p>
                        </div>
                      </div>

                      {doc.tenantName && (
                        <p className="text-sm text-gray-600 ml-8 mb-1">
                          <strong>Tenant:</strong> {doc.tenantName}
                        </p>
                      )}

                      {doc.description && (
                        <p className="text-sm text-gray-600 mb-2 ml-8">
                          <strong>Description:</strong> {doc.description}
                        </p>
                      )}

                      <div className="ml-8 space-y-1">
                        <div className="flex items-center gap-2">
                          {getVerificationBadge(doc.isVerified)}
                          {doc.isVerified && doc.verifiedByName && (
                            <span className="text-xs text-gray-500">
                              by {doc.verifiedByName}
                            </span>
                          )}
                        </div>
                        <p className="text-xs text-gray-500">
                          Uploaded {new Date(doc.uploadedAt).toLocaleDateString()} by {doc.uploadedByName}
                        </p>
                      </div>
                    </div>

                    <div className="flex items-center gap-2 ml-4">
                      <a
                        href={doc.fileUrl}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="p-2 text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                        title="Download"
                      >
                        <FaDownload />
                      </a>
                      {!doc.isVerified && (
                        <>
                          <button
                            onClick={() => handleVerifyDocument(doc.id, true)}
                            className="px-3 py-1.5 bg-green-600 text-white text-sm rounded-lg hover:bg-green-700 transition-colors flex items-center"
                            title="Verify"
                          >
                            <FaCheckCircle className="mr-1" />
                            Verify
                          </button>
                          <button
                            onClick={() => handleVerifyDocument(doc.id, false)}
                            className="px-3 py-1.5 bg-red-600 text-white text-sm rounded-lg hover:bg-red-700 transition-colors flex items-center"
                            title="Reject"
                          >
                            <FaTimesCircle className="mr-1" />
                            Reject
                          </button>
                        </>
                      )}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
