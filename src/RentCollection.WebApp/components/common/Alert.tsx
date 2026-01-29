import { AlertCircle, AlertTriangle, CheckCircle2, Info, X } from 'lucide-react'

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
      icon: CheckCircle2,
      bgColor: 'bg-state-success/10',
      borderColor: 'border-state-success/30',
      iconColor: 'text-state-success',
      textColor: 'text-text-secondary',
      titleColor: 'text-text-primary',
    },
    error: {
      icon: AlertCircle,
      bgColor: 'bg-state-error/10',
      borderColor: 'border-state-error/30',
      iconColor: 'text-state-error',
      textColor: 'text-text-secondary',
      titleColor: 'text-text-primary',
    },
    warning: {
      icon: AlertTriangle,
      bgColor: 'bg-state-warning/10',
      borderColor: 'border-state-warning/30',
      iconColor: 'text-state-warning',
      textColor: 'text-text-secondary',
      titleColor: 'text-text-primary',
    },
    info: {
      icon: Info,
      bgColor: 'bg-state-info/10',
      borderColor: 'border-state-info/30',
      iconColor: 'text-state-info',
      textColor: 'text-text-secondary',
      titleColor: 'text-text-primary',
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
            className={`${textColor} hover:text-text-primary transition-colors ml-3 flex-shrink-0`}
          >
            <X className="w-4 h-4" />
          </button>
        )}
      </div>
    </div>
  )
}
