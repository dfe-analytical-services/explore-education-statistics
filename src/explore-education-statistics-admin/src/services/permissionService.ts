import { User } from '@admin/contexts/AuthContext';
import client from '@admin/services/utils/service';
import { parseISO } from 'date-fns';

export interface GlobalPermissions {
  canAccessSystem: boolean;
  canAccessPrereleasePages: boolean;
  canAccessAnalystPages: boolean;
  canAccessUserAdministrationPages: boolean;
  canAccessAllImports: boolean;
  canAccessMethodologyAdministrationPages: boolean;
  canManageAllTaxonomy: boolean;
}

export interface ReleaseStatusPermissions {
  canMarkDraft: boolean;
  canMarkHigherLevelReview: boolean;
  canMarkApproved: boolean;
}

export type PreReleaseAccess = 'Before' | 'After' | 'Within' | 'NoneSet';

export interface PreReleaseWindowStatus {
  access: PreReleaseAccess;
  start: Date;
  end: Date;
}

const permissionService = {
  getGlobalPermissions(): Promise<GlobalPermissions> {
    return client.get(`/permissions/access`);
  },
  canAccessPrereleasePages(user?: User): Promise<boolean> {
    return Promise.resolve(!!user?.permissions.canAccessPrereleasePages);
  },
  canUpdateRelease(releaseId: string): Promise<boolean> {
    return client.get(`/permissions/release/${releaseId}/update`);
  },
  getReleaseStatusPermissions(
    releaseId: string,
  ): Promise<ReleaseStatusPermissions> {
    return client.get(`/permissions/release/${releaseId}/status`);
  },
  canMakeAmendmentOfRelease(releaseId: string): Promise<boolean> {
    return client.get(`/permissions/release/${releaseId}/amend`);
  },
  canCreatePublicationForTopic(topicId: string): Promise<boolean> {
    return client.get(`/permissions/topic/${topicId}/publication/create`);
  },
  canCreateReleaseForPublication(publicationId: string): Promise<boolean> {
    return client.get(
      `/permissions/publication/${publicationId}/release/create`,
    );
  },
  canUpdateMethodology(methodologyId: string): Promise<boolean> {
    return client.get(`/permissions/methodology/${methodologyId}/update`);
  },
  canMarkMethodologyAsDraft(methodologyId: string): Promise<boolean> {
    return client.get(`/permissions/methodology/${methodologyId}/status/draft`);
  },
  canApproveMethodology(methodologyId: string): Promise<boolean> {
    return client.get(
      `/permissions/methodology/${methodologyId}/status/approve`,
    );
  },
  getPreReleaseWindowStatus(
    releaseId: string,
  ): Promise<PreReleaseWindowStatus> {
    return client
      .get<{
        access: PreReleaseAccess;
        start: string;
        end: string;
      }>(`/permissions/release/${releaseId}/prerelease/status`)
      .then(status => ({
        access: status.access,
        start: parseISO(status.start),
        end: parseISO(status.end),
      }));
  },
};

export default permissionService;
