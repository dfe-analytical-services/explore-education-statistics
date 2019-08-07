import { CommonService } from '@admin/services/common/service';
import { getCaptureGroups } from '@admin/services/util/mock/mock-service';
import MockAdapter from 'axios-mock-adapter';

export default async (mock: MockAdapter) => {
  const mockData = (await import(
    /* webpackChunkName: "mock-data" */ './mock-data'
  )).default;

  const service: CommonService = {
    getBasicPublicationDetails: publicationId =>
      Promise.resolve(mockData.getPublicationDetails(publicationId)),
    getReleaseTypes: () => Promise.resolve(mockData.getReleaseTypes()),
  };

  const getPublicationDetailsForReleaseUrl = /\/publications\/(.*)/;
  const getReleaseTypesUrl = /\/meta\/releasetypes/;

  // getBasicPublicationDetails
  mock.onGet(getPublicationDetailsForReleaseUrl).reply(({ url }) => {
    const [publicationId] = getCaptureGroups(
      getPublicationDetailsForReleaseUrl,
      url,
    );
    return [200, service.getBasicPublicationDetails(publicationId)];
  });

  // getReleaseTypes
  mock.onGet(getReleaseTypesUrl).reply(200, service.getReleaseTypes());
};
