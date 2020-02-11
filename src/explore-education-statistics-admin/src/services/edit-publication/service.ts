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
  createPublication({
    topicId,
    publicationTitle: title,
    selectedContactId: contactId,
    selectedMethodologyId: methodologyId,
    externalMethodology,
  }: CreatePublicationRequest) {
    return client.post(`/topic/${topicId}/publications`, {
      title,
      contactId,
      methodologyId,
      externalMethodology,
    });
  },
};

export default service;
