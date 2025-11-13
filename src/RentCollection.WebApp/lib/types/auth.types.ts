export interface User {
  id: number
  username: string
  email: string
  firstName: string
  lastName: string
  phoneNumber: string
  role: 'Admin' | 'PropertyManager' | 'Viewer'
  isActive: boolean
  lastLoginAt?: string
  createdAt: string
}

export interface LoginDto {
  username: string
  password: string
}

export interface RegisterDto {
  username: string
  email: string
  password: string
  firstName: string
  lastName: string
  phoneNumber: string
  role?: 'Admin' | 'PropertyManager' | 'Viewer'
}

export interface AuthResponse {
  id: number
  username: string
  email: string
  firstName: string
  lastName: string
  role: string
  accessToken: string
  refreshToken: string
  expiresAt: string
}

export interface RefreshTokenDto {
  refreshToken: string
}

export interface AuthContextType {
  user: User | null
  isAuthenticated: boolean
  isLoading: boolean
  login: (credentials: LoginDto) => Promise<boolean>
  register: (data: RegisterDto) => Promise<boolean>
  logout: () => Promise<void>
  refreshToken: () => Promise<boolean>
}
