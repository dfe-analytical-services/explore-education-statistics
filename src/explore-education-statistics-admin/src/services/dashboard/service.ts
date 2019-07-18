import publicationPollyfilla from "@admin/services/dashboard/polyfillas";
import {polyfill} from "@admin/services/util/polyfilla";
import {createClient} from '@admin/services/util/service';
import mocks from './mock/mock-service';
import {AdminDashboardPublication, ThemeAndTopics} from './types';

const apiClient = createClient({
  mockBehaviourRegistrar: mocks,
});

export interface DashboardService {
  getMyThemesAndTopics(): Promise<ThemeAndTopics[]>;
  getMyPublicationsByTopic(
    topicId: string,
  ): Promise<AdminDashboardPublication[]>;
}

const service: DashboardService = {
  getMyThemesAndTopics(): Promise<ThemeAndTopics[]> {
    return apiClient.then(client =>
      client.get<ThemeAndTopics[]>('/me/themes'),
    );
  },
  getMyPublicationsByTopic(
    topicId: string,
  ): Promise<AdminDashboardPublication[]> {
    return apiClient.then(client =>
      client.get<AdminDashboardPublication[]>('/me/publications', {
        params: { topicId },
      }).then(publications => publications.map(publication =>
        polyfill(publication, publicationPollyfilla))),
    );
  },
};

export default service;
