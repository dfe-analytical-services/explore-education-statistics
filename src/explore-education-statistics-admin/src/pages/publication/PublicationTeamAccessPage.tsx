import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import {
  publicationInviteUsersPageRoute,
  publicationTeamAccessRoute,
  PublicationTeamRouteParams,
} from '@admin/routes/publicationRoutes';
import publicationService, {
  PublicationPermissions,
  ReleaseVersionsType,
} from '@admin/services/publicationService';
import { ReleaseVersionSummary } from '@admin/services/releaseVersionService';
import { FormSelect } from '@common/components/form';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React, { useState } from 'react';
import { RouteComponentProps } from 'react-router';
import { generatePath, useHistory } from 'react-router-dom';
import { UserPublicationRole } from '@admin/services/userService';
import orderBy from 'lodash/orderBy';
import ButtonLink from '@admin/components/ButtonLink';
import PublicationReleaseAccess from '@admin/pages/publication/components/PublicationReleaseAccess';
import publicationRoleDisplayName from '@admin/utils/publicationRoleDisplayName';

interface Model {
  releases: ReleaseVersionSummary[];
  publicationRoles: UserPublicationRole[];
  publicationOwners: UserPublicationRole[];
  publicationApprovers: UserPublicationRole[];
  permissions: PublicationPermissions;
}

const PublicationTeamAccessPage = ({
  match,
}: RouteComponentProps<PublicationTeamRouteParams>) => {
  const history = useHistory();
  const { releaseVersionId } = match.params;
  const { publicationId, permissions } = usePublicationContext();
  const [currentReleaseId, setCurrentReleaseId] = useState(
    releaseVersionId ?? '',
  );

  const { value: model, isLoading } = useAsyncHandledRetry<Model>(async () => {
    const { results: releases } = await publicationService.listReleaseVersions(
      publicationId,
      {
        versionsType: ReleaseVersionsType.Latest,
      },
    );
    const publicationRoles = await publicationService.listRoles(publicationId);

    if (!releaseVersionId && releases.length) {
      setCurrentReleaseId(releases[0].id);

      history.replace(
        generatePath<PublicationTeamRouteParams>(
          publicationTeamAccessRoute.path,
          {
            publicationId,
            releaseVersionId: releases[0].id,
          },
        ),
      );
    }

    return {
      releases,
      publicationRoles,
      publicationApprovers: publicationRoles.filter(
        publicationRole => publicationRole.role === 'Allower',
      ),
      publicationOwners: publicationRoles.filter(
        publicationRole => publicationRole.role === 'Owner',
      ),
      permissions,
    };
  });

  if (isLoading || !model) {
    return <LoadingSpinner />;
  }

  const currentRelease = model.releases.find(
    release => release.id === currentReleaseId,
  );

  return (
    <>
      <h2>Manage team access</h2>

      <h3>
        {model.permissions.canUpdateContributorReleaseRole
          ? 'Update publication access'
          : 'Publication access'}
      </h3>

      {model.publicationRoles.length ? (
        <>
          <table data-testid="publicationRoles">
            <thead>
              <tr>
                <th scope="col">Name</th>
                <th scope="col">Email</th>
                <th scope="col">Publication role</th>
              </tr>
            </thead>
            <tbody>
              {orderBy(model.publicationRoles, role => [
                role.userName,
                role.role,
              ]).map(role => (
                <tr key={`${role.id}_${role.role}`}>
                  <td>{role.userName}</td>
                  <td>{role.email}</td>
                  <td>
                    {
                      // Temporarily transforming the displayed role name whilst we have the temporary 'Allower'
                      // publication role. Once the new 'Approver' role is introduced in STEP 10 of the permissions
                      // rework, this can be reverted to display the role without transformation.
                      publicationRoleDisplayName(role.role)
                    }
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          <p>
            {model.publicationOwners.length === 0 && (
              <>
                There are no publication owners. To change this please contact{' '}
              </>
            )}
            {model.publicationApprovers.length === 0 && (
              <>
                There are no publication approvers. To change this please
                contact{' '}
              </>
            )}
            {model.publicationOwners.length > 0 &&
              model.publicationApprovers.length > 0 && (
                <>
                  To edit the publication's owners or approvers please contact{' '}
                </>
              )}
            <a href="mailto:explore.statistics@education.gov.uk">
              explore.statistics@education.gov.uk
            </a>
            .
          </p>
        </>
      ) : (
        <p>
          There are no publication owners or approvers. To change this please
          contact{' '}
          <a href="mailto:explore.statistics@education.gov.uk">
            explore.statistics@education.gov.uk
          </a>
          .
        </p>
      )}

      {model.permissions.canUpdateContributorReleaseRole &&
        currentReleaseId !== '' && (
          <ButtonLink
            to={generatePath<PublicationTeamRouteParams>(
              publicationInviteUsersPageRoute.path,
              {
                publicationId,
                releaseVersionId: currentReleaseId,
              },
            )}
          >
            Invite new contributors
          </ButtonLink>
        )}

      {model.permissions.canViewReleaseTeamAccess && (
        <>
          {model?.releases.length ? (
            <>
              <div>
                <h3>
                  {model.permissions.canUpdateContributorReleaseRole
                    ? 'Update release access'
                    : 'Release access'}
                </h3>

                <FormSelect
                  id="currentRelease"
                  name="release"
                  label="Select release"
                  options={model?.releases.map(release => ({
                    label: release.title,
                    value: release.id,
                  }))}
                  order={[]}
                  value={currentReleaseId}
                  onChange={e => {
                    setCurrentReleaseId(e.target.value);
                    history.replace(
                      generatePath<PublicationTeamRouteParams>(
                        publicationTeamAccessRoute.path,
                        {
                          publicationId,
                          releaseVersionId: e.target.value,
                        },
                      ),
                    );
                  }}
                />
              </div>

              {currentRelease && (
                <PublicationReleaseAccess
                  publicationId={publicationId}
                  release={currentRelease}
                  hasReleaseTeamManagementPermission={
                    model.permissions.canUpdateContributorReleaseRole
                  }
                />
              )}
            </>
          ) : (
            <>
              <h3>Update release access</h3>
              <WarningMessage>
                Create a release for this publication to manage release access.
              </WarningMessage>
            </>
          )}
        </>
      )}
    </>
  );
};

export default PublicationTeamAccessPage;
