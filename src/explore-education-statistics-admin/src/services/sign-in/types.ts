export interface GlobalPermissions {
  canAccessSystem: boolean;
  canAccessPrereleasePages: boolean;
  canAccessAnalystPages: boolean;
  canAccessUserAdministrationPages: boolean;
  canAccessMethodologyAdministrationPages: boolean;
}

export interface User {
  id: string;
  name: string;
  permissions: GlobalPermissions;
  validToken?: boolean;
}

export interface Authentication {
  user?: User;
}
