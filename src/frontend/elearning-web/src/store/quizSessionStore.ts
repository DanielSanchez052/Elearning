import { create } from 'zustand';

interface StartSessionOptions {
  /**
   * Seconds actually left, when part of the duration was already used
   * (e.g. resuming a course exam started earlier). Defaults to durationSec.
   * When provided, the countdown follows a wall-clock deadline instead of
   * decrementing once per tick, so reloads and throttled background tabs
   * cannot stretch the remaining time.
   */
  remainingSec?: number;
}

interface QuizSessionState {
  selectedByQuestion: Record<string, string>;
  durationSec: number;
  timeLeftSec: number;
  isRunning: boolean;
  deadlineMs: number | null;
  startSession: (durationSec: number, options?: StartSessionOptions) => void;
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
  deadlineMs: null,
};

export const useQuizSessionStore = create<QuizSessionState>((set) => ({
  ...initialState,

  startSession: (durationSec, options) => {
    if (options?.remainingSec === undefined) {
      set({
        selectedByQuestion: {},
        durationSec,
        timeLeftSec: durationSec,
        isRunning: true,
        deadlineMs: null,
      });
      return;
    }

    const remainingSec = Math.max(0, Math.min(durationSec, Math.ceil(options.remainingSec)));
    set({
      selectedByQuestion: {},
      durationSec,
      timeLeftSec: remainingSec,
      isRunning: remainingSec > 0,
      deadlineMs: Date.now() + remainingSec * 1000,
    });
  },

  resetSession: () => set({ ...initialState }),

  tick: () =>
    set((state) => {
      if (!state.isRunning || state.timeLeftSec <= 0) return state;
      const nextTime =
        state.deadlineMs === null
          ? state.timeLeftSec - 1
          : Math.max(0, Math.ceil((state.deadlineMs - Date.now()) / 1000));
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
