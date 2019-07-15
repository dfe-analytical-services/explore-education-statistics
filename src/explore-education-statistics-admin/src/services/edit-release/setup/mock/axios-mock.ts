import {ReleaseSetupDetailsUpdateRequest} from "@admin/services/edit-release/setup/types";
import { AxiosInstance } from 'axios';

export default {
  createMockContentApiAxiosInstance: async (axiosInstance: AxiosInstance) => {
    const MockAdaptor = (await import(
      /* webpackChunkName: "axios-mock-adapter" */ 'axios-mock-adapter'
    )).default;

    const mockData = (await import(
      /* webpackChunkName: "mock-data" */ './mock-data'
    )).default;

    const mock = new MockAdaptor(axiosInstance);

    // getReleaseSetupDetails
    mock.onGet(/\/release\/.*\/setup/).reply(({ url }) => {
      const releaseIdMatch = url
        ? url.match(/\/release\/(.*)\/setup/)
        : [''];
      const releaseId = releaseIdMatch ? releaseIdMatch[1] : '';
      return [
        200,
        mockData.getReleaseSetupDetailsForRelease(releaseId),
      ];
    });

    // updateReleaseSetupDetails
    mock.onPost(/\/release\/.*\/setup/).reply((config) => {

      const updateRequest = JSON.parse(config.data) as ReleaseSetupDetailsUpdateRequest;

      const existingRelease = mockData.getReleaseSetupDetailsForRelease(
        updateRequest.id,
      );

      /* eslint-disable no-param-reassign */
      existingRelease.timePeriodCoverageCode = updateRequest.timePeriodCoverageCode;
      existingRelease.timePeriodCoverageStartDate = updateRequest.timePeriodCoverageStartDate;
      existingRelease.scheduledPublishDate = updateRequest.scheduledPublishDate;
      existingRelease.nextReleaseExpectedDate = updateRequest.nextReleaseExpectedDate;
      existingRelease.releaseType = updateRequest.releaseType;
      /* eslint-enable no-param-reassign */

      return [200];
    });

    return axiosInstance;
  },
};