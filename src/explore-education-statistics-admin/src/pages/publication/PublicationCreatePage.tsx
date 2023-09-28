import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import PublicationForm from '@admin/pages/publication/components/PublicationForm';
import { dashboardRoute, ThemeTopicParams } from '@admin/routes/routes';
import topicService from '@admin/services/topicService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import appendQuery from '@common/utils/url/appendQuery';
import React from 'react';
import { RouteComponentProps } from 'react-router';

export default function PublicationCreatePage({
  history,
  match,
}: RouteComponentProps<{ topicId: string }>) {
  const { topicId } = match.params;

  const { value: topic, isLoading } = useAsyncHandledRetry(
    () => topicService.getTopic(topicId),
    [topicId],
  );

  if (isLoading) {
    return <LoadingSpinner loading={isLoading} />;
  }

  if (!topic) {
    return null;
  }

  return (
    <Page breadcrumbs={[{ name: 'Create new publication' }]}>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <PageTitle caption={topic.title} title="Create new publication" />
        </div>
      </div>

      <PublicationForm
        cancelButton={
          <Link
            unvisited
            to={appendQuery<ThemeTopicParams>(dashboardRoute.path, {
              themeId: topic.themeId,
              topicId,
            })}
          >
            Cancel
          </Link>
        }
        topicId={topic.id}
        onSubmit={() => history.push(dashboardRoute.path)}
      />
    </Page>
  );
}
