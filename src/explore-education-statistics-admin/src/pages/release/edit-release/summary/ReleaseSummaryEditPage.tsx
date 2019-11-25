import ManageReleaseContext, {
  ManageRelease,
} from '@admin/pages/release/ManageReleaseContext';
import ReleaseSummaryForm, {
  EditFormValues,
} from '@admin/pages/release/summary/ReleaseSummaryForm';
import { assembleUpdateReleaseSummaryRequestFromForm } from '@admin/pages/release/util/releaseSummaryUtil';
import { summaryRoute } from '@admin/routes/edit-release/routes';
import service from '@admin/services/release/edit-release/summary/service';
import { ReleaseSummaryDetails } from '@admin/services/release/types';
import {
  dateToDayMonthYear,
  dayMonthYearValuesToInputs,
} from '@common/services/publicationService';
import React, { useContext, useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';

const ReleaseSummaryEditPage = ({ history }: RouteComponentProps) => {
  const [releaseSummaryDetails, setReleaseSummaryDetails] = useState<
    ReleaseSummaryDetails
  >();

  const { releaseId, publication } = useContext(
    ManageReleaseContext,
  ) as ManageRelease;

  useEffect(() => {
    service.getReleaseSummaryDetails(releaseId).then(release => {
      setReleaseSummaryDetails(release);
    });
  }, [releaseId]);

  const submitHandler = async (values: EditFormValues) => {
    const updatedReleaseDetails = assembleUpdateReleaseSummaryRequestFromForm(
      releaseId,
      values,
    );

    await service.updateReleaseSummaryDetails(updatedReleaseDetails);
    history.push(summaryRoute.generateLink(publication.id, releaseId));
  };

  const cancelHandler = () =>
    history.push(summaryRoute.generateLink(publication.id, releaseId));

  return (
    <>
      {releaseSummaryDetails && (
        <>
          <h2 className="govuk-heading-m">Edit release summary</h2>

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
            onSubmitHandler={submitHandler}
            onCancelHandler={cancelHandler}
          />
        </>
      )}
    </>
  );
};

export default ReleaseSummaryEditPage;
