// Quiz Types - Backend Structure
export const QuizType = {
  PerLesson: 0,
  CourseExam: 1,
} as const;

export type QuizType = typeof QuizType[keyof typeof QuizType];

export interface QuizQuestion {
  id: string;
  questionText: string;
  type: QuizType;
  isRequired: boolean;
  passScore: number;
  maxAttempts: number;
  orderIndex: number;
  lessonId?: string | null;
  courseId?: string | null;
  options: QuizOption[];
}

export interface QuizOption {
  id: string;
  questionId?: string;
  optionText: string;
  isCorrect?: boolean; // No viene en respuestas GET (seguridad)
  orderIndex: number;
}

// Admin-only response shapes — the admin listing endpoints DO include
// isCorrect (content-authoring staff, not students), unlike QuizQuestion/QuizOption above.
export interface QuizOptionAdmin extends Omit<QuizOption, 'isCorrect'> {
  isCorrect: boolean;
}

export interface QuizQuestionAdmin extends Omit<QuizQuestion, 'options'> {
  options: QuizOptionAdmin[];
}

// Requests
export interface CreateQuizQuestionRequest {
  lessonId?: string | null;
  courseId?: string | null;
  type: QuizType;
  questionText: string;
  passScore: number;
  maxAttempts: number;
  isRequired: boolean;
}

export interface UpdateQuizQuestionRequest {
  questionText: string;
  passScore: number;
  maxAttempts: number;
  isRequired: boolean;
}

export interface CreateQuizOptionRequest {
  optionText: string;
  isCorrect: boolean;
  orderIndex: number;
}

export interface UpdateQuizOptionRequest {
  optionText: string;
  isCorrect: boolean;
}

// Responses
export interface QuizResultDto {
  score: number;
  isPassed: boolean;
  passScore: number;
  totalQuestions: number;
  correctAnswers: number;
  attemptNumber: number;
  maxAttempts: number;
  feedback: string;
  completedAt: string;
}

export interface QuizAttemptDto {
  attemptNumber: number;
  score: number;
  isPassed: boolean;
  completedAt: string;
}

export interface SubmitQuizRequest {
  lessonId?: string | null;
  courseId?: string | null;
  selectedOptionIds: string[];
}
