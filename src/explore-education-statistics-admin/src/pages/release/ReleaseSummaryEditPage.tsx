import { useLastLocation } from '@admin/contexts/LastLocationContext';
import ReleaseSummaryForm, {
  ReleaseSummaryFormValues,
} from '@admin/pages/release/components/ReleaseSummaryForm';
import { useReleaseContext } from '@admin/pages/release/contexts/ReleaseContext';
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
    releaseId,
    release: contextRelease,
    onReleaseChange,
  } = useReleaseContext();

  const { value: release, isLoading } = useAsyncRetry(
    async () =>
      lastLocation && lastLocation !== location
        ? releaseService.getRelease(releaseId)
        : contextRelease,
    [releaseId],
  );

  const handleSubmit = async (values: ReleaseSummaryFormValues) => {
    if (!release) {
      throw new Error('Could not update missing release');
    }

    await releaseService.updateRelease(releaseId, {
      year: Number(values.timePeriodCoverageStartYear),
      timePeriodCoverage: {
        value: values.timePeriodCoverageCode,
      },
      type: values.releaseType ?? 'AdHocStatistics',
      preReleaseAccessList: release.preReleaseAccessList,
      label: values.releaseLabel,
    });

    onReleaseChange();

    history.push(
      generatePath<ReleaseRouteParams>(releaseSummaryRoute.path, {
        publicationId: release.publicationId,
        releaseId,
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
        releaseId,
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
              releaseLabel: release.label,
            }}
            releaseVersion={release.version}
            onSubmit={handleSubmit}
            onCancel={handleCancel}
          />
        </>
      )}
    </LoadingSpinner>
  );
}
