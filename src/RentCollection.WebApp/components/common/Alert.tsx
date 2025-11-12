import { FaCheckCircle, FaExclamationCircle, FaInfoCircle, FaTimes, FaExclamationTriangle } from 'react-icons/fa'

interface AlertProps {
  type: 'success' | 'error' | 'warning' | 'info'
  title?: string
  message: string
  onClose?: () => void
  className?: string
}

export default function Alert({ type, title, message, onClose, className = '' }: AlertProps) {
  const config = {
    success: {
      icon: FaCheckCircle,
      bgColor: 'bg-green-50',
      borderColor: 'border-green-200',
      iconColor: 'text-green-500',
      textColor: 'text-green-800',
      titleColor: 'text-green-900',
    },
    error: {
      icon: FaExclamationCircle,
      bgColor: 'bg-red-50',
      borderColor: 'border-red-200',
      iconColor: 'text-red-500',
      textColor: 'text-red-800',
      titleColor: 'text-red-900',
    },
    warning: {
      icon: FaExclamationTriangle,
      bgColor: 'bg-yellow-50',
      borderColor: 'border-yellow-200',
      iconColor: 'text-yellow-500',
      textColor: 'text-yellow-800',
      titleColor: 'text-yellow-900',
    },
    info: {
      icon: FaInfoCircle,
      bgColor: 'bg-blue-50',
      borderColor: 'border-blue-200',
      iconColor: 'text-blue-500',
      textColor: 'text-blue-800',
      titleColor: 'text-blue-900',
    },
  }

  const { icon: Icon, bgColor, borderColor, iconColor, textColor, titleColor } = config[type]

  return (
    <div className={`${bgColor} ${borderColor} border rounded-lg p-4 ${className}`}>
      <div className="flex items-start">
        <Icon className={`${iconColor} w-5 h-5 mt-0.5 flex-shrink-0`} />
        <div className="ml-3 flex-1">
          {title && <h3 className={`text-sm font-semibold ${titleColor} mb-1`}>{title}</h3>}
          <p className={`text-sm ${textColor}`}>{message}</p>
        </div>
        {onClose && (
          <button
            onClick={onClose}
            className={`${textColor} hover:opacity-75 transition-opacity ml-3 flex-shrink-0`}
          >
            <FaTimes className="w-4 h-4" />
          </button>
        )}
      </div>
    </div>
  )
}
