import { Link } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import {
  useForgotPassword,
  forgotPasswordSchema,
  type ForgotPasswordFormData,
} from '@/hooks/useAuth';

export default function ForgotPasswordForm() {
  const forgot = useForgotPassword();

  const { register, handleSubmit, formState: { errors } } = useForm<ForgotPasswordFormData>({
    resolver: zodResolver(forgotPasswordSchema),
  });

  const onSubmit = (data: ForgotPasswordFormData) => forgot.mutate(data);

  if (forgot.isSuccess) {
    return (
      <div className="text-center space-y-4">
        <div className="inline-flex items-center justify-center w-14 h-14 rounded-2xl bg-emerald-500/10 border border-emerald-500/20 mb-2">
          <svg className="w-7 h-7 text-emerald-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
          </svg>
        </div>
        <p className="text-zinc-300 text-sm leading-relaxed">
          Si el email está registrado, recibirás un correo con las instrucciones en los próximos minutos.
        </p>
        <p className="text-zinc-600 text-xs">Revisa también tu carpeta de spam.</p>
        <Link
          to="/login"
          className="inline-block mt-2 text-sm text-indigo-400 hover:text-indigo-300 transition-colors"
        >
          ← Volver al login
        </Link>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
      <div>
        <label className="block text-sm font-medium text-zinc-300 mb-1.5">Email</label>
        <input
          {...register('email')}
          type="email"
          placeholder="tu@email.com"
          className="w-full px-4 py-2.5 rounded-xl bg-white/[0.04] border border-white/[0.08] text-white placeholder-zinc-600 text-sm focus:outline-none focus:border-indigo-500/60 transition-all"
        />
        {errors.email && (
          <p className="mt-1.5 text-xs text-red-400">{errors.email.message}</p>
        )}
      </div>

      <button
        type="submit"
        disabled={forgot.isPending}
        className="w-full py-2.5 rounded-xl bg-indigo-600 hover:bg-indigo-500 disabled:opacity-50 text-white text-sm font-medium transition-colors"
      >
        {forgot.isPending ? 'Enviando...' : 'Enviar enlace de recuperación'}
      </button>

      <div className="text-center">
        <Link to="/login" className="text-sm text-zinc-500 hover:text-zinc-400 transition-colors">
          ← Volver al login
        </Link>
      </div>
    </form>
  );
}
