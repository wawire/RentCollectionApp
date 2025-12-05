'use client'

import { useState } from 'react'
import Link from 'next/link'
import { FaBars, FaTimes } from 'react-icons/fa'

export default function Navbar() {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false)

  return (
    <nav className="bg-white border-b border-secondary/30 sticky top-0 z-50">
      <div className="container mx-auto px-6">
        <div className="flex items-center justify-between h-16">
          {/* Logo */}
          <Link href="/" className="flex items-center">
            <div className="text-primary font-serif font-normal text-xl tracking-wide">
              RentCollection
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
              <FaTimes className="text-xl text-primary" />
            ) : (
              <FaBars className="text-xl text-primary" />
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
