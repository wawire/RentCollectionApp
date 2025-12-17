'use client'

import { useState, useEffect } from 'react'
import { FaSpinner, FaChartPie, FaDollarSign, FaArrowUp, FaArrowDown, FaBuilding, FaCalendar, FaFileAlt, FaHome, FaUsers } from 'react-icons/fa'
import { reportService, ProfitLossReport } from '@/lib/services/reportService'
import MonthlyReport from '@/components/reports/MonthlyReport'
import PropertyReport from '@/components/reports/PropertyReport'
import TenantReport from '@/components/reports/TenantReport'

type TabType = 'profit-loss' | 'monthly' | 'property' | 'tenant'

export default function ReportsPage() {
  const [activeTab, setActiveTab] = useState<TabType>('profit-loss')
  const [loading, setLoading] = useState(true)
  const [report, setReport] = useState<ProfitLossReport | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [startDate, setStartDate] = useState('')
  const [endDate, setEndDate] = useState('')

  useEffect(() => {
    // Default to current month
    const now = new Date()
    const firstDay = new Date(now.getFullYear(), now.getMonth(), 1)
    const lastDay = new Date(now.getFullYear(), now.getMonth() + 1, 0)

    setStartDate(firstDay.toISOString().split('T')[0])
    setEndDate(lastDay.toISOString().split('T')[0])
  }, [])

  useEffect(() => {
    if (startDate && endDate) {
      loadReport()
    }
  }, [startDate, endDate])

  const loadReport = async () => {
    try {
      setLoading(true)
      setError(null)
      const data = await reportService.getProfitLossReport(startDate, endDate)
      setReport(data)
    } catch (err: any) {
      setError(err.message || 'Failed to load P&L report')
    } finally {
      setLoading(false)
    }
  }

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-KE', {
      style: 'currency',
      currency: 'KES'
    }).format(amount)
  }

  if (loading) {
    return (
      <div className="flex justify-center items-center min-h-screen">
        <FaSpinner className="animate-spin text-4xl text-blue-600" />
      </div>
    )
  }

  const tabs = [
    { id: 'profit-loss' as TabType, label: 'P&L Report', icon: FaChartPie },
    { id: 'monthly' as TabType, label: 'Monthly Report', icon: FaCalendar },
    { id: 'property' as TabType, label: 'Property Report', icon: FaHome },
    { id: 'tenant' as TabType, label: 'Tenant Directory', icon: FaUsers },
  ]

  return (
    <div className="p-6 max-w-7xl mx-auto">
      {/* Header */}
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-gray-800">Reports & Analytics</h1>
        <p className="text-gray-600 mt-1">View and export detailed financial and operational reports</p>
      </div>

      {/* Tabs */}
      <div className="bg-white rounded-lg shadow mb-6">
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
                        ? 'border-blue-600 text-blue-600'
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
      </div>

      {/* Tab Content */}
      {activeTab === 'monthly' && <MonthlyReport />}
      {activeTab === 'property' && <PropertyReport />}
      {activeTab === 'tenant' && <TenantReport />}
      {activeTab === 'profit-loss' && <ProfitLossContent />}
    </div>
  )
}

