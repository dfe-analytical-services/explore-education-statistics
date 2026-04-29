import client from '@admin/services/utils/service';

export interface Role {
  id: string;
  name: string;
  normalizedName: string;
}

export interface GlobalRolesService {
  getRoles(): Promise<Role[]>;
}

const globalRolesService: GlobalRolesService = {
  getRoles(): Promise<Role[]> {
    return client.get<Role[]>('/user-management/roles');
  },
};

export default globalRolesService;
