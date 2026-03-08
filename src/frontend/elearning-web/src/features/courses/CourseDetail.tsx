import { useParams } from 'react-router-dom';
import { useCourseDetail } from '../../hooks/useCourses';

export const CourseDetail = () => {
  const { courseId } = useParams<{ courseId: string }>();
  const { data: course, isLoading } = useCourseDetail(courseId ?? '');

  if (isLoading) return <div>Cargando...</div>;

  return (
    <div className="space-y-4">
      <h2 className="text-2xl font-bold">{course?.title}</h2>
      <p className="text-gray-600">{course?.description}</p>
      <button className="bg-blue-500 text-white px-4 py-2 rounded">
        Enrollarse
      </button>
    </div>
  );
};
