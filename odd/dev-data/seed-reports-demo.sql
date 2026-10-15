-- Datos de demostración para los reportes del MVP 4. Identificables por el correo
-- (@demo-reportes.test) y por los cursos "[Demo] ...". Idempotente: no duplica si se repite.
BEGIN;
SELECT setseed(0.42);

-- 14 estudiantes en Colombia (1), México (2) y Argentina (3), con accesos repartidos en el tiempo.
INSERT INTO users (id, "CountryId", full_name, email, password_hash, role, is_email_verified,
                   last_login_at, login_streak, created_at, updated_at)
SELECT gen_random_uuid(), s.country, s.name, 'demo' || s.n || '@demo-reportes.test', 'demo-sin-acceso',
       'Student', true,
       CASE WHEN s.login_days IS NULL THEN NULL ELSE now() - make_interval(days => s.login_days) END,
       0, now() - make_interval(days => 170 + s.n), now()
FROM (VALUES
  (1, 'Laura Gómez', 1, 1), (2, 'Andrés Pérez', 1, 3), (3, 'Camila Rojas', 1, 6),
  (4, 'Julián Torres', 1, 15), (5, 'Valentina Ruiz', 1, 28), (6, 'Santiago Díaz', 1, 75),
  (7, 'Ximena López', 2, 2), (8, 'Diego Hernández', 2, 9), (9, 'Fernanda Castillo', 2, 22),
  (10, 'Emilio Vargas', 2, 50), (11, 'Renata Morales', 2, 140),
  (12, 'Martina Fernández', 3, 4), (13, 'Tomás Romero', 3, 35), (14, 'Lucía Benítez', 3, NULL)
) AS s(n, name, country, login_days)
WHERE NOT EXISTS (SELECT 1 FROM users u WHERE u.email = 'demo' || s.n || '@demo-reportes.test');

-- Inscripciones en los cursos publicados disponibles en su país, con fechas de los últimos 5 meses.
-- Alrededor del 55 % ya completadas, entre 1 y 20 días después de inscribirse.
INSERT INTO course_enrollments (id, user_id, course_id, status, enrolled_at, completed_at)
SELECT gen_random_uuid(), u.id, c.id,
       CASE WHEN x.done THEN 'Completed' ELSE 'Active' END,
       x.enrolled_at,
       CASE WHEN x.done THEN LEAST(x.enrolled_at + make_interval(days => x.duration), now()) END
FROM users u
JOIN courses c ON c.is_active
  AND (c.is_global OR EXISTS (SELECT 1 FROM course_countries cc
                               WHERE cc.course_id = c.id AND cc.country_id = u."CountryId"))
CROSS JOIN LATERAL (
  SELECT random() < 0.8 AS take,
         random() < 0.55 AS done,
         now() - make_interval(days => (2 + floor(random() * 145))::int, hours => (floor(random() * 20))::int) AS enrolled_at,
         (1 + floor(random() * 20))::int AS duration
  WHERE u.id IS NOT NULL AND c.id IS NOT NULL
) x
WHERE u.email LIKE '%@demo-reportes.test'
  AND x.take
  AND NOT EXISTS (SELECT 1 FROM course_enrollments e WHERE e.user_id = u.id AND e.course_id = c.id);

-- Progreso de lecciones coherente: todas en las completadas; la primera en la mitad de las activas.
INSERT INTO user_lesson_progress (id, enrollment_id, lesson_id, is_completed, completed_at, last_accessed_at)
SELECT gen_random_uuid(), e.id, l.id, true, e.completed_at, e.completed_at
FROM course_enrollments e
JOIN users u ON u.id = e.user_id
JOIN lessons l ON l."CourseId" = e.course_id
WHERE u.email LIKE '%@demo-reportes.test' AND e.status = 'Completed'
  AND NOT EXISTS (SELECT 1 FROM user_lesson_progress p WHERE p.enrollment_id = e.id AND p.lesson_id = l.id);

INSERT INTO user_lesson_progress (id, enrollment_id, lesson_id, is_completed, completed_at, last_accessed_at)
SELECT gen_random_uuid(), e.id, l.id, true, e.enrolled_at + interval '1 day', e.enrolled_at + interval '1 day'
FROM course_enrollments e
JOIN users u ON u.id = e.user_id
JOIN LATERAL (SELECT id FROM lessons WHERE "CourseId" = e.course_id ORDER BY order_index LIMIT 1) l ON true
WHERE u.email LIKE '%@demo-reportes.test' AND e.status = 'Active'
  AND (abs(hashtext(e.id::text)) % 2) = 0
  AND NOT EXISTS (SELECT 1 FROM user_lesson_progress p WHERE p.enrollment_id = e.id);

COMMIT;

SELECT u."CountryId" AS pais, count(DISTINCT u.id) AS estudiantes, count(e.id) AS inscripciones,
       count(e.id) FILTER (WHERE e.status = 'Completed') AS completadas
FROM users u LEFT JOIN course_enrollments e ON e.user_id = u.id
WHERE u.email LIKE '%@demo-reportes.test' GROUP BY u."CountryId" ORDER BY 1;
