import {
  BasicPublicationDetails,
  IdTitlePair,
} from '@admin/services/common/types';
import client from '@admin/services/util/service';

export interface CommonService {
  getBasicPublicationDetails(
    publicationId: string,
  ): Promise<BasicPublicationDetails>;
  getReleaseTypes(): Promise<IdTitlePair[]>;
}

const service: CommonService = {
  getBasicPublicationDetails(
    publicationId: string,
  ): Promise<BasicPublicationDetails> {
    return client.get<BasicPublicationDetails>(
      `/publications/${publicationId}`,
    );
  },
  getReleaseTypes(): Promise<IdTitlePair[]> {
    return client.get<IdTitlePair[]>('/meta/releasetypes');
  },
};

export default service;
