import { useForm, useWatch } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import {
  useRegister,
  useActiveCountries,
  registerSchema,
  type RegisterFormData,
  getApiErrorMessage,
} from '@/hooks/useAuth';
import { Alert, Button, Field, Input, Select } from '@/components/ui';
import { CheckIcon } from '@/components/ui/icons';
import { cn } from '@/lib/utils';
import { PasswordInput } from './PasswordInput';

const requirements = [
  { label: '8+ caracteres', test: (p: string) => p.length >= 8 },
  { label: 'Mayúscula', test: (p: string) => /[A-Z]/.test(p) },
  { label: 'Minúscula', test: (p: string) => /[a-z]/.test(p) },
  { label: 'Número', test: (p: string) => /[0-9]/.test(p) },
];

/** Indicador de fortaleza: cuatro barras y la lista de requisitos que pide el backend. */
function PasswordStrength({ password }: { password: string }) {
  const met = requirements.map((r) => r.test(password));
  const count = met.filter(Boolean).length;
  const barTone = count <= 1 ? 'bg-destructive' : count <= 3 ? 'bg-warning' : 'bg-success';

  return (
    <div className="mt-2 flex flex-col gap-1.5" aria-live="polite">
      <div className="flex gap-1" aria-hidden="true">
        {requirements.map((r, i) => (
          <div key={r.label} className={cn('h-1 flex-1 rounded-full transition-colors', i < count ? barTone : 'bg-secondary-hover')} />
        ))}
      </div>
      <ul className="flex flex-wrap gap-x-3 gap-y-0.5">
        {requirements.map((r, i) => (
          <li
            key={r.label}
            className={cn('flex items-center gap-1 text-xs', met[i] ? 'text-success-foreground' : 'text-subtle-foreground')}
          >
            {met[i] ? <CheckIcon className="size-3" aria-hidden="true" /> : <span aria-hidden="true">·</span>}
            {r.label}
            <span className="sr-only">{met[i] ? ' (cumplido)' : ' (pendiente)'}</span>
          </li>
        ))}
      </ul>
    </div>
  );
}

export default function RegisterForm() {
  const register_ = useRegister();
  const { data: countries, isLoading: loadingCountries } = useActiveCountries();

  const { register, handleSubmit, control, formState: { errors } } = useForm<RegisterFormData>({
    resolver: zodResolver(registerSchema),
  });

  const password = useWatch({ control, name: 'password', defaultValue: '' }) ?? '';

  const onSubmit = (data: RegisterFormData) => register_.mutate(data);

  return (
    <form onSubmit={handleSubmit(onSubmit)} noValidate className="flex flex-col gap-4">
      {register_.isError && <Alert tone="danger">{getApiErrorMessage(register_.error)}</Alert>}

      <Field label="Nombre completo" error={errors.fullName?.message}>
        <Input {...register('fullName')} autoComplete="name" placeholder="Ana Gómez" />
      </Field>

      <Field label="Correo electrónico" error={errors.email?.message}>
        <Input {...register('email')} type="email" autoComplete="email" placeholder="tu@correo.com" />
      </Field>

      <div>
        <Field label="Contraseña" error={errors.password?.message} hint={password ? undefined : 'Mínimo 8 caracteres, con mayúscula, minúscula y número.'}>
          {(control) => (
            <PasswordInput {...control} {...register('password')} autoComplete="new-password" placeholder="••••••••" />
          )}
        </Field>
        {password.length > 0 && <PasswordStrength password={password} />}
      </div>

      <Field label="Confirmar contraseña" error={errors.confirmPassword?.message}>
        {(control) => (
          <PasswordInput {...control} {...register('confirmPassword')} autoComplete="new-password" placeholder="••••••••" />
        )}
      </Field>

      <Field label="País" error={errors.countryId?.message}>
        <Select {...register('countryId', { valueAsNumber: true })} disabled={loadingCountries} defaultValue={0}>
          <option value={0}>{loadingCountries ? 'Cargando países…' : 'Selecciona tu país'}</option>
          {countries?.map((c) => (
            <option key={c.id} value={c.id}>
              {c.name}
            </option>
          ))}
        </Select>
      </Field>

      <Button type="submit" size="lg" fullWidth disabled={register_.isPending} className="mt-2">
        {register_.isPending ? 'Creando cuenta…' : 'Crear cuenta'}
      </Button>
    </form>
  );
}
