import { Checkbox } from '@elearning/ui';

const noop = () => {};

export const States = () => (
  <div className="flex flex-col gap-3">
    <Checkbox label="Lección requerida" checked onChange={noop} />
    <Checkbox label="Enviar recordatorio por email" checked={false} onChange={noop} />
    <Checkbox label="Deshabilitado" disabled />
  </div>
);

export const WithDescription = () => (
  <Checkbox
    label="Lección requerida"
    description="Cuenta para completar el curso y habilitar el examen final."
    defaultChecked
  />
);

export const CountryList = () => (
  <div className="flex w-72 flex-col gap-3 rounded-xl border border-border p-4">
    <p className="text-sm font-medium text-foreground-soft">Países asignados</p>
    <Checkbox label="Colombia" defaultChecked />
    <Checkbox label="México" defaultChecked />
    <Checkbox label="Chile" />
    <Checkbox label="Perú" />
  </div>
);
