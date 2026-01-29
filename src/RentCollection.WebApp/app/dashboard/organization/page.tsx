'use client'

import { useEffect, useMemo, useState } from 'react'
import ProtectedRoute from '@/components/auth/ProtectedRoute'
import Card from '@/components/common/Card'
import Input from '@/components/common/Input'
import Select from '@/components/common/Select'
import Button from '@/components/common/Button'
import { apiClient } from '@/lib/services/api'
import { organizationService } from '@/lib/services/organizationService'
import { propertyService } from '@/lib/services/propertyService'
import { Organization, AssignUserToPropertyRequest } from '@/lib/types/organization.types'
import { Property } from '@/lib/types/property.types'
import { User, UserRole, RegisterRequest } from '@/lib/types/auth.types'

const roleOptions = [
  { value: UserRole.Landlord, label: 'Landlord' },
  { value: UserRole.Manager, label: 'Manager' },
  { value: UserRole.Caretaker, label: 'Caretaker' },
  { value: UserRole.Accountant, label: 'Accountant' },
]

const assignmentRoleOptions = [
  { value: UserRole.Manager, label: 'Manager' },
  { value: UserRole.Caretaker, label: 'Caretaker' },
  { value: UserRole.Accountant, label: 'Accountant' },
]

export default function OrganizationAdminPage() {
  const [organizations, setOrganizations] = useState<Organization[]>([])
  const [properties, setProperties] = useState<Property[]>([])
  const [users, setUsers] = useState<User[]>([])
  const [selectedOrganizationId, setSelectedOrganizationId] = useState<number | ''>('')
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)

  const [newOrganizationName, setNewOrganizationName] = useState('')
  const [userForm, setUserForm] = useState<RegisterRequest>({
    firstName: '',
    lastName: '',
    email: '',
    phoneNumber: '',
    password: '',
    confirmPassword: '',
    role: UserRole.Manager,
  })
  const [assignmentForm, setAssignmentForm] = useState<AssignUserToPropertyRequest>({
    userId: 0,
    assignmentRole: UserRole.Manager,
  })
  const [assignmentPropertyId, setAssignmentPropertyId] = useState<number | ''>('')

  const selectedOrganization = organizations.find((org) => org.id === selectedOrganizationId)

  const filteredUsers = useMemo(() => {
    if (!selectedOrganizationId) return []
    return users.filter((user) => user.organizationId === selectedOrganizationId)
  }, [users, selectedOrganizationId])

  const assignableUsers = useMemo(() => {
    return filteredUsers.filter((user) =>
      [UserRole.Manager, UserRole.Caretaker, UserRole.Accountant].includes(user.role)
    )
  }, [filteredUsers])

  const filteredProperties = useMemo(() => {
    if (!selectedOrganizationId) return []
    return properties.filter((property) => property.organizationId === selectedOrganizationId)
  }, [properties, selectedOrganizationId])

  const loadData = async () => {
    setIsLoading(true)
    setError(null)
    try {
      const [orgs, props, usersResponse] = await Promise.all([
        organizationService.list(),
        propertyService.getAll(),
        apiClient.get<User[]>('/auth'),
      ])

      setOrganizations(orgs)
      setProperties(props)
      setUsers(usersResponse.data)

      if (!selectedOrganizationId && orgs.length > 0) {
        setSelectedOrganizationId(orgs[0].id)
      }
    } catch (err) {
      setError('Failed to load organization data.')
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    void loadData()
  }, [])

  const handleCreateOrganization = async () => {
    if (!newOrganizationName.trim()) {
      setError('Organization name is required.')
      return
    }

    setIsLoading(true)
    setError(null)
    setSuccess(null)
    try {
      const created = await organizationService.create({ name: newOrganizationName.trim() })
      setOrganizations((prev) => [...prev, created])
      setSelectedOrganizationId(created.id)
      setNewOrganizationName('')
      setSuccess('Organization created.')
    } catch {
      setError('Failed to create organization.')
    } finally {
      setIsLoading(false)
    }
  }

  const handleCreateUser = async () => {
    if (!selectedOrganizationId) {
      setError('Select an organization first.')
      return
    }

    setIsLoading(true)
    setError(null)
    setSuccess(null)
    try {
      await organizationService.createUser(selectedOrganizationId, userForm)
      setUserForm({
        firstName: '',
        lastName: '',
        email: '',
        phoneNumber: '',
        password: '',
        confirmPassword: '',
        role: UserRole.Manager,
      })
      await loadData()
      setSuccess('User created and assigned to organization.')
    } catch {
      setError('Failed to create user.')
    } finally {
      setIsLoading(false)
    }
  }

  const handleAssignUser = async () => {
    if (!selectedOrganizationId) {
      setError('Select an organization first.')
      return
    }

    if (!assignmentPropertyId || !assignmentForm.userId) {
      setError('Select a user and property to assign.')
      return
    }

    setIsLoading(true)
    setError(null)
    setSuccess(null)
    try {
      await organizationService.assignUserToProperty(
        selectedOrganizationId,
        assignmentPropertyId,
        assignmentForm
      )
      setSuccess('User assigned to property.')
    } catch {
      setError('Failed to assign user to property.')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <ProtectedRoute allowedRoles={[UserRole.PlatformAdmin]}>
      <div className="min-h-screen bg-gray-50">
        <div className="container mx-auto px-6 py-10">
          <div className="mb-8">
            <h1 className="text-3xl font-semibold text-gray-900">Organization Admin</h1>
            <p className="text-gray-600 mt-2">
              Create organizations, add users, and assign property access.
            </p>
          </div>

          {(error || success) && (
            <div className="mb-6">
              {error && <div className="rounded-lg bg-red-50 text-red-700 px-4 py-2">{error}</div>}
              {success && <div className="rounded-lg bg-green-50 text-green-700 px-4 py-2">{success}</div>}
            </div>
          )}

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            <Card className="p-6">
              <h2 className="text-xl font-semibold text-gray-900 mb-4">Organizations</h2>
              <div className="space-y-4">
                <Input
                  label="New organization name"
                  value={newOrganizationName}
                  onChange={(event) => setNewOrganizationName(event.target.value)}
                  placeholder="e.g. Horizon Property Group"
                  fullWidth
                />
                <Button onClick={handleCreateOrganization} loading={isLoading} fullWidth>
                  Create Organization
                </Button>
                <Select
                  label="Active organization"
                  value={selectedOrganizationId}
                  onChange={(event) => setSelectedOrganizationId(Number(event.target.value))}
                  options={organizations.map((org) => ({ value: org.id, label: org.name }))}
                  placeholder={organizations.length ? 'Select organization' : 'No organizations yet'}
                  fullWidth
                />
                {selectedOrganization && (
                  <div className="rounded-lg border border-gray-200 p-3 text-sm text-gray-600">
                    <div className="font-medium text-gray-900">{selectedOrganization.name}</div>
                    <div>Created {new Date(selectedOrganization.createdAt).toLocaleDateString()}</div>
                  </div>
                )}
              </div>
            </Card>

            <Card className="p-6">
              <h2 className="text-xl font-semibold text-gray-900 mb-4">Create User</h2>
              <div className="space-y-4">
                <Input
                  label="First name"
                  value={userForm.firstName}
                  onChange={(event) => setUserForm({ ...userForm, firstName: event.target.value })}
                  fullWidth
                />
                <Input
                  label="Last name"
                  value={userForm.lastName}
                  onChange={(event) => setUserForm({ ...userForm, lastName: event.target.value })}
                  fullWidth
                />
                <Input
                  label="Email"
                  type="email"
                  value={userForm.email}
                  onChange={(event) => setUserForm({ ...userForm, email: event.target.value })}
                  fullWidth
                />
                <Input
                  label="Phone number"
                  value={userForm.phoneNumber}
                  onChange={(event) => setUserForm({ ...userForm, phoneNumber: event.target.value })}
                  fullWidth
                />
                <Select
                  label="Role"
                  value={userForm.role}
                  onChange={(event) =>
                    setUserForm({ ...userForm, role: event.target.value as UserRole })
                  }
                  options={roleOptions}
                  fullWidth
                />
                <Input
                  label="Temporary password"
                  type="password"
                  value={userForm.password}
                  onChange={(event) => setUserForm({ ...userForm, password: event.target.value })}
                  fullWidth
                />
                <Input
                  label="Confirm password"
                  type="password"
                  value={userForm.confirmPassword}
                  onChange={(event) =>
                    setUserForm({ ...userForm, confirmPassword: event.target.value })
                  }
                  fullWidth
                />
                <Button onClick={handleCreateUser} loading={isLoading} fullWidth>
                  Create User
                </Button>
              </div>
            </Card>

            <Card className="p-6">
              <h2 className="text-xl font-semibold text-gray-900 mb-4">Assign User to Property</h2>
              <div className="space-y-4">
                <Select
                  label="User"
                  value={assignmentForm.userId || ''}
                  onChange={(event) =>
                    setAssignmentForm({
                      ...assignmentForm,
                      userId: Number(event.target.value),
                    })
                  }
                  options={assignableUsers.map((user) => ({
                    value: user.id,
                    label: `${user.fullName} (${user.roleName})`,
                  }))}
                  placeholder={assignableUsers.length ? 'Select user' : 'No assignable users'}
                  fullWidth
                />
                <Select
                  label="Property"
                  value={assignmentPropertyId}
                  onChange={(event) => setAssignmentPropertyId(Number(event.target.value))}
                  options={filteredProperties.map((property) => ({
                    value: property.id,
                    label: property.name,
                  }))}
                  placeholder={filteredProperties.length ? 'Select property' : 'No properties in org'}
                  fullWidth
                />
                <Select
                  label="Assignment role"
                  value={assignmentForm.assignmentRole}
                  onChange={(event) =>
                    setAssignmentForm({
                      ...assignmentForm,
                      assignmentRole: event.target.value as UserRole,
                    })
                  }
                  options={assignmentRoleOptions}
                  fullWidth
                />
                <Button onClick={handleAssignUser} loading={isLoading} fullWidth>
                  Assign User
                </Button>
              </div>
            </Card>
          </div>
        </div>
      </div>
    </ProtectedRoute>
  )
}

