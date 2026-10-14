import { CloseButton } from '@elearning/ui';

export const InHeader = () => (
  <div className="flex w-80 items-center justify-between rounded-xl border border-border bg-card px-6 py-4">
    <h2 className="text-sm font-semibold text-foreground">Detalle del usuario</h2>
    <CloseButton />
  </div>
);

export const Default = () => <CloseButton aria-label="Cerrar notificaciones" />;
