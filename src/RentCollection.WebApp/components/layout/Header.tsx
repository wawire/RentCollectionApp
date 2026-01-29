'use client'

import { useState } from 'react'
import { Bell, LogOut, Menu, Search, Settings, User } from 'lucide-react'
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
    <header className="bg-surface shadow-subtle border-b border-border-muted fixed top-0 right-0 left-0 md:left-64 z-10">
      <div className="flex items-center justify-between px-4 py-3">
        {/* Left Section */}
        <div className="flex items-center space-x-4">
          {/* Mobile Menu Toggle */}
          <button
            onClick={onMenuClick}
            className="md:hidden text-text-secondary hover:text-text-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-secondary/60 focus-visible:ring-offset-2 focus-visible:ring-offset-brand-bg rounded-md"
          >
            <Menu className="w-6 h-6" />
          </button>

          {/* Search Bar */}
          <div className="hidden sm:flex items-center bg-brand-bg rounded-lg px-3 py-2 w-64 lg:w-96 border border-border-muted">
            <Search className="text-text-muted mr-2 w-4 h-4" />
            <input
              type="text"
              placeholder="Search properties, tenants..."
              className="bg-transparent border-none focus:outline-none w-full text-sm text-text-primary placeholder:text-text-muted"
            />
          </div>
        </div>

        {/* Right Section */}
        <div className="flex items-center space-x-3">
          {/* Notifications */}
          <div className="relative">
            <button
              onClick={() => setShowNotifications(!showNotifications)}
              className="relative text-text-secondary hover:text-text-primary p-2 rounded-lg hover:bg-brand-bg/70 transition-colors"
            >
              <Bell className="w-5 h-5" />
              <span className="absolute top-1 right-1 w-2 h-2 bg-state-error rounded-full"></span>
            </button>

            {/* Notifications Dropdown */}
            {showNotifications && (
              <div className="absolute right-0 mt-2 w-80 bg-surface rounded-xl shadow-subtle border border-border-muted py-2">
                <div className="px-4 py-2 border-b border-border-muted">
                  <h3 className="font-semibold text-text-primary">Notifications</h3>
                </div>
                <div className="max-h-96 overflow-y-auto">
                  <div className="px-4 py-3 hover:bg-brand-bg/60 cursor-pointer">
                    <p className="text-sm font-medium text-text-primary">New payment received</p>
                    <p className="text-xs text-text-muted">Unit 12A - KSh 15,000</p>
                  </div>
                  <div className="px-4 py-3 hover:bg-brand-bg/60 cursor-pointer">
                    <p className="text-sm font-medium text-text-primary">Rent reminder sent</p>
                    <p className="text-xs text-text-muted">5 tenants notified</p>
                  </div>
                </div>
                <div className="px-4 py-2 border-t border-border-muted text-center">
                  <button className="text-sm text-brand-secondary hover:text-brand-primary font-medium">
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
                className="flex items-center space-x-2 p-2 rounded-lg hover:bg-brand-bg/70 transition-colors"
              >
                <div className="w-8 h-8 bg-brand-secondary rounded-full flex items-center justify-center text-white font-semibold text-sm">
                  {getInitials(user.fullName)}
                </div>
                <div className="hidden md:block text-left">
                  <p className="text-sm font-medium text-text-primary">{user.fullName}</p>
                  <p className="text-xs text-text-muted">{user.role}</p>
                </div>
              </button>

              {/* User Dropdown */}
              {showUserMenu && (
                <div className="absolute right-0 mt-2 w-48 bg-surface rounded-xl shadow-subtle border border-border-muted py-2">
                  <div className="px-4 py-2 border-b border-border-muted">
                    <p className="text-sm font-medium text-text-primary">{user.fullName}</p>
                    <p className="text-xs text-text-muted">{user.email}</p>
                  </div>
                  <button className="w-full px-4 py-2 text-left text-sm text-text-secondary hover:bg-brand-bg/60 flex items-center space-x-2">
                    <User className="w-4 h-4" />
                    <span>Profile</span>
                  </button>
                  <button className="w-full px-4 py-2 text-left text-sm text-text-secondary hover:bg-brand-bg/60 flex items-center space-x-2">
                    <Settings className="w-4 h-4" />
                    <span>Settings</span>
                  </button>
                  <hr className="my-2 border-border-muted" />
                  <button
                    onClick={logout}
                    className="w-full px-4 py-2 text-left text-sm text-state-error hover:bg-state-error/10 flex items-center space-x-2"
                  >
                    <LogOut className="w-4 h-4" />
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
