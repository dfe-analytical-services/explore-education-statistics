import {createClient} from "@admin/services/util/service";

const apiClient = createClient({});

export interface User {
  id: string;
  name: string;
  permissions: string[];
}

export interface Authentication {
  user?: User;
}

const getUserDetails: () => Promise<User> = () =>
  apiClient.then(client => client.get('/users/mydetails'));

export default {
  getUserDetails,
}