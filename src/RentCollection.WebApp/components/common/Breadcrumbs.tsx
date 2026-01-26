import Link from 'next/link'
import { ChevronRight, Home } from 'lucide-react'

export interface BreadcrumbItem {
  label: string
  href?: string
}

interface BreadcrumbsProps {
  items: BreadcrumbItem[]
  className?: string
}

export default function Breadcrumbs({ items, className = '' }: BreadcrumbsProps) {
  return (
    <nav
      aria-label="Breadcrumb"
      className={`flex items-center space-x-2 text-sm ${className}`}
    >
      <Link
        href="/"
        className="text-text-muted hover:text-brand-secondary transition-colors"
        aria-label="Home"
      >
        <Home className="w-4 h-4" />
      </Link>

      {items.map((item, index) => {
        const isLast = index === items.length - 1

        return (
          <div key={index} className="flex items-center space-x-2">
            <ChevronRight className="text-text-muted w-3.5 h-3.5" />
            {item.href && !isLast ? (
              <Link
                href={item.href}
                className="text-text-muted hover:text-brand-secondary transition-colors"
              >
                {item.label}
              </Link>
            ) : (
              <span
                className={`${isLast ? 'text-text-primary font-medium' : 'text-text-muted'}`}
                aria-current={isLast ? 'page' : undefined}
              >
                {item.label}
              </span>
            )}
          </div>
        )
      })}
    </nav>
  )
}
