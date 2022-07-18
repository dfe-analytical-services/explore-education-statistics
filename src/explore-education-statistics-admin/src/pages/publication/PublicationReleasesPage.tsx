import ButtonLink from '@admin/components/ButtonLink';
import PublicationDraftReleases from '@admin/pages/publication/components/PublicationDraftReleases';
import PublicationPublishedReleases from '@admin/pages/publication/components/PublicationPublishedReleases';
import PublicationScheduledReleases from '@admin/pages/publication/components/PublicationScheduledReleases';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import { PublicationRouteParams } from '@admin/routes/publicationRoutes';
import { releaseCreateRoute } from '@admin/routes/routes';
import WarningMessage from '@common/components/WarningMessage';
import React from 'react';
import { generatePath } from 'react-router';

const PublicationReleasesPage = () => {
  const {
    draftReleases,
    publicationId,
    publication,
    publishedReleases,
    scheduledReleases,
  } = usePublicationContext();

  return (
    <>
      <h2>Manage releases</h2>
      <div className="govuk-grid-row  govuk-!-margin-bottom-6">
        <div className="govuk-grid-column-three-quarters">
          <p>View, edit or amend releases contained within this publication.</p>
        </div>

        {publication.permissions.canCreateReleases && (
          <div className="govuk-grid-column-one-quarter dfe-align--right">
            <ButtonLink
              className="govuk-!-margin-bottom-0"
              to={generatePath<PublicationRouteParams>(
                releaseCreateRoute.path,
                {
                  publicationId,
                },
              )}
              data-testid={`Create new release link for ${publication.title}`}
            >
              Create new release
            </ButtonLink>
          </div>
        )}
      </div>

      {publication ? (
        <>
          {publication.releases.length > 0 ? (
            <>
              {scheduledReleases.length > 0 && <PublicationScheduledReleases />}

              {draftReleases.length > 0 && <PublicationDraftReleases />}

              {publishedReleases.length > 0 && <PublicationPublishedReleases />}
            </>
          ) : (
            <p>There are no releases for this publication yet.</p>
          )}
        </>
      ) : (
        <WarningMessage>
          There was a problem loading this publication.
        </WarningMessage>
      )}
    </>
  );
};

export default PublicationReleasesPage;
