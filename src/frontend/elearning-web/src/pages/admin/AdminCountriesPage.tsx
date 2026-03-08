import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useAdminCountries, useCreateCountry, useToggleCountryStatus } from '@/hooks/admin/countries';
import Drawer from '@/components/ui/Drawer';
import { getApiErrorMessage } from '../../lib/axios';
import type { Country } from '@/types/user.types';

const createCountrySchema = z.object({
  code: z
    .string()
    .length(3, 'El código debe tener exactamente 3 letras.')
    .regex(/^[a-zA-Z]+$/, 'Solo se permiten letras.'),
  name: z
    .string()
    .min(2, 'El nombre es requerido.')
    .max(100, 'Máximo 100 caracteres.'),
});

type CreateCountryForm = z.infer<typeof createCountrySchema>;

export default function AdminCountriesPage() {
  const [drawerMode, setDrawerMode] = useState<'create' | 'toggle' | null>(null);
  const [selected, setSelected] = useState<Country | null>(null);

  const { data: countries, isLoading } = useAdminCountries();

  const openCreate = () => { setSelected(null); setDrawerMode('create'); };
  const openToggle = (c: Country) => { setSelected(c); setDrawerMode('toggle'); };
  const closeDrawer = () => { setDrawerMode(null); setSelected(null); };

  const activeCount = countries?.filter((c) => c.isActive).length ?? 0;
  const inactiveCount = countries?.filter((c) => !c.isActive).length ?? 0;

  return (
    <div className="p-6 text-white">

      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-xl font-semibold text-white">Países</h1>
          <p className="text-sm text-zinc-500 mt-0.5">
            {activeCount} activos · {inactiveCount} inactivos
          </p>
        </div>
        <button
          onClick={openCreate}
          className="flex items-center gap-2 px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white text-sm font-medium transition-colors"
        >
          <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
          </svg>
          Nuevo país
        </button>
      </div>

      {/* Grid de países */}
      {isLoading ? (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
          {Array.from({ length: 6 }).map((_, i) => (
            <div key={i} className="h-20 rounded-xl bg-white/[0.04] animate-pulse" />
          ))}
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
          {countries?.map((country) => (
            <div
              key={country.id}
              className={`flex items-center justify-between p-4 rounded-xl border transition-all ${country.isActive
                ? 'bg-[#111118] border-white/[0.06]'
                : 'bg-[#0d0d14] border-white/[0.03] opacity-60'
                }`}
            >
              <div className="flex items-center gap-3">
                {/* Código */}
                <div className="w-10 h-10 rounded-lg bg-white/[0.04] border border-white/[0.06] flex items-center justify-center flex-shrink-0">
                  <span className="text-xs font-mono font-bold text-zinc-400">{country.code}</span>
                </div>
                <div>
                  <p className="text-sm font-medium text-white">{country.name}</p>
                  <span className={`text-xs ${country.isActive ? 'text-emerald-400' : 'text-zinc-600'}`}>
                    {country.isActive ? 'Activo' : 'Inactivo'}
                  </span>
                </div>
              </div>

              <button
                onClick={() => openToggle(country)}
                className="px-3 py-1.5 rounded-lg bg-white/[0.04] hover:bg-white/[0.08] text-zinc-400 hover:text-white text-xs transition-all"
              >
                {country.isActive ? 'Desactivar' : 'Activar'}
              </button>
            </div>
          ))}

          {countries?.length === 0 && (
            <div className="col-span-3 text-center py-12 text-zinc-500 text-sm">
              No hay países registrados. Crea el primero.
            </div>
          )}
        </div>
      )}

      {/* Drawer */}
      <Drawer
        open={drawerMode !== null}
        onClose={closeDrawer}
        title={drawerMode === 'create' ? 'Nuevo país' : 'Cambiar estado'}
      >
        {drawerMode === 'create' && (
          <CreateCountryForm onSuccess={closeDrawer} />
        )}
        {drawerMode === 'toggle' && selected && (
          <ToggleCountryForm country={selected} onSuccess={closeDrawer} />
        )}
      </Drawer>
    </div>
  );
}

// ── Create Country Form ───────────────────────────────────────────────────────

