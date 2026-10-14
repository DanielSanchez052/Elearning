import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useAdminCountries, useCreateCountry, useToggleCountryStatus } from '@/hooks/admin/countries';
import {
  Alert,
  Badge,
  Button,
  Card,
  Field,
  Input,
  Modal,
  Skeleton,
  StateBlock,
  Switch,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui';
import { GlobeIcon, PlusIcon } from '@/components/ui/icons';
import { getApiErrorMessage } from '../../lib/axios';

const createCountrySchema = z.object({
  name: z.string().trim().min(2, 'El nombre es requerido.').max(100, 'Máximo 100 caracteres.'),
  code: z
    .string()
    .length(3, 'El código debe tener exactamente 3 letras.')
    .regex(/^[a-zA-Z]+$/, 'Solo se permiten letras.'),
});

type CreateCountryForm = z.infer<typeof createCountrySchema>;

export default function AdminCountriesPage() {
  const [createOpen, setCreateOpen] = useState(false);
  const { data: countries, isLoading, isError, refetch } = useAdminCountries();
  const toggle = useToggleCountryStatus();
  const [toggleError, setToggleError] = useState('');

  const activeCount = countries?.filter((c) => c.isActive).length ?? 0;
  const inactiveCount = (countries?.length ?? 0) - activeCount;

  const handleToggle = async (id: number) => {
    setToggleError('');
    try {
      await toggle.mutateAsync(id);
    } catch (e) {
      setToggleError(getApiErrorMessage(e));
    }
  };

  return (
    <div className="flex flex-col gap-6 px-4 py-8 sm:px-8">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-foreground">Países</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            {countries && `${activeCount} ${activeCount === 1 ? 'activo' : 'activos'} · ${inactiveCount} ${inactiveCount === 1 ? 'inactivo' : 'inactivos'}. `}
            Los países inactivos no aparecen en el registro ni en la asignación de cursos.
          </p>
        </div>
        <Button onClick={() => setCreateOpen(true)}>
          <PlusIcon /> Nuevo país
        </Button>
      </div>

      {toggleError && <Alert tone="danger" className="max-w-3xl">{toggleError}</Alert>}

      <Card padding="none" className="max-w-3xl overflow-hidden">
        {isError ? (
          <StateBlock bordered={false} tipo="error" title="No pudimos cargar los países" onAction={() => refetch()} />
        ) : !isLoading && (countries ?? []).length === 0 ? (
          <StateBlock
            bordered={false}
            icon={GlobeIcon}
            title="Aún no hay países"
            text="Agrega el primer país para que los usuarios puedan registrarse y recibir cursos asignados."
            action="Nuevo país"
            onAction={() => setCreateOpen(true)}
          />
        ) : (
          <Table aria-busy={isLoading || undefined}>
            <TableHead>
              <TableRow>
                <TableHeader>Código ISO</TableHeader>
                <TableHeader>Nombre</TableHeader>
                <TableHeader>Estado</TableHeader>
                <TableHeader className="text-right">Activo</TableHeader>
              </TableRow>
            </TableHead>
            <TableBody>
              {isLoading
                ? Array.from({ length: 4 }, (_, i) => (
                    <TableRow key={i}>
                      <TableCell><Skeleton className="h-5 w-12" /></TableCell>
                      <TableCell><Skeleton className="h-3 w-40" /></TableCell>
                      <TableCell><Skeleton className="h-5 w-16 rounded-full" /></TableCell>
                      <TableCell>
                        <div className="flex justify-end">
                          <Skeleton className="h-5 w-9 rounded-full" />
                        </div>
                      </TableCell>
                    </TableRow>
                  ))
                : countries?.map((country) => (
                    <TableRow key={country.id} className="hover:bg-secondary">
                      <TableCell className="py-3">
                        <span className="rounded-md bg-secondary px-2 py-1 font-mono text-xs text-foreground-soft">{country.code}</span>
                      </TableCell>
                      <TableCell className="py-3 font-medium text-foreground">{country.name}</TableCell>
                      <TableCell className="py-3">
                        <Badge tone={country.isActive ? 'success' : 'danger'}>{country.isActive ? 'Activo' : 'Inactivo'}</Badge>
                      </TableCell>
                      <TableCell className="py-3">
                        <div className="flex justify-end">
                          <Switch
                            checked={country.isActive}
                            onCheckedChange={() => handleToggle(country.id)}
                            disabled={toggle.isPending && toggle.variables === country.id}
                            aria-label={`${country.isActive ? 'Desactivar' : 'Activar'} ${country.name}`}
                          />
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
            </TableBody>
          </Table>
        )}
      </Card>

      {createOpen && <CreateCountryModal onClose={() => setCreateOpen(false)} />}
    </div>
  );
}

// ── Create Country Modal ──────────────────────────────────────────────────────

function CreateCountryModal({ onClose }: { onClose: () => void }) {
  const create = useCreateCountry();
  const [error, setError] = useState('');

  const { register, handleSubmit, formState: { errors } } = useForm<CreateCountryForm>({
    resolver: zodResolver(createCountrySchema),
  });

  const onSubmit = async (data: CreateCountryForm) => {
    setError('');
    try {
      await create.mutateAsync({ code: data.code.toUpperCase(), name: data.name.trim() });
      onClose();
    } catch (e) {
      setError(getApiErrorMessage(e));
    }
  };

  return (
    <Modal
      open
      onClose={onClose}
      title="Nuevo país"
      description="Los cursos asignados a este país serán visibles para sus usuarios."
      footer={
        <>
          <Button variant="secondary" onClick={onClose}>
            Cancelar
          </Button>
          <Button type="submit" form="create-country-form" disabled={create.isPending}>
            {create.isPending ? 'Creando…' : 'Crear país'}
          </Button>
        </>
      }
    >
      <form id="create-country-form" onSubmit={handleSubmit(onSubmit)} noValidate className="flex flex-col gap-4">
        <Field label="Nombre" error={errors.name?.message}>
          <Input {...register('name')} placeholder="Ej. Ecuador" />
        </Field>
        <Field label="Código ISO" hint="3 letras (ISO 3166-1 alpha-3), p. ej. COL, MEX." error={errors.code?.message}>
          <Input {...register('code')} placeholder="ECU" maxLength={3} className="font-mono uppercase" />
        </Field>
        {error && <Alert tone="danger">{error}</Alert>}
      </form>
    </Modal>
  );
}
