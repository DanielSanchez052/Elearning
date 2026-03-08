import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useCourseCatalog } from '@/hooks/useCourses';
import AppHeader from '@/components/layout/AppHeader';
import type { CourseSummaryDto } from '@/types/course.types';

export default function CatalogPage() {
  const [search, setSearch] = useState('');
  const [query, setQuery] = useState('');
  const [page, setPage] = useState(1);
  const PAGE_SIZE = 12;

  const { data, isLoading, isError } = useCourseCatalog({
    search: query || undefined,
    page,
    pageSize: PAGE_SIZE,
  });

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setQuery(search);
    setPage(1);
  };

  return (
    <div className="min-h-screen bg-[#0a0a0f] text-white">
      <AppHeader />

      {/* Header */}
      <div className="border-b border-white/[0.06] bg-[#0a0a0f]/80">
        <div className="max-w-7xl mx-auto px-6 py-4 flex items-center justify-between gap-4">
          <div className="flex-1">
            <h1 className="text-lg font-semibold text-white">Catálogo de cursos</h1>
            {data && (
              <p className="text-xs text-zinc-500 mt-0.5">
                {data.totalCount} {data.totalCount === 1 ? 'curso disponible' : 'cursos disponibles'} en tu región
              </p>
            )}
          </div>

          {/* Búsqueda */}
          <div className="flex items-center gap-3">
            <form onSubmit={handleSearch} className="flex items-center gap-2">
              <div className="relative">
                <svg className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-zinc-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                </svg>
                <input
                  value={search}
                  onChange={(e) => setSearch(e.target.value)}
                  placeholder="Buscar cursos..."
                  className="pl-9 pr-4 py-2 rounded-xl bg-white/[0.04] border border-white/[0.08] text-white placeholder-zinc-600 text-sm focus:outline-none focus:border-indigo-500/60 transition-all w-64"
                />
              </div>
              <button
                type="submit"
                className="px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white text-sm font-medium transition-colors"
              >
                Buscar
              </button>
              {query && (
                <button
                  type="button"
                  onClick={() => { setSearch(''); setQuery(''); setPage(1); }}
                  className="px-3 py-2 rounded-xl bg-white/[0.04] hover:bg-white/[0.08] text-zinc-400 text-sm transition-colors"
                >
                  ✕
                </button>
              )}
            </form>
          </div>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-6 py-8">

        {/* Loading */}
        {isLoading && (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5">
            {Array.from({ length: 6 }).map((_, i) => (
              <div key={i} className="rounded-2xl bg-[#111118] border border-white/[0.06] overflow-hidden animate-pulse">
                <div className="h-44 bg-white/[0.04]" />
                <div className="p-5 space-y-3">
                  <div className="h-4 bg-white/[0.04] rounded-lg w-3/4" />
                  <div className="h-3 bg-white/[0.04] rounded-lg w-1/2" />
                  <div className="h-3 bg-white/[0.04] rounded-lg w-full" />
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Error */}
        {isError && (
          <div className="text-center py-20">
            <p className="text-zinc-500 text-sm">No se pudieron cargar los cursos. Intenta de nuevo.</p>
          </div>
        )}

        {/* Sin resultados */}
        {!isLoading && !isError && data?.items.length === 0 && (
          <div className="text-center py-20 space-y-3">
            <div className="inline-flex items-center justify-center w-16 h-16 rounded-2xl bg-white/[0.04] border border-white/[0.06] mb-2">
              <svg className="w-8 h-8 text-zinc-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9.172 16.172a4 4 0 015.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
            <p className="text-zinc-400 text-sm">No se encontraron cursos{query ? ` para "${query}"` : ' en tu región'}.</p>
          </div>
        )}

        {/* Grid de cursos */}
        {!isLoading && !isError && data && data.items.length > 0 && (
          <>
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5">
              {data.items.map((course) => (
                <CourseCard key={course.id} course={course} />
              ))}
            </div>

            {/* Paginación */}
            {data.totalPages > 1 && (
              <div className="flex items-center justify-center gap-2 mt-10">
                <button
                  onClick={() => setPage((p) => Math.max(1, p - 1))}
                  disabled={page === 1}
                  className="px-4 py-2 rounded-xl bg-white/[0.04] border border-white/[0.06] text-zinc-400 text-sm disabled:opacity-30 hover:bg-white/[0.08] transition-colors"
                >
                  ← Anterior
                </button>

                <div className="flex items-center gap-1">
                  {Array.from({ length: data.totalPages }, (_, i) => i + 1)
                    .filter((p) => p === 1 || p === data.totalPages || Math.abs(p - page) <= 1)
                    .reduce<(number | '...')[]>((acc, p, i, arr) => {
                      if (i > 0 && (p as number) - (arr[i - 1] as number) > 1) acc.push('...');
                      acc.push(p);
                      return acc;
                    }, [])
                    .map((p, i) =>
                      p === '...' ? (
                        <span key={`dots-${i}`} className="px-2 text-zinc-600 text-sm">…</span>
                      ) : (
                        <button
                          key={p}
                          onClick={() => setPage(p as number)}
                          className={`w-9 h-9 rounded-xl text-sm font-medium transition-colors ${page === p
                            ? 'bg-indigo-600 text-white'
                            : 'bg-white/[0.04] text-zinc-400 hover:bg-white/[0.08]'
                            }`}
                        >
                          {p}
                        </button>
                      )
                    )}
                </div>

                <button
                  onClick={() => setPage((p) => Math.min(data.totalPages, p + 1))}
                  disabled={page === data.totalPages}
                  className="px-4 py-2 rounded-xl bg-white/[0.04] border border-white/[0.06] text-zinc-400 text-sm disabled:opacity-30 hover:bg-white/[0.08] transition-colors"
                >
                  Siguiente →
                </button>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
}

// ── Course Card ───────────────────────────────────────────────────────────────

function CourseCard({ course }: { course: CourseSummaryDto }) {
  return (
    <Link
      to={`/courses/${course.id}`}
      className="group block rounded-2xl bg-[#111118] border border-white/[0.06] overflow-hidden hover:border-indigo-500/30 hover:shadow-lg hover:shadow-indigo-500/5 transition-all duration-300"
    >
      {/* Thumbnail */}
      <div className="relative h-44 bg-gradient-to-br from-indigo-900/40 to-zinc-900 overflow-hidden">
        {course.thumbnailUrl ? (
          <img
            src={course.thumbnailUrl}
            alt={course.title}
            className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
          />
        ) : (
          <div className="w-full h-full flex items-center justify-center">
            <svg className="w-12 h-12 text-indigo-800/60" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
            </svg>
          </div>
        )}
        {/* Badge global */}
        {course.isGlobal && (
          <span className="absolute top-3 right-3 px-2 py-0.5 rounded-full bg-indigo-500/20 border border-indigo-500/30 text-indigo-300 text-xs font-medium">
            Global
          </span>
        )}
      </div>

      {/* Info */}
      <div className="p-5">
        <h3 className="font-medium text-white text-sm leading-snug line-clamp-2 group-hover:text-indigo-300 transition-colors">
          {course.title}
        </h3>
        {course.description && (
          <p className="mt-1.5 text-xs text-zinc-500 line-clamp-2 leading-relaxed">
            {course.description}
          </p>
        )}

        {/* Footer de la card */}
        <div className="mt-4 pt-4 border-t border-white/[0.06] flex items-center justify-between">
          <span className="text-xs text-zinc-600">{course.instructorName}</span>
          <div className="flex items-center gap-1 text-xs text-zinc-500">
            <svg className="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
            </svg>
            {course.lessonCount} {course.lessonCount === 1 ? 'lección' : 'lecciones'}
          </div>
        </div>
      </div>
    </Link>
  );
}
