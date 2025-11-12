export interface DashboardStats {
  totalProperties: number
  totalUnits: number
  occupiedUnits: number
  vacantUnits: number
  totalTenants: number
  activeTenants: number
  totalRentCollected: number
  totalRentExpected: number
  collectionRate: number
  pendingPayments: number
}

export interface MonthlyReport {
  year: number
  month: number
  monthName: string
  totalRentCollected: number
  totalRentExpected: number
  numberOfPayments: number
  collectionRate: number
}
