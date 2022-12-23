import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import {
  publicationInviteUsersPageRoute,
  publicationTeamAccessRoute,
  PublicationTeamRouteParams,
} from '@admin/routes/publicationRoutes';
import publicationService, {
  PublicationPermissions,
  PublicationWithPermissions,
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
import PublicationManageReleaseTeamAccess from '@admin/pages/publication/components/PublicationManageReleaseTeamAccess';

interface Model {
  releases: ReleaseSummary[];
  publicationRoles: UserPublicationRole[];
  permissions: PublicationPermissions;
}

const PublicationTeamAccessPage = ({
  match,
}: RouteComponentProps<PublicationTeamRouteParams>) => {
  const history = useHistory();
  const { releaseId } = match.params;
  const { publicationId } = usePublicationContext();
  const [currentReleaseId, setCurrentReleaseId] = useState<string>(
    releaseId ?? '',
  );

  const { value: model, isLoading } = useAsyncHandledRetry<Model>(async () => {
    const { results: releases } = await publicationService.listReleases(
      publicationId,
    );
    const publicationRoles = await publicationService.listRoles(publicationId);
    const { permissions } = await publicationService.getPublication<
      PublicationWithPermissions
    >(publicationId, true);

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
      permissions,
    };
  });

  if (isLoading || !model) {
    return <LoadingSpinner />;
  }

  const currentRelease = model.releases.find(
    release => release.id === currentReleaseId,
  );

  const canAmendPublicationContributors =
    currentReleaseId != null &&
    model.permissions.canUpdateContributorReleaseRole;
  const canAmendReleaseContributors =
    currentReleaseId != null &&
    model.permissions.canUpdateContributorReleaseRole;

  return (
    <>
      <h2>Manage team access</h2>

      <h3>
        {canAmendPublicationContributors
          ? 'Update publication access'
          : 'Publication access'}
      </h3>

      {model.publicationRoles.length ? (
        <>
          <table>
            <thead>
              <tr>
                <th className="govuk-!-width-one-half">Name</th>
                <th>Publication role</th>
              </tr>
            </thead>
            <tbody>
              {orderBy(model.publicationRoles, role => [
                role.userName,
                role.role,
              ]).map(role => (
                <tr key={`${role.id}_${role.role}`}>
                  <td>{role.userName}</td>
                  <td>{role.role}</td>
                </tr>
              ))}
            </tbody>
          </table>
          <p>
            To request changing the assigned publication roles, contact the
            Explore education statistics team at{' '}
            <a href="mailto:explore.statistics@education.gov.uk">
              explore.statistics@education.gov.uk
            </a>
            .
          </p>
        </>
      ) : (
        <>
          <p>There are no publication roles currently assigned.</p>
          <p>
            To request assigning publication roles, contact the Explore
            education statistics team at{' '}
            <a href="mailto:explore.statistics@education.gov.uk">
              explore.statistics@education.gov.uk
            </a>
            .
          </p>
        </>
      )}

      {canAmendPublicationContributors && (
        <ButtonLink
          to={generatePath<PublicationTeamRouteParams>(
            publicationInviteUsersPageRoute.path,
            {
              publicationId,
              releaseId: currentReleaseId,
            },
          )}
        >
          Add or remove publication contributors
        </ButtonLink>
      )}

      {model.permissions.canViewReleaseTeamAccess && (
        <>
          {model?.releases.length ? (
            <>
              <div className="govuk-grid-row govuk-!-margin-bottom-4 govuk-!-margin-top-8">
                <div className="govuk-grid-column-full">
                  <h3>
                    {canAmendReleaseContributors
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
              </div>

              {currentRelease && (
                <PublicationManageReleaseTeamAccess
                  publicationId={publicationId}
                  release={currentRelease}
                  showManageContributorsButton={canAmendReleaseContributors}
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
