import client from '@admin/services/utils/service';
import { IdTitlePair } from 'src/services/types/common';
import { PublicationRole } from './types/PublicationRole';

export interface UserStatus {
  id: string;
  name: string;
  email: string;
  role: string;
}

export interface User {
  id: string;
  name: string;
  email: string;
  role: string;
  userPublicationRoles: UserPublicationRole[];
  userReleaseRoles: UserReleaseRole[];
}

export interface UserReleaseRole {
  id: string;
  publication: string;
  release: string;
  role: string;
}

export interface UserPublicationRole {
  id: string;
  publication: string;
  role: PublicationRole;
  userName: string;
  email: string;
}

export interface UserReleaseRoleSubmission {
  releaseId: string;
  releaseRole: string;
}

export interface UserPublicationRoleSubmission {
  publicationId: string;
  publicationRole: PublicationRole;
}

export interface UserInvite {
  email: string;
  roleId: string;
  userReleaseRoles: { releaseId: string; releaseRole: string }[];
  userPublicationRoles: {
    publicationId: string;
    publicationRole: PublicationRole;
  }[];
}

export interface PendingInvite {
  email: string;
  name: string;
  role: string;
  userPublicationRoles: UserPublicationRole[];
  userReleaseRoles: UserReleaseRole[];
}

export interface UserUpdate {
  roleId: string;
}

export interface Role {
  id: string;
  name: string;
  normalizedName: string;
}

export interface ResourceRoles {
  Publication?: PublicationRole[];
  Release?: string[];
}

export interface RemoveUser {
  userId: string;
}

export interface UsersService {
  getRoles(): Promise<Role[]>;
  getReleases(): Promise<IdTitlePair[]>;
  getResourceRoles(): Promise<ResourceRoles>;
  getUser(userId: string): Promise<User>;
  deleteUser(email: string): Promise<RemoveUser>;
  addUserReleaseRole: (
    userId: string,
    userReleaseRole: UserReleaseRoleSubmission,
  ) => Promise<boolean>;
  removeUserReleaseRole: (userReleaseRoleId: string) => Promise<boolean>;

  addUserPublicationRole: (
    userId: string,
    userPublicationRole: UserPublicationRoleSubmission,
  ) => Promise<boolean>;
  removeUserPublicationRole: (
    userId: string,
    userPublicationRole: UserPublicationRole,
  ) => Promise<boolean>;

  getUsers(): Promise<UserStatus[]>;
  getPreReleaseUsers(): Promise<UserStatus[]>;
  getPendingInvites(): Promise<PendingInvite[]>;
  inviteUser: (invite: UserInvite) => Promise<boolean>;
  inviteContributor: (
    email: string,
    publicationId: string,
    releaseIds: string[],
  ) => Promise<boolean>;
  removeContributorReleaseInvites: (
    email: string,
    publicationId: string,
  ) => Promise<boolean>;
  cancelInvite: (email: string) => Promise<boolean>;
  updateUser: (userId: string, update: UserUpdate) => Promise<boolean>;
}

const userService: UsersService = {
  getRoles(): Promise<Role[]> {
    return client.get<Role[]>('/user-management/roles');
  },
  getResourceRoles(): Promise<ResourceRoles> {
    return client.get<ResourceRoles>('/user-management/resource-roles');
  },
  getReleases(): Promise<IdTitlePair[]> {
    return client.get<IdTitlePair[]>('/user-management/releases');
  },

  getUsers(): Promise<UserStatus[]> {
    return client.get<UserStatus[]>('/user-management/users');
  },
  getUser(userId: string): Promise<User> {
    return client.get<User>(`/user-management/users/${userId}`);
  },
  updateUser(userId: string, update: UserUpdate): Promise<boolean> {
    return client.put(`/user-management/users/${userId}`, update);
  },
  deleteUser(email: string): Promise<RemoveUser> {
    return client.delete(`/user-management/user/${email}`);
  },

  addUserReleaseRole(
    userId: string,
    userReleaseRole: UserReleaseRoleSubmission,
  ): Promise<boolean> {
    return client.post(
      `/user-management/users/${userId}/release-role`,
      userReleaseRole,
    );
  },
  removeUserReleaseRole(userReleaseRoleId: string): Promise<boolean> {
    return client.delete(
      `/user-management/users/release-role/${userReleaseRoleId}`,
    );
  },

  addUserPublicationRole(
    userId: string,
    userPublicationRole: UserPublicationRoleSubmission,
  ): Promise<boolean> {
    return client.post(
      `/user-management/users/${userId}/publication-role`,
      userPublicationRole,
    );
  },
  removeUserPublicationRole(
    userId: string,
    userPublicationRole: UserPublicationRole,
  ): Promise<boolean> {
    return client.delete(
      `/user-management/users/publication-role/${userPublicationRole.id}`,
    );
  },

  getPreReleaseUsers(): Promise<UserStatus[]> {
    return client.get<UserStatus[]>('/user-management/pre-release');
  },

  getPendingInvites(): Promise<PendingInvite[]> {
    return client.get<PendingInvite[]>('/user-management/invites');
  },
  inviteUser(invite: UserInvite): Promise<boolean> {
    return client.post(`/user-management/invites`, invite);
  },
  inviteContributor(
    email: string,
    publicationId: string,
    releaseIds: string[],
  ): Promise<boolean> {
    return client.post(
      `/user-management/publications/${publicationId}/invites/contributor`,
      {
        email,
        releaseIds,
      },
    );
  },
  removeContributorReleaseInvites(
    email: string,
    publicationId: string,
  ): Promise<boolean> {
    return client.delete(
      `/user-management/publications/${publicationId}/release-invites/contributor`,
      { data: { email } },
    );
  },
  cancelInvite(email: string): Promise<boolean> {
    return client.delete(`/user-management/invites/${email}`);
  },
};

export default userService;
