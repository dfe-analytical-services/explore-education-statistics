import { PublicationService } from '@admin/services/edit-publication/service';
import {
  getCaptureGroups,
  generateRandomIntegerString,
} from '@admin/services/util/mock/mock-service';
import MockAdapter from 'axios-mock-adapter';

export default async (mock: MockAdapter) => {
  const mockData = (await import(
    /* webpackChunkName: "mock-data" */ './mock-data'
  )).default;

  const dashboardMockData = (await import(
    /* webpackChunkName: "dashboard-mock-data" */ '@admin/services/dashboard/mock/mock-data'
  )).default;

  const publicationMockData = (await import(
    /* webpackChunkName: "common-mock-data" */ '@admin/services/common/mock/mock-data'
    )).default;

  const getMethodologiesUrl = /\/methodologies/;
  const getPublicationAndReleaseContactsUrl = /\/contacts/;
  const createPublicationUrl = /\/topic\/(.*)\/publications/;

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
    const topicId = getCaptureGroups(createPublicationUrl, config.url)[0];

    const request = JSON.parse(config.data) as {
      title: string;
      contactId: string;
      methodologyId: string;
    };

    const contacts = mockData.getPublicationAndReleaseContacts();
    const methodologies = mockData.getMethodologies();
    const selectedContact =
      contacts.find(contact => contact.id === request.contactId) || contacts[0];
    const selectedMethodology = methodologies.find(
      methodology => methodology.id === request.methodologyId,
    );

    const newPublication = {
      id: generateRandomIntegerString(),
      title: request.title,
      methodology: selectedMethodology,
      contact: selectedContact,
      releases: [],
    };

    const publicationsForTopic =
      dashboardMockData.dashboardPublicationsByTopicId[topicId];

    dashboardMockData.dashboardPublicationsByTopicId[
      topicId
    ] = publicationsForTopic
      ? publicationsForTopic.concat(newPublication)
      : [newPublication];

    publicationMockData.publicationDetailsByPublicationId[newPublication.id] = {
      id: newPublication.id,
      title: newPublication.title,
      contact: newPublication.contact,
    };

    return [200];
  });
};
