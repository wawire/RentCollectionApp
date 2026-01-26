import { UserRole } from './auth.types'

export interface Organization {
  id: number
  name: string
  status: 'Pending' | 'Active' | 'Suspended'
  activatedAt?: string
  createdAt: string
}

export interface CreateOrganizationRequest {
  name: string
}

export interface AssignUserToPropertyRequest {
  userId: number
  assignmentRole: UserRole
}
