import ButtonLink from '@admin/components/ButtonLink';
import UserReleaseRoleTable from '@admin/pages/publication/components/ReleaseUserTable';
import { getReleaseApprovalStatusLabel } from '@admin/pages/release/utils/releaseSummaryUtil';
import {
  PublicationTeamRouteParams,
  publicationManageReleaseContributorsPageRoute,
} from '@admin/routes/publicationRoutes';
import { ReleaseSummary } from '@admin/services/releaseService';
import releasePermissionService, {
  UserReleaseRole,
  UserReleaseInvite,
} from '@admin/services/releasePermissionService';
import userService from '@admin/services/userService';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tag from '@common/components/Tag';
import WarningMessage from '@common/components/WarningMessage';
import React from 'react';
import { generatePath } from 'react-router-dom';

interface Props {
  publicationId: string;
  release: ReleaseSummary;
}

const PublicationManageTeamAccess = ({ publicationId, release }: Props) => {
  const {
    value,
    isLoading,
    setState: setUsersAndInvites,
  } = useAsyncHandledRetry<{
    approvers: UserReleaseRole[];
    approverInvites: UserReleaseInvite[];
    contributors: UserReleaseRole[];
    contributorInvites: UserReleaseInvite[];
  }>(async () => {
    const [roles, invites] = await Promise.all([
      releasePermissionService.listRoles(release.id),
      releasePermissionService.listInvites(release.id),
    ]);
    return {
      approvers: roles.filter(role => role.role === 'Approver'),
      approverInvites: invites.filter(invite => invite.role === 'Approver'),
      contributors: roles.filter(role => role.role === 'Contributor'),
      contributorInvites: invites.filter(
        invite => invite.role === 'Contributor',
      ),
    };
  }, [release]);

  const handleUserRemove = async (userId: string) => {
    await releasePermissionService.removeAllUserContributorPermissionsForPublication(
      publicationId,
      userId,
    );
    setUsersAndInvites({
      value: {
        contributors: contributors.filter(c => c.userId !== userId),
        contributorInvites,
        approvers,
        approverInvites,
      },
    });
  };

  const handleUserInvitesRemove = async (email: string) => {
    await userService.removeContributorReleaseInvites(email, publicationId);
    setUsersAndInvites({
      value: {
        contributors,
        contributorInvites: contributorInvites.filter(i => i.email !== email),
        approvers,
        approverInvites,
      },
    });
  };

  if (isLoading) {
    return <LoadingSpinner />;
  }

  const {
    approvers = [],
    approverInvites = [],
    contributors = [],
    contributorInvites = [],
  } = value ?? {};

  return (
    <>
      <h3>
        {`${release.title}${!release.live ? ' (Not live)' : ''}`}
        <Tag
          className="govuk-!-margin-left-2"
          colour={release.approvalStatus === 'Approved' ? 'green' : undefined}
        >
          {getReleaseApprovalStatusLabel(release.approvalStatus)}
        </Tag>
      </h3>

      <h4>Release approvers</h4>
      {approvers.length === 0 && approverInvites.length === 0 ? (
        <WarningMessage>
          There are no approvers or pending approver invites for this release.
        </WarningMessage>
      ) : (
        <UserReleaseRoleTable users={approvers} invites={approverInvites} />
      )}

      <h4>Release contributors</h4>
      {contributors.length === 0 && contributorInvites.length === 0 ? (
        <WarningMessage>
          There are no contributors or pending contributor invites for this
          release.
        </WarningMessage>
      ) : (
        <UserReleaseRoleTable
          users={contributors}
          invites={contributorInvites}
          onUserRemove={handleUserRemove}
          onUserInvitesRemove={handleUserInvitesRemove}
        />
      )}
      <ButtonLink
        to={generatePath<PublicationTeamRouteParams>(
          publicationManageReleaseContributorsPageRoute.path,
          {
            publicationId,
            releaseId: release.id,
          },
        )}
      >
        Add or remove users
      </ButtonLink>
    </>
  );
};

export default PublicationManageTeamAccess;
