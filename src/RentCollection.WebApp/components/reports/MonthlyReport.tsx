'use client'

import { useState } from 'react'
import { Card, Button, Select, LoadingSpinner, Alert } from '@/components/common'
import { useGetYearlyReports } from '@/lib/hooks'
import { useDownloadMonthlyReport } from '@/lib/hooks/useReports'
import { FaDownload, FaChartLine, FaMoneyBillWave, FaCheckCircle } from 'react-icons/fa'

export default function MonthlyReport() {
  const currentYear = new Date().getFullYear()
  const currentMonth = new Date().getMonth() + 1

  const [selectedYear, setSelectedYear] = useState(currentYear)
  const [selectedMonth, setSelectedMonth] = useState(currentMonth)
  const [alert, setAlert] = useState<{ type: 'success' | 'error', message: string } | null>(null)

  const { data: yearlyReports, loading } = useGetYearlyReports(selectedYear)
  const { downloadReport, loading: downloading } = useDownloadMonthlyReport()

  // Generate year options (last 5 years + current year)
  const yearOptions = Array.from({ length: 6 }, (_, i) => {
    const year = currentYear - i
    return { value: year, label: year.toString() }
  })

  const monthOptions = [
    { value: 1, label: 'January' },
    { value: 2, label: 'February' },
    { value: 3, label: 'March' },
    { value: 4, label: 'April' },
    { value: 5, label: 'May' },
    { value: 6, label: 'June' },
    { value: 7, label: 'July' },
    { value: 8, label: 'August' },
    { value: 9, label: 'September' },
    { value: 10, label: 'October' },
    { value: 11, label: 'November' },
    { value: 12, label: 'December' },
  ]

  const selectedMonthReport = yearlyReports.find(
    (report) => report.month === selectedMonth && report.year === selectedYear
  )

  const handleDownload = async () => {
    const success = await downloadReport(selectedYear, selectedMonth)
    if (success) {
      setAlert({ type: 'success', message: 'Report downloaded successfully!' })
      setTimeout(() => setAlert(null), 3000)
    } else {
      setAlert({ type: 'error', message: 'Failed to download report' })
      setTimeout(() => setAlert(null), 3000)
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold text-gray-900">Monthly Financial Report</h2>
      </div>

      {alert && (
        <Alert
          type={alert.type}
          message={alert.message}
          onClose={() => setAlert(null)}
        />
      )}

      {/* Selectors */}
      <Card padding="md">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <Select
            label="Year"
            value={selectedYear}
            onChange={(e) => setSelectedYear(Number(e.target.value))}
            options={yearOptions}
            fullWidth
          />
          <Select
            label="Month"
            value={selectedMonth}
            onChange={(e) => setSelectedMonth(Number(e.target.value))}
            options={monthOptions}
            fullWidth
          />
          <div className="flex items-end">
            <Button
              variant="primary"
              onClick={handleDownload}
              loading={downloading}
              fullWidth
            >
              <FaDownload className="mr-2" />
              Download PDF
            </Button>
          </div>
        </div>
      </Card>

      {/* Report Summary */}
      {loading ? (
        <div className="flex justify-center py-12">
          <LoadingSpinner size="lg" text="Loading report data..." />
        </div>
      ) : selectedMonthReport ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          <Card padding="md" className="bg-gradient-to-br from-blue-50 to-blue-100">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-blue-600 font-medium mb-1">Total Collected</p>
                <p className="text-2xl font-bold text-blue-900">
                  KSh {selectedMonthReport.totalRentCollected.toLocaleString()}
                </p>
              </div>
              <FaMoneyBillWave className="text-3xl text-blue-600" />
            </div>
          </Card>

          <Card padding="md" className="bg-gradient-to-br from-green-50 to-green-100">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-green-600 font-medium mb-1">Expected Rent</p>
                <p className="text-2xl font-bold text-green-900">
                  KSh {selectedMonthReport.totalRentExpected.toLocaleString()}
                </p>
              </div>
              <FaChartLine className="text-3xl text-green-600" />
            </div>
          </Card>

          <Card padding="md" className="bg-gradient-to-br from-purple-50 to-purple-100">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-purple-600 font-medium mb-1">Collection Rate</p>
                <p className="text-2xl font-bold text-purple-900">
                  {selectedMonthReport.collectionRate.toFixed(1)}%
                </p>
              </div>
              <FaCheckCircle className="text-3xl text-purple-600" />
            </div>
          </Card>

          <Card padding="md" className="bg-gradient-to-br from-orange-50 to-orange-100">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-orange-600 font-medium mb-1">Total Payments</p>
                <p className="text-2xl font-bold text-orange-900">
                  {selectedMonthReport.numberOfPayments}
                </p>
              </div>
              <FaCheckCircle className="text-3xl text-orange-600" />
            </div>
          </Card>
        </div>
      ) : (
        <Card padding="lg">
          <p className="text-center text-gray-500">No data available for the selected month</p>
        </Card>
      )}

      {/* Yearly Overview */}
      {!loading && yearlyReports.length > 0 && (
        <Card padding="md">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">
            Year {selectedYear} Overview
          </h3>
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead className="bg-gray-50 border-b border-gray-200">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Month</th>
                  <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase">Collected</th>
                  <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase">Expected</th>
                  <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase">Rate</th>
                  <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase">Payments</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200">
                {yearlyReports.map((report) => (
                  <tr key={report.month} className="hover:bg-gray-50">
                    <td className="px-4 py-3 text-sm font-medium text-gray-900">{report.monthName}</td>
                    <td className="px-4 py-3 text-sm text-right text-gray-900">
                      KSh {report.totalRentCollected.toLocaleString()}
                    </td>
                    <td className="px-4 py-3 text-sm text-right text-gray-600">
                      KSh {report.totalRentExpected.toLocaleString()}
                    </td>
                    <td className="px-4 py-3 text-sm text-right">
                      <span
                        className={`inline-flex px-2 py-1 rounded-full text-xs font-semibold ${
                          report.collectionRate >= 90
                            ? 'bg-green-100 text-green-800'
                            : report.collectionRate >= 70
                            ? 'bg-yellow-100 text-yellow-800'
                            : 'bg-red-100 text-red-800'
                        }`}
                      >
                        {report.collectionRate.toFixed(1)}%
                      </span>
                    </td>
                    <td className="px-4 py-3 text-sm text-right text-gray-900">
                      {report.numberOfPayments}
                    </td>
                  </tr>
                ))}
              </tbody>
              <tfoot className="bg-gray-50 border-t-2 border-gray-300">
                <tr>
                  <td className="px-4 py-3 text-sm font-bold text-gray-900">Total</td>
                  <td className="px-4 py-3 text-sm font-bold text-right text-gray-900">
                    KSh {yearlyReports.reduce((sum, r) => sum + r.totalRentCollected, 0).toLocaleString()}
                  </td>
                  <td className="px-4 py-3 text-sm font-bold text-right text-gray-900">
                    KSh {yearlyReports.reduce((sum, r) => sum + r.totalRentExpected, 0).toLocaleString()}
                  </td>
                  <td className="px-4 py-3 text-sm font-bold text-right text-gray-900">
                    {(
                      (yearlyReports.reduce((sum, r) => sum + r.totalRentCollected, 0) /
                        yearlyReports.reduce((sum, r) => sum + r.totalRentExpected, 0)) *
                      100
                    ).toFixed(1)}%
                  </td>
                  <td className="px-4 py-3 text-sm font-bold text-right text-gray-900">
                    {yearlyReports.reduce((sum, r) => sum + r.numberOfPayments, 0)}
                  </td>
                </tr>
              </tfoot>
            </table>
          </div>
        </Card>
      )}
    </div>
  )
}
