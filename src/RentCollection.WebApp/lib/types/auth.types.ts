export enum UserRole {
  SystemAdmin = 'SystemAdmin',
  Landlord = 'Landlord',
  Caretaker = 'Caretaker',
  Accountant = 'Accountant',
  Tenant = 'Tenant',
}

export enum UserStatus {
  Active = 'Active',
  Suspended = 'Suspended',
  Inactive = 'Inactive',
}

export interface LoginRequest {
  emailOrPhone: string
  password: string
}

export interface RegisterRequest {
  firstName: string
  lastName: string
  email: string
  phoneNumber: string
  password: string
  confirmPassword: string
  role: UserRole
  propertyId?: number
  tenantId?: number
}

export interface AuthResponse {
  userId: number
  email: string
  fullName: string
  phoneNumber: string
  role: UserRole
  token: string
  expiresAt: string
  propertyId?: number
  tenantId?: number
}

export interface User {
  id: number
  firstName: string
  lastName: string
  fullName: string
  email: string
  phoneNumber: string
  role: UserRole
  roleName: string
  status: UserStatus
  statusName: string
  propertyId?: number
  propertyName?: string
  tenantId?: number
  createdAt: string
  lastLoginAt?: string
}

export interface ChangePasswordRequest {
  currentPassword: string
  newPassword: string
  confirmPassword: string
}
