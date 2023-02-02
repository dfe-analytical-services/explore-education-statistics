import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import {
  publicationInviteUsersPageRoute,
  publicationTeamAccessRoute,
  PublicationTeamRouteParams,
} from '@admin/routes/publicationRoutes';
import publicationService, {
  PublicationPermissions,
} from '@admin/services/publicationService';
import { ReleaseSummary } from '@admin/services/releaseService';
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

interface Model {
  releases: ReleaseSummary[];
  publicationRoles: UserPublicationRole[];
  publicationOwners: UserPublicationRole[];
  publicationApprovers: UserPublicationRole[];
  permissions: PublicationPermissions;
}

const PublicationTeamAccessPage = ({
  match,
}: RouteComponentProps<PublicationTeamRouteParams>) => {
  const history = useHistory();
  const { releaseId } = match.params;
  const { publicationId, permissions } = usePublicationContext();
  const [currentReleaseId, setCurrentReleaseId] = useState(releaseId ?? '');

  const { value: model, isLoading } = useAsyncHandledRetry<Model>(async () => {
    const { results: releases } = await publicationService.listReleases(
      publicationId,
    );
    const publicationRoles = await publicationService.listRoles(publicationId);

    if (!releaseId && releases.length) {
      setCurrentReleaseId(releases[0].id);

      history.replace(
        generatePath<PublicationTeamRouteParams>(
          publicationTeamAccessRoute.path,
          {
            publicationId,
            releaseId: releases[0].id,
          },
        ),
      );
    }

    return {
      releases,
      publicationRoles,
      publicationApprovers: publicationRoles.filter(
        publicationRole => publicationRole.role === 'Approver',
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
                  <td>{role.role}</td>
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
                releaseId: currentReleaseId,
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
                          releaseId: e.target.value,
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
