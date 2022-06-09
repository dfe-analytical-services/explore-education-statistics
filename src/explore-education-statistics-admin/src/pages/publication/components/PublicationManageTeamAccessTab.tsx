import ReleaseContributorPermissions from '@admin/pages/publication/components/ReleaseContributorPermissions';
import React from 'react';
import ButtonLink from '@admin/components/ButtonLink';
import { generatePath } from 'react-router-dom';
import { publicationReleaseContributorsRoute } from '@admin/routes/routes';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import releasePermissionService, {
  ContributorViewModel,
  ContributorInvite,
} from '@admin/services/releasePermissionService';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import LoadingSpinner from '@common/components/LoadingSpinner';
import { ReleaseSummary } from '@admin/services/releaseService';
import { BasicPublicationDetails } from '@admin/services/publicationService';
import WarningMessage from '@common/components/WarningMessage';
import userService from '@admin/services/userService';

export interface Props {
  publication: BasicPublicationDetails;
  release: ReleaseSummary;
}

interface Model {
  contributors: ContributorViewModel[];
  invites: ContributorInvite[];
}

const PublicationManageTeamAccessTab = ({ publication, release }: Props) => {
  const {
    value: model = { contributors: [], invites: [] },
    isLoading,
    setState: setContributorsAndInvites,
  } = useAsyncHandledRetry<Model>(async () => {
    const [contributors, invites] = await Promise.all([
      releasePermissionService.listReleaseContributors(release.id),
      releasePermissionService.listReleaseContributorInvites(release.id),
    ]);
    return { contributors, invites };
  }, [publication, release]);

  const handleUserRemove = async (userId: string) => {
    await releasePermissionService.removeAllUserContributorPermissionsForPublication(
      publication.id,
      userId,
    );
    setContributorsAndInvites({
      value: {
        contributors: contributors.filter(c => c.userId !== userId),
        invites,
      },
    });
  };

  const handleUserInvitesRemove = async (email: string) => {
    await userService.removeContributorReleaseInvites(email, publication.id);
    setContributorsAndInvites({
      value: {
        contributors,
        invites: invites.filter(i => i.email !== email),
      },
    });
  };

  if (isLoading) {
    return <LoadingSpinner />;
  }

  const { contributors, invites } = model;

  return (
    <>
      <h2>Update access for release ({release.title})</h2>
      {contributors.length === 0 && invites.length === 0 ? (
        <WarningMessage>
          There are no contributors or pending contributor invites for this
          release.
        </WarningMessage>
      ) : (
        <ReleaseContributorPermissions
          contributors={contributors}
          invites={invites}
          onUserRemove={handleUserRemove}
          onUserInvitesRemove={handleUserInvitesRemove}
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
