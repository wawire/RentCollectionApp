import React from 'react'
import Breadcrumbs, { BreadcrumbItem } from './Breadcrumbs'

interface PageHeaderProps {
  title: string
  subtitle?: string
  actions?: React.ReactNode
  breadcrumbs?: BreadcrumbItem[]
  className?: string
}

export default function PageHeader({ title, subtitle, actions, breadcrumbs, className = '' }: PageHeaderProps) {
  return (
    <div className={`space-y-3 ${className}`}>
      {breadcrumbs && breadcrumbs.length > 0 && (
        <Breadcrumbs items={breadcrumbs} />
      )}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-3xl font-semibold text-text-primary">{title}</h1>
          {subtitle && <p className="text-text-secondary mt-1">{subtitle}</p>}
        </div>
        {actions && <div className="flex items-center gap-3">{actions}</div>}
      </div>
    </div>
  )
}
