'use client'

import { useState, useEffect } from 'react'
import {
  ArrowDown,
  ArrowUp,
  Building2,
  Calendar,
  ChartPie,
  Home,
  PieChart,
  Users,
} from 'lucide-react'
import { reportService, ProfitLossReport } from '@/lib/services/reportService'
import MonthlyReport from '@/components/reports/MonthlyReport'
import PropertyReport from '@/components/reports/PropertyReport'
import TenantReport from '@/components/reports/TenantReport'
import ProtectedRoute from '@/components/auth/ProtectedRoute'
import { Alert, Card, EmptyState, Input, PageHeader, Skeleton, Table } from '@/components/common'

type TabType = 'profit-loss' | 'monthly' | 'property' | 'tenant'

export default function ReportsPage() {
  const [activeTab, setActiveTab] = useState<TabType>('profit-loss')
  const [loading, setLoading] = useState(true)
  const [, setReport] = useState<ProfitLossReport | null>(null)
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

  const tabs = [
    { id: 'profit-loss' as TabType, label: 'P&L Report', icon: PieChart },
    { id: 'monthly' as TabType, label: 'Monthly Report', icon: Calendar },
    { id: 'property' as TabType, label: 'Property Report', icon: Home },
    { id: 'tenant' as TabType, label: 'Tenant Directory', icon: Users },
  ]

  return (
    <ProtectedRoute allowedRoles={['PlatformAdmin', 'Landlord', 'Manager', 'Accountant']}>
      <div className="space-y-6">
        <PageHeader
          title="Reports & Analytics"
          subtitle="View and export detailed financial and operational reports."
          breadcrumbs={[
            { label: 'Dashboard', href: '/dashboard' },
            { label: 'Reports' },
          ]}
        />

        {activeTab === 'profit-loss' && error && (
          <Alert type="error" message={error} />
        )}

        <Card padding="none">
          <div className="border-b border-border-muted">
            <nav className="flex flex-wrap">
              {tabs.map((tab) => {
                const Icon = tab.icon
                const isActive = activeTab === tab.id
                return (
                  <button
                    key={tab.id}
                    onClick={() => setActiveTab(tab.id)}
                    className={`flex items-center gap-2 px-5 py-4 text-sm font-medium border-b-2 transition-colors ${
                      isActive
                        ? 'border-brand-primary text-brand-primary'
                        : 'border-transparent text-text-secondary hover:text-text-primary hover:border-border-muted'
                    }`}
                  >
                    <Icon className="w-4 h-4" />
                    {tab.label}
                  </button>
                )
              })}
            </nav>
          </div>
        </Card>

        {loading ? (
          <Card padding="md">
            <div className="space-y-4">
              <Skeleton height="20px" width="40%" />
              <Skeleton count={3} height="16px" />
              <Skeleton height="220px" />
            </div>
          </Card>
        ) : (
          <>
            {activeTab === 'monthly' && <MonthlyReport />}
            {activeTab === 'property' && <PropertyReport />}
            {activeTab === 'tenant' && <TenantReport />}
            {activeTab === 'profit-loss' && <ProfitLossContent />}
          </>
        )}
      </div>
    </ProtectedRoute>
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
      <Card padding="md">
        <div className="space-y-4">
          <Skeleton height="20px" width="40%" />
          <Skeleton count={3} height="16px" />
          <Skeleton height="220px" />
        </div>
      </Card>
    )
  }

  return (
    <div className="space-y-6">
      <Card padding="md">
        <div className="flex items-center gap-3">
          <Calendar className="text-text-muted w-4 h-4" />
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 w-full">
            <Input
              type="date"
              label="Start Date"
              value={startDate}
              onChange={(e) => setStartDate(e.target.value)}
              fullWidth
            />
            <Input
              type="date"
              label="End Date"
              value={endDate}
              onChange={(e) => setEndDate(e.target.value)}
              fullWidth
            />
          </div>
        </div>
      </Card>

      {error && (
        <Alert type="error" message={error} />
      )}

      {!report && !error && (
        <EmptyState
          title="No report data"
          description="Select a date range to generate the profit and loss report."
        />
      )}

      {report && (
        <>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            <Card className="border-l-4 border-state-success">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-text-secondary text-sm font-medium">Total Income</p>
                  <p className="text-2xl font-semibold text-text-primary mt-1">
                    {formatCurrency(report.totalIncome)}
                  </p>
                  <p className="text-xs text-text-muted mt-1">
                    Rent: {formatCurrency(report.totalRentCollected)}
                  </p>
                </div>
                <ArrowUp className="text-state-success w-8 h-8 opacity-30" />
              </div>
            </Card>

            <Card className="border-l-4 border-state-error">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-text-secondary text-sm font-medium">Total Expenses</p>
                  <p className="text-2xl font-semibold text-text-primary mt-1">
                    {formatCurrency(report.totalExpenses)}
                  </p>
                  <p className="text-xs text-text-muted mt-1">
                    {Object.keys(report.expensesByCategory).length} categories
                  </p>
                </div>
                <ArrowDown className="text-state-error w-8 h-8 opacity-30" />
              </div>
            </Card>

            <Card className={`border-l-4 ${report.netProfit >= 0 ? 'border-brand-primary' : 'border-state-error'}`}>
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-text-secondary text-sm font-medium">Net Profit</p>
                  <p className={`text-2xl font-semibold mt-1 ${report.netProfit >= 0 ? 'text-brand-primary' : 'text-state-error'}`}>
                    {formatCurrency(report.netProfit)}
                  </p>
                  <p className="text-xs text-text-muted mt-1">
                    Margin: {report.profitMargin.toFixed(1)}%
                  </p>
                </div>
                <ChartPie className={`w-8 h-8 opacity-30 ${report.netProfit >= 0 ? 'text-brand-primary' : 'text-state-error'}`} />
              </div>
            </Card>
          </div>

          <Card>
            <h2 className="text-lg font-semibold text-text-primary mb-4">Income Breakdown</h2>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
              <div className="bg-brand-bg/60 p-4 rounded-lg border border-border-muted">
                <p className="text-sm text-text-secondary">Rent Collected</p>
                <p className="text-lg font-semibold text-text-primary">{formatCurrency(report.totalRentCollected)}</p>
                <p className="text-xs text-text-muted mt-1">
                  {report.collectionRate.toFixed(1)}% collection rate
                </p>
              </div>
              <div className="bg-brand-bg/60 p-4 rounded-lg border border-border-muted">
                <p className="text-sm text-text-secondary">Rent Expected</p>
                <p className="text-lg font-semibold text-text-primary">{formatCurrency(report.totalRentExpected)}</p>
              </div>
              <div className="bg-brand-bg/60 p-4 rounded-lg border border-border-muted">
                <p className="text-sm text-text-secondary">Late Fees</p>
                <p className="text-lg font-semibold text-text-primary">{formatCurrency(report.lateFees)}</p>
              </div>
              <div className="bg-brand-bg/60 p-4 rounded-lg border border-border-muted">
                <p className="text-sm text-text-secondary">Security Deposits</p>
                <p className="text-lg font-semibold text-text-primary">{formatCurrency(report.securityDepositsReceived)}</p>
              </div>
            </div>
          </Card>

          {Object.keys(report.expensesByCategory).length > 0 && (
            <Card>
              <h2 className="text-lg font-semibold text-text-primary mb-4">Expense Breakdown by Category</h2>
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {Object.entries(report.expensesByCategory).map(([category, amount]) => {
                  const percentage = report.totalExpenses > 0
                    ? (amount / report.totalExpenses * 100).toFixed(1)
                    : '0.0'
                  return (
                    <div key={category} className="bg-brand-bg/60 p-4 rounded-lg border border-border-muted">
                      <div className="flex justify-between items-start mb-2">
                        <p className="text-sm text-text-secondary font-medium">{category}</p>
                        <p className="text-xs text-text-muted">{percentage}%</p>
                      </div>
                      <p className="text-lg font-semibold text-text-primary">{formatCurrency(amount)}</p>
                      <div className="mt-2 bg-border-muted rounded-full h-2">
                        <div
                          className="bg-brand-secondary rounded-full h-2"
                          style={{ width: `${percentage}%` }}
                        />
                      </div>
                    </div>
                  )
                })}
              </div>
            </Card>
          )}

          {report.propertiesBreakdown && report.propertiesBreakdown.length > 0 && (
            <Card>
              <div className="flex items-center gap-2 mb-4">
                <Building2 className="w-4 h-4 text-text-muted" />
                <h2 className="text-lg font-semibold text-text-primary">Property Performance</h2>
              </div>
              <Table
                data={report.propertiesBreakdown.map((property) => ({
                  ...property,
                  id: property.propertyId,
                }))}
                columns={[
                  {
                    key: 'propertyName',
                    header: 'Property',
                    render: (property) => (
                      <div>
                        <div className="text-sm font-semibold text-text-primary">{property.propertyName}</div>
                        <div className="text-xs text-text-muted">
                          {property.occupiedUnits}/{property.totalUnits} units
                        </div>
                      </div>
                    ),
                  },
                  {
                    key: 'totalIncome',
                    header: 'Income',
                    render: (property) => formatCurrency(property.totalIncome),
                  },
                  {
                    key: 'expenses',
                    header: 'Expenses',
                    render: (property) => (
                      <span className="text-state-error">{formatCurrency(property.expenses)}</span>
                    ),
                  },
                  {
                    key: 'netProfit',
                    header: 'Net Profit',
                    render: (property) => (
                      <span className={`font-semibold ${property.netProfit >= 0 ? 'text-state-success' : 'text-state-error'}`}>
                        {formatCurrency(property.netProfit)}
                      </span>
                    ),
                  },
                  {
                    key: 'occupancyRate',
                    header: 'Occupancy',
                    render: (property) => (
                      <div className="flex items-center gap-2">
                        <span className="text-sm text-text-primary">{property.occupancyRate.toFixed(0)}%</span>
                        <div className="w-16 bg-border-muted rounded-full h-2">
                          <div
                            className="bg-brand-secondary rounded-full h-2"
                            style={{ width: `${property.occupancyRate}%` }}
                          />
                        </div>
                      </div>
                    ),
                  },
                ]}
              />
            </Card>
          )}
        </>
      )}
    </div>
  )
}

