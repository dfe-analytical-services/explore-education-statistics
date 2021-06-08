import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import PublicationForm from '@admin/pages/publication/components/PublicationForm';
import {
  dashboardRoute,
  legacyReleasesRoute,
  PublicationRouteParams,
  ThemeTopicParams,
} from '@admin/routes/routes';
import publicationService from '@admin/services/publicationService';
import appendQuery from '@admin/utils/url/appendQuery';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { generatePath, RouteComponentProps, useHistory } from 'react-router';
import RelatedInformation from '@common/components/RelatedInformation';

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
          />
        </div>
        <div className="govuk-grid-column-one-quarter">
          <RelatedInformation heading="Further actions">
            <ul className="govuk-list">
              <li>
                <Link
                  data-testid={`Legacy releases link for ${publication.title}`}
                  to={generatePath(legacyReleasesRoute.path, {
                    publicationId,
                  })}
                >
                  Manage legacy releases
                </Link>
              </li>
            </ul>
          </RelatedInformation>
        </div>
      </div>
    </Page>
  );
};

export default PublicationEditPage;
