import client from '@admin/services/utils/service';
import { PublicationRole } from '../types/PublicationRole';
import { UserPublicationRoleWithUser } from '../types/userWithRoles';

export interface UserPublicationRoleInvite {
  roleId: string;
  role: PublicationRole;
  userId: string;
  email: string;
}

export interface UserPublicationRoleCreateRequest {
  publicationId: string;
  publicationRole: PublicationRole;
}

export interface UserDrafterRoleCreateRequest {
  email: string;
  publicationId: string;
}

export interface PublicationRolesService {
  listPublicationRoles(
    publicationId: string,
  ): Promise<UserPublicationRoleWithUser[]>;
  listPublicationRoleInvites(
    publicationId: string,
  ): Promise<UserPublicationRoleInvite[]>;
  addPublicationRole(
    userId: string,
    userPublicationRole: UserPublicationRoleCreateRequest,
  ): Promise<boolean>;
  inviteDrafter(
    userDrafterRoleCreateRequest: UserDrafterRoleCreateRequest,
  ): Promise<boolean>;
  removeUserPublicationRole(userPublicationRoleId: string): Promise<boolean>;
  removePublicationDrafter(userPublicationRoleId: string): Promise<boolean>;
}

const publicationRolesService: PublicationRolesService = {
  listPublicationRoles(
    publicationId: string,
  ): Promise<UserPublicationRoleWithUser[]> {
    return client.get(`/publications/${publicationId}/publication-roles`);
  },

  listPublicationRoleInvites(
    publicationId: string,
  ): Promise<UserPublicationRoleInvite[]> {
    return client.get(
      `/publications/${publicationId}/publication-role-invites`,
    );
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

  removeUserPublicationRole(userPublicationRoleId: string): Promise<boolean> {
    return client.delete(`/users/publication-roles/${userPublicationRoleId}`);
  },

  removePublicationDrafter(userPublicationRoleId: string): Promise<boolean> {
    return client.delete(
      `/users/publication-roles/drafters/${userPublicationRoleId}`,
    );
  },
};

export default publicationRolesService;
