import Link from 'next/link'
import { FaChevronRight, FaHome } from 'react-icons/fa'

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
        className="text-gray-500 hover:text-gray-700 transition-colors"
        aria-label="Home"
      >
        <FaHome className="text-base" />
      </Link>

      {items.map((item, index) => {
        const isLast = index === items.length - 1

        return (
          <div key={index} className="flex items-center space-x-2">
            <FaChevronRight className="text-gray-400 text-xs" />
            {item.href && !isLast ? (
              <Link
                href={item.href}
                className="text-gray-500 hover:text-gray-700 transition-colors"
              >
                {item.label}
              </Link>
            ) : (
              <span
                className={`${isLast ? 'text-gray-900 font-medium' : 'text-gray-500'}`}
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
