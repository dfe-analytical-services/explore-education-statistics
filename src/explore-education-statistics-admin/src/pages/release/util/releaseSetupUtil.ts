import {
  dayMonthYearInputsToDate,
  dayMonthYearInputsToValues,
} from '@admin/services/common/types';
import { CreateReleaseRequest } from '@admin/services/release/create-release/types';
import { UpdateReleaseSetupDetailsRequest } from '@admin/services/release/edit-release/setup/types';
import { BaseReleaseSetupDetailsRequest } from '@admin/services/release/types';
import { FormValues as CreateFormValues } from '../create-release/CreateReleasePage';
import { EditFormValues } from '../setup/ReleaseSetupForm';

export const assembleBaseReleaseSetupRequestFromForm = (
  values: EditFormValues,
): BaseReleaseSetupDetailsRequest => {
  return {
    timePeriodCoverage: {
      value: values.timePeriodCoverageCode,
    },
    releaseName: parseInt(values.timePeriodCoverageStartYear, 10),
    publishScheduled: dayMonthYearInputsToDate(values.scheduledPublishDate),
    nextReleaseExpected: dayMonthYearInputsToValues(
      values.nextReleaseExpectedDate,
    ),
    releaseTypeId: values.releaseTypeId,
  };
};

export const assembleCreateReleaseRequestFromForm = (
  publicationId: string,
  values: CreateFormValues,
): CreateReleaseRequest => {
  return {
    publicationId,
    templateReleaseId: values.templateReleaseId,
    ...assembleBaseReleaseSetupRequestFromForm(values),
  };
};

export const assembleUpdateReleaseSetupRequestFromForm = (
  releaseId: string,
  values: EditFormValues,
): UpdateReleaseSetupDetailsRequest => {
  return {
    releaseId,
    ...assembleBaseReleaseSetupRequestFromForm(values),
  };
};

export default {};
