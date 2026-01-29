import Link from 'next/link'
import { Facebook, Instagram, Linkedin, Twitter } from 'lucide-react'

export default function Footer() {
  return (
    <footer className="bg-brand-primary text-brand-bg">
      <div className="container mx-auto px-6 py-12">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-8">
          {/* Company Info */}
          <div>
            <h3 className="text-white font-semibold text-xl mb-4">Hisa Rentals</h3>
            <p className="text-sm text-brand-bg/80">
              Smart rent and property management for Kenyan landlords and agencies.
            </p>
            <div className="flex space-x-4 mt-4">
              <a href="#" className="hover:text-white transition-colors">
                <Facebook className="w-5 h-5" />
              </a>
              <a href="#" className="hover:text-white transition-colors">
                <Twitter className="w-5 h-5" />
              </a>
              <a href="#" className="hover:text-white transition-colors">
                <Instagram className="w-5 h-5" />
              </a>
              <a href="#" className="hover:text-white transition-colors">
                <Linkedin className="w-5 h-5" />
              </a>
            </div>
          </div>

          {/* Quick Links */}
          <div>
            <h4 className="text-white font-semibold mb-4">Quick Links</h4>
            <ul className="space-y-2 text-sm text-brand-bg/80">
              <li>
                <Link href="/#properties" className="hover:text-white transition-colors">
                  Browse Properties
                </Link>
              </li>
              <li>
                <Link href="/#how-it-works" className="hover:text-white transition-colors">
                  How It Works
                </Link>
              </li>
              <li>
                <Link href="/#about" className="hover:text-white transition-colors">
                  About Us
                </Link>
              </li>
              <li>
                <Link href="/login" className="hover:text-white transition-colors">
                  Login
                </Link>
              </li>
            </ul>
          </div>

          {/* For Landlords */}
          <div>
            <h4 className="text-white font-semibold mb-4">For Landlords</h4>
            <ul className="space-y-2 text-sm text-brand-bg/80">
              <li>
                <Link href="/login" className="hover:text-white transition-colors">
                  List Your Property
                </Link>
              </li>
              <li>
                <Link href="/login" className="hover:text-white transition-colors">
                  Manage Tenants
                </Link>
              </li>
              <li>
                <Link href="/login" className="hover:text-white transition-colors">
                  Track Payments
                </Link>
              </li>
              <li>
                <Link href="/login" className="hover:text-white transition-colors">
                  Generate Reports
                </Link>
              </li>
            </ul>
          </div>

          {/* Support */}
          <div>
            <h4 className="text-white font-semibold mb-4">Support</h4>
            <ul className="space-y-2 text-sm text-brand-bg/80">
              <li>
                <a href="#" className="hover:text-white transition-colors">
                  Help Center
                </a>
              </li>
              <li>
                <a href="#" className="hover:text-white transition-colors">
                  Contact Us
                </a>
              </li>
              <li>
                <a href="#" className="hover:text-white transition-colors">
                  Privacy Policy
                </a>
              </li>
              <li>
                <a href="#" className="hover:text-white transition-colors">
                  Terms of Service
                </a>
              </li>
            </ul>
          </div>
        </div>

        <div className="border-t border-brand-bg/20 mt-8 pt-8 text-center text-sm text-brand-bg/80">
          <p>&copy; {new Date().getFullYear()} Hisa Rentals. All rights reserved.</p>
        </div>
      </div>
    </footer>
  )
}
