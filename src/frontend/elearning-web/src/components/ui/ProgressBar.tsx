interface ProgressBarProps {
  progress: number;
  label?: string;
  color?: 'blue' | 'green' | 'red' | 'yellow';
}

const colorClasses = {
  blue: 'bg-blue-500',
  green: 'bg-green-500',
  red: 'bg-red-500',
  yellow: 'bg-yellow-500',
};

export const ProgressBar = ({ progress, label, color = 'blue' }: ProgressBarProps) => {
  const percentage = Math.min(Math.max(progress, 0), 100);

  return (
    <div>
      {label && (
        <div className="flex justify-between mb-2">
          <span className="text-sm font-semibold">{label}</span>
          <span className="text-sm font-semibold">{percentage}%</span>
        </div>
      )}
      <div className="w-full bg-gray-300 rounded-full h-3 overflow-hidden">
        <div
          className={`h-full rounded-full transition-all duration-300 ${colorClasses[color]}`}
          style={{ width: `${percentage}%` }}
        ></div>
      </div>
    </div>
  );
};
