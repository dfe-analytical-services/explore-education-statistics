import { dayMonthYearInputsToValues } from '@admin/services/common/types';
import { CreateReleaseRequest } from '@admin/services/release/create-release/types';
import { UpdateReleaseSetupDetailsRequest } from '@admin/services/release/edit-release/setup/types';
import { BaseReleaseSetupDetailsRequest } from '@admin/services/release/types';
import { BaseFormValues } from '../setup/ReleaseSetupForm';
import { FormValues as CreateFormValues } from '../create-release/CreateReleasePage';
import { FormValues as ReleaaseSetupEditPage } from '../edit-release/setup/ReleaseSetupEditPage';

export const assembleBaseReleaseSetupRequestFromForm = (
  values: BaseFormValues,
): BaseReleaseSetupDetailsRequest => {
  return {
    timePeriodCoverage: {
      value: values.timePeriodCoverageCode,
    },
    releaseName: parseInt(values.timePeriodCoverageStartYear, 10),
    publishScheduled: dayMonthYearInputsToValues(values.scheduledPublishDate),
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
  values: ReleaaseSetupEditPage,
): UpdateReleaseSetupDetailsRequest => {
  return {
    releaseId,
    ...assembleBaseReleaseSetupRequestFromForm(values),
  };
};

export default {};
