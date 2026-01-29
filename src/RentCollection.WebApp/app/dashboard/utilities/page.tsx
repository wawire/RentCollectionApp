'use client'

import { useEffect, useState } from 'react'
import ProtectedRoute from '@/components/auth/ProtectedRoute'
import { Card, Button, Select, Alert, LoadingSpinner } from '@/components/common'
import { useAuth } from '@/contexts/AuthContext'
import { useGetProperties } from '@/lib/hooks/useProperties'
import { useGetUnitsByProperty } from '@/lib/hooks/useUnits'
import { utilityTypeService } from '@/lib/services/utilityTypeService'
import { utilityConfigService } from '@/lib/services/utilityConfigService'
import type {
  UtilityType,
  UtilityConfig,
  CreateUtilityTypeDto,
  CreateUtilityConfigDto,
  UtilityBillingMode,
} from '@/lib/types/utility.types'

const billingModeOptions = [
  { value: 'Fixed', label: 'Fixed' },
  { value: 'Metered', label: 'Metered' },
  { value: 'Shared', label: 'Shared' },
]

export default function UtilitiesPage() {
  const { user } = useAuth()
  const { data: properties } = useGetProperties()
  const [utilityTypes, setUtilityTypes] = useState<UtilityType[]>([])
  const [utilityConfigs, setUtilityConfigs] = useState<UtilityConfig[]>([])
  const [loading, setLoading] = useState(true)
  const [alert, setAlert] = useState<{ type: 'success' | 'error'; message: string } | null>(null)
  const canManageTypes = user?.role === 'PlatformAdmin'

  const [typeForm, setTypeForm] = useState<CreateUtilityTypeDto>({
    name: '',
    billingMode: 'Fixed',
    unitOfMeasure: '',
    description: '',
  })

  const [configForm, setConfigForm] = useState<CreateUtilityConfigDto>({
    utilityTypeId: 0,
    propertyId: 0,
    billingMode: 'Fixed',
    fixedAmount: undefined,
    rate: undefined,
    sharedAmount: undefined,
    effectiveFrom: new Date().toISOString().slice(0, 10),
    effectiveTo: '',
    notes: '',
  })

  const { data: units } = useGetUnitsByProperty(configForm.propertyId || null)

  const loadData = async () => {
    try {
      setLoading(true)
      const [types, configs] = await Promise.all([
        utilityTypeService.getAll(true),
        utilityConfigService.getAll(),
      ])
      setUtilityTypes(types)
      setUtilityConfigs(configs)
    } catch (err: any) {
      setAlert({ type: 'error', message: err.message || 'Failed to load utilities' })
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadData()
  }, [])

  const handleCreateType = async () => {
    try {
      const payload = {
        ...typeForm,
        unitOfMeasure: typeForm.unitOfMeasure?.trim() || undefined,
        description: typeForm.description?.trim() || undefined,
      }
      const created = await utilityTypeService.create(payload)
      setUtilityTypes((prev) => [...prev, created])
      setTypeForm({ name: '', billingMode: 'Fixed', unitOfMeasure: '', description: '' })
      setAlert({ type: 'success', message: 'Utility type created' })
    } catch (err: any) {
      setAlert({ type: 'error', message: err.message || 'Failed to create utility type' })
    }
  }

  const handleCreateConfig = async () => {
    try {
      if (!configForm.utilityTypeId || !configForm.propertyId) {
        setAlert({ type: 'error', message: 'Utility type and property are required' })
        return
      }

      const startDate = configForm.effectiveFrom ? new Date(configForm.effectiveFrom) : null
      const endDate = configForm.effectiveTo ? new Date(configForm.effectiveTo) : null
      if (startDate && endDate && endDate < startDate) {
        setAlert({ type: 'error', message: 'Effective end date cannot be before the start date' })
        return
      }

      if (configForm.fixedAmount !== undefined && configForm.fixedAmount < 0) {
        setAlert({ type: 'error', message: 'Fixed amount cannot be negative' })
        return
      }

      if (configForm.rate !== undefined && configForm.rate < 0) {
        setAlert({ type: 'error', message: 'Rate cannot be negative' })
        return
      }

      if (configForm.sharedAmount !== undefined && configForm.sharedAmount < 0) {
        setAlert({ type: 'error', message: 'Shared amount cannot be negative' })
        return
      }

      if (configForm.billingMode === 'Fixed' && (!configForm.fixedAmount || configForm.fixedAmount <= 0)) {
        setAlert({ type: 'error', message: 'Fixed amount is required for fixed utilities' })
        return
      }

      if (configForm.billingMode === 'Metered' && (!configForm.rate || configForm.rate <= 0)) {
        setAlert({ type: 'error', message: 'Rate is required for metered utilities' })
        return
      }

      if (configForm.billingMode === 'Shared' && (!configForm.sharedAmount || configForm.sharedAmount <= 0)) {
        setAlert({ type: 'error', message: 'Shared amount is required for shared utilities' })
        return
      }

      const payload: CreateUtilityConfigDto = {
        ...configForm,
        unitId: configForm.unitId || undefined,
        fixedAmount: configForm.fixedAmount ? Number(configForm.fixedAmount) : undefined,
        rate: configForm.rate ? Number(configForm.rate) : undefined,
        sharedAmount: configForm.sharedAmount ? Number(configForm.sharedAmount) : undefined,
        effectiveTo: configForm.effectiveTo || undefined,
        notes: configForm.notes || undefined,
      }

      const created = await utilityConfigService.create(payload)
      setUtilityConfigs((prev) => [created, ...prev])
      setAlert({ type: 'success', message: 'Utility configuration saved' })
    } catch (err: any) {
      setAlert({ type: 'error', message: err.message || 'Failed to save configuration' })
    }
  }

  const propertyOptions = [
    { value: 0, label: 'Select Property' },
    ...properties.map((p) => ({ value: p.id, label: p.name })),
  ]

  const unitOptions = [
    { value: 0, label: 'All Units (Property-level)' },
    ...units.map((u) => ({ value: u.id, label: u.unitNumber })),
  ]

  const utilityTypeOptions = [
    { value: 0, label: 'Select Utility Type' },
    ...utilityTypes.map((t) => ({ value: t.id, label: t.name })),
  ]

  return (
    <ProtectedRoute allowedRoles={['Landlord', 'Manager', 'Accountant']}>
      {loading ? (
        <div className="flex items-center justify-center h-96">
          <LoadingSpinner size="lg" text="Loading utilities..." />
        </div>
      ) : (
        <div className="space-y-6">
          <div>
            <h1 className="text-3xl font-serif font-normal text-primary tracking-wide">Utilities</h1>
            <p className="text-primary/60 mt-2 tracking-wide">Configure fixed, metered, or shared utilities</p>
          </div>

          {alert && <Alert type={alert.type} message={alert.message} onClose={() => setAlert(null)} />}

          {canManageTypes ? (
            <Card padding="md">
              <h2 className="text-lg font-semibold text-gray-900 mb-4">Utility Types</h2>
              <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Name</label>
                  <input
                    type="text"
                    value={typeForm.name}
                    onChange={(e) => setTypeForm({ ...typeForm, name: e.target.value })}
                    className="w-full rounded-lg border border-gray-300 px-3 py-2"
                  />
                </div>
                <Select
                  label="Billing Mode"
                  value={typeForm.billingMode}
                  onChange={(event) =>
                    setTypeForm({ ...typeForm, billingMode: event.target.value as UtilityBillingMode })
                  }
                  options={billingModeOptions}
                />
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Unit of Measure</label>
                  <input
                    type="text"
                    value={typeForm.unitOfMeasure}
                    onChange={(e) => setTypeForm({ ...typeForm, unitOfMeasure: e.target.value })}
                    className="w-full rounded-lg border border-gray-300 px-3 py-2"
                    placeholder="kWh, m3, etc."
                  />
                </div>
                <div className="flex items-end">
                  <Button variant="primary" onClick={handleCreateType}>
                    Add Utility Type
                  </Button>
                </div>
              </div>
            </Card>
          ) : (
            <Card padding="md">
              <p className="text-sm text-gray-600">
                Utility types are managed by system administrators.
              </p>
            </Card>
          )}

          <Card padding="md">
            <h2 className="text-lg font-semibold text-gray-900 mb-4">Utility Configurations</h2>
            <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
              <Select
                label="Utility Type"
                value={configForm.utilityTypeId}
                onChange={(event) =>
                  setConfigForm({ ...configForm, utilityTypeId: Number(event.target.value) })
                }
                options={utilityTypeOptions}
              />
              <Select
                label="Property"
                value={configForm.propertyId}
                onChange={(event) =>
                  setConfigForm({
                    ...configForm,
                    propertyId: Number(event.target.value),
                    unitId: 0,
                  })
                }
                options={propertyOptions}
              />
              <Select
                label="Unit"
                value={configForm.unitId ?? 0}
                onChange={(event) => setConfigForm({ ...configForm, unitId: Number(event.target.value) })}
                options={unitOptions}
              />
              <Select
                label="Billing Mode"
                value={configForm.billingMode}
                onChange={(event) =>
                  setConfigForm({ ...configForm, billingMode: event.target.value as UtilityBillingMode })
                }
                options={billingModeOptions}
              />
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Fixed Amount</label>
                <input
                  type="number"
                  value={configForm.fixedAmount || ''}
                  onChange={(e) => setConfigForm({ ...configForm, fixedAmount: Number(e.target.value) })}
                  className="w-full rounded-lg border border-gray-300 px-3 py-2"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Rate</label>
                <input
                  type="number"
                  value={configForm.rate || ''}
                  onChange={(e) => setConfigForm({ ...configForm, rate: Number(e.target.value) })}
                  className="w-full rounded-lg border border-gray-300 px-3 py-2"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Shared Amount</label>
                <input
                  type="number"
                  value={configForm.sharedAmount || ''}
                  onChange={(e) =>
                    setConfigForm({ ...configForm, sharedAmount: Number(e.target.value) })
                  }
                  className="w-full rounded-lg border border-gray-300 px-3 py-2"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Effective From</label>
                <input
                  type="date"
                  value={configForm.effectiveFrom}
                  onChange={(e) => setConfigForm({ ...configForm, effectiveFrom: e.target.value })}
                  className="w-full rounded-lg border border-gray-300 px-3 py-2"
                />
              </div>
            </div>
            <div className="mt-4">
              <Button variant="secondary" onClick={handleCreateConfig}>
                Save Configuration
              </Button>
            </div>
          </Card>

          <Card padding="md">
            <h2 className="text-lg font-semibold text-gray-900 mb-4">Configured Utilities</h2>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="text-left text-gray-500 border-b">
                    <th className="py-2">Utility</th>
                    <th className="py-2">Property</th>
                    <th className="py-2">Unit</th>
                    <th className="py-2">Mode</th>
                    <th className="py-2">Amount/Rate</th>
                    <th className="py-2">Active</th>
                  </tr>
                </thead>
                <tbody>
                  {utilityConfigs.map((config) => (
                    <tr key={config.id} className="border-b last:border-b-0">
                      <td className="py-2">{config.utilityTypeName}</td>
                      <td className="py-2">{config.propertyName}</td>
                      <td className="py-2">{config.unitNumber || 'All Units'}</td>
                      <td className="py-2">{config.billingMode}</td>
                      <td className="py-2">
                        {config.billingMode === 'Fixed' && `KSh ${config.fixedAmount?.toLocaleString() || 0}`}
                        {config.billingMode === 'Metered' &&
                          `KSh ${config.rate?.toLocaleString() || 0} per unit`}
                        {config.billingMode === 'Shared' &&
                          `KSh ${config.sharedAmount?.toLocaleString() || 0} total`}
                      </td>
                      <td className="py-2">{config.isActive ? 'Yes' : 'No'}</td>
                    </tr>
                  ))}
                  {utilityConfigs.length === 0 && (
                    <tr>
                      <td colSpan={6} className="py-6 text-center text-gray-500">
                        No utility configurations yet
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </Card>
        </div>
      )}
    </ProtectedRoute>
  )
}

