import useFormSubmit from '@admin/hooks/useFormSubmit';
import { useManageReleaseContext } from '@admin/pages/release/ManageReleaseContext';
import ReleaseSummaryForm, {
  ReleaseSummaryFormValues,
} from '@admin/pages/release/summary/ReleaseSummaryForm';
import { summaryRoute } from '@admin/routes/edit-release/routes';
import releaseService, {
  ReleaseSummary,
  UpdateReleaseSummaryRequest,
} from '@admin/services/releaseService';
import {
  errorCodeAndFieldNameToFieldError,
  errorCodeToFieldError,
} from '@common/components/form/util/serverValidationHandler';
import React, { useEffect, useState } from 'react';
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
  const [releaseSummaryDetails, setReleaseSummaryDetails] = useState<
    ReleaseSummary
  >();

  const { releaseId, publication } = useManageReleaseContext();

  useEffect(() => {
    releaseService.getReleaseSummary(releaseId).then(release => {
      setReleaseSummaryDetails(release);
    });
  }, [releaseId]);

  const handleSubmit = useFormSubmit<ReleaseSummaryFormValues>(async values => {
    const release: UpdateReleaseSummaryRequest = {
      timePeriodCoverage: {
        value: values.timePeriodCoverageCode,
      },
      releaseName: parseInt(values.timePeriodCoverageStartYear, 10),
      publishScheduled: values.scheduledPublishDate,
      nextReleaseDate: values.nextReleaseDate,
      typeId: values.releaseTypeId,
      releaseId,
    };

    await releaseService.updateReleaseSummary(release);

    history.push(
      summaryRoute.generateLink({ publicationId: publication.id, releaseId }),
    );
  }, errorCodeMappings);

  const handleCancel = () =>
    history.push(
      summaryRoute.generateLink({ publicationId: publication.id, releaseId }),
    );

  return (
    <>
      {releaseSummaryDetails && (
        <>
          <h2 className="govuk-heading-l">Edit release summary</h2>

          <ReleaseSummaryForm<ReleaseSummaryFormValues>
            submitText="Update release summary"
            initialValues={() => ({
              timePeriodCoverageCode:
                releaseSummaryDetails.timePeriodCoverage.value,
              timePeriodCoverageStartYear: releaseSummaryDetails.releaseName.toString(),
              releaseTypeId: releaseSummaryDetails.type.id,
              scheduledPublishDate: new Date(
                releaseSummaryDetails.publishScheduled,
              ),
              nextReleaseDate: releaseSummaryDetails?.nextReleaseDate,
            })}
            onSubmit={handleSubmit}
            onCancel={handleCancel}
          />
        </>
      )}
    </>
  );
};

export default ReleaseSummaryEditPage;
