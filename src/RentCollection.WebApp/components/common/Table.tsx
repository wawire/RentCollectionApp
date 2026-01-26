import React from 'react'

interface Column<T> {
  key: string
  header: string
  render?: (item: T) => React.ReactNode
  sortable?: boolean
}

interface TableProps<T> {
  columns: Column<T>[]
  data: T[]
  onSort?: (key: string) => void
  sortKey?: string
  sortOrder?: 'asc' | 'desc'
  loading?: boolean
  emptyMessage?: string
}

export default function Table<T extends { id?: number | string }>({
  columns,
  data,
  onSort,
  sortKey,
  sortOrder,
  loading = false,
  emptyMessage = 'No data available',
}: TableProps<T>) {
  const handleSort = (key: string, sortable?: boolean) => {
    if (sortable && onSort) {
      onSort(key)
    }
  }

  if (loading) {
    return (
      <div className="w-full overflow-x-auto">
        <table className="w-full border-collapse">
          <thead className="bg-brand-bg/70 border-b border-border-muted">
            <tr>
              {columns.map((col) => (
                <th key={col.key} className="px-6 py-3 text-left text-xs font-semibold text-text-muted uppercase tracking-wider">
                  {col.header}
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="bg-surface divide-y divide-border-muted">
            {[...Array(5)].map((_, idx) => (
              <tr key={idx} className="animate-pulse">
                {columns.map((col) => (
                  <td key={col.key} className="px-6 py-4 whitespace-nowrap">
                    <div className="h-4 bg-brand-bg rounded w-3/4"></div>
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    )
  }

  if (data.length === 0) {
    return (
      <div className="w-full overflow-x-auto">
        <table className="w-full border-collapse">
          <thead className="bg-brand-bg/70 border-b border-border-muted">
            <tr>
              {columns.map((col) => (
                <th key={col.key} className="px-6 py-3 text-left text-xs font-semibold text-text-muted uppercase tracking-wider">
                  {col.header}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            <tr>
              <td colSpan={columns.length} className="px-6 py-12 text-center text-text-muted">
                {emptyMessage}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    )
  }

  return (
    <div className="w-full overflow-x-auto">
      <table className="w-full border-collapse">
        <thead className="bg-brand-bg/70 border-b border-border-muted">
          <tr>
            {columns.map((col) => (
              <th
                key={col.key}
                onClick={() => handleSort(col.key, col.sortable)}
                className={`px-6 py-3 text-left text-xs font-semibold text-text-muted uppercase tracking-wider ${
                  col.sortable ? 'cursor-pointer hover:bg-brand-bg/90' : ''
                }`}
              >
                <div className="flex items-center space-x-1">
                  <span>{col.header}</span>
                  {col.sortable && sortKey === col.key && (
                    <span className="text-brand-secondary">
                      {sortOrder === 'asc' ? '^' : 'v'}
                    </span>
                  )}
                </div>
              </th>
            ))}
          </tr>
        </thead>
        <tbody className="bg-surface divide-y divide-border-muted">
          {data.map((item, idx) => (
            <tr key={item.id || idx} className="hover:bg-brand-bg/50 transition-colors">
              {columns.map((col) => (
                <td key={col.key} className="px-6 py-4 whitespace-nowrap text-sm text-text-primary">
                  {col.render ? col.render(item) : String((item as any)[col.key] || '-')}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
