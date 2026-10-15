-- Cifras esperadas del resumen, calculadas a mano en SQL (solo estudiantes), por periodo y país (0 = todos).
\pset footer off
WITH periods(code, since) AS (VALUES ('7d', now() - interval '7 days'), ('30d', now() - interval '30 days'),
                                     ('90d', now() - interval '90 days'), ('all', NULL::timestamptz)),
     scopes(country) AS (SELECT 0 UNION ALL SELECT id FROM countries WHERE is_active),
     st AS (SELECT * FROM users WHERE role = 'Student'),
     en AS (SELECT e.*, u."CountryId" AS country FROM course_enrollments e JOIN st u ON u.id = e.user_id)
SELECT p.code AS periodo, s.country AS pais,
  (SELECT count(*) FROM st WHERE s.country IN (0, st."CountryId")) AS estudiantes,
  (SELECT count(*) FROM st WHERE s.country IN (0, st."CountryId") AND last_login_at IS NOT NULL
      AND (p.since IS NULL OR last_login_at >= p.since)) AS activos,
  (SELECT count(*) FROM en WHERE s.country IN (0, en.country) AND (p.since IS NULL OR enrolled_at >= p.since)) AS inscripciones,
  (SELECT round(100.0 * count(*) FILTER (WHERE status = 'Completed') / nullif(count(*), 0), 1)
      FROM en WHERE s.country IN (0, en.country) AND (p.since IS NULL OR enrolled_at >= p.since)) AS tasa,
  (SELECT count(*) FROM en WHERE s.country IN (0, en.country) AND status = 'Completed'
      AND (p.since IS NULL OR completed_at >= p.since)) AS completados,
  (SELECT round(avg(extract(epoch FROM completed_at - enrolled_at) / 86400.0)::numeric, 1) FROM en
      WHERE s.country IN (0, en.country) AND status = 'Completed' AND (p.since IS NULL OR completed_at >= p.since)) AS dias
FROM periods p CROSS JOIN scopes s WHERE s.country IN (0, 1, 2, 3) ORDER BY p.code, s.country;
