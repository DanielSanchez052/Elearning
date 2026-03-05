import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import {
  useRegister,
  useActiveCountries,
  registerSchema,
  type RegisterFormData,
  getApiErrorMessage,
} from '@/features/auth/useAuth';

export default function RegisterPage() {
  const [showPassword, setShowPassword] = useState(false);
  const register_ = useRegister();
  const { data: countries, isLoading: loadingCountries } = useActiveCountries();

  const { register, handleSubmit, formState: { errors }, watch } = useForm<RegisterFormData>({
    resolver: zodResolver(registerSchema),
  });

  const password = watch('password', '');

  // Indicador visual de fortaleza de contraseña
  const strength = [
    password.length >= 8,
    /[A-Z]/.test(password),
    /[a-z]/.test(password),
    /[0-9]/.test(password),
  ];
  const strengthCount = strength.filter(Boolean).length;
  const strengthColor = strengthCount <= 1
    ? 'bg-red-500'
    : strengthCount <= 2
      ? 'bg-amber-500'
      : strengthCount <= 3
        ? 'bg-yellow-400'
        : 'bg-emerald-500';

  const onSubmit = (data: RegisterFormData) => register_.mutate(data);

  return (
    <div className="min-h-screen bg-[#0a0a0f] flex items-center justify-center p-4">

      {/* Glow de fondo */}
      <div className="absolute inset-0 overflow-hidden pointer-events-none">
        <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[600px] h-[600px] rounded-full bg-indigo-600/10 blur-[120px]" />
      </div>

      <div className="relative w-full max-w-md">

        {/* Logo / Branding */}
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-12 h-12 rounded-2xl bg-indigo-500/20 border border-indigo-500/30 mb-4">
            <svg className="w-6 h-6 text-indigo-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
            </svg>
          </div>
          <h1 className="text-2xl font-semibold text-white tracking-tight">Crea tu cuenta</h1>
          <p className="text-sm text-zinc-500 mt-1">Accede a cursos de tu región</p>
        </div>

        {/* Card */}
        <div className="bg-[#111118] border border-white/[0.06] rounded-2xl p-8 shadow-2xl">
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">

            {/* Nombre completo */}
            <div>
              <label className="block text-sm font-medium text-zinc-300 mb-1.5">
                Nombre completo
              </label>
              <input
                {...register('fullName')}
                type="text"
                autoComplete="name"
                placeholder="Juan Pérez"
                className="w-full px-4 py-2.5 rounded-xl bg-white/[0.04] border border-white/[0.08] text-white placeholder-zinc-600 text-sm focus:outline-none focus:border-indigo-500/60 focus:bg-white/[0.06] transition-all"
              />
              {errors.fullName && (
                <p className="mt-1.5 text-xs text-red-400">{errors.fullName.message}</p>
              )}
            </div>

            {/* Email */}
            <div>
              <label className="block text-sm font-medium text-zinc-300 mb-1.5">
                Email
              </label>
              <input
                {...register('email')}
                type="email"
                autoComplete="email"
                placeholder="tu@email.com"
                className="w-full px-4 py-2.5 rounded-xl bg-white/[0.04] border border-white/[0.08] text-white placeholder-zinc-600 text-sm focus:outline-none focus:border-indigo-500/60 focus:bg-white/[0.06] transition-all"
              />
              {errors.email && (
                <p className="mt-1.5 text-xs text-red-400">{errors.email.message}</p>
              )}
            </div>

            {/* País */}
            <div>
              <label className="block text-sm font-medium text-zinc-300 mb-1.5">
                País
              </label>
              <select
                {...register('countryId', { valueAsNumber: true })}
                disabled={loadingCountries}
                className="w-full px-4 py-2.5 rounded-xl bg-white/[0.04] border border-white/[0.08] text-white text-sm focus:outline-none focus:border-indigo-500/60 transition-all disabled:opacity-50 appearance-none"
              >
                <option value={0} className="bg-[#111118]">
                  {loadingCountries ? 'Cargando países...' : 'Selecciona tu país'}
                </option>
                {countries?.map((c) => (
                  <option key={c.id} value={c.id} className="bg-[#111118]">
                    {c.name}
                  </option>
                ))}
              </select>
              {errors.countryId && (
                <p className="mt-1.5 text-xs text-red-400">{errors.countryId.message}</p>
              )}
            </div>

            {/* Contraseña */}
            <div>
              <label className="block text-sm font-medium text-zinc-300 mb-1.5">
                Contraseña
              </label>
              <div className="relative">
                <input
                  {...register('password')}
                  type={showPassword ? 'text' : 'password'}
                  autoComplete="new-password"
                  placeholder="••••••••"
                  className="w-full px-4 py-2.5 pr-10 rounded-xl bg-white/[0.04] border border-white/[0.08] text-white placeholder-zinc-600 text-sm focus:outline-none focus:border-indigo-500/60 focus:bg-white/[0.06] transition-all"
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-zinc-500 hover:text-zinc-300 transition-colors"
                >
                  <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                      d={showPassword
                        ? "M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.88 9.88l-3.29-3.29m7.532 7.532l3.29 3.29M3 3l3.59 3.59m0 0A9.953 9.953 0 0112 5c4.478 0 8.268 2.943 9.543 7a10.025 10.025 0 01-4.132 5.411m0 0L21 21"
                        : "M15 12a3 3 0 11-6 0 3 3 0 016 0zM2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"}
                    />
                  </svg>
                </button>
              </div>

              {/* Indicador de fortaleza */}
              {password.length > 0 && (
                <div className="mt-2 space-y-1.5">
                  <div className="flex gap-1">
                    {[0, 1, 2, 3].map((i) => (
                      <div
                        key={i}
                        className={`h-1 flex-1 rounded-full transition-all duration-300 ${i < strengthCount ? strengthColor : 'bg-white/10'
                          }`}
                      />
                    ))}
                  </div>
                  <div className="flex flex-wrap gap-x-3 gap-y-0.5">
                    {[
                      { ok: strength[0], label: '8+ caracteres' },
                      { ok: strength[1], label: 'Mayúscula' },
                      { ok: strength[2], label: 'Minúscula' },
                      { ok: strength[3], label: 'Número' },
                    ].map(({ ok, label }) => (
                      <span
                        key={label}
                        className={`text-xs transition-colors ${ok ? 'text-emerald-400' : 'text-zinc-600'}`}
                      >
                        {ok ? '✓' : '·'} {label}
                      </span>
                    ))}
                  </div>
                </div>
              )}
              {errors.password && (
                <p className="mt-1.5 text-xs text-red-400">{errors.password.message}</p>
              )}
            </div>

            {/* Confirmar contraseña */}
            <div>
              <label className="block text-sm font-medium text-zinc-300 mb-1.5">
                Confirmar contraseña
              </label>
              <input
                {...register('confirmPassword')}
                type="password"
                autoComplete="new-password"
                placeholder="••••••••"
                className="w-full px-4 py-2.5 rounded-xl bg-white/[0.04] border border-white/[0.08] text-white placeholder-zinc-600 text-sm focus:outline-none focus:border-indigo-500/60 focus:bg-white/[0.06] transition-all"
              />
              {errors.confirmPassword && (
                <p className="mt-1.5 text-xs text-red-400">{errors.confirmPassword.message}</p>
              )}
            </div>

            {/* Error de API */}
            {register_.isError && (
              <div className="px-4 py-3 rounded-xl bg-red-500/10 border border-red-500/20 text-red-400 text-sm">
                {getApiErrorMessage(register_.error)}
              </div>
            )}

            {/* Submit */}
            <button
              type="submit"
              disabled={register_.isPending}
              className="w-full py-2.5 rounded-xl bg-indigo-600 hover:bg-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed text-white text-sm font-medium transition-colors mt-2"
            >
              {register_.isPending ? 'Creando cuenta...' : 'Crear cuenta'}
            </button>

          </form>
        </div>

        {/* Footer */}
        <p className="text-center text-sm text-zinc-600 mt-6">
          ¿Ya tienes cuenta?{' '}
          <Link to="/login" className="text-indigo-400 hover:text-indigo-300 transition-colors">
            Ingresa aquí
          </Link>
        </p>

      </div>
    </div>
  );
}
