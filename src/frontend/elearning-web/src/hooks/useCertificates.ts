import { useQuery } from '@tanstack/react-query';
import { certificatesApi } from '@/api/certificates';

export const certificateKeys = {
  all: ['certificates'] as const,
  mine: () => ['certificates', 'me'] as const,
};

export function useMyCertificates(enabled = true) {
  return useQuery({
    queryKey: certificateKeys.mine(),
    queryFn: () => certificatesApi.getMyCertificates().then((r) => r.data),
    enabled,
    staleTime: 1000 * 60,
  });
}
