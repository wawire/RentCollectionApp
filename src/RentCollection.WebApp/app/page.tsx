'use client';

import Link from 'next/link'
import { FaBuilding, FaUsers, FaMoneyBillWave, FaChartLine, FaDoorOpen, FaFileInvoice } from 'react-icons/fa'
import Card from '@/components/common/Card'
import { ProtectedRoute } from '@/components/Auth/ProtectedRoute'

export default function Home() {
  const features = [
    {
      title: 'Dashboard',
      description: 'View real-time statistics, occupancy rates, and revenue metrics',
      icon: FaChartLine,
      href: '/dashboard',
      color: 'text-blue-600',
      bgColor: 'bg-blue-50',
    },
    {
      title: 'Properties',
      description: 'Manage your rental properties and track their performance',
      icon: FaBuilding,
      href: '/properties',
      color: 'text-primary-600',
      bgColor: 'bg-primary-50',
    },
    {
      title: 'Units',
      description: 'Add and manage individual units within your properties',
      icon: FaDoorOpen,
      href: '/units',
      color: 'text-purple-600',
      bgColor: 'bg-purple-50',
    },
    {
      title: 'Tenants',
      description: 'Manage tenant information, leases, and contact details',
      icon: FaUsers,
      href: '/tenants',
      color: 'text-green-600',
      bgColor: 'bg-green-50',
    },
    {
      title: 'Payments',
      description: 'Record and track rent payments with automated reminders',
      icon: FaMoneyBillWave,
      href: '/payments',
      color: 'text-yellow-600',
      bgColor: 'bg-yellow-50',
    },
    {
      title: 'Reports',
      description: 'Generate PDF receipts, monthly reports, and financial summaries',
      icon: FaFileInvoice,
      href: '/reports',
      color: 'text-red-600',
      bgColor: 'bg-red-50',
    },
  ]

  return (
    <ProtectedRoute>
      <div className="min-h-screen">
      {/* Hero Section */}
      <div className="bg-gradient-to-r from-primary-600 to-primary-700 text-white">
        <div className="container mx-auto px-6 py-20">
          <div className="max-w-3xl">
            <h1 className="text-5xl font-bold mb-4">
              Welcome to RentPro
            </h1>
            <p className="text-xl text-primary-100 mb-8">
              A comprehensive property management system to streamline your rental business.
              Manage properties, tenants, and payments all in one place.
            </p>
            <div className="flex gap-4">
              <Link
                href="/dashboard"
                className="bg-white text-primary-700 px-6 py-3 rounded-lg font-semibold hover:bg-primary-50 transition-colors"
              >
                Go to Dashboard
              </Link>
              <Link
                href="/properties"
                className="bg-primary-500 text-white px-6 py-3 rounded-lg font-semibold hover:bg-primary-400 transition-colors border-2 border-white"
              >
                Manage Properties
              </Link>
            </div>
          </div>
        </div>
      </div>

      {/* Features Grid */}
      <div className="container mx-auto px-6 py-16">
        <div className="text-center mb-12">
          <h2 className="text-3xl font-bold text-gray-900 mb-4">
            Everything You Need to Manage Your Rentals
          </h2>
          <p className="text-gray-600 max-w-2xl mx-auto">
            Our platform provides all the tools you need to efficiently manage your rental properties,
            from tenant onboarding to payment tracking.
          </p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 max-w-7xl mx-auto">
          {features.map((feature) => {
            const Icon = feature.icon
            return (
              <Link key={feature.href} href={feature.href} className="block">
                <Card hover className="h-full transition-all duration-200 hover:-translate-y-1">
                  <div className="flex flex-col h-full">
                    <div className={`w-12 h-12 rounded-lg ${feature.bgColor} flex items-center justify-center mb-4`}>
                      <Icon className={`text-2xl ${feature.color}`} />
                    </div>
                    <h3 className="text-xl font-semibold text-gray-900 mb-2">
                      {feature.title}
                    </h3>
                    <p className="text-gray-600 flex-grow">
                      {feature.description}
                    </p>
                    <div className="mt-4 text-primary-600 font-medium flex items-center">
                      <span>Learn more</span>
                      <svg className="w-4 h-4 ml-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                      </svg>
                    </div>
                  </div>
                </Card>
              </Link>
            )
          })}
        </div>
      </div>

      {/* Stats Section */}
      <div className="bg-white border-y border-gray-200">
        <div className="container mx-auto px-6 py-12">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-8">
            <div className="text-center">
              <div className="text-4xl font-bold text-primary-600 mb-2">100%</div>
              <div className="text-gray-600">Cloud-Based</div>
            </div>
            <div className="text-center">
              <div className="text-4xl font-bold text-primary-600 mb-2">24/7</div>
              <div className="text-gray-600">Access Anywhere</div>
            </div>
            <div className="text-center">
              <div className="text-4xl font-bold text-primary-600 mb-2">Secure</div>
              <div className="text-gray-600">Data Protection</div>
            </div>
            <div className="text-center">
              <div className="text-4xl font-bold text-primary-600 mb-2">Easy</div>
              <div className="text-gray-600">User-Friendly</div>
            </div>
          </div>
        </div>
      </div>
    </ProtectedRoute>
  )
}
