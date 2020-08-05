import dashboardService, {
  Theme,
  Topic,
} from '@admin/services/dashboardService';
import useAsyncHandledRetry, {
  AsyncHandledRetryState,
} from '@common/hooks/useAsyncHandledRetry';
import React, { createContext, ReactNode, useContext } from 'react';

export interface ThemeTopicContextState {
  theme: Theme;
  topic: Topic;
}

const ThemeTopicContext = createContext<ThemeTopicContextState | undefined>(
  undefined,
);

interface ThemeTopicContextProviderProps {
  children:
    | ReactNode
    | ((state: AsyncHandledRetryState<ThemeTopicContextState>) => ReactNode);
  themeId: string;
  topicId: string;
}

export const ThemeTopicContextProvider = ({
  children,
  themeId,
  topicId,
}: ThemeTopicContextProviderProps) => {
  const state = useAsyncHandledRetry<
    ThemeTopicContextState | undefined
  >(async () => {
    const themeAndTopics = await dashboardService.getMyThemesAndTopics();

    const theme = themeAndTopics.find(t => t.id === themeId);
    const topic = theme?.topics?.find(t => t.id === topicId);

    if (!theme || !topic) {
      return undefined;
    }

    return {
      theme,
      topic,
    };
  }, [themeId, topicId]);

  return (
    <ThemeTopicContext.Provider value={state.value}>
      {typeof children === 'function' ? children(state) : children}
    </ThemeTopicContext.Provider>
  );
};

export default function useThemeTopicContext(): ThemeTopicContextState {
  const context = useContext(ThemeTopicContext);

  if (!context) {
    throw new Error('Must have a parent ThemeAndTopicContextProvider');
  }

  return context;
}
