import ButtonLink from '@admin/components/ButtonLink';
import { useLastLocation } from '@admin/contexts/LastLocationContext';
import { useReleaseVersionContext } from '@admin/pages/release/contexts/ReleaseVersionContext';
import {
  ReleaseRouteParams,
  releaseSummaryEditRoute,
} from '@admin/routes/releaseRoutes';
import permissionService from '@admin/services/permissionService';
import releaseVersionService from '@admin/services/releaseVersionService';
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

  const { value: releaseVersion, isLoading } = useAsyncRetry(
    async () =>
      lastLocation && lastLocation !== location
        ? releaseVersionService.getReleaseVersion(releaseVersionId)
        : contextRelease,
    [releaseVersionId],
  );

  return (
    <LoadingSpinner loading={isLoading}>
      <h2>Release summary</h2>

      {releaseVersion ? (
        <>
          <p>
            These details will be shown to users to help identify this
            releaseVersion.
          </p>

          <SummaryList>
            <SummaryListItem term="Time period">
              {releaseVersion.timePeriodCoverage.label}
            </SummaryListItem>
            <SummaryListItem term="Release period">
              <time>{releaseVersion.yearTitle}</time>
            </SummaryListItem>
            <SummaryListItem term="Release type">
              {releaseTypes[releaseVersion.type]}
            </SummaryListItem>
            <SummaryListItem term="Release label">
              {releaseVersion.label ?? ''}
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
                  publicationId: releaseVersion.publicationId,
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
