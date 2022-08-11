import ButtonLink from '@admin/components/ButtonLink';
import ReleaseContributorTable from '@admin/pages/publication/components/ReleaseContributorTable';
import { getReleaseApprovalStatusLabel } from '@admin/pages/release/utils/releaseSummaryUtil';
import {
  PublicationTeamRouteParams,
  publicationManageReleaseContributorsPageRoute,
} from '@admin/routes/publicationRoutes';
import { ReleaseListItem } from '@admin/services/releaseService';
import releasePermissionService, {
  ContributorViewModel,
  ContributorInvite,
} from '@admin/services/releasePermissionService';
import userService from '@admin/services/userService';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tag from '@common/components/Tag';
import WarningMessage from '@common/components/WarningMessage';
import React, { ReactNode } from 'react';
import { generatePath } from 'react-router-dom';

interface Props {
  addUserPath?: string; // TODO EES-3217 remove when pages go live
  heading?: ReactNode; // TODO EES-3217 remove when pages go live
  publicationId: string;
  release: ReleaseListItem;
}

const PublicationManageTeamAccess = ({
  addUserPath,
  heading,
  publicationId,
  release,
}: Props) => {
  const {
    value,
    isLoading,
    setState: setContributorsAndInvites,
  } = useAsyncHandledRetry<{
    contributors: ContributorViewModel[];
    invites: ContributorInvite[];
  }>(async () => {
    const [contributors, invites] = await Promise.all([
      releasePermissionService.listReleaseContributors(release.id),
      releasePermissionService.listReleaseContributorInvites(release.id),
    ]);
    return { contributors, invites };
  }, [release]);

  const handleUserRemove = async (userId: string) => {
    await releasePermissionService.removeAllUserContributorPermissionsForPublication(
      publicationId,
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
    await userService.removeContributorReleaseInvites(email, publicationId);
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

  const { contributors = [], invites = [] } = value ?? {};

  return (
    <>
      {heading ?? (
        <h3>
          {`${release.title}${!release.live ? ' (Not live)' : ''}`}
          <Tag
            className="govuk-!-margin-left-2"
            colour={release.approvalStatus === 'Approved' ? 'green' : undefined}
          >
            {getReleaseApprovalStatusLabel(release.approvalStatus)}
          </Tag>
        </h3>
      )}

      {contributors.length === 0 && invites.length === 0 ? (
        <WarningMessage>
          There are no contributors or pending contributor invites for this
          release.
        </WarningMessage>
      ) : (
        <ReleaseContributorTable
          contributors={contributors}
          invites={invites}
          onUserRemove={handleUserRemove}
          onUserInvitesRemove={handleUserInvitesRemove}
        />
      )}
      <ButtonLink
        to={
          addUserPath ??
          generatePath<PublicationTeamRouteParams>(
            publicationManageReleaseContributorsPageRoute.path,
            {
              publicationId,
              releaseId: release.id,
            },
          )
        }
      >
        Add or remove users
      </ButtonLink>
    </>
  );
};

export default PublicationManageTeamAccess;
