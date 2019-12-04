import * as React from 'react';
import { IdTitlePair } from '@admin/services/common/types';

interface ThemeAndTopicsIdsAndTitles extends IdTitlePair {
  topics: IdTitlePair[];
}

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
