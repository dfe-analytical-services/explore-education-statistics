import React, { ReactNode, useState } from 'react';
import ThemeAndTopicContext, {
  SetThemeAndTopicsCallbackParameters,
} from '@admin/components/ThemeAndTopicContext';
import { IdTitlePair } from '@admin/services/common/types';
import { ThemeAndTopics } from '@admin/services/dashboard/types';

interface ThemeAndTopicProps {
  children: ReactNode;
}

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

interface Model {
  theme: ThemeAndTopicsIdsAndTitles;
  topic: IdTitlePair;
}

const ThemeAndTopic = ({ children }: ThemeAndTopicProps) => {
  const [model, setModel] = useState<Model>();

  const onThemeAndTopicChange = (params: SetThemeAndTopicsCallbackParameters) =>
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
