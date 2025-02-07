import { useLastLocation } from '@admin/contexts/LastLocationContext';
import ReleaseSummaryForm, {
  ReleaseSummaryFormValues,
} from '@admin/pages/release/components/ReleaseSummaryForm';
import { useReleaseVersionContext } from '@admin/pages/release/contexts/ReleaseVersionContext';
import {
  ReleaseRouteParams,
  releaseSummaryRoute,
} from '@admin/routes/releaseRoutes';
import releaseVersionService from '@admin/services/releaseVersionService';
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

  const { value: releaseVersion, isLoading } = useAsyncRetry(
    async () =>
      lastLocation && lastLocation !== location
        ? releaseVersionService.getReleaseVersion(releaseVersionId)
        : contextRelease,
    [releaseVersionId],
  );

  const handleSubmit = async (values: ReleaseSummaryFormValues) => {
    if (!releaseVersion) {
      throw new Error('Could not update missing release');
    }

    await releaseVersionService.updateReleaseVersion(releaseVersionId, {
      year: Number(values.timePeriodCoverageStartYear),
      timePeriodCoverage: {
        value: values.timePeriodCoverageCode,
      },
      type: values.releaseType ?? 'AdHocStatistics',
      preReleaseAccessList: releaseVersion.preReleaseAccessList,
      label: values.releaseLabel,
    });

    onReleaseChange();

    history.push(
      generatePath<ReleaseRouteParams>(releaseSummaryRoute.path, {
        publicationId: releaseVersion.publicationId,
        releaseVersionId,
      }),
    );
  };

  const handleCancel = () => {
    if (!releaseVersion) {
      return;
    }

    history.push(
      generatePath<ReleaseRouteParams>(releaseSummaryRoute.path, {
        publicationId: releaseVersion.publicationId,
        releaseVersionId,
      }),
    );
  };

  return (
    <LoadingSpinner loading={isLoading}>
      {releaseVersion && (
        <>
          <h2>Edit release summary</h2>

          <ReleaseSummaryForm
            submitText="Update release summary"
            initialValues={{
              timePeriodCoverageCode: releaseVersion.timePeriodCoverage.value,
              timePeriodCoverageStartYear: releaseVersion.year.toString(),
              releaseType: releaseVersion.type,
              releaseLabel: releaseVersion.label,
            }}
            releaseVersion={releaseVersion.version}
            onSubmit={handleSubmit}
            onCancel={handleCancel}
          />
        </>
      )}
    </LoadingSpinner>
  );
}
