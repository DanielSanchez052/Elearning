import { create } from 'zustand';

interface QuizSessionState {
  selectedByQuestion: Record<string, string>;
  durationSec: number;
  timeLeftSec: number;
  isRunning: boolean;
  startSession: (durationSec: number) => void;
  resetSession: () => void;
  tick: () => void;
  setAnswer: (questionId: string, optionId: string) => void;
  stop: () => void;
}

const initialState = {
  selectedByQuestion: {},
  durationSec: 0,
  timeLeftSec: 0,
  isRunning: false,
};

export const useQuizSessionStore = create<QuizSessionState>((set) => ({
  ...initialState,

  startSession: (durationSec) =>
    set({
      selectedByQuestion: {},
      durationSec,
      timeLeftSec: durationSec,
      isRunning: true,
    }),

  resetSession: () => set({ ...initialState }),

  tick: () =>
    set((state) => {
      if (!state.isRunning || state.timeLeftSec <= 0) return state;
      const nextTime = state.timeLeftSec - 1;
      return {
        ...state,
        timeLeftSec: nextTime,
        isRunning: nextTime > 0,
      };
    }),

  setAnswer: (questionId, optionId) =>
    set((state) => ({
      ...state,
      selectedByQuestion: {
        ...state.selectedByQuestion,
        [questionId]: optionId,
      },
    })),

  stop: () => set((state) => ({ ...state, isRunning: false })),
}));
