import axios, { AxiosError } from 'axios';
import type { ApiError } from '../types';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'https://localhost:7001/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

export default api;

// ── Request interceptor — adjunta el JWT en cada request ─────────────────────
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('access_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// ── Response interceptor — manejo global de errores ──────────────────────────
api.interceptors.response.use(
  (response) => response,
  (error: AxiosError<ApiError>) => {
    // Token expirado o inválido — limpiar sesión y redirigir al login
    if (error.response?.status === 401) {
      localStorage.removeItem('access_token');
      localStorage.removeItem('user');
      // Redirigir sin depender de React Router para evitar imports circulares
      window.location.href = '/login';
    }

    // Propagar el error para que cada query/mutation lo maneje localmente
    return Promise.reject(error);
  }
);

// ── Helper para extraer el mensaje de error del backend ──────────────────────
export function getApiErrorMessage(error: unknown): string {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as ApiError | undefined;
    if (data?.error) return data.error;

    // Errores de red o timeout
    if (!error.response) return 'No se pudo conectar con el servidor.';
  }
  return 'Ocurrió un error inesperado.';
}