function ProfitLossContent() {
  const [loading, setLoading] = useState(true)
  const [report, setReport] = useState<ProfitLossReport | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [startDate, setStartDate] = useState('')
  const [endDate, setEndDate] = useState('')

  useEffect(() => {
    // Default to current month
    const now = new Date()
    const firstDay = new Date(now.getFullYear(), now.getMonth(), 1)
    const lastDay = new Date(now.getFullYear(), now.getMonth() + 1, 0)

    setStartDate(firstDay.toISOString().split('T')[0])
    setEndDate(lastDay.toISOString().split('T')[0])
  }, [])

  useEffect(() => {
    if (startDate && endDate) {
      loadReport()
    }
  }, [startDate, endDate])

  const loadReport = async () => {
    try {
      setLoading(true)
      setError(null)
      const data = await reportService.getProfitLossReport(startDate, endDate)
      setReport(data)
    } catch (err: any) {
      setError(err.message || 'Failed to load P&L report')
    } finally {
      setLoading(false)
    }
  }

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-KE', {
      style: 'currency',
      currency: 'KES'
    }).format(amount)
  }

  if (loading) {
    return (
      <div className="flex justify-center items-center py-12">
        <FaSpinner className="animate-spin text-4xl text-blue-600" />
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Header removed since it's now in parent */}

      {/* Date Range Selector */}
      <div className="bg-white rounded-lg shadow p-4 mb-6">
        <div className="flex items-center gap-4">
          <FaCalendar className="text-gray-600" />
          <div className="flex gap-4 flex-1">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Start Date</label>
              <input
                type="date"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
                className="border border-gray-300 rounded-lg px-4 py-2"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">End Date</label>
              <input
                type="date"
                value={endDate}
                onChange={(e) => setEndDate(e.target.value)}
                className="border border-gray-300 rounded-lg px-4 py-2"
              />
            </div>
          </div>
        </div>
      </div>

      {/* Error Message */}
      {error && (
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded mb-6">
          {error}
        </div>
      )}

      {/* Report */}
      {report && (
        <>
          {/* Summary Cards */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-6">
            {/* Total Revenue */}
            <div className="bg-white rounded-lg shadow p-6 border-l-4 border-green-600">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-gray-600 text-sm font-medium">Total Income</p>
                  <p className="text-2xl font-bold text-gray-800 mt-1">
                    {formatCurrency(report.totalIncome)}
                  </p>
                  <p className="text-xs text-gray-500 mt-1">
                    Rent: {formatCurrency(report.totalRentCollected)}
                  </p>
                </div>
                <FaArrowUp className="text-4xl text-green-600 opacity-20" />
              </div>
            </div>

            {/* Total Expenses */}
            <div className="bg-white rounded-lg shadow p-6 border-l-4 border-red-600">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-gray-600 text-sm font-medium">Total Expenses</p>
                  <p className="text-2xl font-bold text-gray-800 mt-1">
                    {formatCurrency(report.totalExpenses)}
                  </p>
                  <p className="text-xs text-gray-500 mt-1">
                    {Object.keys(report.expensesByCategory).length} categories
                  </p>
                </div>
                <FaArrowDown className="text-4xl text-red-600 opacity-20" />
              </div>
            </div>

            {/* Net Profit */}
            <div className={`bg-white rounded-lg shadow p-6 border-l-4 ${
              report.netProfit >= 0 ? 'border-blue-600' : 'border-red-600'
            }`}>
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-gray-600 text-sm font-medium">Net Profit</p>
                  <p className={`text-2xl font-bold mt-1 ${
                    report.netProfit >= 0 ? 'text-blue-600' : 'text-red-600'
                  }`}>
                    {formatCurrency(report.netProfit)}
                  </p>
                  <p className="text-xs text-gray-500 mt-1">
                    Margin: {report.profitMargin.toFixed(1)}%
                  </p>
                </div>
                <FaChartPie className={`text-4xl opacity-20 ${
                  report.netProfit >= 0 ? 'text-blue-600' : 'text-red-600'
                }`} />
              </div>
            </div>
          </div>

          {/* Income Breakdown */}
          <div className="bg-white rounded-lg shadow p-6 mb-6">
            <h2 className="text-xl font-bold text-gray-800 mb-4">Income Breakdown</h2>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
              <div className="bg-gray-50 p-4 rounded-lg">
                <p className="text-sm text-gray-600">Rent Collected</p>
                <p className="text-lg font-semibold text-gray-900">{formatCurrency(report.totalRentCollected)}</p>
                <p className="text-xs text-gray-500 mt-1">
                  {report.collectionRate.toFixed(1)}% collection rate
                </p>
              </div>
              <div className="bg-gray-50 p-4 rounded-lg">
                <p className="text-sm text-gray-600">Rent Expected</p>
                <p className="text-lg font-semibold text-gray-900">{formatCurrency(report.totalRentExpected)}</p>
              </div>
              <div className="bg-gray-50 p-4 rounded-lg">
                <p className="text-sm text-gray-600">Late Fees</p>
                <p className="text-lg font-semibold text-gray-900">{formatCurrency(report.lateFees)}</p>
              </div>
              <div className="bg-gray-50 p-4 rounded-lg">
                <p className="text-sm text-gray-600">Security Deposits</p>
                <p className="text-lg font-semibold text-gray-900">{formatCurrency(report.securityDepositsReceived)}</p>
              </div>
            </div>
          </div>

          {/* Expense Breakdown */}
          {Object.keys(report.expensesByCategory).length > 0 && (
            <div className="bg-white rounded-lg shadow p-6 mb-6">
              <h2 className="text-xl font-bold text-gray-800 mb-4">Expense Breakdown by Category</h2>
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {Object.entries(report.expensesByCategory).map(([category, amount]) => {
                  const percentage = report.totalExpenses > 0
                    ? (amount / report.totalExpenses * 100).toFixed(1)
                    : '0.0'
                  return (
                    <div key={category} className="bg-gray-50 p-4 rounded-lg">
                      <div className="flex justify-between items-start mb-2">
                        <p className="text-sm text-gray-600 font-medium">{category}</p>
                        <p className="text-xs text-gray-500">{percentage}%</p>
                      </div>
                      <p className="text-lg font-semibold text-gray-900">{formatCurrency(amount)}</p>
                      <div className="mt-2 bg-gray-200 rounded-full h-2">
                        <div
                          className="bg-blue-600 rounded-full h-2"
                          style={{ width: `${percentage}%` }}
                        />
                      </div>
                    </div>
                  )
                })}
              </div>
            </div>
          )}

          {/* Property Breakdown */}
          {report.propertiesBreakdown && report.propertiesBreakdown.length > 0 && (
            <div className="bg-white rounded-lg shadow p-6">
              <h2 className="text-xl font-bold text-gray-800 mb-4 flex items-center gap-2">
                <FaBuilding /> Property Performance
              </h2>
              <div className="overflow-x-auto">
                <table className="min-w-full divide-y divide-gray-200">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Property</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Income</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Expenses</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Net Profit</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Occupancy</th>
                    </tr>
                  </thead>
                  <tbody className="bg-white divide-y divide-gray-200">
                    {report.propertiesBreakdown.map((property) => (
                      <tr key={property.propertyId} className="hover:bg-gray-50">
                        <td className="px-6 py-4">
                          <div className="text-sm font-medium text-gray-900">{property.propertyName}</div>
                          <div className="text-xs text-gray-500">
                            {property.occupiedUnits}/{property.totalUnits} units
                          </div>
                        </td>
                        <td className="px-6 py-4 text-sm text-gray-900">{formatCurrency(property.totalIncome)}</td>
                        <td className="px-6 py-4 text-sm text-red-600">{formatCurrency(property.expenses)}</td>
                        <td className="px-6 py-4">
                          <span className={`text-sm font-semibold ${
                            property.netProfit >= 0 ? 'text-green-600' : 'text-red-600'
                          }`}>
                            {formatCurrency(property.netProfit)}
                          </span>
                        </td>
                        <td className="px-6 py-4">
                          <div className="flex items-center">
                            <div className="text-sm text-gray-900">{property.occupancyRate.toFixed(0)}%</div>
                            <div className="ml-2 w-16 bg-gray-200 rounded-full h-2">
                              <div
                                className="bg-blue-600 rounded-full h-2"
                                style={{ width: `${property.occupancyRate}%` }}
                              />
                            </div>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </>
      )}
    </div>
  )
}
