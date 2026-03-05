import axios from '@/lib/axios';
import type { LoginResponse, CurrentUser } from '../types/user.types';

// ── Request types ─────────────────────────────────────────────────────────────

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  countryId: number;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  token: string;
  newPassword: string;
  confirmPassword: string;
}

export interface VerifyEmailRequest {
  token: string;
}

// ── API functions ─────────────────────────────────────────────────────────────

export const authApi = {
  register: (data: RegisterRequest) =>
    axios.post<{ value: string }>('/auth/register', data),

  login: (data: LoginRequest) =>
    axios.post<LoginResponse>('/auth/login', data),

  verifyEmail: (data: VerifyEmailRequest) =>
    axios.post('/auth/verify-email', data),

  forgotPassword: (data: ForgotPasswordRequest) =>
    axios.post('/auth/forgot-password', data),

  resetPassword: (data: ResetPasswordRequest) =>
    axios.post('/auth/reset-password', data),

  getMe: () =>
    axios.get<CurrentUser>('/auth/me'),
};