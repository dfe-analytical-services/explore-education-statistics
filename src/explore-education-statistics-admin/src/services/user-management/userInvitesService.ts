import client from '@admin/services/utils/service';
import {
  UserPreReleaseRole,
  UserPublicationRole,
} from '../types/userWithRoles';
import { PublicationRole } from '../types/PublicationRole';

export interface PendingInvite {
  email: string;
  role: string;
  userPublicationRoles: UserPublicationRole[];
  userPreReleaseRoles: UserPreReleaseRole[];
}

export interface UserInvite {
  email: string;
  roleId: string;
  userPreReleaseRoles: { releaseId: string }[];
  userPublicationRoles: {
    publicationId: string;
    publicationRole: PublicationRole;
  }[];
}

export interface UserInvitesService {
  getPendingInvites(): Promise<PendingInvite[]>;
  inviteUser: (invite: UserInvite) => Promise<boolean>;
  cancelInvite: (email: string) => Promise<boolean>;
}

const userInvitesService: UserInvitesService = {
  getPendingInvites(): Promise<PendingInvite[]> {
    return client.get<PendingInvite[]>('/user-invites');
  },

  inviteUser(invite: UserInvite): Promise<boolean> {
    return client.post(`/user-invites`, invite);
  },

  cancelInvite(email: string): Promise<boolean> {
    return client.delete(`/user-invites/${email}`);
  },
};

export default userInvitesService;
