import React from 'react'

interface BadgeProps {
  children: React.ReactNode
  variant?: 'default' | 'primary' | 'success' | 'warning' | 'danger' | 'info'
  size?: 'sm' | 'md' | 'lg'
}

const Badge: React.FC<BadgeProps> = ({
  children,
  variant = 'default',
  size = 'md',
}) => {
  const variantClasses = {
    default: 'bg-brand-bg text-text-secondary',
    primary: 'bg-brand-secondary/15 text-brand-primary',
    success: 'bg-state-success/15 text-state-success',
    warning: 'bg-state-warning/15 text-state-warning',
    danger: 'bg-state-error/15 text-state-error',
    info: 'bg-state-info/15 text-state-info',
  }

  const sizeClasses = {
    sm: 'px-2 py-0.5 text-xs',
    md: 'px-2.5 py-1 text-sm',
    lg: 'px-3 py-1.5 text-base',
  }

  return (
    <span
      className={`inline-flex items-center font-medium rounded-full ${variantClasses[variant]} ${sizeClasses[size]}`}
    >
      {children}
    </span>
  )
}

export default Badge
