import ReleaseContributorPermissions from '@admin/pages/publication/components/ReleaseContributorPermissions';
import React from 'react';
import ButtonLink from '@admin/components/ButtonLink';
import { generatePath } from 'react-router-dom';
import { releaseManageTeamAccessAddUsersRoute } from '@admin/routes/routes';
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
    value: contributors,
    isLoading,
    setState,
  } = useAsyncHandledRetry(
    async () =>
      releasePermissionService.getPublicationReleaseContributors(
        publication.id,
        release.id,
      ),
    [publication, release],
  );

  if (!contributors || isLoading) {
    return <LoadingSpinner />;
  }

  const handleUserRemoval = async (userId: string) => {
    await releasePermissionService.deleteAllUserContributorReleaseRolesForPublication(
      publication.id,
      userId,
    );
    setState({ value: contributors.filter(c => c.userId !== userId) });
  };

  return (
    <>
      <h2>Update access for release ({release.title})</h2>
      {!contributors?.length ? (
        <WarningMessage>
          There are no contributors for this release.
        </WarningMessage>
      ) : (
        <ReleaseContributorPermissions
          contributors={contributors}
          handleUserRemoval={handleUserRemoval}
        />
      )}
      <ButtonLink
        to={generatePath<ReleaseRouteParams>(
          releaseManageTeamAccessAddUsersRoute.path,
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
