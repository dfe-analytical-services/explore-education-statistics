import axios from 'axios';
import Client from '@common/services/api/Client';
import { commaSeparated } from '@common/services/util/paramSerializers';
import MockAdapter from 'axios-mock-adapter';

/**
 * If choosing to use the mock API for local development, the mock behaviours need to be
 * registered with the MockAdapter that wraps axios.  Services with mock behaviours to register
 * should pass in an implementation of this type when asking this module to create a new API
 * client (using the createClient() method below).
 */
export type MockBehaviourRegistrar = (mock: MockAdapter) => Promise<void>;

interface Config {
  customBaseUrl?: string;
  mockBehaviourRegistrar: MockBehaviourRegistrar;
}

/**
 * This method creates a new axios instance for individual services, wrapped in our
 * convenience class "Client".  Whilst requesting a new Client instance, the service also
 * passes in a function that, if choosing to use the mock API for development, registers its
 * mock responses alongside this Client.
 *
 * @param customBaseUrl
 * @param mockBehaviourRegistrar - see description for the MockBehaviourRegistrar type.
 */
export const createClient = async ({
  customBaseUrl,
  mockBehaviourRegistrar,
}: Config): Promise<Client> => {
  const baseURL = `${customBaseUrl ||
    `${process.env.CONTENT_API_BASE_URL}/api/`}`;

  const axiosInstance = axios.create({
    baseURL,
    paramsSerializer: commaSeparated,
  });

  if (process.env.USE_MOCK_API === 'true') {
    const MockAdaptor = (await import(
      /* webpackChunkName: "axios-mock-adapter" */ 'axios-mock-adapter'
    )).default;

    const mock = new MockAdaptor(axiosInstance);

    await mockBehaviourRegistrar(mock);
  }

  return new Client(axiosInstance);
};

export default createClient;
