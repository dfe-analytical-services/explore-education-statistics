import useFormSubmit from '@admin/hooks/useFormSubmit';
import { useManageReleaseContext } from '@admin/pages/release/ManageReleaseContext';
import ReleaseSummaryForm, {
  ReleaseSummaryFormValues,
} from '@admin/pages/release/summary/ReleaseSummaryForm';
import { assembleUpdateReleaseSummaryRequestFromForm } from '@admin/pages/release/util/releaseSummaryUtil';
import { summaryRoute } from '@admin/routes/edit-release/routes';
import service from '@admin/services/release/edit-release/summary/service';
import { ReleaseSummaryDetails } from '@admin/services/release/types';
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
    ReleaseSummaryDetails
  >();

  const { releaseId, publication } = useManageReleaseContext();

  useEffect(() => {
    service.getReleaseSummaryDetails(releaseId).then(release => {
      setReleaseSummaryDetails(release);
    });
  }, [releaseId]);

  const handleSubmit = useFormSubmit<ReleaseSummaryFormValues>(async values => {
    const updatedReleaseDetails = assembleUpdateReleaseSummaryRequestFromForm(
      releaseId,
      values,
    );

    await service.updateReleaseSummaryDetails(updatedReleaseDetails);
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
