import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { reportsApi, type OverviewReportParams } from '@/api/admin/reports';

export const reportsKeys = {
  all: ['admin', 'reports'] as const,
  overview: (params: OverviewReportParams) => ['admin', 'reports', 'overview', params] as const,
};

export function useOverviewReport(params: OverviewReportParams, { enabled = true }: { enabled?: boolean } = {}) {
  return useQuery({
    queryKey: reportsKeys.overview(params),
    queryFn: () => reportsApi.getOverview(params).then((r) => r.data),
    placeholderData: keepPreviousData,
    staleTime: 1000 * 60,
    enabled,
  });
}
