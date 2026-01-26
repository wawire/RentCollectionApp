'use client'

import Link from 'next/link'
import { usePathname } from 'next/navigation'
import { useAuth } from '@/contexts/AuthContext'
import { UserRole } from '@/lib/types/auth.types'
import {
  Bell,
  Building2,
  ClipboardCheck,
  DoorOpen,
  FileSignature,
  FileText,
  Home,
  LineChart,
  MessageSquare,
  Receipt,
  Settings,
  Shield,
  Upload,
  Users,
  Wallet,
  Wrench,
  X,
  Banknote,
} from 'lucide-react'

interface SidebarProps {
  isOpen: boolean
  onClose: () => void
}

export default function Sidebar({ isOpen, onClose }: SidebarProps) {
  const pathname = usePathname()
  const { isAuthenticated, user } = useAuth()

  // Don't show sidebar on login/register pages or if not authenticated
  if (pathname === '/login' || pathname === '/register' || !isAuthenticated) {
    return null
  }

  // Role-based navigation items
  const isTenant = user?.role === UserRole.Tenant
  const isLandlordOrAdmin = user?.role === UserRole.Landlord || user?.role === UserRole.PlatformAdmin || user?.role === UserRole.Accountant || user?.role === UserRole.Caretaker

  const tenantNavItems = [
    { name: 'Dashboard', path: '/tenant-portal', icon: Wallet },
    { name: 'Lease Info', path: '/tenant-portal/lease-info', icon: FileSignature },
    { name: 'Maintenance', path: '/tenant-portal/maintenance', icon: Wrench },
    { name: 'Lease Renewals', path: '/tenant-portal/lease-renewals', icon: FileSignature },
    { name: 'Security Deposit', path: '/tenant-portal/security-deposit', icon: Shield },
    { name: 'Documents', path: '/tenant-portal/documents', icon: FileText },
    { name: 'Payment History', path: '/tenant-portal/history', icon: Banknote },
  ]

  const landlordNavSections = [
    {
      title: 'Overview',
      items: [
        { name: 'Home', path: '/', icon: Home },
        { name: 'Dashboard', path: '/dashboard', icon: LineChart },
      ]
    },
    {
      title: 'Property Management',
      items: [
        { name: 'Properties', path: '/properties', icon: Building2 },
        { name: 'Units', path: '/units', icon: DoorOpen },
        { name: 'Tenants', path: '/tenants', icon: Users },
      ]
    },
    {
      title: 'Financial',
      items: [
        { name: 'Payments', path: '/payments', icon: Banknote },
        { name: 'Pending Payments', path: '/payments/pending', icon: ClipboardCheck },
        { name: 'Payment Accounts', path: '/dashboard/payment-accounts', icon: Wallet },
        { name: 'Expenses', path: '/dashboard/expenses', icon: Receipt },
      ]
    },
    {
      title: 'Reports & Analytics',
      items: [
        { name: 'Reports', path: '/dashboard/reports', icon: FileSignature },
        { name: 'Rent Reminders', path: '/dashboard/reminders', icon: MessageSquare },
      ]
    },
    {
      title: 'Operations',
      items: [
        { name: 'Maintenance', path: '/dashboard/maintenance', icon: Wrench },
        { name: 'Lease Renewals', path: '/dashboard/lease-renewals', icon: FileSignature },
        { name: 'Security Deposits', path: '/dashboard/security-deposits', icon: Shield },
        { name: 'Documents', path: '/dashboard/documents', icon: FileText },
      ]
    },
    {
      title: 'System',
      items: [
        { name: 'Bulk Import', path: '/dashboard/bulk-import', icon: Upload },
        { name: 'Notifications', path: '/dashboard/notifications', icon: Bell },
      ]
    }
  ]

  const navItems = isTenant ? tenantNavItems : null
  const navSections = !isTenant ? landlordNavSections : null

  const isActive = (path: string) => {
    if (path === '/') {
      return pathname === path
    }
    return pathname.startsWith(path)
  }

  return (
    <>
      {/* Overlay for mobile */}
      {isOpen && (
        <div
          className="fixed inset-0 bg-black bg-opacity-50 z-20 md:hidden"
          onClick={onClose}
        ></div>
      )}

      {/* Sidebar */}
      <aside
        className={`fixed top-0 left-0 z-30 w-64 h-screen bg-white border-r border-gray-200 transition-transform duration-300 ${
          isOpen ? 'translate-x-0' : '-translate-x-full'
        } md:translate-x-0`}
      >
        {/* Logo Section */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-gray-200">
          <Link href="/" className="flex items-center space-x-2">
            <Building2 className="text-primary-600 text-2xl" />
            <span className="text-xl font-bold text-gray-800">RentPro</span>
          </Link>
          <button
            onClick={onClose}
            className="md:hidden text-gray-600 hover:text-gray-900"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Navigation Links */}
        <nav className="flex-1 px-3 py-4 overflow-y-auto">
          {/* Tenant Navigation (flat) */}
          {navItems && (
            <ul className="space-y-1">
              {navItems.map((item) => {
                const Icon = item.icon
                const active = isActive(item.path)

                return (
                  <li key={item.path}>
                    <Link
                      href={item.path}
                      onClick={() => onClose()}
                      className={`flex items-center space-x-3 px-3 py-2.5 rounded-lg transition-colors ${
                        active
                          ? 'bg-primary-50 text-primary-700 font-semibold'
                          : 'text-gray-700 hover:bg-gray-100 hover:text-gray-900'
                      }`}
                    >
                      <Icon className={`text-lg ${active ? 'text-primary-700' : 'text-gray-500'}`} />
                      <span>{item.name}</span>
                    </Link>
                  </li>
                )
              })}
            </ul>
          )}

          {/* Landlord Navigation (grouped) */}
          {navSections && (
            <div className="space-y-6">
              {navSections.map((section) => (
                <div key={section.title}>
                  <h3 className="px-3 mb-2 text-xs font-semibold text-gray-500 uppercase tracking-wider">
                    {section.title}
                  </h3>
                  <ul className="space-y-1">
                    {section.items.map((item) => {
                      const Icon = item.icon
                      const active = isActive(item.path)

                      return (
                        <li key={item.path}>
                          <Link
                            href={item.path}
                            onClick={() => onClose()}
                            className={`flex items-center space-x-3 px-3 py-2.5 rounded-lg transition-colors ${
                              active
                                ? 'bg-primary-50 text-primary-700 font-semibold'
                                : 'text-gray-700 hover:bg-gray-100 hover:text-gray-900'
                            }`}
                          >
                            <Icon className={`text-lg ${active ? 'text-primary-700' : 'text-gray-500'}`} />
                            <span>{item.name}</span>
                          </Link>
                        </li>
                      )
                    })}
                  </ul>
                </div>
              ))}
            </div>
          )}

          {/* Settings Section */}
          <div className="mt-8 pt-4 border-t border-gray-200">
            <Link
              href={isTenant ? "/tenant-portal/settings" : "/settings"}
              className="flex items-center space-x-3 px-3 py-2.5 rounded-lg text-gray-700 hover:bg-gray-100 hover:text-gray-900 transition-colors"
            >
              <Settings className="text-lg text-gray-500" />
              <span>Settings</span>
            </Link>
          </div>
        </nav>

        {/* Footer Info */}
        <div className="px-6 py-4 border-t border-gray-200">
          <p className="text-xs text-gray-400">Version 1.0.0</p>
        </div>
      </aside>
    </>
  )
}
