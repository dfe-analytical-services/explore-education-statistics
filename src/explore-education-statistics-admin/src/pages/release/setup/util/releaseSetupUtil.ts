import DummyReferenceData from '@admin/pages/DummyReferenceData';
import {FormValues} from '@admin/pages/release/setup/ReleaseSetupForm';
import {dayMonthYearInputsToValues} from '@admin/services/common/types';
import {
  BaseReleaseSetupDetailsRequest,
  CreateReleaseRequest,
  UpdateReleaseSetupDetailsRequest,
} from '@admin/services/edit-release/setup/types';

export const assembleBaseReleaseSetupRequestFromForm = (
  values: FormValues,
): BaseReleaseSetupDetailsRequest => {
  return {
    timePeriodCoverageCode: values.timePeriodCoverageCode,
    timePeriodCoverageStartYear: parseInt(values.timePeriodCoverageStartYear, 10),
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
