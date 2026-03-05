import { useParams } from 'react-router-dom';

export const LessonPage = () => {
  const { courseId, lessonId } = useParams<{ courseId: string; lessonId: string }>();

  return (
    <div className="p-8">
      <h1 className="text-4xl font-bold mb-4">Lección</h1>
      <div className="grid grid-cols-3 gap-4">
        <div className="col-span-2">
          {/* Contenido de la lección */}
          <div className="bg-white rounded shadow p-4">
            Contenido de la lección aquí
          </div>
        </div>
        <div>
          {/* Barra lateral con lecciones */}
          <div className="bg-gray-100 rounded p-4">
            Lecciones relacionadas
          </div>
        </div>
      </div>
    </div>
  );
};
