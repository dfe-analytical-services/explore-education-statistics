import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import PublicationForm from '@admin/pages/publication/components/PublicationForm';
import {
  dashboardRoute,
  PublicationRouteParams,
  ThemeTopicParams,
} from '@admin/routes/routes';
import publicationService from '@admin/services/publicationService';
import appendQuery from '@admin/utils/url/appendQuery';
import LoadingSpinner from '@common/components/LoadingSpinner';
import LegacyReleasesTable from '@admin/pages/publication/components/LegacyReleasesTable';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { RouteComponentProps, useHistory } from 'react-router';

const PublicationEditPage = ({
  match,
}: RouteComponentProps<PublicationRouteParams>) => {
  const { publicationId } = match.params;

  const history = useHistory();

  const { value: publication, isLoading } = useAsyncHandledRetry(
    () => publicationService.getPublication(publicationId),
    [publicationId],
  );

  if (isLoading) {
    return <LoadingSpinner />;
  }

  if (!publication) {
    return null;
  }
  const { contact } = publication;

  return (
    <Page
      title="Manage publication"
      caption={publication.title}
      breadcrumbs={[{ name: 'Manage publication' }]}
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-three-quarters">
          <PublicationForm
            cancelButton={
              <Link
                unvisited
                to={appendQuery<ThemeTopicParams>(dashboardRoute.path, {
                  themeId: publication.themeId,
                  topicId: publication.topicId,
                })}
              >
                Cancel
              </Link>
            }
            initialValues={{
              title: publication.title,
              topicId: publication.topicId,
              teamName: contact?.teamName ?? '',
              teamEmail: contact?.teamEmail ?? '',
              contactName: contact?.contactName ?? '',
              contactTelNo: contact?.contactTelNo ?? '',
            }}
            onSubmit={async ({
              teamName,
              teamEmail,
              contactName,
              contactTelNo,
              ...values
            }) => {
              const updatedPublication = await publicationService.updatePublication(
                publication.id,
                {
                  ...values,
                  topicId: values.topicId ?? publication?.topicId,
                  contact: {
                    teamName,
                    teamEmail,
                    contactName,
                    contactTelNo,
                  },
                },
              );

              history.push(
                appendQuery<ThemeTopicParams>(dashboardRoute.path, {
                  themeId: updatedPublication.themeId,
                  topicId: updatedPublication.topicId,
                }),
              );
            }}
            confirmOnSubmit
          />
          <h2>Manage legacy releases</h2>
          <LegacyReleasesTable publication={publication} />
        </div>
      </div>
    </Page>
  );
};

export default PublicationEditPage;
