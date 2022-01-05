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
    value: contributorsAndInvites = {
      contributors: [],
      pendingInviteEmails: [],
    },
    isLoading,
    setState: setContributorsAndInvites,
  } = useAsyncHandledRetry(
    async () =>
      releasePermissionService.listReleaseContributorsAndInvites(release.id),
    [publication, release],
  );

  const { contributors, pendingInviteEmails } = contributorsAndInvites;

  const handleUserRemove = async (userId: string) => {
    await releasePermissionService.removeAllUserContributorPermissionsForPublication(
      publication.id,
      userId,
    );
    setContributorsAndInvites({
      value: {
        contributors: contributorsAndInvites.contributors.filter(
          c => c.userId !== userId,
        ),
        pendingInviteEmails,
      },
    });
  };

  if (isLoading) {
    return <LoadingSpinner />;
  }

  return (
    <>
      <h2>Update access for release ({release.title})</h2>
      {!contributors.length && !pendingInviteEmails.length ? (
        <WarningMessage>
          There are no contributors or pending contributor invites for this
          release.
        </WarningMessage>
      ) : (
        <ReleaseContributorPermissions
          contributors={contributors}
          pendingInviteEmails={pendingInviteEmails}
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
