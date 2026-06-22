import { PublicationRole } from './PublicationRole';

export interface User {
  id: string;
  name: string;
  email: string;
  role: string;
}

export interface UserWithRoles extends User {
  userPublicationRoles: UserPublicationRole[];
  userPreReleaseRoles: UserPreReleaseRole[];
}

export interface UserPreReleaseRole {
  id: string;
  publication: string;
  release: string;
}

export interface UserPublicationRole {
  id: string;
  publication: string;
  role: PublicationRole;
}

export interface UserPublicationRoleWithUser extends UserPublicationRole {
  userId: string;
  userName: string;
  email: string;
}
