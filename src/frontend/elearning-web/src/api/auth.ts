import { api } from '../lib/axios';
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
    api.post<{ value: string }>('/auth/register', data),

  login: (data: LoginRequest) =>
    api.post<LoginResponse>('/auth/login', data),

  verifyEmail: (data: VerifyEmailRequest) =>
    api.post('/auth/verify-email', data),

  forgotPassword: (data: ForgotPasswordRequest) =>
    api.post('/auth/forgot-password', data),

  resetPassword: (data: ResetPasswordRequest) =>
    api.post('/auth/reset-password', data),

  getMe: () =>
    api.get<CurrentUser>('/auth/me'),
};