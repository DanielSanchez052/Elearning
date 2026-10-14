import { Button, PlusIcon, TrashIcon, PencilIcon, CheckIcon } from '@elearning/ui';

export const Variants = () => (
  <div className="flex flex-wrap items-center gap-3">
    <Button>Guardar curso</Button>
    <Button variant="secondary">Cancelar</Button>
    <Button variant="outline">Vista previa</Button>
    <Button variant="ghost">Ver todos</Button>
    <Button variant="soft"><PlusIcon /> Agregar lección</Button>
  </div>
);

export const StatusActions = () => (
  <div className="flex flex-wrap items-center gap-3">
    <Button variant="success"><CheckIcon /> Marcar completada</Button>
    <Button variant="danger"><TrashIcon /> Eliminar</Button>
    <Button variant="destructive">Eliminar curso</Button>
  </div>
);

export const Sizes = () => (
  <div className="flex flex-wrap items-center gap-3">
    <Button size="sm">Anterior</Button>
    <Button size="md">Inscribirme</Button>
    <Button size="lg">Comenzar examen</Button>
    <Button size="icon" variant="secondary" aria-label="Editar lección"><PencilIcon /></Button>
  </div>
);

export const States = () => (
  <div className="flex flex-wrap items-center gap-3">
    <Button disabled>Guardando...</Button>
    <Button variant="secondary" disabled>Siguiente</Button>
  </div>
);

export const FullWidth = () => (
  <div className="w-80">
    <Button type="submit" fullWidth>Iniciar sesión</Button>
  </div>
);
