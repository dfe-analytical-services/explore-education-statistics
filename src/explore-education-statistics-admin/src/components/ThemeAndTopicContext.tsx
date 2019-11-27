import * as React from 'react';
import { IdTitlePair } from '@admin/services/common/types';
import { ThemeAndTopics } from '@admin/services/dashboard/types';

interface ThemeAndTopicsIdsAndTitles extends IdTitlePair {
  topics: IdTitlePair[];
}

const themeToThemeWithIdTitleAndTopics = (theme: ThemeAndTopics) => ({
  id: theme.id,
  title: theme.title,
  topics: theme.topics.map(topic => ({
    id: topic.id,
    title: topic.title,
  })),
});

export interface SetThemeAndTopicsCallbackParameters {
  theme: ThemeAndTopicsIdsAndTitles;
  topic: IdTitlePair;
}

interface ThisIsWhatTheContextHasInIt {
  setSelectedThemeAndTopic: (
    callbackProperties: SetThemeAndTopicsCallbackParameters,
  ) => void;
  selectedThemeAndTopic: {
    theme: ThemeAndTopicsIdsAndTitles;
    topic: IdTitlePair;
  };
}

const ThemeAndTopicContext = React.createContext<ThisIsWhatTheContextHasInIt>({
  setSelectedThemeAndTopic: () => {},
  selectedThemeAndTopic: {
    theme: {
      id: '',
      topics: [],
      title: '',
    },
    topic: {
      title: '',
      id: '',
    },
  },
});

export default ThemeAndTopicContext;
