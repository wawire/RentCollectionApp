'use client'

import { useState, useEffect } from 'react'
import Link from 'next/link'
import { ArrowLeft, Banknote, Building2, DoorOpen, FileSignature, Loader2, User } from 'lucide-react'
import { tenantPortalService, TenantLeaseInfoDto } from '@/lib/services/tenantPortalService'

export default function LeaseInfoPage() {
  const [leaseInfo, setLeaseInfo] = useState<TenantLeaseInfoDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    fetchLeaseInfo()
  }, [])

  const fetchLeaseInfo = async () => {
    try {
      setLoading(true)
      const data = await tenantPortalService.getLeaseInfo()
      setLeaseInfo(data)
      setError(null)
    } catch (err: any) {
      setError(err.message || 'Failed to load lease information')
    } finally {
      setLoading(false)
    }
  }

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <Loader2 className="animate-spin text-6xl text-blue-600 mx-auto mb-4" />
          <p className="text-gray-600 text-lg">Loading lease information...</p>
        </div>
      </div>
    )
  }

  if (error || !leaseInfo) {
    return (
      <div className="min-h-screen bg-gray-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <Link
            href="/tenant-portal"
            className="inline-flex items-center text-blue-600 hover:text-blue-800 mb-4"
          >
            <ArrowLeft className="mr-2 w-4 h-4" />
            Back to Dashboard
          </Link>
          <div className="bg-red-50 border border-red-200 text-red-800 px-6 py-4 rounded-lg">
            {error || 'Failed to load lease information'}
          </div>
        </div>
      </div>
    )
  }

  const { tenant, property, unit, lease, paymentAccount } = leaseInfo

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Header */}
        <div className="mb-8">
          <Link
            href="/tenant-portal"
            className="inline-flex items-center text-blue-600 hover:text-blue-800 mb-4"
          >
            <ArrowLeft className="mr-2 w-4 h-4" />
            Back to Dashboard
          </Link>
          <h1 className="text-3xl font-bold text-gray-900">Lease Information</h1>
          <p className="mt-2 text-gray-600">
            View your complete lease agreement and property details
          </p>
        </div>

        <div className="space-y-6">
          {/* Tenant Information */}
          <div className="bg-white rounded-lg shadow-md p-6">
            <h2 className="text-xl font-semibold text-gray-900 mb-4 flex items-center">
              <User className="mr-2 text-blue-600 w-5 h-5" />
              Tenant Information
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <p className="text-sm text-gray-600">Full Name</p>
                <p className="text-lg font-medium text-gray-900">{tenant.fullName}</p>
              </div>
              <div>
                <p className="text-sm text-gray-600">Email</p>
                <p className="text-lg font-medium text-gray-900">{tenant.email}</p>
              </div>
              <div>
                <p className="text-sm text-gray-600">Phone Number</p>
                <p className="text-lg font-medium text-gray-900">{tenant.phoneNumber}</p>
              </div>
              {tenant.idNumber && (
                <div>
                  <p className="text-sm text-gray-600">ID Number</p>
                  <p className="text-lg font-medium text-gray-900">{tenant.idNumber}</p>
                </div>
              )}
            </div>
          </div>

          {/* Property Information */}
          <div className="bg-white rounded-lg shadow-md p-6">
            <h2 className="text-xl font-semibold text-gray-900 mb-4 flex items-center">
              <Building2 className="mr-2 text-blue-600 w-5 h-5" />
              Property Information
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <p className="text-sm text-gray-600">Property Name</p>
                <p className="text-lg font-medium text-gray-900">{property.name}</p>
              </div>
              {property.city && (
                <div>
                  <p className="text-sm text-gray-600">City</p>
                  <p className="text-lg font-medium text-gray-900">{property.city}</p>
                </div>
              )}
              {property.address && (
                <div className="md:col-span-2">
                  <p className="text-sm text-gray-600">Address</p>
                  <p className="text-lg font-medium text-gray-900">{property.address}</p>
                </div>
              )}
              {property.description && (
                <div className="md:col-span-2">
                  <p className="text-sm text-gray-600">Description</p>
                  <p className="text-gray-900">{property.description}</p>
                </div>
              )}
              {property.amenities && property.amenities.length > 0 && (
                <div className="md:col-span-2">
                  <p className="text-sm text-gray-600 mb-2">Property Amenities</p>
                  <div className="flex flex-wrap gap-2">
                    {property.amenities.map((amenity, index) => (
                      <span
                        key={index}
                        className="px-3 py-1 bg-blue-50 text-blue-700 rounded-full text-sm"
                      >
                        {amenity}
                      </span>
                    ))}
                  </div>
                </div>
              )}
            </div>
          </div>

          {/* Unit Information */}
          <div className="bg-white rounded-lg shadow-md p-6">
            <h2 className="text-xl font-semibold text-gray-900 mb-4 flex items-center">
              <DoorOpen className="mr-2 text-blue-600 w-5 h-5" />
              Unit Information
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <p className="text-sm text-gray-600">Unit Number</p>
                <p className="text-lg font-medium text-gray-900">{unit.unitNumber}</p>
              </div>
              <div>
                <p className="text-sm text-gray-600">Unit Type</p>
                <p className="text-lg font-medium text-gray-900">{unit.type}</p>
              </div>
              <div>
                <p className="text-sm text-gray-600">Bedrooms</p>
                <p className="text-lg font-medium text-gray-900">{unit.bedrooms}</p>
              </div>
              <div>
                <p className="text-sm text-gray-600">Bathrooms</p>
                <p className="text-lg font-medium text-gray-900">{unit.bathrooms}</p>
              </div>
              <div>
                <p className="text-sm text-gray-600">Square Footage</p>
                <p className="text-lg font-medium text-gray-900">
                  {unit.squareFeet ? `${unit.squareFeet} sq ft` : 'N/A'}
                </p>
              </div>
              {unit.description && (
                <div className="md:col-span-2">
                  <p className="text-sm text-gray-600">Description</p>
                  <p className="text-gray-900">{unit.description}</p>
                </div>
              )}
            </div>
          </div>

          {/* Lease Details */}
          <div className="bg-white rounded-lg shadow-md p-6">
            <h2 className="text-xl font-semibold text-gray-900 mb-4 flex items-center">
              <FileSignature className="mr-2 text-blue-600 w-5 h-5" />
              Lease Agreement
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <p className="text-sm text-gray-600">Monthly Rent</p>
                <p className="text-2xl font-bold text-gray-900">
                  KES {(lease.monthlyRent || 0).toLocaleString()}
                </p>
              </div>
              <div>
                <p className="text-sm text-gray-600">Security Deposit</p>
                <p className="text-2xl font-bold text-gray-900">
                  KES {(lease.securityDeposit || 0).toLocaleString()}
                </p>
              </div>
              <div>
                <p className="text-sm text-gray-600">Lease Start Date</p>
                <p className="text-lg font-medium text-gray-900">
                  {new Date(lease.startDate).toLocaleDateString()}
                </p>
              </div>
              {lease.endDate && (
                <div>
                  <p className="text-sm text-gray-600">Lease End Date</p>
                  <p className="text-lg font-medium text-gray-900">
                    {new Date(lease.endDate).toLocaleDateString()}
                    {lease.daysUntilExpiry !== undefined && lease.daysUntilExpiry > 0 && (
                      <span className="text-sm text-gray-500 ml-2">
                        ({lease.daysUntilExpiry} days remaining)
                      </span>
                    )}
                  </p>
                </div>
              )}
              <div>
                <p className="text-sm text-gray-600">Rent Due Day</p>
                <p className="text-lg font-medium text-gray-900">
                  {lease.rentDueDay}{lease.rentDueDay === 1 ? 'st' : lease.rentDueDay === 2 ? 'nd' : lease.rentDueDay === 3 ? 'rd' : 'th'} of each month
                </p>
              </div>
              <div>
                <p className="text-sm text-gray-600">Status</p>
                <span
                  className={`inline-flex px-3 py-1 rounded-full text-sm font-medium ${
                    lease.isActive
                      ? 'bg-green-100 text-green-800'
                      : 'bg-red-100 text-red-800'
                  }`}
                >
                  {lease.isActive ? 'Active' : 'Inactive'}
                </span>
              </div>
              <div className="md:col-span-2">
                <p className="text-sm text-gray-600 mb-2">Late Fee Policy</p>
                <div className="p-4 bg-gray-50 rounded-lg">
                  <p className="text-gray-900">{lease.lateFeePolicy}</p>
                  <p className="text-sm text-gray-600 mt-2">
                    Grace Period: {lease.lateFeeGracePeriodDays} days
                  </p>
                </div>
              </div>
            </div>
          </div>

          {/* Payment Account Information */}
          <div className="bg-white rounded-lg shadow-md p-6">
            <h2 className="text-xl font-semibold text-gray-900 mb-4 flex items-center">
              <Banknote className="mr-2 text-blue-600 w-5 h-5" />
              Payment Information
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <p className="text-sm text-gray-600">Account Type</p>
                <p className="text-lg font-medium text-gray-900">{paymentAccount.accountType}</p>
              </div>
              <div>
                <p className="text-sm text-gray-600">Account Name</p>
                <p className="text-lg font-medium text-gray-900">{paymentAccount.accountName}</p>
              </div>
              {paymentAccount.accountNumber && (
                <div>
                  <p className="text-sm text-gray-600">Account Number</p>
                  <p className="text-2xl font-bold text-blue-600 font-mono">
                    {paymentAccount.accountNumber}
                  </p>
                </div>
              )}
              {paymentAccount.mpesaPaybill && (
                <div>
                  <p className="text-sm text-gray-600">M-Pesa Paybill</p>
                  <p className="text-2xl font-bold text-blue-600 font-mono">
                    {paymentAccount.mpesaPaybill}
                  </p>
                </div>
              )}
              {paymentAccount.bankName && (
                <div>
                  <p className="text-sm text-gray-600">Bank Name</p>
                  <p className="text-lg font-medium text-gray-900">{paymentAccount.bankName}</p>
                </div>
              )}
            </div>

            {/* Payment Instructions Box */}
            <div className="mt-6 p-4 bg-blue-50 border border-blue-200 rounded-lg">
              <h3 className="font-semibold text-blue-900 mb-2">Payment Instructions</h3>
              <p className="text-sm text-blue-800 whitespace-pre-line">{paymentAccount.instructions}</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
