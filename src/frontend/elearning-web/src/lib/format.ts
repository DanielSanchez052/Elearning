const dateFormat = new Intl.DateTimeFormat('es', { day: 'numeric', month: 'short', year: 'numeric' });
const longDayFormat = new Intl.DateTimeFormat('es', { weekday: 'long', day: 'numeric', month: 'long' });

/** Fecha corta del rediseño: «15 dic 2026». */
export function formatDate(value: string | Date) {
  return dateFormat.format(typeof value === 'string' ? new Date(value) : value).replace('.', '');
}

/** Fecha larga con día de la semana, capitalizada: «Lunes, 12 de octubre». */
export function formatLongDay(value: Date = new Date()) {
  const text = longDayFormat.format(value);
  return text.charAt(0).toUpperCase() + text.slice(1);
}

const numberFormat = new Intl.NumberFormat('es');
const decimalFormat = new Intl.NumberFormat('es', { maximumFractionDigits: 1 });

/** Entero con separador de miles: «1.234». */
export function formatNumber(value: number) {
  return numberFormat.format(value);
}

/** Porcentaje con un decimal como máximo («33,3%», sin espacio como ProgressBar); «—» si no hay dato. */
export function formatPercent(value: number | null | undefined) {
  return value == null ? '—' : `${decimalFormat.format(value)}%`;
}

/** Días con un decimal como máximo («4,3 días»); «—» si no hay dato. */
export function formatDays(value: number | null | undefined) {
  return value == null ? '—' : `${decimalFormat.format(value)} ${value === 1 ? 'día' : 'días'}`;
}

/** Primera palabra del nombre, para saludos («Hola, Ana»). */
export function firstName(fullName: string | undefined) {
  return fullName?.trim().split(/\s+/)[0] ?? '';
}
