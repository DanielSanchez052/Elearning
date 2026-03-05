import { useEffect, useRef, useState } from 'react';
import type { LessonDto } from '@/types/course.types';

interface LessonPlayerProps {
  lesson: LessonDto;
  courseTitle: string;
  allLessons: LessonDto[];
  onClose: () => void;
  onNavigate: (lesson: LessonDto) => void;
}

export default function LessonPlayer({
  lesson,
  courseTitle,
  allLessons,
  onClose,
  onNavigate,
}: LessonPlayerProps) {
  const videoRef = useRef<HTMLVideoElement>(null);

  // Cerrar con Escape
  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };
    document.addEventListener('keydown', handler);
    return () => document.removeEventListener('keydown', handler);
  }, [onClose]);

  // Reiniciar video al cambiar de lección
  useEffect(() => {
    if (videoRef.current) {
      videoRef.current.load();
      videoRef.current.play().catch(() => { });
    }
  }, [lesson.id]);

  // Solo lecciones reproducibles en el sidebar
  const playableLessons = allLessons.filter((l) => l.type !== 'quiz');
  const currentIndex = playableLessons.findIndex((l) => l.id === lesson.id);
  const prevLesson = currentIndex > 0 ? playableLessons[currentIndex - 1] : null;
  const nextLesson = currentIndex < playableLessons.length - 1 ? playableLessons[currentIndex + 1] : null;

  return (
    <div className="fixed inset-0 z-50 bg-[#08080d] flex flex-col">

      {/* Topbar */}
      <div className="flex items-center justify-between px-5 py-3 border-b border-white/[0.06] flex-shrink-0">
        <div className="flex items-center gap-3 min-w-0">
          <button
            onClick={onClose}
            className="flex items-center gap-1.5 text-zinc-400 hover:text-white transition-colors text-sm flex-shrink-0"
          >
            <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
            Cerrar
          </button>
          <span className="text-white/20">|</span>
          <span className="text-zinc-500 text-sm truncate hidden sm:block">{courseTitle}</span>
          <span className="text-white/20 hidden sm:block">›</span>
          <span className="text-white text-sm font-medium truncate">{lesson.title}</span>
        </div>

        {/* Navegación prev/next */}
        <div className="flex items-center gap-2 flex-shrink-0">
          <button
            onClick={() => prevLesson && onNavigate(prevLesson)}
            disabled={!prevLesson}
            className="flex items-center gap-1 px-3 py-1.5 rounded-lg bg-white/[0.04] border border-white/[0.06] text-zinc-400 text-xs disabled:opacity-30 hover:bg-white/[0.08] transition-colors"
          >
            <svg className="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
            </svg>
            Anterior
          </button>
          <button
            onClick={() => nextLesson && onNavigate(nextLesson)}
            disabled={!nextLesson}
            className="flex items-center gap-1 px-3 py-1.5 rounded-lg bg-white/[0.04] border border-white/[0.06] text-zinc-400 text-xs disabled:opacity-30 hover:bg-white/[0.08] transition-colors"
          >
            Siguiente
            <svg className="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
            </svg>
          </button>
        </div>
      </div>

      {/* Body — reproductor + sidebar */}
      <div className="flex flex-1 overflow-hidden">

        {/* Contenido principal */}
        <div className="flex-1 flex items-center justify-center bg-black overflow-hidden">
          {lesson.type === 'video' && lesson.contentUrl ? (
            <video
              ref={videoRef}
              controls
              className="max-h-full max-w-full w-full"
              style={{ maxHeight: 'calc(100vh - 56px)' }}
            >
              <source src={lesson.contentUrl} type="video/mp4" />
              Tu navegador no soporta la reproducción de video.
            </video>
          ) : lesson.type === 'pdf' && lesson.contentUrl ? (
            <PdfViewer url={lesson.contentUrl} />
          ) : (
            <div className="text-center text-zinc-500 text-sm">
              Contenido no disponible.
            </div>
          )}
        </div>

        {/* Sidebar — lista de lecciones */}
        <div className="w-72 flex-shrink-0 border-l border-white/[0.06] overflow-y-auto bg-[#0d0d14]">
          <div className="p-4 border-b border-white/[0.06]">
            <p className="text-xs font-semibold text-zinc-500 uppercase tracking-wider">
              Lecciones del curso
            </p>
            <p className="text-xs text-zinc-600 mt-0.5">
              {currentIndex + 1} de {playableLessons.length}
            </p>
          </div>

          <div className="p-2 space-y-1">
            {playableLessons.map((l, index) => {
              const isActive = l.id === lesson.id;
              return (
                <button
                  key={l.id}
                  onClick={() => onNavigate(l)}
                  className={`w-full flex items-start gap-3 px-3 py-3 rounded-xl text-left transition-all ${isActive
                      ? 'bg-indigo-600/20 border border-indigo-500/30'
                      : 'hover:bg-white/[0.04] border border-transparent'
                    }`}
                >
                  {/* Índice o ícono de reproducción */}
                  <div className={`w-6 h-6 rounded-md flex items-center justify-center flex-shrink-0 mt-0.5 ${isActive ? 'bg-indigo-500' : 'bg-white/[0.06]'
                    }`}>
                    {isActive ? (
                      <svg className="w-3 h-3 text-white" fill="currentColor" viewBox="0 0 24 24">
                        <path d="M8 5v14l11-7z" />
                      </svg>
                    ) : (
                      <span className="text-xs text-zinc-500">{index + 1}</span>
                    )}
                  </div>

                  <div className="flex-1 min-w-0">
                    <p className={`text-xs font-medium leading-snug ${isActive ? 'text-indigo-300' : 'text-zinc-300'
                      }`}>
                      {l.title}
                    </p>
                    <p className="text-xs text-zinc-600 mt-0.5 capitalize">{l.type}</p>
                  </div>
                </button>
              );
            })}
          </div>
        </div>

      </div>
    </div>
  );
}

// ── PDF Viewer ────────────────────────────────────────────────────────────────

function PdfViewer({ url }: { url: string }) {
  const [numPages, setNumPages] = useState<number>(0);
  const [pageNum, setPageNum] = useState(1);
  const [loading, setLoading] = useState(true);

  // Usamos iframe nativo para el MVP — no requiere dependencias adicionales.
  // Para el MVP 2 puedes reemplazarlo con react-pdf para más control.
  return (
    <div className="w-full h-full flex flex-col">
      <iframe
        src={`${url}#toolbar=1&navpanes=0`}
        className="flex-1 w-full border-0"
        title="Visor de PDF"
        onLoad={() => setLoading(false)}
      />
      {loading && (
        <div className="absolute inset-0 flex items-center justify-center bg-black/50">
          <div className="text-zinc-400 text-sm">Cargando PDF...</div>
        </div>
      )}
    </div>
  );
}
