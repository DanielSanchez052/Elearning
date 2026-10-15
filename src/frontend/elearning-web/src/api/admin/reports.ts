import axios from '@/lib/axios';
import type { OverviewReport, ReportPeriod } from '@/types/report.types';

export interface OverviewReportParams {
  period: ReportPeriod;
  /** Solo lo usa el superadmin; al admin el backend siempre lo limita a su país. */
  countryId?: number;
}

export const reportsApi = {
  getOverview: (params: OverviewReportParams) =>
    axios.get<OverviewReport>('/admin/reports/overview', { params }),
};
