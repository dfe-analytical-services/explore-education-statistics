import client from '@admin/services/util/service';
import { UserStatus, UserInvite, Role } from '@admin/services/users/types';

export interface UsersService {
  getRoles(): Promise<Role[]>;
  getUser(userId: string): Promise<UserStatus>;
  getUsers(): Promise<UserStatus[]>;
  getPreReleaseUsers(): Promise<UserStatus[]>;
  getPendingInvites(): Promise<UserStatus[]>;
  inviteUser: (invite: UserInvite) => Promise<boolean>;
  cancelInvite: (email: string) => Promise<boolean>;
}

const service: UsersService = {
  getRoles(): Promise<Role[]> {
    return client.get<Role[]>('/bau/users/roles');
  },
  getUser(userId: string): Promise<UserStatus> {
    return client.get<UserStatus>(`/bau/users/${userId}`);
  },
  getUsers(): Promise<UserStatus[]> {
    return client.get<UserStatus[]>('/bau/users');
  },
  getPreReleaseUsers(): Promise<UserStatus[]> {
    return client.get<UserStatus[]>('/bau/users/pre-release');
  },
  getPendingInvites(): Promise<UserStatus[]> {
    return client.get<UserStatus[]>('/bau/users/pending');
  },
  inviteUser(invite: UserInvite): Promise<boolean> {
    return client.post(`/bau/users/invite`, invite);
  },
  cancelInvite(email: string): Promise<boolean> {
    return client.delete(`/bau/users/invite/${email}`);
  },
};

export default service;
