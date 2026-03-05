
export interface Country {
  id: number;
  name: string;
  code: string;
  isActive: boolean;
}

export interface UserSummaryDto {
  id: string;
  fullName: string;
  email: string;
  role: UserRole;
  country: string;
  countryId: number;
  isEmailVerified: boolean;
  createdAt: string;
}

export interface LoggedUser {
  id: string;
  fullName: string;
  email: string;
  role: UserRole;
  country: string;
}

export interface LoginResponse {
  accessToken: string;
  expiresAt: string; // ISO date string
  user: LoggedUser;
}

export interface CurrentUser {
  id: string;
  fullName: string;
  email: string;
  role: UserRole;
  country: string;
  countryId: number;
  createdAt: string;
  loginStreak: number;
}

export type UserRole = 'student' | 'instructor' | 'admin' | 'superadmin';

export const ROLES: Record<UserRole, string> = {
  student: 'Estudiante',
  instructor: 'Instructor',
  admin: 'Administrador',
  superadmin: 'Super Administrador',
};

