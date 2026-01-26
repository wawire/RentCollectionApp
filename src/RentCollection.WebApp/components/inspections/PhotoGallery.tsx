'use client'

import { useState } from 'react'
import { PhotoType } from '@/lib/types'
import { ChevronLeft, ChevronRight, Trash2, X } from 'lucide-react'

interface InspectionPhoto {
  id: number
  photoUrl: string
  caption?: string
  photoType: PhotoType
  inspectionItemId?: number
  takenAt: string
}

interface PhotoGalleryProps {
  photos: InspectionPhoto[]
  onDeletePhoto?: (photoId: number) => void
  canDelete?: boolean
}

export default function PhotoGallery({ photos, onDeletePhoto, canDelete = false }: PhotoGalleryProps) {
  const [selectedPhoto, setSelectedPhoto] = useState<InspectionPhoto | null>(null)
  const [currentIndex, setCurrentIndex] = useState(0)

  const groupedPhotos = {
    moveIn: photos.filter(p => p.photoType === PhotoType.MoveIn),
    moveOut: photos.filter(p => p.photoType === PhotoType.MoveOut),
    damage: photos.filter(p => p.photoType === PhotoType.Damage),
    before: photos.filter(p => p.photoType === PhotoType.Before),
    after: photos.filter(p => p.photoType === PhotoType.After)
  }

  const openLightbox = (photo: InspectionPhoto, index: number) => {
    setSelectedPhoto(photo)
    setCurrentIndex(index)
  }

  const closeLightbox = () => {
    setSelectedPhoto(null)
  }

  const nextPhoto = () => {
    if (currentIndex < photos.length - 1) {
      setCurrentIndex(currentIndex + 1)
      setSelectedPhoto(photos[currentIndex + 1])
    }
  }

  const prevPhoto = () => {
    if (currentIndex > 0) {
      setCurrentIndex(currentIndex - 1)
      setSelectedPhoto(photos[currentIndex - 1])
    }
  }

  const getPhotoTypeLabel = (type: PhotoType) => {
    const labels = {
      [PhotoType.MoveIn]: 'Move-In',
      [PhotoType.MoveOut]: 'Move-Out',
      [PhotoType.Damage]: 'Damage',
      [PhotoType.Before]: 'Before Repair',
      [PhotoType.After]: 'After Repair'
    }
    return labels[type] || 'Unknown'
  }

  const getPhotoTypeBadgeColor = (type: PhotoType) => {
    const colors = {
      [PhotoType.MoveIn]: 'bg-blue-100 text-blue-800',
      [PhotoType.MoveOut]: 'bg-purple-100 text-purple-800',
      [PhotoType.Damage]: 'bg-red-100 text-red-800',
      [PhotoType.Before]: 'bg-yellow-100 text-yellow-800',
      [PhotoType.After]: 'bg-green-100 text-green-800'
    }
    return colors[type] || 'bg-gray-100 text-gray-800'
  }

  const PhotoCard = ({ photo, index }: { photo: InspectionPhoto; index: number }) => (
    <div className="relative group cursor-pointer" onClick={() => openLightbox(photo, index)}>
      <div className="aspect-square relative overflow-hidden rounded-lg border border-gray-200 bg-gray-100">
        <img
          src={photo.photoUrl}
          alt={photo.caption || 'Inspection photo'}
          className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-200"
        />
        <div className="absolute inset-0 bg-black opacity-0 group-hover:opacity-20 transition-opacity"></div>
      </div>
      <div className="mt-2">
        <span className={`inline-block px-2 py-1 text-xs font-medium rounded ${getPhotoTypeBadgeColor(photo.photoType)}`}>
          {getPhotoTypeLabel(photo.photoType)}
        </span>
        {photo.caption && (
          <p className="mt-1 text-sm text-gray-600 line-clamp-2">{photo.caption}</p>
        )}
      </div>
      {canDelete && onDeletePhoto && (
        <button
          onClick={(e) => {
            e.stopPropagation()
            if (confirm('Are you sure you want to delete this photo?')) {
              onDeletePhoto(photo.id)
            }
          }}
          className="absolute top-2 right-2 bg-red-500 text-white p-2 rounded-full opacity-0 group-hover:opacity-100 transition-opacity hover:bg-red-600"
        >
          <Trash2 className="w-3 h-3" />
        </button>
      )}
    </div>
  )

  if (photos.length === 0) {
    return (
      <div className="text-center py-12 bg-gray-50 rounded-lg border border-gray-200">
        <p className="text-gray-500">No photos uploaded yet</p>
      </div>
    )
  }

  return (
    <div className="space-y-8">
      {/* Before/After Comparison View */}
      {(groupedPhotos.before.length > 0 || groupedPhotos.after.length > 0) && (
        <div className="bg-white rounded-lg border border-gray-200 p-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Before/After Comparison</h3>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {/* Before Photos */}
            <div>
              <h4 className="text-sm font-medium text-gray-700 mb-3">Before Repair</h4>
              {groupedPhotos.before.length > 0 ? (
                <div className="grid grid-cols-2 gap-3">
                  {groupedPhotos.before.map((photo, index) => (
                    <PhotoCard
                      key={photo.id}
                      photo={photo}
                      index={photos.indexOf(photo)}
                    />
                  ))}
                </div>
              ) : (
                <div className="aspect-square bg-gray-100 rounded-lg border-2 border-dashed border-gray-300 flex items-center justify-center">
                  <p className="text-gray-400 text-sm">No before photos</p>
                </div>
              )}
            </div>

            {/* After Photos */}
            <div>
              <h4 className="text-sm font-medium text-gray-700 mb-3">After Repair</h4>
              {groupedPhotos.after.length > 0 ? (
                <div className="grid grid-cols-2 gap-3">
                  {groupedPhotos.after.map((photo, index) => (
                    <PhotoCard
                      key={photo.id}
                      photo={photo}
                      index={photos.indexOf(photo)}
                    />
                  ))}
                </div>
              ) : (
                <div className="aspect-square bg-gray-100 rounded-lg border-2 border-dashed border-gray-300 flex items-center justify-center">
                  <p className="text-gray-400 text-sm">No after photos</p>
                </div>
              )}
            </div>
          </div>
        </div>
      )}

      {/* Other Photo Categories */}
      {groupedPhotos.moveIn.length > 0 && (
        <div>
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Move-In Photos</h3>
          <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
            {groupedPhotos.moveIn.map((photo, index) => (
              <PhotoCard
                key={photo.id}
                photo={photo}
                index={photos.indexOf(photo)}
              />
            ))}
          </div>
        </div>
      )}

      {groupedPhotos.moveOut.length > 0 && (
        <div>
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Move-Out Photos</h3>
          <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
            {groupedPhotos.moveOut.map((photo, index) => (
              <PhotoCard
                key={photo.id}
                photo={photo}
                index={photos.indexOf(photo)}
              />
            ))}
          </div>
        </div>
      )}

      {groupedPhotos.damage.length > 0 && (
        <div>
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Damage Photos</h3>
          <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
            {groupedPhotos.damage.map((photo, index) => (
              <PhotoCard
                key={photo.id}
                photo={photo}
                index={photos.indexOf(photo)}
              />
            ))}
          </div>
        </div>
      )}

      {/* Lightbox Modal */}
      {selectedPhoto && (
        <div className="fixed inset-0 bg-black bg-opacity-90 z-50 flex items-center justify-center p-4">
          <button
            onClick={closeLightbox}
            className="absolute top-4 right-4 text-white text-2xl hover:text-gray-300 z-10"
          >
            <X className="w-6 h-6" />
          </button>

          {currentIndex > 0 && (
            <button
              onClick={prevPhoto}
              className="absolute left-4 text-white text-3xl hover:text-gray-300 z-10"
            >
              <ChevronLeft className="w-8 h-8" />
            </button>
          )}

          {currentIndex < photos.length - 1 && (
            <button
              onClick={nextPhoto}
              className="absolute right-4 text-white text-3xl hover:text-gray-300 z-10"
            >
              <ChevronRight className="w-8 h-8" />
            </button>
          )}

          <div className="max-w-5xl w-full">
            <img
              src={selectedPhoto.photoUrl}
              alt={selectedPhoto.caption || 'Inspection photo'}
              className="w-full h-auto max-h-[80vh] object-contain"
            />
            <div className="mt-4 text-white">
              <span className={`inline-block px-3 py-1 text-sm font-medium rounded ${getPhotoTypeBadgeColor(selectedPhoto.photoType)}`}>
                {getPhotoTypeLabel(selectedPhoto.photoType)}
              </span>
              {selectedPhoto.caption && (
                <p className="mt-2 text-lg">{selectedPhoto.caption}</p>
              )}
              <p className="mt-1 text-sm text-gray-400">
                Photo {currentIndex + 1} of {photos.length}
              </p>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
