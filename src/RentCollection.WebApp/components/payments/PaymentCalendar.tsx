'use client'

import { useState, useMemo } from 'react'
import { Payment } from '@/lib/types'
import { Card, Badge, Button } from '@/components/common'
import { FaChevronLeft, FaChevronRight, FaCheckCircle, FaClock, FaExclamationTriangle } from 'react-icons/fa'

interface PaymentCalendarProps {
  payments: Payment[]
}

export default function PaymentCalendar({ payments }: PaymentCalendarProps) {
  const [currentDate, setCurrentDate] = useState(new Date())

  // Get current month and year
  const year = currentDate.getFullYear()
  const month = currentDate.getMonth()

  // Get first day of month and number of days
  const firstDayOfMonth = new Date(year, month, 1)
  const lastDayOfMonth = new Date(year, month + 1, 0)
  const daysInMonth = lastDayOfMonth.getDate()
  const startingDayOfWeek = firstDayOfMonth.getDay()

  // Month navigation
  const goToPreviousMonth = () => {
    setCurrentDate(new Date(year, month - 1, 1))
  }

  const goToNextMonth = () => {
    setCurrentDate(new Date(year, month + 1, 1))
  }

  const goToToday = () => {
    setCurrentDate(new Date())
  }

  // Group payments by date
  const paymentsByDate = useMemo(() => {
    const grouped: Record<string, Payment[]> = {}

    payments.forEach(payment => {
      const paymentDate = new Date(payment.paymentDate)
      if (paymentDate.getMonth() === month && paymentDate.getFullYear() === year) {
        const dateKey = paymentDate.getDate().toString()
        if (!grouped[dateKey]) {
          grouped[dateKey] = []
        }
        grouped[dateKey].push(payment)
      }
    })

    return grouped
  }, [payments, month, year])

  // Calculate summary for a day
  const getDaySummary = (day: number) => {
    const paymentsForDay = paymentsByDate[day.toString()] || []
    const completed = paymentsForDay.filter(p => p.statusName === 'Completed').length
    const pending = paymentsForDay.filter(p => p.statusName === 'Pending').length
    const total = paymentsForDay.reduce((sum, p) => sum + p.amount, 0)

    return { completed, pending, total, count: paymentsForDay.length }
  }

  // Generate calendar days
  const calendarDays = []

  // Add empty cells for days before month starts
  for (let i = 0; i < startingDayOfWeek; i++) {
    calendarDays.push(<div key={`empty-${i}`} className="aspect-square"></div>)
  }

  // Add days of the month
  for (let day = 1; day <= daysInMonth; day++) {
    const summary = getDaySummary(day)
    const isToday = new Date().getDate() === day &&
                    new Date().getMonth() === month &&
                    new Date().getFullYear() === year

    calendarDays.push(
      <div
        key={day}
        className={`aspect-square border border-gray-200 p-2 ${
          isToday ? 'bg-primary-50 border-primary-400' : 'bg-white'
        } ${summary.count > 0 ? 'cursor-pointer hover:bg-gray-50' : ''}`}
      >
        <div className="h-full flex flex-col">
          <div className="flex items-center justify-between mb-1">
            <span className={`text-sm font-medium ${isToday ? 'text-primary-600' : 'text-gray-700'}`}>
              {day}
            </span>
            {isToday && (
              <div className="w-2 h-2 bg-primary-600 rounded-full"></div>
            )}
          </div>

          {summary.count > 0 && (
            <div className="flex-1 space-y-1">
              {summary.completed > 0 && (
                <div className="flex items-center gap-1 text-xs text-green-600">
                  <FaCheckCircle className="text-[10px]" />
                  <span>{summary.completed}</span>
                </div>
              )}
              {summary.pending > 0 && (
                <div className="flex items-center gap-1 text-xs text-yellow-600">
                  <FaClock className="text-[10px]" />
                  <span>{summary.pending}</span>
                </div>
              )}
              <div className="text-[10px] text-gray-600 font-semibold mt-1">
                KSh {summary.total.toLocaleString()}
              </div>
            </div>
          )}
        </div>
      </div>
    )
  }

  const monthNames = [
    'January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December'
  ]

  const dayNames = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat']

  // Calculate monthly totals
  const monthlyTotals = useMemo(() => {
    const completed = payments.filter(p => {
      const d = new Date(p.paymentDate)
      return d.getMonth() === month && d.getFullYear() === year && p.statusName === 'Completed'
    }).reduce((sum, p) => sum + p.amount, 0)

    const pending = payments.filter(p => {
      const d = new Date(p.paymentDate)
      return d.getMonth() === month && d.getFullYear() === year && p.statusName === 'Pending'
    }).reduce((sum, p) => sum + p.amount, 0)

    const total = payments.filter(p => {
      const d = new Date(p.paymentDate)
      return d.getMonth() === month && d.getFullYear() === year
    }).length

    return { completed, pending, total }
  }, [payments, month, year])

  return (
    <Card padding="lg">
      <div className="space-y-4">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div>
            <h2 className="text-2xl font-serif font-normal text-primary">
              {monthNames[month]} {year}
            </h2>
            <p className="text-sm text-gray-600 mt-1">
              {monthlyTotals.total} payment{monthlyTotals.total !== 1 ? 's' : ''} this month
            </p>
          </div>

          <div className="flex items-center gap-2">
            <Button variant="secondary" size="sm" onClick={goToPreviousMonth}>
              <FaChevronLeft />
            </Button>
            <Button variant="secondary" size="sm" onClick={goToToday}>
              Today
            </Button>
            <Button variant="secondary" size="sm" onClick={goToNextMonth}>
              <FaChevronRight />
            </Button>
          </div>
        </div>

        {/* Monthly Summary */}
        <div className="grid grid-cols-3 gap-4 p-4 bg-gray-50 rounded-lg">
          <div className="text-center">
            <p className="text-2xl font-bold text-green-600">
              KSh {monthlyTotals.completed.toLocaleString()}
            </p>
            <p className="text-xs text-gray-600 mt-1">Completed</p>
          </div>
          <div className="text-center border-l border-r border-gray-300">
            <p className="text-2xl font-bold text-yellow-600">
              KSh {monthlyTotals.pending.toLocaleString()}
            </p>
            <p className="text-xs text-gray-600 mt-1">Pending</p>
          </div>
          <div className="text-center">
            <p className="text-2xl font-bold text-primary-600">
              KSh {(monthlyTotals.completed + monthlyTotals.pending).toLocaleString()}
            </p>
            <p className="text-xs text-gray-600 mt-1">Total</p>
          </div>
        </div>

        {/* Legend */}
        <div className="flex items-center gap-4 text-xs text-gray-600">
          <div className="flex items-center gap-1">
            <FaCheckCircle className="text-green-600" />
            <span>Completed</span>
          </div>
          <div className="flex items-center gap-1">
            <FaClock className="text-yellow-600" />
            <span>Pending</span>
          </div>
        </div>

        {/* Calendar Grid */}
        <div>
          {/* Day headers */}
          <div className="grid grid-cols-7 gap-px mb-px">
            {dayNames.map(day => (
              <div
                key={day}
                className="text-center text-xs font-semibold text-gray-600 py-2 bg-gray-100"
              >
                {day}
              </div>
            ))}
          </div>

          {/* Calendar days */}
          <div className="grid grid-cols-7 gap-px bg-gray-200">
            {calendarDays}
          </div>
        </div>
      </div>
    </Card>
  )
}
