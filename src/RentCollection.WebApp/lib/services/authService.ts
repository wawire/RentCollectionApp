import axios from 'axios';
import type {
  LoginCredentials,
  RegisterData,
  AuthResponse,
  User,
  ChangePasswordData,
} from '../types/auth.types';

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'https://localhost:7000/api';

// Create axios instance with interceptors
const axiosInstance = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor to add auth token
axiosInstance.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor to handle auth errors
axiosInstance.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Clear auth data and redirect to login
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      if (typeof window !== 'undefined' && !window.location.pathname.includes('/login')) {
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  }
);

export const authService = {
  async login(credentials: LoginCredentials): Promise<AuthResponse> {
    const response = await axiosInstance.post<AuthResponse>('/auth/login', credentials);

    // Store token and user data
    localStorage.setItem('token', response.data.token);
    localStorage.setItem('user', JSON.stringify({
      email: response.data.email,
      firstName: response.data.firstName,
      lastName: response.data.lastName,
      role: response.data.role,
    }));

    return response.data;
  },

  async register(data: RegisterData): Promise<AuthResponse> {
    const response = await axiosInstance.post<AuthResponse>('/auth/register', data);

    // Store token and user data
    localStorage.setItem('token', response.data.token);
    localStorage.setItem('user', JSON.stringify({
      email: response.data.email,
      firstName: response.data.firstName,
      lastName: response.data.lastName,
      role: response.data.role,
    }));

    return response.data;
  },

  async getCurrentUser(): Promise<User> {
    const response = await axiosInstance.get<User>('/auth/me');
    return response.data;
  },

  async changePassword(data: ChangePasswordData): Promise<void> {
    await axiosInstance.post('/auth/change-password', data);
  },

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    if (typeof window !== 'undefined') {
      window.location.href = '/login';
    }
  },

  getToken(): string | null {
    if (typeof window === 'undefined') return null;
    return localStorage.getItem('token');
  },

  getStoredUser(): Partial<User> | null {
    if (typeof window === 'undefined') return null;
    const userStr = localStorage.getItem('user');
    if (!userStr) return null;
    try {
      return JSON.parse(userStr);
    } catch {
      return null;
    }
  },

  isAuthenticated(): boolean {
    return !!this.getToken();
  },

  // Admin-only endpoints
  async getAllUsers(): Promise<User[]> {
    const response = await axiosInstance.get<User[]>('/auth/users');
    return response.data;
  },

  async getUserById(id: string): Promise<User> {
    const response = await axiosInstance.get<User>(`/auth/users/${id}`);
    return response.data;
  },

  async updateUser(id: string, data: Partial<User>): Promise<void> {
    await axiosInstance.put(`/auth/users/${id}`, data);
  },

  async deleteUser(id: string): Promise<void> {
    await axiosInstance.delete(`/auth/users/${id}`);
  },
};
