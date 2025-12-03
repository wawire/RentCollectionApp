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

      {/* Main Container with Sidebar */}
      <div className="flex">
        {/* Desktop Sidebar */}
        <aside className="hidden lg:block w-80 sticky top-0 h-screen">
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
          <div className="bg-white border-b border-gray-200 sticky top-0 z-10">
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
            <div className="mb-8">
              <h2 className="text-2xl font-serif font-normal text-primary tracking-wide leading-relaxed">
                {filteredUnits.length > 0 ? `${filteredUnits.length} properties available` : 'Available Properties'}
              </h2>
            </div>

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
        </main>
      </div>

      <Footer />
    </div>
  )
}
