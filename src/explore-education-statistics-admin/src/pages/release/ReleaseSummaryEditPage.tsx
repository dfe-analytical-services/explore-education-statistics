import useFormSubmit from '@admin/hooks/useFormSubmit';
import ReleaseSummaryForm, {
  ReleaseSummaryFormValues,
} from '@admin/pages/release/components/ReleaseSummaryForm';
import { useManageReleaseContext } from '@admin/pages/release/contexts/ManageReleaseContext';
import { summaryRoute } from '@admin/routes/releaseRoutes';
import releaseService from '@admin/services/releaseService';
import {
  errorCodeAndFieldNameToFieldError,
  errorCodeToFieldError,
} from '@common/components/form/util/serverValidationHandler';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import React from 'react';
import { RouteComponentProps } from 'react-router';

const errorCodeMappings = [
  errorCodeToFieldError(
    'SLUG_NOT_UNIQUE',
    'timePeriodCoverageStartYear',
    'Choose a unique combination of time period and start year',
  ),
  errorCodeAndFieldNameToFieldError(
    'PARTIAL_DATE_NOT_VALID',
    'NextReleaseDate',
    'nextReleaseDate',
    'Enter a valid date',
  ),
];

const ReleaseSummaryEditPage = ({ history }: RouteComponentProps) => {
  const { releaseId, publication } = useManageReleaseContext();

  const { value: release, isLoading } = useAsyncRetry(
    () => releaseService.getRelease(releaseId),
    [releaseId],
  );

  const handleSubmit = useFormSubmit<ReleaseSummaryFormValues>(async values => {
    if (!release) {
      throw new Error('Could not update missing release');
    }

    await releaseService.updateRelease(releaseId, {
      internalReleaseNote: release.internalReleaseNote,
      nextReleaseDate: release.nextReleaseDate,
      publishScheduled: release.publishScheduled,
      status: release.status,
      timePeriodCoverage: {
        value: values.timePeriodCoverageCode,
      },
      releaseName: values.timePeriodCoverageStartYear,
      typeId: values.releaseTypeId,
    });

    history.push(
      summaryRoute.generateLink({ publicationId: publication.id, releaseId }),
    );
  }, errorCodeMappings);

  const handleCancel = () =>
    history.push(
      summaryRoute.generateLink({ publicationId: publication.id, releaseId }),
    );

  return (
    <LoadingSpinner loading={isLoading}>
      {release && (
        <>
          <h2 className="govuk-heading-l">Edit release summary</h2>

          <ReleaseSummaryForm<ReleaseSummaryFormValues>
            submitText="Update release summary"
            initialValues={() => ({
              timePeriodCoverageCode: release.timePeriodCoverage.value,
              timePeriodCoverageStartYear: release.releaseName.toString(),
              releaseTypeId: release.type.id,
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
