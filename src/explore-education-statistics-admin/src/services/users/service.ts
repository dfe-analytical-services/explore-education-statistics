import client from '@admin/services/util/service';
import {
  User,
  UserStatus,
  UserInvite,
  Role,
  UserReleaseRole,
  ReleaseRole,
  UserUpdate,
  UserReleaseRoleSubmission,
} from '@admin/services/users/types';

export interface UsersService {
  getRoles(): Promise<Role[]>;
  getReleaseRoles(): Promise<ReleaseRole[]>;

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
  updateUser: (user: UserUpdate) => Promise<boolean>;
}

const service: UsersService = {
  getRoles(): Promise<Role[]> {
    return client.get<Role[]>('/user-management/roles');
  },
  getReleaseRoles(): Promise<ReleaseRole[]> {
    return client.get<ReleaseRole[]>('/user-management/release-roles');
  },
  // user-management/releases (ID,Title)

  getUsers(): Promise<UserStatus[]> {
    return client.get<UserStatus[]>('/user-management/users');
  },
  getUser(userId: string): Promise<User> {
    return client.get<User>(`/user-management/users/${userId}`);
  },
  updateUser(user: UserUpdate): Promise<boolean> {
    return client.put(`/user-management/users/${user.id}`, user);
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
      `/user-management/users/${userId}/release-role/${userReleaseRole.id}`,
    );
  },

  getPreReleaseUsers(): Promise<UserStatus[]> {
    return client.get<UserStatus[]>('/user-management/users/pre-release');
  },

  getInvitedUsers(): Promise<UserStatus[]> {
    return client.get<UserStatus[]>('/user-management/invites');
  },
  inviteUser(invite: UserInvite): Promise<boolean> {
    return client.post(`/user-management/invites`, invite);
  },
  cancelInvite(email: string): Promise<boolean> {
    return client.delete(`/user-management/invite/${email}`);
  },
};

export default service;
