import axios from 'axios';
import Client from '@common/services/api/Client';
import { commaSeparated } from '@common/services/util/paramSerializers';
import MockAdapter from "axios-mock-adapter";

export type MockBehaviourRegistrar = (mock: MockAdapter) => Promise<void>;

interface Config {
  customBaseUrl?: string;
  mockBehaviourRegistrar: MockBehaviourRegistrar;
}

export const createClient = async ({ customBaseUrl, mockBehaviourRegistrar }: Config) => {

  const baseURL = `${customBaseUrl || (`${process.env.CONTENT_API_BASE_URL}/api/`)}`;

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