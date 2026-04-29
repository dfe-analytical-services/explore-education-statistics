import client from '@admin/services/utils/service';
import { PublicationRole } from '../types/PublicationRole';

export interface UserPublicationRoleCreateRequest {
  publicationId: string;
  publicationRole: PublicationRole;
}

export interface UserDrafterRoleCreateRequest {
  email: string;
  publicationId: string;
}

export interface PublicationRolesService {
  addPublicationRole(
    userId: string,
    userPublicationRole: UserPublicationRoleCreateRequest,
  ): Promise<boolean>;
  inviteDrafter(
    userDrafterRoleCreateRequest: UserDrafterRoleCreateRequest,
  ): Promise<boolean>;
  removeUserPublicationRole(userPublicationRoleId: string): Promise<boolean>;
}

const publicationRolesService: PublicationRolesService = {
  addPublicationRole(
    userId: string,
    userPublicationRole: UserPublicationRoleCreateRequest,
  ): Promise<boolean> {
    return client.post(
      `/users/${userId}/publication-roles`,
      userPublicationRole,
    );
  },

  inviteDrafter(
    userDrafterRoleCreateRequest: UserDrafterRoleCreateRequest,
  ): Promise<boolean> {
    return client.post(
      `/users/publication-roles/invite-drafter`,
      userDrafterRoleCreateRequest,
    );
  },

  removeUserPublicationRole(userPublicationRoleId: string): Promise<boolean> {
    return client.delete(`/users/publication-roles/${userPublicationRoleId}`);
  },
};

export default publicationRolesService;
