import { dateToDayMonthYear } from '@admin/services/common/types';
import { UpdateReleaseSetupDetailsRequest } from '@admin/services/release/edit-release/setup/types';
import { getCaptureGroups } from '@admin/services/util/mock/mock-service';
import MockAdapter from 'axios-mock-adapter';

export default async (mock: MockAdapter) => {
  const mockData = (await import(
    /* webpackChunkName: "mock-data" */ './mock-data'
  )).default;

  const mockCommonData = (await import(
    /* webpackChunkName: "mock-dashboard-data" */ '@admin/services/common/mock/mock-data'
  )).default;

  const getReleaseSetupDetailsUrl = /\/release\/(.*)\/setup/;

  const updateReleaseSetupDetailsUrl = /\/release\/(.*)\/setup/;

  mock.onGet(getReleaseSetupDetailsUrl).reply(({ url }) => {
    const [releaseId] = getCaptureGroups(getReleaseSetupDetailsUrl, url);
    return [200, mockData.getReleaseSetupDetailsForRelease(releaseId)];
  });

  mock.onPost(updateReleaseSetupDetailsUrl).reply(config => {
    const updateRequest = JSON.parse(
      config.data,
    ) as UpdateReleaseSetupDetailsRequest;

    const existingRelease = mockData.getReleaseSetupDetailsForRelease(
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
