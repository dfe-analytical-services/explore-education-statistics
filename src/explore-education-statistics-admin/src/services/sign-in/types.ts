export interface User {
  id: string;
  name: string;
  permissions: string[];
}

export interface Authentication {
  user?: User;
}