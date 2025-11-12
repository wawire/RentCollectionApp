import { IconType } from 'react-icons'
import { Card } from '@/components/common'

interface StatsCardProps {
  title: string
  value: string | number
  subtitle?: string
  icon: IconType
  color: 'blue' | 'purple' | 'green' | 'yellow' | 'red'
  trend?: {
    value: number
    isPositive: boolean
  }
}

export default function StatsCard({ title, value, subtitle, icon: Icon, color, trend }: StatsCardProps) {
  const colorConfig = {
    blue: {
      bg: 'bg-blue-50',
      icon: 'text-blue-600',
      trend: 'text-blue-600',
    },
    purple: {
      bg: 'bg-purple-50',
      icon: 'text-purple-600',
      trend: 'text-purple-600',
    },
    green: {
      bg: 'bg-green-50',
      icon: 'text-green-600',
      trend: 'text-green-600',
    },
    yellow: {
      bg: 'bg-yellow-50',
      icon: 'text-yellow-600',
      trend: 'text-yellow-600',
    },
    red: {
      bg: 'bg-red-50',
      icon: 'text-red-600',
      trend: 'text-red-600',
    },
  }

  const config = colorConfig[color]

  return (
    <Card padding="md">
      <div className="flex items-start justify-between">
        <div className="flex-1">
          <p className="text-sm font-medium text-gray-600 mb-1">{title}</p>
          <p className="text-3xl font-bold text-gray-900">{value}</p>
          {subtitle && <p className="text-xs text-gray-500 mt-1">{subtitle}</p>}
          {trend && (
            <div className={`flex items-center mt-2 text-sm ${trend.isPositive ? 'text-green-600' : 'text-red-600'}`}>
              <span>{trend.isPositive ? '↑' : '↓'}</span>
              <span className="ml-1">{Math.abs(trend.value)}%</span>
              <span className="text-gray-500 ml-1">vs last month</span>
            </div>
          )}
        </div>
        <div className={`${config.bg} p-3 rounded-lg`}>
          <Icon className={`w-6 h-6 ${config.icon}`} />
        </div>
      </div>
    </Card>
  )
}
