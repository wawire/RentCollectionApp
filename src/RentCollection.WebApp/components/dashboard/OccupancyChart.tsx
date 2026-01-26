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
      <h2 className="text-lg font-semibold text-text-primary mb-4">Unit Occupancy</h2>

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
              stroke="var(--color-brand-bg)"
              strokeWidth="12"
            />
            {/* Occupied arc */}
            <circle
              cx="50"
              cy="50"
              r="40"
              fill="none"
              stroke="var(--color-state-success)"
              strokeWidth="12"
              strokeDasharray={`${(occupancyRate / 100) * 251.2} 251.2`}
              strokeLinecap="round"
            />
          </svg>
          <div className="absolute inset-0 flex flex-col items-center justify-center">
            <span className="text-3xl font-semibold text-text-primary">{occupancyRate.toFixed(1)}%</span>
            <span className="text-sm text-text-muted">Occupied</span>
          </div>
        </div>

        {/* Legend */}
        <div className="mt-6 space-y-2 w-full">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-2">
              <div className="w-3 h-3 bg-state-success rounded-full"></div>
              <span className="text-sm text-text-secondary">Occupied Units</span>
            </div>
            <span className="text-sm font-semibold text-text-primary">
              {occupied} ({total > 0 ? ((occupied / total) * 100).toFixed(0) : 0}%)
            </span>
          </div>
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-2">
              <div className="w-3 h-3 bg-border-muted rounded-full"></div>
              <span className="text-sm text-text-secondary">Vacant Units</span>
            </div>
            <span className="text-sm font-semibold text-text-primary">
              {vacant} ({total > 0 ? ((vacant / total) * 100).toFixed(0) : 0}%)
            </span>
          </div>
          <div className="pt-2 border-t border-border-muted">
            <div className="flex items-center justify-between">
              <span className="text-sm font-medium text-text-secondary">Total Units</span>
              <span className="text-sm font-semibold text-text-primary">{total}</span>
            </div>
          </div>
        </div>
      </div>
    </Card>
  )
}
