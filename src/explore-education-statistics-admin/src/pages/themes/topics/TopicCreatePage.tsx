import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import TopicForm from '@admin/pages/themes/topics/components/TopicForm';
import { ThemeParams, themesRoute } from '@admin/routes/routes';
import themeService from '@admin/services/themeService';
import topicService from '@admin/services/topicService';
import appendQuery from '@common/utils/url/appendQuery';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import { useHistory, useParams } from 'react-router';
import React from 'react';

const TopicCreatePage = () => {
  const { themeId } = useParams<ThemeParams>();
  const history = useHistory();

  const { value: theme, isLoading } = useAsyncHandledRetry(
    () => themeService.getTheme(themeId),
    [themeId],
  );

  if (isLoading) {
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
      title="Create topic"
      caption={theme.title}
      breadcrumbs={[
        { name: 'Manage themes and topics', link: themesPath },
        { name: 'Create topic' },
      ]}
    >
      <LoadingSpinner loading={isLoading}>
        <TopicForm
          cancelButton={
            <Link unvisited to={themesPath}>
              Cancel
            </Link>
          }
          onSubmit={async values => {
            await topicService.createTopic({
              ...values,
              themeId: theme.id,
            });

            history.push(themesPath);
          }}
        />
      </LoadingSpinner>
    </Page>
  );
};

export default TopicCreatePage;
