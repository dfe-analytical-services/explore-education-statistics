import Link from '@admin/components/Link';
import PublicationManageTeamAccess from '@admin/pages/publication/components/PublicationManageTeamAccess';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import {
  publicationInviteUsersPageRoute,
  publicationTeamAccessRoute,
  PublicationTeamRouteParams,
} from '@admin/routes/publicationRoutes';
import publicationService from '@admin/services/publicationService';
import { ReleaseSummary } from '@admin/services/releaseService';
import { FormSelect } from '@common/components/form';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React, { useState } from 'react';
import { RouteComponentProps } from 'react-router';
import { generatePath, useHistory } from 'react-router-dom';

interface Model {
  releases: ReleaseSummary[];
  publicationRoles: {
    id: string;
    userName: string;
    role: string;
  }[];
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
    const publicationRoles = await publicationService.getRoles(publicationId);

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
      publicationRoles: publicationRoles.map(role => ({
        id: role.id,
        userName: role.userName,
        role: role.role,
      })),
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
              {model.publicationRoles.map(role => (
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

      {model?.releases.length ? (
        <>
          <div className="govuk-grid-row govuk-!-margin-bottom-8">
            <div className="govuk-grid-column-two-thirds">
              <h3>Update release access</h3>

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
            {currentReleaseId && (
              <div className="govuk-grid-column-one-third dfe-align--right">
                <h3 className="govuk-!-font-size-19">Other options</h3>
                <Link
                  to={generatePath<PublicationTeamRouteParams>(
                    publicationInviteUsersPageRoute.path,
                    {
                      publicationId,
                      releaseId: currentReleaseId,
                    },
                  )}
                >
                  Invite new users
                </Link>
              </div>
            )}
          </div>

          {currentRelease && (
            <PublicationManageTeamAccess
              publicationId={publicationId}
              release={currentRelease}
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
  );
};

export default PublicationTeamAccessPage;
