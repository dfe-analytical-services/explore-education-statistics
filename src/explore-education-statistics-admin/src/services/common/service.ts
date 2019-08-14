import {BasicPublicationDetails, IdTitlePair, TimePeriodCoverageGroup,} from '@admin/services/common/types';
import client from '@admin/services/util/service';

export interface CommonService {
  getBasicPublicationDetails(
    publicationId: string,
  ): Promise<BasicPublicationDetails>;
  getReleaseTypes(): Promise<IdTitlePair[]>;
  getTimePeriodCoverageGroups(): Promise<TimePeriodCoverageGroup[]>;
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
  getTimePeriodCoverageGroups(): Promise<TimePeriodCoverageGroup[]> {
    return client.get<TimePeriodCoverageGroup[]>('/meta/timeidentifiers');
  },
};

export default service;
