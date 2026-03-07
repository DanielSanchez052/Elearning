import { useRef, useState, useCallback, useEffect } from 'react';

interface FileUploadFieldProps {
  label: string;
  accept: string;
  maxMB: number;
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
  if (secs < 60) return `${Math.round(secs)}s`;
  const m = Math.floor(secs / 60);
  const s = Math.round(secs % 60);
  return `${m}m ${s}s`;
}

// ── Types ─────────────────────────────────────────────────────────────────────

type UploadPhase = 'idle' | 'uploading' | 'processing' | 'done' | 'error';

interface UploadStats {
  progress: number;       // 0–100
  speed: number;       // bytes/s
  remaining: number;       // seconds
  uploaded: number;       // bytes so far (estimated)
  total: number;       // total bytes
}

// ── Component ─────────────────────────────────────────────────────────────────

export default function FileUploadField({
  label,
  accept,
  maxMB,
  currentUrl,
  onUpload,
  onClear,
  hint,
}: FileUploadFieldProps) {
  const inputRef = useRef<HTMLInputElement>(null);
  const startTimeRef = useRef<number>(0);
  const lastProgressRef = useRef<{ pct: number; time: number }>({ pct: 0, time: 0 });

  const [phase, setPhase] = useState<UploadPhase>('idle');
  const [url, setUrl] = useState(currentUrl ?? '');
  const [error, setError] = useState('');
  const [fileName, setFileName] = useState('');
  const [fileSize, setFileSize] = useState(0);
  const [isDragging, setIsDragging] = useState(false);
  const [stats, setStats] = useState<UploadStats>({
    progress: 0, speed: 0, remaining: 0, uploaded: 0, total: 0,
  });

  // Sync external URL changes (edit mode)
  useEffect(() => {
    if (currentUrl && !url) setUrl(currentUrl);
  }, [currentUrl]);

  const onProgress = useCallback((pct: number) => {
    const now = Date.now();
    const elapsed = (now - startTimeRef.current) / 1000;  // seconds
    const last = lastProgressRef.current;

    const deltaTime = (now - last.time) / 1000;
    const deltaPct = pct - last.pct;

    let speed = 0;
    let remaining = Infinity;

    if (deltaTime > 0 && deltaPct > 0) {
      // bytes uploaded in this delta
      const bytesInDelta = (deltaPct / 100) * fileSize;
      speed = bytesInDelta / deltaTime;

      const pctLeft = 100 - pct;
      const bytesLeft = (pctLeft / 100) * fileSize;
      remaining = speed > 0 ? bytesLeft / speed : Infinity;
    } else if (elapsed > 0) {
      // fallback: overall average
      const uploadedBytes = (pct / 100) * fileSize;
      speed = uploadedBytes / elapsed;
      const bytesLeft = fileSize - uploadedBytes;
      remaining = speed > 0 ? bytesLeft / speed : Infinity;
    }

    lastProgressRef.current = { pct, time: now };

    setStats({
      progress: pct,
      speed,
      remaining,
      uploaded: (pct / 100) * fileSize,
      total: fileSize,
    });

    // At 100% network-wise, switch to "processing" phase
    if (pct >= 100) setPhase('processing');
  }, [fileSize]);

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
      const resultUrl = await onUpload(file, onProgress);
      setUrl(resultUrl);
      setPhase('done');
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
    setUrl('');
    setError('');
    setPhase('idle');
    setFileName('');
    setFileSize(0);
    if (inputRef.current) inputRef.current.value = '';
    onClear?.();
  };

  // ── Render helpers ──────────────────────────────────────────────────────────

  const isUploading = phase === 'uploading';
  const isProcessing = phase === 'processing';
  const isActive = isUploading || isProcessing;

  return (
    <div>
      <label className="block text-sm font-medium text-zinc-300 mb-1.5">{label}</label>

      {/* ── Done state ── */}
      {url && phase !== 'uploading' && phase !== 'processing' ? (
        <div className="flex items-center gap-3 px-4 py-3 rounded-xl bg-emerald-500/5 border border-emerald-500/25 group">
          <div className="w-8 h-8 rounded-lg bg-emerald-500/10 flex items-center justify-center flex-shrink-0">
            <svg className="w-4 h-4 text-emerald-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
            </svg>
          </div>
          <div className="flex-1 min-w-0">
            <p className="text-sm text-zinc-200 truncate">
              {fileName || url.split('/').pop()}
            </p>
            {fileSize > 0 && (
              <p className="text-xs text-zinc-500 mt-0.5">{formatBytes(fileSize)}</p>
            )}
          </div>
          <button
            type="button"
            onClick={handleClear}
            className="opacity-0 group-hover:opacity-100 text-zinc-500 hover:text-red-400 transition-all text-xs flex items-center gap-1"
          >
            <svg className="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
            Quitar
          </button>
        </div>

        /* ── Upload in progress ── */
      ) : isActive ? (
        <UploadProgress
          phase={phase}
          stats={stats}
          fileName={fileName}
        />

        /* ── Error state ── */
      ) : phase === 'error' ? (
        <div
          onClick={() => { setPhase('idle'); setError(''); }}
          className="flex items-center gap-3 px-4 py-3 rounded-xl bg-red-500/5 border border-red-500/25 cursor-pointer hover:bg-red-500/10 transition-all"
        >
          <div className="w-8 h-8 rounded-lg bg-red-500/10 flex items-center justify-center flex-shrink-0">
            <svg className="w-4 h-4 text-red-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </div>
          <div className="flex-1">
            <p className="text-sm text-red-400">{error}</p>
            <p className="text-xs text-zinc-500 mt-0.5">Haz clic para intentar de nuevo</p>
          </div>
        </div>

        /* ── Idle dropzone ── */
      ) : (
        <div
          onDrop={handleDrop}
          onDragOver={(e) => { e.preventDefault(); setIsDragging(true); }}
          onDragLeave={() => setIsDragging(false)}
          onClick={() => inputRef.current?.click()}
          className={`
            relative flex flex-col items-center justify-center gap-2.5 px-4 py-7 rounded-xl
            border border-dashed cursor-pointer transition-all duration-200
            ${isDragging
              ? 'border-indigo-500/60 bg-indigo-500/[0.06] scale-[0.99]'
              : 'border-white/[0.10] bg-white/[0.02] hover:border-indigo-500/40 hover:bg-white/[0.04]'
            }
          `}
        >
          <div className={`w-10 h-10 rounded-xl flex items-center justify-center transition-all ${isDragging ? 'bg-indigo-500/15' : 'bg-white/[0.04]'
            }`}>
            <svg className={`w-5 h-5 transition-colors ${isDragging ? 'text-indigo-400' : 'text-zinc-500'}`}
              fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
                d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
            </svg>
          </div>
          <div className="text-center">
            <p className="text-sm text-zinc-400">
              <span className="text-indigo-400 font-medium">Selecciona un archivo</span>
              {' '}o arrastra aquí
            </p>
            {hint && <p className="text-xs text-zinc-600 mt-0.5">{hint}</p>}
          </div>
          <input
            ref={inputRef}
            type="file"
            accept={accept}
            className="hidden"
            onChange={(e) => {
              const file = e.target.files?.[0];
              if (file) handleFile(file);
            }}
          />
        </div>
      )}

      {/* Error message (for size validation) */}
      {error && phase !== 'error' && (
        <p className="mt-1.5 text-xs text-red-400">{error}</p>
      )}
    </div>
  );
}

