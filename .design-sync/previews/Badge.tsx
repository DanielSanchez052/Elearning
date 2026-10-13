import { Badge, CheckIcon, GlobeIcon } from '@elearning/ui';

export const Tones = () => (
  <div className="flex flex-wrap gap-2">
    <Badge>Borrador</Badge>
    <Badge tone="brand">Global</Badge>
    <Badge tone="success">Inscrito</Badge>
    <Badge tone="warning">Pendiente</Badge>
    <Badge tone="info">Nuevo</Badge>
    <Badge tone="danger">Inactivo</Badge>
  </div>
);

export const WithIcon = () => (
  <div className="flex flex-wrap gap-2">
    <Badge tone="success"><CheckIcon /> Aprobado</Badge>
    <Badge tone="brand"><GlobeIcon /> 3 países</Badge>
  </div>
);

export const Roles = () => (
  <div className="flex flex-wrap gap-2">
    <Badge tone="danger">superadmin</Badge>
    <Badge tone="warning">admin</Badge>
    <Badge tone="info">instructor</Badge>
    <Badge>student</Badge>
  </div>
);
