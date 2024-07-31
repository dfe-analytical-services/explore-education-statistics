import ButtonLink from '@admin/components/ButtonLink';
import { useLastLocation } from '@admin/contexts/LastLocationContext';
import { useReleaseVersionContext } from '@admin/pages/release/contexts/ReleaseContext';
import {
  ReleaseRouteParams,
  releaseSummaryEditRoute,
} from '@admin/routes/releaseRoutes';
import permissionService from '@admin/services/permissionService';
import releaseService from '@admin/services/releaseService';
import Gate from '@common/components/Gate';
import LoadingSpinner from '@common/components/LoadingSpinner';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import { releaseTypes } from '@common/services/types/releaseType';
import React from 'react';
import { generatePath, useLocation } from 'react-router';

const ReleaseSummaryPage = () => {
  const location = useLocation();
  const lastLocation = useLastLocation();

  const { releaseVersion: contextRelease, releaseVersionId } =
    useReleaseVersionContext();

  const { value: release, isLoading } = useAsyncRetry(
    async () =>
      lastLocation && lastLocation !== location
        ? releaseService.getRelease(releaseVersionId)
        : contextRelease,
    [releaseVersionId],
  );

  return (
    <LoadingSpinner loading={isLoading}>
      <h2>Release summary</h2>

      {release ? (
        <>
          <p>
            These details will be shown to users to help identify this release.
          </p>

          <SummaryList>
            <SummaryListItem term="Time period">
              {release.timePeriodCoverage.label}
            </SummaryListItem>
            <SummaryListItem term="Release period">
              <time>{release.yearTitle}</time>
            </SummaryListItem>
            <SummaryListItem term="Release type">
              {releaseTypes[release.type]}
            </SummaryListItem>
          </SummaryList>

          <Gate
            condition={() =>
              permissionService.canUpdateRelease(releaseVersionId)
            }
          >
            <ButtonLink
              to={generatePath<ReleaseRouteParams>(
                releaseSummaryEditRoute.path,
                {
                  publicationId: release.publicationId,
                  releaseVersionId,
                },
              )}
            >
              Edit release summary
            </ButtonLink>
          </Gate>
        </>
      ) : (
        <WarningMessage>Could not load release summary</WarningMessage>
      )}
    </LoadingSpinner>
  );
};

export default ReleaseSummaryPage;
