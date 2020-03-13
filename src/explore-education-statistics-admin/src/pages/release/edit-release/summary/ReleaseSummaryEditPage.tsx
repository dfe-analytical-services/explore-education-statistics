import useFormSubmit from '@admin/hooks/useFormSubmit';
import { useManageReleaseContext } from '@admin/pages/release/ManageReleaseContext';
import ReleaseSummaryForm, {
  EditFormValues,
} from '@admin/pages/release/summary/ReleaseSummaryForm';
import { assembleUpdateReleaseSummaryRequestFromForm } from '@admin/pages/release/util/releaseSummaryUtil';
import { summaryRoute } from '@admin/routes/edit-release/routes';
import service from '@admin/services/release/edit-release/summary/service';
import { ReleaseSummaryDetails } from '@admin/services/release/types';
import {
  errorCodeAndFieldNameToFieldError,
  errorCodeToFieldError,
} from '@common/components/form/util/serverValidationHandler';
import {
  dateToDayMonthYear,
  dayMonthYearValuesToInputs,
} from '@common/services/publicationService';
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

  const handleSubmit = useFormSubmit<EditFormValues>(async values => {
    const updatedReleaseDetails = assembleUpdateReleaseSummaryRequestFromForm(
      releaseId,
      values,
    );

    await service.updateReleaseSummaryDetails(updatedReleaseDetails);
    history.push(summaryRoute.generateLink(publication.id, releaseId));
  }, errorCodeMappings);

  const cancelHandler = () => {
    history.push(summaryRoute.generateLink(publication.id, releaseId));
  };

  return (
    <>
      {releaseSummaryDetails && (
        <>
          <h2 className="govuk-heading-l">Edit release summary</h2>

          <ReleaseSummaryForm
            submitButtonText="Update release summary"
            initialValuesSupplier={(): EditFormValues => ({
              timePeriodCoverageCode:
                releaseSummaryDetails.timePeriodCoverage.value,
              timePeriodCoverageStartYear: releaseSummaryDetails.releaseName.toString(),
              releaseTypeId: releaseSummaryDetails.type.id,
              scheduledPublishDate: dayMonthYearValuesToInputs(
                dateToDayMonthYear(
                  new Date(releaseSummaryDetails.publishScheduled),
                ),
              ),
              nextReleaseDate: dayMonthYearValuesToInputs(
                releaseSummaryDetails.nextReleaseDate,
              ),
            })}
            onSubmitHandler={handleSubmit}
            onCancelHandler={cancelHandler}
          />
        </>
      )}
    </>
  );
};

export default ReleaseSummaryEditPage;
