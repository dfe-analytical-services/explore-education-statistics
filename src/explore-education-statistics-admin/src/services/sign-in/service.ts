import { User } from '@admin/services/sign-in/types';
import { createClient } from '@admin/services/util/service';
import mocks from './mock/mock-service';

const apiClient = createClient({
  mockBehaviourRegistrar: mocks,
});

export interface LoginService {
  getUserDetails: () => Promise<User>;
  getSignInLink: () => string;
  getSignOutLink: () => string;
}

const service: LoginService = {
  getUserDetails: () =>
    apiClient.then(client => client.get('/users/mydetails')),
  getSignInLink: () => '/api/signin',
  getSignOutLink: () => '/api/signout',
};

export default service;
