import { useState } from 'react';
import { useAdminUsers, useChangeUserRole, useChangeUserCountry } from '@/hooks/admin/users';
import { useAdminCountries } from '@/hooks/admin/countries';
import { useAuthStore } from '../../store/authStore';
import Drawer from '@/components/ui/Drawer';
import { getApiErrorMessage } from '../../lib/axios';
import type { UserSummaryDto, UserRole } from '@/types/user.types';
import { ROLES } from '@/types/user.types';

const PAGE_SIZE = 20;

export default function AdminUsersPage() {
  const user = useAuthStore((s) => s.user);
  const isSuperAdmin = user?.role === 'superadmin';

  // Filtros
  const [search, setSearch] = useState('');
  const [query, setQuery] = useState('');
  const [role, setRole] = useState('');
  const [countryId, setCountryId] = useState<number | undefined>();
  const [verified, setVerified] = useState<boolean | undefined>();
  const [page, setPage] = useState(1);

  // Drawer
  const [selected, setSelected] = useState<UserSummaryDto | null>(null);

  const { data, isLoading } = useAdminUsers({
    search: query || undefined,
    role: role || undefined,
    countryId,
    isEmailVerified: verified,
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
    setSearch(''); setQuery(''); setRole('');
    setCountryId(undefined); setVerified(undefined); setPage(1);
  };

  return (
    <div className="p-6 text-white">

      {/* Header */}
      <div className="mb-6">
        <h1 className="text-xl font-semibold text-white">Usuarios</h1>
        <p className="text-sm text-zinc-500 mt-0.5">
          {data?.totalCount ?? '—'} usuarios{isSuperAdmin ? ' en total' : ' en tu país'}
        </p>
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
              placeholder="Buscar por nombre o email..."
              className="pl-9 pr-4 py-2 rounded-xl bg-white/[0.04] border border-white/[0.08] text-white placeholder-zinc-600 text-sm focus:outline-none focus:border-indigo-500/60 transition-all w-64"
            />
          </div>
          <button type="submit" className="px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white text-sm font-medium transition-colors">
            Buscar
          </button>
        </form>

        {/* Filtro rol */}
        <select
          value={role}
          onChange={(e) => { setRole(e.target.value); setPage(1); }}
          className="px-3 py-2 rounded-xl bg-white/[0.04] border border-white/[0.08] text-sm text-zinc-300 focus:outline-none focus:border-indigo-500/60 transition-all"
        >
          <option value="">Todos los roles</option>
          {Object.entries(ROLES).map(([k, v]) => (
            <option key={k} value={k}>{v}</option>
          ))}
        </select>

        {/* Filtro país — solo Super Admin */}
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

        {/* Filtro verificado */}
        <select
          value={verified === undefined ? '' : String(verified)}
          onChange={(e) => { setVerified(e.target.value === '' ? undefined : e.target.value === 'true'); setPage(1); }}
          className="px-3 py-2 rounded-xl bg-white/[0.04] border border-white/[0.08] text-sm text-zinc-300 focus:outline-none focus:border-indigo-500/60 transition-all"
        >
          <option value="">Todos</option>
          <option value="true">Verificados</option>
          <option value="false">Sin verificar</option>
        </select>

        {(query || role || countryId || verified !== undefined) && (
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
              {['Usuario', 'Rol', 'País', 'Verificado', 'Registro', ''].map((h) => (
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
              : data?.items.map((u) => (
                <tr key={u.id} className="hover:bg-white/[0.02] transition-colors">
                  <td className="px-4 py-3">
                    <div>
                      <p className="text-sm text-white font-medium">{u.fullName}</p>
                      <p className="text-xs text-zinc-500 mt-0.5">{u.email}</p>
                    </div>
                  </td>
                  <td className="px-4 py-3">
                    <RoleBadge role={u.role} />
                  </td>
                  <td className="px-4 py-3 text-sm text-zinc-400">{u.country}</td>
                  <td className="px-4 py-3">
                    <span className={`inline-flex items-center gap-1 text-xs ${u.isEmailVerified ? 'text-emerald-400' : 'text-zinc-600'
                      }`}>
                      <span className={`w-1.5 h-1.5 rounded-full ${u.isEmailVerified ? 'bg-emerald-400' : 'bg-zinc-600'
                        }`} />
                      {u.isEmailVerified ? 'Verificado' : 'Pendiente'}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-xs text-zinc-600">
                    {new Date(u.createdAt).toLocaleDateString('es', { day: '2-digit', month: 'short', year: 'numeric' })}
                  </td>
                  <td className="px-4 py-3">
                    <button
                      onClick={() => setSelected(u)}
                      className="px-3 py-1.5 rounded-lg bg-white/[0.04] hover:bg-white/[0.08] text-zinc-400 hover:text-white text-xs transition-all"
                    >
                      Editar
                    </button>
                  </td>
                </tr>
              ))
            }
          </tbody>
        </table>

        {!isLoading && data?.items.length === 0 && (
          <div className="text-center py-12 text-zinc-500 text-sm">
            No se encontraron usuarios con los filtros aplicados.
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
            <span className="px-3 py-1.5 text-xs text-zinc-400">
              {page} / {data.totalPages}
            </span>
            <button onClick={() => setPage((p) => p + 1)} disabled={page === data.totalPages}
              className="px-3 py-1.5 rounded-lg bg-white/[0.04] text-zinc-400 text-xs disabled:opacity-30 hover:bg-white/[0.08] transition-colors">
              Siguiente →
            </button>
          </div>
        </div>
      )}

      {/* Drawer de edición */}
      <Drawer
        open={!!selected}
        onClose={() => setSelected(null)}
        title="Editar usuario"
      >
        {selected && (
          <UserEditForm
            user={selected}
            countries={countries ?? []}
            isSuperAdmin={isSuperAdmin}
            onSuccess={() => setSelected(null)}
          />
        )}
      </Drawer>
    </div>
  );
}

// ── User Edit Form ────────────────────────────────────────────────────────────

function UserEditForm({
  user,
  countries,
  isSuperAdmin,
  onSuccess,
}: {
  user: UserSummaryDto;
  countries: import('@/types/user.types').Country[];
  isSuperAdmin: boolean;
  onSuccess: () => void;
}) {
  const changeRole = useChangeUserRole();
  const changeCountry = useChangeUserCountry();

  const [role, setRole] = useState(user.role);
  const [countryId, setCountryId] = useState(user.countryId);
  const [error, setError] = useState('');

  // Roles que puede asignar según el rol del editor
  const assignableRoles: UserRole[] = isSuperAdmin
    ? ['student', 'instructor', 'admin', 'superadmin']
    : ['student', 'instructor'];

  const handleSave = async () => {
    setError('');
    try {
      if (role !== user.role)
        await changeRole.mutateAsync({ id: user.id, role });
      if (isSuperAdmin && countryId !== user.countryId)
        await changeCountry.mutateAsync({ id: user.id, countryId });
      onSuccess();
    } catch (e) {
      setError(getApiErrorMessage(e));
    }
  };

  const isPending = changeRole.isPending || changeCountry.isPending;

  return (
    <div className="space-y-6">
      {/* Info del usuario */}
      <div className="p-4 rounded-xl bg-white/[0.03] border border-white/[0.06]">
        <p className="text-white font-medium text-sm">{user.fullName}</p>
        <p className="text-zinc-500 text-xs mt-0.5">{user.email}</p>
        <div className="mt-2">
          <span className={`text-xs ${user.isEmailVerified ? 'text-emerald-400' : 'text-zinc-500'}`}>
            {user.isEmailVerified ? '✓ Email verificado' : '· Email no verificado'}
          </span>
        </div>
      </div>

      {/* Rol */}
      <div>
        <label className="block text-sm font-medium text-zinc-300 mb-1.5">Rol</label>
        <select
          value={role}
          onChange={(e) => setRole(e.target.value as UserRole)}
          className="w-full px-3 py-2.5 rounded-xl bg-white/[0.04] border border-white/[0.08] text-white text-sm focus:outline-none focus:border-indigo-500/60 transition-all"
        >
          {assignableRoles.map((r) => (
            <option key={r} value={r} className="bg-[#0d0d14]">{ROLES[r]}</option>
          ))}
        </select>
      </div>

      {/* País — solo Super Admin */}
      {isSuperAdmin && (
        <div>
          <label className="block text-sm font-medium text-zinc-300 mb-1.5">País</label>
          <select
            value={countryId}
            onChange={(e) => setCountryId(Number(e.target.value))}
            className="w-full px-3 py-2.5 rounded-xl bg-white/[0.04] border border-white/[0.08] text-white text-sm focus:outline-none focus:border-indigo-500/60 transition-all"
          >
            {countries.filter((c) => c.isActive).map((c) => (
              <option key={c.id} value={c.id} className="bg-[#0d0d14]">{c.name}</option>
            ))}
          </select>
        </div>
      )}

      {error && (
        <div className="px-4 py-3 rounded-xl bg-red-500/10 border border-red-500/20 text-red-400 text-sm">
          {error}
        </div>
      )}

      <button
        onClick={handleSave}
        disabled={isPending || (role === user.role && countryId === user.countryId)}
        className="w-full py-2.5 rounded-xl bg-indigo-600 hover:bg-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed text-white text-sm font-medium transition-colors"
      >
        {isPending ? 'Guardando...' : 'Guardar cambios'}
      </button>
    </div>
  );
}

// ── Role Badge ────────────────────────────────────────────────────────────────

function RoleBadge({ role }: { role: UserRole }) {
  const styles: Record<UserRole, string> = {
    superadmin: 'bg-purple-500/10 border-purple-500/20 text-purple-400',
    admin: 'bg-blue-500/10   border-blue-500/20   text-blue-400',
    instructor: 'bg-amber-500/10  border-amber-500/20  text-amber-400',
    student: 'bg-zinc-500/10   border-zinc-500/20   text-zinc-400',
  };

  return (
    <span className={`inline-flex px-2 py-0.5 rounded-full border text-xs font-medium ${styles[role]}`}>
      {ROLES[role]}
    </span>
  );
}
