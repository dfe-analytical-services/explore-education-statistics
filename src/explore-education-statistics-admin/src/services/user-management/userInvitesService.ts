import client from '@admin/services/utils/service';
import {
  UserPreReleaseRole,
  UserPublicationRole,
} from '../types/userWithRoles';
import { PublicationRole } from '../types/PublicationRole';
import { GlobalRole } from '../types/GlobalRole';

export interface PendingInvite {
  email: string;
  globalRole: GlobalRole;
  userPublicationRoles: UserPublicationRole[];
  userPreReleaseRoles: UserPreReleaseRole[];
}

export interface UserInvite {
  email: string;
  isBau: boolean;
  userPreReleaseRoles: { releaseId: string }[];
  userPublicationRoles: {
    publicationId: string;
    publicationRole: PublicationRole;
  }[];
}

export interface UserInvitesService {
  getPendingInvites(): Promise<PendingInvite[]>;
  inviteUser: (invite: UserInvite) => Promise<void>;
  cancelInvite: (email: string) => Promise<boolean>;
}

const userInvitesService: UserInvitesService = {
  getPendingInvites(): Promise<PendingInvite[]> {
    return client.get<PendingInvite[]>('/user-invites');
  },

  inviteUser(invite: UserInvite): Promise<void> {
    return client.post(`/user-invites`, invite);
  },

  cancelInvite(email: string): Promise<boolean> {
    return client.delete(`/user-invites`, {
      params: { email },
    });
  },
};

export default userInvitesService;
