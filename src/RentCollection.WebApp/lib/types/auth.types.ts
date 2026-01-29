export enum UserRole {
  PlatformAdmin = 'PlatformAdmin',
  Landlord = 'Landlord',
  Caretaker = 'Caretaker',
  Manager = 'Manager',
  Accountant = 'Accountant',
  Tenant = 'Tenant',
}

export enum UserStatus {
  Invited = 'Invited',
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
  organizationId?: number
  propertyId?: number
  tenantId?: number
}

export interface AuthResponse {
  userId: number
  email: string
  fullName: string
  phoneNumber: string
  role: UserRole
  status: UserStatus
  isVerified: boolean
  verifiedAt?: string
  verificationChannel?: 'Email' | 'Phone'
  mustChangePassword: boolean
  token: string
  expiresAt: string
  propertyId?: number
  tenantId?: number
  organizationId: number
  organizationStatus?: 'Pending' | 'Active' | 'Suspended'
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
  isVerified: boolean
  verifiedAt?: string
  verificationChannel?: 'Email' | 'Phone'
  mustChangePassword: boolean
  propertyId?: number
  propertyName?: string
  tenantId?: number
  organizationId: number
  createdAt: string
  lastLoginAt?: string
}

export interface ChangePasswordRequest {
  currentPassword: string
  newPassword: string
  confirmPassword: string
}

