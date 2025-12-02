'use client'

import { FaMapMarkerAlt, FaTimes } from 'react-icons/fa'

interface FilterSidebarProps {
  searchTerm: string
  setSearchTerm: (value: string) => void
  minPrice: number | ''
  setMinPrice: (value: number | '') => void
  maxPrice: number | ''
  setMaxPrice: (value: number | '') => void
  bedrooms: number | ''
  setBedrooms: (value: number | '') => void
  propertyType: string
  setPropertyType: (value: string) => void
  onClose?: () => void
  isMobile?: boolean
}

export default function FilterSidebar(props: FilterSidebarProps) {
  const {
    searchTerm, setSearchTerm, minPrice, setMinPrice,
    maxPrice, setMaxPrice, bedrooms, setBedrooms,
    propertyType, setPropertyType, onClose, isMobile = false
  } = props

  const propertyTypes = [
    { id: 'all', label: 'All Types', icon: '🏠' },
    { id: 'apartment', label: 'Apartment', icon: '🏢' },
    { id: 'house', label: 'House', icon: '🏡' },
    { id: 'studio', label: 'Studio', icon: '🛏️' },
  ]

  const bedroomOptions = [
    { value: '', label: 'Any' },
    { value: 1, label: '1' },
    { value: 2, label: '2' },
    { value: 3, label: '3' },
    { value: 4, label: '4+' },
  ]

  const clearFilters = () => {
    setSearchTerm('')
    setMinPrice('')
    setMaxPrice('')
    setBedrooms('')
    setPropertyType('all')
  }

  const hasActiveFilters = searchTerm || minPrice || maxPrice || bedrooms || propertyType !== 'all'

  return (
    <div className="h-full bg-white border-r border-gray-200 overflow-y-auto">
      <div className="p-6">
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-xl font-bold text-gray-900">Filters</h2>
          {isMobile && (
            <button onClick={onClose} className="p-2 hover:bg-gray-100 rounded-lg">
              <FaTimes className="text-gray-600" />
            </button>
          )}
        </div>

        <div className="mb-6">
          <label className="block text-sm font-semibold text-gray-700 mb-3">
            Location
          </label>
          <div className="relative">
            <FaMapMarkerAlt className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" />
            <input
              type="text"
              placeholder="Search location..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full pl-10 pr-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
            />
          </div>
        </div>

        <div className="mb-6">
          <label className="block text-sm font-semibold text-gray-700 mb-3">
            Property Type
          </label>
          <div className="space-y-2">
            {propertyTypes.map((type) => (
              <button
                key={type.id}
                onClick={() => setPropertyType(type.id)}
                className={'w-full flex items-center space-x-3 p-3 rounded-lg transition-colors ' + (
                  propertyType === type.id
                    ? 'bg-primary-50 border-2 border-primary-600'
                    : 'bg-gray-50 border-2 border-transparent hover:bg-gray-100'
                )}
              >
                <span className="text-2xl">{type.icon}</span>
                <span className={'font-medium ' + (propertyType === type.id ? 'text-primary-700' : 'text-gray-700')}>
                  {type.label}
                </span>
              </button>
            ))}
          </div>
        </div>

        <div className="mb-6">
          <label className="block text-sm font-semibold text-gray-700 mb-3">
            Price Range (KES)
          </label>
          <div className="space-y-3">
            <input
              type="number"
              placeholder="Min price"
              value={minPrice}
              onChange={(e) => setMinPrice(e.target.value === '' ? '' : Number(e.target.value))}
              className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
            />
            <input
              type="number"
              placeholder="Max price"
              value={maxPrice}
              onChange={(e) => setMaxPrice(e.target.value === '' ? '' : Number(e.target.value))}
              className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
            />
          </div>
        </div>

        <div className="mb-6">
          <label className="block text-sm font-semibold text-gray-700 mb-3">
            Bedrooms
          </label>
          <div className="grid grid-cols-5 gap-2">
            {bedroomOptions.map((option) => (
              <button
                key={option.value}
                onClick={() => setBedrooms(option.value as any)}
                className={'p-3 rounded-lg font-medium transition-colors ' + (
                  bedrooms === option.value
                    ? 'bg-primary-600 text-white'
                    : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                )}
              >
                {option.label}
              </button>
            ))}
          </div>
        </div>

        {hasActiveFilters && (
          <button
            onClick={clearFilters}
            className="w-full py-3 border-2 border-gray-300 rounded-lg font-semibold text-gray-700 hover:bg-gray-50 transition-colors"
          >
            Clear All Filters
          </button>
        )}
      </div>
    </div>
  )
}
