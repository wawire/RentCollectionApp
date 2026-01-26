'use client'

import { useEffect } from 'react'
import { AlertCircle, AlertTriangle, CheckCircle2, Info, X } from 'lucide-react'

export interface ToastProps {
  id: string
  type: 'success' | 'error' | 'info' | 'warning'
  message: string
  duration?: number
  onClose: (id: string) => void
}

export default function Toast({ id, type, message, duration = 5000, onClose }: ToastProps) {
  useEffect(() => {
    if (duration > 0) {
      const timer = setTimeout(() => {
        onClose(id)
      }, duration)

      return () => clearTimeout(timer)
    }
  }, [id, duration, onClose])

  const getStyles = () => {
    switch (type) {
      case 'success':
        return {
          bg: 'bg-state-success/10',
          border: 'border-state-success/30',
          text: 'text-text-primary',
          icon: <CheckCircle2 className="text-state-success w-4 h-4" />,
        }
      case 'error':
        return {
          bg: 'bg-state-error/10',
          border: 'border-state-error/30',
          text: 'text-text-primary',
          icon: <AlertCircle className="text-state-error w-4 h-4" />,
        }
      case 'warning':
        return {
          bg: 'bg-state-warning/10',
          border: 'border-state-warning/30',
          text: 'text-text-primary',
          icon: <AlertTriangle className="text-state-warning w-4 h-4" />,
        }
      case 'info':
        return {
          bg: 'bg-state-info/10',
          border: 'border-state-info/30',
          text: 'text-text-primary',
          icon: <Info className="text-state-info w-4 h-4" />,
        }
    }
  }

  const styles = getStyles()

  return (
    <div
      className={`${styles.bg} ${styles.border} ${styles.text} border rounded-lg shadow-subtle p-4 mb-3 flex items-start gap-3 animate-slide-in-right max-w-md`}
      role="alert"
      aria-live="assertive"
      aria-atomic="true"
    >
      <div className="flex-shrink-0 mt-0.5">{styles.icon}</div>
      <p className="flex-1 text-sm font-medium">{message}</p>
      <button
        onClick={() => onClose(id)}
        className={`flex-shrink-0 ${styles.text} hover:opacity-70 transition-opacity`}
        aria-label="Close notification"
      >
        <X className="w-4 h-4" />
      </button>
    </div>
  )
}
