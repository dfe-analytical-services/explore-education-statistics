import { ThemeAndTopicsIdsAndTitles } from '@admin/components/ThemeAndTopic';
import { IdTitlePair } from '@admin/services/common/types';
import { createContext } from 'react';

export interface SetThemeAndTopicCallbackParameters {
  theme: ThemeAndTopicsIdsAndTitles;
  topic: IdTitlePair;
}

interface SelectedThemeAndTopic {
  setSelectedThemeAndTopic: (
    callbackProperties: SetThemeAndTopicCallbackParameters,
  ) => void;
  selectedThemeAndTopic: {
    theme: ThemeAndTopicsIdsAndTitles;
    topic: IdTitlePair;
  };
}

const ThemeAndTopicContext = createContext<SelectedThemeAndTopic>({
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
