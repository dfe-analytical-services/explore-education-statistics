import DummyReferenceData, {DateType, TimePeriodCoverageGroup,} from '@admin/pages/DummyReferenceData';
import ReleaseSetupForm, {FormValues} from "@admin/pages/release/setup/ReleaseSetupForm";
import {setupRoute} from "@admin/routes/edit-release/routes";
import service from '@admin/services/edit-release/setup/service';
import {ReleaseSetupDetails, ReleaseSetupDetailsUpdateRequest,} from '@admin/services/edit-release/setup/types';
import React, {useEffect, useState} from 'react';
import {RouteComponentProps} from 'react-router';
import ReleasePageTemplate from '../components/ReleasePageTemplate';

interface MatchProps {
  releaseId: string;
}

const ReleaseSetupEditPage = ({
  match,
  history,
}: RouteComponentProps<MatchProps>) => {
  const { releaseId } = match.params;

  const [releaseSetupDetails, setReleaseSetupDetails] = useState<
    ReleaseSetupDetails
  >();

  useEffect(() => {
    service.getReleaseSetupDetails(releaseId).then(release => {
      setReleaseSetupDetails(release);
    });
  }, [releaseId]);

  const isDayMonthYearDateTypeSelected = (
    timePeriodGroup?: TimePeriodCoverageGroup,
  ) =>
    timePeriodGroup
      ? DateType.DayMonthYear === timePeriodGroup.startDateType
      : false;

  const isDayMonthYearDateTypeCodeSelected = (timePeriodGroupCode?: string) =>
    timePeriodGroupCode
      ? isDayMonthYearDateTypeSelected(
      DummyReferenceData.findTimePeriodCoverageGroup(timePeriodGroupCode),
      )
      : false;

  const submitHandler = (values: FormValues) => {
    const updatedReleaseDetails: ReleaseSetupDetailsUpdateRequest = {
      id: releaseId,
      timePeriodCoverageCode: values.timePeriodCoverageCode,
      timePeriodCoverageStartDate:
        isDayMonthYearDateTypeCodeSelected(
          values.timePeriodCoverageCode,
        ) && values.timePeriodCoverageStartDate
          ? values.timePeriodCoverageStartDate
          : {
            year: values.timePeriodCoverageStartDateYearOnly,
          },
      scheduledPublishDate: values.scheduledPublishDate,
      nextReleaseExpectedDate: values.nextReleaseExpectedDate,
      releaseType: DummyReferenceData.findReleaseType(
        values.releaseTypeId,
      ),
    };
    service
      .updateReleaseSetupDetails(updatedReleaseDetails)
      .then(_ => history.push(setupRoute.generateLink(releaseId)));
  };

  return (
    <>
      {releaseSetupDetails && (
        <ReleasePageTemplate
          releaseId={releaseId}
          publicationTitle={releaseSetupDetails.publicationTitle}
        >
          <h2 className="govuk-heading-m">Edit release setup</h2>

          <ReleaseSetupForm
            releaseSetupDetails={releaseSetupDetails}
            onSubmitHandler={submitHandler}
          />

        </ReleasePageTemplate>
      )}
    </>
  );
};

export default ReleaseSetupEditPage;
