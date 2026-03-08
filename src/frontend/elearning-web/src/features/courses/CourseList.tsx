import { useCourseCatalog } from '../../hooks/useCourses';

export const CourseList = () => {
  const { data, isLoading, error } = useCourseCatalog();
  const courses = data?.items ?? [];

  if (isLoading) return <div>Cargando cursos...</div>;
  if (error) return <div>Error cargando cursos</div>;

  return (
    <div className="space-y-4">
      {courses?.map((course: any) => (
        <div key={course.id} className="border rounded p-4">
          <h3 className="text-xl font-bold">{course.title}</h3>
          <p className="text-gray-600">{course.description}</p>
        </div>
      ))}
    </div>
  );
};
