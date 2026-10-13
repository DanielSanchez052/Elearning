# ElearningUI — convenciones

Plataforma de e-learning multipaís con tema **solo oscuro**. Todo el texto de la interfaz va en **español**.

## Envoltura obligatoria

Envuelve cada pantalla en `AppRoot`: pone el fondo de página (`bg-background`) y el color de texto (`text-foreground`). Sin `AppRoot`, los componentes oscuros quedan sobre el fondo blanco del documento y el texto blanco no se ve. (La envoltura `rounded-xl p-6` del README es solo de las vistas previas.)

```jsx
const { AppRoot } = window.ElearningUI;
<AppRoot className="min-h-screen">{/* pantalla */}</AppRoot>
```

`Modal` y `Drawer` son controlados (`open` + `onClose`) y usan `position: fixed` sobre toda la ventana.

## Estilos: clases de Tailwind con tokens

Los componentes se configuran por props (`variant`, `size`, `tone`, `padding`). Para tu propio layout usa clases de Tailwind. **Solo existen las clases compiladas en `styles.css`**: usa las familias de abajo y **no uses colores crudos** (`bg-zinc-900`, `text-indigo-400`, `#111118`) ni valores arbitrarios (`w-[480px]`); para medidas puntuales usa `style`.

| Uso | Clases |
|---|---|
| Superficies (de más profunda a más elevada) | `bg-sunken`, `bg-background`, `bg-sidebar`, `bg-card`; translúcidas `bg-secondary`, `bg-secondary-hover` |
| Texto | `text-foreground` (títulos), `text-foreground-soft` (cuerpo), `text-muted-foreground` (secundario), `text-subtle-foreground` (metadatos) |
| Marca (índigo) | `bg-primary`, `text-brand-foreground`, fondos suaves `bg-brand/10`, `bg-brand/20`, `border-brand/30` |
| Estados | `success`, `warning`, `info`, `destructive`: `bg-<estado>`, `bg-<estado>/10`, `border-<estado>/30`, `text-<estado>-foreground` |
| Bordes | `border border-border` (por defecto), `border-input`, `border-border-strong`; `divide-y divide-border` |
| Layout | `flex`, `grid`, `grid-cols-1…6` (`sm:`/`md:`/`lg:`), `gap-0…12`, `p-*`/`px-*`/`py-*`/`m-*` 0–16, `space-y-*`, `mx-auto`, `max-w-xs…7xl`, `w-full`, `w-48…96`, `h-8…64`, `size-3…16` |
| Tipografía | `text-xs…4xl`, `font-medium`/`semibold`/`bold`/`mono`, `leading-*`, `tracking-wider uppercase`, `truncate`, `line-clamp-2` |
| Forma | `rounded-md`/`lg`/`xl`/`2xl`/`full`, `shadow-lg`/`2xl` |

Patrones de la app: página `max-w-7xl mx-auto px-6 py-8`; título `text-2xl font-semibold text-foreground` + subtítulo `text-sm text-muted-foreground`; paneles con `Card`; listas dentro de un panel con `Card variant="subtle" padding="sm"`.

## Íconos

El bundle incluye 49 íconos de lucide con sufijo `Icon` (`PlusIcon`, `BookOpenIcon`, `GraduationCapIcon`, `PlayCircleIcon`, `ClockIcon`, `LockIcon`, `TrophyIcon`, `MedalIcon`, `AwardIcon`, `BellIcon`, `UserIcon`, `UsersIcon`, `GlobeIcon`, `SearchIcon`, `PencilIcon`, `TrashIcon`, `CheckCircleIcon`, `AlertTriangleIcon`, `ChevronRightIcon`…). Dentro de `Button` y `Badge` se dimensionan solos; en otros sitios usa `className="size-4"`. No hay otra librería de íconos.

## Dónde mirar

- `styles.css` → `_ds_bundle.css`: los tokens (`--background`, `--card`, `--primary`, `--brand`, `--success`…) están en `:root`.
- `components/<grupo>/<Name>/<Name>.prompt.md` (ejemplos) y `<Name>.d.ts` (props) de cada componente.

## Ejemplo

```jsx
const { AppRoot, Card, CardHeader, CardTitle, CardDescription, Badge, Button, ProgressBar, PlayCircleIcon } = window.ElearningUI;

<AppRoot className="min-h-screen">
  <main className="max-w-7xl mx-auto px-6 py-8 space-y-6">
    <div>
      <h1 className="text-2xl font-semibold text-foreground">Mis cursos</h1>
      <p className="mt-1 text-sm text-muted-foreground">Continúa donde lo dejaste.</p>
    </div>
    <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between gap-2">
            <CardTitle>Excel Avanzado</CardTitle>
            <Badge tone="success">Inscrito</Badge>
          </div>
          <CardDescription>Lección 3 de 4 · Funciones BUSCARX</CardDescription>
        </CardHeader>
        <ProgressBar value={75} label="3 / 4 lecciones" showValue tone="success" />
        <Button className="mt-5" fullWidth><PlayCircleIcon /> Continuar</Button>
      </Card>
    </div>
  </main>
</AppRoot>
```
