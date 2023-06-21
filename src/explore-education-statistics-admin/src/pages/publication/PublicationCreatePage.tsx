import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import PublicationForm from '@admin/pages/publication/components/PublicationForm';
import { dashboardRoute, ThemeTopicParams } from '@admin/routes/routes';
import publicationService from '@admin/services/publicationService';
import topicService from '@admin/services/topicService';
import appendQuery from '@common/utils/url/appendQuery';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { useHistory, useParams } from 'react-router';

const PublicationCreatePage = () => {
  const { topicId } = useParams<{ topicId: string }>();
  const history = useHistory();

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
        {/* EES-2464
        <div className="govuk-grid-column-one-third">
          <RelatedInformation heading="Help and guidance">
            <ul className="govuk-list">
              <li>
                <Link to="/documentation/create-new-publication" target="blank">
                  Creating a new publication
                </Link>
              </li>
            </ul>
          </RelatedInformation>
        </div> */}
      </div>

      <PublicationForm
        showTitleInput
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
        onSubmit={async ({
          teamName,
          teamEmail,
          contactName,
          contactTelNo,
          ...values
        }) => {
          await publicationService.createPublication({
            ...values,
            topicId: topic.id,
            contact: {
              teamName,
              teamEmail,
              contactName,
              contactTelNo,
            },
          });

          history.push(dashboardRoute.path);
        }}
      />
    </Page>
  );
};

export default PublicationCreatePage;
