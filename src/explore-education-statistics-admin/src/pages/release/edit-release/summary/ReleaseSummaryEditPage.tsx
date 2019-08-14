import { TimePeriodCoverageGroup } from '@admin/pages/DummyReferenceData';
import ManageReleaseContext, {
  ManageRelease,
} from '@admin/pages/release/ManageReleaseContext';
import ReleaseSummaryForm, {
  EditFormValues,
} from '@admin/pages/release/summary/ReleaseSummaryForm';
import { assembleUpdateReleaseSummaryRequestFromForm } from '@admin/pages/release/util/releaseSummaryUtil';
import { summaryRoute } from '@admin/routes/edit-release/routes';
import {
  dateToDayMonthYear,
  dayMonthYearValuesToInputs,
} from '@admin/services/common/types';
import service from '@admin/services/release/edit-release/summary/service';
import { ReleaseSummaryDetails } from '@admin/services/release/types';
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

  const submitHandler = (values: EditFormValues) => {
    const updatedReleaseDetails = assembleUpdateReleaseSummaryRequestFromForm(
      releaseId,
      values,
    );

    service
      .updateReleaseSummaryDetails(updatedReleaseDetails)
      .then(_ =>
        history.push(summaryRoute.generateLink(publication.id, releaseId)),
      );
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
            initialValuesSupplier={(
              _: TimePeriodCoverageGroup[],
            ): EditFormValues => ({
              timePeriodCoverageCode:
                releaseSummaryDetails.timePeriodCoverage.value,
              timePeriodCoverageStartYear: releaseSummaryDetails.releaseName.toString(),
              releaseTypeId: releaseSummaryDetails.typeId,
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
