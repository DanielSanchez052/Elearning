import { Field, SearchInput } from '@elearning/ui';

export const Default = () => (
  <SearchInput className="w-80" aria-label="Buscar cursos" placeholder="Buscar cursos" />
);

export const WithValue = () => (
  <SearchInput className="w-80" aria-label="Buscar usuarios" defaultValue="ana.gomez" />
);

export const InField = () => (
  <div className="w-80">
    <Field label="Buscar usuario" hint="Por nombre o email.">
      {(control) => <SearchInput {...control} placeholder="Ana Gómez" />}
    </Field>
  </div>
);
