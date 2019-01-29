import axios from 'axios';
import { commaSeparated } from '../util/paramSerializers';

export const baseUrl = {
  content: process.env.CONTENT_API_BASE_URL,
  data: process.env.DATA_API_BASE_URL,
};

export const contentApi = axios.create({
  baseURL: `${baseUrl.content}/api/`,
  paramsSerializer: commaSeparated,
});

export const dataApi = axios.create({
  baseURL: `${baseUrl.data}/api/`,
  paramsSerializer: commaSeparated,
});
