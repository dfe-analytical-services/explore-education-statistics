import { GlobalPermissions, User } from '@admin/services/sign-in/types';
import client from '@admin/services/util/service';

export type PreReleaseAccess = 'Before' | 'After' | 'Within' | 'NoneSet';

export interface PreReleaseWindowStatus {
  preReleaseAccess: PreReleaseAccess;
  preReleaseWindowStartTime: Date;
  preReleaseWindowEndTime: Date;
}

const permissionService = {
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
  canMarkReleaseAsDraft: (releaseId: string): Promise<boolean> => {
    return client.get(`/permissions/release/${releaseId}/status/draft`);
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
  canMarkMethodologyAsDraft: (methodologyId: string): Promise<boolean> => {
    return client.get(`/permissions/methodology/${methodologyId}/status/draft`);
  },
  canApproveMethodology: (methodologyId: string): Promise<boolean> => {
    return client.get(
      `/permissions/methodology/${methodologyId}/status/approve`,
    );
  },
  getPreReleaseWindowStatus: (
    releaseId: string,
  ): Promise<PreReleaseWindowStatus> => {
    return client
      .get<PreReleaseWindowStatus>(
        `/permissions/release/${releaseId}/prerelease/status`,
      )
      .then(status => ({
        ...status,
        preReleaseWindowStartTime: new Date(status.preReleaseWindowStartTime),
        preReleaseWindowEndTime: new Date(status.preReleaseWindowEndTime),
      }));
  },
};

export default permissionService;
