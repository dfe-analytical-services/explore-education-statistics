import DummyReferenceData, {
  DateType,
  TimePeriodCoverageGroup,
} from '@admin/pages/DummyReferenceData';
import { FormValues } from '@admin/pages/release/setup/ReleaseSetupForm';
import { dayMonthYearInputsToValues } from '@admin/services/common/types';
import {
  BaseReleaseSetupDetailsRequest,
  CreateReleaseRequest,
  UpdateReleaseSetupDetailsRequest,
} from '@admin/services/edit-release/setup/types';

export const isDayMonthYearDateTypeSelected = (
  timePeriodGroup?: TimePeriodCoverageGroup,
) =>
  timePeriodGroup
    ? DateType.DayMonthYear === timePeriodGroup.startDateType
    : false;

export const isYearOnlyDateTypeSelected = (
  timePeriodGroup?: TimePeriodCoverageGroup,
) =>
  timePeriodGroup ? DateType.Year === timePeriodGroup.startDateType : false;

export const isDayMonthYearDateTypeCodeSelected = (
  timePeriodGroupCode?: string,
) =>
  timePeriodGroupCode
    ? isDayMonthYearDateTypeSelected(
        DummyReferenceData.findTimePeriodCoverageGroup(timePeriodGroupCode),
      )
    : false;

export const isYearOnlyDateTypeCodeSelected = (timePeriodGroupCode?: string) =>
  timePeriodGroupCode
    ? isYearOnlyDateTypeSelected(
        DummyReferenceData.findTimePeriodCoverageGroup(timePeriodGroupCode),
      )
    : false;

export const assembleBaseReleaseSetupRequestFromForm = (
  values: FormValues,
): BaseReleaseSetupDetailsRequest => {
  return {
    timePeriodCoverageCode: values.timePeriodCoverageCode,
    timePeriodCoverageStartDate:
      isDayMonthYearDateTypeCodeSelected(values.timePeriodCoverageCode) &&
      values.timePeriodCoverageStartDate
        ? dayMonthYearInputsToValues(values.timePeriodCoverageStartDate)
        : {
            year: parseInt(
              values.timePeriodCoverageStartDateYearOnly || '0',
              10,
            ),
          },
    scheduledPublishDate: dayMonthYearInputsToValues(
      values.scheduledPublishDate,
    ),
    nextReleaseExpectedDate: dayMonthYearInputsToValues(
      values.nextReleaseExpectedDate,
    ),
    releaseType: values.releaseTypeId
      ? DummyReferenceData.findReleaseType(values.releaseTypeId)
      : DummyReferenceData.releaseTypeOptions[0],
  };
};

export const assembleCreateReleaseRequestFromForm = (
  publicationId: string,
  values: FormValues,
): CreateReleaseRequest => {
  return {
    publicationId,
    ...assembleBaseReleaseSetupRequestFromForm(values),
  };
};

export const assembleUpdateReleaseSetupRequestFromForm = (
  releaseId: string,
  values: FormValues,
): UpdateReleaseSetupDetailsRequest => {
  return {
    releaseId,
    ...assembleBaseReleaseSetupRequestFromForm(values),
  };
};

export default {};
