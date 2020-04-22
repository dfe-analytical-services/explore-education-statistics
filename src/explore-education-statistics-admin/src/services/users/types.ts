import { IdTitlePair } from '../common/types';

export interface UserStatus {
  id: string;
  name: string;
  email: string;
  role: string;
}

export interface User {
  id: string;
  name: string;
  email: string;
  role: string;
  userReleaseRoles: UserReleaseRole[];
}

export interface UserReleaseRole {
  publication: IdTitlePair;
  release: IdTitlePair;
  releaseRole: ReleaseRole;
}

export interface UserInvite {
  email: string;
  roleId: string;
}

export interface UserUpdate {
  id: string;
  roleId: string;
}

export interface Role {
  id: string;
  name: string;
  normalizedName: string;
}

export interface ReleaseRole {
  name: string;
  value: string;
}

export default {};
