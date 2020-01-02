export interface User {
  id: string;
  name: string;
  permissions: string[];
  validToken?: boolean;
}

export interface Authentication {
  user?: User;
}
