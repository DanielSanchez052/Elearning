interface QuizResultProps {
  score: number;
  passingScore: number;
  passed: boolean;
  correctAnswers: number;
  totalQuestions: number;
  onRetry?: () => void;
  onContinue?: () => void;
}

export const QuizResult = ({
  score,
  passingScore,
  passed,
  correctAnswers,
  totalQuestions,
  onRetry,
  onContinue,
}: QuizResultProps) => {
  const percentage = Math.round((score / 100) * 100);

  return (
    <div className="text-center py-8">
      <div className={`text-6xl mb-4 ${passed ? '✓' : '✕'}`}>
        {passed ? '🎉' : '😔'}
      </div>

      <h2 className={`text-3xl font-bold mb-2 ${passed ? 'text-green-600' : 'text-red-600'}`}>
        {passed ? '¡Aprobaste!' : 'No aprobaste'}
      </h2>

      <p className="text-xl mb-6">
        Puntuación: <span className="font-bold">{percentage}%</span>
      </p>
      <p className="text-sm text-gray-500 mb-4">
        Puntaje mínimo requerido: {passingScore}%
      </p>

      <div className="mb-6 max-w-xs mx-auto">
        <p className="text-gray-600 mb-2">
          {correctAnswers} de {totalQuestions} respuestas correctas
        </p>
        <div className="w-full bg-gray-300 rounded h-4">
          <div
            className={`h-4 rounded ${passed ? 'bg-green-500' : 'bg-red-500'}`}
            style={{ width: `${percentage}%` }}
          ></div>
        </div>
      </div>

      <div className="space-x-4">
        {onRetry && !passed && (
          <button
            onClick={onRetry}
            className="px-6 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
          >
            Reintentar
          </button>
        )}
        {onContinue && (
          <button
            onClick={onContinue}
            className="px-6 py-2 bg-green-500 text-white rounded hover:bg-green-600"
          >
            Continuar
          </button>
        )}
      </div>
    </div>
  );
};
