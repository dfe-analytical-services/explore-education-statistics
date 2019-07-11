import axios from 'axios';
import { commaSeparated } from '../util/paramSerializers';
import Client from './Client';

export const baseUrl = {
  content: process.env.CONTENT_API_BASE_URL,
  data: process.env.DATA_API_BASE_URL,
};

export const contentApi = new Client(
  axios.create({
    baseURL: `${baseUrl.content}/api/`,
    paramsSerializer: commaSeparated,
  }),
);
