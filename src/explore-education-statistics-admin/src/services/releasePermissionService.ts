import client from '@admin/services/utils/service';

export interface ManageAccessPage {
  publicationId: string;
  publicationTitle: string;
  releases: ManageAccessPageRelease[];
}

export interface ManageAccessPageRelease {
  releaseId: string;
  releaseTitle: string;
  userList: ManageAccessPageContributor[];
}

export interface ManageAccessPageContributor {
  userId: string;
  userFullName: string;
  releaseId: string;
  releaseRoleId?: string;
}

const releasePermissionService = {
  getPublicationContributors(publicationId: string): Promise<ManageAccessPage> {
    return client.get(`/publications/${publicationId}/contributors`);
  },
};

export default releasePermissionService;
