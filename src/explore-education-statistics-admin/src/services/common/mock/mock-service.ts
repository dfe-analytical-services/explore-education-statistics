import { CommonService } from '@admin/services/common/service';
import getCaptureGroups from '@admin/services/util/mock/mock-service';
import MockAdapter from 'axios-mock-adapter';

export default async (mock: MockAdapter) => {
  const mockData = (await import(
    /* webpackChunkName: "mock-data" */ './mock-data'
  )).default;

  const service: CommonService = {
    getPublicationDetailsForRelease: releaseId =>
      Promise.resolve(mockData.getPublicationDetailsForRelease(releaseId)),
  };

  const getPublicationDetailsForReleaseUrl = /\/release\/(.*)\/publication/;

  // getPublicationDetailsForRelease
  mock.onGet(getPublicationDetailsForReleaseUrl).reply(({ url }) => {
    const [releaseId] = getCaptureGroups(
      getPublicationDetailsForReleaseUrl,
      url,
    );
    return [200, service.getPublicationDetailsForRelease(releaseId)];
  });
};
