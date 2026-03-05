import { CourseCard } from './CourseCard';

interface Course {
  id: string;
  title: string;
  description: string;
  thumbnail?: string;
  instructor: string;
}

interface CourseCatalogProps {
  courses: Course[];
  isLoading: boolean;
}

export const CourseCatalog = ({ courses, isLoading }: CourseCatalogProps) => {
  if (isLoading) return <div className="text-center py-8">Cargando cursos...</div>;

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      {courses.map((course) => (
        <CourseCard
          key={course.id}
          id={course.id}
          title={course.title}
          description={course.description}
          thumbnail={course.thumbnail}
          instructor={course.instructor}
        />
      ))}
    </div>
  );
};
