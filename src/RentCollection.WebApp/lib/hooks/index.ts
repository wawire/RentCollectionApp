// Properties hooks
export {
  useGetProperties,
  useGetProperty,
  useCreateProperty,
  useUpdateProperty,
  useDeleteProperty,
} from './useProperties'

// Units hooks
export {
  useGetUnits,
  useGetUnit,
  useGetUnitsByProperty,
  useGetVacantUnits,
  useCreateUnit,
  useUpdateUnit,
  useDeleteUnit,
} from './useUnits'

// Tenants hooks
export {
  useGetTenants,
  useGetTenant,
  useGetActiveTenants,
  useCreateTenant,
  useUpdateTenant,
  useDeleteTenant,
  useActivateTenant,
  useDeactivateTenant,
} from './useTenants'

// Payments hooks
export {
  useGetPayments,
  useGetPayment,
  useGetTenantPayments,
  useGetPropertyPayments,
  useGetPaymentsByDateRange,
  useCreatePayment,
  useDeletePayment,
  useDownloadPaymentReceipt,
} from './usePayments'

// Dashboard hooks
export {
  useGetDashboardStats,
  useGetMonthlyReport,
  useGetYearlyReports,
} from './useDashboard'

// SMS hooks
export {
  useSendSms,
  useSendRentReminder,
  useSendPaymentReceipt,
} from './useSms'

// Reports hooks
export {
  useDownloadMonthlyReport,
  useDownloadTenantList,
} from './useReports'
