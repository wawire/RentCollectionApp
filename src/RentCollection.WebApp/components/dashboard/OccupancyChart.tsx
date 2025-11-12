import { Card } from '@/components/common'

interface OccupancyChartProps {
  occupied: number
  vacant: number
  occupancyRate: number
}

export default function OccupancyChart({ occupied, vacant, occupancyRate }: OccupancyChartProps) {
  const total = occupied + vacant

  return (
    <Card padding="md">
      <h2 className="text-lg font-semibold text-gray-900 mb-4">Unit Occupancy</h2>

      <div className="flex flex-col items-center">
        {/* Donut Chart */}
        <div className="relative w-48 h-48">
          <svg className="w-full h-full transform -rotate-90" viewBox="0 0 100 100">
            {/* Background circle */}
            <circle
              cx="50"
              cy="50"
              r="40"
              fill="none"
              stroke="#f3f4f6"
              strokeWidth="12"
            />
            {/* Occupied arc */}
            <circle
              cx="50"
              cy="50"
              r="40"
              fill="none"
              stroke="#10b981"
              strokeWidth="12"
              strokeDasharray={`${(occupancyRate / 100) * 251.2} 251.2`}
              strokeLinecap="round"
            />
          </svg>
          <div className="absolute inset-0 flex flex-col items-center justify-center">
            <span className="text-3xl font-bold text-gray-900">{occupancyRate.toFixed(1)}%</span>
            <span className="text-sm text-gray-500">Occupied</span>
          </div>
        </div>

        {/* Legend */}
        <div className="mt-6 space-y-2 w-full">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-2">
              <div className="w-3 h-3 bg-green-500 rounded-full"></div>
              <span className="text-sm text-gray-700">Occupied Units</span>
            </div>
            <span className="text-sm font-semibold text-gray-900">
              {occupied} ({total > 0 ? ((occupied / total) * 100).toFixed(0) : 0}%)
            </span>
          </div>
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-2">
              <div className="w-3 h-3 bg-gray-300 rounded-full"></div>
              <span className="text-sm text-gray-700">Vacant Units</span>
            </div>
            <span className="text-sm font-semibold text-gray-900">
              {vacant} ({total > 0 ? ((vacant / total) * 100).toFixed(0) : 0}%)
            </span>
          </div>
          <div className="pt-2 border-t border-gray-200">
            <div className="flex items-center justify-between">
              <span className="text-sm font-medium text-gray-700">Total Units</span>
              <span className="text-sm font-bold text-gray-900">{total}</span>
            </div>
          </div>
        </div>
      </div>
    </Card>
  )
}
