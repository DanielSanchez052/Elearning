import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { forgotPasswordSchema, type ForgotPasswordFormData } from '@/hooks/useAuth';
import { Alert, Button, Field, Input } from '@/components/ui';

interface ForgotPasswordFormProps {
  onSubmit: (data: ForgotPasswordFormData) => void;
  isPending: boolean;
  /** Mensaje de error de la API. */
  error?: string;
}

/** Formulario de recuperación. La página envía la solicitud y muestra el mensaje «enlace enviado». */
export default function ForgotPasswordForm({ onSubmit, isPending, error }: ForgotPasswordFormProps) {
  const { register, handleSubmit, formState: { errors } } = useForm<ForgotPasswordFormData>({
    resolver: zodResolver(forgotPasswordSchema),
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)} noValidate className="flex flex-col gap-4">
      {error && <Alert tone="danger">{error}</Alert>}

      <Field label="Correo electrónico" error={errors.email?.message}>
        <Input {...register('email')} type="email" autoComplete="email" placeholder="tu@correo.com" />
      </Field>

      <Button type="submit" size="lg" fullWidth disabled={isPending} className="mt-2">
        {isPending ? 'Enviando…' : 'Enviar enlace'}
      </Button>
    </form>
  );
}
