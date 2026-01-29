'use client'

import { useState } from 'react'
import { Card } from '@/components/common'
import MonthlyReport from '@/components/reports/MonthlyReport'
import PropertyReport from '@/components/reports/PropertyReport'
import TenantReport from '@/components/reports/TenantReport'
import { BarChart3, Home, Users } from 'lucide-react'

type TabType = 'monthly' | 'property' | 'tenant'

export default function ReportsPage() {
  const [activeTab, setActiveTab] = useState<TabType>('monthly')

  const tabs = [
    { id: 'monthly' as TabType, label: 'Monthly Report', icon: BarChart3 },
    { id: 'property' as TabType, label: 'Property Report', icon: Home },
    { id: 'tenant' as TabType, label: 'Tenant Directory', icon: Users },
  ]

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-serif font-normal text-primary tracking-wide">Reports & Analytics</h1>
        <p className="text-primary/60 mt-2 tracking-wide">View and export detailed financial and operational reports</p>
      </div>

      {/* Tabs */}
      <Card padding="none">
        <div className="border-b border-gray-200">
          <nav className="flex -mb-px">
            {tabs.map((tab) => {
              const Icon = tab.icon
              const isActive = activeTab === tab.id
              return (
                <button
                  key={tab.id}
                  onClick={() => setActiveTab(tab.id)}
                  className={`
                    flex items-center px-6 py-4 text-sm font-medium border-b-2 transition-colors
                    ${
                      isActive
                        ? 'border-primary-500 text-primary-600'
                        : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                    }
                  `}
                >
                  <Icon className="mr-2" />
                  {tab.label}
                </button>
              )
            })}
          </nav>
        </div>
      </Card>

      {/* Tab Content */}
      <div className="mt-6">
        {activeTab === 'monthly' && <MonthlyReport />}
        {activeTab === 'property' && <PropertyReport />}
        {activeTab === 'tenant' && <TenantReport />}
      </div>
    </div>
  )
}
