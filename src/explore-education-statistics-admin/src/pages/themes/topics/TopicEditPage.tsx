import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import TopicForm from '@admin/pages/themes/topics/components/TopicForm';
import {
  ThemeParams,
  themesRoute,
  ThemeTopicParams,
} from '@admin/routes/routes';
import themeService from '@admin/services/themeService';
import topicService from '@admin/services/topicService';
import appendQuery from '@common/utils/url/appendQuery';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { useHistory, useParams } from 'react-router';

const TopicEditPage = () => {
  const { themeId, topicId } = useParams<ThemeTopicParams>();
  const history = useHistory();

  const { value: theme, isLoading: isThemeLoading } = useAsyncHandledRetry(
    () => themeService.getTheme(themeId),
    [themeId],
  );

  const { value: topic, isLoading: isTopicLoading } = useAsyncHandledRetry(
    () => topicService.getTopic(topicId),
    [topicId],
  );

  if (isThemeLoading) {
    return <LoadingSpinner />;
  }

  if (!theme) {
    return null;
  }

  const themesPath = appendQuery<ThemeParams>(themesRoute.path, {
    themeId: theme.id,
  });

  return (
    <Page
      title="Edit topic"
      caption={theme.title}
      breadcrumbs={[
        { name: 'Manage themes and topics', link: themesPath },
        { name: 'Edit topic' },
      ]}
    >
      <LoadingSpinner loading={isTopicLoading}>
        {topic && (
          <TopicForm
            initialValues={{
              title: topic?.title,
            }}
            cancelButton={
              <Link unvisited to={themesPath}>
                Cancel
              </Link>
            }
            onSubmit={async values => {
              await topicService.updateTopic(topic.id, {
                ...values,
                themeId: theme.id,
              });
              history.push(themesPath);
            }}
          />
        )}
      </LoadingSpinner>
    </Page>
  );
};

export default TopicEditPage;
