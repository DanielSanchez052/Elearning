import { StateBlock, GraduationCapIcon, SearchIcon } from '@elearning/ui';

export const Empty = () => (
  <div className="w-full">
    <StateBlock
      icon={GraduationCapIcon}
      title="Aún no estás inscrito en ningún curso"
      text="Explora el catálogo e inscríbete para empezar a aprender. Tu progreso aparecerá aquí."
      action="Explorar cursos"
    />
  </div>
);

export const LoadError = () => (
  <div className="w-full">
    <StateBlock
      tipo="error"
      title="No pudimos cargar tu dashboard"
      text="Revisa tu conexión e inténtalo de nuevo. Tus cursos y tu progreso están a salvo."
    />
  </div>
);

export const CompactNoResults = () => (
  <div className="w-full">
    <StateBlock
      compact
      bordered={false}
      icon={SearchIcon}
      title="Sin resultados para «macros»"
      text="Prueba con otra palabra o revisa la ortografía."
      action="Limpiar búsqueda"
      actionVariant="outline"
    />
  </div>
);
