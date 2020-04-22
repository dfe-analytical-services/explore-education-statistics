export interface UserStatus {
  id: string;
  name: string;
  email: string;
  role: string;
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
