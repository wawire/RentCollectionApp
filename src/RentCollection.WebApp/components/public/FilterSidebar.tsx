'use client'

import { useState, useEffect } from 'react'
import { MapPin, X, ChevronDown, ChevronUp, Home, Building2, DoorOpen, Wifi, Car, Zap, Droplets, Wind, Flame, Shield } from 'lucide-react'
import Slider from 'rc-slider'
import 'rc-slider/assets/index.css'

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
  rentalType: string
  setRentalType: (value: string) => void
  onClose?: () => void
  isMobile?: boolean
}

export default function FilterSidebar(props: FilterSidebarProps) {
  const {
    searchTerm, setSearchTerm, minPrice, setMinPrice,
    maxPrice, setMaxPrice, bedrooms, setBedrooms,
    propertyType, setPropertyType, rentalType, setRentalType,
    onClose, isMobile = false
  } = props

  // Client-side mounting state
  const [isMounted, setIsMounted] = useState(false)

  // Section collapse states
  const [priceOpen, setPriceOpen] = useState(true)
  const [bedroomsOpen, setBedroomsOpen] = useState(true)
  const [propertyTypeOpen, setPropertyTypeOpen] = useState(true)
  const [amenitiesOpen, setAmenitiesOpen] = useState(false)
  const [rentalTypeOpen, setRentalTypeOpen] = useState(true)

  // Slider state for price range
  const [priceRange, setPriceRange] = useState<[number, number]>([
    minPrice || 0,
    maxPrice || 150000
  ])

  // Ensure component is mounted before rendering slider
  useEffect(() => {
    setIsMounted(true)
  }, [])

  // Sync slider with filter state
  useEffect(() => {
    setPriceRange([minPrice || 0, maxPrice || 150000])
  }, [minPrice, maxPrice])

  const handlePriceChange = (values: number | number[]) => {
    const [min, max] = Array.isArray(values) ? values : [values, values]
    setPriceRange([min, max])
  }

  const handlePriceCommit = (values: number | number[]) => {
    const [min, max] = Array.isArray(values) ? values : [values, values]
    setMinPrice(min === 0 ? '' : min)
    setMaxPrice(max === 150000 ? '' : max)
  }

  const [selectedAmenities, setSelectedAmenities] = useState<string[]>([])

  const propertyTypes = [
    { id: 'all', label: 'All Properties', icon: Home },
    { id: 'apartment', label: 'Apartments', icon: Building2 },
    { id: 'house', label: 'Houses', icon: Home },
    { id: 'studio', label: 'Studios', icon: DoorOpen },
  ]

  const amenities = [
    { id: 'wifi', label: 'WiFi', icon: Wifi },
    { id: 'parking', label: 'Parking', icon: Car },
    { id: 'power_backup', label: 'Power Backup', icon: Zap },
    { id: 'water', label: 'Water Supply', icon: Droplets },
    { id: 'ac', label: 'Air Conditioning', icon: Wind },
    { id: 'gas', label: 'Gas Connection', icon: Flame },
    { id: 'security', label: 'Security', icon: Shield },
  ]

  const rentalTypes = [
    { id: 'all', label: 'All', description: 'Show all rental options' },
    { id: 'rent', label: 'For Rent', description: 'Monthly, flexible terms' },
    { id: 'lease', label: 'For Lease', description: '6-12 month commitment' },
  ]

  const bedroomOptions = [
    { value: '', label: 'Any' },
    { value: 1, label: '1 BR' },
    { value: 2, label: '2 BR' },
    { value: 3, label: '3 BR' },
    { value: 4, label: '4 BR' },
    { value: 5, label: '5+ BR' },
  ]

  const priceQuickFilters = [
    { label: 'Under 20K', min: 0, max: 20000 },
    { label: '20K-40K', min: 20000, max: 40000 },
    { label: '40K-60K', min: 40000, max: 60000 },
    { label: '60K+', min: 60000, max: 150000 },
  ]

  const toggleAmenity = (amenityId: string) => {
    setSelectedAmenities(prev =>
      prev.includes(amenityId)
        ? prev.filter(id => id !== amenityId)
        : [...prev, amenityId]
    )
  }

  const clearFilters = () => {
    setSearchTerm('')
    setMinPrice('')
    setMaxPrice('')
    setBedrooms('')
    setPropertyType('all')
    setSelectedAmenities([])
    setRentalType('all')
    setPriceRange([0, 150000])
  }

  const removeFilter = (filterType: string, value?: any) => {
    switch (filterType) {
      case 'search':
        setSearchTerm('')
        break
      case 'price':
        setMinPrice('')
        setMaxPrice('')
        setPriceRange([0, 150000])
        break
      case 'bedrooms':
        setBedrooms('')
        break
      case 'propertyType':
        setPropertyType('all')
        break
      case 'rentalType':
        setRentalType('all')
        break
      case 'amenity':
        setSelectedAmenities(prev => prev.filter(a => a !== value))
        break
    }
  }

  const hasActiveFilters = searchTerm || minPrice || maxPrice || bedrooms ||
    propertyType !== 'all' || selectedAmenities.length > 0 || rentalType !== 'all'

  const activeFilterCount =
    (searchTerm ? 1 : 0) +
    (minPrice || maxPrice ? 1 : 0) +
    (bedrooms ? 1 : 0) +
    (propertyType !== 'all' ? 1 : 0) +
    selectedAmenities.length +
    (rentalType !== 'all' ? 1 : 0)

  const FilterSection = ({ title, children, isOpen, toggle, count }: any) => (
    <div className="border-b border-gray-200 pb-4 mb-4">
      <button
        onClick={toggle}
        className="w-full flex items-center justify-between py-2 hover:text-primary-600 transition-colors group"
      >
        <div className="flex items-center gap-2">
          <h3 className="text-sm font-bold text-gray-900 uppercase tracking-wide">{title}</h3>
          {count > 0 && (
            <span className="bg-primary-600 text-white text-xs font-bold px-1.5 py-0.5 rounded-full">
              {count}
            </span>
          )}
        </div>
        {isOpen ? <ChevronUp size={18} /> : <ChevronDown size={18} />}
      </button>
      {isOpen && <div className="mt-4 animate-fadeIn">{children}</div>}
    </div>
  )

  return (
    <div className="h-full bg-white border-r border-gray-200 flex flex-col">
      <div className="flex-1 overflow-y-auto">
        <div className="p-5">
          {/* Header */}
          <div className="flex items-center justify-between mb-6 pb-4 border-b border-gray-200">
            <div className="flex items-center gap-2">
              <h2 className="text-lg font-bold text-gray-900">Filters</h2>
              {activeFilterCount > 0 && (
                <span className="bg-primary-600 text-white text-sm font-bold px-2 py-0.5 rounded-full">
                  {activeFilterCount}
                </span>
              )}
            </div>
            {isMobile && (
              <button onClick={onClose} className="p-2 hover:bg-gray-100 rounded-lg transition-colors">
                <X size={20} className="text-gray-600" />
              </button>
            )}
          </div>

          {/* Active Filters Chips */}
          {hasActiveFilters && (
            <div className="mb-6 pb-4 border-b border-gray-200">
              <div className="flex items-center justify-between mb-2">
                <p className="text-xs font-bold text-gray-700 uppercase tracking-wide">Active Filters</p>
                <button
                  onClick={clearFilters}
                  className="text-xs text-primary-600 hover:text-primary-700 font-medium"
                >
                  Clear All
                </button>
              </div>
              <div className="flex flex-wrap gap-2">
                {searchTerm && (
                  <button
                    onClick={() => removeFilter('search')}
                    className="inline-flex items-center gap-1 px-2 py-1 bg-primary-50 text-primary-700 rounded-full text-xs font-medium hover:bg-primary-100 transition-colors"
                  >
                    <span>Search: {searchTerm.substring(0, 15)}{searchTerm.length > 15 ? '...' : ''}</span>
                    <X size={12} />
                  </button>
                )}
                {(minPrice || maxPrice) && (
                  <button
                    onClick={() => removeFilter('price')}
                    className="inline-flex items-center gap-1 px-2 py-1 bg-primary-50 text-primary-700 rounded-full text-xs font-medium hover:bg-primary-100 transition-colors"
                  >
                    <span>
                      {minPrice || 0}K - {maxPrice ? `${maxPrice / 1000}K` : '150K+'}
                    </span>
                    <X size={12} />
                  </button>
                )}
                {bedrooms && (
                  <button
                    onClick={() => removeFilter('bedrooms')}
                    className="inline-flex items-center gap-1 px-2 py-1 bg-primary-50 text-primary-700 rounded-full text-xs font-medium hover:bg-primary-100 transition-colors"
                  >
                    <span>{bedrooms} {bedrooms === 1 ? 'Bedroom' : 'Bedrooms'}</span>
                    <X size={12} />
                  </button>
                )}
                {propertyType !== 'all' && (
                  <button
                    onClick={() => removeFilter('propertyType')}
                    className="inline-flex items-center gap-1 px-2 py-1 bg-primary-50 text-primary-700 rounded-full text-xs font-medium hover:bg-primary-100 transition-colors"
                  >
                    <span>{propertyTypes.find(t => t.id === propertyType)?.label}</span>
                    <X size={12} />
                  </button>
                )}
                {rentalType !== 'all' && (
                  <button
                    onClick={() => removeFilter('rentalType')}
                    className="inline-flex items-center gap-1 px-2 py-1 bg-primary-50 text-primary-700 rounded-full text-xs font-medium hover:bg-primary-100 transition-colors"
                  >
                    <span>{rentalTypes.find(t => t.id === rentalType)?.label}</span>
                    <X size={12} />
                  </button>
                )}
              </div>
            </div>
          )}

          {/* Location Search */}
          <div className="mb-6">
            <label className="block text-xs font-bold text-gray-700 mb-2 uppercase tracking-wide">
              Location
            </label>
            <div className="relative">
              <MapPin className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" size={18} />
              <input
                type="text"
                placeholder="Search location..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full pl-10 pr-4 py-2.5 text-sm border border-gray-300 rounded-md focus:ring-2 focus:ring-primary-500 focus:border-transparent transition-all"
              />
            </div>
          </div>

          {/* Price Range Slider */}
          <FilterSection
            title="Price Range (KES)"
            isOpen={priceOpen}
            toggle={() => setPriceOpen(!priceOpen)}
            count={(minPrice || maxPrice) ? 1 : 0}
          >
            <div className="px-2">
              {isMounted ? (
                <Slider
                  range
                  min={0}
                  max={150000}
                  step={1000}
                  value={priceRange}
                  onChange={handlePriceChange}
                  onAfterChange={handlePriceCommit}
                  trackStyle={{ backgroundColor: '#D4AF37' }}
                  handleStyle={{
                    borderColor: '#D4AF37',
                    backgroundColor: '#fff',
                    boxShadow: '0 2px 4px rgba(0,0,0,0.2)',
                  }}
                  railStyle={{ backgroundColor: '#DDE1E4' }}
                />
              ) : (
                <div className="h-6 bg-gray-100 rounded animate-pulse" />
              )}
              <div className="flex justify-between mt-4 mb-4">
                <div className="text-sm">
                  <span className="text-gray-500">Min:</span>{' '}
                  <span className="font-semibold text-gray-900">
                    {priceRange[0].toLocaleString()}
                  </span>
                </div>
                <div className="text-sm">
                  <span className="text-gray-500">Max:</span>{' '}
                  <span className="font-semibold text-gray-900">
                    {priceRange[1] === 150000 ? '150K+' : priceRange[1].toLocaleString()}
                  </span>
                </div>
              </div>
              {/* Quick Price Filters */}
              <div className="grid grid-cols-2 gap-2">
                {priceQuickFilters.map((filter) => (
                  <button
                    key={filter.label}
                    onClick={() => {
                      setPriceRange([filter.min, filter.max])
                      setMinPrice(filter.min === 0 ? '' : filter.min)
                      setMaxPrice(filter.max === 150000 ? '' : filter.max)
                    }}
                    className="px-3 py-1.5 text-xs border border-gray-300 rounded-md hover:bg-primary-50 hover:border-primary-600 hover:text-primary-700 transition-colors font-medium"
                  >
                    {filter.label}
                  </button>
                ))}
              </div>
            </div>
          </FilterSection>

          {/* Bedrooms */}
          <FilterSection
            title="Bedrooms"
            isOpen={bedroomsOpen}
            toggle={() => setBedroomsOpen(!bedroomsOpen)}
            count={bedrooms ? 1 : 0}
          >
            <div className="grid grid-cols-3 gap-2">
              {bedroomOptions.map((option) => (
                <button
                  key={option.value}
                  onClick={() => setBedrooms(option.value as any)}
                  className={`p-2.5 rounded-md font-semibold text-sm transition-all ${
                    bedrooms === option.value
                      ? 'bg-primary-600 text-white shadow-sm'
                      : 'bg-gray-100 text-gray-700 hover:bg-gray-200 border border-gray-200'
                  }`}
                >
                  {option.label}
                </button>
              ))}
            </div>
          </FilterSection>

          {/* Rental Type */}
          <FilterSection
            title="Rental Type"
            isOpen={rentalTypeOpen}
            toggle={() => setRentalTypeOpen(!rentalTypeOpen)}
            count={rentalType !== 'all' ? 1 : 0}
          >
            <div className="space-y-2">
              {rentalTypes.map((type) => (
                <label key={type.id} className="flex items-start cursor-pointer group">
                  <input
                    type="radio"
                    name="rentalType"
                    checked={rentalType === type.id}
                    onChange={() => setRentalType(type.id)}
                    className="mt-0.5 h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300"
                  />
                  <div className="ml-3">
                    <span className="text-sm font-medium text-gray-900 group-hover:text-primary-600 transition-colors">
                      {type.label}
                    </span>
                    <p className="text-xs text-gray-500 mt-0.5">{type.description}</p>
                  </div>
                </label>
              ))}
            </div>
          </FilterSection>

          {/* Property Type */}
          <FilterSection
            title="Property Type"
            isOpen={propertyTypeOpen}
            toggle={() => setPropertyTypeOpen(!propertyTypeOpen)}
            count={propertyType !== 'all' ? 1 : 0}
          >
            <div className="grid grid-cols-2 gap-2">
              {propertyTypes.map((type) => {
                const Icon = type.icon
                return (
                  <button
                    key={type.id}
                    onClick={() => setPropertyType(type.id)}
                    className={`flex flex-col items-center p-3 rounded-lg transition-all ${
                      propertyType === type.id
                        ? 'bg-primary-600 text-white shadow-md'
                        : 'bg-gray-50 text-gray-700 hover:bg-gray-100 border border-gray-200'
                    }`}
                  >
                    <Icon size={22} className="mb-1.5" />
                    <span className="text-xs font-medium">{type.label}</span>
                  </button>
                )
              })}
            </div>
          </FilterSection>

          {/* Amenities */}
          <FilterSection
            title="Amenities"
            isOpen={amenitiesOpen}
            toggle={() => setAmenitiesOpen(!amenitiesOpen)}
            count={selectedAmenities.length}
          >
            <div className="space-y-2.5">
              {amenities.map((amenity) => {
                const Icon = amenity.icon
                return (
                  <label key={amenity.id} className="flex items-center cursor-pointer group">
                    <input
                      type="checkbox"
                      checked={selectedAmenities.includes(amenity.id)}
                      onChange={() => toggleAmenity(amenity.id)}
                      className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
                    />
                    <Icon size={16} className="ml-3 mr-2 text-gray-500 group-hover:text-primary-600 transition-colors" />
                    <span className="text-sm text-gray-700 group-hover:text-gray-900 transition-colors">
                      {amenity.label}
                    </span>
                  </label>
                )
              })}
            </div>
          </FilterSection>
        </div>
      </div>

      {/* Mobile Bottom Action Bar */}
      {isMobile && (
        <div className="border-t border-gray-200 p-4 bg-white">
          <div className="flex gap-3">
            <button
              onClick={clearFilters}
              className="flex-1 py-3 border border-gray-300 rounded-lg font-semibold text-sm text-gray-700 hover:bg-gray-50 transition-colors"
            >
              Reset
            </button>
            <button
              onClick={onClose}
              className="flex-1 py-3 bg-primary-600 text-white rounded-lg font-semibold text-sm hover:bg-primary-700 transition-colors"
            >
              Apply Filters
            </button>
          </div>
        </div>
      )}
    </div>
  )
}
