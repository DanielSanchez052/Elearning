import { useMutation, useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { z } from 'zod';
import { authApi } from '@/api/auth';
import { countriesApi } from '@/api/countries';
import { useAuthStore } from '@/features/auth/authStore';
import { getApiErrorMessage } from '@/lib/axios';

// ── Zod schemas — espejean las reglas del backend ─────────────────────────────

export const registerSchema = z.object({
  fullName: z
    .string()
    .min(2, 'El nombre debe tener al menos 2 caracteres.')
    .max(150, 'El nombre no puede superar 150 caracteres.'),
  email: z
    .string()
    .email('El email no tiene un formato válido.'),
  password: z
    .string()
    .min(8, 'Debe tener al menos 8 caracteres.')
    .regex(/[A-Z]/, 'Debe contener al menos una letra mayúscula.')
    .regex(/[a-z]/, 'Debe contener al menos una letra minúscula.')
    .regex(/[0-9]/, 'Debe contener al menos un número.'),
  confirmPassword: z.string(),
  countryId: z
    .number({ error: 'El país es requerido.' })
    .min(1, 'El país es requerido.'),
}).refine((d) => d.password === d.confirmPassword, {
  message: 'Las contraseñas no coinciden.',
  path: ['confirmPassword'],
});

export const loginSchema = z.object({
  email: z.string().min(1, 'El email es requerido.'),
  password: z.string().min(1, 'La contraseña es requerida.'),
});

export const forgotPasswordSchema = z.object({
  email: z.string().email('El email no tiene un formato válido.'),
});

export const resetPasswordSchema = z.object({
  newPassword: z
    .string()
    .min(8, 'Debe tener al menos 8 caracteres.')
    .regex(/[A-Z]/, 'Debe contener al menos una letra mayúscula.')
    .regex(/[a-z]/, 'Debe contener al menos una letra minúscula.')
    .regex(/[0-9]/, 'Debe contener al menos un número.'),
  confirmPassword: z.string(),
}).refine((d) => d.newPassword === d.confirmPassword, {
  message: 'Las contraseñas no coinciden.',
  path: ['confirmPassword'],
});

export type RegisterFormData = z.infer<typeof registerSchema>;
export type LoginFormData = z.infer<typeof loginSchema>;
export type ForgotPasswordFormData = z.infer<typeof forgotPasswordSchema>;
export type ResetPasswordFormData = z.infer<typeof resetPasswordSchema>;

// ── Hooks ─────────────────────────────────────────────────────────────────────

export function useActiveCountries() {
  return useQuery({
    queryKey: ['countries', 'active'],
    queryFn: () => countriesApi.getActive().then((r) => r.data),
    staleTime: Infinity, // los países no cambian frecuentemente
  });
}

export function useRegister() {
  const navigate = useNavigate();

  return useMutation({
    mutationFn: (data: RegisterFormData) =>
      authApi.register({
        fullName: data.fullName,
        email: data.email,
        password: data.password,
        countryId: data.countryId,
      }),
    onSuccess: () => {
      navigate('/login?registered=true');
    },
  });
}

export function useLogin() {
  const navigate = useNavigate();
  const setAuth = useAuthStore((s) => s.setAuth);

  return useMutation({
    mutationFn: (data: LoginFormData) =>
      authApi.login(data).then((r) => r.data),
    onSuccess: async (loginResponse) => {
      // Guardar token primero para que getMe pueda usarlo
      localStorage.setItem('access_token', loginResponse.accessToken);

      // Obtener el perfil completo del usuario
      const me = await authApi.getMe().then((r) => r.data);
      setAuth(me, loginResponse.accessToken);

      navigate('/dashboard');
    },
  });
}

export function useForgotPassword() {
  return useMutation({
    mutationFn: (data: ForgotPasswordFormData) =>
      authApi.forgotPassword(data),
  });
}

export function useResetPassword(token: string) {
  const navigate = useNavigate();

  return useMutation({
    mutationFn: (data: ResetPasswordFormData) =>
      authApi.resetPassword({
        token,
        newPassword: data.newPassword,
        confirmPassword: data.confirmPassword,
      }),
    onSuccess: () => {
      navigate('/login?reset=true');
    },
  });
}

export function useVerifyEmail(token: string) {
  return useQuery({
    queryKey: ['verify-email', token],
    queryFn: () => authApi.verifyEmail({ token }),
    enabled: !!token,
    retry: false, // no reintentar — si falla, falla
  });
}

// Helper para mostrar errores de la API en los formularios
export { getApiErrorMessage };