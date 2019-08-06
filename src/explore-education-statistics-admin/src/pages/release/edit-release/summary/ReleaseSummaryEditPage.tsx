import { TimePeriodCoverageGroup } from '@admin/pages/DummyReferenceData';
import ReleaseSummaryForm, {
  EditFormValues,
} from '@admin/pages/release/summary/ReleaseSummaryForm';
import { assembleUpdateReleaseSummaryRequestFromForm } from '@admin/pages/release/util/releaseSummaryUtil';
import { setupRoute } from '@admin/routes/edit-release/routes';
import { dayMonthYearValuesToInputs } from '@admin/services/common/types';
import service from '@admin/services/release/edit-release/summary/service';
import { ReleaseSummaryDetails } from '@admin/services/release/types';
import React, { useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';
import ReleasePageTemplate from '../components/ReleasePageTemplate';

interface MatchProps {
  releaseId: string;
}

const ReleaseSummaryEditPage = ({
  match,
  history,
}: RouteComponentProps<MatchProps>) => {
  const { releaseId } = match.params;

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
      .then(_ => history.push(setupRoute.generateLink(releaseId)));
  };

  const cancelHandler = () => history.push(setupRoute.generateLink(releaseId));

  return (
    <>
      {releaseSummaryDetails && (
        <ReleasePageTemplate
          releaseId={releaseId}
          publicationTitle={releaseSummaryDetails.publicationTitle}
        >
          <h2 className="govuk-heading-m">Edit release setup</h2>

          <ReleaseSummaryForm
            submitButtonText="Update release status"
            initialValuesSupplier={(
              _: TimePeriodCoverageGroup[],
            ): EditFormValues => ({
              timePeriodCoverageCode:
                releaseSummaryDetails.timePeriodCoverageCode,
              timePeriodCoverageStartYear: releaseSummaryDetails.timePeriodCoverageStartYear.toString(),
              releaseTypeId: releaseSummaryDetails.releaseType.id,
              scheduledPublishDate: dayMonthYearValuesToInputs(
                releaseSummaryDetails.scheduledPublishDate,
              ),
              nextReleaseExpectedDate: dayMonthYearValuesToInputs(
                releaseSummaryDetails.nextReleaseExpectedDate,
              ),
            })}
            onSubmitHandler={submitHandler}
            onCancelHandler={cancelHandler}
          />
        </ReleasePageTemplate>
      )}
    </>
  );
};

export default ReleaseSummaryEditPage;
