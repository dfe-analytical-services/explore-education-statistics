import React, {useEffect, useState} from 'react';
import {RouteComponentProps} from 'react-router';
import ReleaseSetupForm, {FormValues} from "@admin/pages/release/setup/ReleaseSetupForm";
import {setupRoute} from "@admin/routes/edit-release/routes";
import {IdTitlePair} from '@admin/services/common/types';
import service from '@admin/services/edit-release/setup/service';
import {ReleaseSetupDetails, ReleaseSetupDetailsUpdateRequest,} from '@admin/services/edit-release/setup/types';
import DummyReferenceData, {DateType, TimePeriodCoverageGroup,} from '@admin/pages/DummyReferenceData';
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

  const [timePeriodCoverageGroups, setTimePeriodCoverageGroups] = useState<
    TimePeriodCoverageGroup[]
  >();

  const [releaseTypes, setReleaseTypes] = useState<IdTitlePair[]>();

  useEffect(() => {
    service.getReleaseSetupDetails(releaseId).then(release => {
      setReleaseSetupDetails(release);
      setTimePeriodCoverageGroups(DummyReferenceData.timePeriodCoverageGroups);
      setReleaseTypes(DummyReferenceData.releaseTypeOptions);
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
      {releaseSetupDetails && timePeriodCoverageGroups && releaseTypes && (
        <ReleasePageTemplate
          releaseId={releaseId}
          publicationTitle={releaseSetupDetails.publicationTitle}
        >
          <h2 className="govuk-heading-m">Edit release setup</h2>

          <ReleaseSetupForm
            releaseSetupDetails={releaseSetupDetails}
            timePeriodCoverageGroups={timePeriodCoverageGroups}
            releaseTypes={releaseTypes}
            onSubmitHandler={submitHandler}
          />

        </ReleasePageTemplate>
      )}
    </>
  );
};

export default ReleaseSetupEditPage;
