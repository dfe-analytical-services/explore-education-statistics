import client from '@admin/services/utils/service';
import { PublicationRole } from '../types/PublicationRole';
import { UserPublicationRoleWithUser } from '../types/userWithRoles';

export interface UserPublicationRoleCreateRequest {
  publicationId: string;
  publicationRole: PublicationRole;
}

export interface UserDrafterRoleCreateRequest {
  email: string;
  publicationId: string;
}

export interface UpdatePublicationDraftersRequest {
  userIds: string[];
}

export interface PublicationRolesService {
  listPublicationRoles(
    publicationId: string,
  ): Promise<UserPublicationRoleWithUser[]>;
  addPublicationRole(
    userId: string,
    userPublicationRole: UserPublicationRoleCreateRequest,
  ): Promise<boolean>;
  inviteDrafter(
    userDrafterRoleCreateRequest: UserDrafterRoleCreateRequest,
  ): Promise<boolean>;
  updatePublicationDrafters(
    publicationId: string,
    updatePublicationDraftersRequest: UpdatePublicationDraftersRequest,
  ): Promise<boolean>;
  removeUserPublicationRole(userPublicationRoleId: string): Promise<boolean>;
}

const publicationRolesService: PublicationRolesService = {
  listPublicationRoles(
    publicationId: string,
  ): Promise<UserPublicationRoleWithUser[]> {
    return client.get(`/publications/${publicationId}/publication-roles`);
  },

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

  updatePublicationDrafters(
    publicationId: string,
    updatePublicationDraftersRequest: UpdatePublicationDraftersRequest,
  ): Promise<boolean> {
    return client.patch(
      `publications/${publicationId}/update-drafters`,
      updatePublicationDraftersRequest,
    );
  },

  removeUserPublicationRole(userPublicationRoleId: string): Promise<boolean> {
    return client.delete(`/users/publication-roles/${userPublicationRoleId}`);
  },
};

export default publicationRolesService;
