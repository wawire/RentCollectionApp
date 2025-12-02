'use client'

import { useState } from 'react'
import { MapPin, X, ChevronDown, ChevronUp, Home, Building2, DoorOpen, Wifi, Car, Zap, Droplets, Wind, Flame, Shield } from 'lucide-react'

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

  const [amenitiesOpen, setAmenitiesOpen] = useState(true)
  const [rentalTypeOpen, setRentalTypeOpen] = useState(true)
  const [selectedAmenities, setSelectedAmenities] = useState<string[]>([])
  const [rentalType, setRentalType] = useState<string>('all')

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
    { id: 'all', label: 'All', description: 'Show both leasing and renting options' },
    { id: 'rent', label: 'For Rent', description: 'Monthly payments, flexible terms' },
    { id: 'lease', label: 'For Lease', description: '6-12 month commitment' },
  ]

  const bedroomOptions = [
    { value: '', label: 'Any' },
    { value: 1, label: '1' },
    { value: 2, label: '2' },
    { value: 3, label: '3' },
    { value: 4, label: '4' },
    { value: 5, label: '5+' },
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
  }

  const hasActiveFilters = searchTerm || minPrice || maxPrice || bedrooms ||
    propertyType !== 'all' || selectedAmenities.length > 0 || rentalType !== 'all'

  const FilterSection = ({ title, children, isOpen, toggle }: any) => (
    <div className="border-b border-gray-200 pb-4 mb-4">
      <button
        onClick={toggle}
        className="w-full flex items-center justify-between py-2 hover:text-primary-600 transition-colors"
      >
        <h3 className="text-sm font-bold text-gray-900 uppercase tracking-wide">{title}</h3>
        {isOpen ? <ChevronUp size={18} /> : <ChevronDown size={18} />}
      </button>
      {isOpen && <div className="mt-4">{children}</div>}
    </div>
  )

  return (
    <div className="h-full bg-white border-r border-gray-200 overflow-y-auto">
      <div className="p-5">
        {/* Header */}
        <div className="flex items-center justify-between mb-6 pb-4 border-b border-gray-200">
          <h2 className="text-lg font-bold text-gray-900">Filter Properties</h2>
          {isMobile && (
            <button onClick={onClose} className="p-2 hover:bg-gray-100 rounded-lg transition-colors">
              <X size={20} className="text-gray-600" />
            </button>
          )}
        </div>

        {/* Location Search */}
        <div className="mb-6">
          <label className="block text-xs font-bold text-gray-700 mb-2 uppercase tracking-wide">
            Location
          </label>
          <div className="relative">
            <MapPin className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" size={18} />
            <input
              type="text"
              placeholder="Enter location..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full pl-10 pr-4 py-2.5 text-sm border border-gray-300 rounded-md focus:ring-2 focus:ring-primary-500 focus:border-transparent transition-all"
            />
          </div>
        </div>

        {/* Rental Type (Kenyan Standard) */}
        <FilterSection title="Rental Type" isOpen={rentalTypeOpen} toggle={() => setRentalTypeOpen(!rentalTypeOpen)}>
          <div className="space-y-2">
            {rentalTypes.map((type) => (
              <label key={type.id} className="flex items-start cursor-pointer group">
                <input
                  type="radio"
                  name="rentalType"
                  checked={rentalType === type.id}
                  onChange={() => setRentalType(type.id)}
                  className="mt-0.5 h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
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
        <div className="mb-6">
          <label className="block text-xs font-bold text-gray-700 mb-3 uppercase tracking-wide">
            Property Type
          </label>
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
        </div>

        {/* Price Range */}
        <div className="mb-6">
          <label className="block text-xs font-bold text-gray-700 mb-3 uppercase tracking-wide">
            Monthly Rent (KES)
          </label>
          <div className="grid grid-cols-2 gap-3">
            <input
              type="number"
              placeholder="Min"
              value={minPrice}
              onChange={(e) => setMinPrice(e.target.value === '' ? '' : Number(e.target.value))}
              className="px-3 py-2.5 text-sm border border-gray-300 rounded-md focus:ring-2 focus:ring-primary-500 focus:border-transparent"
            />
            <input
              type="number"
              placeholder="Max"
              value={maxPrice}
              onChange={(e) => setMaxPrice(e.target.value === '' ? '' : Number(e.target.value))}
              className="px-3 py-2.5 text-sm border border-gray-300 rounded-md focus:ring-2 focus:ring-primary-500 focus:border-transparent"
            />
          </div>
        </div>

        {/* Bedrooms */}
        <div className="mb-6">
          <label className="block text-xs font-bold text-gray-700 mb-3 uppercase tracking-wide">
            Bedrooms
          </label>
          <div className="grid grid-cols-6 gap-2">
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
        </div>

        {/* Amenities */}
        <FilterSection title="Amenities" isOpen={amenitiesOpen} toggle={() => setAmenitiesOpen(!amenitiesOpen)}>
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

        {/* Clear Filters Button */}
        {hasActiveFilters && (
          <button
            onClick={clearFilters}
            className="w-full mt-6 py-3 bg-gray-100 hover:bg-gray-200 border border-gray-300 rounded-lg font-semibold text-sm text-gray-700 transition-colors"
          >
            Clear All Filters ({
              (searchTerm ? 1 : 0) +
              (minPrice || maxPrice ? 1 : 0) +
              (bedrooms ? 1 : 0) +
              (propertyType !== 'all' ? 1 : 0) +
              selectedAmenities.length +
              (rentalType !== 'all' ? 1 : 0)
            })
          </button>
        )}

        {/* Vacancy Status Badge */}
        <div className="mt-6 p-3 bg-green-50 border border-green-200 rounded-lg">
          <p className="text-xs font-semibold text-green-800 flex items-center">
            <span className="w-2 h-2 bg-green-500 rounded-full mr-2"></span>
            Showing vacant properties only
          </p>
        </div>
      </div>
    </div>
  )
}
