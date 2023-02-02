import ButtonLink from '@admin/components/ButtonLink';
import ReleaseUserTable from '@admin/pages/publication/components/ReleaseUserTable';
import { getReleaseApprovalStatusLabel } from '@admin/pages/release/utils/releaseSummaryUtil';
import {
  publicationManageReleaseContributorsPageRoute,
  PublicationTeamRouteParams,
} from '@admin/routes/publicationRoutes';
import { ReleaseSummary } from '@admin/services/releaseService';
import releasePermissionService from '@admin/services/releasePermissionService';
import userService from '@admin/services/userService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tag from '@common/components/Tag';
import WarningMessage from '@common/components/WarningMessage';
import React from 'react';
import { generatePath } from 'react-router-dom';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import { useQuery } from '@tanstack/react-query';
import releasePermissionQueries from '@admin/queries/releasePermissionQueries';

interface Props {
  publicationId: string;
  release: ReleaseSummary;
  hasReleaseTeamManagementPermission: boolean;
}

const PublicationReleaseAccess = ({
  publicationId,
  release,
  hasReleaseTeamManagementPermission = false,
}: Props) => {
  const {
    data: roles = [],
    isLoading: rolesLoading,
    refetch: refetchRoles,
  } = useQuery({
    ...releasePermissionQueries.listRoles(release.id),
  });

  const {
    data: invites = [],
    isLoading: invitesLoading,
    refetch: refetchInvites,
  } = useQuery({
    ...releasePermissionQueries.listInvites(release.id),
  });

  const approvers = roles.filter(role => role.role === 'Approver');
  const approverInvites = invites.filter(invite => invite.role === 'Approver');
  const contributors = roles.filter(role => role.role === 'Contributor');
  const contributorInvites = invites.filter(
    invite => invite.role === 'Contributor',
  );

  const handleUserRemove = hasReleaseTeamManagementPermission
    ? async (userId: string) => {
        await releasePermissionService.removeAllUserContributorPermissionsForPublication(
          publicationId,
          userId,
        );
        await refetchRoles();
      }
    : undefined;

  const handleUserInvitesRemove = hasReleaseTeamManagementPermission
    ? async (email: string) => {
        await userService.removeContributorReleaseInvites(email, publicationId);
        await refetchInvites();
      }
    : undefined;

  return (
    <LoadingSpinner loading={rolesLoading || invitesLoading}>
      <SummaryList className="govuk-!-margin-bottom-8">
        <SummaryListItem term="Release">
          {`${release.title}${!release.live ? ' (Not live)' : ''}`}
        </SummaryListItem>
        <SummaryListItem term="Status">
          <Tag
            colour={release.approvalStatus === 'Approved' ? 'green' : undefined}
          >
            {getReleaseApprovalStatusLabel(release.approvalStatus)}
          </Tag>
        </SummaryListItem>
      </SummaryList>

      <h4 className="govuk-!-margin-bottom-0">Release approvers</h4>
      {approvers.length === 0 && approverInvites.length === 0 ? (
        <WarningMessage>
          There are no approvers or pending approver invites for this release.
        </WarningMessage>
      ) : (
        <ReleaseUserTable
          data-testid="releaseApprovers"
          users={approvers}
          invites={approverInvites}
        />
      )}

      <h4 className="govuk-!-margin-bottom-0">Release contributors</h4>
      {contributors.length === 0 && contributorInvites.length === 0 ? (
        <WarningMessage>
          There are no contributors or pending contributor invites for this
          release.
        </WarningMessage>
      ) : (
        <ReleaseUserTable
          data-testid="releaseContributors"
          users={contributors}
          invites={contributorInvites}
          onUserRemove={handleUserRemove}
          onUserInvitesRemove={handleUserInvitesRemove}
        />
      )}
      {hasReleaseTeamManagementPermission && (
        <ButtonLink
          to={generatePath<PublicationTeamRouteParams>(
            publicationManageReleaseContributorsPageRoute.path,
            {
              publicationId,
              releaseId: release.id,
            },
          )}
        >
          Manage release contributors
        </ButtonLink>
      )}
    </LoadingSpinner>
  );
};

export default PublicationReleaseAccess;
