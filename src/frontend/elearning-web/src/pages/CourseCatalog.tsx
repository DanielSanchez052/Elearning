import { useCourses } from '../hooks/useCourses';

export const CourseCatalogPage = () => {
  const { data: courses, isLoading, error } = useCourses();

  if (isLoading) return <div className="p-8">Cargando cursos...</div>;
  if (error) return <div className="p-8">Error cargando cursos</div>;

  return (
    <div className="p-8">
      <h1 className="text-4xl font-bold mb-4">Catálogo de Cursos</h1>
      <div className="grid grid-cols-3 gap-4">
        {courses?.map((course: any) => (
          <div key={course.id} className="border rounded p-4 shadow">
            <h2 className="text-xl font-bold">{course.title}</h2>
            <p className="text-gray-600">{course.description}</p>
            <button className="mt-4 bg-blue-500 text-white px-4 py-2 rounded">
              Ver Curso
            </button>
          </div>
        ))}
      </div>
    </div>
  );
};
