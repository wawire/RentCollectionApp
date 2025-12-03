import type { Metadata } from 'next'
import './globals.css'
import MainLayout from '@/components/layout/MainLayout'
import { AuthProvider } from '@/contexts/AuthContext'
import { urwClassico, fenwick, edmondsans } from '@/lib/fonts'

export const metadata: Metadata = {
  title: 'RentCollection - Property Management System',
  description: 'Manage rental properties, tenants, and payments efficiently',
}

export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <html lang="en" className={`${urwClassico.variable} ${fenwick.variable} ${edmondsans.variable}`}>
      <body className="antialiased font-sans">
        <AuthProvider>
          <MainLayout>{children}</MainLayout>
        </AuthProvider>
      </body>
    </html>
  )
}
