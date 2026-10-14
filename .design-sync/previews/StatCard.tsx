import { StatCard, BarChartIcon, CheckCircleIcon, UsersIcon } from '@elearning/ui';

export const QuizResult = () => (
  <div className="grid w-full gap-4 md:grid-cols-3">
    <StatCard label="Puntaje" value="90%" tone="success" hint="Mínimo para aprobar: 70%" />
    <StatCard label="Correctas" value="9 / 10" />
    <StatCard label="Intento" value="1 de 3" hint="Te quedan 2 intentos" />
  </div>
);

export const Failed = () => (
  <div className="w-64">
    <StatCard label="Puntaje" value="60%" tone="danger" hint="Mínimo para aprobar: 70%" />
  </div>
);

export const ComingSoon = () => (
  <div className="grid w-full gap-4 md:grid-cols-3">
    <StatCard label="Alumnos activos" icon={UsersIcon} hint="Estudiantes con actividad en los últimos 30 días." />
    <StatCard label="Cursos completados" icon={CheckCircleIcon} hint="Inscripciones completadas por periodo." />
    <StatCard label="Ranking por país" icon={BarChartIcon} hint="Avance promedio por país." />
  </div>
);
