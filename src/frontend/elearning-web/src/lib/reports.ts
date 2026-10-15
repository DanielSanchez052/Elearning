import type { ReportPeriod } from '@/types/report.types';

/** Opciones del selector de periodo de los reportes. */
export const reportPeriodOptions: { value: ReportPeriod; label: string }[] = [
  { value: '7d', label: '7 días' },
  { value: '30d', label: '30 días' },
  { value: '90d', label: '90 días' },
  { value: 'all', label: 'Todo' },
];
