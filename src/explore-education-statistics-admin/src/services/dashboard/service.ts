import { ReleaseStatus } from '@admin/components/ReleaseServiceStatus';
import { PrereleaseContactDetails } from '@admin/services/common/types';
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

const service = {
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
      .then(publications => publications || [])
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
      `/release/${releaseId}/prerelease-contact/${email}`,
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
