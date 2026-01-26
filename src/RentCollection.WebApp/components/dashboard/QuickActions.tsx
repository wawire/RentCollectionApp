import Link from 'next/link'
import { Card } from '@/components/common'
import { Banknote, MessageSquare, Plus, UserPlus } from 'lucide-react'

export default function QuickActions() {
  const actions = [
    {
      label: 'Add Property',
      href: '/properties/new',
      icon: Plus,
      color: 'bg-blue-500 hover:bg-blue-600',
    },
    {
      label: 'Add Tenant',
      href: '/tenants/new',
      icon: UserPlus,
      color: 'bg-green-500 hover:bg-green-600',
    },
    {
      label: 'Record Payment',
      href: '/payments/new',
      icon: Banknote,
      color: 'bg-yellow-500 hover:bg-yellow-600',
    },
    {
      label: 'Send Reminder',
      href: '/notifications',
      icon: MessageSquare,
      color: 'bg-purple-500 hover:bg-purple-600',
    },
  ]

  return (
    <Card padding="md">
      <h2 className="text-lg font-semibold text-gray-900 mb-4">Quick Actions</h2>

      <div className="grid grid-cols-2 gap-3">
        {actions.map((action) => {
          const Icon = action.icon
          return (
            <Link
              key={action.href}
              href={action.href}
              className={`${action.color} text-white rounded-lg p-4 flex flex-col items-center justify-center space-y-2 transition-colors shadow-sm hover:shadow-md`}
            >
              <Icon className="w-6 h-6" />
              <span className="text-sm font-medium text-center">{action.label}</span>
            </Link>
          )
        })}
      </div>

      {/* Additional Links */}
      <div className="mt-4 pt-4 border-t border-gray-200 space-y-2">
        <Link
          href="/reports"
          className="block text-sm text-gray-700 hover:text-primary-600 transition-colors"
        >
          → Generate Monthly Report
        </Link>
        <Link
          href="/units"
          className="block text-sm text-gray-700 hover:text-primary-600 transition-colors"
        >
          → View Vacant Units
        </Link>
        <Link
          href="/tenants"
          className="block text-sm text-gray-700 hover:text-primary-600 transition-colors"
        >
          → Manage Tenants
        </Link>
      </div>
    </Card>
  )
}
