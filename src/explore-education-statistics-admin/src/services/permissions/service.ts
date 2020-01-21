import client from '@admin/services/util/service';

const service = {
  canAccessSystem: (): Promise<boolean> => {
    return client.get(`/permissions/access`);
  },
  canManageAllUsers: (): Promise<boolean> => {
    return client.get(`/permissions/users/manage`);
  },
  canManageAllMethodologies: (): Promise<boolean> => {
    return client.get(`/permissions/methodologies/manage`);
  },
  canAccessAdministration: (): Promise<boolean> => {
    return client.get(`/permissions/administration/access`);
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
