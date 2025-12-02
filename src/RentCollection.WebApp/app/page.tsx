'use client'

import { useState } from 'react'
import Link from 'next/link'
import { FaMapMarkerAlt, FaBed, FaBath, FaSearch, FaStar, FaHeart, FaCheckCircle, FaUserPlus, FaClipboardList } from 'react-icons/fa'
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

  const categories = [
    { id: 'all', label: 'All Properties', icon: '🏠' },
    { id: 'apartment', label: 'Apartments', icon: '🏢' },
    { id: 'house', label: 'Houses', icon: '🏡' },
    { id: 'studio', label: 'Studio', icon: '🛏️' },
  ]

  return (
    <div className="min-h-screen bg-white">
      <Navbar />
      {/* Hero Section with Search */}
      <div className="relative bg-cover bg-center" style={{ backgroundImage: 'linear-gradient(rgba(0,0,0,0.3), rgba(0,0,0,0.3)), url(https://images.unsplash.com/photo-1560518883-ce09059eeffa?w=1200)' }}>
        <div className="container mx-auto px-6 py-32">
          <div className="max-w-3xl mx-auto text-center text-white">
            <h1 className="text-5xl md:text-6xl font-bold mb-6">
              Find Your Perfect Home in Kenya
            </h1>
            <p className="text-xl md:text-2xl mb-8 text-gray-100">
              Discover quality rental properties. Apply online in minutes.
            </p>

            {/* Search Bar */}
            <div className="bg-white rounded-full shadow-2xl p-2 flex flex-col md:flex-row gap-2 max-w-4xl mx-auto">
              <div className="flex-1 relative">
                <FaSearch className="absolute left-6 top-1/2 transform -translate-y-1/2 text-gray-400" />
                <input
                  type="text"
                  placeholder="Search location or property name..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="w-full pl-14 pr-4 py-4 rounded-full focus:outline-none text-gray-900"
                />
              </div>
              <button
                onClick={() => document.getElementById('properties')?.scrollIntoView({ behavior: 'smooth' })}
                className="bg-primary-600 hover:bg-primary-700 text-white px-8 py-4 rounded-full font-semibold transition-colors"
              >
                Search
              </button>
            </div>
          </div>
        </div>
      </div>

      {/* Category Tabs */}
      <div className="border-b border-gray-200 bg-white">
        <div className="container mx-auto px-6">
          <div className="flex space-x-8 overflow-x-auto py-4">
            {categories.map((cat) => (
              <button
                key={cat.id}
                onClick={() => setPropertyType(cat.id)}
                className={`flex flex-col items-center min-w-fit space-y-2 pb-2 border-b-2 transition-colors ${
                  propertyType === cat.id
                    ? 'border-primary-600 text-primary-600'
                    : 'border-transparent text-gray-600 hover:text-gray-900'
                }`}
              >
                <span className="text-2xl">{cat.icon}</span>
                <span className="text-sm font-medium whitespace-nowrap">{cat.label}</span>
              </button>
            ))}
          </div>
        </div>
      </div>

      {/* Advanced Filters */}
      <div className="bg-gray-50 border-b border-gray-200">
        <div className="container mx-auto px-6 py-4">
          <div className="flex flex-wrap gap-3">
            <select
              value={bedrooms}
              onChange={(e) => setBedrooms(e.target.value === '' ? '' : Number(e.target.value))}
              className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:outline-none bg-white"
            >
              <option value="">Bedrooms</option>
              <option value="1">1 Bed</option>
              <option value="2">2 Beds</option>
              <option value="3">3 Beds</option>
              <option value="4">4+ Beds</option>
            </select>
            <input
              type="number"
              placeholder="Min Price (KES)"
              value={minPrice}
              onChange={(e) => setMinPrice(e.target.value === '' ? '' : Number(e.target.value))}
              className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:outline-none w-40"
            />
            <input
              type="number"
              placeholder="Max Price (KES)"
              value={maxPrice}
              onChange={(e) => setMaxPrice(e.target.value === '' ? '' : Number(e.target.value))}
              className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:outline-none w-40"
            />
            {(searchTerm || minPrice || maxPrice || bedrooms) && (
              <button
                onClick={() => {
                  setSearchTerm('')
                  setMinPrice('')
                  setMaxPrice('')
                  setBedrooms('')
                }}
                className="px-4 py-2 text-gray-600 hover:text-gray-900 font-medium"
              >
                Clear all
              </button>
            )}
          </div>
        </div>
      </div>

      {/* Properties Section */}
      <div id="properties" className="container mx-auto px-6 py-16">
        <div className="mb-12">
          <h2 className="text-4xl font-bold text-gray-900 mb-3">
            {filteredUnits.length > 0 ? `${filteredUnits.length} properties` : 'Available Properties'}
          </h2>
          <p className="text-gray-600 text-lg">Find your perfect rental home</p>
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
              }}
              className="text-primary-600 font-semibold hover:underline text-lg"
            >
              Clear filters
            </button>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            {filteredUnits.map((unit) => (
              <Link
                key={unit.id}
                href={`/units/${unit.id}/apply`}
                className="group cursor-pointer"
              >
                {/* Image Placeholder */}
                <div className="relative aspect-square mb-3 rounded-xl overflow-hidden bg-gray-200">
                  <div className="absolute inset-0 bg-gradient-to-br from-primary-400 to-primary-600 flex items-center justify-center">
                    <span className="text-white text-6xl font-bold opacity-20">
                      {unit.bedrooms}BR
                    </span>
                  </div>
                  <button className="absolute top-3 right-3 p-2 bg-white/90 rounded-full hover:bg-white transition-colors">
                    <FaHeart className="text-gray-600" />
                  </button>
                  <div className="absolute bottom-3 left-3">
                    <span className="bg-white px-3 py-1 rounded-full text-sm font-semibold text-gray-900">
                      Unit {unit.unitNumber}
                    </span>
                  </div>
                </div>

                {/* Property Details */}
                <div>
                  <div className="flex items-start justify-between mb-1">
                    <h3 className="font-semibold text-gray-900 group-hover:text-primary-600 transition-colors">
                      {unit.propertyName}
                    </h3>
                    <div className="flex items-center text-sm">
                      <FaStar className="text-yellow-400 mr-1" />
                      <span className="font-semibold">4.8</span>
                    </div>
                  </div>
                  <p className="text-gray-600 text-sm mb-2 flex items-center">
                    <FaMapMarkerAlt className="mr-1 text-gray-400" size={12} />
                    {unit.propertyLocation || 'Nairobi'}
                  </p>
                  <p className="text-gray-600 text-sm mb-2">
                    {unit.bedrooms} bed • {unit.bathrooms} bath
                  </p>
                  <p className="text-gray-900">
                    <span className="font-bold">KES {unit.monthlyRent.toLocaleString()}</span>
                    <span className="text-gray-600 text-sm"> /month</span>
                  </p>
                </div>
              </Link>
            ))}
          </div>
        )}
      </div>

      {/* How It Works Section */}
      <div id="how-it-works" className="bg-gray-50 py-20">
        <div className="container mx-auto px-6">
          <div className="text-center mb-16">
            <h2 className="text-4xl font-bold text-gray-900 mb-4">How It Works</h2>
            <p className="text-xl text-gray-600 max-w-2xl mx-auto">
              Find and rent your perfect home in three simple steps
            </p>
          </div>

          <div className="grid md:grid-cols-3 gap-12 max-w-5xl mx-auto">
            <div className="text-center">
              <div className="bg-primary-100 w-20 h-20 rounded-full flex items-center justify-center mx-auto mb-6">
                <FaSearch className="text-4xl text-primary-600" />
              </div>
              <h3 className="text-2xl font-bold mb-4">1. Browse Properties</h3>
              <p className="text-gray-600">
                Search through our curated list of quality rental properties across Kenya. Filter by location, price, and amenities.
              </p>
            </div>

            <div className="text-center">
              <div className="bg-primary-100 w-20 h-20 rounded-full flex items-center justify-center mx-auto mb-6">
                <FaClipboardList className="text-4xl text-primary-600" />
              </div>
              <h3 className="text-2xl font-bold mb-4">2. Apply Online</h3>
              <p className="text-gray-600">
                Fill out a simple application form. No account needed. Your application goes directly to the landlord for review.
              </p>
            </div>

            <div className="text-center">
              <div className="bg-primary-100 w-20 h-20 rounded-full flex items-center justify-center mx-auto mb-6">
                <FaCheckCircle className="text-4xl text-primary-600" />
              </div>
              <h3 className="text-2xl font-bold mb-4">3. Move In</h3>
              <p className="text-gray-600">
                Once approved, receive your login credentials and access your tenant portal to manage payments and communications.
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* About Section */}
      <div id="about" className="py-20 bg-white">
        <div className="container mx-auto px-6">
          <div className="max-w-4xl mx-auto text-center">
            <h2 className="text-4xl font-bold text-gray-900 mb-6">Why Choose RentPro?</h2>
            <p className="text-xl text-gray-600 mb-12">
              Modern property management for the Kenyan market
            </p>

            <div className="grid md:grid-cols-2 gap-8 text-left">
              <div className="bg-gray-50 p-8 rounded-xl">
                <h3 className="text-xl font-bold mb-3">For Tenants</h3>
                <ul className="space-y-3 text-gray-600">
                  <li className="flex items-start">
                    <FaCheckCircle className="text-green-500 mr-3 mt-1 flex-shrink-0" />
                    <span>Browse properties without creating an account</span>
                  </li>
                  <li className="flex items-start">
                    <FaCheckCircle className="text-green-500 mr-3 mt-1 flex-shrink-0" />
                    <span>Simple online application process</span>
                  </li>
                  <li className="flex items-start">
                    <FaCheckCircle className="text-green-500 mr-3 mt-1 flex-shrink-0" />
                    <span>Track payments and lease details online</span>
                  </li>
                  <li className="flex items-start">
                    <FaCheckCircle className="text-green-500 mr-3 mt-1 flex-shrink-0" />
                    <span>Direct communication with landlords</span>
                  </li>
                </ul>
              </div>

              <div className="bg-primary-50 p-8 rounded-xl">
                <h3 className="text-xl font-bold mb-3">For Landlords</h3>
                <ul className="space-y-3 text-gray-600">
                  <li className="flex items-start">
                    <FaCheckCircle className="text-primary-600 mr-3 mt-1 flex-shrink-0" />
                    <span>Manage multiple properties in one place</span>
                  </li>
                  <li className="flex items-start">
                    <FaCheckCircle className="text-primary-600 mr-3 mt-1 flex-shrink-0" />
                    <span>Review tenant applications efficiently</span>
                  </li>
                  <li className="flex items-start">
                    <FaCheckCircle className="text-primary-600 mr-3 mt-1 flex-shrink-0" />
                    <span>Track rent payments and generate reports</span>
                  </li>
                  <li className="flex items-start">
                    <FaCheckCircle className="text-primary-600 mr-3 mt-1 flex-shrink-0" />
                    <span>SMS notifications for important updates</span>
                  </li>
                </ul>
              </div>
            </div>

            <div className="mt-12">
              <Link
                href="/login"
                className="inline-block bg-primary-600 hover:bg-primary-700 text-white px-10 py-4 rounded-full font-semibold text-lg transition-colors"
              >
                Get Started Today
              </Link>
            </div>
          </div>
        </div>
      </div>

      <Footer />
    </div>
  )
}
