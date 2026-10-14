import { useState, type ReactNode } from 'react';
import { Plus, Trash2 } from 'lucide-react';
import {
  Alert, Avatar, Badge, Button, Card, CardDescription, CardHeader, CardTitle, Checkbox,
  Drawer, Field, Input, Modal, Pagination, ProgressBar, RadioCard, SearchInput, SegmentedTabs, Select,
  Skeleton, Spinner, StatCard, StateBlock, Switch, Table, TableBody, TableCell, TableHead, TableHeader,
  TableRow, Textarea,
} from '@/components/ui';
import { BarChartIcon, GraduationCapIcon } from '@/components/ui/icons';
import { CourseCard } from '@/components/courses/CourseCard';
import { LessonRow } from '@/components/courses/LessonRow';

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
  const [tab, setTab] = useState<'todos' | 'activo' | 'completado'>('todos');
  const [page, setPage] = useState(4);
  const [scope, setScope] = useState('global');
  const [required, setRequired] = useState(true);
  const [active, setActive] = useState(false);

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

        <Section title="Estados de vista">
          <div className="grid gap-4 md:grid-cols-2">
            <StateBlock
              icon={GraduationCapIcon}
              title="Aún no estás inscrito en ningún curso"
              text="Explora el catálogo e inscríbete para empezar a aprender."
              action="Explorar cursos"
              compact
            />
            <StateBlock tipo="error" title="No pudimos cargar tu dashboard" compact />
          </div>
          <div className="flex flex-col gap-3" aria-busy="true">
            <Skeleton className="h-4 w-48" />
            <Skeleton className="h-8 w-64" />
            <Skeleton className="h-24 rounded-2xl" />
          </div>
          <div className="flex items-center gap-4">
            <Spinner size="sm" />
            <Spinner />
            <Spinner size="lg" />
          </div>
        </Section>

        <Section title="Alertas">
          <div className="max-w-xl space-y-3">
            <Alert tone="success" title="Cuenta creada">Revisa tu correo para verificar tu email.</Alert>
            <Alert tone="danger">Email o contraseña incorrectos.</Alert>
            <Alert tone="warning">Tu email aún no está verificado.</Alert>
            <Alert>Te enviaremos un enlace para restablecer tu contraseña.</Alert>
          </div>
        </Section>

        <Section title="Avatar y pestañas">
          <div className="flex items-center gap-3">
            <Avatar name="Ana Gómez" size="xs" />
            <Avatar name="Ana Gómez" />
            <Avatar name="Carlos Rivas" size="md" tone="neutral" />
            <Avatar name="Ana Gómez" size="lg" />
          </div>
          <SegmentedTabs
            aria-label="Estado del curso"
            value={tab}
            onChange={setTab}
            options={[
              { value: 'todos', label: 'Todos', count: 4 },
              { value: 'activo', label: 'Activos', count: 2 },
              { value: 'completado', label: 'Completados', count: 1 },
            ]}
          />
        </Section>

        <Section title="Tabla y paginación">
          <div className="max-w-sm">
            <SearchInput aria-label="Buscar usuarios" placeholder="Buscar por nombre o email" />
          </div>
          <Card padding="none" className="overflow-hidden">
            <Table>
              <TableHead>
                <TableRow>
                  <TableHeader>Usuario</TableHeader>
                  <TableHeader>Rol</TableHeader>
                  <TableHeader>Estado</TableHeader>
                </TableRow>
              </TableHead>
              <TableBody>
                {[
                  ['Ana Gómez', 'Estudiante', true],
                  ['Carlos Rivas', 'Instructor', true],
                  ['Luisa Pérez', 'Estudiante', false],
                ].map(([name, role, verified]) => (
                  <TableRow key={name as string}>
                    <TableCell>
                      <span className="flex items-center gap-3 text-foreground">
                        <Avatar name={name as string} tone="neutral" />
                        {name}
                      </span>
                    </TableCell>
                    <TableCell>
                      <Badge tone={role === 'Instructor' ? 'info' : 'neutral'}>{role}</Badge>
                    </TableCell>
                    <TableCell>
                      <Badge tone={verified ? 'success' : 'warning'}>{verified ? 'Verificado' : 'Sin verificar'}</Badge>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
            <Pagination
              className="border-t border-border px-6 py-3"
              page={page}
              totalPages={12}
              onPageChange={setPage}
              summary={`Página ${page} de 12`}
            />
          </Card>
        </Section>

        <Section title="Selección">
          <Card className="max-w-xl space-y-5">
            <div role="radiogroup" aria-label="Alcance del curso" className="grid gap-3 sm:grid-cols-2">
              <RadioCard
                name="scope"
                value="global"
                checked={scope === 'global'}
                onChange={() => setScope('global')}
                label="Global"
                description="Visible en todos los países."
              />
              <RadioCard
                name="scope"
                value="pais"
                checked={scope === 'pais'}
                onChange={() => setScope('pais')}
                label="Por país"
                description="Solo en los países que elijas."
              />
            </div>
            <div className="space-y-2">
              <RadioCard name="review" tone="success" defaultChecked label="BUSCARX" trailing={<Badge tone="success">Correcta</Badge>} />
              <RadioCard name="review" tone="danger" label="CONSULTAV" trailing={<Badge tone="danger">Tu respuesta</Badge>} />
            </div>
            <div className="flex flex-col gap-3">
              <Checkbox label="Lección requerida" description="Cuenta para completar el curso." defaultChecked />
              <Checkbox label="Colombia" />
              <Checkbox label="Deshabilitado" disabled />
            </div>
            <Switch checked={required} onCheckedChange={setRequired} label="Pregunta requerida" />
            <Switch checked={active} onCheckedChange={setActive} aria-label="País activo" />
          </Card>
        </Section>

        <Section title="Estadísticas">
          <div className="grid gap-4 md:grid-cols-3">
            <StatCard label="Puntaje" value="90%" tone="success" hint="Mínimo para aprobar: 70%" />
            <StatCard label="Correctas" value="9 / 10" />
            <StatCard label="Ranking por país" icon={BarChartIcon} hint="Próximamente" />
          </div>
        </Section>

        <Section title="Cursos (dominio)">
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            <CourseCard
              to="/design-system"
              title="Excel Avanzado"
              description="Tablas dinámicas, funciones de búsqueda y automatización con macros."
              thumbnailUrl={null}
              isGlobal
              instructorName="Carlos Rivas"
              lessonCount={9}
              badges={<Badge tone="info">Inscrito</Badge>}
              date="3 mar 2026"
            />
            <CourseCard
              to="/design-system"
              title="Seguridad de la información"
              description="Buenas prácticas para proteger los datos de la empresa."
              thumbnailUrl={null}
              isGlobal={false}
              instructorName="Luisa Pérez"
              lessonCount={1}
            />
          </div>
          <Card padding="none" className="divide-y divide-border overflow-hidden">
            <LessonRow index={1} title="Bienvenida y objetivos" type="video" isRequired status="done" onClick={() => {}} />
            <LessonRow index={2} title="Guía de atajos de teclado" type="pdf" isRequired={false} status="pending" current onClick={() => {}} />
            <LessonRow
              index={3}
              title="Quiz: funciones lógicas"
              type="quiz"
              isRequired
              status="locked"
              lockReason="Se desbloquea al completar las lecciones requeridas anteriores."
            />
          </Card>
          <p className="text-xs text-subtle-foreground">Las tarjetas y filas de curso viven en components/courses (no se publican en ElearningUI).</p>
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
