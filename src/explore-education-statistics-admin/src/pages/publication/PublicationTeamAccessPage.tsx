import Link from '@admin/components/Link';
import PublicationManageTeamAccess from '@admin/pages/publication/components/PublicationManageTeamAccess';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import {
  PublicationTeamRouteParams,
  publicationInviteUsersPageRoute,
  publicationTeamAccessRoute,
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

const PublicationTeamAccessPage = ({
  match,
}: RouteComponentProps<PublicationTeamRouteParams>) => {
  const history = useHistory();
  const { releaseId } = match.params;
  const { publicationId } = usePublicationContext();
  const [currentReleaseId, setCurrentReleaseId] = useState<string>(
    releaseId ?? '',
  );

  const { value: releases, isLoading } = useAsyncHandledRetry<ReleaseSummary[]>(
    async () => {
      const fetchedReleases = await publicationService.getReleases(
        publicationId,
      );
      if (!fetchedReleases.length) {
        return [];
      }
      if (!releaseId && fetchedReleases.length) {
        setCurrentReleaseId(fetchedReleases[0].id);
        history.replace(
          generatePath<PublicationTeamRouteParams>(
            publicationTeamAccessRoute.path,
            {
              publicationId,
              releaseId: fetchedReleases[0].id,
            },
          ),
        );
      }

      return fetchedReleases;
    },
  );

  const currentRelease = releases?.find(
    release => release.id === currentReleaseId,
  );

  if (isLoading) {
    return <LoadingSpinner />;
  }

  if (!releases?.length) {
    return (
      <>
        <h2>Update release access</h2>
        <WarningMessage>
          Create a release for this publication to manage team access.
        </WarningMessage>
      </>
    );
  }

  return (
    <>
      <div className="govuk-grid-row govuk-!-margin-bottom-8">
        <div className="govuk-grid-column-two-thirds">
          <h2>Update release access</h2>

          <FormSelect
            id="currentRelease"
            name="release"
            label="Select release"
            options={releases?.map(release => ({
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
        <div className="govuk-grid-column-one-third dfe-align--right">
          <h3 className="govuk-!-font-size-19">Other options</h3>
          {currentReleaseId && (
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
          )}
        </div>
      </div>

      {currentRelease && (
        <PublicationManageTeamAccess
          publicationId={publicationId}
          release={currentRelease}
        />
      )}
    </>
  );
};

export default PublicationTeamAccessPage;
