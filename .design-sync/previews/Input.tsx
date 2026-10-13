import { Input } from '@elearning/ui';

export const Default = () => (
  <div className="w-80">
    <Input placeholder="Buscar cursos..." />
  </div>
);

export const Filled = () => (
  <div className="w-80">
    <Input type="email" defaultValue="ana.gomez@ejemplo.com" />
  </div>
);

export const Invalid = () => (
  <div className="w-80">
    <Input type="password" defaultValue="1234" aria-invalid />
  </div>
);

export const Disabled = () => (
  <div className="w-80">
    <Input defaultValue="CO" disabled />
  </div>
);
