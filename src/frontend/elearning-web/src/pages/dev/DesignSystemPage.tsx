import { useState, type ReactNode } from 'react';
import { Plus, Trash2 } from 'lucide-react';
import {
  Badge, Button, Card, CardDescription, CardHeader, CardTitle,
  Drawer, Field, Input, Modal, ProgressBar, Select, Textarea,
} from '@/components/ui';

/**
 * Catálogo visual del sistema de diseño (solo en desarrollo: ruta /design-system).
 * Sirve para revisar tokens y componentes antes de migrar las páginas.
 */

const surfaces = [
  { name: 'sunken', className: 'bg-sunken' },
  { name: 'background', className: 'bg-background' },
  { name: 'sidebar', className: 'bg-sidebar' },
  { name: 'card', className: 'bg-card' },
  { name: 'secondary', className: 'bg-secondary' },
  { name: 'secondary-hover', className: 'bg-secondary-hover' },
];

const colors = [
  { name: 'primary', className: 'bg-primary' },
  { name: 'brand', className: 'bg-brand' },
  { name: 'success', className: 'bg-success' },
  { name: 'warning', className: 'bg-warning' },
  { name: 'info', className: 'bg-info' },
  { name: 'destructive', className: 'bg-destructive' },
];

const texts = [
  { name: 'foreground', className: 'text-foreground' },
  { name: 'foreground-soft', className: 'text-foreground-soft' },
  { name: 'muted-foreground', className: 'text-muted-foreground' },
  { name: 'subtle-foreground', className: 'text-subtle-foreground' },
  { name: 'placeholder', className: 'text-placeholder' },
  { name: 'brand-foreground', className: 'text-brand-foreground' },
];

function Section({ title, children }: { title: string; children: ReactNode }) {
  return (
    <section className="space-y-4">
      <h2 className="text-xs font-semibold uppercase tracking-wider text-subtle-foreground">{title}</h2>
      {children}
    </section>
  );
}

export default function DesignSystemPage() {
  const [modalOpen, setModalOpen] = useState(false);
  const [drawerOpen, setDrawerOpen] = useState(false);

  return (
    <div className="min-h-screen bg-background text-foreground">
      <div className="mx-auto max-w-5xl space-y-12 px-6 py-10">
        <header>
          <h1 className="text-2xl font-semibold">Sistema de diseño</h1>
          <p className="mt-1 text-sm text-muted-foreground">Tokens y componentes base de ELearning.</p>
        </header>

        <Section title="Superficies">
          <div className="grid grid-cols-2 gap-3 sm:grid-cols-3">
            {surfaces.map((s) => (
              <div key={s.name} className={`${s.className} rounded-xl border border-border p-4`}>
                <p className="text-sm text-foreground">{s.name}</p>
              </div>
            ))}
          </div>
        </Section>

        <Section title="Color">
          <div className="grid grid-cols-3 gap-3 sm:grid-cols-6">
            {colors.map((c) => (
              <div key={c.name} className="space-y-2">
                <div className={`${c.className} h-12 rounded-xl`} />
                <p className="text-xs text-muted-foreground">{c.name}</p>
              </div>
            ))}
          </div>
        </Section>

        <Section title="Texto">
          <div className="space-y-1">
            {texts.map((t) => (
              <p key={t.name} className={`${t.className} text-sm`}>{t.name} — El curso empieza cuando estés listo.</p>
            ))}
          </div>
        </Section>

        <Section title="Botones">
          <div className="flex flex-wrap items-center gap-3">
            <Button>Guardar curso</Button>
            <Button variant="secondary">Cancelar</Button>
            <Button variant="outline">Vista previa</Button>
            <Button variant="ghost">Ver todos</Button>
            <Button variant="soft"><Plus /> Agregar lección</Button>
            <Button variant="success">Marcar completada</Button>
            <Button variant="danger"><Trash2 /> Eliminar</Button>
            <Button variant="destructive">Confirmar eliminación</Button>
            <Button disabled>Deshabilitado</Button>
          </div>
          <div className="flex flex-wrap items-center gap-3">
            <Button size="sm">Pequeño</Button>
            <Button size="md">Mediano</Button>
            <Button size="lg">Grande</Button>
            <Button size="icon" variant="secondary" aria-label="Agregar"><Plus /></Button>
          </div>
        </Section>

        <Section title="Insignias">
          <div className="flex flex-wrap gap-2">
            <Badge>Borrador</Badge>
            <Badge tone="brand">Global</Badge>
            <Badge tone="success">Inscrito</Badge>
            <Badge tone="warning">Pendiente</Badge>
            <Badge tone="info">Nuevo</Badge>
            <Badge tone="danger">Inactivo</Badge>
          </div>
        </Section>

        <Section title="Progreso">
          <div className="max-w-md space-y-4">
            <ProgressBar value={35} label="Subiendo video" showValue size="sm" />
            <ProgressBar value={72} label="3 / 4 lecciones requeridas" showValue tone="success" />
            <ProgressBar value={40} tone="danger" />
          </div>
        </Section>

        <Section title="Formularios">
          <Card className="max-w-xl space-y-4">
            <Field label="Título del curso" hint="Visible en el catálogo.">
              <Input placeholder="Excel Avanzado" />
            </Field>
            <Field label="País">
              <Select defaultValue="co">
                <option value="co">Colombia</option>
                <option value="mx">México</option>
              </Select>
            </Field>
            <Field label="Descripción" error="La descripción es obligatoria.">
              <Textarea rows={3} />
            </Field>
          </Card>
        </Section>

        <Section title="Tarjetas">
          <div className="grid gap-4 sm:grid-cols-3">
            <Card>
              <CardHeader>
                <CardTitle>Default</CardTitle>
                <CardDescription>Panel principal.</CardDescription>
              </CardHeader>
            </Card>
            <Card variant="subtle">
              <CardHeader>
                <CardTitle>Subtle</CardTitle>
                <CardDescription>Elemento dentro de un panel.</CardDescription>
              </CardHeader>
            </Card>
            <Card variant="interactive">
              <CardHeader>
                <CardTitle>Interactive</CardTitle>
                <CardDescription>Pasa el cursor por encima.</CardDescription>
              </CardHeader>
            </Card>
          </div>
        </Section>

        <Section title="Diálogos">
          <div className="flex gap-3">
            <Button variant="outline" onClick={() => setModalOpen(true)}>Abrir modal</Button>
            <Button variant="outline" onClick={() => setDrawerOpen(true)}>Abrir drawer</Button>
          </div>
        </Section>
      </div>

      <Modal
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        title="Eliminar lección"
        description="Esta acción no se puede deshacer."
        size="sm"
        footer={
          <>
            <Button variant="secondary" onClick={() => setModalOpen(false)}>Cancelar</Button>
            <Button variant="destructive" onClick={() => setModalOpen(false)}>Eliminar</Button>
          </>
        }
      >
        <p className="text-sm text-foreground-soft">Se eliminará "Introducción" y el progreso asociado.</p>
      </Modal>

      <Drawer open={drawerOpen} onClose={() => setDrawerOpen(false)} title="Detalle del usuario">
        <p className="text-sm text-foreground-soft">Contenido del drawer.</p>
      </Drawer>
    </div>
  );
}
