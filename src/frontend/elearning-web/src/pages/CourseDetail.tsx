import { useParams } from 'react-router-dom';
import { useCourseById, useCourseProgress } from '../hooks/useCourses';

export const CourseDetailPage = () => {
  const { courseId } = useParams<{ courseId: string }>();
  const { data: course, isLoading: courseLoading } = useCourseById(courseId!);
  const { data: progress } = useCourseProgress(courseId!);

  if (courseLoading) return <div className="p-8">Cargando...</div>;

  return (
    <div className="p-8">
      <h1 className="text-4xl font-bold mb-4">{course?.title}</h1>
      <p className="text-xl text-gray-600 mb-4">{course?.description}</p>

      <div className="bg-gray-100 p-4 rounded mb-4">
        <h2 className="font-bold">Progreso: {progress?.progress || 0}%</h2>
        <div className="w-full bg-gray-300 rounded mt-2 h-4">
          <div
            className="bg-blue-500 h-4 rounded"
            style={{ width: `${progress?.progress || 0}%` }}
          ></div>
        </div>
      </div>
    </div>
  );
};
