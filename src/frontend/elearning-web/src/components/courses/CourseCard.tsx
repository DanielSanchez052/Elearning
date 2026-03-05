import { Card } from '../ui/Card';
import { Button } from '../ui/Button';

interface CourseCardProps {
  id: string;
  title: string;
  description: string;
  thumbnail?: string;
  instructor: string;
}

export const CourseCard = ({ 
  id, 
  title, 
  description, 
  thumbnail, 
  instructor 
}: CourseCardProps) => {
  return (
    <Card>
      {thumbnail && (
        <img 
          src={thumbnail} 
          alt={title} 
          className="w-full h-40 object-cover rounded mb-4"
        />
      )}
      <h3 className="text-xl font-bold mb-2">{title}</h3>
      <p className="text-gray-600 mb-2">{description}</p>
      <p className="text-sm text-gray-500 mb-4">Por: {instructor}</p>
      <Button variant="primary" size="sm">Ver Curso</Button>
    </Card>
  );
};
