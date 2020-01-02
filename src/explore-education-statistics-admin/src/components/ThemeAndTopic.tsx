import ThemeAndTopicContext, {
  SetThemeAndTopicCallbackParameters,
} from '@admin/components/ThemeAndTopicContext';
import { IdTitlePair } from '@admin/services/common/types';
import React, { ReactNode, useState } from 'react';

interface ThemeAndTopicProps {
  children: ReactNode;
}

export interface ThemeAndTopicsIdsAndTitles extends IdTitlePair {
  topics: IdTitlePair[];
}

interface Model {
  theme: ThemeAndTopicsIdsAndTitles;
  topic: IdTitlePair;
}

const ThemeAndTopic = ({ children }: ThemeAndTopicProps) => {
  const [model, setModel] = useState<Model>();

  const onThemeAndTopicChange = (params: SetThemeAndTopicCallbackParameters) =>
    setModel(params);

  const contextValue = model
    ? {
        setSelectedThemeAndTopic: onThemeAndTopicChange,
        selectedThemeAndTopic: {
          theme: model.theme,
          topic: model.topic,
        },
      }
    : {
        setSelectedThemeAndTopic: onThemeAndTopicChange,
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
      };

  return (
    <>
      <ThemeAndTopicContext.Provider value={contextValue}>
        {children}
      </ThemeAndTopicContext.Provider>
    </>
  );
};

export default ThemeAndTopic;
