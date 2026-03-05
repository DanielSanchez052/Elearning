import { AuthLayout } from '@/features/auth/AuthLayout';
import { useVerifyEmail } from '@/features/auth/useAuth';
import { Link, useSearchParams } from 'react-router-dom';

export default function VerifyEmailPage() {
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token') ?? '';
  const { isLoading, isSuccess, isError } = useVerifyEmail(token);

  return (
    <AuthLayout title="Verificación de email" subtitle="">
      <div className="text-center space-y-4">
        {isLoading && (
          <>
            <div className="inline-flex items-center justify-center w-14 h-14 rounded-2xl bg-indigo-500/10 border border-indigo-500/20 mb-2">
              <svg className="w-6 h-6 text-indigo-400 animate-spin" fill="none" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
              </svg>
            </div>
            <p className="text-zinc-400 text-sm">Verificando tu email...</p>
          </>
        )}

        {isSuccess && (
          <>
            <div className="inline-flex items-center justify-center w-14 h-14 rounded-2xl bg-emerald-500/10 border border-emerald-500/20 mb-2">
              <svg className="w-7 h-7 text-emerald-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
              </svg>
            </div>
            <p className="text-zinc-300 text-sm">¡Tu email ha sido verificado correctamente!</p>
            <Link
              to="/login"
              className="inline-block mt-2 px-6 py-2.5 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white text-sm font-medium transition-colors"
            >
              Ingresar ahora
            </Link>
          </>
        )}

        {isError && (
          <>
            <div className="inline-flex items-center justify-center w-14 h-14 rounded-2xl bg-red-500/10 border border-red-500/20 mb-2">
              <svg className="w-7 h-7 text-red-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </div>
            <p className="text-zinc-400 text-sm">
              El enlace de verificación no es válido o ya fue utilizado.
            </p>
            <Link
              to="/login"
              className="inline-block text-sm text-indigo-400 hover:text-indigo-300 transition-colors"
            >
              Volver al login
            </Link>
          </>
        )}
      </div>
    </AuthLayout>
  );
}

