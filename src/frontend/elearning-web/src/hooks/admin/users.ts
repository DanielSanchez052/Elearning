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

/**
 * Instructores que se pueden asignar a un curso. El backend ya limita el listado del admin a su
 * país; para el resto de roles no se pide (`/admin/users` es solo de admin y superadmin).
 */
export function useAssignableInstructors(enabled: boolean) {
  const params: GetUsersParams = { role: 'instructor', pageSize: 100 };
  return useQuery({
    queryKey: usersKeys.byParams(params),
    queryFn: () => usersApi.getUsers(params).then((r) => r.data.items),
    staleTime: 1000 * 60 * 2,
    enabled,
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
