import type React from 'react';
import { Drawer, Badge, Button, Field, Select } from '@elearning/ui';

const noop = () => {};
// Los overlays usan position: fixed; este marco (transform) los contiene para que la tarjeta muestre el estado abierto completo.
const Frame = ({ children, height }: { children: React.ReactNode; height: number }) => (
  <div className="relative overflow-hidden rounded-xl" style={{ height, transform: 'translateZ(0)' }}>{children}</div>
);


export const UserDetail = () => (
  <Frame height={440}>
  <Drawer open onClose={noop} title="Detalle del usuario">
    <div className="flex flex-col gap-5">
      <div>
        <p className="text-base font-semibold text-foreground">Ana Gómez</p>
        <p className="text-sm text-muted-foreground">ana.gomez@ejemplo.com</p>
      </div>
      <div className="flex gap-2">
        <Badge tone="info">instructor</Badge>
        <Badge>Colombia</Badge>
      </div>
      <Field label="Rol">
        <Select defaultValue="instructor">
          <option value="student">Estudiante</option>
          <option value="instructor">Instructor</option>
          <option value="admin">Administrador</option>
        </Select>
      </Field>
      <Button fullWidth>Guardar cambios</Button>
    </div>
  </Drawer>
  </Frame>
);
