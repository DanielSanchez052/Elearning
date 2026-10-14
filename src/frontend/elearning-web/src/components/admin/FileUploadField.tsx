import { useRef, useState, useCallback, useId } from 'react';
import { Button, Label, ProgressBar } from '@/components/ui';
import { AlertTriangleIcon, CheckCircleIcon, UploadIcon, XIcon } from '@/components/ui/icons';
import { cn } from '@/lib/utils';

interface FileUploadFieldProps {
  label: string;
  accept: string;
  maxMB: number;
  /** URL ya subida: la fuente de verdad la tiene el padre (onUpload la fija, onClear la borra). */
  currentUrl?: string | null;
  onUpload: (file: File, onProgress: (pct: number) => void) => Promise<string>;
  onClear?: () => void;
  hint?: string;
}

// ── Helpers ───────────────────────────────────────────────────────────────────

function formatBytes(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

function formatSeconds(secs: number): string {
  if (!isFinite(secs) || secs <= 0) return '—';
  if (secs < 60) return `${Math.round(secs)} s`;
  const m = Math.floor(secs / 60);
  const s = Math.round(secs % 60);
  return `${m} min ${s} s`;
}

// ── Types ─────────────────────────────────────────────────────────────────────

type UploadPhase = 'idle' | 'uploading' | 'processing' | 'error';

interface UploadStats {
  progress: number; // 0–100
  speed: number; // bytes/s
  remaining: number; // seconds
  uploaded: number; // bytes so far (estimated)
  total: number; // total bytes
}

// ── Component ─────────────────────────────────────────────────────────────────

/** Zona de subida de archivos del rediseño: arrastrar o seleccionar, progreso con velocidad y tiempo restante. */
export default function FileUploadField({ label, accept, maxMB, currentUrl, onUpload, onClear, hint }: FileUploadFieldProps) {
  const inputId = useId();
  const inputRef = useRef<HTMLInputElement>(null);
  const startTimeRef = useRef<number>(0);
  const lastProgressRef = useRef<{ pct: number; time: number }>({ pct: 0, time: 0 });

  const [phase, setPhase] = useState<UploadPhase>('idle');
  const [error, setError] = useState('');
  const [fileName, setFileName] = useState('');
  const [fileSize, setFileSize] = useState(0);
  const [isDragging, setIsDragging] = useState(false);
  const [stats, setStats] = useState<UploadStats>({ progress: 0, speed: 0, remaining: 0, uploaded: 0, total: 0 });

  const url = currentUrl ?? '';

  const onProgress = useCallback(
    (pct: number) => {
      const now = Date.now();
      const elapsed = (now - startTimeRef.current) / 1000;
      const last = lastProgressRef.current;
      const deltaTime = (now - last.time) / 1000;
      const deltaPct = pct - last.pct;

      let speed = 0;
      let remaining = Infinity;

      if (deltaTime > 0 && deltaPct > 0) {
        speed = ((deltaPct / 100) * fileSize) / deltaTime;
        const bytesLeft = ((100 - pct) / 100) * fileSize;
        remaining = speed > 0 ? bytesLeft / speed : Infinity;
      } else if (elapsed > 0) {
        const uploadedBytes = (pct / 100) * fileSize;
        speed = uploadedBytes / elapsed;
        remaining = speed > 0 ? (fileSize - uploadedBytes) / speed : Infinity;
      }

      lastProgressRef.current = { pct, time: now };
      setStats({ progress: pct, speed, remaining, uploaded: (pct / 100) * fileSize, total: fileSize });

      // Al 100 % de red, el servidor aún procesa el archivo.
      if (pct >= 100) setPhase('processing');
    },
    [fileSize]
  );

  const handleFile = async (file: File) => {
    setError('');

    if (file.size > maxMB * 1024 * 1024) {
      setError(`El archivo supera el límite de ${maxMB} MB.`);
      return;
    }

    setFileName(file.name);
    setFileSize(file.size);
    startTimeRef.current = Date.now();
    lastProgressRef.current = { pct: 0, time: Date.now() };
    setPhase('uploading');
    setStats({ progress: 0, speed: 0, remaining: 0, uploaded: 0, total: file.size });

    try {
      await onUpload(file, onProgress);
      setPhase('idle');
    } catch {
      setError('Error al subir el archivo. Intenta de nuevo.');
      setPhase('error');
    }
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);
    const file = e.dataTransfer.files[0];
    if (file) handleFile(file);
  };

  const handleClear = () => {
    setError('');
    setPhase('idle');
    setFileName('');
    setFileSize(0);
    if (inputRef.current) inputRef.current.value = '';
    onClear?.();
  };

  const isActive = phase === 'uploading' || phase === 'processing';

  return (
    <div>
      <Label htmlFor={inputId}>{label}</Label>
      <input
        ref={inputRef}
        id={inputId}
        type="file"
        accept={accept}
        className="sr-only"
        onChange={(e) => {
          const file = e.target.files?.[0];
          if (file) handleFile(file);
        }}
      />

      {isActive ? (
        <div className="flex flex-col gap-2 rounded-xl border border-border bg-secondary px-4 py-4">
          <ProgressBar
            value={phase === 'processing' ? 100 : stats.progress}
            size="sm"
            showValue
            label={phase === 'processing' ? `Procesando ${fileName} en el servidor…` : `Subiendo ${fileName}`}
          />
          {phase === 'uploading' && (
            <div className="flex flex-wrap items-center justify-between gap-2 text-xs text-subtle-foreground">
              <span>
                {stats.total > 0 && `${formatBytes(stats.uploaded)} / ${formatBytes(stats.total)}`}
                {stats.speed > 0 && ` · ${formatBytes(stats.speed)}/s`}
              </span>
              {stats.remaining > 0 && isFinite(stats.remaining) && stats.progress < 99 && (
                <span>{formatSeconds(stats.remaining)} restantes</span>
              )}
            </div>
          )}
        </div>
      ) : url ? (
        <div className="flex items-center gap-3 rounded-xl border border-success/30 bg-success/10 px-4 py-3">
          <CheckCircleIcon className="size-5 shrink-0 text-success-foreground" aria-hidden="true" />
          <div className="min-w-0 flex-1">
            <p className="truncate text-sm text-foreground">{fileName || url.split('/').pop()}</p>
            {fileSize > 0 && <p className="text-xs text-subtle-foreground">{formatBytes(fileSize)}</p>}
          </div>
          <Button variant="ghost" size="sm" onClick={() => inputRef.current?.click()}>
            Reemplazar
          </Button>
          <Button variant="ghost" size="icon" aria-label={`Quitar ${label.toLowerCase()}`} onClick={handleClear}>
            <XIcon />
          </Button>
        </div>
      ) : (
        <div
          onDrop={handleDrop}
          onDragOver={(e) => {
            e.preventDefault();
            setIsDragging(true);
          }}
          onDragLeave={() => setIsDragging(false)}
          className={cn(
            'flex flex-wrap items-center gap-4 rounded-xl border border-dashed p-4 transition-colors',
            isDragging ? 'border-brand bg-brand/10' : 'border-border-strong',
            phase === 'error' && 'border-destructive/40'
          )}
        >
          <span
            className={cn(
              'flex size-10 shrink-0 items-center justify-center rounded-lg',
              phase === 'error' ? 'bg-destructive/10 text-destructive-foreground' : 'bg-secondary text-subtle-foreground'
            )}
          >
            {phase === 'error' ? <AlertTriangleIcon className="size-5" aria-hidden="true" /> : <UploadIcon className="size-5" aria-hidden="true" />}
          </span>
          <div className="min-w-0 flex-1">
            <p className="text-sm text-foreground-soft">{phase === 'error' ? error : 'Arrastra un archivo o selecciónalo'}</p>
            {hint && <p className="mt-0.5 text-xs text-subtle-foreground">{hint}</p>}
          </div>
          <Button variant="secondary" size="sm" onClick={() => inputRef.current?.click()}>
            {phase === 'error' ? 'Intentar de nuevo' : 'Seleccionar'}
          </Button>
        </div>
      )}

      {error && phase !== 'error' && <p className="mt-1.5 text-xs text-destructive-foreground">{error}</p>}
    </div>
  );
}

export { FileUploadField };
export type { FileUploadFieldProps };
