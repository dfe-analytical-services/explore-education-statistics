import { ReleaseStatus } from '@admin/components/ReleaseServiceStatus';
import { PrereleaseContactDetails } from '@admin/services/common/types';
import client from '@admin/services/util/service';

import {
  AdminDashboardPublication,
  AdminDashboardRelease,
  ThemeAndTopics,
} from './types';

const service = {
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
  getDraftReleases(): Promise<AdminDashboardRelease[]> {
    return client.get<AdminDashboardRelease[]>('/releases/draft');
  },
  getScheduledReleases(): Promise<AdminDashboardRelease[]> {
    return client.get<AdminDashboardRelease[]>('/releases/scheduled');
  },
  getAvailablePreReleaseContacts(): Promise<PrereleaseContactDetails[]> {
    return client.get<PrereleaseContactDetails[]>('/prerelease/contacts');
  },
  getPreReleaseContactsForRelease(
    releaseId: string,
  ): Promise<PrereleaseContactDetails[]> {
    return client.get<PrereleaseContactDetails[]>(
      `/release/${releaseId}/prerelease-contacts`,
    );
  },
  addPreReleaseContactToRelease(
    releaseId: string,
    email: string,
  ): Promise<PrereleaseContactDetails[]> {
    return client.post<PrereleaseContactDetails[]>(
      `/release/${releaseId}/prerelease-contact`,
      {
        email,
      },
    );
  },
  removePreReleaseContactFromRelease(
    releaseId: string,
    email: string,
  ): Promise<PrereleaseContactDetails[]> {
    return client.delete<PrereleaseContactDetails[]>(
      `/release/${releaseId}/prerelease-contact/${email}`,
    );
  },
  getReleaseStatus(releaseId: string): Promise<ReleaseStatus> {
    return client.get<ReleaseStatus>(`/releases/${releaseId}/status`);
  },
};

export default service;
