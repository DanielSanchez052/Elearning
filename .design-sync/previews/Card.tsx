import { Card, CardHeader, CardTitle, CardDescription, Badge, Button, ProgressBar, BookOpenIcon } from '@elearning/ui';

export const Default = () => (
  <Card className="w-96">
    <CardHeader>
      <CardTitle>Tu progreso</CardTitle>
      <CardDescription>Excel Avanzado · 4 lecciones requeridas</CardDescription>
    </CardHeader>
    <ProgressBar value={75} label="3 / 4 lecciones completadas" showValue tone="success" />
  </Card>
);

export const CourseCard = () => (
  <Card variant="interactive" padding="none" className="w-80 overflow-hidden">
    <div className="flex h-32 items-center justify-center bg-brand/20">
      <BookOpenIcon className="size-10 text-brand-foreground" />
    </div>
    <div className="flex flex-col gap-3 p-5">
      <div className="flex items-center justify-between gap-2">
        <h3 className="text-base font-semibold text-foreground">Excel Avanzado</h3>
        <Badge tone="brand">Global</Badge>
      </div>
      <p className="text-sm text-muted-foreground">Tablas dinámicas, BUSCARX y macros para analizar datos.</p>
      <p className="text-xs text-subtle-foreground">Laura Méndez · 8 lecciones</p>
    </div>
  </Card>
);

export const Subtle = () => (
  <Card padding="sm" className="w-96">
    <div className="flex flex-col gap-2">
      <Card variant="subtle" padding="sm">
        <p className="text-sm text-foreground-soft">1. Introducción a las tablas dinámicas</p>
      </Card>
      <Card variant="subtle" padding="sm">
        <p className="text-sm text-foreground-soft">2. Funciones BUSCARX y FILTRAR</p>
      </Card>
    </div>
  </Card>
);

export const Elevated = () => (
  <Card padding="lg" elevated className="w-96">
    <CardHeader>
      <CardTitle>Verifica tu correo</CardTitle>
      <CardDescription>Te enviamos un enlace a ana.gomez@ejemplo.com.</CardDescription>
    </CardHeader>
    <Button fullWidth>Reenviar correo</Button>
  </Card>
);
