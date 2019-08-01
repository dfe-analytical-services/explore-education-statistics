import {dayMonthYearInputsToValues} from '@admin/services/common/types';
import {CreateReleaseRequest} from "@admin/services/release/create-release/types";
import {UpdateReleaseSetupDetailsRequest} from "@admin/services/release/edit-release/setup/types";
import {BaseReleaseSetupDetailsRequest} from "@admin/services/release/types";
import DummyReferenceData from '../../DummyReferenceData';
import {FormValues} from '../setup/ReleaseSetupForm';

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
