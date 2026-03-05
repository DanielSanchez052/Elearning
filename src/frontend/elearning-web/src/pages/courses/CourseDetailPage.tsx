import { useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useCourseDetail } from '@/hooks/useCourses';
import type { LessonDto } from '@/types/course.types';
import LessonPlayer from '@/components/courses/LessonPlayer';

export default function CourseDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { data: course, isLoading, isError } = useCourseDetail(id ?? '');
  const [activeLesson, setActiveLesson] = useState<LessonDto | null>(null);

  if (isLoading) return <CourseDetailSkeleton />;

  if (isError || !course) {
    return (
      <div className="min-h-screen bg-[#0a0a0f] flex items-center justify-center">
        <div className="text-center space-y-3">
          <p className="text-zinc-400 text-sm">Curso no encontrado.</p>
          <Link to="/courses" className="text-indigo-400 text-sm hover:text-indigo-300 transition-colors">
            ← Volver al catálogo
          </Link>
        </div>
      </div>
    );
  }

  const videoLessons = course.lessons.filter((l) => l.type === 'video');
  const pdfLessons = course.lessons.filter((l) => l.type === 'pdf');
  const quizLessons = course.lessons.filter((l) => l.type === 'quiz');

  return (
    <>
      <div className="min-h-screen bg-[#0a0a0f] text-white">

        {/* Breadcrumb */}
        <div className="border-b border-white/[0.06]">
          <div className="max-w-5xl mx-auto px-6 py-3 flex items-center gap-2 text-sm text-zinc-500">
            <Link to="/courses" className="hover:text-zinc-300 transition-colors">Catálogo</Link>
            <span>/</span>
            <span className="text-zinc-300 truncate max-w-xs">{course.title}</span>
          </div>
        </div>

        <div className="max-w-5xl mx-auto px-6 py-10 space-y-10">

          {/* Hero del curso */}
          <div className="flex gap-8 items-start">
            {/* Thumbnail */}
            <div className="hidden md:block w-56 h-36 rounded-2xl overflow-hidden flex-shrink-0 bg-gradient-to-br from-indigo-900/40 to-zinc-900">
              {course.thumbnailUrl ? (
                <img src={course.thumbnailUrl} alt={course.title} className="w-full h-full object-cover" />
              ) : (
                <div className="w-full h-full flex items-center justify-center">
                  <svg className="w-10 h-10 text-indigo-800/60" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
                  </svg>
                </div>
              )}
            </div>

            {/* Info */}
            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-2 mb-3">
                {course.isGlobal && (
                  <span className="px-2 py-0.5 rounded-full bg-indigo-500/20 border border-indigo-500/30 text-indigo-300 text-xs font-medium">
                    Global
                  </span>
                )}
                <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${course.isActive
                  ? 'bg-emerald-500/10 border border-emerald-500/20 text-emerald-400'
                  : 'bg-zinc-500/10 border border-zinc-500/20 text-zinc-400'
                  }`}>
                  {course.isActive ? 'Publicado' : 'Borrador'}
                </span>
              </div>

              <h1 className="text-2xl font-semibold text-white leading-tight">{course.title}</h1>

              {course.description && (
                <p className="mt-3 text-zinc-400 text-sm leading-relaxed">{course.description}</p>
              )}

              <div className="mt-4 flex flex-wrap items-center gap-4 text-sm text-zinc-500">
                <span className="flex items-center gap-1.5">
                  <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                  </svg>
                  {course.instructorName}
                </span>
                <span className="flex items-center gap-1.5">
                  <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                  </svg>
                  {course.lessons.length} lecciones
                </span>
                {course.countries.length > 0 && !course.isGlobal && (
                  <span className="flex items-center gap-1.5">
                    <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3.055 11H5a2 2 0 012 2v1a2 2 0 002 2 2 2 0 012 2v2.945M8 3.935V5.5A2.5 2.5 0 0010.5 8h.5a2 2 0 012 2 2 2 0 104 0 2 2 0 012-2h1.064M15 20.488V18a2 2 0 012-2h3.064" />
                    </svg>
                    {course.countries.map((c) => c.name).join(', ')}
                  </span>
                )}
              </div>
            </div>
          </div>

          {/* Lecciones */}
          {course.lessons.length === 0 ? (
            <div className="text-center py-12 rounded-2xl bg-[#111118] border border-white/[0.06]">
              <p className="text-zinc-500 text-sm">Este curso aún no tiene lecciones.</p>
            </div>
          ) : (
            <div className="space-y-8">

              {/* Videos */}
              {videoLessons.length > 0 && (
                <LessonSection
                  title="Videos"
                  icon="video"
                  lessons={videoLessons}
                  onSelect={setActiveLesson}
                />
              )}

              {/* PDFs */}
              {pdfLessons.length > 0 && (
                <LessonSection
                  title="Material de lectura"
                  icon="pdf"
                  lessons={pdfLessons}
                  onSelect={setActiveLesson}
                />
              )}

              {/* Quizzes */}
              {quizLessons.length > 0 && (
                <LessonSection
                  title="Evaluaciones"
                  icon="quiz"
                  lessons={quizLessons}
                  onSelect={setActiveLesson}
                />
              )}

            </div>
          )}
        </div>
      </div>

      {/* Reproductor en pantalla completa */}
      {activeLesson && (
        <LessonPlayer
          lesson={activeLesson}
          courseTitle={course.title}
          allLessons={course.lessons}
          onClose={() => setActiveLesson(null)}
          onNavigate={setActiveLesson}
        />
      )}
    </>
  );
}

// ── Lesson Section ────────────────────────────────────────────────────────────

interface LessonSectionProps {
  title: string;
  icon: 'video' | 'pdf' | 'quiz';
  lessons: LessonDto[];
  onSelect: (lesson: LessonDto) => void;
}

function LessonSection({ title, icon, lessons, onSelect }: LessonSectionProps) {
  const iconMap = {
    video: (
      <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M14.752 11.168l-3.197-2.132A1 1 0 0010 9.87v4.263a1 1 0 001.555.832l3.197-2.132a1 1 0 000-1.664z" />
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
      </svg>
    ),
    pdf: (
      <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
      </svg>
    ),
    quiz: (
      <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
      </svg>
    ),
  };

  return (
    <div>
      <div className="flex items-center gap-2 mb-3">
        <span className="text-zinc-400">{iconMap[icon]}</span>
        <h2 className="text-sm font-semibold text-zinc-300 uppercase tracking-wider">{title}</h2>
        <span className="text-xs text-zinc-600">({lessons.length})</span>
      </div>

      <div className="space-y-2">
        {lessons.map((lesson, index) => (
          <button
            key={lesson.id}
            onClick={() => lesson.type !== 'quiz' ? onSelect(lesson) : undefined}
            disabled={lesson.type === 'quiz'}
            className="w-full flex items-center gap-4 px-5 py-4 rounded-xl bg-[#111118] border border-white/[0.06] hover:border-indigo-500/30 hover:bg-indigo-500/5 disabled:opacity-50 disabled:cursor-not-allowed transition-all text-left group"
          >
            {/* Número */}
            <span className="w-7 h-7 rounded-lg bg-white/[0.04] border border-white/[0.06] flex items-center justify-center text-xs text-zinc-500 flex-shrink-0 group-hover:border-indigo-500/20 transition-colors">
              {index + 1}
            </span>

            {/* Título */}
            <span className="flex-1 text-sm text-zinc-300 group-hover:text-white transition-colors">
              {lesson.title}
            </span>

            {/* Badges */}
            <div className="flex items-center gap-2 flex-shrink-0">
              {lesson.isRequired && (
                <span className="text-xs text-zinc-600">Requerida</span>
              )}
              {lesson.type === 'quiz' ? (
                <span className="text-xs text-amber-500/70">Próximamente</span>
              ) : (
                <svg className="w-4 h-4 text-zinc-600 group-hover:text-indigo-400 transition-colors" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                </svg>
              )}
            </div>
          </button>
        ))}
      </div>
    </div>
  );
}

// ── Skeleton ──────────────────────────────────────────────────────────────────

function CourseDetailSkeleton() {
  return (
    <div className="min-h-screen bg-[#0a0a0f]">
      <div className="border-b border-white/[0.06] h-10" />
      <div className="max-w-5xl mx-auto px-6 py-10 space-y-8 animate-pulse">
        <div className="flex gap-8">
          <div className="hidden md:block w-56 h-36 rounded-2xl bg-white/[0.04]" />
          <div className="flex-1 space-y-3">
            <div className="h-7 bg-white/[0.04] rounded-lg w-2/3" />
            <div className="h-4 bg-white/[0.04] rounded-lg w-full" />
            <div className="h-4 bg-white/[0.04] rounded-lg w-3/4" />
          </div>
        </div>
        <div className="space-y-2">
          {Array.from({ length: 4 }).map((_, i) => (
            <div key={i} className="h-14 rounded-xl bg-white/[0.04]" />
          ))}
        </div>
      </div>
    </div>
  );
}
