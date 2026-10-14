import { Button, Spinner } from '@elearning/ui';

export const Sizes = () => (
  <div className="flex items-center gap-4">
    <Spinner size="sm" />
    <Spinner size="md" />
    <Spinner size="lg" />
  </div>
);

export const VerifyingEmail = () => (
  <div className="flex flex-col items-center gap-3 text-center">
    <Spinner size="lg" label="Verificando tu email" />
    <p className="text-sm text-muted-foreground">Verificando tu email…</p>
  </div>
);

export const InButton = () => (
  <Button disabled>
    <Spinner size="sm" className="text-primary-foreground" /> Guardando orden
  </Button>
);
