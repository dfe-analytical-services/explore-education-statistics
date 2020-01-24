import { ContactDetails, IdTitlePair } from '@admin/services/common/types';
import { CreatePublicationRequest } from '@admin/services/edit-publication/types';
import client from '@admin/services/util/service';

const service = {
  getMethodologies(): Promise<IdTitlePair[]> {
    return client.get<IdTitlePair[]>('/methodologies');
  },
  getPublicationAndReleaseContacts(): Promise<ContactDetails[]> {
    return client.get<ContactDetails[]>('/contacts');
  },
  createPublication(createRequest: CreatePublicationRequest) {
    return client.post(`/topic/${createRequest.topicId}/publications`, {
      title: createRequest.publicationTitle,
      contactId: createRequest.selectedContactId,
      methodologyId: createRequest.selectedMethodologyId,
    });
  },
};

export default service;
