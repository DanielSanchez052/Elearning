import { useQuery, useMutation, useQueryClient, keepPreviousData } from '@tanstack/react-query';
import { usersApi, type GetUsersParams } from '@/api/admin/users';

// ── Query keys ────────────────────────────────────────────────────────────────

export const usersKeys = {
  all: ['admin', 'users'] as const,
  byParams: (params: GetUsersParams) => ['admin', 'users', params] as const,
};

// ── Queries ───────────────────────────────────────────────────────────────────

export function useAdminUsers(params: GetUsersParams = {}) {
  return useQuery({
    queryKey: usersKeys.byParams(params),
    queryFn: () => usersApi.getUsers(params).then((r) => r.data),
    placeholderData: keepPreviousData,
    staleTime: 1000 * 60 * 2,
  });
}

// ── Mutations ─────────────────────────────────────────────────────────────────

export function useChangeUserRole() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, role }: { id: string; role: string }) =>
      usersApi.changeUserRole(id, role),
    onSuccess: () => qc.invalidateQueries({ queryKey: usersKeys.all }),
  });
}

export function useChangeUserCountry() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, countryId }: { id: string; countryId: number }) =>
      usersApi.changeUserCountry(id, countryId),
    onSuccess: () => qc.invalidateQueries({ queryKey: usersKeys.all }),
  });
}
