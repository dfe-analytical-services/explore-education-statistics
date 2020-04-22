import client from '@admin/services/util/service';
import {
  User,
  UserStatus,
  UserInvite,
  Role,
  ReleaseRole,
  UserUpdate,
} from '@admin/services/users/types';

export interface UsersService {
  getRoles(): Promise<Role[]>;
  getReleaseRoles(): Promise<ReleaseRole[]>;
  getUser(userId: string): Promise<User>;
  getUsers(): Promise<UserStatus[]>;
  getPreReleaseUsers(): Promise<UserStatus[]>;
  getInvitedUsers(): Promise<UserStatus[]>;
  inviteUser: (invite: UserInvite) => Promise<boolean>;
  cancelInvite: (email: string) => Promise<boolean>;
  updateUser: (user: UserUpdate) => Promise<boolean>;
}

const service: UsersService = {
  getRoles(): Promise<Role[]> {
    return client.get<Role[]>('/bau/users/roles');
  },
  getReleaseRoles(): Promise<ReleaseRole[]> {
    return client.get<ReleaseRole[]>('/bau/users/release-roles');
  },
  getUser(userId: string): Promise<User> {
    return client.get<User>(`/bau/users/${userId}`);
  },
  getUsers(): Promise<UserStatus[]> {
    return client.get<UserStatus[]>('/bau/users');
  },
  getPreReleaseUsers(): Promise<UserStatus[]> {
    return client.get<UserStatus[]>('/bau/users/pre-release');
  },
  getInvitedUsers(): Promise<UserStatus[]> {
    return client.get<UserStatus[]>('/bau/users/invite');
  },
  inviteUser(invite: UserInvite): Promise<boolean> {
    return client.post(`/bau/users/invite`, invite);
  },
  cancelInvite(email: string): Promise<boolean> {
    return client.delete(`/bau/users/invite/${email}`);
  },
  updateUser(user: UserUpdate): Promise<boolean> {
    return client.put(`/bau/users/${user.id}`, user);
  },
};

export default service;
