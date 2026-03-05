interface MobileLevelIndicatorProps {
  level: number;
  nextLevelPercentage: number;
  totalPoints: number;
}

export const MobileLevelIndicator = ({
  level,
  nextLevelPercentage,
  totalPoints,
}: MobileLevelIndicatorProps) => {
  return (
    <div className="bg-gradient-to-r from-purple-500 to-pink-500 rounded-lg p-4 text-white">
      <div className="flex justify-between items-center mb-3">
        <div>
          <p className="text-sm opacity-90">Nivel Móvil</p>
          <p className="text-3xl font-bold">{level}</p>
        </div>
        <div className="text-4xl">📱</div>
      </div>

      <div>
        <div className="flex justify-between text-xs mb-1">
          <span>Progreso al siguiente nivel</span>
          <span>{nextLevelPercentage}%</span>
        </div>
        <div className="w-full bg-white bg-opacity-30 rounded-full h-2">
          <div
            className="h-2 bg-white rounded-full transition-all duration-300"
            style={{ width: `${nextLevelPercentage}%` }}
          ></div>
        </div>
      </div>

      <p className="text-xs mt-3">Total de puntos: {totalPoints}</p>
    </div>
  );
};
