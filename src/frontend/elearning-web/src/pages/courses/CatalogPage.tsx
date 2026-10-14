import { useEffect, useState } from 'react';
import { useAuthStore } from '@/store/authStore';
import { useCourseCatalog } from '@/hooks/useCourses';
import { Pagination, SearchInput, Skeleton, StateBlock } from '@/components/ui';
import { BookOpenIcon, SearchIcon } from '@/components/ui/icons';
import { CourseCard } from '@/components/courses/CourseCard';
import { formatDate } from '@/lib/format';

const PAGE_SIZE = 12;

function CatalogSkeleton() {
  return (
    <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3" aria-busy="true">
      {Array.from({ length: 6 }, (_, i) => (
        <div key={i} className="overflow-hidden rounded-2xl border border-border bg-card">
          <Skeleton className="aspect-video w-full rounded-none" />
          <div className="space-y-3 p-5">
            <Skeleton className="h-4 w-3/4" />
            <Skeleton className="h-3 w-full" />
            <Skeleton className="h-3 w-2/3" />
          </div>
        </div>
      ))}
    </div>
  );
}

export default function CatalogPage() {
  const country = useAuthStore((s) => s.user?.country);
  const [search, setSearch] = useState('');
  const [query, setQuery] = useState('');
  const [page, setPage] = useState(1);

  // Busca mientras se escribe, con una pausa corta para no consultar en cada tecla.
  useEffect(() => {
    const t = setTimeout(() => {
      setQuery(search.trim());
      setPage(1);
    }, 300);
    return () => clearTimeout(t);
  }, [search]);

  const { data, isLoading, isError, refetch, isPlaceholderData } = useCourseCatalog({
    search: query || undefined,
    page,
    pageSize: PAGE_SIZE,
  });

  const courses = data?.items ?? [];
  const total = data?.totalCount ?? 0;
  const from = (page - 1) * PAGE_SIZE + 1;
  const to = from + courses.length - 1;
  const countText = query
    ? `${total} ${total === 1 ? 'resultado' : 'resultados'} para «${query}»`
    : `Mostrando ${from}–${to} de ${total} ${total === 1 ? 'curso' : 'cursos'}`;

  const clearSearch = () => {
    setSearch('');
    setQuery('');
    setPage(1);
  };

  return (
    <main className="mx-auto flex w-full max-w-7xl flex-col gap-6 px-4 py-8 sm:px-6">
      <div className="flex flex-col items-start justify-between gap-4 md:flex-row">
        <div className="flex-1">
          <h1 className="text-2xl font-semibold text-foreground">Cursos</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Cursos globales{country ? ` y los asignados a ${country}` : ''}.
          </p>
        </div>
        <SearchInput
          className="w-full md:w-80"
          aria-label="Buscar por título"
          placeholder="Buscar por título"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
      </div>

      {isLoading ? (
        <CatalogSkeleton />
      ) : isError ? (
        <StateBlock
          tipo="error"
          title="No pudimos cargar el catálogo"
          text="Revisa tu conexión e inténtalo de nuevo."
          onAction={() => refetch()}
        />
      ) : courses.length === 0 ? (
        query ? (
          <StateBlock
            icon={SearchIcon}
            title={`No encontramos cursos para «${query}»`}
            text="Prueba con otra palabra o revisa la ortografía. Solo buscamos por título."
            action="Limpiar búsqueda"
            actionVariant="secondary"
            onAction={clearSearch}
          />
        ) : (
          <StateBlock
            icon={BookOpenIcon}
            title="Todavía no hay cursos disponibles"
            text="Cuando se publiquen cursos globales o para tu país, aparecerán aquí."
          />
        )
      ) : (
        <>
          <p className="text-sm text-muted-foreground" aria-live="polite">{countText}</p>
          <div className={`grid gap-6 sm:grid-cols-2 lg:grid-cols-3 ${isPlaceholderData ? 'opacity-60' : ''}`}>
            {courses.map((course) => (
              <CourseCard
                key={course.id}
                to={`/courses/${course.id}`}
                title={course.title}
                description={course.description}
                thumbnailUrl={course.thumbnailUrl}
                isGlobal={course.isGlobal}
                instructorName={course.instructorName}
                lessonCount={course.lessonCount}
                date={formatDate(course.createdAt)}
              />
            ))}
          </div>
          <Pagination
            className="border-t border-border pt-6"
            page={page}
            totalPages={data?.totalPages ?? 1}
            onPageChange={(p) => {
              setPage(p);
              window.scrollTo({ top: 0 });
            }}
            summary={`Página ${page} de ${data?.totalPages ?? 1}`}
          />
        </>
      )}
    </main>
  );
}
