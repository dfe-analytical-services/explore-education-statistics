import ReleaseContributorPermissions from '@admin/pages/publication/components/ReleaseContributorPermissions';
import React from 'react';
import ButtonLink from '@admin/components/ButtonLink';
import { generatePath } from 'react-router-dom';
import { publicationReleaseContributorsRoute } from '@admin/routes/routes';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import releasePermissionService from '@admin/services/releasePermissionService';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import LoadingSpinner from '@common/components/LoadingSpinner';
import { ReleaseSummary } from '@admin/services/releaseService';
import { BasicPublicationDetails } from '@admin/services/publicationService';
import WarningMessage from '@common/components/WarningMessage';

export interface Props {
  publication: BasicPublicationDetails;
  release: ReleaseSummary;
}

const PublicationManageTeamAccessTab = ({ publication, release }: Props) => {
  const {
    value: contributors = [],
    isLoading,
    setState: setContributors,
  } = useAsyncHandledRetry(
    async () => releasePermissionService.listReleaseContributors(release.id),
    [publication, release],
  );

  const handleUserRemove = async (userId: string) => {
    await releasePermissionService.removeAllUserContributorPermissionsForPublication(
      publication.id,
      userId,
    );
    setContributors({
      value: contributors.filter(c => c.userId !== userId),
    });
  };

  if (isLoading) {
    return <LoadingSpinner />;
  }

  return (
    <>
      <h2>Update access for release ({release.title})</h2>
      {!contributors.length ? (
        <WarningMessage>
          There are no contributors for this release.
        </WarningMessage>
      ) : (
        <ReleaseContributorPermissions
          contributors={contributors}
          onUserRemove={handleUserRemove}
        />
      )}
      <ButtonLink
        to={generatePath<ReleaseRouteParams>(
          publicationReleaseContributorsRoute.path,
          {
            publicationId: publication.id,
            releaseId: release.id,
          },
        )}
      >
        Add or remove users
      </ButtonLink>
    </>
  );
};

export default PublicationManageTeamAccessTab;
