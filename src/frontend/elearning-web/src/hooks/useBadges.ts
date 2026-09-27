import { useQuery } from '@tanstack/react-query';
import { badgesApi } from '@/api/badges';

export const badgeKeys = {
  all: ['badges'] as const,
  mine: () => ['badges', 'me'] as const,
};

export function useMyBadges(enabled = true) {
  return useQuery({
    queryKey: badgeKeys.mine(),
    queryFn: () => badgesApi.getMyBadges().then((r) => r.data),
    enabled,
    staleTime: 1000 * 60,
  });
}