function CreateCountryForm({ onSuccess }: { onSuccess: () => void }) {
  const create = useCreateCountry();
  const [error, setError] = useState('');

  const { register, handleSubmit, formState: { errors } } = useForm<CreateCountryForm>({
    resolver: zodResolver(createCountrySchema),
  });

  const onSubmit = async (data: CreateCountryForm) => {
    setError('');
    try {
      await create.mutateAsync({
        code: data.code.toUpperCase(),
        name: data.name.trim(),
      });
      onSuccess();
    } catch (e) { setError(getApiErrorMessage(e)); }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
      <div>
        <label className="block text-sm font-medium text-zinc-300 mb-1.5">
          Código ISO (3 letras)
        </label>
        <input
          {...register('code')}
          placeholder="COL"
          maxLength={3}
          className="w-full px-4 py-2.5 rounded-xl bg-white/[0.04] border border-white/[0.08] text-white placeholder-zinc-600 text-sm font-mono uppercase focus:outline-none focus:border-indigo-500/60 transition-all"
        />
        {errors.code && <p className="mt-1.5 text-xs text-red-400">{errors.code.message}</p>}
        <p className="mt-1 text-xs text-zinc-600">Estándar ISO 3166-1 alpha-3. Ej: COL, MEX, ARG, PER</p>
      </div>

      <div>
        <label className="block text-sm font-medium text-zinc-300 mb-1.5">Nombre del país</label>
        <input
          {...register('name')}
          placeholder="Colombia"
          className="w-full px-4 py-2.5 rounded-xl bg-white/[0.04] border border-white/[0.08] text-white placeholder-zinc-600 text-sm focus:outline-none focus:border-indigo-500/60 transition-all"
        />
        {errors.name && <p className="mt-1.5 text-xs text-red-400">{errors.name.message}</p>}
      </div>

      {error && (
        <div className="px-4 py-3 rounded-xl bg-red-500/10 border border-red-500/20 text-red-400 text-sm">
          {error}
        </div>
      )}

      <button
        type="submit"
        disabled={create.isPending}
        className="w-full py-2.5 rounded-xl bg-indigo-600 hover:bg-indigo-500 disabled:opacity-50 text-white text-sm font-medium transition-colors"
      >
        {create.isPending ? 'Creando...' : 'Crear país'}
      </button>
    </form>
  );
}

// ── Toggle Country Form ───────────────────────────────────────────────────────

function ToggleCountryForm({ country, onSuccess }: { country: Country; onSuccess: () => void }) {
  const toggle = useToggleCountryStatus();
  const [error, setError] = useState('');

  const handleToggle = async () => {
    setError('');
    try {
      await toggle.mutateAsync(country.id);
      onSuccess();
    } catch (e) { setError(getApiErrorMessage(e)); }
  };

  return (
    <div className="space-y-6">
      <div className="p-4 rounded-xl bg-white/[0.03] border border-white/[0.06]">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-lg bg-white/[0.04] border border-white/[0.06] flex items-center justify-center">
            <span className="text-xs font-mono font-bold text-zinc-400">{country.code}</span>
          </div>
          <div>
            <p className="text-white font-medium text-sm">{country.name}</p>
            <span className={`text-xs ${country.isActive ? 'text-emerald-400' : 'text-zinc-500'}`}>
              Actualmente {country.isActive ? 'activo' : 'inactivo'}
            </span>
          </div>
        </div>
      </div>

      <div className={`p-4 rounded-xl border ${country.isActive
        ? 'bg-amber-500/5 border-amber-500/20'
        : 'bg-emerald-500/5 border-emerald-500/20'
        }`}>
        <p className={`text-sm ${country.isActive ? 'text-amber-300' : 'text-emerald-300'}`}>
          {country.isActive
            ? 'Al desactivar este país, los usuarios de ese país no podrán ver los cursos asignados a él.'
            : 'Al activar este país, sus usuarios podrán acceder a los cursos asignados.'
          }
        </p>
      </div>

      {error && (
        <div className="px-4 py-3 rounded-xl bg-red-500/10 border border-red-500/20 text-red-400 text-sm">
          {error}
        </div>
      )}

      <button
        onClick={handleToggle}
        disabled={toggle.isPending}
        className={`w-full py-2.5 rounded-xl text-white text-sm font-medium transition-colors disabled:opacity-50 ${country.isActive
          ? 'bg-amber-600 hover:bg-amber-500'
          : 'bg-emerald-600 hover:bg-emerald-500'
          }`}
      >
        {toggle.isPending
          ? 'Procesando...'
          : country.isActive ? 'Desactivar país' : 'Activar país'
        }
      </button>
    </div>
  );
}
