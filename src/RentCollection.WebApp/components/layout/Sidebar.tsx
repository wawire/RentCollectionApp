'use client'

import Link from 'next/link'
import { usePathname } from 'next/navigation'
import { useAuth } from '@/contexts/AuthContext'
import { UserRole } from '@/lib/types/auth.types'
import {
  FaHome,
  FaChartLine,
  FaBuilding,
  FaDoorOpen,
  FaUsers,
  FaMoneyBillWave,
  FaFileAlt,
  FaSms,
  FaCog,
  FaTimes,
  FaWallet,
  FaClipboardCheck,
  FaFileContract,
  FaBell,
} from 'react-icons/fa'

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
  const isLandlordOrAdmin = user?.role === UserRole.Landlord || user?.role === UserRole.SystemAdmin || user?.role === UserRole.Accountant || user?.role === UserRole.Caretaker

  const tenantNavItems = [
    { name: 'Dashboard', path: '/tenant-portal', icon: FaWallet },
    { name: 'Lease Info', path: '/tenant-portal/lease-info', icon: FaFileContract },
    { name: 'Documents', path: '/tenant-portal/documents', icon: FaFileAlt },
    { name: 'Payment History', path: '/tenant-portal/history', icon: FaMoneyBillWave },
  ]

  const landlordNavItems = [
    { name: 'Home', path: '/', icon: FaHome },
    { name: 'Dashboard', path: '/dashboard', icon: FaChartLine },
    { name: 'Properties', path: '/properties', icon: FaBuilding },
    { name: 'Units', path: '/units', icon: FaDoorOpen },
    { name: 'Tenants', path: '/tenants', icon: FaUsers },
    { name: 'Payments', path: '/payments', icon: FaMoneyBillWave },
    { name: 'Pending Payments', path: '/payments/pending', icon: FaClipboardCheck },
    { name: 'Documents', path: '/dashboard/documents', icon: FaFileAlt },
    { name: 'Notifications', path: '/dashboard/notifications', icon: FaBell },
    { name: 'Reports', path: '/reports', icon: FaFileContract },
  ]

  const navItems = isTenant ? tenantNavItems : landlordNavItems

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
            <FaBuilding className="text-primary-600 text-2xl" />
            <span className="text-xl font-bold text-gray-800">RentPro</span>
          </Link>
          <button
            onClick={onClose}
            className="md:hidden text-gray-600 hover:text-gray-900"
          >
            <FaTimes className="w-5 h-5" />
          </button>
        </div>

        {/* Navigation Links */}
        <nav className="flex-1 px-3 py-4 overflow-y-auto">
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

          {/* Settings Section */}
          <div className="mt-8 pt-4 border-t border-gray-200">
            <Link
              href={isTenant ? "/tenant-portal/settings" : "/settings"}
              className="flex items-center space-x-3 px-3 py-2.5 rounded-lg text-gray-700 hover:bg-gray-100 hover:text-gray-900 transition-colors"
            >
              <FaCog className="text-lg text-gray-500" />
              <span>Settings</span>
            </Link>
          </div>
        </nav>

        {/* Footer Info */}
        <div className="px-6 py-4 border-t border-gray-200">
          <div className="flex items-center space-x-2 text-sm text-gray-500">
            <div className="w-2 h-2 bg-green-500 rounded-full"></div>
            <span>All systems operational</span>
          </div>
          <p className="text-xs text-gray-400 mt-1">Version 1.0.0</p>
        </div>
      </aside>
    </>
  )
}
