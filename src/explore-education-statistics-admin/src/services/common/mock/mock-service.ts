import {CommonService} from "@admin/services/common/service";
import MockAdapter from 'axios-mock-adapter';

export default async (mock: MockAdapter) => {
  const mockData = (await import(
    /* webpackChunkName: "mock-data" */ './mock-data'
  )).default;

  const service: CommonService = {
    getPublicationDetailsForRelease: releaseId =>
      Promise.resolve(mockData.getPublicationDetailsForRelease(releaseId)),
  }

  // getPublicationDetailsForRelease
  mock.onGet(/\/release\/.*\/publication/).reply(({ url }) => {
    const releaseIdMatch = url ? url.match(/\/release\/(.*)\/publication/) : [''];
    const releaseId = releaseIdMatch ? releaseIdMatch[1] : '';
    return [200, service.getPublicationDetailsForRelease(releaseId)];
  });
};
