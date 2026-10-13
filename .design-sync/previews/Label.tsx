import { Label, Input } from '@elearning/ui';

export const WithControl = () => (
  <div className="w-80">
    <Label htmlFor="search">Buscar por título</Label>
    <Input id="search" placeholder="Excel Avanzado" />
  </div>
);
