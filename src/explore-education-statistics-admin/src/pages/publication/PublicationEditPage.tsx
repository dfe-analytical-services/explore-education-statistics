import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import PublicationForm from '@admin/pages/publication/components/PublicationForm';
import {
  legacyReleaseCreateRoute,
  legacyReleaseEditRoute,
  LegacyReleaseRouteParams,
} from '@admin/routes/legacyReleaseRoutes';
import {
  dashboardRoute,
  PublicationRouteParams,
  ThemeTopicParams,
} from '@admin/routes/routes';
import publicationService, {
  MyPublication,
} from '@admin/services/publicationService';
import legacyReleaseService, {
  LegacyRelease,
} from '@admin/services/legacyReleaseService';
import appendQuery from '@admin/utils/url/appendQuery';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import LegacyReleasesTable from '@admin/pages/publication/components/LegacyReleasesTable';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { generatePath, RouteComponentProps, useHistory } from 'react-router';

interface Model {
  publication: MyPublication;
  legacyReleases: LegacyRelease[];
}

const PublicationEditPage = ({
  match,
}: RouteComponentProps<PublicationRouteParams>) => {
  const { publicationId } = match.params;

  const history = useHistory();

  const { value: model, isLoading } = useAsyncHandledRetry<Model>(async () => {
    const [publication, legacyReleases] = await Promise.all([
      publicationService.getMyPublication(publicationId),
      legacyReleaseService.listLegacyReleases(publicationId),
    ]);

    return {
      publication,
      legacyReleases,
    };
  }, [publicationId]);

  if (isLoading || !model) {
    return <LoadingSpinner />;
  }

  const { publication, legacyReleases } = model;
  const { contact } = publication;

  return (
    <Page
      title="Manage publication"
      caption={publication.title}
      breadcrumbs={[{ name: 'Manage publication' }]}
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-three-quarters">
          {publication.isSuperseded && (
            <WarningMessage>This publication is archived.</WarningMessage>
          )}
          {publication.supersededById && !publication.isSuperseded && (
            <WarningMessage>
              This publication will be archived when its superseding publication
              has a live release published.
            </WarningMessage>
          )}
          <PublicationForm
            publicationId={publicationId}
            showSupersededBy={
              publication.permissions.canUpdatePublicationSupersededBy
            }
            showTitleInput={publication.permissions.canUpdatePublicationTitle}
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
              summary: publication.summary,
              topicId: publication.topicId,
              teamName: contact.teamName,
              teamEmail: contact.teamEmail,
              contactName: contact.contactName,
              contactTelNo: contact.contactTelNo,
              supersededById: publication.supersededById,
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
          <LegacyReleasesTable
            createRoute={generatePath<LegacyReleaseRouteParams>(
              legacyReleaseCreateRoute.path,
              {
                publicationId: publication.id,
              },
            )}
            editRoute={id => {
              return generatePath<LegacyReleaseRouteParams>(
                legacyReleaseEditRoute.path,
                {
                  publicationId: publication.id,
                  legacyReleaseId: id,
                },
              );
            }}
            legacyReleases={legacyReleases}
            publicationId={publication.id}
          />
        </div>
      </div>
    </Page>
  );
};

export default PublicationEditPage;
