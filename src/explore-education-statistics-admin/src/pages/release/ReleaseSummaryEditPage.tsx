import { useLastLocation } from '@admin/contexts/LastLocationContext';
import ReleaseSummaryForm, {
  ReleaseSummaryFormValues,
} from '@admin/pages/release/components/ReleaseSummaryForm';
import { useReleaseVersionContext } from '@admin/pages/release/contexts/ReleaseContext';
import {
  ReleaseRouteParams,
  releaseSummaryRoute,
} from '@admin/routes/releaseRoutes';
import releaseService from '@admin/services/releaseService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import React from 'react';
import { generatePath, RouteComponentProps, useLocation } from 'react-router';

export default function ReleaseSummaryEditPage({
  history,
}: RouteComponentProps) {
  const location = useLocation();
  const lastLocation = useLastLocation();

  const {
    releaseVersionId,
    releaseVersion: contextRelease,
    onReleaseChange,
  } = useReleaseVersionContext();

  const { value: release, isLoading } = useAsyncRetry(
    async () =>
      lastLocation && lastLocation !== location
        ? releaseService.getRelease(releaseVersionId)
        : contextRelease,
    [releaseVersionId],
  );

  const handleSubmit = async (values: ReleaseSummaryFormValues) => {
    if (!release) {
      throw new Error('Could not update missing release');
    }

    await releaseService.updateRelease(releaseVersionId, {
      year: Number(values.timePeriodCoverageStartYear),
      timePeriodCoverage: {
        value: values.timePeriodCoverageCode,
      },
      type: values.releaseType ?? 'AdHocStatistics',
      preReleaseAccessList: release.preReleaseAccessList,
    });

    onReleaseChange();

    history.push(
      generatePath<ReleaseRouteParams>(releaseSummaryRoute.path, {
        publicationId: release.publicationId,
        releaseVersionId,
      }),
    );
  };

  const handleCancel = () => {
    if (!release) {
      return;
    }

    history.push(
      generatePath<ReleaseRouteParams>(releaseSummaryRoute.path, {
        publicationId: release.publicationId,
        releaseVersionId,
      }),
    );
  };

  return (
    <LoadingSpinner loading={isLoading}>
      {release && (
        <>
          <h2>Edit release summary</h2>

          <ReleaseSummaryForm
            submitText="Update release summary"
            initialValues={{
              timePeriodCoverageCode: release.timePeriodCoverage.value,
              timePeriodCoverageStartYear: release.year.toString(),
              releaseType: release.type,
            }}
            onSubmit={handleSubmit}
            onCancel={handleCancel}
          />
        </>
      )}
    </LoadingSpinner>
  );
}
