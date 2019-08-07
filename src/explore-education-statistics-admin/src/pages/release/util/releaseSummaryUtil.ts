import {
  dayMonthYearInputsToDate,
  dayMonthYearInputsToValues,
} from '@admin/services/common/types';
import { CreateReleaseRequest } from '@admin/services/release/create-release/types';
import { UpdateReleaseSummaryDetailsRequest } from '@admin/services/release/edit-release/summary/types';
import { BaseReleaseSummaryDetailsRequest } from '@admin/services/release/types';
import { FormValues as CreateFormValues } from '../create-release/CreateReleasePage';
import { EditFormValues } from '../summary/ReleaseSummaryForm';

export const assembleBaseReleaseSummaryRequestFromForm = (
  values: EditFormValues,
): BaseReleaseSummaryDetailsRequest => {
  return {
    timePeriodCoverage: {
      value: values.timePeriodCoverageCode,
    },
    releaseName: parseInt(values.timePeriodCoverageStartYear, 10),
    publishScheduled: dayMonthYearInputsToDate(values.scheduledPublishDate),
    nextReleaseDate: dayMonthYearInputsToValues(values.nextReleaseDate),
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
    ...assembleBaseReleaseSummaryRequestFromForm(values),
  };
};

export const assembleUpdateReleaseSummaryRequestFromForm = (
  releaseId: string,
  values: EditFormValues,
): UpdateReleaseSummaryDetailsRequest => {
  return {
    releaseId,
    ...assembleBaseReleaseSummaryRequestFromForm(values),
  };
};

export default {};
