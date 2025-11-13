interface SkeletonProps {
  className?: string
  variant?: 'text' | 'circular' | 'rectangular'
  width?: string | number
  height?: string | number
  count?: number
}

export default function Skeleton({
  className = '',
  variant = 'text',
  width,
  height,
  count = 1,
}: SkeletonProps) {
  const getVariantClasses = () => {
    switch (variant) {
      case 'circular':
        return 'rounded-full'
      case 'rectangular':
        return 'rounded'
      case 'text':
      default:
        return 'rounded'
    }
  }

  const getDefaultDimensions = () => {
    switch (variant) {
      case 'circular':
        return { width: width || '48px', height: height || '48px' }
      case 'rectangular':
        return { width: width || '100%', height: height || '128px' }
      case 'text':
      default:
        return { width: width || '100%', height: height || '20px' }
    }
  }

  const dimensions = getDefaultDimensions()
  const style = {
    width: typeof dimensions.width === 'number' ? `${dimensions.width}px` : dimensions.width,
    height: typeof dimensions.height === 'number' ? `${dimensions.height}px` : dimensions.height,
  }

  if (count > 1) {
    return (
      <div className="space-y-2">
        {Array.from({ length: count }).map((_, index) => (
          <div
            key={index}
            className={`bg-gray-200 animate-pulse ${getVariantClasses()} ${className}`}
            style={style}
            role="status"
            aria-label="Loading..."
          />
        ))}
      </div>
    )
  }

  return (
    <div
      className={`bg-gray-200 animate-pulse ${getVariantClasses()} ${className}`}
      style={style}
      role="status"
      aria-label="Loading..."
    />
  )
}

// Preset skeleton components
export function SkeletonCard({ className = '' }: { className?: string }) {
  return (
    <div className={`border border-gray-200 rounded-lg p-4 space-y-3 ${className}`}>
      <Skeleton height="24px" width="60%" />
      <Skeleton count={3} height="16px" />
      <div className="flex gap-2 pt-2">
        <Skeleton width="80px" height="32px" />
        <Skeleton width="80px" height="32px" />
      </div>
    </div>
  )
}

export function SkeletonTable({ rows = 5, columns = 4 }: { rows?: number; columns?: number }) {
  return (
    <div className="w-full">
      {/* Header */}
      <div className="flex gap-4 p-4 border-b border-gray-200">
        {Array.from({ length: columns }).map((_, i) => (
          <Skeleton key={`header-${i}`} width="120px" height="20px" />
        ))}
      </div>
      {/* Rows */}
      {Array.from({ length: rows }).map((_, rowIndex) => (
        <div key={`row-${rowIndex}`} className="flex gap-4 p-4 border-b border-gray-200">
          {Array.from({ length: columns }).map((_, colIndex) => (
            <Skeleton key={`cell-${rowIndex}-${colIndex}`} width="120px" height="20px" />
          ))}
        </div>
      ))}
    </div>
  )
}
