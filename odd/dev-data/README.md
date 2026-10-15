# Datos de desarrollo

Scripts SQL para poblar la base de **desarrollo** con datos de prueba. No usar en producción.

## Reportes (MVP 4)
- `seed-reports-demo.sql`: 14 estudiantes `demoN@demo-reportes.test` (Colombia, México y Argentina) con accesos repartidos en el tiempo, inscripciones de los últimos 5 meses (parte completadas) y su progreso de lecciones. Usa los cursos publicados que existan; para tener variedad conviene crear antes algún curso `[Demo] ...`. Es idempotente: se puede repetir sin duplicar.
- `cleanup-reports-demo.sql`: borra los estudiantes de demo y toda su actividad (los cursos `[Demo]` se eliminan aparte desde el panel).
- `expected-overview-report.sql`: calcula a mano en SQL las métricas del resumen (por periodo y país) para compararlas con `GET /api/admin/reports/overview`.

Ejecución (con la cadena de conexión de user-secrets):

```bash
psql -h localhost -U postgres -d elearning -v ON_ERROR_STOP=1 -f odd/dev-data/seed-reports-demo.sql
```
