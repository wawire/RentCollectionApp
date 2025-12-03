'use client'

import { useState } from 'react'
import Link from 'next/link'
import { MapPin, Bed, Bath, Search, Star, Heart, Filter } from 'lucide-react'
import { useVacantUnits } from '@/lib/hooks/usePublicListings'
import LoadingSpinner from '@/components/common/LoadingSpinner'
import Navbar from '@/components/layout/Navbar'
import Footer from '@/components/layout/Footer'
import FilterSidebar from '@/components/public/FilterSidebar'

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

      {/* Property Type Horizontal Bar */}
      <div className="bg-white border-b border-secondary/30 sticky top-0 z-20">
        <div className="container mx-auto px-6 py-3">
          <div className="flex flex-wrap items-center gap-3">
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
        </div>
      </div>

      {/* Main Container with Sidebar */}
      <div className="flex">
        {/* Desktop Sidebar */}
        <aside className="hidden lg:block w-80 sticky top-16 h-screen">
          <FilterSidebar
            searchTerm={searchTerm}
            setSearchTerm={setSearchTerm}
            minPrice={minPrice}
            setMinPrice={setMinPrice}
            maxPrice={maxPrice}
            setMaxPrice={setMaxPrice}
            bedrooms={bedrooms}
            setBedrooms={setBedrooms}
            propertyType={propertyType}
            setPropertyType={setPropertyType}
            rentalType={rentalType}
            setRentalType={setRentalType}
          />
        </aside>

        {/* Mobile Filter Modal */}
        {mobileFilterOpen && (
          <div className="fixed inset-0 z-50 lg:hidden">
            <div className="absolute inset-0 bg-black bg-opacity-50" onClick={() => setMobileFilterOpen(false)} />
            <div className="absolute left-0 top-0 bottom-0 w-80 bg-white">
              <FilterSidebar
                searchTerm={searchTerm}
                setSearchTerm={setSearchTerm}
                minPrice={minPrice}
                setMinPrice={setMinPrice}
                maxPrice={maxPrice}
                setMaxPrice={setMaxPrice}
                bedrooms={bedrooms}
                setBedrooms={setBedrooms}
                propertyType={propertyType}
                setPropertyType={setPropertyType}
                rentalType={rentalType}
                setRentalType={setRentalType}
                onClose={() => setMobileFilterOpen(false)}
                isMobile={true}
              />
            </div>
          </div>
        )}

        {/* Main Content */}
        <main className="flex-1 min-h-screen">
          {/* Search Bar Section */}
          <div className="bg-white border-b border-gray-200 sticky top-16 z-10">
            <div className="container mx-auto px-6 py-4">
              <div className="flex gap-3">
                {/* Mobile Filter Button */}
                <button
                  onClick={() => setMobileFilterOpen(true)}
                  className="lg:hidden flex items-center gap-2 px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50"
                >
                  <Filter size={16} />
                  <span>Filters</span>
                </button>

                {/* Search Bar */}
                <div className="flex-1 relative">
                  <Search className="absolute left-4 top-1/2 transform -translate-y-1/2 text-gray-400" size={18} />
                  <input
                    type="text"
                    placeholder="Search by property name or location..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    className="w-full pl-12 pr-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                  />
                </div>
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
          <div className="space-y-12">
            {/* Chunk properties into groups of 12 */}
            {Array.from({ length: Math.ceil(filteredUnits.length / 12) }).map((_, chunkIndex) => {
              const startIndex = chunkIndex * 12
              const chunk = filteredUnits.slice(startIndex, startIndex + 12)
              const ctaType = chunkIndex % 3 // Cycle through 3 CTA types
              const showCTA = chunkIndex < Math.ceil(filteredUnits.length / 12) - 1 // Show CTA except after last chunk

              return (
                <div key={`chunk-${chunkIndex}`}>
                  {/* Property Grid */}
                  <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 2xl:grid-cols-6 gap-6">
                    {chunk.map((unit) => (
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

                  {/* CTA Section after chunk (except last one) */}
                  {showCTA && (
                    <>
                      {ctaType === 0 && (
                        <div className="mt-12 bg-gradient-to-br from-primary/5 to-accent/10 rounded-2xl p-8 border border-accent/20">
                          <div className="max-w-2xl mx-auto text-center">
                            <div className="w-16 h-16 mx-auto mb-4 rounded-full bg-accent/20 flex items-center justify-center">
                              <svg className="w-8 h-8 text-accent" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                              </svg>
                            </div>
                            <h3 className="text-2xl font-serif font-medium text-primary mb-3 tracking-wide">
                              Can't find what you're looking for?
                            </h3>
                            <p className="text-primary/70 mb-6 tracking-wide">
                              Let us help you find your perfect home. Our team can show you properties that match your exact needs.
                            </p>
                            <Link
                              href="/#contact"
                              className="inline-flex items-center gap-2 px-6 py-3 bg-accent hover:bg-accent-600 text-primary font-medium rounded-full transition-all shadow-sm tracking-wide"
                            >
                              Contact Us
                              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                              </svg>
                            </Link>
                          </div>
                        </div>
                      )}

                      {ctaType === 1 && (
                        <div className="mt-12 bg-white rounded-2xl p-8 border-2 border-accent/30 shadow-sm">
                          <div className="max-w-2xl mx-auto text-center">
                            <div className="w-16 h-16 mx-auto mb-4 rounded-full bg-primary flex items-center justify-center">
                              <svg className="w-8 h-8 text-accent" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
                              </svg>
                            </div>
                            <h3 className="text-2xl font-serif font-medium text-primary mb-3 tracking-wide">
                              Property Owner?
                            </h3>
                            <p className="text-primary/70 mb-6 tracking-wide">
                              List your property with RentCollection and reach thousands of verified tenants. Zero listing fees.
                            </p>
                            <Link
                              href="/#for-landlords"
                              className="inline-flex items-center gap-2 px-6 py-3 bg-primary hover:bg-primary/90 text-white font-medium rounded-full transition-all shadow-sm tracking-wide"
                            >
                              List Your Property
                              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                              </svg>
                            </Link>
                          </div>
                        </div>
                      )}

                      {ctaType === 2 && (
                        <div className="mt-12 bg-gradient-to-r from-accent/10 to-primary/5 rounded-2xl p-8 border border-secondary/30">
                          <div className="max-w-2xl mx-auto text-center">
                            <div className="w-16 h-16 mx-auto mb-4 rounded-full bg-white flex items-center justify-center shadow-sm">
                              <svg className="w-8 h-8 text-accent" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
                              </svg>
                            </div>
                            <h3 className="text-2xl font-serif font-medium text-primary mb-3 tracking-wide">
                              Get New Listings First
                            </h3>
                            <p className="text-primary/70 mb-6 tracking-wide">
                              Subscribe to receive instant notifications when properties matching your preferences become available.
                            </p>
                            <div className="flex gap-2 max-w-md mx-auto">
                              <input
                                type="email"
                                placeholder="Enter your email"
                                className="flex-1 px-4 py-3 border-b-2 border-secondary/30 focus:border-accent outline-none bg-transparent text-primary placeholder:text-primary/40 tracking-wide"
                              />
                              <button className="px-6 py-3 bg-accent hover:bg-accent-600 text-primary font-medium rounded-full transition-all shadow-sm tracking-wide whitespace-nowrap">
                                Subscribe
                              </button>
                            </div>
                          </div>
                        </div>
                      )}
                    </>
                  )}
                </div>
              )
            })}
          </div>
        )}
          </div>
        </main>
      </div>

      <Footer />
    </div>
  )
}
