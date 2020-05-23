import { TimePeriodCoverageGroup } from '@admin/services/common/types';
import { AdminDashboardRelease } from '@admin/services/dashboard/types';
import { CreateReleaseRequest } from '@admin/services/release/create-release/types';
import { UpdateReleaseSummaryDetailsRequest } from '@admin/services/release/edit-release/summary/types';
import { BaseReleaseSummaryDetailsRequest } from '@admin/services/release/types';
import { ReleaseStatus } from '@common/services/publicationService';
import { CreateReleaseFormValues } from '../create-release/CreateReleasePage';
import { ReleaseSummaryFormValues } from '../summary/ReleaseSummaryForm';

export const assembleBaseReleaseSummaryRequestFromForm = (
  values: ReleaseSummaryFormValues,
): BaseReleaseSummaryDetailsRequest => {
  return {
    timePeriodCoverage: {
      value: values.timePeriodCoverageCode,
    },
    releaseName: parseInt(values.timePeriodCoverageStartYear, 10),
    publishScheduled: values.scheduledPublishDate,
    nextReleaseDate: values.nextReleaseDate,
    typeId: values.releaseTypeId,
  };
};

export const assembleCreateReleaseRequestFromForm = (
  publicationId: string,
  values: CreateReleaseFormValues,
): CreateReleaseRequest => {
  return {
    publicationId,
    templateReleaseId:
      values.templateReleaseId !== 'new' ? values.templateReleaseId : '',
    ...assembleBaseReleaseSummaryRequestFromForm(values),
  };
};

export const assembleUpdateReleaseSummaryRequestFromForm = (
  releaseId: string,
  values: ReleaseSummaryFormValues,
): UpdateReleaseSummaryDetailsRequest => {
  return {
    releaseId,
    ...assembleBaseReleaseSummaryRequestFromForm(values),
  };
};

export const findTimePeriodCoverageGroup = (
  code: string,
  timePeriodCoverageGroups: TimePeriodCoverageGroup[],
) => {
  return (
    timePeriodCoverageGroups.find(group =>
      group.timeIdentifiers
        .map(option => option.identifier.value)
        .some(id => id === code),
    ) || timePeriodCoverageGroups[0]
  );
};

export const getReleaseStatusLabel = (approvalStatus: ReleaseStatus) => {
  switch (approvalStatus) {
    case 'Draft':
      return 'Draft';
    case 'HigherLevelReview':
      return 'In Review';
    case 'Approved':
      return 'Approved';
    default:
      return undefined;
  }
};

const getLiveLatestLabel = (isLive: boolean, isLatest: boolean) => {
  if (isLive && isLatest) {
    return '(Live - Latest release)';
  }
  if (isLive) {
    return '(Live)';
  }
  return '(not Live)';
};

export const getReleaseSummaryLabel = (release: AdminDashboardRelease) =>
  `${release.title} ${getLiveLatestLabel(release.live, release.latestRelease)}`;
