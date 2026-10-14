import { useEffect, useState } from 'react';
import { useAdminUsers, useChangeUserRole, useChangeUserCountry } from '@/hooks/admin/users';
import { useAdminCountries } from '@/hooks/admin/countries';
import { useAuthStore } from '../../store/authStore';
import {
  Alert,
  Avatar,
  Badge,
  Button,
  Card,
  Drawer,
  Field,
  Pagination,
  SearchInput,
  Select,
  Skeleton,
  StateBlock,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui';
import { CheckIcon, MailIcon, PencilIcon, UsersIcon } from '@/components/ui/icons';
import { getApiErrorMessage } from '../../lib/axios';
import { formatDate } from '@/lib/format';
import { roleTone } from '@/lib/roles';
import type { Country, UserSummaryDto, UserRole } from '@/types/user.types';
import { ROLES } from '@/types/user.types';

const PAGE_SIZE = 20;

function VerifiedBadge({ verified }: { verified: boolean }) {
  return verified ? (
    <Badge tone="success">
      <CheckIcon /> Verificado
    </Badge>
  ) : (
    <Badge tone="warning">
      <MailIcon /> Sin verificar
    </Badge>
  );
}

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

  // Busca mientras se escribe, con una pausa corta.
  useEffect(() => {
    const t = setTimeout(() => {
      setQuery(search.trim());
      setPage(1);
    }, 300);
    return () => clearTimeout(t);
  }, [search]);

  const { data, isLoading, isError, refetch, isPlaceholderData } = useAdminUsers({
    search: query || undefined,
    role: role || undefined,
    countryId,
    isEmailVerified: verified,
    page,
    pageSize: PAGE_SIZE,
  });

  const { data: countries } = useAdminCountries();
  const users = data?.items ?? [];
  const total = data?.totalCount ?? 0;
  const hasFilters = Boolean(query || role || countryId || verified !== undefined);

  const resetFilters = () => {
    setSearch('');
    setQuery('');
    setRole('');
    setCountryId(undefined);
    setVerified(undefined);
    setPage(1);
  };

  return (
    <div className="flex flex-col gap-6 px-4 py-8 sm:px-8">
      <div>
        <h1 className="text-2xl font-semibold text-foreground">Usuarios</h1>
        <p className="mt-1 text-sm text-muted-foreground">
          {data ? `${total} ${total === 1 ? 'usuario' : 'usuarios'}` : 'Usuarios'}
          {isSuperAdmin ? ' en todos los países' : user?.country ? ` de ${user.country}` : ''}
        </p>
      </div>

      <div className="flex flex-wrap items-center gap-3">
        <SearchInput
          className="min-w-64 flex-1"
          aria-label="Buscar por nombre o email"
          placeholder="Buscar por nombre o email"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <Select
          aria-label="Rol"
          className="w-full sm:w-48"
          value={role}
          onChange={(e) => {
            setRole(e.target.value);
            setPage(1);
          }}
        >
          <option value="">Todos los roles</option>
          {Object.entries(ROLES).map(([k, v]) => (
            <option key={k} value={k}>
              {v}
            </option>
          ))}
        </Select>
        {isSuperAdmin && countries && (
          <Select
            aria-label="País"
            className="w-full sm:w-48"
            value={countryId ?? ''}
            onChange={(e) => {
              setCountryId(e.target.value ? Number(e.target.value) : undefined);
              setPage(1);
            }}
          >
            <option value="">Todos los países</option>
            {countries.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </Select>
        )}
        <Select
          aria-label="Verificación"
          className="w-full sm:w-56"
          value={verified === undefined ? '' : String(verified)}
          onChange={(e) => {
            setVerified(e.target.value === '' ? undefined : e.target.value === 'true');
            setPage(1);
          }}
        >
          <option value="">Verificados y sin verificar</option>
          <option value="true">Verificados</option>
          <option value="false">Sin verificar</option>
        </Select>
        {hasFilters && (
          <Button variant="ghost" size="sm" onClick={resetFilters}>
            Limpiar filtros
          </Button>
        )}
      </div>

      <Card padding="none" className="overflow-hidden">
        {isError ? (
          <StateBlock bordered={false} tipo="error" title="No pudimos cargar los usuarios" onAction={() => refetch()} />
        ) : !isLoading && users.length === 0 ? (
          <StateBlock
            bordered={false}
            icon={UsersIcon}
            title={hasFilters ? 'Ningún usuario coincide con los filtros' : 'Todavía no hay usuarios'}
            text={hasFilters ? 'Prueba con otro nombre o email, o quita algún filtro.' : undefined}
            action={hasFilters ? 'Limpiar filtros' : undefined}
            actionVariant="secondary"
            onAction={resetFilters}
          />
        ) : (
          <>
            <Table className={isPlaceholderData ? 'opacity-60' : undefined} aria-busy={isLoading || undefined}>
              <TableHead>
                <TableRow>
                  <TableHeader>Usuario</TableHeader>
                  <TableHeader>Rol</TableHeader>
                  <TableHeader>País</TableHeader>
                  <TableHeader>Verificado</TableHeader>
                  <TableHeader>Registro</TableHeader>
                  <TableHeader className="text-right">
                    <span className="sr-only">Acciones</span>
                  </TableHeader>
                </TableRow>
              </TableHead>
              <TableBody>
                {isLoading
                  ? Array.from({ length: 6 }, (_, i) => (
                      <TableRow key={i}>
                        <TableCell>
                          <div className="flex items-center gap-3">
                            <Skeleton className="size-8 rounded-full" />
                            <div className="space-y-1.5">
                              <Skeleton className="h-3 w-40" />
                              <Skeleton className="h-3 w-48" />
                            </div>
                          </div>
                        </TableCell>
                        <TableCell><Skeleton className="h-5 w-20 rounded-full" /></TableCell>
                        <TableCell><Skeleton className="h-3 w-20" /></TableCell>
                        <TableCell><Skeleton className="h-5 w-24 rounded-full" /></TableCell>
                        <TableCell><Skeleton className="h-3 w-20" /></TableCell>
                        <TableCell />
                      </TableRow>
                    ))
                  : users.map((u) => (
                      <TableRow key={u.id} className="hover:bg-secondary">
                        <TableCell className="py-3">
                          <div className="flex items-center gap-3">
                            <Avatar name={u.fullName} tone="neutral" />
                            <div className="min-w-0">
                              <p className="truncate font-medium text-foreground">{u.fullName}</p>
                              <p className="truncate text-xs text-muted-foreground">{u.email}</p>
                            </div>
                          </div>
                        </TableCell>
                        <TableCell className="py-3">
                          <Badge tone={roleTone[u.role]}>{ROLES[u.role] ?? u.role}</Badge>
                        </TableCell>
                        <TableCell className="py-3">{u.country}</TableCell>
                        <TableCell className="py-3">
                          <VerifiedBadge verified={u.isEmailVerified} />
                        </TableCell>
                        <TableCell className="py-3 text-muted-foreground tabular-nums whitespace-nowrap">{formatDate(u.createdAt)}</TableCell>
                        <TableCell className="py-3 text-right">
                          <Button variant="ghost" size="sm" onClick={() => setSelected(u)} aria-label={`Editar a ${u.fullName}`}>
                            <PencilIcon /> Editar
                          </Button>
                        </TableCell>
                      </TableRow>
                    ))}
              </TableBody>
            </Table>
            {data && (
              <Pagination
                className="border-t border-border px-6 py-3"
                page={page}
                totalPages={data.totalPages}
                onPageChange={setPage}
                summary={`Mostrando ${(page - 1) * PAGE_SIZE + 1}–${Math.min(page * PAGE_SIZE, total)} de ${total}`}
              />
            )}
          </>
        )}
      </Card>

      <Drawer open={!!selected} onClose={() => setSelected(null)} title="Editar usuario">
        {selected && (
          <UserEditForm
            key={selected.id}
            user={selected}
            countries={countries ?? []}
            isSuperAdmin={isSuperAdmin}
            onDone={() => setSelected(null)}
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
  onDone,
}: {
  user: UserSummaryDto;
  countries: Country[];
  isSuperAdmin: boolean;
  onDone: () => void;
}) {
  const changeRole = useChangeUserRole();
  const changeCountry = useChangeUserCountry();

  const [role, setRole] = useState(user.role);
  const [countryId, setCountryId] = useState(user.countryId);
  const [error, setError] = useState('');

  // Roles que puede asignar según el rol del editor.
  const assignableRoles: UserRole[] = isSuperAdmin
    ? ['student', 'instructor', 'admin', 'superadmin']
    : ['student', 'instructor'];

  const handleSave = async () => {
    setError('');
    try {
      if (role !== user.role) await changeRole.mutateAsync({ id: user.id, role });
      if (isSuperAdmin && countryId !== user.countryId) await changeCountry.mutateAsync({ id: user.id, countryId });
      onDone();
    } catch (e) {
      setError(getApiErrorMessage(e));
    }
  };

  const isPending = changeRole.isPending || changeCountry.isPending;
  const unchanged = role === user.role && countryId === user.countryId;

  return (
    <div className="flex flex-col gap-5">
      <div className="flex items-center gap-3">
        <Avatar name={user.fullName} size="md" />
        <div className="min-w-0">
          <p className="text-base font-semibold text-foreground">{user.fullName}</p>
          <p className="truncate text-sm text-muted-foreground">{user.email}</p>
        </div>
      </div>
      <div className="flex flex-wrap gap-2">
        <Badge tone={roleTone[user.role]}>{ROLES[user.role]}</Badge>
        <Badge>{user.country}</Badge>
        <VerifiedBadge verified={user.isEmailVerified} />
      </div>
      <div className="border-t border-border" />

      <Field label="Rol" hint="El rol define a qué secciones del panel puede acceder.">
        <Select value={role} onChange={(e) => setRole(e.target.value as UserRole)}>
          {/* Si el rol actual no es asignable por este editor, se muestra igual para no perderlo. */}
          {!assignableRoles.includes(user.role) && <option value={user.role}>{ROLES[user.role]}</option>}
          {assignableRoles.map((r) => (
            <option key={r} value={r}>
              {ROLES[r]}
            </option>
          ))}
        </Select>
      </Field>

      <Field
        label="País"
        hint={isSuperAdmin ? 'El usuario verá los cursos globales y los de este país.' : 'Solo un superadmin puede cambiar el país.'}
      >
        <Select value={countryId} onChange={(e) => setCountryId(Number(e.target.value))} disabled={!isSuperAdmin}>
          {!countries.some((c) => c.id === user.countryId) && <option value={user.countryId}>{user.country}</option>}
          {countries
            .filter((c) => c.isActive || c.id === user.countryId)
            .map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
        </Select>
      </Field>

      {error && <Alert tone="danger">{error}</Alert>}

      <div className="flex gap-2 pt-2">
        <Button variant="secondary" onClick={onDone}>
          Cancelar
        </Button>
        <Button className="flex-1" onClick={handleSave} disabled={isPending || unchanged}>
          {isPending ? 'Guardando…' : 'Guardar cambios'}
        </Button>
      </div>
    </div>
  );
}
