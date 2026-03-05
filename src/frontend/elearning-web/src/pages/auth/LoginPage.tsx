import { Link, useSearchParams } from 'react-router-dom';
import LoginForm from '@/features/auth/LoginForm';

export default function LoginPage() {
  const [searchParams] = useSearchParams();
  const justRegistered = searchParams.get('registered') === 'true';
  const justReset = searchParams.get('reset') === 'true';

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
          <h1 className="text-2xl font-semibold text-white tracking-tight">Bienvenido de nuevo</h1>
          <p className="text-sm text-zinc-500 mt-1">Ingresa a tu cuenta para continuar</p>
        </div>

        {/* Notificaciones */}
        {justRegistered && (
          <div className="mb-4 px-4 py-3 rounded-xl bg-emerald-500/10 border border-emerald-500/20 text-emerald-400 text-sm text-center">
            Cuenta creada. Revisa tu email para verificarla antes de ingresar.
          </div>
        )}
        {justReset && (
          <div className="mb-4 px-4 py-3 rounded-xl bg-emerald-500/10 border border-emerald-500/20 text-emerald-400 text-sm text-center">
            Contraseña actualizada. Ya puedes ingresar.
          </div>
        )}

        {/* Card */}
        <div className="bg-[#111118] border border-white/[0.06] rounded-2xl p-8 shadow-2xl">
          <LoginForm />
        </div>

        {/* Footer */}
        <p className="text-center text-sm text-zinc-600 mt-6">
          ¿No tienes cuenta?{' '}
          <Link to="/register" className="text-indigo-400 hover:text-indigo-300 transition-colors">
            Regístrate
          </Link>
        </p>

      </div>
    </div>
  );
}
