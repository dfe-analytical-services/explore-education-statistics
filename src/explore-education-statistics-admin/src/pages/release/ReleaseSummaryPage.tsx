import ButtonLink from '@admin/components/ButtonLink';
import { useReleaseContext } from '@admin/pages/release/contexts/ReleaseContext';
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
import React from 'react';
import { generatePath } from 'react-router';

const ReleaseSummaryPage = () => {
  const { release: contextRelease, releaseId } = useReleaseContext();

  const { value: release = contextRelease, isLoading } = useAsyncRetry(
    () => releaseService.getRelease(releaseId),
    [releaseId],
  );

  return (
    <LoadingSpinner loading={isLoading}>
      <h2>Release summary</h2>

      {release ? (
        <>
          <p>
            These details will be shown to users to help identify this release.
          </p>

          <h3>Publication details</h3>
          <SummaryList>
            <SummaryListItem term="Publication title">
              {release.publicationTitle}
            </SummaryListItem>
            <SummaryListItem term="Lead statistician">
              {release.contact?.contactName}
            </SummaryListItem>
          </SummaryList>

          <h3>Release summary</h3>
          <SummaryList>
            <SummaryListItem term="Time period">
              {release.timePeriodCoverage.label}
            </SummaryListItem>
            <SummaryListItem term="Release period">
              <time>{release.yearTitle}</time>
            </SummaryListItem>
            <SummaryListItem term="Release type">
              {release.type.title}
            </SummaryListItem>
          </SummaryList>

          <Gate condition={() => permissionService.canUpdateRelease(releaseId)}>
            <ButtonLink
              to={generatePath<ReleaseRouteParams>(
                releaseSummaryEditRoute.path,
                {
                  publicationId: release.publicationId,
                  releaseId,
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
