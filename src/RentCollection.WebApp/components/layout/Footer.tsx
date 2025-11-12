export default function Footer() {
  const currentYear = new Date().getFullYear()

  return (
    <footer className="bg-white border-t border-gray-200 mt-auto">
      <div className="px-6 py-4">
        <div className="flex flex-col md:flex-row justify-between items-center space-y-2 md:space-y-0">
          {/* Copyright */}
          <div className="text-sm text-gray-600">
            &copy; {currentYear} RentPro. All rights reserved.
          </div>

          {/* Links */}
          <div className="flex items-center space-x-6 text-sm text-gray-600">
            <a href="#" className="hover:text-primary-600 transition-colors">
              Documentation
            </a>
            <a href="#" className="hover:text-primary-600 transition-colors">
              Support
            </a>
            <a href="#" className="hover:text-primary-600 transition-colors">
              Privacy Policy
            </a>
          </div>
        </div>
      </div>
    </footer>
  )
}
