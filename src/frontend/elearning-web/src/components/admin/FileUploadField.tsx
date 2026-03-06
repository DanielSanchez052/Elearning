import { useRef, useState } from 'react';

interface FileUploadFieldProps {
  label:       string;
  accept:      string;        // e.g. "video/mp4", "application/pdf", "image/*"
  maxMB:       number;
  currentUrl?: string | null;
  onUpload:    (file: File, onProgress: (pct: number) => void) => Promise<string>;
  onClear?:    () => void;
  hint?:       string;
}

export default function FileUploadField({
  label,
  accept,
  maxMB,
  currentUrl,
  onUpload,
  onClear,
  hint,
}: FileUploadFieldProps) {
  const inputRef               = useRef<HTMLInputElement>(null);
  const [progress, setProgress] = useState<number | null>(null);
  const [error,    setError]    = useState('');
  const [url,      setUrl]      = useState(currentUrl ?? '');

  const handleFile = async (file: File) => {
    setError('');
    if (file.size > maxMB * 1024 * 1024) {
      setError(`El archivo no puede superar ${maxMB} MB.`);
      return;
    }
    try {
      setProgress(0);
      const resultUrl = await onUpload(file, setProgress);
      setUrl(resultUrl);
    } catch {
      setError('Error al subir el archivo. Intenta de nuevo.');
    } finally {
      setProgress(null);
    }
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    const file = e.dataTransfer.files[0];
    if (file) handleFile(file);
  };

  const handleClear = () => {
    setUrl('');
    setError('');
    if (inputRef.current) inputRef.current.value = '';
    onClear?.();
  };

  return (
    <div>
      <label className="block text-sm font-medium text-zinc-300 mb-1.5">{label}</label>

      {/* Preview / zona de carga */}
      {url ? (
        <div className="flex items-center gap-3 px-4 py-3 rounded-xl bg-white/[0.04] border border-emerald-500/30">
          <svg className="w-4 h-4 text-emerald-400 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
          </svg>
          <span className="text-sm text-zinc-300 flex-1 truncate">
            {url.split('/').pop()}
          </span>
          <button
            type="button"
            onClick={handleClear}
            className="text-zinc-500 hover:text-red-400 transition-colors text-xs"
          >
            Quitar
          </button>
        </div>
      ) : progress !== null ? (
        <div className="px-4 py-3 rounded-xl bg-white/[0.04] border border-white/[0.08] space-y-2">
          <div className="flex items-center justify-between text-xs text-zinc-400">
            <span>Subiendo...</span>
            <span>{progress}%</span>
          </div>
          <div className="w-full bg-white/[0.08] rounded-full h-1.5">
            <div
              className="bg-indigo-500 h-1.5 rounded-full transition-all duration-300"
              style={{ width: `${progress}%` }}
            />
          </div>
        </div>
      ) : (
        <div
          onDrop={handleDrop}
          onDragOver={(e) => e.preventDefault()}
          onClick={() => inputRef.current?.click()}
          className="flex flex-col items-center justify-center gap-2 px-4 py-6 rounded-xl bg-white/[0.02] border border-dashed border-white/[0.12] hover:border-indigo-500/40 hover:bg-white/[0.04] cursor-pointer transition-all"
        >
          <svg className="w-6 h-6 text-zinc-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
          </svg>
          <p className="text-sm text-zinc-500">
            <span className="text-indigo-400">Selecciona un archivo</span> o arrastra aquí
          </p>
          {hint && <p className="text-xs text-zinc-600">{hint}</p>}
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

      {error && <p className="mt-1.5 text-xs text-red-400">{error}</p>}

      {/* Input oculto para que React Hook Form lea la URL */}
      <input type="hidden" name="__url" value={url} readOnly />
    </div>
  );
}

// Exportar la URL para usarla desde el padre
export { FileUploadField };
export type { FileUploadFieldProps };
