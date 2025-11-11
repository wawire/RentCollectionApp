import Link from 'next/link'
import { FaBuilding, FaUsers, FaMoneyBillWave, FaChartLine } from 'react-icons/fa'

export default function Home() {
  return (
    <main className="min-h-screen bg-gradient-to-br from-primary-500 to-primary-700">
      <div className="container mx-auto px-4 py-16">
        <div className="text-center text-white mb-16">
          <h1 className="text-5xl font-bold mb-4">Rent Collection System</h1>
          <p className="text-xl opacity-90">Manage your rental properties efficiently</p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 max-w-6xl mx-auto">
          <Link href="/dashboard" className="card hover:shadow-xl transition-shadow">
            <div className="flex flex-col items-center text-center">
              <FaChartLine className="text-primary-600 text-5xl mb-4" />
              <h2 className="text-xl font-semibold mb-2">Dashboard</h2>
              <p className="text-gray-600">View statistics and reports</p>
            </div>
          </Link>

          <Link href="/properties" className="card hover:shadow-xl transition-shadow">
            <div className="flex flex-col items-center text-center">
              <FaBuilding className="text-primary-600 text-5xl mb-4" />
              <h2 className="text-xl font-semibold mb-2">Properties</h2>
              <p className="text-gray-600">Manage your properties</p>
            </div>
          </Link>

          <Link href="/tenants" className="card hover:shadow-xl transition-shadow">
            <div className="flex flex-col items-center text-center">
              <FaUsers className="text-primary-600 text-5xl mb-4" />
              <h2 className="text-xl font-semibold mb-2">Tenants</h2>
              <p className="text-gray-600">Manage tenant information</p>
            </div>
          </Link>

          <Link href="/payments" className="card hover:shadow-xl transition-shadow">
            <div className="flex flex-col items-center text-center">
              <FaMoneyBillWave className="text-primary-600 text-5xl mb-4" />
              <h2 className="text-xl font-semibold mb-2">Payments</h2>
              <p className="text-gray-600">Track rent payments</p>
            </div>
          </Link>
        </div>
      </div>
    </main>
  )
}
