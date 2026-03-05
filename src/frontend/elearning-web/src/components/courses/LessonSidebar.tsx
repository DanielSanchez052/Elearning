interface Lesson {
  id: string;
  title: string;
  type: 'VIDEO' | 'PDF' | 'TEXT';
  completed: boolean;
}

interface LessonSidebarProps {
  lessons: Lesson[];
  currentLessonId?: string;
  onSelectLesson: (lessonId: string) => void;
}

export const LessonSidebar = ({
  lessons,
  currentLessonId,
  onSelectLesson
}: LessonSidebarProps) => {
  return (
    <aside className="bg-gray-50 p-4 rounded border">
      <h3 className="font-bold text-lg mb-4">Lecciones</h3>
      <ul className="space-y-2">
        {lessons.map((lesson) => (
          <li key={lesson.id}>
            <button
              onClick={() => onSelectLesson(lesson.id)}
              className={`w-full text-left p-2 rounded ${currentLessonId === lesson.id
                  ? 'bg-blue-500 text-white'
                  : 'hover:bg-gray-200'
                }`}
            >
              <div className="flex items-center gap-2">
                <span>{lesson.completed ? '✓' : '○'}</span>
                <span className="flex-1">{lesson.title}</span>
                <span className="text-xs">({lesson.type})</span>
              </div>
            </button>
          </li>
        ))}
      </ul>
    </aside>
  );
};
