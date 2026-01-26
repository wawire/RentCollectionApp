'use client'

import { Card } from '@/components/common'
import { AlertTriangle, CheckCircle2, Clock } from 'lucide-react'

interface PaymentStatusWidgetProps {
  unitsPaid: number
  unitsOverdue: number
  unitsPending: number
  totalOccupied: number
}

export default function PaymentStatusWidget({
  unitsPaid,
  unitsOverdue,
  unitsPending,
  totalOccupied
}: PaymentStatusWidgetProps) {
  const paymentRate = totalOccupied > 0 ? (unitsPaid / totalOccupied) * 100 : 0

  return (
    <Card padding="lg">
      <div className="space-y-4">
        {/* Header */}
        <div className="flex items-center justify-between">
          <h3 className="text-lg font-semibold text-gray-900">Payment Status</h3>
          <div className="text-right">
            <p className="text-2xl font-bold text-primary-600">{paymentRate.toFixed(0)}%</p>
            <p className="text-xs text-gray-500">Payment Rate</p>
          </div>
        </div>

        {/* Payment Status Grid */}
        <div className="grid grid-cols-3 gap-4">
          {/* Paid */}
          <div className="text-center p-4 bg-green-50 rounded-lg border border-green-200">
            <div className="flex justify-center mb-2">
              <CheckCircle2 className="text-green-600 w-6 h-6" />
            </div>
            <p className="text-3xl font-bold text-green-600">{unitsPaid}</p>
            <p className="text-sm text-gray-600 mt-1">Paid</p>
            <p className="text-xs text-gray-500 mt-1">
              {totalOccupied > 0 ? ((unitsPaid / totalOccupied) * 100).toFixed(0) : 0}%
            </p>
          </div>

          {/* Pending */}
          <div className="text-center p-4 bg-yellow-50 rounded-lg border border-yellow-200">
            <div className="flex justify-center mb-2">
              <Clock className="text-yellow-600 w-6 h-6" />
            </div>
            <p className="text-3xl font-bold text-yellow-600">{unitsPending}</p>
            <p className="text-sm text-gray-600 mt-1">Pending</p>
            <p className="text-xs text-gray-500 mt-1">
              {totalOccupied > 0 ? ((unitsPending / totalOccupied) * 100).toFixed(0) : 0}%
            </p>
          </div>

          {/* Overdue */}
          <div className="text-center p-4 bg-red-50 rounded-lg border border-red-200">
            <div className="flex justify-center mb-2">
              <AlertTriangle className="text-red-600 w-6 h-6" />
            </div>
            <p className="text-3xl font-bold text-red-600">{unitsOverdue}</p>
            <p className="text-sm text-gray-600 mt-1">Overdue</p>
            <p className="text-xs text-gray-500 mt-1">
              {totalOccupied > 0 ? ((unitsOverdue / totalOccupied) * 100).toFixed(0) : 0}%
            </p>
          </div>
        </div>

        {/* Progress Bar */}
        <div className="space-y-2">
          <div className="flex items-center justify-between text-sm">
            <span className="text-gray-600">Overall Payment Status</span>
            <span className="font-semibold text-gray-900">
              {unitsPaid} / {totalOccupied} units paid
            </span>
          </div>
          <div className="w-full bg-gray-200 rounded-full h-3 overflow-hidden">
            <div className="flex h-full">
              {/* Paid segment */}
              <div
                className="bg-green-500 transition-all"
                style={{ width: `${totalOccupied > 0 ? (unitsPaid / totalOccupied) * 100 : 0}%` }}
              ></div>
              {/* Pending segment */}
              <div
                className="bg-yellow-500 transition-all"
                style={{ width: `${totalOccupied > 0 ? (unitsPending / totalOccupied) * 100 : 0}%` }}
              ></div>
              {/* Overdue segment */}
              <div
                className="bg-red-500 transition-all"
                style={{ width: `${totalOccupied > 0 ? (unitsOverdue / totalOccupied) * 100 : 0}%` }}
              ></div>
            </div>
          </div>
          <div className="flex items-center justify-between text-xs text-gray-500">
            <div className="flex items-center gap-1">
              <div className="w-3 h-3 bg-green-500 rounded"></div>
              <span>Paid</span>
            </div>
            <div className="flex items-center gap-1">
              <div className="w-3 h-3 bg-yellow-500 rounded"></div>
              <span>Pending</span>
            </div>
            <div className="flex items-center gap-1">
              <div className="w-3 h-3 bg-red-500 rounded"></div>
              <span>Overdue</span>
            </div>
          </div>
        </div>
      </div>
    </Card>
  )
}
