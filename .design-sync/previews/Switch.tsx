import { Switch } from '@elearning/ui';

const noop = () => {};

export const States = () => (
  <div className="flex items-center gap-4">
    <Switch checked onCheckedChange={noop} aria-label="País activo" />
    <Switch checked={false} onCheckedChange={noop} aria-label="País inactivo" />
    <Switch checked disabled onCheckedChange={noop} aria-label="Deshabilitado" />
  </div>
);

export const WithLabel = () => (
  <div className="w-80">
    <Switch checked onCheckedChange={noop} label="Pregunta requerida" description="El estudiante debe responderla para enviar." />
  </div>
);
