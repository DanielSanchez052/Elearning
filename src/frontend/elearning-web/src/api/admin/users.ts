import axios from '@/lib/axios';
import type { PagedResult, UserSummaryDto } from '@/types';

// ── Params ────────────────────────────────────────────────────────────────────

export interface GetUsersParams {
  countryId?: number;
  role?: string;
  search?: string;
  isEmailVerified?: boolean;
  page?: number;
  pageSize?: number;
}

// ── API ───────────────────────────────────────────────────────────────────────

export const usersApi = {
  getUsers: (params: GetUsersParams = {}) =>
    axios.get<PagedResult<UserSummaryDto>>('/admin/users', { params }),

  getUserById: (id: string) =>
    axios.get<UserSummaryDto>(`/admin/users/${id}`),

  changeUserRole: (id: string, role: string) =>
    axios.patch(`/admin/users/${id}/role`, { role }),

  changeUserCountry: (id: string, countryId: number) =>
    axios.patch(`/admin/users/${id}/country`, { countryId }),
};
