/** Periodo de los reportes, igual que en el backend (`?period=`). */
export type ReportPeriod = '7d' | '30d' | '90d' | 'all';

export interface ReportKpis {
  students: number;
  activeStudents: number;
  enrollments: number;
  completions: number;
  /** De las inscripciones del periodo, % que ya están completadas; null sin inscripciones. */
  completionRate: number | null;
  /** Días promedio entre inscripción y finalización de los cursos completados en el periodo. */
  avgDaysToComplete: number | null;
}

export interface PopularCourse {
  courseId: string;
  title: string;
  enrollments: number;
  enrollmentsCompleted: number;
  completionRate: number | null;
}

export interface CountryReportRow {
  countryId: number;
  countryName: string;
  kpis: ReportKpis;
}

export interface OverviewReport {
  period: ReportPeriod;
  since: string | null;
  countryId: number | null;
  countryName: string | null;
  kpis: ReportKpis;
  popularCourses: PopularCourse[];
  /** Comparativa por país: solo para el superadmin sin filtro de país. */
  countries: CountryReportRow[] | null;
}
