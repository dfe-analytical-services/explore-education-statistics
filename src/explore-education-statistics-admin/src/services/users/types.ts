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

export interface Role {
  id: string;
  name: string;
  normalizedName: string;
}

export default {};
