'use client'

import { useState, useEffect } from 'react'
import { useParams, useRouter } from 'next/navigation'
import { FaArrowLeft, FaCheckCircle } from 'react-icons/fa'
import Link from 'next/link'
import { publicListingService } from '@/lib/services/publicListingService'
import { useTenantApplication } from '@/lib/hooks/usePublicListings'
import { PublicUnitListing } from '@/lib/types/public.types'
import LoadingSpinner from '@/components/common/LoadingSpinner'

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
      <div className="min-h-screen flex items-center justify-center">
        <LoadingSpinner size="lg" />
      </div>
    )
  }

  if (!unit) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <h2 className="text-2xl font-bold text-gray-900 mb-4">Unit not found</h2>
          <Link href="/" className="text-primary-600 hover:underline">
            Back to listings
          </Link>
        </div>
      </div>
    )
  }

  if (success) {
    return (
      <div className="min-h-screen bg-gray-50 py-12">
        <div className="container mx-auto px-6">
          <div className="max-w-2xl mx-auto bg-white rounded-lg shadow-lg p-8 text-center">
            <FaCheckCircle className="text-6xl text-green-500 mx-auto mb-4" />
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
    )
  }

  return (
    <div className="min-h-screen bg-gray-50 py-12">
      <div className="container mx-auto px-6">
        <Link href="/" className="inline-flex items-center text-primary-600 hover:underline mb-6">
          <FaArrowLeft className="mr-2" />
          Back to listings
        </Link>

        <div className="max-w-4xl mx-auto">
          {/* Unit Summary */}
          <div className="bg-white rounded-lg shadow-md p-6 mb-6">
            <h1 className="text-3xl font-bold text-gray-900 mb-4">Apply for Unit {unit.unitNumber}</h1>
            <div className="grid md:grid-cols-2 gap-4">
              <div>
                <p className="text-gray-600 text-sm">Property</p>
                <p className="text-gray-900 font-medium">{unit.propertyName}</p>
              </div>
              <div>
                <p className="text-gray-600 text-sm">Location</p>
                <p className="text-gray-900 font-medium">{unit.propertyLocation}</p>
              </div>
              <div>
                <p className="text-gray-600 text-sm">Monthly Rent</p>
                <p className="text-2xl font-bold text-primary-600">
                  KES {unit.monthlyRent.toLocaleString()}
                </p>
              </div>
              <div>
                <p className="text-gray-600 text-sm">Unit Details</p>
                <p className="text-gray-900 font-medium">
                  {unit.bedrooms} Bed • {unit.bathrooms} Bath
                  {unit.squareFeet && ` • ${unit.squareFeet} sq ft`}
                </p>
              </div>
            </div>
          </div>

          {/* Application Form */}
          <div className="bg-white rounded-lg shadow-md p-8">
            <h2 className="text-2xl font-bold text-gray-900 mb-6">Application Details</h2>

            {error && (
              <div className="bg-red-50 border border-red-200 rounded-lg p-4 mb-6">
                <p className="text-red-600 font-medium">{error}</p>
              </div>
            )}

            <form onSubmit={handleSubmit} className="space-y-6">
              {/* Personal Information */}
              <div>
                <h3 className="text-lg font-semibold text-gray-900 mb-4">Personal Information</h3>
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
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
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
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                    />
                  </div>
                  <div>
                    <label className="block text-gray-700 font-medium mb-2">
                      Email <span className="text-red-500">*</span>
                    </label>
                    <input
                      type="email"
                      name="email"
                      value={formData.email}
                      onChange={handleChange}
                      required
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                    />
                  </div>
                  <div>
                    <label className="block text-gray-700 font-medium mb-2">
                      Phone Number <span className="text-red-500">*</span>
                    </label>
                    <input
                      type="tel"
                      name="phoneNumber"
                      value={formData.phoneNumber}
                      onChange={handleChange}
                      required
                      placeholder="+254..."
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                    />
                  </div>
                  <div className="md:col-span-2">
                    <label className="block text-gray-700 font-medium mb-2">
                      ID Number (Optional)
                    </label>
                    <input
                      type="text"
                      name="idNumber"
                      value={formData.idNumber}
                      onChange={handleChange}
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                    />
                  </div>
                </div>
              </div>

              {/* Lease Information */}
              <div>
                <h3 className="text-lg font-semibold text-gray-900 mb-4">Lease Information</h3>
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
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
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
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                    />
                  </div>
                  <div className="md:col-span-2">
                    <label className="block text-gray-700 font-medium mb-2">
                      Security Deposit (Optional)
                    </label>
                    <div className="relative">
                      <span className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-500">
                        KES
                      </span>
                      <input
                        type="number"
                        name="securityDeposit"
                        value={formData.securityDeposit}
                        onChange={handleChange}
                        placeholder="0.00"
                        step="0.01"
                        className="w-full pl-16 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                      />
                    </div>
                  </div>
                </div>
              </div>

              {/* Additional Notes */}
              <div>
                <label className="block text-gray-700 font-medium mb-2">
                  Additional Notes (Optional)
                </label>
                <textarea
                  name="applicationNotes"
                  value={formData.applicationNotes}
                  onChange={handleChange}
                  rows={4}
                  placeholder="Tell us more about yourself or ask any questions..."
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                />
              </div>

              {/* Submit Button */}
              <div className="flex gap-4">
                <button
                  type="submit"
                  disabled={submitting}
                  className="flex-1 bg-primary-600 text-white px-6 py-3 rounded-lg font-semibold hover:bg-primary-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {submitting ? 'Submitting...' : 'Submit Application'}
                </button>
                <Link
                  href="/"
                  className="px-6 py-3 border border-gray-300 rounded-lg font-semibold text-gray-700 hover:bg-gray-50 transition-colors"
                >
                  Cancel
                </Link>
              </div>

              <p className="text-gray-600 text-sm">
                By submitting this application, you agree to our terms and conditions.
                The landlord will review your application and contact you if approved.
              </p>
            </form>
          </div>
        </div>
      </div>
    </div>
  )
}
