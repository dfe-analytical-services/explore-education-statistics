import { GlobalPermissions, User } from '@admin/services/sign-in/types';
import client from '@admin/services/util/service';

const service = {
  getGlobalPermissions: (): Promise<GlobalPermissions> => {
    return client.get(`/permissions/access`);
  },
  canAccessPrereleasePages: (user?: User): Promise<boolean> => {
    return Promise.resolve(
      user ? user.permissions.canAccessPrereleasePages : false,
    );
  },
  canUpdateRelease: (releaseId: string): Promise<boolean> => {
    return client.get(`/permissions/release/${releaseId}/update`);
  },
  canSubmitReleaseForHigherLevelReview: (
    releaseId: string,
  ): Promise<boolean> => {
    return client.get(`/permissions/release/${releaseId}/status/submit`);
  },
  canApproveRelease: (releaseId: string): Promise<boolean> => {
    return client.get(`/permissions/release/${releaseId}/status/approve`);
  },
  canCreatePublicationForTopic: (topicId: string): Promise<boolean> => {
    return client.get(`/permissions/topic/${topicId}/publication/create`);
  },
  canCreateReleaseForPublication: (publicationId: string): Promise<boolean> => {
    return client.get(
      `/permissions/publication/${publicationId}/release/create`,
    );
  },
};

export default service;
