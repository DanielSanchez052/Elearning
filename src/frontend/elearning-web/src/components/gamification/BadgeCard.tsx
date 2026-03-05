interface BadgeCardProps {
  name: string;
  description: string;
  icon: string;
  earnedAt?: string;
}

export const BadgeCard = ({ name, description, icon, earnedAt }: BadgeCardProps) => {
  return (
    <div className="flex flex-col items-center p-4 border rounded text-center">
      <div className="text-6xl mb-2">{icon}</div>
      <h3 className="font-bold text-lg mb-1">{name}</h3>
      <p className="text-sm text-gray-600 mb-2">{description}</p>
      {earnedAt && (
        <p className="text-xs text-green-600">Obtenido: {new Date(earnedAt).toLocaleDateString()}</p>
      )}
    </div>
  );
};
