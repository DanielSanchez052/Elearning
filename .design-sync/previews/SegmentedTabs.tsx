import { SegmentedTabs } from '@elearning/ui';

const noop = () => {};

export const MyCourses = () => (
  <SegmentedTabs
    aria-label="Estado del curso"
    value="activo"
    onChange={noop}
    options={[
      { value: 'todos', label: 'Todos', count: 4 },
      { value: 'activo', label: 'Activos', count: 2 },
      { value: 'completado', label: 'Completados', count: 1 },
      { value: 'abandonado', label: 'Abandonados', count: 1 },
    ]}
  />
);

export const AdminCourses = () => (
  <SegmentedTabs
    aria-label="Estado de publicación"
    value="publicados"
    onChange={noop}
    options={[
      { value: 'publicados', label: 'Publicados' },
      { value: 'borradores', label: 'Borradores' },
    ]}
  />
);
