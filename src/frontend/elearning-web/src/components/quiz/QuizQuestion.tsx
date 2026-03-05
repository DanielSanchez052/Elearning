interface QuizQuestionProps {
  text: string;
  options: Array<{ id: string; text: string }>;
  onAnswer: (optionId: string) => void;
  selectedOption?: string;
}

export const QuizQuestion = ({ 
  text, 
  options, 
  onAnswer, 
  selectedOption 
}: QuizQuestionProps) => {
  return (
    <div className="border rounded p-6 bg-white">
      <h3 className="text-xl font-bold mb-4">{text}</h3>
      <div className="space-y-3">
        {options.map((option) => (
          <label key={option.id} className="flex items-center p-3 border rounded hover:bg-gray-100 cursor-pointer">
            <input
              type="radio"
              name="quiz-option"
              value={option.id}
              checked={selectedOption === option.id}
              onChange={() => onAnswer(option.id)}
              className="mr-3"
            />
            <span>{option.text}</span>
          </label>
        ))}
      </div>
    </div>
  );
};
