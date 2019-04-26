export interface User {
  id: string;
  name: string;
  permissions: string[];
}

export class PrototypeLoginService {
  private static currentUser: User;

  private static USERS: User[] = [
    { id: 'user1', name: 'John Smith', permissions: [''] },
    { id: 'user2', name: 'User 2', permissions: [''] },
    { id: 'user3', name: 'User 3', permissions: [''] },
    { id: 'user4', name: 'User 4', permissions: [''] },
    { id: 'user5', name: 'User 5', permissions: [''] },
  ];

  public static getUserList(): User[] {
    return PrototypeLoginService.USERS;
  }

  public static getUser(user: string) {
    return PrototypeLoginService.USERS.filter(_ => _.id === user)[0];
  }
}
