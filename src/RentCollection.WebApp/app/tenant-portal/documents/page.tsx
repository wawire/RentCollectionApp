'use client'

import { useState, useEffect } from 'react'
import Link from 'next/link'
import { FaArrowLeft, FaUpload, FaFileAlt, FaCheckCircle, FaClock, FaDownload, FaTrash, FaSpinner } from 'react-icons/fa'
import { documentService, DocumentDto } from '@/lib/services/documentService'

const documentTypes = [
  { value: 'LeaseAgreement', label: 'Lease Agreement' },
  { value: 'IDCopy', label: 'ID Copy' },
  { value: 'ProofOfIncome', label: 'Proof of Income' },
  { value: 'ReferenceLetter', label: 'Reference Letter' },
  { value: 'Other', label: 'Other' },
]

export default function DocumentsPage() {
  const [documents, setDocuments] = useState<DocumentDto[]>([])
  const [loading, setLoading] = useState(true)
  const [uploading, setUploading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)

  // Upload form state
  const [selectedFile, setSelectedFile] = useState<File | null>(null)
  const [documentType, setDocumentType] = useState('LeaseAgreement')
  const [description, setDescription] = useState('')

  useEffect(() => {
    fetchDocuments()
  }, [])

  const fetchDocuments = async () => {
    try {
      setLoading(true)
      const data = await documentService.getMyDocuments()
      setDocuments(data)
      setError(null)
    } catch (err: any) {
      setError(err.message || 'Failed to load documents')
    } finally {
      setLoading(false)
    }
  }

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      const file = e.target.files[0]
      // Validate file size (10MB max)
      if (file.size > 10 * 1024 * 1024) {
        setError('File size must be less than 10MB')
        return
      }
      setSelectedFile(file)
      setError(null)
    }
  }

  const handleUpload = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!selectedFile) {
      setError('Please select a file to upload')
      return
    }

    try {
      setUploading(true)
      setError(null)
      setSuccessMessage(null)

      await documentService.uploadDocument(selectedFile, documentType, undefined, description)

      setSuccessMessage('Document uploaded successfully!')
      setSelectedFile(null)
      setDescription('')
      setDocumentType('LeaseAgreement')

      // Reset file input
      const fileInput = document.getElementById('fileInput') as HTMLInputElement
      if (fileInput) fileInput.value = ''

      // Refresh document list
      await fetchDocuments()
    } catch (err: any) {
      setError(err.message || 'Failed to upload document')
    } finally {
      setUploading(false)
    }
  }

  const handleDelete = async (documentId: number, fileName: string) => {
    if (!confirm(`Are you sure you want to delete ${fileName}?`)) {
      return
    }

    try {
      await documentService.deleteDocument(documentId)
      setSuccessMessage('Document deleted successfully')
      await fetchDocuments()
    } catch (err: any) {
      setError(err.message || 'Failed to delete document')
    }
  }

  const getDocumentTypeLabel = (type: string) => {
    return documentTypes.find(dt => dt.value === type)?.label || type
  }

  const getVerificationBadge = (isVerified: boolean, verifiedBy: string | null) => {
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

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Header */}
        <div className="mb-8">
          <Link
            href="/tenant-portal"
            className="inline-flex items-center text-blue-600 hover:text-blue-800 mb-4"
          >
            <FaArrowLeft className="mr-2" />
            Back to Dashboard
          </Link>
          <h1 className="text-3xl font-bold text-gray-900">My Documents</h1>
          <p className="mt-2 text-gray-600">
            Upload and manage your lease agreements, ID copies, and other documents
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

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Upload Form */}
          <div className="lg:col-span-1">
            <div className="bg-white rounded-lg shadow-md p-6">
              <h2 className="text-xl font-semibold text-gray-900 mb-4 flex items-center">
                <FaUpload className="mr-2 text-blue-600" />
                Upload Document
              </h2>
              <form onSubmit={handleUpload} className="space-y-4">
                <div>
                  <label htmlFor="documentType" className="block text-sm font-medium text-gray-700 mb-1">
                    Document Type
                  </label>
                  <select
                    id="documentType"
                    value={documentType}
                    onChange={(e) => setDocumentType(e.target.value)}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  >
                    {documentTypes.map(type => (
                      <option key={type.value} value={type.value}>
                        {type.label}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label htmlFor="fileInput" className="block text-sm font-medium text-gray-700 mb-1">
                    Select File
                  </label>
                  <input
                    id="fileInput"
                    type="file"
                    onChange={handleFileChange}
                    accept=".pdf,.jpg,.jpeg,.png,.doc,.docx"
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                  <p className="mt-1 text-xs text-gray-500">
                    Accepted: PDF, JPG, PNG, DOC, DOCX (Max 10MB)
                  </p>
                </div>

                <div>
                  <label htmlFor="description" className="block text-sm font-medium text-gray-700 mb-1">
                    Description (Optional)
                  </label>
                  <textarea
                    id="description"
                    value={description}
                    onChange={(e) => setDescription(e.target.value)}
                    rows={3}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    placeholder="Add any notes about this document..."
                  />
                </div>

                <button
                  type="submit"
                  disabled={uploading || !selectedFile}
                  className="w-full bg-blue-600 text-white py-2 px-4 rounded-lg hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed flex items-center justify-center"
                >
                  {uploading ? (
                    <>
                      <FaSpinner className="animate-spin mr-2" />
                      Uploading...
                    </>
                  ) : (
                    <>
                      <FaUpload className="mr-2" />
                      Upload Document
                    </>
                  )}
                </button>
              </form>
            </div>
          </div>

          {/* Documents List */}
          <div className="lg:col-span-2">
            <div className="bg-white rounded-lg shadow-md p-6">
              <h2 className="text-xl font-semibold text-gray-900 mb-4 flex items-center">
                <FaFileAlt className="mr-2 text-blue-600" />
                My Documents ({documents.length})
              </h2>

              {loading ? (
                <div className="flex items-center justify-center py-12">
                  <FaSpinner className="animate-spin text-4xl text-blue-600" />
                </div>
              ) : documents.length === 0 ? (
                <div className="text-center py-12">
                  <FaFileAlt className="mx-auto text-6xl text-gray-300 mb-4" />
                  <p className="text-gray-500 text-lg">No documents uploaded yet</p>
                  <p className="text-gray-400 text-sm mt-2">
                    Upload your first document using the form on the left
                  </p>
                </div>
              ) : (
                <div className="space-y-4">
                  {documents.map((doc) => (
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

                          {doc.description && (
                            <p className="text-sm text-gray-600 mb-2 ml-8">
                              {doc.description}
                            </p>
                          )}

                          <div className="ml-8 space-y-1">
                            <div className="flex items-center gap-2">
                              {getVerificationBadge(doc.isVerified, doc.verifiedByName)}
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
                          <button
                            onClick={() => handleDelete(doc.id, doc.fileName)}
                            className="p-2 text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                            title="Delete"
                          >
                            <FaTrash />
                          </button>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
