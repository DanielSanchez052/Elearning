import type React from 'react';
import { Modal, Button, Field, Input } from '@elearning/ui';

const noop = () => {};
// Los overlays usan position: fixed; este marco (transform) los contiene para que la tarjeta muestre el estado abierto completo.
const Frame = ({ children, height }: { children: React.ReactNode; height: number }) => (
  <div className="relative overflow-hidden rounded-xl" style={{ height, transform: 'translateZ(0)' }}>{children}</div>
);


export const ConfirmDelete = () => (
  <Frame height={400}>
  <Modal
    open
    onClose={noop}
    size="sm"
    title="Eliminar lección"
    description="Esta acción no se puede deshacer."
    footer={
      <>
        <Button variant="secondary">Cancelar</Button>
        <Button variant="destructive">Eliminar</Button>
      </>
    }
  >
    <p className="text-sm text-foreground-soft">Se eliminará "Introducción a las tablas dinámicas" y el progreso de 24 estudiantes.</p>
  </Modal>
  </Frame>
);

export const FormDialog = () => (
  <Frame height={460}>
  <Modal
    open
    onClose={noop}
    title="Nuevo país"
    description="Los cursos asignados a este país serán visibles para sus usuarios."
    footer={
      <>
        <Button variant="secondary">Cancelar</Button>
        <Button type="submit">Crear país</Button>
      </>
    }
  >
    <div className="flex flex-col gap-4">
      <Field label="Nombre">
        <Input defaultValue="Chile" />
      </Field>
      <Field label="Código" hint="Dos letras, ISO 3166-1.">
        <Input defaultValue="CL" />
      </Field>
    </div>
  </Modal>
  </Frame>
);
