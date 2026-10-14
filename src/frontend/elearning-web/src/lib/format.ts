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

/** Primera palabra del nombre, para saludos («Hola, Ana»). */
export function firstName(fullName: string | undefined) {
  return fullName?.trim().split(/\s+/)[0] ?? '';
}
