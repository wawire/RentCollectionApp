import UnitCard from './UnitCard'
import { Unit } from '@/lib/types'
import { Alert } from '@/components/common'

interface UnitListProps {
  units: Unit[]
  onUpdate: () => void
}

export default function UnitList({ units, onUpdate }: UnitListProps) {
  if (units.length === 0) {
    return (
      <div className="text-center py-12">
        <Alert type="info" message="No units found. Add your first unit to get started!" />
      </div>
    )
  }

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      {units.map((unit) => (
        <UnitCard key={unit.id} unit={unit} onUpdate={onUpdate} />
      ))}
    </div>
  )
}
