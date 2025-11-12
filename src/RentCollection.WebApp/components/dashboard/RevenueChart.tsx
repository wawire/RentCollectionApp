'use client'

import { useGetYearlyReports } from '@/lib/hooks'
import { Card, LoadingSpinner } from '@/components/common'
import { useState } from 'react'

export default function RevenueChart() {
  const currentYear = new Date().getFullYear()
  const [selectedYear, setSelectedYear] = useState(currentYear)
  const { data: reports, loading } = useGetYearlyReports(selectedYear)

  const maxRevenue = Math.max(...(reports?.map(r => r.totalRentCollected) || [0]), 1)

  return (
    <Card padding="md">
      <div className="flex items-center justify-between mb-6">
        <h2 className="text-lg font-semibold text-gray-900">Monthly Revenue Trend</h2>
        <select
          value={selectedYear}
          onChange={(e) => setSelectedYear(Number(e.target.value))}
          className="px-3 py-1.5 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary-500"
        >
          {[currentYear - 2, currentYear - 1, currentYear].map((year) => (
            <option key={year} value={year}>
              {year}
            </option>
          ))}
        </select>
      </div>

      {loading ? (
        <div className="py-8">
          <LoadingSpinner size="md" text="Loading revenue data..." />
        </div>
      ) : reports && reports.length > 0 ? (
        <div className="space-y-4">
          {/* Bar Chart */}
          <div className="h-64 flex items-end justify-between space-x-2">
            {reports.map((report) => {
              const height = (report.totalRentCollected / maxRevenue) * 100
              const isCurrentMonth = report.month === new Date().getMonth() + 1 && report.year === new Date().getFullYear()

              return (
                <div key={report.month} className="flex-1 flex flex-col items-center">
                  {/* Bar */}
                  <div className="w-full flex flex-col items-center justify-end h-56 relative group">
                    <div
                      className={`w-full rounded-t-lg transition-all duration-300 ${
                        isCurrentMonth ? 'bg-primary-600' : 'bg-primary-400 hover:bg-primary-500'
                      }`}
                      style={{ height: `${height}%` }}
                    >
                      {/* Tooltip */}
                      <div className="opacity-0 group-hover:opacity-100 absolute bottom-full mb-2 left-1/2 transform -translate-x-1/2 bg-gray-900 text-white text-xs rounded py-1 px-2 whitespace-nowrap transition-opacity">
                        <div className="font-semibold">KSh {report.totalRentCollected.toLocaleString()}</div>
                        <div className="text-gray-300">{report.numberOfPayments} payments</div>
                        <div className="text-gray-300">{report.collectionRate.toFixed(1)}% collected</div>
                      </div>
                    </div>
                  </div>
                  {/* Month Label */}
                  <div className="text-xs text-gray-600 mt-2 font-medium">
                    {report.monthName.slice(0, 3)}
                  </div>
                </div>
              )
            })}
          </div>

          {/* Summary Stats */}
          <div className="grid grid-cols-3 gap-4 pt-4 border-t border-gray-200">
            <div className="text-center">
              <p className="text-2xl font-bold text-gray-900">
                KSh {reports.reduce((sum, r) => sum + r.totalRentCollected, 0).toLocaleString()}
              </p>
              <p className="text-xs text-gray-500 mt-1">Total Collected</p>
            </div>
            <div className="text-center">
              <p className="text-2xl font-bold text-gray-900">
                KSh {reports.reduce((sum, r) => sum + r.totalRentExpected, 0).toLocaleString()}
              </p>
              <p className="text-xs text-gray-500 mt-1">Total Expected</p>
            </div>
            <div className="text-center">
              <p className="text-2xl font-bold text-green-600">
                {(
                  (reports.reduce((sum, r) => sum + r.totalRentCollected, 0) /
                    reports.reduce((sum, r) => sum + r.totalRentExpected, 0)) *
                  100
                ).toFixed(1)}%
              </p>
              <p className="text-xs text-gray-500 mt-1">Collection Rate</p>
            </div>
          </div>
        </div>
      ) : (
        <div className="text-center py-8 text-gray-500">
          <p>No revenue data available for {selectedYear}</p>
        </div>
      )}
    </Card>
  )
}
