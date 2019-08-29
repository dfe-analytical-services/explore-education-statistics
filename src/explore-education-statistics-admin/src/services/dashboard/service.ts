import {
  publicationPolyfilla,
  releasePolyfilla,
} from '@admin/services/dashboard/polyfillas';
import client from '@admin/services/util/service';

import {
  AdminDashboardPublication,
  AdminDashboardRelease,
  ThemeAndTopics,
} from './types';

export interface DashboardService {
  getMyThemesAndTopics(): Promise<ThemeAndTopics[]>;
  getMyPublicationsByTopic(
    topicId: string,
  ): Promise<AdminDashboardPublication[]>;
  getDraftReleases(): Promise<AdminDashboardRelease[]>;
  getScheduledReleases(): Promise<AdminDashboardRelease[]>;
}

const service: DashboardService = {
  getMyThemesAndTopics(): Promise<ThemeAndTopics[]> {
    return client.get<ThemeAndTopics[]>('/me/themes');
  },
  getMyPublicationsByTopic(
    topicId: string,
  ): Promise<AdminDashboardPublication[]> {
    return client
      .get<AdminDashboardPublication[]>('/me/publications', {
        params: { topicId },
      })
      .then(publications => publications.map(publicationPolyfilla));
  },
  getDraftReleases(): Promise<AdminDashboardRelease[]> {
    return client
      .get<AdminDashboardRelease[]>('/releases/draft')
      .then(releases => releases.map(releasePolyfilla));
  },
  getScheduledReleases(): Promise<AdminDashboardRelease[]> {
    return client
      .get<AdminDashboardRelease[]>('/releases/scheduled')
      .then(releases => releases.map(releasePolyfilla));
  },
};

export default service;
