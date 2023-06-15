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
import useFormSubmit from '@common/hooks/useFormSubmit';
import { mapFieldErrors } from '@common/validation/serverValidations';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import { generatePath, useHistory, useLocation } from 'react-router';
import React from 'react';

const errorMappings = [
  mapFieldErrors<ReleaseSummaryFormValues>({
    target: 'timePeriodCoverageStartYear',
    messages: {
      SlugNotUnique:
        'Choose a unique combination of time period and start year',
    },
  }),
];

const ReleaseSummaryEditPage = () => {
  const history = useHistory();
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

  const handleSubmit = useFormSubmit<ReleaseSummaryFormValues>(async values => {
    if (!release) {
      throw new Error('Could not update missing release');
    }

    const nextRelease = await releaseService.updateRelease(releaseId, {
      year: Number(values.timePeriodCoverageStartYear),
      timePeriodCoverage: {
        value: values.timePeriodCoverageCode,
      },
      type: values.releaseType ?? 'AdHocStatistics',
      preReleaseAccessList: release.preReleaseAccessList,
    });

    onReleaseChange(nextRelease);

    history.push(
      generatePath<ReleaseRouteParams>(releaseSummaryRoute.path, {
        publicationId: release.publicationId,
        releaseId,
      }),
    );
  }, errorMappings);

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

          <ReleaseSummaryForm<ReleaseSummaryFormValues>
            submitText="Update release summary"
            initialValues={() => ({
              timePeriodCoverageCode: release.timePeriodCoverage.value,
              timePeriodCoverageStartYear: release.year.toString(),
              releaseType: release.type,
            })}
            onSubmit={handleSubmit}
            onCancel={handleCancel}
          />
        </>
      )}
    </LoadingSpinner>
  );
};

export default ReleaseSummaryEditPage;
