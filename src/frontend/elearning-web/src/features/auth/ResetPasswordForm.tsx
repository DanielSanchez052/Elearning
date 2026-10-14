import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import {
  useResetPassword,
  resetPasswordSchema,
  type ResetPasswordFormData,
  getApiErrorMessage,
} from '@/hooks/useAuth';
import { Alert, Button, Field } from '@/components/ui';
import { PasswordInput } from './PasswordInput';

/** Formulario de nueva contraseña. La página decide qué mostrar si el enlace no trae token. */
export default function ResetPasswordForm({ token }: { token: string }) {
  const reset = useResetPassword(token);

  const { register, handleSubmit, formState: { errors } } = useForm<ResetPasswordFormData>({
    resolver: zodResolver(resetPasswordSchema),
  });

  const onSubmit = (data: ResetPasswordFormData) => reset.mutate(data);

  return (
    <form onSubmit={handleSubmit(onSubmit)} noValidate className="flex flex-col gap-4">
      {reset.isError && <Alert tone="danger">{getApiErrorMessage(reset.error)}</Alert>}

      <Field label="Nueva contraseña" hint="Mínimo 8 caracteres, con mayúscula, minúscula y número." error={errors.newPassword?.message}>
        {(control) => (
          <PasswordInput {...control} {...register('newPassword')} autoComplete="new-password" placeholder="••••••••" />
        )}
      </Field>

      <Field label="Confirmar contraseña" error={errors.confirmPassword?.message}>
        {(control) => (
          <PasswordInput {...control} {...register('confirmPassword')} autoComplete="new-password" placeholder="••••••••" />
        )}
      </Field>

      <Button type="submit" size="lg" fullWidth disabled={reset.isPending} className="mt-2">
        {reset.isPending ? 'Guardando…' : 'Guardar contraseña'}
      </Button>
    </form>
  );
}
