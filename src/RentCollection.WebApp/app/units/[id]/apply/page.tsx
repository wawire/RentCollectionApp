'use client'

import { useState, useEffect } from 'react'
import { useParams, useRouter } from 'next/navigation'
import { ArrowLeft, CheckCircle, MapPin, Bed, Bath, Maximize2, Calendar, DollarSign, Phone, Mail, User, FileText, Shield, Wifi, Car, Zap, Droplets, Wind, Flame, Home } from 'lucide-react'
import Link from 'next/link'
import { publicListingService } from '@/lib/services/publicListingService'
import { useTenantApplication } from '@/lib/hooks/usePublicListings'
import { PublicUnitListing } from '@/lib/types/public.types'
import LoadingSpinner from '@/components/common/LoadingSpinner'
import Breadcrumb from '@/components/common/Breadcrumb'
import Navbar from '@/components/layout/Navbar'
import Footer from '@/components/layout/Footer'

export default function TenantApplicationPage() {
  const params = useParams()
  const router = useRouter()
  const unitId = parseInt(params.id as string)

  const [unit, setUnit] = useState<PublicUnitListing | null>(null)
  const [loading, setLoading] = useState(true)
  const { submitApplication, submitting, error, success, reset } = useTenantApplication()

  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    phoneNumber: '',
    idNumber: '',
    leaseStartDate: '',
    leaseEndDate: '',
    securityDeposit: '',
    applicationNotes: '',
  })

  useEffect(() => {
    const fetchUnit = async () => {
      try {
        const data = await publicListingService.getUnitDetails(unitId)
        setUnit(data)
      } catch (err) {
        console.error('Failed to fetch unit details:', err)
      } finally {
        setLoading(false)
      }
    }

    if (unitId) {
      fetchUnit()
    }
  }, [unitId])

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    })
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    const application = {
      firstName: formData.firstName,
      lastName: formData.lastName,
      email: formData.email,
      phoneNumber: formData.phoneNumber,
      idNumber: formData.idNumber || undefined,
      unitId: unitId,
      leaseStartDate: formData.leaseStartDate,
      leaseEndDate: formData.leaseEndDate || undefined,
      securityDeposit: formData.securityDeposit ? parseFloat(formData.securityDeposit) : undefined,
      applicationNotes: formData.applicationNotes || undefined,
    }

    const success = await submitApplication(application)

    if (success) {
      // Scroll to success message
      window.scrollTo({ top: 0, behavior: 'smooth' })
    }
  }

  if (loading) {
    return (
      <>
        <Navbar />
        <div className="min-h-screen flex items-center justify-center">
          <LoadingSpinner size="lg" />
        </div>
        <Footer />
      </>
    )
  }

  if (!unit) {
    return (
      <>
        <Navbar />
        <div className="min-h-screen flex items-center justify-center">
          <div className="text-center">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">Unit not found</h2>
            <Link href="/" className="text-primary-600 hover:underline">
              Back to listings
            </Link>
          </div>
        </div>
        <Footer />
      </>
    )
  }

  if (success) {
    return (
      <>
        <Navbar />
        <div className="min-h-screen bg-gray-50 py-12">
          <div className="max-w-3xl mx-auto px-6">
            <div className="max-w-2xl mx-auto bg-white rounded-lg shadow-lg p-8 text-center">
              <CheckCircle className="w-16 h-16 text-green-500 mx-auto mb-4" />
              <h1 className="text-3xl font-bold text-gray-900 mb-4">Application Submitted!</h1>
              <p className="text-lg text-gray-600 mb-6">
                Thank you for your application. The landlord will review it and contact you soon.
              </p>
              <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-6 text-left">
                <h3 className="font-semibold text-gray-900 mb-2">What happens next?</h3>
                <ul className="text-gray-700 space-y-2 text-sm">
                  <li>✓ Your application has been sent to the landlord</li>
                  <li>✓ The landlord will review your information</li>
                  <li>✓ If approved, you'll receive login credentials via email</li>
                  <li>✓ You can then access the tenant portal to view lease details and make payments</li>
                </ul>
              </div>
              <div className="flex gap-4 justify-center">
                <Link
                  href="/"
                  className="bg-primary-600 text-white px-6 py-3 rounded-lg font-medium hover:bg-primary-700 transition-colors"
                >
                  Browse More Units
                </Link>
                <button
                  onClick={() => window.print()}
                  className="bg-gray-200 text-gray-700 px-6 py-3 rounded-lg font-medium hover:bg-gray-300 transition-colors"
                >
                  Print Confirmation
                </button>
              </div>
            </div>
          </div>
        </div>
        <Footer />
      </>
    )
  }

  // Mock amenities data (to be replaced with real data from backend)
  const amenities = [
    { icon: Wifi, label: 'WiFi' },
    { icon: Car, label: 'Parking' },
    { icon: Zap, label: 'Power Backup' },
    { icon: Droplets, label: 'Water Supply' },
    { icon: Wind, label: 'Air Conditioning' },
    { icon: Shield, label: 'Security' },
  ]

  return (
    <>
      <Navbar />
      <div className="min-h-screen bg-gray-50">
        {/* Breadcrumb Navigation */}
        <div className="bg-white border-b border-gray-200">
          <div className="max-w-7xl mx-auto px-6 py-4">
            <Breadcrumb
              items={[
                { label: 'Browse Properties', href: '/' },
                { label: unit.propertyName || 'Property', href: '/' },
                { label: `Unit ${unit.unitNumber}` },
              ]}
            />
          </div>
        </div>

        {/* Main Content - Split Layout */}
        <div className="max-w-7xl mx-auto px-6 py-8">
          <div className="grid lg:grid-cols-2 gap-8">
            {/* Left Column - Image Gallery & Property Details (Sticky) */}
            <div className="lg:sticky lg:top-24 lg:self-start space-y-6">
              {/* Image Gallery */}
              <div className="bg-white rounded-xl shadow-md overflow-hidden">
                {/* Main Image */}
                <div className="relative aspect-[4/3] bg-gradient-to-br from-primary-400 to-primary-600">
                  <div className="absolute inset-0 flex items-center justify-center">
                    <span className="text-white text-6xl font-bold opacity-30">
                      {unit.bedrooms}BR
                    </span>
                  </div>
                  <div className="absolute top-4 left-4">
                    <span className="bg-green-500 text-white px-3 py-1.5 rounded-full text-xs font-bold shadow-lg">
                      Available Now
                    </span>
                  </div>
                  <div className="absolute bottom-4 left-4">
                    <Link
                      href="/"
                      className="flex items-center gap-2 bg-white/90 backdrop-blur-sm px-4 py-2 rounded-lg text-sm font-medium text-gray-900 hover:bg-white transition-colors"
                    >
                      <ArrowLeft size={16} />
                      Back to Listings
                    </Link>
                  </div>
                </div>

                {/* Thumbnail Gallery Placeholder */}
                <div className="p-4 grid grid-cols-4 gap-2">
                  {[1, 2, 3, 4].map((i) => (
                    <div
                      key={i}
                      className="aspect-square bg-gradient-to-br from-gray-200 to-gray-300 rounded-lg cursor-pointer hover:opacity-75 transition-opacity"
                    />
                  ))}
                </div>
              </div>

              {/* Key Property Details Card */}
              <div className="bg-white rounded-xl shadow-md p-6">
                <h2 className="text-2xl font-bold text-gray-900 mb-2">{unit.propertyName}</h2>
                <p className="text-gray-600 flex items-center gap-2 mb-4">
                  <MapPin size={18} className="text-gray-400" />
                  {unit.propertyLocation || 'Nairobi, Kenya'}
                </p>

                {/* Price */}
                <div className="bg-primary-50 border border-primary-200 rounded-lg p-4 mb-4">
                  <p className="text-sm text-gray-600 mb-1">Monthly Rent</p>
                  <p className="text-3xl font-bold text-primary-600">
                    KES {unit.monthlyRent.toLocaleString()}
                  </p>
                  <p className="text-xs text-gray-500 mt-1">+ utilities</p>
                </div>

                {/* Key Features Grid */}
                <div className="grid grid-cols-3 gap-4 mb-4">
                  <div className="text-center p-3 bg-gray-50 rounded-lg">
                    <Bed className="w-6 h-6 mx-auto mb-2 text-gray-600" />
                    <p className="text-sm font-semibold text-gray-900">{unit.bedrooms}</p>
                    <p className="text-xs text-gray-500">Bedrooms</p>
                  </div>
                  <div className="text-center p-3 bg-gray-50 rounded-lg">
                    <Bath className="w-6 h-6 mx-auto mb-2 text-gray-600" />
                    <p className="text-sm font-semibold text-gray-900">{unit.bathrooms}</p>
                    <p className="text-xs text-gray-500">Bathrooms</p>
                  </div>
                  {unit.squareFeet && (
                    <div className="text-center p-3 bg-gray-50 rounded-lg">
                      <Maximize2 className="w-6 h-6 mx-auto mb-2 text-gray-600" />
                      <p className="text-sm font-semibold text-gray-900">{unit.squareFeet}</p>
                      <p className="text-xs text-gray-500">sq ft</p>
                    </div>
                  )}
                </div>

                {/* Amenities */}
                <div className="border-t border-gray-200 pt-4">
                  <h3 className="text-sm font-bold text-gray-900 mb-3">Amenities</h3>
                  <div className="grid grid-cols-2 gap-2">
                    {amenities.map((amenity, index) => {
                      const Icon = amenity.icon
                      return (
                        <div key={index} className="flex items-center gap-2 text-sm text-gray-700">
                          <Icon size={16} className="text-primary-600" />
                          <span>{amenity.label}</span>
                        </div>
                      )
                    })}
                  </div>
                </div>
              </div>

              {/* Map Placeholder */}
              <div className="bg-white rounded-xl shadow-md overflow-hidden">
                <div className="aspect-[16/9] bg-gradient-to-br from-gray-100 to-gray-200 flex items-center justify-center">
                  <div className="text-center">
                    <MapPin className="w-12 h-12 mx-auto mb-2 text-gray-400" />
                    <p className="text-sm text-gray-500">Interactive Map</p>
                  </div>
                </div>
              </div>
            </div>

            {/* Right Column - Application Form */}
            <div className="space-y-6">
              <div className="bg-white rounded-xl shadow-md p-8">
                <div className="flex items-center justify-between mb-6">
                  <div>
                    <h1 className="text-3xl font-bold text-gray-900 mb-2">
                      Apply for Unit {unit.unitNumber}
                    </h1>
                    <p className="text-gray-600">Fill out the form below to submit your application</p>
                  </div>
                </div>

                {error && (
                  <div className="bg-red-50 border border-red-200 rounded-lg p-4 mb-6">
                    <p className="text-red-600 font-medium">{error}</p>
                  </div>
                )}

                <form onSubmit={handleSubmit} className="space-y-6">
                  {/* Personal Information */}
                  <div>
                    <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
                      <User size={20} className="text-primary-600" />
                      Personal Information
                    </h3>
                    <div className="grid md:grid-cols-2 gap-4">
                      <div>
                        <label className="block text-gray-700 font-medium mb-2">
                          First Name <span className="text-red-500">*</span>
                        </label>
                        <input
                          type="text"
                          name="firstName"
                          value={formData.firstName}
                          onChange={handleChange}
                          required
                          className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent transition-all"
                        />
                      </div>
                      <div>
                        <label className="block text-gray-700 font-medium mb-2">
                          Last Name <span className="text-red-500">*</span>
                        </label>
                        <input
                          type="text"
                          name="lastName"
                          value={formData.lastName}
                          onChange={handleChange}
                          required
                          className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent transition-all"
                        />
                      </div>
                      <div>
                        <label className="block text-gray-700 font-medium mb-2 flex items-center gap-2">
                          <Mail size={16} className="text-gray-400" />
                          Email <span className="text-red-500">*</span>
                        </label>
                        <input
                          type="email"
                          name="email"
                          value={formData.email}
                          onChange={handleChange}
                          required
                          className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent transition-all"
                        />
                      </div>
                      <div>
                        <label className="block text-gray-700 font-medium mb-2 flex items-center gap-2">
                          <Phone size={16} className="text-gray-400" />
                          Phone Number <span className="text-red-500">*</span>
                        </label>
                        <input
                          type="tel"
                          name="phoneNumber"
                          value={formData.phoneNumber}
                          onChange={handleChange}
                          required
                          placeholder="+254..."
                          className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent transition-all"
                        />
                      </div>
                      <div className="md:col-span-2">
                        <label className="block text-gray-700 font-medium mb-2 flex items-center gap-2">
                          <FileText size={16} className="text-gray-400" />
                          ID Number (Optional)
                        </label>
                        <input
                          type="text"
                          name="idNumber"
                          value={formData.idNumber}
                          onChange={handleChange}
                          className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent transition-all"
                        />
                      </div>
                    </div>
                  </div>

                  {/* Lease Information */}
                  <div>
                    <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
                      <Calendar size={20} className="text-primary-600" />
                      Lease Information
                    </h3>
                    <div className="grid md:grid-cols-2 gap-4">
                      <div>
                        <label className="block text-gray-700 font-medium mb-2">
                          Lease Start Date <span className="text-red-500">*</span>
                        </label>
                        <input
                          type="date"
                          name="leaseStartDate"
                          value={formData.leaseStartDate}
                          onChange={handleChange}
                          required
                          min={new Date().toISOString().split('T')[0]}
                          className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent transition-all"
                        />
                      </div>
                      <div>
                        <label className="block text-gray-700 font-medium mb-2">
                          Lease End Date (Optional)
                        </label>
                        <input
                          type="date"
                          name="leaseEndDate"
                          value={formData.leaseEndDate}
                          onChange={handleChange}
                          min={formData.leaseStartDate}
                          className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent transition-all"
                        />
                      </div>
                      <div className="md:col-span-2">
                        <label className="block text-gray-700 font-medium mb-2 flex items-center gap-2">
                          <DollarSign size={16} className="text-gray-400" />
                          Security Deposit (Optional)
                        </label>
                        <div className="relative">
                          <span className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-500 font-medium">
                            KES
                          </span>
                          <input
                            type="number"
                            name="securityDeposit"
                            value={formData.securityDeposit}
                            onChange={handleChange}
                            placeholder="0.00"
                            step="0.01"
                            className="w-full pl-16 pr-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent transition-all"
                          />
                        </div>
                      </div>
                    </div>
                  </div>

                  {/* Additional Notes */}
                  <div>
                    <label className="block text-gray-700 font-medium mb-2 flex items-center gap-2">
                      <FileText size={16} className="text-gray-400" />
                      Additional Notes (Optional)
                    </label>
                    <textarea
                      name="applicationNotes"
                      value={formData.applicationNotes}
                      onChange={handleChange}
                      rows={4}
                      placeholder="Tell us more about yourself or ask any questions..."
                      className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent transition-all resize-none"
                    />
                  </div>

                  {/* Submit Button */}
                  <div className="border-t border-gray-200 pt-6">
                    <button
                      type="submit"
                      disabled={submitting}
                      className="w-full bg-primary-600 text-white px-8 py-4 rounded-lg font-bold text-lg hover:bg-primary-700 transition-all disabled:opacity-50 disabled:cursor-not-allowed shadow-lg hover:shadow-xl"
                    >
                      {submitting ? 'Submitting Application...' : 'Submit Application'}
                    </button>
                    <p className="text-gray-600 text-sm text-center mt-4">
                      By submitting this application, you agree to our terms and conditions.
                      The landlord will review your application and contact you if approved.
                    </p>
                  </div>
                </form>
              </div>

              {/* Trust Signals */}
              <div className="bg-blue-50 border border-blue-200 rounded-xl p-6">
                <h3 className="font-bold text-gray-900 mb-3 flex items-center gap-2">
                  <Shield size={20} className="text-blue-600" />
                  Why Apply Through RentPro?
                </h3>
                <ul className="space-y-2 text-sm text-gray-700">
                  <li className="flex items-start gap-2">
                    <CheckCircle size={16} className="text-green-500 mt-0.5 flex-shrink-0" />
                    <span>Verified properties and landlords</span>
                  </li>
                  <li className="flex items-start gap-2">
                    <CheckCircle size={16} className="text-green-500 mt-0.5 flex-shrink-0" />
                    <span>Secure application process</span>
                  </li>
                  <li className="flex items-start gap-2">
                    <CheckCircle size={16} className="text-green-500 mt-0.5 flex-shrink-0" />
                    <span>Quick response from property owners</span>
                  </li>
                  <li className="flex items-start gap-2">
                    <CheckCircle size={16} className="text-green-500 mt-0.5 flex-shrink-0" />
                    <span>Your personal information is protected</span>
                  </li>
                </ul>
              </div>
            </div>
          </div>
        </div>
      </div>
      <Footer />
    </>
  )
}
