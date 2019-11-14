import axios, { AxiosInstance } from 'axios';
import { commaSeparated } from '../util/paramSerializers';
import Client from './Client';

export type AxiosConfigurer = (axios: AxiosInstance) => AxiosInstance;

export const baseUrl = {
  content: process.env.CONTENT_API_BASE_URL,
  data: process.env.DATA_API_BASE_URL || '/api/data',
  function: process.env.FUNCTION_API_BASE_URL,
};

// @ts-ignore
const axiosConfigurer: AxiosConfigurer =
  typeof window !== 'undefined' && window.AxiosConfigurer
    ? // @ts-ignore
      window.AxiosConfigurer
    : instance => instance;

export const contentApi = new Client(
  axiosConfigurer(
    axios.create({
      baseURL: `${baseUrl.content}/`,
      paramsSerializer: commaSeparated,
    }),
  ),
);

export const dataApi = new Client(
  axiosConfigurer(
    axios.create({
      baseURL: `${baseUrl.data}/`,
      paramsSerializer: commaSeparated,
    }),
  ),
);

export const functionApi = new Client(
  axiosConfigurer(
    axios.create({
      baseURL: `${baseUrl.function}/`,
      paramsSerializer: commaSeparated,
    }),
  ),
);
