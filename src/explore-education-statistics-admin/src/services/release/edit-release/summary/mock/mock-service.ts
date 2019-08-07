import { dateToDayMonthYear } from '@admin/services/common/types';
import { UpdateReleaseSummaryDetailsRequest } from '@admin/services/release/edit-release/summary/types';
import { getCaptureGroups } from '@admin/services/util/mock/mock-service';
import MockAdapter from 'axios-mock-adapter';

export default async (mock: MockAdapter) => {
  const mockData = (await import(
    /* webpackChunkName: "mock-data" */ './mock-data'
  )).default;

  const mockCommonData = (await import(
    /* webpackChunkName: "mock-dashboard-data" */ '@admin/services/common/mock/mock-data'
  )).default;

  const getReleaseSummaryDetailsUrl = /\/releases\/(.*)\/summary/;

  const updateReleaseSummaryDetailsUrl = /\/releases\/(.*)\/summary/;

  mock.onGet(getReleaseSummaryDetailsUrl).reply(({ url }) => {
    const [releaseId] = getCaptureGroups(getReleaseSummaryDetailsUrl, url);
    return [200, mockData.getReleaseSummaryDetailsForRelease(releaseId)];
  });

  mock.onPost(updateReleaseSummaryDetailsUrl).reply(config => {
    const updateRequest = JSON.parse(
      config.data,
    ) as UpdateReleaseSummaryDetailsRequest;

    const existingRelease = mockData.getReleaseSummaryDetailsForRelease(
      updateRequest.releaseId,
    );

    existingRelease.timePeriodCoverageCode =
      updateRequest.timePeriodCoverage.value;
    existingRelease.scheduledPublishDate = dateToDayMonthYear(
      updateRequest.publishScheduled,
    );
    existingRelease.nextReleaseExpectedDate = updateRequest.nextReleaseExpected;
    existingRelease.releaseType =
      mockCommonData
        .getReleaseTypes()
        .find(type => type.id === updateRequest.releaseTypeId) ||
      mockCommonData.getReleaseTypes()[0];
    /* eslint-enable no-param-reassign */

    return [200];
  });
};
