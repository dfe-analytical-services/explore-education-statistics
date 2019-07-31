import { PublicationService } from '@admin/services/edit-publication/service';
import { CreatePublicationRequest } from '@admin/services/edit-publication/types';
import { generateRandomIntegerString } from '@admin/services/util/mock/mock-service';
import MockAdapter from 'axios-mock-adapter';

export default async (mock: MockAdapter) => {
  const mockData = (await import(
    /* webpackChunkName: "mock-data" */ './mock-data'
  )).default;

  const dashboardMockData = (await import(
    /* webpackChunkName: "dashboard-mock-data" */ '@admin/services/dashboard/mock/mock-data'
  )).default;

  const getMethodologiesUrl = /\/methodologies/;
  const getPublicationAndReleaseContactsUrl = /\/publication\/contacts/;
  const createPublicationUrl = /\/publication\/create/;

  const service: PublicationService = {
    getMethodologies: () => Promise.resolve(mockData.getMethodologies()),
    getPublicationAndReleaseContacts: () =>
      Promise.resolve(mockData.getPublicationAndReleaseContacts()),
    createPublication: _ => Promise.resolve(),
  };

  mock.onGet(getMethodologiesUrl).reply(200, service.getMethodologies());
  mock
    .onGet(getPublicationAndReleaseContactsUrl)
    .reply(200, service.getPublicationAndReleaseContacts());
  mock.onPost(createPublicationUrl).reply(config => {
    const request = JSON.parse(config.data) as CreatePublicationRequest;

    const contacts = mockData.getPublicationAndReleaseContacts();
    const methodologies = mockData.getMethodologies();
    const selectedContact =
      contacts.find(contact => contact.id === request.selectedContactId) ||
      contacts[0];
    const selectedMethodology = methodologies.find(
      methodology => methodology.id === request.selectedMethodologyId,
    );
    const newPublication = {
      id: generateRandomIntegerString(1000000),
      title: request.publicationTitle,
      methodology: selectedMethodology,
      contact: selectedContact,
      releases: [],
    };

    const publicationsForTopic =
      dashboardMockData.dashboardPublicationsByTopicId[request.topicId];

    dashboardMockData.dashboardPublicationsByTopicId[
      request.topicId
    ] = publicationsForTopic
      ? publicationsForTopic.concat(newPublication)
      : [newPublication];

    return [200];
  });
};
