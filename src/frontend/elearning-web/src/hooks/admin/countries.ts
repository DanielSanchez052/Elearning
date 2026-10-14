import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { countriesApi } from '@/api/admin/countries';

// ── Query keys ────────────────────────────────────────────────────────────────

export const countriesKeys = {
  all: ['admin', 'countries'] as const,
  byId: (id: number) => ['admin', 'countries', id] as const,
};

// ── Queries ───────────────────────────────────────────────────────────────────

// `/admin/countries` es solo para admin y superadmin: pasa `enabled: false` para el resto de roles.
export function useAdminCountries({ enabled = true }: { enabled?: boolean } = {}) {
  return useQuery({
    queryKey: countriesKeys.all,
    queryFn: () => countriesApi.getCountries().then((r) => r.data),
    staleTime: 1000 * 60 * 5,
    enabled,
  });
}

// ── Mutations ─────────────────────────────────────────────────────────────────

export function useCreateCountry() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: { code: string; name: string }) =>
      countriesApi.createCountry(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: countriesKeys.all }),
  });
}

export function useToggleCountryStatus() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => countriesApi.toggleCountryStatus(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: countriesKeys.all }),
  });
}
