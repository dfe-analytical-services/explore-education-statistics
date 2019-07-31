import {PublicationService} from "@admin/services/edit-publication/service";
import MockAdapter from 'axios-mock-adapter';

export default async (mock: MockAdapter) => {
  const mockData = (await import(
    /* webpackChunkName: "mock-data" */ './mock-data'
  )).default;

  const getMethodologiesUrl = /\/methodologies/;
  const getPublicationAndReleaseContactsUrl = /\/publication\/contacts/;

  const service: PublicationService = {
    getMethodologies: () => Promise.resolve(mockData.getMethodologies()),
    getPublicationAndReleaseContacts: () => Promise.resolve(mockData.getPublicationAndReleaseContacts()),
  };

  mock.onGet(getMethodologiesUrl).reply(200, service.getMethodologies());
  mock.onGet(getPublicationAndReleaseContactsUrl).reply(200, service.getPublicationAndReleaseContacts());
};
