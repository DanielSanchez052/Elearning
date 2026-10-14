# design-sync — notas del repo

## Cómo se construye
- El repo es una **app** (no publica paquete). El sistema de diseño se compila aparte con `npm --prefix src/frontend/elearning-web run build:ds` (`cfg.buildCmd`): `vite.ds.config.ts` (modo librería, entrada `src/components/ui/ds-library.ts`) + `tsconfig.ds.json` → `src/frontend/elearning-web/dist-ds/` (gitignored), que trae su propio `package.json` (`@elearning/ui`). **Recompila siempre antes del convertidor** si cambió algo en `src/components/ui` o `index.css`.
- `cfg.entry` apunta a `dist-ds/index.js`; `--node-modules src/frontend/elearning-web/node_modules`.
- CSS = Tailwind compilado desde `src/index.css` + `src/components/ui/ds-library.css` (lista fija `@source inline(...)`). Fuera de la app no hay escaneo de Tailwind: **una clase que no esté en `dist-ds/styles.css` no existe para Claude Design**. Si una vista previa o la cabecera de convenciones usa una clase nueva, agrégala a `ds-library.css`.
- Íconos: conjunto curado `src/components/ui/icons.ts` vía `cfg.extraEntries` (no lucide-react completo: sube el bundle de 130 KB a 1,3 MB y su ícono `Badge` choca con el componente `Badge`).

## Correcciones de configuración (y por qué)
- `componentSrcMap`: `Field/Label/Select/Textarea` viven en `Input.tsx` (el emparejamiento por nombre no los encuentra); `Drawer` fijado. `CardHeader/CardTitle/CardDescription` y `TableHead/TableBody/TableRow/TableHeader/TableCell` excluidos como tarjetas: se documentan dentro de `Card` y `Table`.
- `dtsPropsFor`: el extractor filtra los atributos nativos del DOM (`onClick`, `value`, `onChange`, `disabled`, `type`…) de los componentes que extienden `ComponentProps<'button'|'input'|…>`; los contratos de Button, CloseButton, Card, Input, Textarea, Select, Label, Field, AppRoot, Checkbox, RadioCard, SearchInput y Table están escritos a mano. **Si cambian las props de esos componentes, actualiza `dtsPropsFor`.**
- `dtsPropsFor` también en SegmentedTabs (el genérico `<T extends string>` queda sin resolver en el `.d.ts`; el contrato usa `string`) y en StateBlock/StatCard (el tipo del prop `icon` salía como `ComponentClass | FunctionComponent` sin importar).
- `leadingJsdoc` no lee el JSDoc de `export default function`: los componentes usan `export function` (+ `export default` aparte si hace falta).
- `provider: AppRoot` con `rounded-xl p-6`: el harness pinta `body{background:#fff}` y el sistema es solo oscuro.
- Overlays (`Modal`, `Drawer`): `cardMode: single`. Sus vistas previas los envuelven en un `Frame` con `transform: translateZ(0)` para que el `position: fixed` quede contenido en la tarjeta.
- `cardMode: column` en Button, CloseButton, ProgressBar, Field, Input, Textarea, Card, Alert, Skeleton, RadioCard, SearchInput, Switch, Table, StateBlock, StatCard y Pagination (`[GRID_OVERFLOW]`, o anchos completos por diseño).
- Vistas previas de componentes controlados (Checkbox, RadioCard, Switch, SegmentedTabs, Pagination) son estáticas: valor fijo + `noop` como manejador. No se usa `useState` en `previews/`.

## Entorno (Windows)
- Verificación de render con **Microsoft Edge** (no hay Chromium de Playwright): `DS_CHROMIUM_PATH="C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe"`; `playwright` instalado en `.ds-sync/` con `PLAYWRIGHT_SKIP_BROWSER_DOWNLOAD=1`.
- No dejes la shell con el directorio actual dentro de `ds-bundle/`: Windows bloquea el borrado y el convertidor falla con `EPERM` en `rmSync`.
- Para validar clases responsivas contra `_ds_bundle.css`, no uses `grep 'md\\:w-64'` en Git Bash (el escapado falla y da 0 falsos); busca con contexto, p. ej. `grep -o '.\{8\}w-64.\{3\}'`.
- Un cambio en `ds-library.css` o `index.css` cambia `styleSha` y marca **todos** los componentes como `changed`: hay que recalificarlos aunque sus vistas previas no cambien (ocurrió en la Fase 0 del rediseño).

## Known render warns
- Ninguno pendiente: los `[GRID_OVERFLOW]` se resolvieron con `cardMode`.

## Re-sync risks
- Sincronizado tras la fase 5 del rediseño (`Table` relative, `StateBlock.titleAs`, `--subtle-foreground` #85858e, `--success-strong`/`--destructive-strong`, foco en `Modal`/`Drawer`).
- **Los estilos de `Button` viven en `components/ui/button-variants.ts`.** Al cambiar solo ese archivo, el driver no marcó `Button` ni `Modal` como `changed` (sus vistas previas no cambiaron). Si se toca `button-variants.ts`, haz una revisión puntual: `package-capture.mjs --components Button,Modal --spot-check-components Button,Modal`.
- Los componentes de dominio (`components/courses`: CourseCard, CourseThumbnail, LessonRow) **no se publican** en ElearningUI (usan react-router y tipos de la app). Las plantillas `templates/elearning-redesign/` del proyecto los recrean con clases; si el rediseño cambia, recuérdalo al comparar.
- El proyecto de Claude Design contiene archivos que no produce este build (`templates/`, `uploads/`, `_shots/`): son del trabajo de diseño, nunca los borres en una reconciliación.
- `dtsPropsFor` está escrito a mano: se desincroniza en silencio si cambian las props de esos componentes.
- La lista de clases de `ds-library.css` y la tabla de `conventions.md` deben coincidir; valida la cabecera contra `_ds_bundle.css` en cada sincronización.
- Las clases de la app (zinc/indigo crudos) también están en el CSS porque `index.css` escanea toda la app; la cabecera prohíbe usarlas, pero existen.
- Sin fuente de marca: se usa la pila `font-sans` del sistema (no hay `@font-face`).
