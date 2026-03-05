export interface Quiz {
  id: string;
  lessonId: string;
  questions: QuizQuestion[];
  passingScore: number;
}

export interface QuizQuestion {
  id: string;
  quizId: string;
  text: string;
  type: 'MULTIPLE_CHOICE' | 'TRUE_FALSE' | 'SHORT_ANSWER';
  options: QuizOption[];
  correctOptionId?: string;
  order: number;
}

export interface QuizOption {
  id: string;
  questionId: string;
  text: string;
  isCorrect: boolean;
  order: number;
}

export interface QuizResult {
  id: string;
  userId: string;
  quizId: string;
  score: number;
  passed: boolean;
  submittedAt: string;
  answers: QuizAnswer[];
}

export interface QuizAnswer {
  questionId: string;
  selectedOptionId?: string;
  selectedText?: string;
  isCorrect: boolean;
}
