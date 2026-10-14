import { Link } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useLogin, loginSchema, type LoginFormData, getApiErrorMessage } from '@/hooks/useAuth';
import { Alert, Button, Field, Input } from '@/components/ui';
import { PasswordInput } from './PasswordInput';

export default function LoginForm() {
  const login = useLogin();

  const { register, handleSubmit, formState: { errors } } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
  });

  const onSubmit = (data: LoginFormData) => login.mutate(data);

  return (
    <form onSubmit={handleSubmit(onSubmit)} noValidate className="flex flex-col gap-4">
      {login.isError && <Alert tone="danger">{getApiErrorMessage(login.error)}</Alert>}

      <Field label="Correo electrónico" error={errors.email?.message}>
        <Input {...register('email')} type="email" autoComplete="email" placeholder="tu@correo.com" />
      </Field>

      <div className="flex flex-col gap-1.5">
        <Field label="Contraseña" error={errors.password?.message}>
          {(control) => (
            <PasswordInput {...control} {...register('password')} autoComplete="current-password" placeholder="••••••••" />
          )}
        </Field>
        <Link to="/forgot-password" className="self-end text-xs text-brand-foreground hover:text-foreground">
          ¿Olvidaste tu contraseña?
        </Link>
      </div>

      <Button type="submit" size="lg" fullWidth disabled={login.isPending} className="mt-2">
        {login.isPending ? 'Ingresando…' : 'Iniciar sesión'}
      </Button>
    </form>
  );
}
