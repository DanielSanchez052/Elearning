-- Borra los datos de demostración de los reportes (estudiantes @demo-reportes.test y su actividad).
-- Los cursos "[Demo] ..." se eliminan aparte desde el panel (borrado lógico) o a mano.
BEGIN;
DELETE FROM user_lesson_progress WHERE enrollment_id IN (
  SELECT e.id FROM course_enrollments e JOIN users u ON u.id = e.user_id WHERE u.email LIKE '%@demo-reportes.test');
DELETE FROM course_enrollments WHERE user_id IN (SELECT id FROM users WHERE email LIKE '%@demo-reportes.test');
DELETE FROM notifications WHERE user_id IN (SELECT id FROM users WHERE email LIKE '%@demo-reportes.test');
DELETE FROM user_badges WHERE user_id IN (SELECT id FROM users WHERE email LIKE '%@demo-reportes.test');
DELETE FROM users WHERE email LIKE '%@demo-reportes.test';
COMMIT;
