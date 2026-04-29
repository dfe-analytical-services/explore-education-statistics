import client from '@admin/services/utils/service';

export interface PreReleaseUserSummary {
  email: string;
}

export interface PreReleaseUser extends PreReleaseUserSummary {
  userId: string;
  name: string;
}

export interface PreReleaseInvitePlan {
  alreadyAccepted: string[];
  alreadyInvited: string[];
  invitable: string[];
}

export interface PreReleaseUserService {
  getAllPreReleaseUsers(): Promise<PreReleaseUser[]>;
  getPreReleaseUsers(
    releaseVersionId: string,
  ): Promise<PreReleaseUserSummary[]>;
  getPreReleaseUsersInvitePlan(
    releaseVersionId: string,
    emails: string[],
  ): Promise<PreReleaseInvitePlan>;
  removePreReleaseRoleById(userPreReleaseRoleId: string): Promise<boolean>;
  removePreReleaseRoleByEmail(
    releaseVersionId: string,
    email: string,
  ): Promise<void>;
  grantPreReleaseAccessForMany(
    releaseVersionId: string,
    emails: string[],
  ): Promise<PreReleaseUserSummary[]>;
  grantPreReleaseAccess(userId: string, releaseId: string): Promise<boolean>;
}

const preReleaseUserService: PreReleaseUserService = {
  getAllPreReleaseUsers(): Promise<PreReleaseUser[]> {
    return client.get('/pre-release/users');
  },

  getPreReleaseUsers(
    releaseVersionId: string,
  ): Promise<PreReleaseUserSummary[]> {
    return client.get(
      `/pre-release/release-versions/${releaseVersionId}/users`,
    );
  },

  getPreReleaseUsersInvitePlan(
    releaseVersionId: string,
    emails: string[],
  ): Promise<PreReleaseInvitePlan> {
    return client.post(
      `/pre-release/release-versions/${releaseVersionId}/users/invite-plan`,
      {
        emails,
      },
    );
  },

  removePreReleaseRoleById(userPreReleaseRoleId: string): Promise<boolean> {
    return client.delete(`/pre-release/roles/${userPreReleaseRoleId}`);
  },

  removePreReleaseRoleByEmail(
    releaseVersionId: string,
    email: string,
  ): Promise<void> {
    return client.delete(
      `/pre-release/release-versions/${releaseVersionId}/users/by-email`,
      {
        data: {
          email,
        },
      },
    );
  },

  grantPreReleaseAccessForMany(
    releaseVersionId: string,
    emails: string[],
  ): Promise<PreReleaseUserSummary[]> {
    return client.post(
      `/pre-release/release-versions/${releaseVersionId}/users`,
      {
        emails,
      },
    );
  },

  grantPreReleaseAccess(userId: string, releaseId: string): Promise<boolean> {
    return client.post(`/pre-release/releases/${releaseId}/users/${userId}`);
  },
};

export default preReleaseUserService;
