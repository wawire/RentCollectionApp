'use client'

import { useState, useRef } from 'react'
import { PhotoType } from '@/lib/types'
import { Button } from '@/components/common'
import { CheckCircle2, Loader2, UploadCloud, X } from 'lucide-react'

interface PhotoUploadProps {
  inspectionId: number
  onUploadSuccess?: (photoData: any) => void
  onUploadError?: (error: string) => void
  inspectionItemId?: number
  defaultPhotoType?: PhotoType
}

export default function PhotoUpload({
  inspectionId,
  onUploadSuccess,
  onUploadError,
  inspectionItemId,
  defaultPhotoType = PhotoType.MoveOut
}: PhotoUploadProps) {
  const [selectedFile, setSelectedFile] = useState<File | null>(null)
  const [previewUrl, setPreviewUrl] = useState<string | null>(null)
  const [caption, setCaption] = useState('')
  const [photoType, setPhotoType] = useState<PhotoType>(defaultPhotoType)
  const [uploading, setUploading] = useState(false)
  const [uploadSuccess, setUploadSuccess] = useState(false)
  const [error, setError] = useState('')
  const fileInputRef = useRef<HTMLInputElement>(null)

  const handleFileSelect = (file: File) => {
    // Validate file type
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp']
    if (!allowedTypes.includes(file.type)) {
      setError('Please select a valid image file (JPG, PNG, or WebP)')
      return
    }

    // Validate file size (10MB)
    const maxSize = 10 * 1024 * 1024
    if (file.size > maxSize) {
      setError('File size must be less than 10MB')
      return
    }

    setSelectedFile(file)
    setError('')
    setUploadSuccess(false)

    // Create preview
    const reader = new FileReader()
    reader.onloadend = () => {
      setPreviewUrl(reader.result as string)
    }
    reader.readAsDataURL(file)
  }

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault()
    e.stopPropagation()

    const files = e.dataTransfer.files
    if (files && files.length > 0) {
      handleFileSelect(files[0])
    }
  }

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault()
    e.stopPropagation()
  }

  const handleFileInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = e.target.files
    if (files && files.length > 0) {
      handleFileSelect(files[0])
    }
  }

  const handleUpload = async () => {
    if (!selectedFile) return

    setUploading(true)
    setError('')
    setUploadSuccess(false)

    try {
      const formData = new FormData()
      formData.append('file', selectedFile)
      formData.append('caption', caption)
      formData.append('photoType', photoType.toString())
      if (inspectionItemId) {
        formData.append('inspectionItemId', inspectionItemId.toString())
      }

      const token = localStorage.getItem('token')
      const response = await fetch(`/api/moveoutinspections/${inspectionId}/upload-photo`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`
        },
        body: formData
      })

      if (!response.ok) {
        const errorData = await response.json()
        throw new Error(errorData.error || 'Failed to upload photo')
      }

      const data = await response.json()
      setUploadSuccess(true)

      // Reset form after successful upload
      setTimeout(() => {
        setSelectedFile(null)
        setPreviewUrl(null)
        setCaption('')
        setUploadSuccess(false)
        if (fileInputRef.current) {
          fileInputRef.current.value = ''
        }
      }, 2000)

      onUploadSuccess?.(data)
    } catch (err: any) {
      const errorMessage = err.message || 'Failed to upload photo'
      setError(errorMessage)
      onUploadError?.(errorMessage)
    } finally {
      setUploading(false)
    }
  }

  const handleClear = () => {
    setSelectedFile(null)
    setPreviewUrl(null)
    setCaption('')
    setError('')
    setUploadSuccess(false)
    if (fileInputRef.current) {
      fileInputRef.current.value = ''
    }
  }

  return (
    <div className="space-y-4">
      {/* Drag and Drop Area */}
      {!selectedFile && (
        <div
          onDrop={handleDrop}
          onDragOver={handleDragOver}
          onClick={() => fileInputRef.current?.click()}
          className="border-2 border-dashed border-gray-300 rounded-lg p-8 text-center cursor-pointer hover:border-primary-500 hover:bg-primary-50 transition-colors"
        >
          <UploadCloud className="mx-auto w-12 h-12 text-gray-400 mb-3" />
          <p className="text-gray-600 mb-2">
            Drag and drop a photo here, or click to select
          </p>
          <p className="text-sm text-gray-500">
            JPG, PNG, or WebP (max 10MB)
          </p>
          <input
            ref={fileInputRef}
            type="file"
            accept="image/jpeg,image/jpg,image/png,image/webp"
            onChange={handleFileInputChange}
            className="hidden"
          />
        </div>
      )}

      {/* Preview and Form */}
      {selectedFile && previewUrl && (
        <div className="space-y-4">
          {/* Image Preview */}
          <div className="relative">
            <img
              src={previewUrl}
              alt="Preview"
              className="w-full max-h-96 object-contain rounded-lg border border-gray-200"
            />
            <button
              onClick={handleClear}
              className="absolute top-2 right-2 bg-red-500 text-white p-2 rounded-full hover:bg-red-600 transition-colors"
              disabled={uploading}
            >
              <X className="w-4 h-4" />
            </button>
          </div>

          {/* File Info */}
          <div className="text-sm text-gray-600">
            <p><strong>File:</strong> {selectedFile.name}</p>
            <p><strong>Size:</strong> {(selectedFile.size / 1024 / 1024).toFixed(2)} MB</p>
          </div>

          {/* Caption Input */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Caption (Optional)
            </label>
            <input
              type="text"
              value={caption}
              onChange={(e) => setCaption(e.target.value)}
              placeholder="Add a description for this photo..."
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              disabled={uploading}
            />
          </div>

          {/* Photo Type Select */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Photo Type
            </label>
            <select
              value={photoType}
              onChange={(e) => setPhotoType(Number(e.target.value) as PhotoType)}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              disabled={uploading}
            >
              <option value={PhotoType.MoveIn}>Move-In</option>
              <option value={PhotoType.MoveOut}>Move-Out</option>
              <option value={PhotoType.Damage}>Damage</option>
              <option value={PhotoType.Before}>Before Repair</option>
              <option value={PhotoType.After}>After Repair</option>
            </select>
          </div>

          {/* Upload Button */}
          <Button
            onClick={handleUpload}
            variant="primary"
            loading={uploading}
            disabled={uploading || uploadSuccess}
            fullWidth
          >
            {uploading ? (
              <>
                <Loader2 className="animate-spin mr-2 w-4 h-4" />
                Uploading...
              </>
            ) : uploadSuccess ? (
              <>
                <CheckCircle2 className="mr-2 w-4 h-4" />
                Upload Successful
              </>
            ) : (
              'Upload Photo'
            )}
          </Button>
        </div>
      )}

      {/* Error Message */}
      {error && (
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg">
          {error}
        </div>
      )}

      {/* Success Message */}
      {uploadSuccess && (
        <div className="bg-green-50 border border-green-200 text-green-700 px-4 py-3 rounded-lg">
          Photo uploaded successfully!
        </div>
      )}
    </div>
  )
}
