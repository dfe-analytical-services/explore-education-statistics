import React from 'react';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import releasePermissionService from '@admin/services/releasePermissionService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import classNames from 'classnames';
import Button from '@common/components/Button';
import userService from '@admin/services/userService';

export interface Props {
  releaseId: string;
}

const ReleaseContributorPermissions = ({ releaseId }: Props) => {
  const { value: releaseContributors, isLoading } = useAsyncRetry(
    () => releasePermissionService.getReleaseContributors(releaseId),
    [releaseId],
  );

  if (isLoading) {
    return <LoadingSpinner />;
  }

  if (releaseContributors && releaseContributors?.length === 0) {
    return (
      <p>
        No users with contributor access. You can add people on{' '}
        <a href="#inviteNewUsersTab">Invite users tab</a>.
      </p>
    );
  }

  return (
    <table>
      <tbody>
        {releaseContributors!.map(contributor => (
          <tr key={contributor.userId}>
            <td>{contributor.userFullName}</td>
            <td
              className={classNames(
                'govuk-!-margin-0',
                'govuk-tag',
                'dfe-align--centre',
                contributor.releaseRoleId
                  ? 'govuk-tag--grey'
                  : 'govuk-tag--red',
              )}
            >
              {contributor.releaseRoleId ? 'Access Granted' : 'No Access'}
            </td>
            <td>
              {contributor.releaseRoleId ? (
                <Button
                  variant="warning"
                  onClick={() =>
                    userService.removeUserReleaseRole(contributor.releaseRoleId)
                  }
                >
                  Remove access
                </Button>
              ) : (
                <Button
                  variant="secondary"
                  onClick={() =>
                    userService.addUserReleaseRole(contributor.userId, {
                      releaseId: contributor.releaseId,
                      releaseRole: 'Contributor',
                    })
                  }
                >
                  Grant access
                </Button>
              )}
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
};

export default ReleaseContributorPermissions;
