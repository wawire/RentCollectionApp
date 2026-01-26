'use client'

import { useEffect, useState } from 'react'
import ProtectedRoute from '@/components/auth/ProtectedRoute'
import { Card, Button, Select, Alert, LoadingSpinner } from '@/components/common'
import { useGetProperties } from '@/lib/hooks/useProperties'
import { useGetUnitsByProperty } from '@/lib/hooks/useUnits'
import { utilityConfigService } from '@/lib/services/utilityConfigService'
import { meterReadingService } from '@/lib/services/meterReadingService'
import type {
  UtilityConfig,
  CreateMeterReadingDto,
  MeterReading,
} from '@/lib/types/utility.types'

export default function MeterReadingsPage() {
  const { data: properties } = useGetProperties()
  const [utilityConfigs, setUtilityConfigs] = useState<UtilityConfig[]>([])
  const [readings, setReadings] = useState<MeterReading[]>([])
  const [loading, setLoading] = useState(true)
  const [alert, setAlert] = useState<{ type: 'success' | 'error'; message: string } | null>(null)

  const [propertyId, setPropertyId] = useState<number | 'all'>('all')
  const [unitId, setUnitId] = useState<number | 'all'>('all')
  const [utilityConfigId, setUtilityConfigId] = useState<number | 'all'>('all')

  const [form, setForm] = useState<CreateMeterReadingDto>({
    utilityConfigId: 0,
    unitId: 0,
    readingDate: new Date().toISOString().slice(0, 10),
    readingValue: 0,
    photoUrl: '',
    notes: '',
  })

  const { data: units } = useGetUnitsByProperty(propertyId === 'all' ? null : propertyId)

  const loadConfigs = async (selectedPropertyId?: number) => {
    const configs = await utilityConfigService.getAll(selectedPropertyId)
    setUtilityConfigs(configs)
  }

  const loadReadings = async () => {
    try {
      setLoading(true)
      const data = await meterReadingService.getAll(
        propertyId === 'all' ? undefined : propertyId,
        unitId === 'all' ? undefined : unitId,
        utilityConfigId === 'all' ? undefined : utilityConfigId
      )
      setReadings(data)
    } catch (err: any) {
      setAlert({ type: 'error', message: err.message || 'Failed to load meter readings' })
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadConfigs()
    loadReadings()
  }, [])

  useEffect(() => {
    if (propertyId !== 'all') {
      loadConfigs(propertyId)
    }
  }, [propertyId])

  const handleCreateReading = async () => {
    try {
      if (!form.utilityConfigId || !form.unitId) {
        setAlert({ type: 'error', message: 'Utility config and unit are required' })
        return
      }
      const payload: CreateMeterReadingDto = {
        ...form,
        photoUrl: form.photoUrl || undefined,
        notes: form.notes || undefined,
      }
      await meterReadingService.create(payload)
      setAlert({ type: 'success', message: 'Meter reading saved' })
      await loadReadings()
    } catch (err: any) {
      setAlert({ type: 'error', message: err.message || 'Failed to save reading' })
    }
  }

  const propertyOptions = [
    { value: 'all', label: 'All Properties' },
    ...properties.map((p) => ({ value: p.id, label: p.name })),
  ]

  const unitOptions = [
    { value: 'all', label: 'All Units' },
    ...units.map((u) => ({ value: u.id, label: u.unitNumber })),
  ]

  const configOptions = [
    { value: 'all', label: 'All Utilities' },
    ...utilityConfigs.map((c) => ({
      value: c.id,
      label: `${c.utilityTypeName} - ${c.unitNumber || 'All Units'}`,
    })),
  ]

  if (loading) {
    return (
      <ProtectedRoute allowedRoles={['PlatformAdmin', 'Landlord', 'Manager', 'Caretaker']}>
        <div className="flex items-center justify-center h-96">
          <LoadingSpinner size="lg" text="Loading meter readings..." />
        </div>
      </ProtectedRoute>
    )
  }

  return (
    <ProtectedRoute allowedRoles={['PlatformAdmin', 'Landlord', 'Manager', 'Caretaker']}>
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-serif font-normal text-primary tracking-wide">Meter Readings</h1>
          <p className="text-primary/60 mt-2 tracking-wide">Capture and track metered utilities</p>
        </div>

        {alert && <Alert type={alert.type} message={alert.message} onClose={() => setAlert(null)} />}

        <Card padding="md">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">New Reading</h2>
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <Select
              label="Utility Config"
              value={form.utilityConfigId}
              onChange={(event) => setForm({ ...form, utilityConfigId: Number(event.target.value) })}
              options={[{ value: 0, label: 'Select Utility' }, ...configOptions.filter((o) => o.value !== 'all')]}
            />
            <Select
              label="Unit"
              value={form.unitId}
              onChange={(event) => setForm({ ...form, unitId: Number(event.target.value) })}
              options={[{ value: 0, label: 'Select Unit' }, ...unitOptions.filter((o) => o.value !== 'all')]}
            />
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Reading Date</label>
              <input
                type="date"
                value={form.readingDate}
                onChange={(e) => setForm({ ...form, readingDate: e.target.value })}
                className="w-full rounded-lg border border-gray-300 px-3 py-2"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Reading Value</label>
              <input
                type="number"
                value={form.readingValue}
                onChange={(e) => setForm({ ...form, readingValue: Number(e.target.value) })}
                className="w-full rounded-lg border border-gray-300 px-3 py-2"
              />
            </div>
          </div>
          <div className="mt-4">
            <Button variant="primary" onClick={handleCreateReading}>
              Save Reading
            </Button>
          </div>
        </Card>

        <Card padding="md">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Readings Log</h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
            <Select
              label="Property"
              value={propertyId}
              onChange={(event) => setPropertyId(event.target.value === 'all' ? 'all' : Number(event.target.value))}
              options={propertyOptions}
            />
            <Select
              label="Unit"
              value={unitId}
              onChange={(event) => setUnitId(event.target.value === 'all' ? 'all' : Number(event.target.value))}
              options={unitOptions}
            />
            <Select
              label="Utility"
              value={utilityConfigId}
              onChange={(event) => setUtilityConfigId(event.target.value === 'all' ? 'all' : Number(event.target.value))}
              options={configOptions}
            />
          </div>
          <div className="mb-4">
            <Button variant="secondary" onClick={loadReadings}>
              Refresh Readings
            </Button>
          </div>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="text-left text-gray-500 border-b">
                  <th className="py-2">Utility</th>
                  <th className="py-2">Unit</th>
                  <th className="py-2">Date</th>
                  <th className="py-2">Reading</th>
                </tr>
              </thead>
              <tbody>
                {readings.map((reading) => (
                  <tr key={reading.id} className="border-b last:border-b-0">
                    <td className="py-2">{reading.utilityName}</td>
                    <td className="py-2">{reading.unitNumber}</td>
                    <td className="py-2">{new Date(reading.readingDate).toLocaleDateString()}</td>
                    <td className="py-2">{reading.readingValue.toLocaleString()}</td>
                  </tr>
                ))}
                {readings.length === 0 && (
                  <tr>
                    <td colSpan={4} className="py-6 text-center text-gray-500">
                      No meter readings found
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </Card>
      </div>
    </ProtectedRoute>
  )
}

