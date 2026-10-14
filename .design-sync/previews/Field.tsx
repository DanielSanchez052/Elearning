import { Field, Input, SearchInput, Select, Textarea } from '@elearning/ui';

export const WithHint = () => (
  <div className="w-96">
    <Field label="Título del curso" hint="Se muestra en el catálogo de todos los países asignados.">
      <Input placeholder="Excel Avanzado" />
    </Field>
  </div>
);

export const WithError = () => (
  <div className="w-96">
    <Field label="Correo electrónico" error="Ingresa un correo válido.">
      <Input type="email" defaultValue="ana.gomez@" />
    </Field>
  </div>
);

export const Form = () => (
  <div className="flex w-96 flex-col gap-4">
    <Field label="Nombre completo">
      <Input defaultValue="Ana Gómez" />
    </Field>
    <Field label="País">
      <Select defaultValue="co">
        <option value="co">Colombia</option>
        <option value="mx">México</option>
      </Select>
    </Field>
    <Field label="Descripción" hint="Máximo 500 caracteres.">
      <Textarea rows={3} placeholder="Cuéntanos sobre ti" />
    </Field>
  </div>
);

export const RenderProp = () => (
  <div className="w-96">
    <Field label="Buscar curso" hint="Para controles que no son un Input directo, usa una función: recibe id y atributos aria.">
      {(control) => <SearchInput {...control} placeholder="Excel Avanzado" />}
    </Field>
  </div>
);
