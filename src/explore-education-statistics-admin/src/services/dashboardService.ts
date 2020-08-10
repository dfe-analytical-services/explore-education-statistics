import { ContactDetails } from '@admin/services/contactService';
import { MyRelease } from '@admin/services/releaseService';
import { IdTitlePair } from '@admin/services/types/common';
import client from '@admin/services/utils/service';

export interface ExternalMethodology {
  title: string;
  url: string;
}
export interface AdminDashboardPublication {
  id: string;
  title: string;
  methodology?: IdTitlePair;
  externalMethodology?: ExternalMethodology;
  releases: MyRelease[];
  contact?: ContactDetails;
  permissions: {
    canCreateReleases: boolean;
    canUpdatePublication: boolean;
  };
}

const dashboardService = {
  getMyPublicationsByTopic(
    topicId: string,
  ): Promise<AdminDashboardPublication[]> {
    return client.get<AdminDashboardPublication[]>('/me/publications', {
      params: { topicId },
    });
  },
};

export default dashboardService;
