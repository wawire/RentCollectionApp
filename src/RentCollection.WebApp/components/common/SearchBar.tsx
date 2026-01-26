'use client'

import { useState } from 'react'
import { Search, X } from 'lucide-react'

interface SearchBarProps {
  value?: string
  onChange: (value: string) => void
  placeholder?: string
  onSearch?: () => void
  className?: string
}

export default function SearchBar({
  value: controlledValue,
  onChange,
  placeholder = 'Search...',
  onSearch,
  className = '',
}: SearchBarProps) {
  const [internalValue, setInternalValue] = useState('')
  const value = controlledValue !== undefined ? controlledValue : internalValue

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = e.target.value
    if (controlledValue === undefined) {
      setInternalValue(newValue)
    }
    onChange(newValue)
  }

  const handleClear = () => {
    if (controlledValue === undefined) {
      setInternalValue('')
    }
    onChange('')
  }

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter' && onSearch) {
      onSearch()
    }
  }

  return (
    <div className={`relative ${className}`}>
      <div className="absolute left-0 bottom-3 pointer-events-none">
        <Search className="text-primary/40" />
      </div>
      <input
        type="text"
        value={value}
        onChange={handleChange}
        onKeyDown={handleKeyDown}
        placeholder={placeholder}
        className="w-full pl-7 pr-10 py-3 border-b-2 border-secondary/30 focus:border-accent outline-none transition-colors bg-transparent text-primary placeholder:text-primary/40"
      />
      {value && (
        <button
          onClick={handleClear}
          className="absolute right-0 bottom-3 flex items-center text-primary/40 hover:text-primary/60 transition-colors"
        >
          <X />
        </button>
      )}
    </div>
  )
}
