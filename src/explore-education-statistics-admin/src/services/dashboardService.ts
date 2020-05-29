import { ContactDetails } from '@admin/services/contactService';
import { Release } from '@admin/services/releaseService';
import { IdTitlePair } from '@admin/services/types/common';
import client from '@admin/services/utils/service';

export interface ThemeAndTopics {
  title: string;
  id: string;
  topics: {
    title: string;
    id: string;
  }[];
}

export interface ExternalMethodology {
  title: string;
  url: string;
}

export interface AdminDashboardPublication {
  id: string;
  title: string;
  methodology?: IdTitlePair;
  externalMethodology: ExternalMethodology;
  releases: Release[];
  contact: ContactDetails;
  permissions: {
    canCreateReleases: boolean;
  };
}

const dashboardService = {
  getMyThemesAndTopics(): Promise<ThemeAndTopics[]> {
    return client.get<ThemeAndTopics[]>('/me/themes');
  },
  getMyPublicationsByTopic(
    topicId: string,
  ): Promise<AdminDashboardPublication[]> {
    return client.get<AdminDashboardPublication[]>('/me/publications', {
      params: { topicId },
    });
  },
};

export default dashboardService;
