import TenantCard from './TenantCard'
import { Tenant } from '@/lib/types'
import { Alert } from '@/components/common'

interface TenantListProps {
  tenants: Tenant[]
  onUpdate: () => void
}

export default function TenantList({ tenants, onUpdate }: TenantListProps) {
  if (tenants.length === 0) {
    return (
      <div className="text-center py-12">
        <Alert type="info" message="No tenants found. Add your first tenant to get started!" />
      </div>
    )
  }

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      {tenants.map((tenant) => (
        <TenantCard key={tenant.id} tenant={tenant} onUpdate={onUpdate} />
      ))}
    </div>
  )
}
