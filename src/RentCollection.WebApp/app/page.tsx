'use client'

import { useState } from 'react'
import Link from 'next/link'
import { MapPin, Bed, Bath, Search, Star, Heart, Filter } from 'lucide-react'
import { useVacantUnits } from '@/lib/hooks/usePublicListings'
import LoadingSpinner from '@/components/common/LoadingSpinner'
import Navbar from '@/components/layout/Navbar'
import Footer from '@/components/layout/Footer'

export default function PublicLandingPage() {
  const { units, loading, error } = useVacantUnits()
  const [searchTerm, setSearchTerm] = useState('')
  const [minPrice, setMinPrice] = useState<number | ''>('')
  const [maxPrice, setMaxPrice] = useState<number | ''>('')
  const [bedrooms, setBedrooms] = useState<number | ''>('')
  const [propertyType, setPropertyType] = useState<string>('all')
  const [rentalType, setRentalType] = useState<string>('all')
  const [mobileFilterOpen, setMobileFilterOpen] = useState(false)

  const filteredUnits = units.filter((unit) => {
    const matchesSearch =
      searchTerm === '' ||
      unit.propertyName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      unit.propertyLocation?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      unit.unitNumber.toLowerCase().includes(searchTerm.toLowerCase())

    const matchesMinPrice = minPrice === '' || unit.monthlyRent >= minPrice
    const matchesMaxPrice = maxPrice === '' || unit.monthlyRent <= maxPrice
    const matchesBedrooms = bedrooms === '' || unit.bedrooms === bedrooms

    // Rental type filtering (1=Rent, 2=Lease, 3=Both)
    const matchesRentalType =
      rentalType === 'all' ||
      (rentalType === 'rent' && (unit.rentalType === 1 || unit.rentalType === 3)) ||
      (rentalType === 'lease' && (unit.rentalType === 2 || unit.rentalType === 3))

    // Debug: Log first unit to check values
    if (units.indexOf(unit) === 0) {
      console.log('Filter Debug:', {
        minPrice,
        maxPrice,
        unitPrice: unit.monthlyRent,
        matchesMinPrice,
        matchesMaxPrice,
        rentalType: unit.rentalType
      })
    }

    return matchesSearch && matchesMinPrice && matchesMaxPrice && matchesBedrooms && matchesRentalType
  })

  return (
    <div className="min-h-screen bg-bg-light">
      <Navbar />

      {/* Main Content - Full Width */}
      <main className="w-full">
        {/* Search Bar Section */}
        <div className="bg-white border-b border-secondary/30">
          <div className="container mx-auto px-6 py-4">
            <div className="relative">
              <Search className="absolute left-4 top-1/2 transform -translate-y-1/2 text-primary/40" size={18} />
              <input
                type="text"
                placeholder="Search by property name or location..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full pl-12 pr-4 py-3 border-b-2 border-secondary/30 focus:border-accent outline-none transition-colors bg-transparent text-primary placeholder:text-primary/40"
              />
            </div>
          </div>
        </div>

        {/* Horizontal Filter Bar */}
        <div className="bg-white border-b border-secondary/30">
          <div className="container mx-auto px-6 py-4">
            {/* Property Type Filters */}
            <div className="flex flex-wrap items-center gap-3 mb-4">
              <button
                onClick={() => setPropertyType('all')}
                className={`px-4 py-2 rounded-full text-sm font-medium tracking-wide transition-all ${
                  propertyType === 'all'
                    ? 'bg-accent text-primary shadow-sm'
                    : 'bg-white text-primary/70 border border-secondary/50 hover:border-accent/50'
                }`}
              >
                All Properties
              </button>
              <button
                onClick={() => setPropertyType('apartment')}
                className={`px-4 py-2 rounded-full text-sm font-medium tracking-wide transition-all ${
                  propertyType === 'apartment'
                    ? 'bg-accent text-primary shadow-sm'
                    : 'bg-white text-primary/70 border border-secondary/50 hover:border-accent/50'
                }`}
              >
                Apartments
              </button>
              <button
                onClick={() => setPropertyType('house')}
                className={`px-4 py-2 rounded-full text-sm font-medium tracking-wide transition-all ${
                  propertyType === 'house'
                    ? 'bg-accent text-primary shadow-sm'
                    : 'bg-white text-primary/70 border border-secondary/50 hover:border-accent/50'
                }`}
              >
                Houses
              </button>
              <button
                onClick={() => setPropertyType('studio')}
                className={`px-4 py-2 rounded-full text-sm font-medium tracking-wide transition-all ${
                  propertyType === 'studio'
                    ? 'bg-accent text-primary shadow-sm'
                    : 'bg-white text-primary/70 border border-secondary/50 hover:border-accent/50'
                }`}
              >
                Studio
              </button>
            </div>

            {/* Bedrooms and More Filters */}
            <div className="flex flex-wrap items-center justify-between gap-3">
              <div className="flex items-center gap-3">
                <span className="text-sm font-medium text-primary/70 tracking-wide">Beds:</span>
                {['any', 1, 2, 3, 4].map((bed) => (
                  <button
                    key={bed}
                    onClick={() => setBedrooms(bed === 'any' ? '' : bed as number)}
                    className={`px-3 py-1.5 rounded-full text-sm font-medium tracking-wide transition-all ${
                      (bed === 'any' && bedrooms === '') || bedrooms === bed
                        ? 'bg-primary text-white'
                        : 'bg-white text-primary/70 border border-secondary/50 hover:border-primary/30'
                    }`}
                  >
                    {bed === 'any' ? 'Any' : bed === 4 ? '4+' : bed}
                  </button>
                ))}
                <button
                  onClick={() => setMobileFilterOpen(true)}
                  className="flex items-center gap-2 px-4 py-1.5 rounded-full text-sm font-medium text-primary/70 border border-secondary/50 hover:border-accent/50 transition-colors tracking-wide"
                >
                  <Filter size={14} />
                  More Filters
                </button>
              </div>
              <span className="text-sm font-medium text-primary/50 tracking-wide">
                {filteredUnits.length} properties
              </span>
            </div>
          </div>
        </div>

        {/* Properties Section */}
        <div className="container mx-auto px-6 py-8">

        {loading ? (
          <div className="flex justify-center items-center py-20">
            <LoadingSpinner size="lg" />
          </div>
        ) : error ? (
          <div className="bg-red-50 border border-red-200 rounded-xl p-8 text-center">
            <p className="text-red-600 font-semibold text-lg">{error}</p>
            <p className="text-gray-600 mt-2">Please try again later</p>
          </div>
        ) : filteredUnits.length === 0 ? (
          <div className="bg-gray-50 border border-gray-200 rounded-xl p-16 text-center">
            <p className="text-gray-600 text-xl mb-4">No properties match your search</p>
            <button
              onClick={() => {
                setSearchTerm('')
                setMinPrice('')
                setMaxPrice('')
                setBedrooms('')
                setPropertyType('all')
                setRentalType('all')
              }}
              className="text-primary-600 font-semibold hover:underline text-lg"
            >
              Clear filters
            </button>
          </div>
        ) : (
          <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 2xl:grid-cols-6 gap-6">
            {filteredUnits.map((unit) => (
              <Link
                key={unit.id}
                href={`/units/${unit.id}/apply`}
                className="group bg-white rounded-lg overflow-hidden hover:shadow-lg transition-all duration-300 border border-secondary/30 hover:border-accent/20"
              >
                {/* Image Placeholder */}
                <div className="relative aspect-[4/3] overflow-hidden bg-secondary/30">
                  <div className="absolute inset-0 bg-gradient-to-br from-primary/5 via-primary/10 to-primary/20 flex items-center justify-center">
                    <span className="text-primary/10 text-4xl font-display font-normal tracking-wider">
                      {unit.bedrooms}BR
                    </span>
                  </div>
                  <button className="absolute top-3 right-3 p-2 bg-white/95 rounded-full hover:bg-white transition-colors">
                    <Heart className="text-primary/40 hover:text-accent transition-colors" size={15} />
                  </button>
                  <div className="absolute bottom-3 left-3">
                    <span className="bg-white/95 px-2.5 py-1 rounded text-[10px] font-medium text-primary tracking-wider">
                      {unit.unitNumber}
                    </span>
                  </div>
                </div>

                {/* Property Details */}
                <div className="p-4">
                  <div className="flex items-start justify-between mb-2">
                    <h3 className="font-serif font-medium text-primary group-hover:text-accent transition-colors text-sm leading-relaxed tracking-wide line-clamp-1 flex-1">
                      {unit.propertyName}
                    </h3>
                    <div className="flex items-center text-[10px] ml-2 flex-shrink-0">
                      <Star className="text-accent/80 mr-0.5 fill-accent/80" size={12} />
                      <span className="font-normal text-primary/70">4.8</span>
                    </div>
                  </div>
                  <p className="text-primary/50 text-[11px] mb-3 flex items-center line-clamp-1 leading-relaxed tracking-wide">
                    <MapPin className="mr-1 text-primary/30 flex-shrink-0" size={11} />
                    <span>{unit.propertyLocation || 'Nairobi, Kenya'}</span>
                  </p>
                  <div className="flex items-center gap-3 text-[11px] text-primary/50 mb-3 tracking-wide">
                    <span className="flex items-center gap-1">
                      <Bed className="text-primary/40" size={13} />
                      <span className="font-normal">{unit.bedrooms}</span>
                    </span>
                    <span className="flex items-center gap-1">
                      <Bath className="text-primary/40" size={13} />
                      <span className="font-normal">{unit.bathrooms}</span>
                    </span>
                  </div>
                  <div className="pt-3 border-t border-secondary/30">
                    <p className="text-accent font-serif font-normal text-base tracking-wide leading-tight">
                      KES {unit.monthlyRent.toLocaleString()}
                    </p>
                    <p className="text-[10px] text-primary/40 font-normal tracking-wider mt-0.5">per month</p>
                  </div>
                </div>
              </Link>
            ))}
          </div>
        )}
        </div>

        {/* More Filters Modal */}
        {mobileFilterOpen && (
          <div className="fixed inset-0 z-50 bg-black/50 backdrop-blur-sm" onClick={() => setMobileFilterOpen(false)}>
            <div className="absolute right-0 top-0 bottom-0 w-full max-w-md bg-white shadow-2xl overflow-y-auto" onClick={(e) => e.stopPropagation()}>
              <div className="p-6">
                <div className="flex items-center justify-between mb-6">
                  <h3 className="text-xl font-serif font-medium text-primary tracking-wide">More Filters</h3>
                  <button
                    onClick={() => setMobileFilterOpen(false)}
                    className="p-2 hover:bg-secondary/30 rounded-full transition-colors"
                  >
                    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                    </svg>
                  </button>
                </div>

                {/* Rental Type */}
                <div className="mb-6">
                  <label className="block text-sm font-medium text-primary/70 mb-3 tracking-wide">Rental Type</label>
                  <div className="flex gap-2">
                    {[
                      { value: 'all', label: 'All' },
                      { value: 'rent', label: 'Rent' },
                      { value: 'lease', label: 'Lease' },
                    ].map((type) => (
                      <button
                        key={type.value}
                        onClick={() => setRentalType(type.value)}
                        className={`flex-1 px-3 py-2 rounded-lg text-sm font-medium tracking-wide transition-all ${
                          rentalType === type.value
                            ? 'bg-accent text-primary'
                            : 'bg-white text-primary/70 border border-secondary/50'
                        }`}
                      >
                        {type.label}
                      </button>
                    ))}
                  </div>
                </div>

                {/* Price Range */}
                <div className="mb-6">
                  <label className="block text-sm font-medium text-primary/70 mb-3 tracking-wide">
                    Price Range (KES per month)
                  </label>
                  <div className="flex gap-3">
                    <input
                      type="number"
                      placeholder="Min"
                      value={minPrice}
                      onChange={(e) => setMinPrice(e.target.value === '' ? '' : Number(e.target.value))}
                      className="flex-1 px-3 py-2 border-b-2 border-secondary/30 focus:border-accent outline-none transition-colors bg-transparent text-primary"
                    />
                    <span className="text-primary/50">—</span>
                    <input
                      type="number"
                      placeholder="Max"
                      value={maxPrice}
                      onChange={(e) => setMaxPrice(e.target.value === '' ? '' : Number(e.target.value))}
                      className="flex-1 px-3 py-2 border-b-2 border-secondary/30 focus:border-accent outline-none transition-colors bg-transparent text-primary"
                    />
                  </div>
                </div>

                {/* Clear Filters Button */}
                <button
                  onClick={() => {
                    setSearchTerm('')
                    setMinPrice('')
                    setMaxPrice('')
                    setBedrooms('')
                    setPropertyType('all')
                    setRentalType('all')
                    setMobileFilterOpen(false)
                  }}
                  className="w-full px-4 py-3 bg-primary text-white rounded-lg font-medium tracking-wide hover:bg-primary/90 transition-colors"
                >
                  Clear All Filters
                </button>
              </div>
            </div>
          </div>
        )}
      </main>

      <Footer />
    </div>
  )
}
