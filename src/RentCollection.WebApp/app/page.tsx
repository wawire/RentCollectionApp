'use client'

import { useState, useEffect } from 'react'
import Link from 'next/link'
import { FaMapMarkerAlt, FaBed, FaBath, FaRulerCombined, FaSearch, FaUserPlus } from 'react-icons/fa'
import { useVacantUnits } from '@/lib/hooks/usePublicListings'
import LoadingSpinner from '@/components/common/LoadingSpinner'

export default function PublicLandingPage() {
  const { units, loading, error } = useVacantUnits()
  const [searchTerm, setSearchTerm] = useState('')
  const [minPrice, setMinPrice] = useState<number | ''>('')
  const [maxPrice, setMaxPrice] = useState<number | ''>('')
  const [bedrooms, setBedrooms] = useState<number | ''>('')

  const filteredUnits = units.filter((unit) => {
    const matchesSearch =
      searchTerm === '' ||
      unit.propertyName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      unit.propertyLocation?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      unit.unitNumber.toLowerCase().includes(searchTerm.toLowerCase())

    const matchesMinPrice = minPrice === '' || unit.monthlyRent >= minPrice
    const matchesMaxPrice = maxPrice === '' || unit.monthlyRent <= maxPrice
    const matchesBedrooms = bedrooms === '' || unit.bedrooms === bedrooms

    return matchesSearch && matchesMinPrice && matchesMaxPrice && matchesBedrooms
  })

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Hero Section */}
      <div className="bg-gradient-to-r from-primary-600 to-primary-700 text-white">
        <div className="container mx-auto px-6 py-16">
          <div className="max-w-4xl mx-auto text-center">
            <h1 className="text-5xl font-bold mb-4">Find Your Perfect Home</h1>
            <p className="text-xl text-primary-100 mb-8">
              Browse available rental units and apply online in minutes. No account required to view listings.
            </p>
            <div className="flex flex-col sm:flex-row gap-4 justify-center">
              <Link
                href="#listings"
                className="bg-white text-primary-700 px-8 py-3 rounded-lg font-semibold hover:bg-primary-50 transition-colors"
              >
                Browse Units
              </Link>
              <Link
                href="/login"
                className="bg-primary-500 text-white px-8 py-3 rounded-lg font-semibold hover:bg-primary-400 transition-colors border-2 border-white"
              >
                Landlord Login
              </Link>
            </div>
          </div>
        </div>
      </div>

      {/* Search & Filter Section */}
      <div className="bg-white border-b border-gray-200 sticky top-0 z-10 shadow-sm">
        <div className="container mx-auto px-6 py-6">
          <div className="grid grid-cols-1 md:grid-cols-5 gap-4">
            <div className="md:col-span-2 relative">
              <FaSearch className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" />
              <input
                type="text"
                placeholder="Search by location, property, or unit..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              />
            </div>
            <div>
              <input
                type="number"
                placeholder="Min Rent (KES)"
                value={minPrice}
                onChange={(e) => setMinPrice(e.target.value === '' ? '' : Number(e.target.value))}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              />
            </div>
            <div>
              <input
                type="number"
                placeholder="Max Rent (KES)"
                value={maxPrice}
                onChange={(e) => setMaxPrice(e.target.value === '' ? '' : Number(e.target.value))}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              />
            </div>
            <div>
              <select
                value={bedrooms}
                onChange={(e) => setBedrooms(e.target.value === '' ? '' : Number(e.target.value))}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              >
                <option value="">Any Bedrooms</option>
                <option value="1">1 Bedroom</option>
                <option value="2">2 Bedrooms</option>
                <option value="3">3 Bedrooms</option>
                <option value="4">4+ Bedrooms</option>
              </select>
            </div>
          </div>
        </div>
      </div>

      {/* Listings Section */}
      <div id="listings" className="container mx-auto px-6 py-12">
        <div className="mb-8">
          <h2 className="text-3xl font-bold text-gray-900 mb-2">
            Available Units
            {filteredUnits.length > 0 && (
              <span className="text-primary-600"> ({filteredUnits.length})</span>
            )}
          </h2>
          <p className="text-gray-600">Browse our collection of rental units and apply online</p>
        </div>

        {loading ? (
          <div className="flex justify-center items-center py-20">
            <LoadingSpinner size="lg" />
          </div>
        ) : error ? (
          <div className="bg-red-50 border border-red-200 rounded-lg p-6 text-center">
            <p className="text-red-600 font-medium">{error}</p>
            <p className="text-gray-600 mt-2">Please try again later</p>
          </div>
        ) : filteredUnits.length === 0 ? (
          <div className="bg-gray-50 border border-gray-200 rounded-lg p-12 text-center">
            <p className="text-gray-600 text-lg mb-4">No units found matching your criteria</p>
            <button
              onClick={() => {
                setSearchTerm('')
                setMinPrice('')
                setMaxPrice('')
                setBedrooms('')
              }}
              className="text-primary-600 font-medium hover:underline"
            >
              Clear filters
            </button>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {filteredUnits.map((unit) => (
              <Link
                key={unit.id}
                href={`/units/${unit.id}/apply`}
                className="block bg-white rounded-lg shadow-md hover:shadow-xl transition-all duration-200 overflow-hidden hover:-translate-y-1"
              >
                <div className="p-6">
                  {/* Property Name & Location */}
                  <div className="mb-4">
                    <h3 className="text-xl font-bold text-gray-900 mb-1">{unit.propertyName}</h3>
                    <div className="flex items-center text-gray-600 text-sm">
                      <FaMapMarkerAlt className="mr-1" />
                      <span>{unit.propertyLocation || 'Location not specified'}</span>
                    </div>
                  </div>

                  {/* Unit Number */}
                  <div className="mb-4">
                    <span className="inline-block bg-primary-100 text-primary-700 px-3 py-1 rounded-full text-sm font-medium">
                      Unit {unit.unitNumber}
                    </span>
                  </div>

                  {/* Unit Details */}
                  <div className="grid grid-cols-3 gap-4 mb-4 py-4 border-t border-b border-gray-200">
                    <div className="text-center">
                      <FaBed className="text-2xl text-gray-400 mx-auto mb-1" />
                      <p className="text-gray-900 font-medium">{unit.bedrooms}</p>
                      <p className="text-gray-500 text-xs">Beds</p>
                    </div>
                    <div className="text-center">
                      <FaBath className="text-2xl text-gray-400 mx-auto mb-1" />
                      <p className="text-gray-900 font-medium">{unit.bathrooms}</p>
                      <p className="text-gray-500 text-xs">Baths</p>
                    </div>
                    {unit.squareFeet && (
                      <div className="text-center">
                        <FaRulerCombined className="text-2xl text-gray-400 mx-auto mb-1" />
                        <p className="text-gray-900 font-medium">{unit.squareFeet}</p>
                        <p className="text-gray-500 text-xs">Sq Ft</p>
                      </div>
                    )}
                  </div>

                  {/* Description */}
                  {unit.description && (
                    <p className="text-gray-600 text-sm mb-4 line-clamp-2">{unit.description}</p>
                  )}

                  {/* Price & Apply Button */}
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="text-gray-500 text-xs">Monthly Rent</p>
                      <p className="text-2xl font-bold text-primary-600">
                        KES {unit.monthlyRent.toLocaleString()}
                      </p>
                    </div>
                    <button className="bg-primary-600 text-white px-6 py-2 rounded-lg font-medium hover:bg-primary-700 transition-colors flex items-center gap-2">
                      <FaUserPlus />
                      Apply
                    </button>
                  </div>
                </div>
              </Link>
            ))}
          </div>
        )}
      </div>

      {/* Call to Action */}
      <div className="bg-primary-600 text-white">
        <div className="container mx-auto px-6 py-16 text-center">
          <h2 className="text-3xl font-bold mb-4">Are you a landlord?</h2>
          <p className="text-xl text-primary-100 mb-8 max-w-2xl mx-auto">
            Manage your properties, tenants, and payments efficiently with our comprehensive platform
          </p>
          <Link
            href="/login"
            className="inline-block bg-white text-primary-700 px-8 py-3 rounded-lg font-semibold hover:bg-primary-50 transition-colors"
          >
            Get Started
          </Link>
        </div>
      </div>
    </div>
  )
}
