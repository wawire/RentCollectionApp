import type { Metadata } from 'next'
import './globals.css'
import MainLayout from '@/components/layout/MainLayout'
import { AuthProvider } from '@/lib/contexts'
import { ToastProvider } from '@/components/common'

export const metadata: Metadata = {
  title: 'RentPro - Rent Collection System',
  description: 'Manage rental properties, tenants, and payments efficiently',
}

export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <html lang="en">
      <body className="antialiased">
        <AuthProvider>
          <ToastProvider>
            <MainLayout>{children}</MainLayout>
          </ToastProvider>
        </AuthProvider>
      </body>
    </html>
  )
}
