import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useAdminCourses, useToggleCourseStatus, useDeleteCourse, useAdminCountries } from '@/hooks/useAdmin';
import { useAuthStore } from '../../store/authStore';
import Drawer from '@/components/ui/Drawer';
import { getApiErrorMessage } from '../../lib/axios';
import type { CourseSummaryDto } from '../../types/course.types';

const PAGE_SIZE = 20;

export default function AdminCoursesPage() {
  const user = useAuthStore((s) => s.user);
  const isSuperAdmin = user?.role === 'superadmin';
  const isInstructor = user?.role === 'instructor';

  const [search, setSearch] = useState('');
  const [query, setQuery] = useState('');
  const [isActive, setIsActive] = useState<boolean | undefined>();
  const [countryId, setCountryId] = useState<number | undefined>();
  const [page, setPage] = useState(1);
  const [selected, setSelected] = useState<CourseSummaryDto | null>(null);

  const { data, isLoading } = useAdminCourses({
    search: query || undefined,
    isActive,
    countryId,
    page,
    pageSize: PAGE_SIZE,
  });

  const { data: countries } = useAdminCountries();

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setQuery(search);
    setPage(1);
  };

  const resetFilters = () => {
    setSearch(''); setQuery('');
    setIsActive(undefined); setCountryId(undefined); setPage(1);
  };

  return (
    <div className="p-6 text-white">

      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-xl font-semibold text-white">Cursos</h1>
          <p className="text-sm text-zinc-500 mt-0.5">
            {data?.totalCount ?? '—'} cursos{isSuperAdmin ? ' en total' : ' en tu país'}
          </p>
        </div>
        {!isInstructor && (
          <Link
            to="/courses/new"
            className="flex items-center gap-2 px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white text-sm font-medium transition-colors"
          >
            <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
            </svg>
            Nuevo curso
          </Link>
        )}
      </div>

      {/* Filtros */}
      <div className="flex flex-wrap gap-3 mb-6">
        <form onSubmit={handleSearch} className="flex gap-2">
          <div className="relative">
            <svg className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-zinc-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
            <input
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Buscar cursos..."
              className="pl-9 pr-4 py-2 rounded-xl bg-white/[0.04] border border-white/[0.08] text-white placeholder-zinc-600 text-sm focus:outline-none focus:border-indigo-500/60 transition-all w-56"
            />
          </div>
          <button type="submit" className="px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white text-sm font-medium transition-colors">
            Buscar
          </button>
        </form>

        <select
          value={isActive === undefined ? '' : String(isActive)}
          onChange={(e) => { setIsActive(e.target.value === '' ? undefined : e.target.value === 'true'); setPage(1); }}
          className="px-3 py-2 rounded-xl bg-white/[0.04] border border-white/[0.08] text-sm text-zinc-300 focus:outline-none focus:border-indigo-500/60 transition-all"
        >
          <option value="">Todos los estados</option>
          <option value="true">Publicados</option>
          <option value="false">Borradores</option>
        </select>

        {isSuperAdmin && countries && (
          <select
            value={countryId ?? ''}
            onChange={(e) => { setCountryId(e.target.value ? Number(e.target.value) : undefined); setPage(1); }}
            className="px-3 py-2 rounded-xl bg-white/[0.04] border border-white/[0.08] text-sm text-zinc-300 focus:outline-none focus:border-indigo-500/60 transition-all"
          >
            <option value="">Todos los países</option>
            {countries.map((c) => (
              <option key={c.id} value={c.id}>{c.name}</option>
            ))}
          </select>
        )}

        {(query || isActive !== undefined || countryId) && (
          <button onClick={resetFilters} className="px-3 py-2 rounded-xl bg-white/[0.04] text-zinc-400 text-sm hover:bg-white/[0.08] transition-colors">
            Limpiar filtros
          </button>
        )}
      </div>

      {/* Tabla */}
      <div className="rounded-2xl border border-white/[0.06] overflow-hidden">
        <table className="w-full">
          <thead>
            <tr className="border-b border-white/[0.06] bg-white/[0.02]">
              {['Curso', 'Instructor', 'Lecciones', 'Estado', 'Alcance', ''].map((h) => (
                <th key={h} className="px-4 py-3 text-left text-xs font-medium text-zinc-500 uppercase tracking-wider">
                  {h}
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-white/[0.04]">
            {isLoading
              ? Array.from({ length: 8 }).map((_, i) => (
                <tr key={i} className="animate-pulse">
                  {Array.from({ length: 6 }).map((_, j) => (
                    <td key={j} className="px-4 py-3">
                      <div className="h-3 bg-white/[0.04] rounded-lg" />
                    </td>
                  ))}
                </tr>
              ))
              : data?.items.map((course) => (
                <tr key={course.id} className="hover:bg-white/[0.02] transition-colors">
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-3">
                      <div className="w-10 h-10 rounded-lg overflow-hidden bg-gradient-to-br from-indigo-900/40 to-zinc-900 flex-shrink-0">
                        {course.thumbnailUrl
                          ? <img src={course.thumbnailUrl} alt="" className="w-full h-full object-cover" />
                          : <div className="w-full h-full flex items-center justify-center">
                            <svg className="w-4 h-4 text-indigo-800/60" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
                            </svg>
                          </div>
                        }
                      </div>
                      <p className="text-sm text-white font-medium line-clamp-1 max-w-xs">{course.title}</p>
                    </div>
                  </td>
                  <td className="px-4 py-3 text-sm text-zinc-400">{course.instructorName}</td>
                  <td className="px-4 py-3 text-sm text-zinc-400">{course.lessonCount}</td>
                  <td className="px-4 py-3">
                    <span className={`inline-flex items-center gap-1 text-xs px-2 py-0.5 rounded-full border font-medium ${course.isActive
                      ? 'bg-emerald-500/10 border-emerald-500/20 text-emerald-400'
                      : 'bg-zinc-500/10 border-zinc-500/20 text-zinc-500'
                      }`}>
                      <span className={`w-1 h-1 rounded-full ${course.isActive ? 'bg-emerald-400' : 'bg-zinc-500'}`} />
                      {course.isActive ? 'Publicado' : 'Borrador'}
                    </span>
                  </td>
                  <td className="px-4 py-3">
                    <span className={`text-xs ${course.isGlobal ? 'text-indigo-400' : 'text-zinc-500'}`}>
                      {course.isGlobal ? 'Global' : 'Por país'}
                    </span>
                  </td>
                  <td className="px-4 py-3">
                    <button
                      onClick={() => setSelected(course)}
                      className="px-3 py-1.5 rounded-lg bg-white/[0.04] hover:bg-white/[0.08] text-zinc-400 hover:text-white text-xs transition-all"
                    >
                      Acciones
                    </button>
                  </td>
                </tr>
              ))
            }
          </tbody>
        </table>

        {!isLoading && data?.items.length === 0 && (
          <div className="text-center py-12 text-zinc-500 text-sm">
            No se encontraron cursos con los filtros aplicados.
          </div>
        )}
      </div>

      {/* Paginación */}
      {data && data.totalPages > 1 && (
        <div className="flex items-center justify-between mt-4">
          <p className="text-xs text-zinc-600">
            Mostrando {((page - 1) * PAGE_SIZE) + 1}–{Math.min(page * PAGE_SIZE, data.totalCount)} de {data.totalCount}
          </p>
          <div className="flex gap-2">
            <button onClick={() => setPage((p) => p - 1)} disabled={page === 1}
              className="px-3 py-1.5 rounded-lg bg-white/[0.04] text-zinc-400 text-xs disabled:opacity-30 hover:bg-white/[0.08] transition-colors">
              ← Anterior
            </button>
            <span className="px-3 py-1.5 text-xs text-zinc-400">{page} / {data.totalPages}</span>
            <button onClick={() => setPage((p) => p + 1)} disabled={page === data.totalPages}
              className="px-3 py-1.5 rounded-lg bg-white/[0.04] text-zinc-400 text-xs disabled:opacity-30 hover:bg-white/[0.08] transition-colors">
              Siguiente →
            </button>
          </div>
        </div>
      )}

      {/* Drawer de acciones */}
      <Drawer open={!!selected} onClose={() => setSelected(null)} title="Acciones del curso">
        {selected && (
          <CourseActionsDrawer
            course={selected}
            onClose={() => setSelected(null)}
          />
        )}
      </Drawer>
    </div>
  );
}

// ── Course Actions Drawer ─────────────────────────────────────────────────────

function CourseActionsDrawer({ course, onClose }: { course: CourseSummaryDto; onClose: () => void }) {
  const toggle = useToggleCourseStatus();
  const destroy = useDeleteCourse();
  const [error, setError] = useState('');
  const [confirmDelete, setConfirm] = useState(false);

  const handleToggle = async () => {
    try {
      await toggle.mutateAsync(course.id);
      onClose();
    } catch (e) { setError(getApiErrorMessage(e)); }
  };

  const handleDelete = async () => {
    try {
      await destroy.mutateAsync(course.id);
      onClose();
    } catch (e) { setError(getApiErrorMessage(e)); setConfirm(false); }
  };

  return (
    <div className="space-y-6">
      {/* Info */}
      <div className="p-4 rounded-xl bg-white/[0.03] border border-white/[0.06]">
        <p className="text-white font-medium text-sm line-clamp-2">{course.title}</p>
        <p className="text-zinc-500 text-xs mt-1">{course.instructorName}</p>
        <div className="mt-2 flex items-center gap-2">
          <span className={`text-xs px-2 py-0.5 rounded-full border ${course.isActive
            ? 'bg-emerald-500/10 border-emerald-500/20 text-emerald-400'
            : 'bg-zinc-500/10 border-zinc-500/20 text-zinc-500'
            }`}>
            {course.isActive ? 'Publicado' : 'Borrador'}
          </span>
          <span className="text-xs text-zinc-600">{course.lessonCount} lecciones</span>
        </div>
      </div>

      {/* Acciones */}
      <div className="space-y-2">
        <Link
          to={`/courses/${course.id}`}
          className="flex items-center gap-3 w-full px-4 py-3 rounded-xl bg-white/[0.03] border border-white/[0.06] hover:border-indigo-500/30 text-zinc-300 hover:text-white text-sm transition-all"
        >
          <svg className="w-4 h-4 text-zinc-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0zM2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
          </svg>
          Ver curso
        </Link>

        <Link
          to={`/admin/courses/${course.id}/edit`}
          className="flex items-center gap-3 w-full px-4 py-3 rounded-xl bg-white/[0.03] border border-white/[0.06] hover:border-indigo-500/30 text-zinc-300 hover:text-white text-sm transition-all"
        >
          <svg className="w-4 h-4 text-zinc-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
          </svg>
          Editar curso
        </Link>

        <Link
          to={`/admin/courses/${course.id}/quizzes`}
          className="flex items-center gap-3 w-full px-4 py-3 rounded-xl bg-white/[0.03] border border-white/[0.06] hover:border-blue-500/30 text-zinc-300 hover:text-blue-300 text-sm transition-all"
        >
          <svg className="w-4 h-4 text-zinc-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          Gestionar examen
        </Link>
        <button
          onClick={handleToggle}
          disabled={toggle.isPending}
          className="flex items-center gap-3 w-full px-4 py-3 rounded-xl bg-white/[0.03] border border-white/[0.06] hover:border-amber-500/30 text-zinc-300 hover:text-amber-300 text-sm transition-all disabled:opacity-50"
        >
          <svg className="w-4 h-4 text-zinc-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d={course.isActive
              ? "M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728A9 9 0 015.636 5.636m12.728 12.728L5.636 5.636"
              : "M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
            } />
          </svg>
          {toggle.isPending ? 'Procesando...' : course.isActive ? 'Despublicar' : 'Publicar'}
        </button>

        {/* Eliminar con confirmación */}
        {!confirmDelete ? (
          <button
            onClick={() => setConfirm(true)}
            className="flex items-center gap-3 w-full px-4 py-3 rounded-xl bg-white/[0.03] border border-white/[0.06] hover:border-red-500/30 text-zinc-300 hover:text-red-400 text-sm transition-all"
          >
            <svg className="w-4 h-4 text-zinc-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
            </svg>
            Eliminar curso
          </button>
        ) : (
          <div className="p-4 rounded-xl bg-red-500/10 border border-red-500/20 space-y-3">
            <p className="text-red-400 text-sm">¿Confirmas que quieres eliminar este curso?</p>
            <div className="flex gap-2">
              <button
                onClick={handleDelete}
                disabled={destroy.isPending}
                className="flex-1 py-2 rounded-lg bg-red-600 hover:bg-red-500 text-white text-sm font-medium transition-colors disabled:opacity-50"
              >
                {destroy.isPending ? 'Eliminando...' : 'Sí, eliminar'}
              </button>
              <button
                onClick={() => setConfirm(false)}
                className="flex-1 py-2 rounded-lg bg-white/[0.04] text-zinc-400 text-sm hover:bg-white/[0.08] transition-colors"
              >
                Cancelar
              </button>
            </div>
          </div>
        )}
      </div>

      {
        error && (
          <div className="px-4 py-3 rounded-xl bg-red-500/10 border border-red-500/20 text-red-400 text-sm">
            {error}
          </div>
        )
      }
    </div >
  );
}
