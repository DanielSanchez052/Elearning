import { z } from 'zod';

// Auth schemas
export const loginSchema = z.object({
  email: z.string().email('Email inválido'),
  password: z.string().min(6, 'Contraseña mínimo 6 caracteres'),
});

export const registerSchema = z.object({
  email: z.string().email('Email inválido'),
  fullName: z.string().min(2, 'Nombre mínimo 2 caracteres'),
  password: z.string().min(6, 'Contraseña mínimo 6 caracteres'),
  confirmPassword: z.string(),
}).refine((data) => data.password === data.confirmPassword, {
  message: 'Las contraseñas no coinciden',
  path: ['confirmPassword'],
});

// Course schemas
export const createCourseSchema = z.object({
  title: z.string().min(3, 'Título mínimo 3 caracteres'),
  description: z.string().min(10, 'Descripción mínimo 10 caracteres'),
  isGlobal: z.boolean(),
  countryIds: z.array(z.number()).optional(),
});

// Lesson schemas
export const createLessonSchema = z.object({
  courseId: z.string().uuid(),
  title: z.string().min(3, 'Título mínimo 3 caracteres'),
  type: z.enum(['VIDEO', 'PDF', 'TEXT']),
  content: z.string().min(1, 'Contenido requerido'),
});

export type LoginInput = z.infer<typeof loginSchema>;
export type RegisterInput = z.infer<typeof registerSchema>;
export type CreateCourseInput = z.infer<typeof createCourseSchema>;
export type CreateLessonInput = z.infer<typeof createLessonSchema>;
