import client from '@admin/services/util/service';
import { UserStatus } from '@admin/services/users/types';

export interface UsersService {
  getUsers(): Promise<UserStatus[]>;
}

const service: UsersService = {
  getUsers(): Promise<UserStatus[]> {
    return client.get<UserStatus[]>('/bau/users');
  },
};

export default service;
