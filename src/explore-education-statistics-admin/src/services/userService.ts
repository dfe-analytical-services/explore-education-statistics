import client from '@admin/services/utils/service';
import { IdTitlePair } from 'src/services/types/common';

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
  userReleaseRoles: UserReleaseRole[];
}

export interface UserReleaseRole {
  id: string;
  publication: string;
  release: string;
  role: string;
}

export interface UserReleaseRoleSubmission {
  releaseId: string;
  releaseRole: string;
}

export interface UserInvite {
  email: string;
  roleId: string;
}

export interface UserUpdate {
  roleId: string;
}

export interface Role {
  id: string;
  name: string;
  normalizedName: string;
}

export interface ReleaseRole {
  name: string;
  value: string;
}

export interface UsersService {
  getRoles(): Promise<Role[]>;
  getReleaseRoles(): Promise<ReleaseRole[]>;
  getReleases(): Promise<IdTitlePair[]>;

  getUser(userId: string): Promise<User>;

  addUserReleaseRole: (
    userId: string,
    userReleaseRole: UserReleaseRoleSubmission,
  ) => Promise<boolean>;
  removeUserReleaseRole: (
    userId: string,
    userReleaseRole: UserReleaseRole,
  ) => Promise<boolean>;

  getUsers(): Promise<UserStatus[]>;
  getPreReleaseUsers(): Promise<UserStatus[]>;
  getInvitedUsers(): Promise<UserStatus[]>;
  inviteUser: (invite: UserInvite) => Promise<boolean>;
  cancelInvite: (email: string) => Promise<boolean>;
  updateUser: (userId: string, update: UserUpdate) => Promise<boolean>;
}

const userService: UsersService = {
  getRoles(): Promise<Role[]> {
    return client.get<Role[]>('/user-management/roles');
  },
  getReleaseRoles(): Promise<ReleaseRole[]> {
    return client.get<ReleaseRole[]>('/user-management/release-roles');
  },
  getReleases(): Promise<IdTitlePair[]> {
    return client.get<IdTitlePair[]>('/user-management/releases');
  },
  // user-management/releases (ID,Title)

  getUsers(): Promise<UserStatus[]> {
    return client.get<UserStatus[]>('/user-management/users');
  },
  getUser(userId: string): Promise<User> {
    return client.get<User>(`/user-management/users/${userId}`);
  },
  updateUser(userId: string, update: UserUpdate): Promise<boolean> {
    return client.put(`/user-management/users/${userId}`, update);
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
  removeUserReleaseRole(
    userId: string,
    userReleaseRole: UserReleaseRole,
  ): Promise<boolean> {
    return client.delete(
      `/user-management/users/release-role/${userReleaseRole.id}`,
    );
  },

  getPreReleaseUsers(): Promise<UserStatus[]> {
    return client.get<UserStatus[]>('/user-management/pre-release');
  },

  getInvitedUsers(): Promise<UserStatus[]> {
    return client.get<UserStatus[]>('/user-management/invites');
  },
  inviteUser(invite: UserInvite): Promise<boolean> {
    return client.post(`/user-management/invites`, invite);
  },
  cancelInvite(email: string): Promise<boolean> {
    return client.delete(`/user-management/invites/${email}`);
  },
};

export default userService;
