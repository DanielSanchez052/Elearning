import type { EnrollmentStatusValue } from '@/types/enrollment.types';

export type EnrollmentStatusKey = 'active' | 'completed' | 'abandoned';

/** El estado puede llegar como código numérico o como texto según el serializador del backend. */
export function enrollmentStatusKey(status: EnrollmentStatusValue): EnrollmentStatusKey {
  if (status === 1 || status === 'Completed') return 'completed';
  if (status === 2 || status === 'Abandoned') return 'abandoned';
  return 'active';
}

/** Etiqueta y tono de Badge según los tonos fijos del rediseño (activo = info, completado = success). */
export const enrollmentStatusMeta: Record<
  EnrollmentStatusKey,
  { label: string; tone: 'info' | 'success' | 'neutral'; cta: string }
> = {
  active: { label: 'Activo', tone: 'info', cta: 'Continuar' },
  completed: { label: 'Completado', tone: 'success', cta: 'Ver curso' },
  abandoned: { label: 'Abandonado', tone: 'neutral', cta: 'Retomar' },
};

/** Texto de progreso único del rediseño: «X / Y lecciones requeridas». */
export function requiredLessonsLabel(completed: number, required: number) {
  return `${completed} / ${required} lecciones requeridas`;
}
