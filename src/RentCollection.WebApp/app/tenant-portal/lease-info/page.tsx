'use client'

import { useState, useEffect } from 'react'
import Link from 'next/link'
import { FaArrowLeft, FaHome, FaUser, FaFileContract, FaMoneyBillWave, FaSpinner, FaBuilding, FaDoorOpen } from 'react-icons/fa'
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
          <FaSpinner className="animate-spin text-6xl text-blue-600 mx-auto mb-4" />
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
            <FaArrowLeft className="mr-2" />
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
            <FaArrowLeft className="mr-2" />
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
              <FaUser className="mr-2 text-blue-600" />
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
              <div>
                <p className="text-sm text-gray-600">ID Number</p>
                <p className="text-lg font-medium text-gray-900">{tenant.idNumber}</p>
              </div>
              <div>
                <p className="text-sm text-gray-600">Occupation</p>
                <p className="text-lg font-medium text-gray-900">{tenant.occupation || 'N/A'}</p>
              </div>
              <div>
                <p className="text-sm text-gray-600">Emergency Contact</p>
                <p className="text-lg font-medium text-gray-900">
                  {tenant.emergencyContactName && tenant.emergencyContactPhone
                    ? `${tenant.emergencyContactName} (${tenant.emergencyContactPhone})`
                    : 'N/A'}
                </p>
              </div>
            </div>
          </div>

          {/* Property Information */}
          <div className="bg-white rounded-lg shadow-md p-6">
            <h2 className="text-xl font-semibold text-gray-900 mb-4 flex items-center">
              <FaBuilding className="mr-2 text-blue-600" />
              Property Information
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <p className="text-sm text-gray-600">Property Name</p>
                <p className="text-lg font-medium text-gray-900">{property.name}</p>
              </div>
              <div>
                <p className="text-sm text-gray-600">Property Type</p>
                <p className="text-lg font-medium text-gray-900">{property.propertyType}</p>
              </div>
              <div className="md:col-span-2">
                <p className="text-sm text-gray-600">Address</p>
                <p className="text-lg font-medium text-gray-900">{property.address}</p>
              </div>
              {property.description && (
                <div className="md:col-span-2">
                  <p className="text-sm text-gray-600">Description</p>
                  <p className="text-gray-900">{property.description}</p>
                </div>
              )}
            </div>
          </div>

          {/* Unit Information */}
          <div className="bg-white rounded-lg shadow-md p-6">
            <h2 className="text-xl font-semibold text-gray-900 mb-4 flex items-center">
              <FaDoorOpen className="mr-2 text-blue-600" />
              Unit Information
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <p className="text-sm text-gray-600">Unit Number</p>
                <p className="text-lg font-medium text-gray-900">{unit.unitNumber}</p>
              </div>
              <div>
                <p className="text-sm text-gray-600">Unit Type</p>
                <p className="text-lg font-medium text-gray-900">{unit.unitType}</p>
              </div>
              <div>
                <p className="text-sm text-gray-600">Bedrooms</p>
                <p className="text-lg font-medium text-gray-900">{unit.numberOfBedrooms}</p>
              </div>
              <div>
                <p className="text-sm text-gray-600">Bathrooms</p>
                <p className="text-lg font-medium text-gray-900">{unit.numberOfBathrooms}</p>
              </div>
              <div>
                <p className="text-sm text-gray-600">Square Footage</p>
                <p className="text-lg font-medium text-gray-900">
                  {unit.squareFootage ? `${unit.squareFootage} sq ft` : 'N/A'}
                </p>
              </div>
              <div>
                <p className="text-sm text-gray-600">Floor</p>
                <p className="text-lg font-medium text-gray-900">{unit.floor || 'N/A'}</p>
              </div>
              {unit.features && (
                <div className="md:col-span-2">
                  <p className="text-sm text-gray-600 mb-2">Features & Amenities</p>
                  <div className="flex flex-wrap gap-2">
                    {unit.features.split(',').map((feature, index) => (
                      <span
                        key={index}
                        className="px-3 py-1 bg-blue-50 text-blue-700 rounded-full text-sm"
                      >
                        {feature.trim()}
                      </span>
                    ))}
                  </div>
                </div>
              )}
            </div>
          </div>

          {/* Lease Details */}
          <div className="bg-white rounded-lg shadow-md p-6">
            <h2 className="text-xl font-semibold text-gray-900 mb-4 flex items-center">
              <FaFileContract className="mr-2 text-blue-600" />
              Lease Agreement
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <p className="text-sm text-gray-600">Monthly Rent</p>
                <p className="text-2xl font-bold text-gray-900">
                  KES {lease.rentAmount.toLocaleString()}
                </p>
              </div>
              <div>
                <p className="text-sm text-gray-600">Security Deposit</p>
                <p className="text-2xl font-bold text-gray-900">
                  KES {lease.securityDeposit.toLocaleString()}
                </p>
              </div>
              <div>
                <p className="text-sm text-gray-600">Lease Start Date</p>
                <p className="text-lg font-medium text-gray-900">
                  {new Date(lease.leaseStartDate).toLocaleDateString()}
                </p>
              </div>
              <div>
                <p className="text-sm text-gray-600">Lease End Date</p>
                <p className="text-lg font-medium text-gray-900">
                  {new Date(lease.leaseEndDate).toLocaleDateString()}
                </p>
              </div>
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
              {lease.specialTerms && (
                <div className="md:col-span-2">
                  <p className="text-sm text-gray-600 mb-2">Special Terms & Conditions</p>
                  <div className="p-4 bg-gray-50 rounded-lg">
                    <p className="text-gray-900 whitespace-pre-line">{lease.specialTerms}</p>
                  </div>
                </div>
              )}
            </div>
          </div>

          {/* Payment Account Information */}
          <div className="bg-white rounded-lg shadow-md p-6">
            <h2 className="text-xl font-semibold text-gray-900 mb-4 flex items-center">
              <FaMoneyBillWave className="mr-2 text-blue-600" />
              Payment Information
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <p className="text-sm text-gray-600">Payment Account Number</p>
                <p className="text-2xl font-bold text-blue-600 font-mono">
                  {paymentAccount.paymentAccountNumber}
                </p>
                <p className="text-xs text-gray-500 mt-1">
                  Use this account number when making M-Pesa payments
                </p>
              </div>
              <div>
                <p className="text-sm text-gray-600">Late Fee Policy</p>
                <p className="text-lg font-medium text-gray-900">
                  {paymentAccount.lateFeeType === 'Percentage'
                    ? `${paymentAccount.lateFeePercentage}% after grace period`
                    : `KES ${paymentAccount.lateFeeAmount?.toLocaleString()} after grace period`}
                </p>
              </div>
              <div>
                <p className="text-sm text-gray-600">Grace Period</p>
                <p className="text-lg font-medium text-gray-900">
                  {paymentAccount.gracePeriodDays} days
                </p>
              </div>
              <div>
                <p className="text-sm text-gray-600">Auto-Generate Payments</p>
                <p className="text-lg font-medium text-gray-900">
                  {paymentAccount.autoGeneratePayments ? 'Enabled' : 'Disabled'}
                </p>
              </div>
            </div>

            {/* Payment Instructions Box */}
            <div className="mt-6 p-4 bg-blue-50 border border-blue-200 rounded-lg">
              <h3 className="font-semibold text-blue-900 mb-2">How to Pay via M-Pesa</h3>
              <ol className="list-decimal list-inside space-y-1 text-sm text-blue-800">
                <li>Go to M-Pesa menu</li>
                <li>Select Lipa na M-Pesa</li>
                <li>Select Pay Bill</li>
                <li>Enter Business Number: <span className="font-bold">[Your Paybill]</span></li>
                <li>Enter Account Number: <span className="font-bold font-mono">{paymentAccount.paymentAccountNumber}</span></li>
                <li>Enter Amount: <span className="font-bold">KES {lease.rentAmount.toLocaleString()}</span></li>
                <li>Enter your M-Pesa PIN and confirm</li>
              </ol>
              <p className="mt-3 text-xs text-blue-700">
                Your payment will be automatically recorded in the system when you use the correct account number.
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
