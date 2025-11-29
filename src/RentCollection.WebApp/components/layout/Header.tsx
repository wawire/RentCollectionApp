'use client'

import { useState } from 'react'
import { useAuth } from '@/lib/contexts/AuthContext'
import { useRouter, usePathname } from 'next/navigation'
import { FaBars, FaBell, FaSearch, FaUser, FaCog, FaSignOutAlt } from 'react-icons/fa'
import { useAuth } from '@/contexts/AuthContext'

interface HeaderProps {
  onMenuClick: () => void
}

export default function Header({ onMenuClick }: HeaderProps) {
  const { user, logout } = useAuth()
  const [showUserMenu, setShowUserMenu] = useState(false)
  const [showNotifications, setShowNotifications] = useState(false)

  // Get user initials
  const getInitials = (fullName: string) => {
    return fullName
      .split(' ')
      .map((n) => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2)
  }

  return (
    <header className="bg-white shadow-sm border-b border-gray-200 fixed top-0 right-0 left-0 md:left-64 z-10">
      <div className="flex items-center justify-between px-4 py-3">
        {/* Left Section */}
        <div className="flex items-center space-x-4">
          {/* Mobile Menu Toggle */}
          <button
            onClick={onMenuClick}
            className="md:hidden text-gray-600 hover:text-gray-900 focus:outline-none"
          >
            <FaBars className="w-6 h-6" />
          </button>

          {/* Search Bar */}
          <div className="hidden sm:flex items-center bg-gray-100 rounded-lg px-3 py-2 w-64 lg:w-96">
            <FaSearch className="text-gray-400 mr-2" />
            <input
              type="text"
              placeholder="Search properties, tenants..."
              className="bg-transparent border-none focus:outline-none w-full text-sm"
            />
          </div>
        </div>

        {/* Right Section */}
        <div className="flex items-center space-x-3">
          {/* Notifications */}
          <div className="relative">
            <button
              onClick={() => setShowNotifications(!showNotifications)}
              className="relative text-gray-600 hover:text-gray-900 p-2 rounded-lg hover:bg-gray-100 transition-colors"
            >
              <FaBell className="w-5 h-5" />
              <span className="absolute top-1 right-1 w-2 h-2 bg-red-500 rounded-full"></span>
            </button>

            {/* Notifications Dropdown */}
            {showNotifications && (
              <div className="absolute right-0 mt-2 w-80 bg-white rounded-lg shadow-lg border border-gray-200 py-2">
                <div className="px-4 py-2 border-b border-gray-200">
                  <h3 className="font-semibold text-gray-900">Notifications</h3>
                </div>
                <div className="max-h-96 overflow-y-auto">
                  <div className="px-4 py-3 hover:bg-gray-50 cursor-pointer">
                    <p className="text-sm font-medium text-gray-900">New payment received</p>
                    <p className="text-xs text-gray-500">Unit 12A - KSh 15,000</p>
                  </div>
                  <div className="px-4 py-3 hover:bg-gray-50 cursor-pointer">
                    <p className="text-sm font-medium text-gray-900">Rent reminder sent</p>
                    <p className="text-xs text-gray-500">5 tenants notified</p>
                  </div>
                </div>
                <div className="px-4 py-2 border-t border-gray-200 text-center">
                  <button className="text-sm text-primary-600 hover:text-primary-700 font-medium">
                    View all notifications
                  </button>
                </div>
              </div>
            )}
          </div>

          {/* User Menu */}
          {user && (
            <div className="relative">
              <button
                onClick={() => setShowUserMenu(!showUserMenu)}
                className="flex items-center space-x-2 p-2 rounded-lg hover:bg-gray-100 transition-colors"
              >
                <div className="w-8 h-8 bg-blue-600 rounded-full flex items-center justify-center text-white font-semibold text-sm">
                  {getInitials(user.fullName)}
                </div>
                <div className="hidden md:block text-left">
                  <p className="text-sm font-medium text-gray-900">{user.fullName}</p>
                  <p className="text-xs text-gray-500">{user.role}</p>
                </div>
              </button>

              {/* User Dropdown */}
              {showUserMenu && (
                <div className="absolute right-0 mt-2 w-48 bg-white rounded-lg shadow-lg border border-gray-200 py-2">
                  <div className="px-4 py-2 border-b border-gray-200">
                    <p className="text-sm font-medium text-gray-900">{user.fullName}</p>
                    <p className="text-xs text-gray-500">{user.email}</p>
                  </div>
                  <button className="w-full px-4 py-2 text-left text-sm text-gray-700 hover:bg-gray-100 flex items-center space-x-2">
                    <FaUser className="w-4 h-4" />
                    <span>Profile</span>
                  </button>
                  <button className="w-full px-4 py-2 text-left text-sm text-gray-700 hover:bg-gray-100 flex items-center space-x-2">
                    <FaCog className="w-4 h-4" />
                    <span>Settings</span>
                  </button>
                  <hr className="my-2 border-gray-200" />
                  <button
                    onClick={logout}
                    className="w-full px-4 py-2 text-left text-sm text-red-600 hover:bg-red-50 flex items-center space-x-2"
                  >
                    <FaSignOutAlt className="w-4 h-4" />
                    <span>Sign Out</span>
                  </button>
                </div>
              )}
            </div>
          )}
        </div>
      </div>
    </header>
  )
}
