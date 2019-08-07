import { TimePeriodCoverageGroup } from '@admin/pages/DummyReferenceData';
import ReleaseSummaryForm, {
  EditFormValues,
} from '@admin/pages/release/summary/ReleaseSummaryForm';
import { assembleUpdateReleaseSummaryRequestFromForm } from '@admin/pages/release/util/releaseSummaryUtil';
import { summaryRoute } from '@admin/routes/edit-release/routes';
import {dateToDayMonthYear, dayMonthYearValuesToInputs} from '@admin/services/common/types';
import service from '@admin/services/release/edit-release/summary/service';
import { ReleaseSummaryDetails } from '@admin/services/release/types';
import React, { useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';
import ReleasePageTemplate from '../components/ReleasePageTemplate';

interface MatchProps {
  publicationId: string;
  releaseId: string;
}

const ReleaseSummaryEditPage = ({
  match,
  history,
}: RouteComponentProps<MatchProps>) => {
  const { publicationId, releaseId } = match.params;

  const [releaseSummaryDetails, setReleaseSummaryDetails] = useState<
    ReleaseSummaryDetails
  >();

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
      .then(_ => history.push(summaryRoute.generateLink(publicationId, releaseId)));
  };

  const cancelHandler = () => history.push(summaryRoute.generateLink(publicationId, releaseId));

  return (
    <>
      {releaseSummaryDetails && (
        <>
          <h2 className="govuk-heading-m">Edit release setup</h2>

          <ReleaseSummaryForm
            submitButtonText="Update release status"
            initialValuesSupplier={(
              _: TimePeriodCoverageGroup[],
            ): EditFormValues => ({
              timePeriodCoverageCode:
                releaseSummaryDetails.timePeriodCoverageCode,
              timePeriodCoverageStartYear: releaseSummaryDetails.releaseName.toString(),
              releaseTypeId: releaseSummaryDetails.typeId,
              scheduledPublishDate: dayMonthYearValuesToInputs(
                dateToDayMonthYear(new Date(releaseSummaryDetails.publishScheduled)),
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
