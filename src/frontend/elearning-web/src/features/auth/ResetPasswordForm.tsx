import { Link, useSearchParams } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import {
  useResetPassword,
  resetPasswordSchema,
  type ResetPasswordFormData,
  getApiErrorMessage,
} from '@/hooks/useAuth';

export default function ResetPasswordForm() {
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token') ?? '';

  const reset = useResetPassword(token);

  const { register, handleSubmit, formState: { errors } } = useForm<ResetPasswordFormData>({
    resolver: zodResolver(resetPasswordSchema),
  });

  const onSubmit = (data: ResetPasswordFormData) => reset.mutate(data);

  if (!token) {
    return (
      <div className="text-center space-y-4">
        <p className="text-zinc-400 text-sm">
          El enlace de recuperación no es válido o ha expirado.
        </p>
        <Link
          to="/forgot-password"
          className="inline-block text-sm text-indigo-400 hover:text-indigo-300 transition-colors"
        >
          Solicitar un nuevo enlace
        </Link>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
      <div>
        <label className="block text-sm font-medium text-zinc-300 mb-1.5">
          Nueva contraseña
        </label>
        <input
          {...register('newPassword')}
          type="password"
          placeholder="••••••••"
          className="w-full px-4 py-2.5 rounded-xl bg-white/[0.04] border border-white/[0.08] text-white placeholder-zinc-600 text-sm focus:outline-none focus:border-indigo-500/60 transition-all"
        />
        {errors.newPassword && (
          <p className="mt-1.5 text-xs text-red-400">{errors.newPassword.message}</p>
        )}
      </div>

      <div>
        <label className="block text-sm font-medium text-zinc-300 mb-1.5">
          Confirmar contraseña
        </label>
        <input
          {...register('confirmPassword')}
          type="password"
          placeholder="••••••••"
          className="w-full px-4 py-2.5 rounded-xl bg-white/[0.04] border border-white/[0.08] text-white placeholder-zinc-600 text-sm focus:outline-none focus:border-indigo-500/60 transition-all"
        />
        {errors.confirmPassword && (
          <p className="mt-1.5 text-xs text-red-400">{errors.confirmPassword.message}</p>
        )}
      </div>

      {reset.isError && (
        <div className="px-4 py-3 rounded-xl bg-red-500/10 border border-red-500/20 text-red-400 text-sm">
          {getApiErrorMessage(reset.error)}
        </div>
      )}

      <button
        type="submit"
        disabled={reset.isPending}
        className="w-full py-2.5 rounded-xl bg-indigo-600 hover:bg-indigo-500 disabled:opacity-50 text-white text-sm font-medium transition-colors"
      >
        {reset.isPending ? 'Guardando...' : 'Guardar nueva contraseña'}
      </button>
    </form>
  );
}