// ── Upload Progress Panel ──────────────────────────────────────────────────────

function UploadProgress({
  phase,
  stats,
  fileName,
}: {
  phase: UploadPhase;
  stats: UploadStats;
  fileName: string;
}) {
  const isProcessing = phase === 'processing';
  const pct = isProcessing ? 100 : stats.progress;

  return (
    <div className="px-4 py-4 rounded-xl bg-white/[0.03] border border-white/[0.08] space-y-3">

      {/* File name + status */}
      <div className="flex items-center justify-between gap-3">
        <div className="flex items-center gap-2.5 min-w-0">
          <div className="w-7 h-7 rounded-lg bg-indigo-500/10 flex items-center justify-center flex-shrink-0">
            {isProcessing ? (
              <svg className="w-3.5 h-3.5 text-indigo-400 animate-spin" fill="none" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                <path className="opacity-75" fill="currentColor"
                  d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
              </svg>
            ) : (
              <svg className="w-3.5 h-3.5 text-indigo-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                  d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
              </svg>
            )}
          </div>
          <p className="text-sm text-zinc-300 truncate">{fileName}</p>
        </div>
        <span className="text-sm font-mono text-indigo-400 flex-shrink-0 tabular-nums">
          {isProcessing ? '100%' : `${pct}%`}
        </span>
      </div>

      {/* Progress bar */}
      <div className="relative w-full bg-white/[0.06] rounded-full h-1.5 overflow-hidden">
        {isProcessing ? (
          // Indeterminate shimmer when processing server-side
          <div className="absolute inset-0 rounded-full bg-gradient-to-r from-indigo-600 via-indigo-400 to-indigo-600 animate-[shimmer_1.5s_ease-in-out_infinite] bg-[length:200%_100%]" />
        ) : (
          <div
            className="h-full rounded-full bg-indigo-500 transition-all duration-300 ease-out"
            style={{ width: `${pct}%` }}
          />
        )}
      </div>

      {/* Stats row */}
      <div className="flex items-center justify-between text-xs text-zinc-500">
        {isProcessing ? (
          <span className="text-indigo-400/70">Procesando en el servidor...</span>
        ) : (
          <>
            <div className="flex items-center gap-3">
              {/* Speed */}
              {stats.speed > 0 && (
                <span className="flex items-center gap-1">
                  <svg className="w-3 h-3" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                      d="M13 10V3L4 14h7v7l9-11h-7z" />
                  </svg>
                  {formatBytes(stats.speed)}/s
                </span>
              )}
              {/* Uploaded / total */}
              {stats.total > 0 && (
                <span>
                  {formatBytes(stats.uploaded)} / {formatBytes(stats.total)}
                </span>
              )}
            </div>
            {/* Time remaining */}
            {stats.remaining > 0 && isFinite(stats.remaining) && pct < 99 && (
              <span className="flex items-center gap-1">
                <svg className="w-3 h-3" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                    d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                {formatSeconds(stats.remaining)} restantes
              </span>
            )}
          </>
        )}
      </div>
    </div>
  );
}

export { FileUploadField };
export type { FileUploadFieldProps };
