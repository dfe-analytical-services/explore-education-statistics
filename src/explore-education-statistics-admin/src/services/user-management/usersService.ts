import client from '@admin/services/utils/service';
import { User, UserWithRoles } from '../types/userWithRoles';

export interface UserGlobalRoleUpdateRequest {
  roleId: string;
}

export interface RemoveUser {
  userId: string;
}

export interface UsersService {
  getAllUsers(): Promise<User[]>;
  getUser(userId: string): Promise<UserWithRoles>;
  updateUserGlobalRole: (
    userId: string,
    update: UserGlobalRoleUpdateRequest,
  ) => Promise<boolean>;
  deleteUser(email: string): Promise<RemoveUser>;
}

const userService: UsersService = {
  getAllUsers(): Promise<User[]> {
    return client.get<User[]>('/users');
  },

  getUser(userId: string): Promise<UserWithRoles> {
    return client.get<UserWithRoles>(`users/${userId}`);
  },

  updateUserGlobalRole(
    userId: string,
    update: UserGlobalRoleUpdateRequest,
  ): Promise<boolean> {
    return client.put(`users/${userId}`, update);
  },

  deleteUser(email: string): Promise<RemoveUser> {
    return client.delete(`users/${email}`);
  },
};

export default userService;
