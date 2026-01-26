'use client'

import { useState, useRef, useEffect } from 'react'
import { usePathname, useRouter } from 'next/navigation'
import Link from 'next/link'
import { Bell, ChevronDown, LogOut, Menu, Settings, User, X } from 'lucide-react'
import { useAuth } from '@/contexts/AuthContext'

export default function Navbar() {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false)
  const [profileDropdownOpen, setProfileDropdownOpen] = useState(false)
  const [notificationsOpen, setNotificationsOpen] = useState(false)
  const profileRef = useRef<HTMLDivElement>(null)
  const notificationsRef = useRef<HTMLDivElement>(null)
  const { isAuthenticated, user, logout } = useAuth()
  const pathname = usePathname()
  const router = useRouter()

  // Close dropdowns when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (profileRef.current && !profileRef.current.contains(event.target as Node)) {
        setProfileDropdownOpen(false)
      }
      if (notificationsRef.current && !notificationsRef.current.contains(event.target as Node)) {
        setNotificationsOpen(false)
      }
    }

    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [])

  // Don't show navbar on login/register pages
  if (pathname === '/login' || pathname === '/register') {
    return null
  }

  // Get user initials for avatar
  const getUserInitials = () => {
    if (!user?.fullName) return 'U'
    const names = user.fullName.split(' ')
    if (names.length >= 2) {
      return `${names[0][0]}${names[1][0]}`.toUpperCase()
    }
    return names[0][0].toUpperCase()
  }

  // Authenticated Navbar
  if (isAuthenticated && user) {
    return (
      <nav className="bg-white border-b border-gray-200 sticky top-0 z-40">
        <div className="max-w-full mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between h-16">
            {/* Left: Logo */}
            <div className="flex items-center">
              <Link href={user.role === 'Tenant' ? '/tenant-portal' : '/dashboard'} className="flex items-center">
                <div className="text-gray-900 font-bold text-xl">
                  RentPro
                </div>
              </Link>
            </div>

            {/* Right: Notifications & Profile */}
            <div className="flex items-center gap-4">
              {/* Notifications */}
              <div className="relative" ref={notificationsRef}>
                <button
                  onClick={() => setNotificationsOpen(!notificationsOpen)}
                  className="p-2 rounded-lg hover:bg-gray-100 transition-colors relative"
                >
                  <Bell className="w-5 h-5 text-gray-600" />
                  {/* Notification badge */}
                  <span className="absolute top-1 right-1 w-2 h-2 bg-red-500 rounded-full"></span>
                </button>

                {/* Notifications Dropdown */}
                {notificationsOpen && (
                  <div className="absolute right-0 mt-2 w-80 bg-white rounded-lg shadow-lg border border-gray-200 py-2">
                    <div className="px-4 py-2 border-b border-gray-200">
                      <h3 className="font-semibold text-gray-900">Notifications</h3>
                    </div>
                    <div className="max-h-96 overflow-y-auto">
                      {/* Placeholder notifications */}
                      <div className="px-4 py-3 hover:bg-gray-50 cursor-pointer">
                        <p className="text-sm text-gray-900 font-medium">Payment received</p>
                        <p className="text-xs text-gray-500 mt-1">Tenant paid rent for January</p>
                        <p className="text-xs text-gray-400 mt-1">2 hours ago</p>
                      </div>
                      <div className="px-4 py-3 hover:bg-gray-50 cursor-pointer">
                        <p className="text-sm text-gray-900 font-medium">Maintenance request</p>
                        <p className="text-xs text-gray-500 mt-1">New request from Unit 2A</p>
                        <p className="text-xs text-gray-400 mt-1">5 hours ago</p>
                      </div>
                      <div className="px-4 py-3 text-center">
                        <Link
                          href="/dashboard/notifications"
                          className="text-sm text-blue-600 hover:text-blue-700 font-medium"
                          onClick={() => setNotificationsOpen(false)}
                        >
                          View all notifications
                        </Link>
                      </div>
                    </div>
                  </div>
                )}
              </div>

              {/* Profile Dropdown */}
              <div className="relative" ref={profileRef}>
                <button
                  onClick={() => setProfileDropdownOpen(!profileDropdownOpen)}
                  className="flex items-center gap-3 p-2 rounded-lg hover:bg-gray-100 transition-colors"
                >
                  {/* Avatar */}
                  <div className="w-8 h-8 rounded-full bg-blue-600 flex items-center justify-center text-white font-semibold text-sm">
                    {getUserInitials()}
                  </div>
                  <div className="hidden md:block text-left">
                    <p className="text-sm font-medium text-gray-900">{user.fullName}</p>
                    <p className="text-xs text-gray-500">{user.role}</p>
                  </div>
                  <ChevronDown className="w-4 h-4 text-gray-400" />
                </button>

                {/* Profile Dropdown Menu */}
                {profileDropdownOpen && (
                  <div className="absolute right-0 mt-2 w-56 bg-white rounded-lg shadow-lg border border-gray-200 py-1">
                    <div className="px-4 py-3 border-b border-gray-200">
                      <p className="text-sm font-medium text-gray-900">{user.fullName}</p>
                      <p className="text-xs text-gray-500 mt-1">{user.email}</p>
                    </div>

                    <Link
                      href={user.role === 'Tenant' ? '/tenant-portal' : '/dashboard'}
                      className="flex items-center gap-3 px-4 py-2 text-sm text-gray-700 hover:bg-gray-50"
                      onClick={() => setProfileDropdownOpen(false)}
                    >
                      <User className="w-4 h-4 text-gray-400" />
                      Dashboard
                    </Link>

                    <Link
                      href={user.role === 'Tenant' ? '/tenant-portal/settings' : '/settings'}
                      className="flex items-center gap-3 px-4 py-2 text-sm text-gray-700 hover:bg-gray-50"
                      onClick={() => setProfileDropdownOpen(false)}
                    >
                      <Settings className="w-4 h-4 text-gray-400" />
                      Settings
                    </Link>

                    <div className="border-t border-gray-200 mt-1 pt-1">
                      <button
                        onClick={() => {
                          setProfileDropdownOpen(false)
                          logout()
                        }}
                        className="flex items-center gap-3 px-4 py-2 text-sm text-red-600 hover:bg-red-50 w-full text-left"
                      >
                        <LogOut className="w-4 h-4" />
                        Sign out
                      </button>
                    </div>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      </nav>
    )
  }

  // Public Navbar (for unauthenticated users)
  return (
    <nav className="bg-white border-b border-secondary/30 sticky top-0 z-50">
      <div className="container mx-auto px-6">
        <div className="flex items-center justify-between h-16">
          {/* Logo */}
          <Link href="/" className="flex items-center">
            <div className="text-primary font-serif font-normal text-xl tracking-wide">
              RentPro
            </div>
          </Link>

          {/* Desktop Navigation */}
          <div className="hidden md:flex items-center space-x-8">
            <Link href="/" className="text-primary/70 hover:text-accent font-normal tracking-wide transition-colors text-sm">
              Properties
            </Link>
            <Link href="/#how-it-works" className="text-primary/70 hover:text-accent font-normal tracking-wide transition-colors text-sm">
              How It Works
            </Link>
            <Link href="/#for-landlords" className="text-primary/70 hover:text-accent font-normal tracking-wide transition-colors text-sm">
              For Landlords
            </Link>
            <Link href="/#contact" className="text-primary/70 hover:text-accent font-normal tracking-wide transition-colors text-sm">
              Contact
            </Link>
          </div>

          {/* Right side - Login & Sign Up */}
          <div className="hidden md:flex items-center space-x-3">
            <Link
              href="/login"
              className="px-4 py-2 text-sm font-medium text-primary/70 hover:text-primary transition-colors tracking-wide"
            >
              Login
            </Link>
            <Link
              href="/register"
              className="px-4 py-2 text-sm font-medium text-primary bg-accent hover:bg-accent-600 rounded-full transition-all tracking-wide shadow-sm"
            >
              Sign Up
            </Link>
          </div>

          {/* Mobile menu button */}
          <button
            onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
            className="md:hidden p-2 rounded-lg hover:bg-secondary/30 transition-colors"
          >
            {mobileMenuOpen ? (
              <X className="text-xl text-primary" />
            ) : (
              <Menu className="text-xl text-primary" />
            )}
          </button>
        </div>

        {/* Mobile Navigation */}
        {mobileMenuOpen && (
          <div className="md:hidden py-4 border-t border-secondary/30">
            <div className="flex flex-col space-y-3">
              <Link
                href="/"
                className="text-primary/70 hover:text-accent font-normal py-2 tracking-wide text-sm"
                onClick={() => setMobileMenuOpen(false)}
              >
                Properties
              </Link>
              <Link
                href="/#how-it-works"
                className="text-primary/70 hover:text-accent font-normal py-2 tracking-wide text-sm"
                onClick={() => setMobileMenuOpen(false)}
              >
                How It Works
              </Link>
              <Link
                href="/#for-landlords"
                className="text-primary/70 hover:text-accent font-normal py-2 tracking-wide text-sm"
                onClick={() => setMobileMenuOpen(false)}
              >
                For Landlords
              </Link>
              <Link
                href="/#contact"
                className="text-primary/70 hover:text-accent font-normal py-2 tracking-wide text-sm"
                onClick={() => setMobileMenuOpen(false)}
              >
                Contact
              </Link>
              <div className="border-t border-secondary/30 pt-4 flex flex-col space-y-2">
                <Link
                  href="/login"
                  className="text-primary/70 hover:text-accent font-normal py-2 tracking-wide text-sm"
                  onClick={() => setMobileMenuOpen(false)}
                >
                  Login
                </Link>
                <Link
                  href="/register"
                  className="px-4 py-2 text-sm font-medium text-primary bg-accent hover:bg-accent-600 rounded-full transition-all tracking-wide text-center shadow-sm"
                  onClick={() => setMobileMenuOpen(false)}
                >
                  Sign Up
                </Link>
              </div>
            </div>
          </div>
        )}
      </div>
    </nav>
  )
}
